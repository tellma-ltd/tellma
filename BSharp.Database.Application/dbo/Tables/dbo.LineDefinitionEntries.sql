CREATE TABLE [dbo].LineDefinitionEntries (
	[LineDefinitionId]					NVARCHAR (50) CONSTRAINT [FK_LineDefinitionEntries_LineDefinitions] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[EntryNumber]						INT,
	CONSTRAINT [PK_LineDefinitionEntries] PRIMARY KEY CLUSTERED ([LineDefinitionId], [EntryNumber]),

	[Direction]							SMALLINT			NOT NULL,
	-- Source = -1 (n/a), 0 (get from line def), 1 (get from Entry), 2 (get from line), 3 (from account), 4 (from other entry data), 8 (from balancing), 9 (from bll script)

	-- Account is invisible in a tab, unless the source specifies it is entered by user. or in Manual line
	[AccountSource]						SMALLINT			NOT NULL DEFAULT 0,-- 0: set from line def, 1: entered by User, 3: computed by system based on other info
	[AccountId]							INT, -- invisible, except in 

	[AccountTypeSource]					SMALLINT			NOT NULL DEFAULT 2, -- 0:set from line def, 3: from account
	[AccountTypeId]						NVARCHAR (50)	REFERENCES dbo.[AccountTypes]([Id]), -- 

	[AgentDefinitionSource]				SMALLINT			NOT NULL DEFAULT 2, --  -1: n/a, 0:set from line def, 3: from account
	[AgentDefinitionId]					NVARCHAR (50)	REFERENCES dbo.AgentDefinitions([Id]),

	[ResourceClassificationCode]		NVARCHAR (255),

	[AgentSource]						SMALLINT			NOT NULL DEFAULT 1, --  -1: n/a, 3: from account
	[AgentId]							INT				REFERENCES dbo.Agents([Id]),	-- fixed in the case of ERCA, e.g., VAT

	[ResourceSource]					SMALLINT			NOT NULL DEFAULT 1,
	[ResourceId]						INT				REFERENCES dbo.Resources([Id]),	-- Fixed in the case of unallocated expense
	
	[CurrencySource]					SMALLINT			NOT NULL DEFAULT 2,
	[CurrencyId]						NCHAR (3)		REFERENCES dbo.Currencies([Id]),	-- Fixed in the case of unallocated expense

	[EntryClassificationSource]					SMALLINT			NOT NULL DEFAULT 0,
	[EntryClassificationCode]			NVARCHAR (255),
	
	[MonetaryValueSource]				SMALLINT			NOT NULL DEFAULT 1,
	[QuantitySource]					SMALLINT			NOT NULL DEFAULT 1,
	[ExternalReferenceSource]			SMALLINT			NOT NULL DEFAULT 2,
	[AdditionalReferenceSource]			SMALLINT			NOT NULL DEFAULT 2,
	[RelatedAgentSource]				SMALLINT			NOT NULL DEFAULT 2,
	[RelatedAmountSource]				SMALLINT			NOT NULL DEFAULT 2,
	[DueDateSource]						SMALLINT			NOT NULL DEFAULT 1
);
