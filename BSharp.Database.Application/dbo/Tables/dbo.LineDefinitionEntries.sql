CREATE TABLE [dbo].LineDefinitionEntries (
	[Id]								INT					CONSTRAINT [PK_LineDefinitionEntries] PRIMARY KEY NONCLUSTERED IDENTITY,
	[LineDefinitionId]					NVARCHAR (50)		NOT NULL CONSTRAINT [FK_LineDefinitionEntries_LineDefinitions] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[EntryNumber]						INT					NOT NULL  CONSTRAINT [CK_LineDefinitionEntries_EntryNumber]	CHECK([EntryNumber] >= 0),
	CONSTRAINT [IX_LineDefinitionEntries] UNIQUE CLUSTERED ([LineDefinitionId], [EntryNumber]),
	[Direction]							SMALLINT			NOT NULL,
	-- Source = -1 (n/a), 0 (get from line def), 1 (get from Entry), 2 (get from line), 3 (from account), 4 (from other entry data), 8 (from balancing), 9 (from bll script)
	[AccountTypeParentCode]				NVARCHAR (255),
	[AgentDefinitionList]				NVARCHAR (1024),
	[CurrencySource]					SMALLINT			NOT NULL DEFAULT 2,
	[AgentSource]						SMALLINT			NOT NULL DEFAULT 1, --  -1: n/a, 3: from account
	[ResourceSource]					SMALLINT			NOT NULL DEFAULT 1,
	[EntryTypeCode]						NVARCHAR (255),
	[NotedAgentDefinitionId]	NVARCHAR (50),	
	[MonetaryValueSource]		SMALLINT			NOT NULL DEFAULT 2,
	[QuantitySource]			SMALLINT			NOT NULL DEFAULT -1,
	[ExternalReferenceSource]	SMALLINT			NOT NULL DEFAULT -1,
	[AdditionalReferenceSource]	SMALLINT			NOT NULL DEFAULT -1,
	[NotedAgentSource]			SMALLINT			NOT NULL DEFAULT -1,
	[NotedAmountSource]			SMALLINT			NOT NULL DEFAULT -1,
	[DueDateSource]				SMALLINT			NOT NULL DEFAULT -1
);
