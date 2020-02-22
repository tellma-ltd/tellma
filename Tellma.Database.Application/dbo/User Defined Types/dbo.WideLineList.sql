CREATE TYPE [dbo].[WideLineList] AS TABLE (
	[Index]						INT	,
	[DocumentIndex]				INT				INDEX IX_WideLineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				NVARCHAR (50)	NOT NULL,
	[ResponsibilityCenterId]	INT,
	[CurrencyId]				NCHAR (3),
	[AgentId]					INT,
	[ResourceId]				INT,
	[MonetaryValue]				DECIMAL (19,4),
	[Quantity]					DECIMAL (19,4),
	[UnitId]					INT,
	
	[Value]						DECIMAL (19,4),
	[Memo]						NVARCHAR (255),
	
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
	[Quantity0]					DECIMAL (19,4),
	[UnitId0]					INT,
	[Value0]					DECIMAL (19,4),-- equivalent in functional currency		
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
	[Quantity1]					DECIMAL (19,4),
	[UnitId1]					INT,
	[Value1]					DECIMAL (19,4),-- equivalent in functional currency		
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
	[Quantity2]					DECIMAL (19,4),
	[UnitId2]					INT,
	[Value2]					DECIMAL (19,4),-- equivalent in functional currency
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