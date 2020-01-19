CREATE TABLE [dbo].[Entries] (
--	These are for transactions only. If there are entries from requests or inquiries, etc=> other tables
	[Id]						INT				CONSTRAINT [PK_Entries] PRIMARY KEY IDENTITY,
	[LineId]					INT				NOT NULL CONSTRAINT [FK_Entries__LineId] REFERENCES [dbo].[Lines] ([Id]) ON DELETE CASCADE,
	[EntryNumber]				INT				NOT NULL DEFAULT 1,
	[Direction]					SMALLINT		NOT NULL CONSTRAINT [CK_Entries__Direction]	CHECK ([Direction] IN (-1, 1)),
	[AccountId]					INT				NOT NULL CONSTRAINT [FK_Entries__AccountId] REFERENCES [dbo].[Accounts] ([Id]),
	-- Agent Id is required in Entries only if we have Agent Definition in the account
	[AgentId]					INT				REFERENCES dbo.Agents([Id]),
	-- Resource Id is Required in Entries only if we have resource classification in the account
	[ResourceId]				INT				CONSTRAINT [FK_Entries__ResourceId] REFERENCES dbo.Resources([Id]),
	-- required when the resource classification is anything but Cash
	[ResponsibilityCenterId]	INT				REFERENCES dbo.ResponsibilityCenters([Id]),
	--[AccountIdentifier]			NVARCHAR (10)	CONSTRAINT [FK_Entries__AccountIdentifier] REFERENCES dbo.AccountIdentifiers([Id]), -- to resolve Uniqueness Constraint
	
	--[ResourceIdentifier]		NVARCHAR (10),
	-- When resource is specified, and it has currency, it takes the resource currency. Otherwise, the user must specify it
	-- Entry Type  is required only if NOT NULL
	[EntryTypeId]		INT				CONSTRAINT [FK_Entries__EntryTypeId] REFERENCES [dbo].[EntryTypes] ([Id]),
	-- Due Date is required only for certain resources, 
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
--	Manufacturing and expiry date apply to the composite pair (ResourceId and BatchCode)
	--[Memo]						NVARCHAR (255),
	[MonetaryValue]				DECIMAL (19,4),--			NOT NULL DEFAULT 0,
-- Tracking additive measures, the data type is to be decided by AA
	[Count]						DECIMAL (19,4),--	NOT NULL DEFAULT 0,
	[Mass]						DECIMAL (19,4),--	NOT NULL DEFAULT 0,
	[Volume]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping	
	[Time]						DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- ServiceTimeUnit
	
	[Value]						DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- equivalent in functional currency
-- The following are sort of dynamic properties that capture information for reporting purposes
	[ExternalReference]			NVARCHAR (50),
	[AdditionalReference]		NVARCHAR (50),
	[RelatedAgentId]			INT,
	[RelatedAgentName]			NVARCHAR (50), -- In case, it is not necessary to define the agent, we simply capture the agent name.
	[RelatedAmount]				DECIMAL (19,4),		-- e.g., amount subject to tax
	[RelatedDate]					DATE,
	[Time1]						TIME (0),	-- from time
	[Time2]						TIME (0),	-- to time
-- for auditing
	[CreatedAt]					DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[CreatedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Entries__CreatedById] REFERENCES [dbo].[Users] ([Id]),
	[ModifiedAt]				DATETIMEOFFSET(7)NOT NULL DEFAULT SYSDATETIMEOFFSET(),
	[ModifiedById]				INT				NOT NULL DEFAULT CONVERT(INT, SESSION_CONTEXT(N'UserId')) CONSTRAINT [FK_Entries__ModifiedById] REFERENCES [dbo].[Users] ([Id]),	
);
GO
CREATE INDEX [IX_Entries__LineId] ON [dbo].[Entries]([LineId]);
GO
CREATE INDEX [IX_Entries__AccountId] ON [dbo].[Entries]([AccountId]);
GO
CREATE INDEX [IX_Entries__EntryTypeId] ON [dbo].[Entries]([EntryTypeId]);
GO