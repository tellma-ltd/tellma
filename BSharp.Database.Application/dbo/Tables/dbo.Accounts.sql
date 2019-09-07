CREATE TABLE [dbo].[Accounts] (
	[Id]										INT					CONSTRAINT [PK_Accounts] PRIMARY KEY IDENTITY,
	-- to simplify the account migration, and to reconciliation with the previous system. Users can start entering Jvs immediately afterwards.
	[CustomClassificationId]					INT,
	-- Once the data is imported, the classification of accounts in a manner that is consistent with Ifrs can start.
	-- The allowable values are the lowest level of the calculation trees in Ifrs Taxonomies: (financial position, comprehensive income, by function)
	-- To generate the above financial statements , classifications of childen of same parent can all be aggregated to the parent,
	-- or can some be combined into catchall "other", like Other Inventories, Other property plant and equipment, etc.
	-- To generate additional disclosures, the user must design disclosures using appropriate Ifrs concepts, and then each account 
	-- could be mapped to any concept from that disclosure.
	[IfrsClassificationId]						NVARCHAR (255)		CONSTRAINT [FK_Accounts__IfrsClassificationId] FOREIGN KEY ([IfrsClassificationId]) REFERENCES [dbo].[IfrsAccountClassifications] ([Id]),
	[Name]										NVARCHAR (255)		NOT NULL CONSTRAINT [CK_Accounts__Name] UNIQUE,
	[Name2]										NVARCHAR (255),
	[Name3]										NVARCHAR (255),
	-- To import accounts, or to control sort order, a code is required. Otherwise, it is not.
	[Code]										NVARCHAR (30)		CONSTRAINT [CK_Accounts__Code] UNIQUE,
	[PartyReference]							NVARCHAR (255), -- how it is referred to by the other party
	[HasSingleAgentId]							BIT					DEFAULT 1,
	-- to link several accounts to the same agent.
	[AgentId]									INT					CONSTRAINT [FK_Accounts__AgentId] FOREIGN KEY ([AgentId]) REFERENCES [dbo].[Agents] ([Id]),
	-- necessary to generate notes such as cash flow (direct), statement of change of equity, notes on non current assets
	-- notes on expenses by nature, etc.
	[DefaultDebitIfrsEntryClassificationId]		NVARCHAR (255)		CONSTRAINT [FK_Accounts__DebitIfrsEntryClassificationId] FOREIGN KEY ([DefaultDebitIfrsEntryClassificationId]) REFERENCES [dbo].[IfrsEntryClassifications] ([Id]),
	[DefaultCreditIfrsEntryClassificationId]	NVARCHAR (255)		CONSTRAINT [FK_Accounts__CreditIfrsEntryClassificationId] FOREIGN KEY ([DefaultCreditIfrsEntryClassificationId]) REFERENCES [dbo].[IfrsEntryClassifications] ([Id]),
-- The business segment that "owns" the asset/liablity, and whose performance is assessed by the revenue/expense
-- I propose making it part of the account, especially to track budget. Jiad complained about opening accounts
-- also, smart sales posting is easier since a resource can tell the nature of expense, but not the responsibility center
	-- called SegmentId in B10. When not needed, we use the entity itself.
	[ResponsibilityCenterId]					INT					DEFAULT CONVERT(INT, SESSION_CONTEXT(N'BusinessEntityId')) CONSTRAINT [FK_Accounts__ResponsibilityCenterId] FOREIGN KEY ([ResponsibilityCenterId]) REFERENCES [dbo].[ResponsibilityCenters] ([Id]),
	[HasSingleResourceId]						BIT					DEFAULT 1,
	-- Make the default null, if subsidiary journals are resource based such as inventory, fixed assets, allowance, bonus and overtime.
	[DefaultResourceId]							INT					,--DEFAULT CONVERT(INT, SESSION_CONTEXT(N'FunctionalCurrencyId')) CONSTRAINT [FK_Accounts__DefaultResourceId] REFERENCES [dbo].[Resources] ([Id]),
	-- To transfer a document from requested to authorized, we need an evidence that the responsible actor
	-- has authorized it. If responsibility changes frequently, we use roles. 
	-- However, if responsibility center can be external to account, we may have to move these
	-- to a separate table...
	[ResponsibleActorId]						INT, -- e.g., Ashenafi
	[ResponsibleRoleId]							INT, -- e.g., Marketing Dept Manager
	[CustodianActorId]							INT, -- Alex
	[CustodianRoleId]							INT, -- Raw Materials Warehouse Keeper
	-- Inactive means, it does not appear to the use when classifying an entry
	[IsActive]									BIT					NOT NULL DEFAULT 1,
	-- Audit details
	[CreatedAt]									DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]								INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]								DATETIMEOFFSET(7)	NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]								INT					NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Accounts__ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),
	CONSTRAINT [CK_Accounts__ExpenseByNatureIsRequired] CHECK(
		([IfrsClassificationId] NOT IN (N'CostOfSales', N'DistributionCosts', N'AdministrativeExpense'))
		OR (
			[DefaultDebitIfrsEntryClassificationId] IS NOT NULL
			AND [DefaultCreditIfrsEntryClassificationId] IS NOT NULL
			AND [DefaultDebitIfrsEntryClassificationId] = [DefaultCreditIfrsEntryClassificationId]
		)
	)
);
GO