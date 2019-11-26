CREATE TABLE [dbo].[DocumentLineEntries] (
--	These are for transactions only. If there are entries from requests or inquiries, etc=> other tables
	[Id]						INT				CONSTRAINT [PK_DocumentLineEntries] PRIMARY KEY IDENTITY,
	[DocumentLineId]			INT				NOT NULL CONSTRAINT [FK_DocumentLineEntries__DocumentLineId] FOREIGN KEY ([DocumentLineId])	REFERENCES [dbo].[DocumentLines] ([Id]) ON DELETE CASCADE,
	[EntryNumber]				INT				NOT NULL DEFAULT 1,
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_DocumentLineEntries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				NOT NULL CONSTRAINT [FK_DocumentLineEntries__AccountId]	FOREIGN KEY ([AccountId]) REFERENCES [dbo].[Accounts] ([Id]),

	[ResponsibilityCenterId]	INT				REFERENCES dbo.ResponsibilityCenters([Id]),
	[AgentRelationDefinitionId]	NVARCHAR (50)	REFERENCES dbo.AgentRelationDefinitions([Id]),
	[AgentId]					INT				REFERENCES dbo.Agents([Id]),
	[ResourceId]				INT				NOT NULL CONSTRAINT [FK_DocumentLineEntries__ResourceId] REFERENCES dbo.Resources([Id]),
	-- Entry Type is used to tag entries in a manner that does not affect the account balance
	-- However, consider the case of acc depreciation. We want to map to a different GL. In that case, we set some account definition
	-- to enforce a certain entry classification
	[EntryTypeId]				NVARCHAR(255)	CONSTRAINT [FK_DocumentLineEntries__EntryTypes]	FOREIGN KEY ([EntryTypeId]) REFERENCES [dbo].[EntryTypes] ([Id]),
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
	[BatchCode]					NVARCHAR (50),
	[DueDate]					DATE, -- applies to temporary accounts, such as loans and borrowings
	[MonetaryValue]				MONEY			NOT NULL DEFAULT 0,
	[CurrencyId]				NCHAR (3)		NOT NULL REFERENCES dbo.Currencies([Id]),
-- Tracking additive measures, the data type is to be decided by AA
	[Count]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
	[Mass]						DECIMAL (18,2)	NOT NULL DEFAULT 0,
	
	[Time]						DECIMAL (18,2)	NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Volume]					DECIMAL (18,2)	NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping

	[Value]						VTYPE			NOT NULL DEFAULT 0, -- equivalent in functional currency
-- The following are sort of dynamic properties that capture information for reporting purposes
	[ExternalReference]			NVARCHAR (255),
	[AdditionalReference]		NVARCHAR (255),
	[RelatedAgentId]			INT,
	[RelatedAgentName]			NVARCHAR (50), -- In case, it is not necessary to define the agent, we simply capture the agent name.
	[RelatedAmount]				MONEY,		-- e.g., amount subject to tax
	[Time1]						TIME (0),	-- from time
	[Time2]						TIME (0),	-- to time
	[SortKey]					INT,
-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineEntries__CreatedById] FOREIGN KEY ([CreatedById])	REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_DocumentLineEntries__ModifiedById] FOREIGN KEY ([ModifiedById])REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_DocumentLineEntries__DocumentId] ON [dbo].[DocumentLineEntries]([DocumentLineId]);
GO
CREATE INDEX [IX_DocumentLineEntries__AccountId] ON [dbo].[DocumentLineEntries]([AccountId]);
GO
CREATE INDEX [IX_DocumentLineEntries__IfrsEntryClassificationId] ON [dbo].[DocumentLineEntries]([EntryTypeId]);
GO