CREATE TABLE [dbo].[SubAccounts] (
	[Id]							INT					CONSTRAINT [PK_SubAccounts] PRIMARY KEY IDENTITY,
	-- To transfer an entry from requested to authorized, we need an evidence that the responsible center manager has authorized it.
	[ResponsibilityCenterId]		INT					NOT NULL CONSTRAINT [FK_SubAccounts__ResponsibilityCenterId] REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[GLAccountId]					INT					NOT NULL CONSTRAINT [FK_SubAccounts__GLAccountId] REFERENCES [dbo].[GLAccounts] ([Id]),
	[AgentId]						INT,
	[ResourceId]					INT					CONSTRAINT [FK_SubAccounts__ResourceId] REFERENCES [dbo].[Resources] ([Id]),
	-- If the account demands a state, such as for the case of a bank account, a supplier account, or a customer account
	-- we would normally mention that in GLAccount, or in the table GLAccountTypeStates, which has - by default - only two 
	-- states: active and deprecated. When certain type (e.g., trade debtor) requires a state,
	-- we can define those states and then define a workflow specifying who is permitted to modify the account state.
-- Entry Property
	[EntryTypeId]					INT					CONSTRAINT [FK_SubAccounts__EntryTypeId] REFERENCES dbo.[EntryTypes],
	[IsDeprecated]					BIT					NOT NULL DEFAULT 0,
	-- Audit details
	[CreatedAt]						DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_SubAccounts__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]					DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]					INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_SubAccounts__ModifiedById] REFERENCES [dbo].[Users] ([Id])
);
GO
CREATE UNIQUE INDEX [IX_SubAccounts__Code] ON dbo.SubAccounts([Code]) WHERE [Code] IS NOT NULL;
GO