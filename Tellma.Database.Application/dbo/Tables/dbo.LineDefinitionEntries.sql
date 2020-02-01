CREATE TABLE [dbo].LineDefinitionEntries (
	[Id]						INT					CONSTRAINT [PK_LineDefinitionEntries] PRIMARY KEY NONCLUSTERED IDENTITY,
	[LineDefinitionId]			NVARCHAR (50)		NOT NULL CONSTRAINT [FK_LineDefinitionEntries_LineDefinitions] REFERENCES [dbo].[LineDefinitions] ([Id]),
	[EntryNumber]				INT					NOT NULL  CONSTRAINT [CK_LineDefinitionEntries_EntryNumber]	CHECK([EntryNumber] >= 0),
	CONSTRAINT [IX_LineDefinitionEntries] UNIQUE CLUSTERED ([LineDefinitionId], [EntryNumber]),
	[Direction]					SMALLINT			NOT NULL,
	-- Source = -1 (n/a), 0 (get from line def), 1 (get from Entry), 2 (get from line), 3 (from account), 4 (from other entry data), 8 (from balancing), 9 (from bll script)

-- Source = -1 (n/a), 1 (get from line), 2 (get from entry), 4-7 (from other entry data), 8 (from balancing), 9 (from bll script)
-- 4: from resource/agent/currency etc./5 from (Resource, Account Type), 6: from Counter/Contra/Noted in Line, 7:

	[AccountTypeParentCode]		NVARCHAR (255),
	[AgentDefinitionList]		NVARCHAR (1024),

	[ResponsibilityCenterSource]NVARCHAR (50),
	[AgentSource]				NVARCHAR (50),
	[ResourceSource]			NVARCHAR (50),
	[CurrencySource]			NVARCHAR (50),

--	[MonetaryValueSource]		NVARCHAR (50),
---- Tracking additive measures, the data type is to be decided by AA
--	[CountSource]				NVARCHAR (50),
--	[MassSource]				NVARCHAR (50),
--	[VolumeSource]				NVARCHAR (50),
--	[TimeSource]				NVARCHAR (50),
	
--	[ValueSource]				NVARCHAR (50),
	[EntryTypeCode]				NVARCHAR (255),
	[NotedAgentDefinitionId]	NVARCHAR (50),	
	[QuantitySource]			NVARCHAR (50),
	[NotedAgentSource]			NVARCHAR (50),
	[NotedAmountSource]			NVARCHAR (50)
);
