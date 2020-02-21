CREATE TABLE [dbo].[Accounts] (
-- Criteria for defining a new account: Do we want to give a code? Do we want to de-activate or add additional properties?
-- If the answer is yes, then we define a new account. If not, use properties in Entries.
-- For reporting purposes, we want to avoid showing large dimensions such as customers, resources, etc...
	[Id]							INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	-- To transfer an entry from requested to authorized, we need an evidence that the responsible center manager has authorized it.
	[ResponsibilityCenterId]		INT					CONSTRAINT [FK_Accounts__ResponsibilityCenterId] REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	[Name]							NVARCHAR (255)		NOT NULL,
	[Name2]							NVARCHAR (255),
	[Name3]							NVARCHAR (255),
	[Code]							NVARCHAR (50), -- used for import.
	--[PartyReference]				NVARCHAR (50), -- how it is referred to by the other party
	[AccountTypeId]					INT					NOT NULL CONSTRAINT [FK_Accounts__AccountTypeId] REFERENCES [dbo].[AccountTypes] ([Id]),
	[IsCurrent]						BIT					NOT NULL,
--
	[LegacyClassificationId]		INT					CONSTRAINT [FK_Accounts__LegacyClassificationId] REFERENCES [dbo].[LegacyClassifications] ([Id]),

	[LegacyTypeId]					NVARCHAR (50)		CONSTRAINT [CK_Accounts__LegacyType] REFERENCES dbo.[LegacyTypes]([Id]),
-- Major properties: NULL means it is not defined.
	[AgentDefinitionId]				NVARCHAR (50),
	--[HasAgent]						BIT				NOT NULL DEFAULT 0,
	--CONSTRAINT [CK_Accounts_AgentDefinitionId_HasAgent] CHECK([HasAgent] = 0 OR [AgentDefinitionId] IS NOT NULL),
	[HasResource]					BIT				NOT NULL DEFAULT 0,
	-- This is like HasSubaccount, where each account might refer to a different agreement/loan/etc. type.
	[HasIdentifier]					BIT				NOT NULL DEFAULT 0,
	[IsRelated]						BIT				NOT NULL DEFAULT 0,

	[HasExternalReference]			BIT				NOT NULL DEFAULT 0,	
	[HasAdditionalReference]		BIT				NOT NULL DEFAULT 0,	
	[HasNotedAgentId]				BIT				NOT NULL DEFAULT 0,
	[HasNotedAgentName]				BIT				NOT NULL DEFAULT 0,
	[HasNotedAmount]				BIT				NOT NULL DEFAULT 0,	
	[HasNotedDate]					BIT				NOT NULL DEFAULT 0,	

-- Minor properties: range of values is restricted by defining a major property. For example, 
-- If AgentId is NULL:
--	a) If the agent definition itself is null, then it is not defined
--	b) if the agent definition itself is not null, then it is to be defined in entries.
	
	[AgentId]						INT,
	CONSTRAINT [FK_Accounts__AgentDefinitionId_AgentId] FOREIGN KEY ([AgentId], [AgentDefinitionId]) REFERENCES [dbo].[Agents] ([Id], [DefinitionId]),
	[ResourceId]					INT					CONSTRAINT [FK_Accounts__ResourceId] REFERENCES [dbo].[Resources] ([Id]),
	-- Especially needed for non-smart accounts to support multi-currencies
	[CurrencyId]					NCHAR (3)			CONSTRAINT [FK_Accounts__CurrencyId] REFERENCES [dbo].[Currencies] ([Id]),
	-- If identifier is specified in Account, it is more like an alias to be memorized and used by non-accountants
	-- If specified in Entries, it is more like the suffix code of a subsidiary account (prefix code is the account itself)
	-- For asset accounts, we have have free choice on debit, but non zero balances on credit.
	-- When it is necessary to be able to deactivate a sub-account, or treat it as a first class citizen, we need to make
	-- it a full fledged-account.
	-- If the account demands a state, such as for the case of a bank account, a supplier account, or a customer account
	-- we would normally mention that in AccountType, or in the table AccountTypeStates, which has - by default - only two 
	-- states: active and deprecated, defined at the top level. When certain type (e.g., trade debtor) requires a state,
	-- we can define those states and then define a workflow specifying who is permitted to modify the account state.
	[Identifier]					NVARCHAR (10),
-- Entry Property
	[EntryTypeId]					INT					CONSTRAINT [FK_Accounts__EntryTypeId] REFERENCES dbo.[EntryTypes],
	[SmartKey]				NCHAR (3),
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
--CREATE UNIQUE INDEX [IX_Accounts__Id_Currency_AgentDefinition_AccountType_IsCurrent_Agent_Resource_RC_EntryType_Identitfier] ON dbo.Accounts(
--			[CurrencyId],
--			[AgentDefinitionId],
--			[AccountTypeId],
--			[IsCurrent],
--			[AgentId],
--			[ResourceId],
--			[ResponsibilityCenterId],
--			[Identifier],
--			[EntryTypeId]
--);
