CREATE TYPE [dbo].[WideLineList] AS TABLE (
	[Index]						INT				PRIMARY KEY,
	[DocumentIndex]				INT				NOT NULL,
	[Id]						INT				NOT NULL DEFAULT 0,
	
	[DefinitionId]				NVARCHAR (50)	NOT NULL,
	[ResponsibilityCenterId]	INT,
	[CurrencyId]				NCHAR (3),
	[AgentId]					INT,
	[ResourceId]				INT,
	[MonetaryValue]				DECIMAL (19,4),--			NOT NULL DEFAULT 0,
	[Count]						DECIMAL (19,4),--	NOT NULL DEFAULT 0,
	[Mass]						DECIMAL (19,4),--	NOT NULL DEFAULT 0,
	[Volume]					DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping	
	[Time]						DECIMAL (19,4),--	NOT NULL DEFAULT 0, -- ServiceTimeUnit
	
	[Value]						DECIMAL (19,4),
	[Memo]						NVARCHAR (255), -- a textual description for statements and reports

	
	[Direction0]				SMALLINT,
	[AgentId0]					INT,
	[ResourceId0]				INT,
	[ResponsibilityCenterId0]	INT,
	[AccountIdentifier0]		NVARCHAR (10),
	[ResourceIdentifier0]		NVARCHAR (10),
	[CurrencyId0]				NCHAR (3),
	[EntryTypeId0]				INT,
	[DueDate0]					DATE,
	[MonetaryValue0]			DECIMAL (19,4),
	[Count0]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass0]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume0]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time0]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value0]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency		
	[Time10]					TIME (0),	-- from time
	[Time20]					TIME (0),	-- to time
	[ExternalReference0]		NVARCHAR (50),
	[AdditionalReference0]		NVARCHAR (50),
	[NotedAgentId0]				INT,
	[NotedAgentName0]			NVARCHAR (50),
	[NotedAmount0]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate0]				DATE,

	[Direction1]				SMALLINT,
	[AgentId1]					INT,
	[ResourceId1]				INT,
	[ResponsibilityCenterId1]	INT,
	[AccountIdentifier1]		NVARCHAR (10),
	[ResourceIdentifier1]		NVARCHAR (10),
	[CurrencyId1]				NCHAR (3),
	[EntryTypeId1]				INT,
	[DueDate1]					DATE,
	[MonetaryValue1]			DECIMAL (19,4),
	[Count1]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass1]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume1]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time1]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value1]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency		
	[Time11]					TIME (0),	-- from time
	[Time21]					TIME (0),	-- to time
	[ExternalReference1]		NVARCHAR (51),
	[AdditionalReference1]		NVARCHAR (51),
	[NotedAgentId1]				INT,
	[NotedAgentName1]			NVARCHAR (51),
	[NotedAmount1]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate1]				DATE,

	[Direction2]				SMALLINT,
	[AgentId2]					INT,
	[ResourceId2]				INT,
	[ResponsibilityCenterId2]	INT,
	[AccountIdentifier2]		NVARCHAR (10),
	[ResourceIdentifier2]		NVARCHAR (10),
	[CurrencyId2]				NCHAR (3),
	[EntryTypeId2]				INT,
	[DueDate2]					DATE,
	[MonetaryValue2]			DECIMAL (19,4),
	[Count2]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- CountUnit
	[Mass2]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- MassUnit, like LTZ bar, cement bag, etc
	[Volume2]					DECIMAL (19,4)		NOT NULL DEFAULT 0, -- VolumeUnit, possibly for shipping
	[Time2]						DECIMAL (19,4)		NOT NULL DEFAULT 0, -- ServiceTimeUnit
	[Value2]					DECIMAL (19,4)		NOT NULL DEFAULT 0 ,-- equivalent in functional currency
	[RelatedDate2]				DATE,
	[Time12]					TIME (0),	-- from time
	[Time22]					TIME (0),	-- to time
	[ExternalReference2]		NVARCHAR (52),
	[AdditionalReference2]		NVARCHAR (52),
	[NotedAgentId2]				INT,
	[NotedAgentName2]			NVARCHAR (52),
	[NotedAmount2]				DECIMAL (29,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate2]				DATE
);