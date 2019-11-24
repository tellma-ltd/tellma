CREATE TABLE [dbo].[Accounts] (
-- When migrating from PT, we have three cases:
-- G/L accounts: migrated to DefinitionId = N'gl-accounts'. Code can be the same as the PT number
-- Trade Debtors: migrated to DefinitionId = N'trade-debtors-accounts', and to Account classification Trade debtors
-- Trade Creditors: same story
	[Id]							INT					CONSTRAINT [PK__Accounts] PRIMARY KEY IDENTITY,
	[AccountDefinitionId]			NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Accounts__AccountDefinitionId] FOREIGN KEY ([AccountDefinitionId]) REFERENCES [dbo].[AccountDefinitions] ([Id]),
	[AccountClassificationId]		INT					CONSTRAINT [FK_Accounts__AccountClassificationId] FOREIGN KEY ([AccountClassificationId]) REFERENCES [dbo].[AccountClassifications] ([Id]) ON DELETE CASCADE,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party

	-- technically, Currency Id is set by the resource: Money/ETB, Money/USD etc, whenever the account is used for financial tracking only, such as 
	-- Financial assets, Financial liabilities, Equity, Financial gains, and financial losses.
	[HasSingleCurrency]				BIT					NOT NULL DEFAULT 1, 
	[CurrencyId]					NCHAR (3)			NOT NULL DEFAULT CONVERT(NCHAR(3), SESSION_CONTEXT(N'FunctionalCurrencyId')) CONSTRAINT [FK_Accounts__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	[HasSpecificLiquidity]			BIT					NOT NULL DEFAULT 1, -- is either current or non current.
	[IsCurrent]						BIT					NOT NULL DEFAULT 1,
	[HasSingleResponsibilityCenterId] BIT,
	[ResponsibilityCenterId]		INT					CONSTRAINT [FK_Accounts__ResponsibilityCenterId] FOREIGN KEY ([ResponsibilityCenterId]) REFERENCES [dbo].[ResponsibilityCenters] ([Id]) ON DELETE CASCADE,

	[AgentRelationDefinitionId]		NVARCHAR(50),
	[HasSingleAgent]				BIT,
	[AgentId]						INT					CONSTRAINT [FK_Accounts__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]),
	[HasSingleResource]				BIT,
	[ResourceId]					INT					CONSTRAINT [FK_Accounts__ResourceId] FOREIGN KEY ([ResourceId])	REFERENCES [dbo].[Resources] ([Id]),

	[HasSingleEntryTypeId]			BIT,
	[EntryTypeId]					NVARCHAR (255),
	-- To transfer an entry from requested to authorized, we need an evidence that the responsible center manager has authorized it.
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_Accounts__Id_AccountDefinitionId] ON dbo.Accounts([Id], [AccountDefinitionId]);