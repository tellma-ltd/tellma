CREATE TABLE [dbo].[Accounts] (
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	-- To transfer an entry from requested to authorized, we need an evidence that the responsible center manager has authorized it.
	[CenterId]						INT					CONSTRAINT [FK_Accounts__CenterId] REFERENCES [dbo].[Centers] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[AccountTypeId]					INT					NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[CustomClassificationId]		INT					CONSTRAINT [FK_Accounts__CustomClassificationId] REFERENCES [dbo].[LegacyClassifications] ([Id]),
	[IsSmart]						BIT				NOT NULL DEFAULT 0,
	[IsRelated]						BIT				NOT NULL DEFAULT 0,
	[AgentId]						INT				CONSTRAINT [FK_Accounts__AgentId] REFERENCES [dbo].[Resources] ([Id]),
	[ResourceId]					INT				CONSTRAINT [FK_Accounts__ResourceId] REFERENCES [dbo].[Resources] ([Id]),
	[CurrencyId]					NCHAR (3)		CONSTRAINT [FK_Accounts__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	-- This is like Subaccount Id, where each subaccount might refer to a different agreement/loan/etc. type.
	-- For asset accounts, we have free choice on debit, but non zero balances on credit.
	-- When it is necessary to be able to deactivate a sub-account or have a state/workflow, we need to assign it here
	[Identifier]					NVARCHAR (10),
	[EntryTypeId]					INT					CONSTRAINT [FK_Accounts__EntryTypeId] REFERENCES dbo.[EntryTypes],
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_Accounts__Code] ON dbo.Accounts([Code]) WHERE [Code] IS NOT NULL;
GO
-- Problem is: if we have two accounts of same type: Employee Car loans, and Employee House loans, that are commonly used,
-- and they are referred to in smart screens, and they need to be kept separate because of different business rules or terms
-- then it is best to subdivide the account type into "loan types". 
-- subdividing the OtherReceivables into the various loan types
--CREATE UNIQUE INDEX [IX_Accounts__Currency_AccountType_IsCurrent_Agent_Resource_Center_EntryType_Identitfier] ON dbo.Accounts(
--			[CurrencyId],
--			[AccountTypeId],
--			[AgentId],
--			[ResourceId],
--			[CenterId],
--			[Identifier],
--			[EntryTypeId]
--) WHERE IsSmart = 1;
