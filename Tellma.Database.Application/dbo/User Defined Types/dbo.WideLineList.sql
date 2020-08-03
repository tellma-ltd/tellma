CREATE TYPE [dbo].[WideLineList] AS TABLE (
	[Index]						INT	,
	[DocumentIndex]				INT				NOT NULL DEFAULT 0 INDEX IX_WideLineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				INT,
	[PostingDate]				DATE,
	[TemplateLineId]			INT,
	[Multiplier]				DECIMAL (19,4),
	[Memo]						NVARCHAR (255),
	
	[Id0]						INT				NOT NULL DEFAULT 0,
	[Direction0]				SMALLINT,
	[AccountId0]				INT,
	[CustodyId0]				INT,
	[ResourceId0]				INT,
	[CenterId0]					INT,
	[CurrencyId0]				NCHAR (3),
	[EntryTypeId0]				INT,
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

	[Id1]						INT				NOT NULL DEFAULT 0,
	[Direction1]				SMALLINT,
	[AccountId1]				INT,
	[CustodyId1]				INT,
	[ResourceId1]				INT,
	[CenterId1]					INT,
	[CurrencyId1]				NCHAR (3),
	[EntryTypeId1]				INT,
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

	[Id2]						INT				NULL DEFAULT 0, -- since a wide line may be two entries only
	[Direction2]				SMALLINT,
	[AccountId2]				INT,
	[CustodyId2]				INT,
	[ResourceId2]				INT,
	[CenterId2]					INT,
	[CurrencyId2]				NCHAR (3),
	[EntryTypeId2]				INT,
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
	[NotedDate2]				DATE,

	[Id3]						INT				NULL DEFAULT 0,
	[Direction3]				SMALLINT,
	[AccountId3]				INT,
	[CustodyId3]				INT,
	[ResourceId3]				INT,
	[CenterId3]					INT,
	[CurrencyId3]				NCHAR (3),
	[EntryTypeId3]				INT,
	[MonetaryValue3]			DECIMAL (19,4),
	[Quantity3]					DECIMAL (19,4),
	[UnitId3]					INT,
	[Value3]					DECIMAL (19,4),-- equivalent in functional currency		
	[Time13]					DATETIME2 (2),	-- from time
	[Time23]					DATETIME2 (2),	-- to time
	[ExternalReference3]		NVARCHAR (51),
	[AdditionalReference3]		NVARCHAR (51),
	[NotedRelationId3]			INT,
	[NotedAgentName3]			NVARCHAR (51),
	[NotedAmount3]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate3]				DATE,

	[Id4]						INT				NULL DEFAULT 0,
	[Direction4]				SMALLINT,
	[AccountId4]				INT,
	[CustodyId4]				INT,
	[ResourceId4]				INT,
	[CenterId4]					INT,
	[CurrencyId4]				NCHAR (3),
	[EntryTypeId4]				INT,
	[MonetaryValue4]			DECIMAL (19,4),
	[Quantity4]					DECIMAL (19,4),
	[UnitId4]					INT,
	[Value4]					DECIMAL (19,4),-- equivalent in functional currency		
	[Time14]					DATETIME2 (2),	-- from time
	[Time24]					DATETIME2 (2),	-- to time
	[ExternalReference4]		NVARCHAR (51),
	[AdditionalReference4]		NVARCHAR (51),
	[NotedRelationId4]			INT,
	[NotedAgentName4]			NVARCHAR (51),
	[NotedAmount4]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate4]				DATE,

	[Id5]						INT				NULL DEFAULT 0,
	[Direction5]				SMALLINT,
	[AccountId5]				INT,
	[CustodyId5]				INT,
	[ResourceId5]				INT,
	[CenterId5]					INT,
	[CurrencyId5]				NCHAR (3),
	[EntryTypeId5]				INT,
	[MonetaryValue5]			DECIMAL (19,4),
	[Quantity5]					DECIMAL (19,4),
	[UnitId5]					INT,
	[Value5]					DECIMAL (19,4),-- equivalent in functional currency		
	[Time15]					DATETIME2 (2),	-- from time
	[Time25]					DATETIME2 (2),	-- to time
	[ExternalReference5]		NVARCHAR (51),
	[AdditionalReference5]		NVARCHAR (51),
	[NotedRelationId5]			INT,
	[NotedAgentName5]			NVARCHAR (51),
	[NotedAmount5]				DECIMAL (19,4), 	-- used in Tax accounts, to store the quantiy of taxable item
	[NotedDate5]				DATE
);