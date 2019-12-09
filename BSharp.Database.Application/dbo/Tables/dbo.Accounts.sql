CREATE TABLE [dbo].[Accounts] (
-- When migrating from PT, we have three cases:
-- G/L accounts: migrated to DefinitionId = N'gl-accounts'. Code can be the same as the PT number
-- Trade Debtors: migrated to DefinitionId = N'trade-debtors-accounts', and to Account classification Trade debtors
-- Trade Creditors: same story 
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	[ResponsibilityCenterId]		INT					CONSTRAINT [FK_Accounts__ResponsibilityCenterId] REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	[AccountClassificationId]		INT					CONSTRAINT [FK_Accounts__AccountClassificationId] 
														REFERENCES [dbo].[AccountClassifications] ([Id]) ON DELETE CASCADE,
	[IsSmart]						BIT					NOT NULL DEFAULT 0,
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party

-- Major properties: NULL means it is not defined.
	[AccountTypeId]					NVARCHAR (50)		NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[ContractType]					NVARCHAR (50)		CONSTRAINT [CK_Accounts__ContractType] REFERENCES dbo.[ContractTypes]([Id]),
	[AgentDefinitionId]				NVARCHAR (50),
	[ResourceClassificationId]		INT					CONSTRAINT [FK_Accounts__ResourceClassificationId] REFERENCES [dbo].[ResourceClassifications] ([Id]),
	CONSTRAINT [CK_Accounts__IsSmart_ResourceClassificationId] CHECK ([IsSmart] = 0 OR [ResourceClassificationId] IS NOT NULL AND [ContractType] IS NOT NULL),
	[IsCurrent]						BIT,
-- Minor properties: range of values is restricted by defining a major property. For example, if AccountTypeId = N'Payable', then responsibility center
-- must be an operating segment. 
-- NULL means two things:
--	a) If the type itself is null, then it is not defined
--	b) if the type itself is not null, then it is to be defined in entries.
	[AgentId]						INT					CONSTRAINT [FK_Accounts__AgentId] REFERENCES [dbo].[Agents] ([Id]),
	[ResourceId]					INT					CONSTRAINT [FK_Accounts__ResourceId] REFERENCES [dbo].[Resources] ([Id]),
	[Identifier]					NVARCHAR (10)		CONSTRAINT [FK_Accounts__Identifier] REFERENCES dbo.[AccountIdentifiers]([Id]), -- to resolve Uniqueness Constraint
-- Entry Property
	[EntryClassificationId]			INT					CONSTRAINT [FK_Accounts__EntryClassificationId] REFERENCES dbo.[EntryClassifications],
	-- To transfer an entry from requested to authorized, we need an evidence that the responsible center manager has authorized it.
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id])
);
GO
--CREATE UNIQUE INDEX [IX_Accounts__Id_AccountDefinitionId] ON dbo.Accounts([Id], [AccountTypeId]);
CREATE UNIQUE INDEX [IX_Accounts__Id_AccountTypeId_AgentDefinitionId_ResourceClassificationId] ON dbo.Accounts(
			[AccountTypeId],
			[AgentDefinitionId],
			[ResourceClassificationId],
			[IsCurrent],
			[AgentId],
			[ResourceId],
			[ResponsibilityCenterId],
			[Identifier],
			[EntryClassificationId]
) WHERE [IsSmart] = 1;