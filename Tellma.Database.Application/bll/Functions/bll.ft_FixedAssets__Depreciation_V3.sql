CREATE FUNCTION [bll].[ft_FixedAssets__Depreciation_V3]
(
-- Using Claude.AI
-- DECLARE
-- Even Better version than V2, allows correct depreciation even when the asset changes center during the month
	@PostingDate DATE, -- depreciate till Posting Date. This is usually date for transfer, disposal, or end of month
	@StartDate DATE = N'1753-01-01', -- Typically from archive datae
	@ResourceDefinitionId INT = NULL,
	@ResourceId INT = NULL
)
	RETURNS @Widelines TABLE
	(
		[Index]						INT	,
		[DocumentIndex]				INT				NOT NULL DEFAULT 0,-- INDEX IX_WideLineList_DocumentIndex ([DocumentIndex]),
		PRIMARY KEY ([Index], [DocumentIndex]),
		[Id]						INT				NOT NULL DEFAULT 0,
		[DefinitionId]				INT,
		[PostingDate]				DATE,
		[Memo]						NVARCHAR (255),
		[Boolean1]					BIT,
		[Decimal1]					DECIMAL (19,6),
		[Decimal2]					DECIMAL (19,6),
		[Text1]						NVARCHAR(50),
		[Text2]						NVARCHAR(50),
	
		[Id0]						INT				NOT NULL DEFAULT 0,
		[Direction0]				SMALLINT,
		[AccountId0]				INT,
		[AgentId0]					INT,
		[NotedAgentId0]				INT,
		[ResourceId0]				INT,
		[CenterId0]					INT,
		[CurrencyId0]				NCHAR (3),
		[EntryTypeId0]				INT,
		[MonetaryValue0]			DECIMAL (19,6),
		[Quantity0]					DECIMAL (19,6),
		[UnitId0]					INT,
		[Value0]					DECIMAL (19,6),		
		[Time10]					DATETIME2 (2),
		[Duration0]					DECIMAL (19,6),
		[DurationUnitId0]			INT,
		[Time20]					DATETIME2 (2),
		[ExternalReference0]		NVARCHAR (50),
		[ReferenceSourceId0]		INT,
		[InternalReference0]		NVARCHAR (50),
		[NotedAgentName0]			NVARCHAR (50),
		[NotedAmount0]				DECIMAL (19,6),
		[NotedDate0]				DATE,
		[NotedResourceId0]			INT,

		[Id1]						INT				NULL DEFAULT 0,
		[Direction1]				SMALLINT,
		[AccountId1]				INT,
		[AgentId1]					INT,
		[NotedAgentId1]				INT,
		[ResourceId1]				INT,
		[CenterId1]					INT,
		[CurrencyId1]				NCHAR (3),
		[EntryTypeId1]				INT,
		[MonetaryValue1]			DECIMAL (19,6),
		[Quantity1]					DECIMAL (19,6),
		[UnitId1]					INT,
		[Value1]					DECIMAL (19,6),		
		[Time11]					DATETIME2 (2),
		[Duration1]					DECIMAL (19,6),
		[DurationUnitId1]			INT,
		[Time21]					DATETIME2 (2),
		[ExternalReference1]		NVARCHAR (50),
		[ReferenceSourceId1]		INT,
		[InternalReference1]		NVARCHAR (50),
		[NotedAgentName1]			NVARCHAR (50),
		[NotedAmount1]				DECIMAL (19,6),
		[NotedDate1]				DATE,
		[NotedResourceId1]			INT,

		[Id2]						INT				NULL DEFAULT 0, -- since a wide line may be two entries only
		[Direction2]				SMALLINT,
		[AccountId2]				INT,
		[AgentId2]					INT,
		[NotedAgentId2]				INT,
		[ResourceId2]				INT,
		[CenterId2]					INT,
		[CurrencyId2]				NCHAR (3),
		[EntryTypeId2]				INT,
		[MonetaryValue2]			DECIMAL (19,6),
		[Quantity2]					DECIMAL (19,6),
		[UnitId2]					INT,
		[Value2]					DECIMAL (19,6),
		[Time12]					DATETIME2 (2),
		[Duration2]					DECIMAL (19,6),
		[DurationUnitId2]			INT,
		[Time22]					DATETIME2 (2),
		[ExternalReference2]		NVARCHAR (50),
		[ReferenceSourceId2]		INT,
		[InternalReference2]		NVARCHAR (50),
		[NotedAgentName2]			NVARCHAR (50),
		[NotedAmount2]				DECIMAL (19,6),
		[NotedDate2]				DATE,
		[NotedResourceId2]			INT,

		[Id3]						INT				NULL DEFAULT 0,
		[Direction3]				SMALLINT,
		[AccountId3]				INT,
		[AgentId3]					INT,
		[NotedAgentId3]				INT,
		[ResourceId3]				INT,
		[CenterId3]					INT,
		[CurrencyId3]				NCHAR (3),
		[EntryTypeId3]				INT,
		[MonetaryValue3]			DECIMAL (19,6),
		[Quantity3]					DECIMAL (19,6),
		[UnitId3]					INT,
		[Value3]					DECIMAL (19,6),		
		[Time13]					DATETIME2 (2),
		[Duration3]					DECIMAL (19,6),
		[DurationUnitId3]			INT,
		[Time23]					DATETIME2 (2),
		[ExternalReference3]		NVARCHAR (50),
		[ReferenceSourceId3]		INT,
		[InternalReference3]		NVARCHAR (50),
		[NotedAgentName3]			NVARCHAR (50),
		[NotedAmount3]				DECIMAL (19,6),
		[NotedDate3]				DATE,
		[NotedResourceId3]			INT,

		[Id4]						INT				NULL DEFAULT 0,
		[Direction4]				SMALLINT,
		[AccountId4]				INT,
		[AgentId4]					INT,
		[NotedAgentId4]				INT,
		[ResourceId4]				INT,
		[CenterId4]					INT,
		[CurrencyId4]				NCHAR (3),
		[EntryTypeId4]				INT,
		[MonetaryValue4]			DECIMAL (19,6),
		[Quantity4]					DECIMAL (19,6),
		[UnitId4]					INT,
		[Value4]					DECIMAL (19,6),		
		[Time14]					DATETIME2 (2),
		[Duration4]					DECIMAL (19,6),
		[DurationUnitId4]			INT,
		[Time24]					DATETIME2 (2),
		[ExternalReference4]		NVARCHAR (50),
		[ReferenceSourceId4]		INT,
		[InternalReference4]		NVARCHAR (50),
		[NotedAgentName4]			NVARCHAR (50),
		[NotedAmount4]				DECIMAL (19,6),
		[NotedDate4]				DATE,
		[NotedResourceId4]			INT,

		[Id5]						INT				NULL DEFAULT 0,
		[Direction5]				SMALLINT,
		[AccountId5]				INT,
		[AgentId5]					INT,
		[NotedAgentId5]				INT,
		[ResourceId5]				INT,
		[CenterId5]					INT,
		[CurrencyId5]				NCHAR (3),
		[EntryTypeId5]				INT,
		[MonetaryValue5]			DECIMAL (19,6),
		[Quantity5]					DECIMAL (19,6),
		[UnitId5]					INT,
		[Value5]					DECIMAL (19,6),		
		[Time15]					DATETIME2 (2),
		[Duration5]					DECIMAL (19,6),
		[DurationUnitId5]			INT,
		[Time25]					DATETIME2 (2),
		[ExternalReference5]		NVARCHAR (50),
		[ReferenceSourceId5]		INT,
		[InternalReference5]		NVARCHAR (50),
		[NotedAgentName5]			NVARCHAR (50),
		[NotedAmount5]				DECIMAL (19,6),
		[NotedDate5]				DATE,
		[NotedResourceId5]			INT,

		[Id6]						INT				NULL DEFAULT 0,
		[Direction6]				SMALLINT,
		[AccountId6]				INT,
		[AgentId6]					INT,
		[NotedAgentId6]				INT,
		[ResourceId6]				INT,
		[CenterId6]					INT,
		[CurrencyId6]				NCHAR (3),
		[EntryTypeId6]				INT,
		[MonetaryValue6]			DECIMAL (19,6),
		[Quantity6]					DECIMAL (19,6),
		[UnitId6]					INT,
		[Value6]					DECIMAL (19,6),		
		[Time16]					DATETIME2 (2),
		[Duration6]					DECIMAL (19,6),
		[DurationUnitId6]			INT,
		[Time26]					DATETIME2 (2),
		[ExternalReference6]		NVARCHAR (50),
		[ReferenceSourceId6]		INT,
		[InternalReference6]		NVARCHAR (50),
		[NotedAgentName6]			NVARCHAR (50),
		[NotedAmount6]				DECIMAL (19,6),
		[NotedDate6]				DATE,
		[NotedResourceId6]			INT,

		[Id7]						INT				NULL DEFAULT 0,
		[Direction7]				SMALLINT,
		[AccountId7]				INT,
		[AgentId7]					INT,
		[NotedAgentId7]				INT,
		[ResourceId7]				INT,
		[CenterId7]					INT,
		[CurrencyId7]				NCHAR (3),
		[EntryTypeId7]				INT,
		[MonetaryValue7]			DECIMAL (19,6),
		[Quantity7]					DECIMAL (19,6),
		[UnitId7]					INT,
		[Value7]					DECIMAL (19,6),		
		[Time17]					DATETIME2 (2),
		[Duration7]					DECIMAL (19,6),
		[DurationUnitId7]			INT,
		[Time27]					DATETIME2 (2),
		[ExternalReference7]		NVARCHAR (50),
		[ReferenceSourceId7]		INT,
		[InternalReference7]		NVARCHAR (50),
		[NotedAgentName7]			NVARCHAR (50),
		[NotedAmount7]				DECIMAL (19,6),
		[NotedDate7]				DATE,
		[NotedResourceId7]			INT,

		[Id8]						INT				NULL DEFAULT 0,
		[Direction8]				SMALLINT,
		[AccountId8]				INT,
		[AgentId8]					INT,
		[NotedAgentId8]				INT,
		[ResourceId8]				INT,
		[CenterId8]					INT,
		[CurrencyId8]				NCHAR (3),
		[EntryTypeId8]				INT,
		[MonetaryValue8]			DECIMAL (19,6),
		[Quantity8]					DECIMAL (19,6),
		[UnitId8]					INT,
		[Value8]					DECIMAL (19,6),		
		[Time18]					DATETIME2 (2),
		[Duration8]					DECIMAL (19,6),
		[DurationUnitId8]			INT,
		[Time28]					DATETIME2 (2),
		[ExternalReference8]		NVARCHAR (50),
		[ReferenceSourceId8]		INT,
		[InternalReference8]		NVARCHAR (50),
		[NotedAgentName8]			NVARCHAR (50),
		[NotedAmount8]				DECIMAL (19,6),
		[NotedDate8]				DATE,
		[NotedResourceId8]			INT,

		[Id9]						INT				NULL DEFAULT 0,
		[Direction9]				SMALLINT,
		[AccountId9]				INT,
		[AgentId9]					INT,
		[NotedAgentId9]				INT,
		[ResourceId9]				INT,
		[CenterId9]					INT,
		[CurrencyId9]				NCHAR (3),
		[EntryTypeId9]				INT,
		[MonetaryValue9]			DECIMAL (19,6),
		[Quantity9]					DECIMAL (19,6),
		[UnitId9]					INT,
		[Value9]					DECIMAL (19,6),		
		[Time19]					DATETIME2 (2),
		[Duration9]					DECIMAL (19,6),
		[DurationUnitId9]			INT,
		[Time29]					DATETIME2 (2),
		[ExternalReference9]		NVARCHAR (50),
		[ReferenceSourceId9]		INT,
		[InternalReference9]		NVARCHAR (50),
		[NotedAgentName9]			NVARCHAR (50),
		[NotedAmount9]				DECIMAL (19,6),
		[NotedDate9]				DATE,
		[NotedResourceId9]			INT,

		[Id10]						INT				NULL DEFAULT 0,
		[Direction10]				SMALLINT,
		[AccountId10]				INT,
		[AgentId10]					INT,
		[NotedAgentId10]			INT,
		[ResourceId10]				INT,
		[CenterId10]				INT,
		[CurrencyId10]				NCHAR (3),
		[EntryTypeId10]				INT,
		[MonetaryValue10]			DECIMAL (19,6),
		[Quantity10]				DECIMAL (19,6),
		[UnitId10]					INT,
		[Value10]					DECIMAL (19,6),		
		[Time110]					DATETIME2 (2),
		[Duration10]				DECIMAL (19,6),
		[DurationUnitId10]			INT,
		[Time210]					DATETIME2 (2),
		[ExternalReference10]		NVARCHAR (50),
		[ReferenceSourceId10]		INT,
		[InternalReference10]		NVARCHAR (50),
		[NotedAgentName10]			NVARCHAR (50),
		[NotedAmount10]				DECIMAL (19,6),
		[NotedDate10]				DATE,
		[NotedResourceId10]			INT,

		[Id11]						INT				NULL DEFAULT 0,
		[Direction11]				SMALLINT,
		[AccountId11]				INT,
		[AgentId11]					INT,
		[NotedAgentId11]			INT,
		[ResourceId11]				INT,
		[CenterId11]				INT,
		[CurrencyId11]				NCHAR (3),
		[EntryTypeId11]				INT,
		[MonetaryValue11]			DECIMAL (19,6),
		[Quantity11]				DECIMAL (19,6),
		[UnitId11]					INT,
		[Value11]					DECIMAL (19,6),		
		[Time111]					DATETIME2 (2),
		[Duration11]				DECIMAL (19,6),
		[DurationUnitId11]			INT,
		[Time211]					DATETIME2 (2),
		[ExternalReference11]		NVARCHAR (50),
		[ReferenceSourceId11]		INT,
		[InternalReference11]		NVARCHAR (50),
		[NotedAgentName11]			NVARCHAR (50),
		[NotedAmount11]				DECIMAL (19,6),
		[NotedDate11]				DATE,
		[NotedResourceId11]			INT,

		[Id12]						INT				NULL DEFAULT 0,
		[Direction12]				SMALLINT,
		[AccountId12]				INT,
		[AgentId12]					INT,
		[NotedAgentId12]			INT,
		[ResourceId12]				INT,
		[CenterId12]				INT,
		[CurrencyId12]				NCHAR (3),
		[EntryTypeId12]				INT,
		[MonetaryValue12]			DECIMAL (19,6),
		[Quantity12]				DECIMAL (19,6),
		[UnitId12]					INT,
		[Value12]					DECIMAL (19,6),		
		[Time112]					DATETIME2 (2),
		[Duration12]				DECIMAL (19,6),
		[DurationUnitId12]			INT,
		[Time212]					DATETIME2 (2),
		[ExternalReference12]		NVARCHAR (50),
		[ReferenceSourceId12]		INT,
		[InternalReference12]		NVARCHAR (50),
		[NotedAgentName12]			NVARCHAR (50),
		[NotedAmount12]				DECIMAL (19,6),
		[NotedDate12]				DATE,
		[NotedResourceId12]			INT,

		[Id13]						INT				NULL DEFAULT 0,
		[Direction13]				SMALLINT,
		[AccountId13]				INT,
		[AgentId13]					INT,
		[NotedAgentId13]			INT,
		[ResourceId13]				INT,
		[CenterId13]				INT,
		[CurrencyId13]				NCHAR (3),
		[EntryTypeId13]				INT,
		[MonetaryValue13]			DECIMAL (19,6),
		[Quantity13]				DECIMAL (19,6),
		[UnitId13]					INT,
		[Value13]					DECIMAL (19,6),		
		[Time113]					DATETIME2 (2),
		[Duration13]				DECIMAL (19,6),
		[DurationUnitId13]			INT,
		[Time213]					DATETIME2 (2),
		[ExternalReference13]		NVARCHAR (50),
		[ReferenceSourceId13]		INT,
		[InternalReference13]		NVARCHAR (50),
		[NotedAgentName13]			NVARCHAR (50),
		[NotedAmount13]				DECIMAL (19,6),
		[NotedDate13]				DATE,
		[NotedResourceId13]			INT,

		[Id14]						INT				NULL DEFAULT 0,
		[Direction14]				SMALLINT,
		[AccountId14]				INT,
		[AgentId14]					INT,
		[NotedAgentId14]			INT,
		[ResourceId14]				INT,
		[CenterId14]				INT,
		[CurrencyId14]				NCHAR (3),
		[EntryTypeId14]				INT,
		[MonetaryValue14]			DECIMAL (19,6),
		[Quantity14]				DECIMAL (19,6),
		[UnitId14]					INT,
		[Value14]					DECIMAL (19,6),		
		[Time114]					DATETIME2 (2),
		[Duration14]				DECIMAL (19,6),
		[DurationUnitId14]			INT,
		[Time214]					DATETIME2 (2),
		[ExternalReference14]		NVARCHAR (50),
		[ReferenceSourceId14]		INT,
		[InternalReference14]		NVARCHAR (50),
		[NotedAgentName14]			NVARCHAR (50),
		[NotedAmount14]				DECIMAL (19,6),
		[NotedDate14]				DATE,
		[NotedResourceId14]			INT,

		[Id15]						INT				NULL DEFAULT 0,
		[Direction15]				SMALLINT,
		[AccountId15]				INT,
		[AgentId15]					INT,
		[NotedAgentId15]			INT,
		[ResourceId15]				INT,
		[CenterId15]				INT,
		[CurrencyId15]				NCHAR (3),
		[EntryTypeId15]				INT,
		[MonetaryValue15]			DECIMAL (19,6),
		[Quantity15]				DECIMAL (19,6),
		[UnitId15]					INT,
		[Value15]					DECIMAL (19,6),		
		[Time115]					DATETIME2 (2),
		[Duration15]				DECIMAL (19,6),
		[DurationUnitId15]			INT,
		[Time215]					DATETIME2 (2),
		[ExternalReference15]		NVARCHAR (50),
		[ReferenceSourceId15]		INT,
		[InternalReference15]		NVARCHAR (50),
		[NotedAgentName15]			NVARCHAR (50),
		[NotedAmount15]				DECIMAL (19,6),
		[NotedDate15]				DATE,
		[NotedResourceId15]			INT
	)
AS
	BEGIN

DECLARE @MonthUnit INT = dal.fn_UnitCode__Id(N'mo');
DECLARE @DayUnit   INT = dal.fn_UnitCode__Id(N'd');

-- @StartDate is always overridden to the start of the posting month.
-- Only variances whose NotedDate falls on or after @StartDate will be posted.
SET @StartDate = dbo.fn_MonthStart(@PostingDate);

-- ============================================================
-- Account-type boundary nodes
-- ============================================================
DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
DECLARE @ROUNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'RightofuseAssets');
DECLARE @IPCNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyCompleted');
DECLARE @IANode  HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');

-- ============================================================
-- STEP 1: Identify fixed-asset accounts and their depreciation
--         entry type so we can separate cost entries from
--         depreciation entries later.
-- ============================================================
DECLARE @FixedAssetsAccountIds TABLE (
	[FixedAssetAccountId]                INT PRIMARY KEY,
	[AccumulatedDepreciationEntryTypeId] INT,
	INDEX IX1 ([AccumulatedDepreciationEntryTypeId])
);
INSERT INTO @FixedAssetsAccountIds ([FixedAssetAccountId], [AccumulatedDepreciationEntryTypeId])
SELECT A.[Id],
	CASE
		WHEN AC.[Node].IsDescendantOf(@PPENode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN AC.[Node].IsDescendantOf(@ROUNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN AC.[Node].IsDescendantOf(@IPCNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationInvestmentProperty')
		WHEN AC.[Node].IsDescendantOf(@IANode)  = 1 THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
		ELSE NULL
	END
FROM dbo.Accounts A
JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
WHERE A.[IsActive]       = 1
  AND A.[IsAutoSelected] = 1  -- excludes variance accounts (MA 2026-01-30)
  AND (  AC.[Node].IsDescendantOf(@PPENode) = 1
      OR AC.[Node].IsDescendantOf(@ROUNode) = 1
      OR AC.[Node].IsDescendantOf(@IANode)  = 1)
  AND AC.[Concept] NOT IN (N'ConstructionInProgress', N'IntangibleAssetsUnderDevelopment');

DECLARE @AccumulatedDepreciationEntryTypeIds IdList;
INSERT INTO @AccumulatedDepreciationEntryTypeIds
SELECT DISTINCT [AccumulatedDepreciationEntryTypeId] FROM @FixedAssetsAccountIds;

-- ============================================================
-- STEP 2a: Fast path — assets that never changed center/agent.
--   These are collapsed to a single summarised row so the
--   recursive CTE below does not have to walk every historical
--   journal entry.
-- ============================================================
DECLARE @SummarizedFixedAssets TABLE (
	[FixedAssetId]  INT,
	[CenterId]      INT,
	[AgentId]       INT,
	[NotedAgentId]  INT,
	[PeriodStart]   DATE,
	[Amount]        DECIMAL(19,6),
	[Quantity]      DECIMAL(19,6),
	INDEX IX1 ([FixedAssetId], [PeriodStart]),
	INDEX IX2 ([PeriodStart]) INCLUDE ([FixedAssetId], [Amount], [Quantity])
);
INSERT INTO @SummarizedFixedAssets (
	[FixedAssetId], [CenterId], [AgentId], [NotedAgentId],
	[PeriodStart], [Amount], [Quantity])
SELECT E.[ResourceId],
	MIN(E.[CenterId])                                                       AS [CenterId],
	MIN(E.[AgentId])                                                        AS [AgentId],
	MIN(E.[NotedAgentId])                                                   AS [NotedAgentId],
	MIN(E.[NotedDate])                                                      AS [PeriodStart],
	SUM(E.[Direction] * E.[MonetaryValue]
	    - E.[Direction] * ISNULL(E.[NotedAmount], 0))                       AS [Amount],
	SUM(E.[Direction] * E.[Quantity])                                       AS [Quantity]
FROM dbo.Entries E
JOIN dbo.Lines          L  ON L.[Id]  = E.[LineId]
JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
JOIN dbo.Resources       R  ON R.[Id]  = E.[ResourceId]
JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
JOIN dbo.Lookups        LK4 ON LK4.[Id] = R.[Lookup4Id]
JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
WHERE L.[State] = 4
  AND R.[Code] <> N'0'
  AND L.[PostingDate] <= DATEADD(DAY, 1, @PostingDate)
  AND LK4.[Code] <> N'NA'
  AND (@ResourceId           IS NULL OR R.[Id]  = @ResourceId)
  AND (@ResourceDefinitionId IS NULL OR RD.[Id] = @ResourceDefinitionId)
GROUP BY E.[ResourceId]
HAVING MIN(E.[CenterId])     = MAX(E.[CenterId])
   AND MIN(E.[AgentId])      = MAX(E.[AgentId])
   AND MIN(E.[NotedAgentId]) = MAX(E.[NotedAgentId]);


-- ============================================================
-- STEP 2b: Cost journal — one row per non-depreciation event.
--   Direction convention:
--     +1  acquisition / positive transfer  (start of NotedDate)
--     -1  disposal    / negative transfer  (start of NotedDate)
--   Depreciation entries are excluded here; they appear in
--   @PostedDepreciations instead.
--   Assets with mid-period transfers (not in @SummarizedFixedAssets)
--   take the full journal path; assets in @SummarizedFixedAssets
--   are represented by a single +1 summarised row.
-- ============================================================
DECLARE @FixedAssetsJournal TABLE (
	[FixedAssetId]  INT,
	[CenterId]      INT,
	[AgentId]       INT,
	[NotedAgentId]  INT,
	[PeriodStart]   DATE,            -- effective start of this event (start of day)
	[Amount]        DECIMAL(19,6),
	[Quantity]      DECIMAL(19,6),
	[Direction]     SMALLINT,
	INDEX IX1 ([FixedAssetId], [PeriodStart]),
	INDEX IX2 ([FixedAssetId], [Direction]) INCLUDE ([CenterId], [AgentId], [NotedAgentId]),
	INDEX IX3 ([PeriodStart]) INCLUDE ([FixedAssetId], [Amount], [Quantity])
);
INSERT INTO @FixedAssetsJournal (
	[FixedAssetId], [CenterId], [AgentId], [NotedAgentId],
	[PeriodStart], [Amount], [Quantity], [Direction])
-- Full-journal path: assets NOT in @SummarizedFixedAssets
SELECT E.[ResourceId],
	E.[CenterId], E.[AgentId], E.[NotedAgentId],
	-- Disposals/negative transfers are bumped one day forward so they
	-- act as a period-end marker at the start of the day after.
	IIF((E.[Direction] = -1 AND E.[Value] = 0
	     OR SIGN(E.[Direction] * E.[Value]) = -1)
	    AND LD.[Code] IN (
	        N'ManualLine',
	        N'ToDepreciationAndAmortisationExpenseFromNoncurrentAssets.E',
	        N'ToAccruedIncomeAndLossesOnDisposalsFromPPEAndGainsOnDisposals'),
	    DATEADD(DAY, 1, E.[NotedDate]),
	    E.[NotedDate])                                                       AS [PeriodStart],
	(E.[Direction] * E.[MonetaryValue]
	 - E.[Direction] * ISNULL(E.[NotedAmount], 0))                          AS [Amount],
	E.[Direction] * E.[Quantity]                                            AS [Quantity],
	IIF(E.[MonetaryValue] = 0 AND E.[Quantity] = 0,
	    E.[Direction],
	    SIGN(E.[Direction] * E.[MonetaryValue]))                             AS [Direction]
FROM dbo.Entries E
JOIN dbo.Lines          L  ON L.[Id]  = E.[LineId]
JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
JOIN dbo.Resources       R  ON R.[Id]  = E.[ResourceId]
JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
JOIN dbo.Lookups        LK4 ON LK4.[Id] = R.[Lookup4Id]
JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
WHERE L.[State] = 4
  AND E.[EntryTypeId] NOT IN (SELECT [Id] FROM @AccumulatedDepreciationEntryTypeIds)
  AND R.[Code] <> N'0'
  AND L.[PostingDate] <= DATEADD(DAY, 1, @PostingDate)
  AND LK4.[Code] <> N'NA'
  AND E.[ResourceId] NOT IN (SELECT [FixedAssetId] FROM @SummarizedFixedAssets)
  AND (@ResourceId           IS NULL OR R.[Id]  = @ResourceId)
  AND (@ResourceDefinitionId IS NULL OR RD.[Id] = @ResourceDefinitionId)
UNION ALL
-- Summarised path: single collapsed row per asset
SELECT [FixedAssetId], [CenterId], [AgentId], [NotedAgentId],
	IIF([PeriodStart] < @StartDate, @StartDate, [PeriodStart]),
	[Amount], [Quantity], +1
FROM @SummarizedFixedAssets
WHERE NOT ([Quantity] = 0 AND ABS([Amount]) < 0.1);


-- ============================================================
-- STEP 3: Compute the active date range per asset.
--   ToDate = earlier of (start of asset + total life) and
--            (day after @PostingDate).
-- ============================================================
DECLARE @AssetDateRanges TABLE (
	[FixedAssetId]  INT PRIMARY KEY,
	[FromDate]      DATE,
	[ToDate]        DATE,
	INDEX IX1 ([FixedAssetId], [FromDate]),
	INDEX IX2 ([FromDate], [ToDate]) INCLUDE ([FixedAssetId])
);
INSERT INTO @AssetDateRanges
SELECT FixedAssetId,
	MIN([PeriodStart]) AS [FromDate],
	CASE
		WHEN DATEADD(MONTH, SUM([Quantity]), MIN([PeriodStart])) <= @PostingDate
			THEN DATEADD(MONTH, SUM([Quantity]), MIN([PeriodStart]))
		ELSE DATEADD(DAY, 1, @PostingDate)
	END AS [ToDate]
FROM @FixedAssetsJournal
GROUP BY [FixedAssetId];


-- ============================================================
-- STEP 4: For each (asset, date), determine the active
--   center/agent/notedAgent — used to assign depreciation to
--   the correct responsibility center.
--   When two journal rows share the same PeriodStart (e.g. an
--   outgoing transfer and an incoming transfer on the same day),
--   Direction DESC ensures the +1 (incoming) row wins, which
--   gives the correct post-transfer state.
-- ============================================================
DECLARE @LastCenterPerDate TABLE (
	[FixedAssetId]  INT,
	[PeriodStart]   DATE,
	[CenterId]      INT,
	[AgentId]       INT,
	[NotedAgentId]  INT,
	PRIMARY KEY ([FixedAssetId], [PeriodStart]),
	INDEX IX1 ([FixedAssetId], [PeriodStart]) INCLUDE ([CenterId], [AgentId], [NotedAgentId])
);
INSERT INTO @LastCenterPerDate
SELECT f.[FixedAssetId], f.[PeriodStart], f.[CenterId], f.[AgentId], f.[NotedAgentId]
FROM (
	SELECT [FixedAssetId], [PeriodStart], [CenterId], [AgentId], [NotedAgentId],
		ROW_NUMBER() OVER (
			PARTITION BY [FixedAssetId], [PeriodStart]
			ORDER BY [Direction] DESC  -- +1 before -1 on same date (MA 2026-03-12)
		) AS rn
	FROM @FixedAssetsJournal
) f
WHERE f.rn = 1;

-- ============================================================
-- STEP 5: Pre-compute the center/agent in effect at each
--   month-end within each asset's active range.
--   This avoids repeated correlated sub-queries later.
-- ============================================================
DECLARE @LastCenterLookup TABLE (
	[FixedAssetId]  INT,
	[ReferenceDate] DATE,
	[CenterId]      INT,
	[AgentId]       INT,
	[NotedAgentId]  INT,
	PRIMARY KEY ([FixedAssetId], [ReferenceDate])
);
INSERT INTO @LastCenterLookup
SELECT c.[FixedAssetId], d.[MonthEndDate], c.[CenterId], c.[AgentId], c.[NotedAgentId]
FROM @AssetDateRanges r
CROSS APPLY dbo.ft_GetMonthEndDates(r.[FromDate], r.[ToDate]) d
OUTER APPLY (
	SELECT TOP 1 lc.*
	FROM @LastCenterPerDate lc
	WHERE lc.[FixedAssetId] = r.[FixedAssetId]
	  AND lc.[PeriodStart] <= d.[MonthEndDate]
	ORDER BY lc.[PeriodStart] DESC
) c;

-- ============================================================
-- STEP 6: Insert month-end period markers (Direction = -1,
--   Amount = 0, Quantity = 0) into the journal.  These act as
--   row separators so the LEAD() window function in the next
--   step can compute PeriodEnd for each cost event.
--   A marker is only inserted when no real journal entry
--   already falls on that date for this asset.
-- ============================================================
INSERT INTO @FixedAssetsJournal (
	[FixedAssetId], [PeriodStart], [CenterId], [AgentId], [NotedAgentId],
	[Amount], [Quantity], [Direction])
SELECT r.[FixedAssetId],
	DATEADD(DAY, 1, d.[MonthEndDate]),
	(SELECT TOP 1 c.[CenterId]    FROM @LastCenterPerDate c
	 WHERE c.[FixedAssetId] = r.[FixedAssetId] AND c.[PeriodStart] <= d.[MonthEndDate]
	 ORDER BY c.[PeriodStart] DESC),
	(SELECT TOP 1 c.[AgentId]     FROM @LastCenterPerDate c
	 WHERE c.[FixedAssetId] = r.[FixedAssetId] AND c.[PeriodStart] <= d.[MonthEndDate]
	 ORDER BY c.[PeriodStart] DESC),
	(SELECT TOP 1 c.[NotedAgentId] FROM @LastCenterPerDate c
	 WHERE c.[FixedAssetId] = r.[FixedAssetId] AND c.[PeriodStart] <= d.[MonthEndDate]
	 ORDER BY c.[PeriodStart] DESC),
	0, 0, -1
FROM @AssetDateRanges r
CROSS APPLY dbo.ft_GetMonthEndDates(r.[FromDate], r.[ToDate]) d
WHERE NOT EXISTS (
	SELECT 1 FROM @FixedAssetsJournal faj
	WHERE faj.[FixedAssetId] = r.[FixedAssetId]
	  AND faj.[PeriodStart]  = DATEADD(DAY, 1, d.[MonthEndDate]))
ORDER BY r.[FixedAssetId], d.[MonthEndDate] OPTION (RECOMPILE);

-- ============================================================
-- STEP 7: Build @PeriodData — one row per contiguous period
--   within each asset's life.  PeriodEnd is derived via LEAD().
--   PeriodUsage is the fraction of a month elapsed in that
--   period (1.0 for full months, <1.0 for partial months).
--   IsValidPeriod: a period is valid only if Start <= End and
--   Usage is in [0,1]; otherwise the row is a sentinel/marker
--   and carries no depreciation.
--   NOTE: ORDER BY PeriodStart, Direction DESC ensures
--   incoming transfers (+1) are numbered before same-day
--   outgoing transfers (-1), preserving correct book-value
--   sequencing in the recursive CTE that follows.
-- ============================================================
DECLARE @PeriodData TABLE (
	[FixedAssetId]              INT,
	[RowNum]                    INT,
	[PeriodStart]               DATE,
	[Amount]                    DECIMAL(19,6),
	[Quantity]                  DECIMAL(19,6),
	[PeriodEnd]                 DATE,
	[PeriodUsage]               DECIMAL(19,6),
	[DepreciationCenterId]      INT,
	[DepreciationAgentId]       INT,
	[DepreciationNotedAgentId]  INT,
	[MonthDiff]    AS DATEDIFF(MONTH, [PeriodStart], [PeriodEnd]) PERSISTED,
	[IsValidPeriod] AS CASE WHEN [PeriodStart] <= [PeriodEnd]
	                         AND [PeriodUsage] BETWEEN 0 AND 1
	                        THEN 1 ELSE 0 END PERSISTED,
	PRIMARY KEY ([FixedAssetId], [RowNum]),
	INDEX IX1 ([FixedAssetId], [PeriodStart]) INCLUDE ([PeriodEnd])
);
INSERT INTO @PeriodData (
	[RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd],
	[Amount], [Quantity], [PeriodUsage],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT
	ROW_NUMBER() OVER (PARTITION BY [FixedAssetId]
	                   ORDER BY [PeriodStart], [Direction] DESC) AS [RowNum],
	[FixedAssetId], [PeriodStart],
	DATEADD(DAY, -1,
		ISNULL(LEAD([PeriodStart]) OVER (PARTITION BY [FixedAssetId]
		                                 ORDER BY [PeriodStart]),
		       DATEADD(DAY, 1, @PostingDate))) AS [PeriodEnd],
	[Amount], [Quantity],
	CASE
		WHEN R.[UnitId] = @DayUnit
			THEN [Quantity]
		ELSE dbo.fn_DateDiffWithPrecision_V2(@MonthUnit, [PeriodStart],
			DATEADD(DAY, -1,
				ISNULL(LEAD([PeriodStart]) OVER (PARTITION BY [FixedAssetId]
				                                 ORDER BY [PeriodStart]),
				       DATEADD(DAY, 1, @PostingDate))))
	END AS [PeriodUsage],
	lc.[CenterId], lc.[AgentId], lc.[NotedAgentId]
FROM @FixedAssetsJournal faj
JOIN dbo.Resources R ON R.[Id] = faj.[FixedAssetId]
OUTER APPLY (
	SELECT TOP 1 lc.[CenterId], lc.[AgentId], lc.[NotedAgentId]
	FROM @LastCenterPerDate lc
	WHERE lc.[FixedAssetId] = faj.[FixedAssetId]
	  AND lc.[PeriodStart] <= faj.[PeriodStart]
	ORDER BY lc.[PeriodStart] DESC
) lc
OPTION (RECOMPILE);

-- ============================================================
-- STEP 8: Recursive CTE — compute straight-line depreciation
--   for every valid period.
--
--   For each period:
--     BookMinusResidual  = book value available for depreciation
--                          at the start of this period
--     RemainingLifeTime  = months of life remaining at start
--     PeriodDepreciation = BookMinusResidual × PeriodUsage
--                          / RemainingLifeTime
--
--   Guards (MA 2026-03-01):
--     GREATEST(0, ...) on carry-forward values ensures
--     rounding cannot drive RemainingLifeTime or BookMinusResidual
--     below zero, preventing a sign-flip cascade in subsequent
--     periods.
--     LEAST(PeriodDepreciation, BookMinusResidual) in the final
--     INSERT caps each period's charge at the remaining balance.
-- ============================================================
DECLARE @FixedAssetsDepreciations TABLE (
	[RowNum]                    INT,
	[FixedAssetId]              INT,
	[PeriodStart]               DATE,
	[PeriodEnd]                 DATE,         -- = NotedDate of the depreciation entry
	[BookMinusResidual]         DECIMAL(19,6),
	[RemainingLifeTime]         DECIMAL(19,6),
	[PeriodUsage]               DECIMAL(19,6),
	[PeriodDepreciation]        DECIMAL(19,6),
	[DepreciationCenterId]      INT,
	[DepreciationAgentId]       INT,
	[DepreciationNotedAgentId]  INT,
	PRIMARY KEY ([FixedAssetId], [RowNum]),
	INDEX IX1 ([FixedAssetId], [PeriodStart], [PeriodEnd]),
	INDEX IX2 ([FixedAssetId]) INCLUDE ([PeriodDepreciation], [PeriodUsage])
);
WITH BookValueRecursive AS (
	-- Anchor: first period for each asset
	SELECT
		[RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd], [Amount],
		CAST([Quantity]  AS DECIMAL(19,6)) AS [RemainingLifeTime],
		IIF([IsValidPeriod] = 1, [PeriodUsage], 0) AS [PeriodUsage],
		CAST([Amount]    AS DECIMAL(19,6)) AS [BookMinusResidual],
		CASE WHEN [Quantity] = 0 THEN CAST(0 AS DECIMAL(19,6))
		     ELSE CAST(([Amount] * [PeriodUsage]) / [Quantity] AS DECIMAL(19,6))
		END AS [PeriodDepreciation],
		[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId],
		[IsValidPeriod]
	FROM @PeriodData WHERE [RowNum] = 1

	UNION ALL

	-- Recursive: each subsequent period
	SELECT
		p.[RowNum], p.[FixedAssetId], p.[PeriodStart], p.[PeriodEnd], p.[Amount],
		-- Carry-forward remaining lifetime; floor at zero to prevent sign-flip
		CAST(p.[Quantity] + GREATEST(0, b.[RemainingLifeTime] - b.[PeriodUsage])
		     AS DECIMAL(19,6)) AS [RemainingLifeTime],
		IIF(p.[IsValidPeriod] = 1, p.[PeriodUsage], 0) AS [PeriodUsage],
		-- Carry-forward book value; floor at zero to prevent sign-flip
		CAST(GREATEST(0, p.[Amount] + b.[BookMinusResidual] - b.[PeriodDepreciation])
		     AS DECIMAL(19,6)) AS [BookMinusResidual],
		CASE
			WHEN (p.[Quantity] + GREATEST(0, b.[RemainingLifeTime] - b.[PeriodUsage])) = 0
				THEN CAST(0 AS DECIMAL(19,6))
			ELSE CAST(
				(GREATEST(0, p.[Amount] + b.[BookMinusResidual] - b.[PeriodDepreciation])
				 * p.[PeriodUsage])
				/ (p.[Quantity] + GREATEST(0, b.[RemainingLifeTime] - b.[PeriodUsage]))
			AS DECIMAL(19,6))
		END AS [PeriodDepreciation],
		p.[DepreciationCenterId], p.[DepreciationAgentId], p.[DepreciationNotedAgentId],
		p.[IsValidPeriod]
	FROM @PeriodData p
	INNER JOIN BookValueRecursive b
		ON p.[RowNum] = b.[RowNum] + 1
	   AND p.[FixedAssetId] = b.[FixedAssetId]
)
INSERT INTO @FixedAssetsDepreciations (
	[RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd],
	[BookMinusResidual], [RemainingLifeTime],
	[PeriodUsage], [PeriodDepreciation],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT
	[RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd],
	[BookMinusResidual], [RemainingLifeTime],
	ROUND(IIF([RemainingLifeTime] >= [PeriodUsage], [PeriodUsage], [RemainingLifeTime]), 4),
	-- Cap period charge at remaining book value to prevent over-depreciation
	ROUND(LEAST([PeriodDepreciation], GREATEST(0, [BookMinusResidual])), 2),
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
FROM BookValueRecursive
WHERE [IsValidPeriod] = 1
OPTION (MAXRECURSION 0);

-- ============================================================
-- STEP 9: Read posted depreciation entries.
--
--   V4 CHANGE vs V3: grouped by NotedDate instead of Time1/Time2.
--
--   Rationale: The canonical identity of a depreciation entry
--   in the asset ledger is (Asset, NotedDate, CenterId, AgentId,
--   NotedAgentId).  NotedDate = the date at end-of-day on which
--   the depreciation took effect.  Time1/Time2 carry period
--   information useful for P&L reporting but are not always
--   populated (e.g. manual write-off journals such as JV0789
--   may have NULL Time1/Time2).  Using NotedDate ensures all
--   depreciation entries — regular, accelerated, and manual —
--   are visible to the variance calculation.
-- ============================================================
DECLARE @PostedDepreciations TABLE (
	[FixedAssetId]              INT,
	[NotedDate]                 DATE,   -- V4: replaces PeriodStart + PeriodEnd
	[PeriodUsage]               DECIMAL(19,6),
	[PeriodDepreciation]        DECIMAL(19,6),
	[DepreciationCenterId]      INT,
	[DepreciationAgentId]       INT,
	[DepreciationNotedAgentId]  INT,
	INDEX IX ([FixedAssetId], [NotedDate])
);
INSERT INTO @PostedDepreciations (
	[FixedAssetId], [NotedDate],
	[PeriodUsage], [PeriodDepreciation],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT
	E.[ResourceId],
	E.[NotedDate],                          -- V4: was E.[Time1], E.[Time2]
	SUM(E.[Direction] * E.[Quantity])       AS [PeriodUsage],
	SUM(E.[Direction] * E.[MonetaryValue])  AS [PeriodDepreciation],
	E.[CenterId], E.[AgentId], E.[NotedAgentId]
FROM dbo.Entries E
JOIN dbo.Lines           L  ON L.[Id]  = E.[LineId]
JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
WHERE L.[State] = 4
  AND LD.[Code] <> N'ToIncomeStatementAbstractFromRetainedEarnings'
  AND L.[PostingDate] <= @PostingDate
  AND E.[ResourceId] IN (SELECT [FixedAssetId] FROM @FixedAssetsDepreciations)
  AND E.[EntryTypeId] IN (SELECT [Id] FROM @AccumulatedDepreciationEntryTypeIds)
GROUP BY E.[ResourceId], E.[NotedDate],   -- V4: was E.[Time1], E.[Time2]
         E.[CenterId], E.[AgentId], E.[NotedAgentId];

-- ============================================================
-- STEP 10: Compute variance = computed − posted (per asset,
--   per NotedDate, per responsibility assignment).
--
--   V4 CHANGE vs V3: UNION ALL now aligns on NotedDate.
--     Computed side: PeriodEnd becomes NotedDate.
--     Posted   side: NotedDate is used directly.
--   PeriodStart (nullable) is retained from the computed side
--   so it can be written to Time1 of the expense-account entry
--   in @Widelines.
--
--   Rows where the net variance is zero (or within rounding
--   tolerance of 0.10) are suppressed.
-- ============================================================
DECLARE @VarianceDepreciations TABLE (
	[FixedAssetId]              INT,
	[PeriodStart]               DATE NULL,  -- from computed side; NULL for posted-only rows
	[NotedDate]                 DATE,       -- V4: replaces PeriodEnd
	[UsageVariance]             DECIMAL(19,6),
	[DepreciationVariance]      DECIMAL(19,6),
	[DepreciationCenterId]      INT,
	[DepreciationAgentId]       INT,
	[DepreciationNotedAgentId]  INT,
	[DepreciationEntryTypeId]   INT,        -- populated downstream if needed
	INDEX IX ([FixedAssetId], [PeriodStart], [NotedDate])
);
INSERT INTO @VarianceDepreciations (
	[FixedAssetId], [PeriodStart], [NotedDate],
	[UsageVariance], [DepreciationVariance],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT
	[FixedAssetId],
	MAX([PeriodStart])           AS [PeriodStart],   -- non-NULL only from computed side
	[NotedDate],
	SUM([PeriodUsage])           AS [UsageVariance],
	SUM([PeriodDepreciation])    AS [DepreciationVariance],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
FROM (
	-- Computed schedule (positive values)
	SELECT [FixedAssetId], [PeriodStart],
	       [PeriodEnd]          AS [NotedDate],   -- PeriodEnd = NotedDate of entry
	       [PeriodUsage], [PeriodDepreciation],
	       [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
	FROM @FixedAssetsDepreciations
	UNION ALL
	-- Posted entries (stored as negative values; they cancel the computed)
	SELECT [FixedAssetId], NULL AS [PeriodStart],
	       [NotedDate],
	       [PeriodUsage], [PeriodDepreciation],
	       [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
	FROM @PostedDepreciations
) T
GROUP BY [FixedAssetId], [NotedDate],
         [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
HAVING SUM([PeriodUsage]) <> 0 OR ABS(SUM([PeriodDepreciation])) > 0.1;

DELETE VD
FROM @VarianceDepreciations VD
WHERE EXISTS (
    SELECT 1
    FROM (
        SELECT [FixedAssetId],
               SUM([PeriodDepreciation]) AS TotalComputed
        FROM @FixedAssetsDepreciations
        GROUP BY [FixedAssetId]
    ) C
    JOIN (
        SELECT [FixedAssetId],
               ABS(SUM([PeriodDepreciation])) AS TotalPosted
        FROM @PostedDepreciations
        GROUP BY [FixedAssetId]
    ) P ON P.[FixedAssetId] = C.[FixedAssetId]
    WHERE C.[FixedAssetId] = VD.[FixedAssetId]
      AND P.[TotalPosted] >= C.[TotalComputed] - 0.1  -- rounding tolerance
);

-- Suppress current-period variance rows for assets whose computed
-- depreciation for the current period is zero (fully depreciated or
-- not yet started). Any non-zero posted entries at this date are
-- correction/reallocation entries that must not be reversed.
DELETE VD
FROM @VarianceDepreciations VD
WHERE VD.[NotedDate] >= @StartDate
  AND NOT EXISTS (
    SELECT 1
    FROM @FixedAssetsDepreciations FAD
    WHERE FAD.[FixedAssetId] = VD.[FixedAssetId]
      AND FAD.[PeriodStart]  >= @StartDate
      AND FAD.[PeriodDepreciation] > 0   -- there IS genuine depreciation this period
  );

-- ============================================================
-- STEP 11: Build Widelines output.
--
--   Each variance row becomes one double-sided journal line:
--     Side 0 (Direction = +1): Depreciation expense account.
--       Time1 = PeriodStart, Time2 = NotedDate  (for P&L period)
--     Side 1 (Direction = -1): Fixed asset / accumulated depr.
--       Time1 = NULL, Time2 = NULL               (not needed per
--       asset-ledger semantics; NotedDate carries the date)
--       NotedDate1 = NotedDate
--
--   V4 CHANGE vs V3:
--     Asset side now has NULL Time1/Time2 and relies solely on
--     NotedDate1, consistent with how all other cost events are
--     recorded against the asset account.
--
--   Only rows whose NotedDate falls within the current posting
--   month (@StartDate .. @PostingDate) are emitted.
-- ============================================================
--DECLARE @Widelines WidelineList;
INSERT INTO @Widelines (
	[Index], [DocumentIndex], [PostingDate],
	[Direction0], [CenterId0], [AgentId0], [ResourceId0], [NotedAgentId0],
	[Quantity0], [UnitId0], [MonetaryValue0], [CurrencyId0],
	[Time10], [Time20], [EntryTypeId0],
	[Direction1], [CenterId1], [AgentId1], [ResourceId1], [NotedAgentId1],
	[Quantity1], [UnitId1], [MonetaryValue1], [CurrencyId1],
	[Time11], [Time21], [NotedDate1], [EntryTypeId1])
SELECT
	ROW_NUMBER() OVER (ORDER BY [FixedAssetId], [PeriodStart], [NotedDate]) - 1 AS [Index],
	0                                                       AS [DocumentIndex],
	[NotedDate]                                             AS [PostingDate],
	-- Side 0: depreciation expense
	+1,
	[DepreciationCenterId], [DepreciationAgentId], [FixedAssetId], [DepreciationNotedAgentId],
	[UsageVariance],  R.[UnitId],  [DepreciationVariance],  R.[CurrencyId],
	[PeriodStart],    [NotedDate],                           -- Time1, Time2 for P&L period
	bll.fn_Center__EntryType([DepreciationCenterId], NULL)  AS [EntryTypeId0],
	-- Side 1: asset / accumulated depreciation account
	-1,
	[DepreciationCenterId], [DepreciationAgentId], [FixedAssetId], [DepreciationNotedAgentId],
	[UsageVariance],  R.[UnitId],  [DepreciationVariance],  R.[CurrencyId],
	NULL, NULL,                                              -- V4: Time1/Time2 not needed for asset account
	[NotedDate]                                             AS [NotedDate1],
	CASE
		WHEN RD.[ResourceDefinitionType] = N'PropertyPlantAndEquipment'
			THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN RD.[ResourceDefinitionType] = N'InvestmentProperty'
			THEN dal.fn_EntryTypeConcept__Id(N'DepreciationInvestmentProperty')
		WHEN RD.[ResourceDefinitionType] = N'IntangibleAssetsOtherThanGoodwill'
			THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
	END AS [EntryTypeId1]
FROM @VarianceDepreciations VD
JOIN dbo.Resources          R  ON VD.[FixedAssetId] = R.[Id]
JOIN dbo.ResourceDefinitions RD ON R.[DefinitionId]  = RD.[Id]
WHERE [NotedDate] >= @StartDate   -- only emit entries for the current posting month
OPTION (RECOMPILE);

	RETURN
END
