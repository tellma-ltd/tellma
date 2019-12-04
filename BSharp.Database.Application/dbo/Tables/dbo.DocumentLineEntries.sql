CREATE TABLE [dbo].[DocumentLineEntries] (
--	These are for transactions only. If there are entries from requests or inquiries, etc=> other tables
	[Id]						INT				CONSTRAINT [PK_DocumentLineEntries] PRIMARY KEY IDENTITY,
	[DocumentLineId]			INT				NOT NULL CONSTRAINT [FK_DocumentLineEntries__DocumentLineId] FOREIGN KEY ([DocumentLineId])	REFERENCES [dbo].[DocumentLines] ([Id]) ON DELETE CASCADE,
	[EntryNumber]				INT				NOT NULL DEFAULT 1,
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_DocumentLineEntries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				NOT NULL CONSTRAINT [FK_DocumentLineEntries__AccountId]	FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([Id]),

	[ContractType]				NVARCHAR (50)	CONSTRAINT [CK_DocumentLineEntries__ContractType] CHECK ( [ContractType] IN (
										N'OnHand',
										N'OnDemand',
										N'InTransit',
										N'Receivable',--/PrepaidExpense
										N'Deposit',
										N'Loan',
										N'AccruedIncome',
										N'Equity',
										N'AccruedExpense',
										N'Payable',--/UnearnedRevenue
										N'Retention',
										N'Borrowing',
										N'Revenue',
										N'Expense'
									)),
	[AgentDefinitionId]			NVARCHAR (50)	REFERENCES dbo.AgentDefinitions([Id]),
	[ResourceClassificationId]	INT				CONSTRAINT [FK_DocumentLineEntries__ResourceClassificationId] REFERENCES [dbo].[ResourceClassifications] ([Id]),
	[IsCurrent]					BIT,

	[AgentId]					INT				REFERENCES dbo.Agents([Id]),
	[ResourceId]				INT				NOT NULL CONSTRAINT [FK_DocumentLineEntries__ResourceId] REFERENCES dbo.Resources([Id]),
	[ResponsibilityCenterId]	INT				REFERENCES dbo.ResponsibilityCenters([Id]),
	[AccountDescriptorId]		NVARCHAR (10),--	CONSTRAINT [FK_DocumentLineEntriess__AccountDescriptorId] REFERENCES dbo.AccountDescriptors([Id]), -- to resolve Uniqueness Constraint
	
	[ResourceDescriptorId]		NVARCHAR (10),
	[CurrencyId]				NCHAR (3)		NOT NULL REFERENCES dbo.Currencies([Id]),
	
	-- Entry Type is used to tag entries in a manner that does not affect the account balance
	-- However, consider the case of acc depreciation. We want to map to a different GL. In that case, we set some account definition
	-- to enforce a certain entry classification
	[EntryClassificationId]		INT				CONSTRAINT [FK_DocumentLineEntries__EntryClassificationId] REFERENCES [dbo].[EntryClassifications] ([Id]),
	[DueDate]					DATE, -- applies to temporary accounts, such as loans and borrowings	

-- Revenues Account: The customer
-- COGS: The customer (could be unnamed)
-- Expense Accounts other than COS: The consumer.
	-- The business segment that "owns" the asset/liablity, and whose performance is assessed by the revenue/expense
	-- Smart sales posting is easier since a resource can tell the nature of expense, but not the responsibility center
	-- called SegmentId in B10. When not needed, we use the entity itself.
-- Resource is defined as
--	The good/service sold for revenues and direct expenses
--	The good/service consumed for indirect expenses
	--[ResourceInstanceId]		INT				CONSTRAINT [FK_DocumentLineEntries__ResourcePInstanceId] FOREIGN KEY ([ResourceInstanceId]) REFERENCES [dbo].[ResourceInstances] ([Id]),
--	Manufacturing and expiry date apply to the composite pair (ResourceId and BatchCode)
	--[Memo]						NVARCHAR (255),
	[MonetaryValue]				MONEY			NOT NULL DEFAULT 0,
-- Tracking additive measures, the data type is to be decided by AA
	[Count]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
	[Mass]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
	[Volume]					DECIMAL (18,2)	NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping	
	[Time]						DECIMAL (18,2)	NOT NULL DEFAULT 0, -- ServiceTimeUnit
	
	[Value]						VTYPE			NOT NULL DEFAULT 0, -- equivalent in functional currency
-- The following are sort of dynamic properties that capture information for reporting purposes
	[ExternalReference]			NVARCHAR (50),
	[AdditionalReference]		NVARCHAR (50),
	[RelatedAgentId]			INT,
	[RelatedAgentName]			NVARCHAR (50), -- In case, it is not necessary to define the agent, we simply capture the agent name.
	[RelatedAmount]				MONEY,		-- e.g., amount subject to tax
	[Time1]						TIME (0),	-- from time
	[Time2]						TIME (0),	-- to time
	[SortKey]					INT,
-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineEntries__CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineEntries__ModifiedById]  FOREIGN KEY ([ModifiedById]) REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_DocumentLineEntries__DocumentLineId] ON [dbo].[DocumentLineEntries]([DocumentLineId]);
GO
CREATE INDEX [IX_DocumentLineEntries__AccountId] ON [dbo].[DocumentLineEntries]([AccountId]);
GO
CREATE INDEX [IX_DocumentLineEntries__EntryClassificationId] ON [dbo].[DocumentLineEntries]([EntryClassificationId]);
GO