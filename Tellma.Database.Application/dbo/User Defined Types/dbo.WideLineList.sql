CREATE TYPE [dbo].[WideLineList] AS TABLE (
	[Index]						INT	,
	[DocumentIndex]				INT				INDEX IX_WideLineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				NVARCHAR (50)	NOT NULL,
	[Memo]						NVARCHAR (255),
	
	[Id0]						INT,
	[Direction0]				SMALLINT,
	[AccountId0]				INT,
	[RelationId0]				INT,
	[ContractId0]				INT,
	[ResourceId0]				INT,
	[CenterId0]					INT,
	[CurrencyId0]				NCHAR (3),
	[EntryTypeId0]				INT,
	[DueDate0]					DATE,
	[MonetaryValue0]			DECIMAL (19,4),
	[Quantity0]					DECIMAL (19,4),
	[UnitId0]					INT,
	[Value0]					DECIMAL (19,4),-- equivalent in functional currency		
	[Time10]					DATETIME2 (2),	-- from time
	[Time20]					DATETIME2 (2),	-- to time
	[ExternalReference0]		NVARCHAR (50),
	[AdditionalReference0]		NVARCHAR (50),
	[NotedRelationId0]			INT,
	[NotedAgentName0]			NVARCHAR (50),
	[NotedAmount0]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate0]				DATE,

	[Id1]						INT,
	[Direction1]				SMALLINT,
	[AccountId1]				INT,
	[RelationId1]				INT,
	[ContractId1]				INT,
	[ResourceId1]				INT,
	[CenterId1]					INT,
	[CurrencyId1]				NCHAR (3),
	[EntryTypeId1]				INT,
	[DueDate1]					DATE,
	[MonetaryValue1]			DECIMAL (19,4),
	[Quantity1]					DECIMAL (19,4),
	[UnitId1]					INT,
	[Value1]					DECIMAL (19,4),-- equivalent in functional currency		
	[Time11]					DATETIME2 (2),	-- from time
	[Time21]					DATETIME2 (2),	-- to time
	[ExternalReference1]		NVARCHAR (51),
	[AdditionalReference1]		NVARCHAR (51),
	[NotedRelationId1]			INT,
	[NotedAgentName1]			NVARCHAR (51),
	[NotedAmount1]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate1]				DATE,

	[Id2]						INT,
	[Direction2]				SMALLINT,
	[AccountId2]				INT,
	[RelationId2]				INT,
	[ContractId2]				INT,
	[ResourceId2]				INT,
	[CenterId2]					INT,
	[CurrencyId2]				NCHAR (3),
	[EntryTypeId2]				INT,
	[DueDate2]					DATE,
	[MonetaryValue2]			DECIMAL (19,4),
	[Quantity2]					DECIMAL (19,4),
	[UnitId2]					INT,
	[Value2]					DECIMAL (19,4),-- equivalent in functional currency
	[RelatedDate2]				DATE,
	[Time12]					DATETIME2 (2),	-- from time
	[Time22]					DATETIME2 (2),	-- to time
	[ExternalReference2]		NVARCHAR (52),
	[AdditionalReference2]		NVARCHAR (52),
	[NotedRelationId2]			INT,
	[NotedAgentName2]			NVARCHAR (52),
	[NotedAmount2]				DECIMAL (29,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate2]				DATE
);