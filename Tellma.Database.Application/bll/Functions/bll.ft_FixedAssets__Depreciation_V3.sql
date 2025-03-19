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
DECLARE @MonthUnit INT = dal.fn_UnitCode__Id(N'mo'), @DayUnit INT = dal.fn_UnitCode__Id(N'd');
Set @StartDate = dbo.fn_MonthStart(@PostingDate)

DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
DECLARE @ROUNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'RightofuseAssets');
DECLARE @IPCNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyCompleted');
DECLARE @IANode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');
DECLARE @FunctionalCurrencyId NCHAR (3) = dal.fn_FunctionalCurrencyId();

DECLARE @FixedAssetsAccountIds TABLE (
	[FixedAssetAccountId] INT PRIMARY KEY,
	[AccumulatedDepreciationEntryTypeId] INT,
	INDEX IX1 ([AccumulatedDepreciationEntryTypeId]) -- For lookups in IN clause
)
INSERT INTO @FixedAssetsAccountIds([FixedAssetAccountId] , [AccumulatedDepreciationEntryTypeId])
SELECT A.[Id] AS [FixedAssetAccountId], 
CASE
		WHEN AC.[Node].IsDescendantOf(@PPENode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN AC.[Node].IsDescendantOf(@ROUNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN AC.[Node].IsDescendantOf(@IPCNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationInvestmentProperty')
		WHEN AC.[Node].IsDescendantOf(@IANode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
		ELSE NULL
END AS [AccumulatedDepreciationEntryTypeId]
FROM dbo.[Accounts] A
JOIN dbo.[AccountTypes] AC ON AC.[Id] = A.[AccountTypeId]
WHERE A.[IsActive] = 1
AND (
	AC.[Node].IsDescendantOf(@PPENode) = 1 OR
	AC.[Node].IsDescendantOf(@ROUNode) = 1 OR
	AC.[Node].IsDescendantOf(@IANode) = 1
);--select *, dal.fn_Account__Name([FixedAssetAccountId]) AS AccountName from @FixedAssetsAccountIds;

DECLARE @AccumulatedDepreciationEntryTypeIds IdList;
INSERT INTO @AccumulatedDepreciationEntryTypeIds 
SELECT DISTINCT [AccumulatedDepreciationEntryTypeId] FROM @FixedAssetsAccountIds;

DECLARE @SummarizedFixedAssets TABLE (
	[FixedAssetId] INT,
	[CenterId] INT,
	[AgentId] INT,
	[NotedAgentId] INT,
	[PeriodStart] DATE,
	[Amount] DECIMAL (19, 6),
	[Quantity] DECIMAL (19, 6),
	INDEX IX1 ([FixedAssetId], [PeriodStart]),
	INDEX IX2 ([PeriodStart]) INCLUDE ([FixedAssetId], [Amount], [Quantity]) -- For aggregations
);
INSERT INTO @SummarizedFixedAssets([FixedAssetId], [CenterId], [AgentId], [NotedAgentId], [PeriodStart], [Amount], [Quantity]) 
SELECT E.[ResourceId] As [FixedAssetId], 
		MIN(E.[CenterId]) AS [CenterId], 
		MIN(E.[AgentId]) AS [AgentId], 
		MIN(E.[NotedAgentId]) AS [NotedAgentId], 
		MIN(E.[NotedDate]) AS [PeriodStart],
		SUM((E.[Direction] * E.[MonetaryValue] - E.[Direction] * ISNULL(E.[NotedAmount], 0))) AS [Amount],
		SUM(E.[Direction] * E.[Quantity]) AS [Quantity]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
WHERE L.[State] = 4
AND R.[Code] <> N'0' -- Inactive FA should not be excluded if they have balance.
AND L.[PostingDate] <= DATEADD(DAY, 1, @PostingDate)
AND (@ResourceId IS NULL OR R.[Id] = @ResourceId)
AND (@ResourceDefinitionId IS NULL OR RD.[Id] = @ResourceDefinitionId)
GROUP BY E.[ResourceId]
HAVING MIN(E.[CenterId]) = MAX(E.[CenterId])
AND MIN(E.[AgentId]) = MAX(E.[AgentId])
AND MIN(E.[NotedAgentId]) = MAX(E.[NotedAgentId])

DECLARE @FixedAssetsJournal TABLE  (
	[FixedAssetId] INT,
	[CenterId] INT,
	[AgentId] INT,
	[NotedAgentId] INT,
	[PeriodStart] DATE,
	[Amount] DECIMAL (19, 6), 
	[Quantity] DECIMAL (19, 6),
	[Direction] SMALLINT,
	INDEX IX1 ([FixedAssetId], [PeriodStart]),
	INDEX IX2 ([FixedAssetId], [Direction]) INCLUDE ([CenterId], [AgentId], [NotedAgentId]), -- For the ROW_NUMBER partition    
	INDEX IX3 ([PeriodStart]) INCLUDE ([FixedAssetId], [Amount], [Quantity]) -- For aggregations
);
INSERT INTO @FixedAssetsJournal([FixedAssetId], [CenterId], [AgentId], [NotedAgentId], [PeriodStart], [Amount], [Quantity], [Direction])
SELECT E.[ResourceId] As [FixedAssetId], E.[CenterId], E.[AgentId], E.[NotedAgentId],
		IIF((E.[Direction] = -1 AND E.[Value] = 0 OR SIGN(E.[Direction] * E.[Value]) = -1)
			AND LD.[Code] IN (N'ManualLine', N'ToDepreciationAndAmortisationExpenseFromNoncurrentAssets.E', 
								N'ToAccruedIncomeAndLossesOnDisposalsFromPPEAndGainsOnDisposals'),
			DATEADD(DAY, 1, E.[NotedDate]), E.[NotedDate]
		) AS [PeriodStart],
		(E.[Direction] * E.[MonetaryValue] - E.[Direction] * ISNULL(E.[NotedAmount], 0)) AS [Amount],
		E.[Direction] * E.[Quantity] As [Quantity],		
		IIF(E.[MonetaryValue] = 0 AND E.[Quantity] = 0, E.[Direction], SIGN(E.[Direction] * E.[MonetaryValue])) AS [Direction]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
WHERE L.[State] = 4
AND E.[EntryTypeId] NOT IN (SELECT [Id] FROM @AccumulatedDepreciationEntryTypeIds)
AND R.[Code] <> N'0' -- Inactive FA should not be excluded if they have balance.
AND L.[PostingDate] <= DATEADD(DAY, 1, @PostingDate)
AND E.[ResourceId] NOT IN (SELECT [FixedAssetId] FROM @SummarizedFixedAssets)
AND (@ResourceId IS NULL OR R.[Id] = @ResourceId)
AND (@ResourceDefinitionId IS NULL OR RD.[Id] = @ResourceDefinitionId)
UNION ALL
SELECT [FixedAssetId], [CenterId], [AgentId], [NotedAgentId], IIF([PeriodStart] < @StartDate, @StartDate, [PeriodStart]), [Amount], [Quantity], +1 AS [Direction]
FROM @SummarizedFixedAssets
WHERE NOT ([Quantity] = 0 AND ABS([Amount]) < 0.1)
;

-- for each asset, month take from last 
DECLARE @AssetDateRanges TABLE  (
	[FixedAssetId] INT PRIMARY KEY,
	[FromDate] DATE,
	[ToDate] DATE,
	INDEX IX1 ([FixedAssetId], [FromDate]),    INDEX IX2 ([FromDate], [ToDate]) INCLUDE ([FixedAssetId])
);
INSERT INTO @AssetDateRanges
-- Get FromDate, ToDate and TillDate for each FixedAssetId
SELECT 
    FixedAssetId,
    MIN(PeriodStart) AS FromDate,
    CASE 
        WHEN DATEADD(MONTH, SUM([Quantity]), MIN(PeriodStart)) <= @PostingDate THEN DATEADD(MONTH, SUM([Quantity]), MIN(PeriodStart))
        ELSE DATEADD(DAY, 1, @PostingDate)
    END AS ToDate
FROM @FixedAssetsJournal
GROUP BY FixedAssetId

DECLARE @LastCenterPerDate TABLE  (
	[FixedAssetId] INT,
	[PeriodStart] DATE,
	[CenterId] INT,
	[AgentId] INT,
	[NotedAgentId] INT,
    PRIMARY KEY ([FixedAssetId], [PeriodStart]),
	INDEX IX1 ([FixedAssetId], [PeriodStart]) INCLUDE ([CenterId], [AgentId], [NotedAgentId])
);
INSERT INTO @LastCenterPerDate
    -- Get the last CenterId for each FixedAssetId and PeriodStart
    -- When there are two entries on same date, take the one with highest Direction
SELECT 
    f.[FixedAssetId],
    f.[PeriodStart],
    f.[CenterId],
	f.[AgentId],
	f.[NotedAgentId]
FROM (
    SELECT 
        [FixedAssetId],
        [PeriodStart],
        [CenterId],
		[AgentId],
		[NotedAgentId],
        ROW_NUMBER() OVER (
            PARTITION BY FixedAssetId, PeriodStart 
            ORDER BY Direction DESC
        ) AS rn
    FROM @FixedAssetsJournal
) f
WHERE f.rn = 1

--1s
-- Pre-calculate the last centers
DECLARE @LastCenterLookup TABLE  (
    [FixedAssetId] INT,
    [ReferenceDate] DATE,
    [CenterId] INT,
    [AgentId] INT,
    [NotedAgentId] INT,
    PRIMARY KEY ([FixedAssetId], [ReferenceDate])
);
INSERT INTO @LastCenterLookup
SELECT 
    c.FixedAssetId,
    d.MonthEndDate,
    c.CenterId,
    c.AgentId,
    c.NotedAgentId
FROM @AssetDateRanges r
CROSS APPLY dbo.ft_GetMonthEndDates(r.FromDate, r.ToDate) d
OUTER APPLY (
    SELECT TOP 1 lc.*
    FROM @LastCenterPerDate lc
    WHERE lc.FixedAssetId = r.FixedAssetId
      AND lc.PeriodStart <= d.MonthEndDate
    ORDER BY lc.PeriodStart DESC
) c;

--1s
INSERT INTO @FixedAssetsJournal([FixedAssetId],[PeriodStart], [CenterId], [AgentId], [NotedAgentId], [Amount], [Quantity], [Direction])
SELECT-- N'@FixedAssetsJournal' AS [Table], 
    r.FixedAssetId,
    DATEADD(DAY, 1, d.MonthEndDate) AS EndOfMonth,
    (
        SELECT TOP 1 c.[CenterId]
        FROM @LastCenterPerDate c
        WHERE c.FixedAssetId = r.FixedAssetId
          AND c.PeriodStart <= d.MonthEndDate
        ORDER BY c.PeriodStart DESC
    ) AS DepreciationCenterId, 
	(
        SELECT TOP 1 c.[AgentId]
        FROM @LastCenterPerDate c
        WHERE c.FixedAssetId = r.FixedAssetId
          AND c.PeriodStart <= d.MonthEndDate
        ORDER BY c.PeriodStart DESC
    ) AS DepreciationAgentId, 
	(
        SELECT TOP 1 c.[NotedAgentId]
        FROM @LastCenterPerDate c
        WHERE c.FixedAssetId = r.FixedAssetId
          AND c.PeriodStart <= d.MonthEndDate
        ORDER BY c.PeriodStart DESC
    ) AS DepreciationNotedAgentId,
	0 As [Amount], 0 AS [Quantity], -1 AS [Direction]
FROM @AssetDateRanges r
CROSS APPLY dbo.ft_GetMonthEndDates(r.FromDate, r.ToDate) d
WHERE NOT EXISTS (
    SELECT 1 
    FROM @FixedAssetsJournal faj 
    WHERE faj.FixedAssetId = r.FixedAssetId 
      AND faj.PeriodStart =  DATEADD(DAY, 1, d.MonthEndDate)
)
ORDER BY r.FixedAssetId, d.MonthEndDate OPTION(RECOMPILE);

-- 6s

DECLARE @PeriodData TABLE  (
    [FixedAssetId] INT,
    [RowNum] INT,
    [PeriodStart] DATE,
    [Amount] DECIMAL (19, 6), 
    [Quantity] DECIMAL (19, 6),
    [PeriodEnd] DATE,
    [PeriodUsage] DECIMAL (19, 6),
    [DepreciationCenterId] INT,
    [DepreciationAgentId] INT,
    [DepreciationNotedAgentId] INT,
    [MonthDiff] AS DATEDIFF(MONTH, [PeriodStart], [PeriodEnd]) PERSISTED,
    [IsValidPeriod] AS CASE WHEN [PeriodStart] <= [PeriodEnd] AND [PeriodUsage] BETWEEN 0 AND 1 THEN 1 ELSE 0 END PERSISTED,
    PRIMARY KEY ([FixedAssetId],[RowNum]),	INDEX IX1 ([FixedAssetId], [PeriodStart]) INCLUDE ([PeriodEnd])
);
INSERT INTO @PeriodData(
        [RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd], 
        [Amount], [Quantity], [PeriodUsage],
        [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
    )
    SELECT
        ROW_NUMBER() OVER (PARTITION BY [FixedAssetId] ORDER BY PeriodStart) AS [RowNum],
        [FixedAssetId], [PeriodStart], 
        DATEADD(day, -1, ISNULL(LEAD(PeriodStart) OVER (PARTITION BY FixedAssetId ORDER BY PeriodStart), DATEADD(DAY, 1, @PostingDate))) AS [PeriodEnd],
        [Amount], [Quantity],
		CASE
			WHEN R.[UnitId] = @DayUnit THEN [Quantity]
			ELSE dbo.fn_DateDiffWithPrecision_V2(@MonthUnit, [PeriodStart], 
				DATEADD(day, -1, ISNULL(LEAD(PeriodStart) OVER (PARTITION BY FixedAssetId ORDER BY PeriodStart), DATEADD(DAY, 1, @PostingDate))))
		END AS [PeriodUsage],
        lc.CenterId,
        lc.AgentId,
        lc.NotedAgentId
    FROM @FixedAssetsJournal faj
	JOIN dbo.Resources R ON R.[Id] = faj.[FixedAssetId]
    OUTER APPLY (
        SELECT TOP 1 lc.CenterId, lc.AgentId, lc.NotedAgentId
        FROM @LastCenterPerDate lc
        WHERE lc.FixedAssetId = faj.FixedAssetId
          AND lc.PeriodStart <= faj.PeriodStart
        ORDER BY lc.PeriodStart DESC
    ) lc OPTION (RECOMPILE);
--72 s
DECLARE @FixedAssetsDepreciations TABLE  (
	[RowNum]					INT,
	[FixedAssetId]				INT,
	[PeriodStart]				DATE, 
	[PeriodEnd]					DATE,
	[BookMinusResidual]			DECIMAL (19, 6), -- till Period Start, exclusive
	[RemainingLifeTime]			DECIMAL (19, 6), -- till Period Start, exclusive
	[PeriodUsage]				DECIMAL (19, 6), -- Period Start to Period End, both inclusive
	[PeriodDepreciation]		DECIMAL (19, 6), -- for straight line, [BookMinusResidual] * [Period Usage] / [Remaining Life Time]
	[DepreciationCenterId]		INT, -- As of Period Start, exclusive
	[DepreciationAgentId]		INT, -- As of Period Start, exclusive
	[DepreciationNotedAgentId]	INT, -- As of Period Start, exclusive
	PRIMARY KEY ([FixedAssetId], [RowNum]),
	INDEX IX1 ([FixedAssetId], [PeriodStart], [PeriodEnd]),
	INDEX IX2 ([FixedAssetId]) INCLUDE ([PeriodDepreciation], [PeriodUsage])
);
WITH BookValueRecursive AS (
    -- Anchor: Calculate first row
    SELECT 
        [RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd], [Amount],
        CAST([Quantity] AS DECIMAL(19,6)) AS [RemainingLifeTime],
        IIF([IsValidPeriod] = 1, [PeriodUsage], 0) AS [PeriodUsage],
        CAST([Amount] AS DECIMAL(19,6)) AS [BookMinusResidual],
        CASE 
            WHEN [Quantity] = 0 THEN CAST(0 AS DECIMAL(19,6))
            ELSE CAST(([Amount] * [PeriodUsage]) / [Quantity] AS DECIMAL(19,6))
        END AS [PeriodDepreciation],
        [DepreciationCenterId],
        [DepreciationAgentId],
        [DepreciationNotedAgentId],
		[IsValidPeriod]
    FROM @PeriodData
    WHERE [RowNum] = 1

    UNION ALL
    -- Recursive: Calculate subsequent rows
    SELECT
        p.[RowNum], p.[FixedAssetId], p.[PeriodStart], p.[PeriodEnd], p.[Amount],
        CAST(p.[Quantity] + (b.[RemainingLifeTime] - b.[PeriodUsage]) AS DECIMAL(19,6)) AS [RemainingLifeTime],
		IIF(p.[IsValidPeriod] = 1, p.[PeriodUsage], 0) AS [PeriodUsage],
		CAST(p.[Amount] + b.[BookMinusResidual] - b.[PeriodDepreciation] AS DECIMAL(19,6)) AS [BookMinusResidual],
        CASE 
            WHEN (p.[Quantity] + (b.[RemainingLifeTime] - b.[PeriodUsage])) = 0 THEN CAST(0 AS DECIMAL(19,6))
			ELSE CAST(((p.[Amount] + b.[BookMinusResidual] - b.[PeriodDepreciation]) * p.[PeriodUsage]) / (p.[Quantity] + (b.[RemainingLifeTime] - b.[PeriodUsage])) AS DECIMAL(19,6))
        END AS [PeriodDepreciation],
        p.[DepreciationCenterId],
        p.[DepreciationAgentId],
        p.[DepreciationNotedAgentId],
		p.[IsValidPeriod]
    FROM @PeriodData p
    INNER JOIN BookValueRecursive b ON p.[RowNum] = b.[RowNum] + 1 AND p.[FixedAssetId] = b.[FixedAssetId]
)
INSERT INTO @FixedAssetsDepreciations([RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd], [BookMinusResidual], [RemainingLifeTime],
	[PeriodUsage], [PeriodDepreciation], [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT [RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd], [BookMinusResidual], [RemainingLifeTime],
    ROUND(IIF([RemainingLifeTime] >= [PeriodUsage], [PeriodUsage], [RemainingLifeTime]), 4) AS [PeriodUsage],
	ROUND([PeriodDepreciation], 2), [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
FROM BookValueRecursive
WHERE [IsValidPeriod] = 1
OPTION (MAXRECURSION 0);

DECLARE @PostedDepreciations TABLE  (
	[FixedAssetId]				INT,
	[PeriodStart]				DATE, 
	[PeriodEnd]					DATE,
	[PeriodUsage]				DECIMAL (19, 6), -- Period Start to Period End, both inclusive
	[PeriodDepreciation]		DECIMAL (19, 6), -- for straight line, [BookMinusResidual] * [Period Usage] / [Remaining Life Time]
	[DepreciationCenterId]		INT, -- As of Period Start, exclusive
	[DepreciationAgentId]		INT, -- As of Period Start, exclusive
	[DepreciationNotedAgentId]	INT, -- As of Period Start, exclusive
	INDEX IX ([FixedAssetId], [PeriodStart], [PeriodEnd])
);
INSERT INTO @PostedDepreciations([FixedAssetId], [PeriodStart], [PeriodEnd], [PeriodUsage], [PeriodDepreciation],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT E.[ResourceId] AS [FixedAssetId], E.[Time1] AS [PeriodStart], E.[Time2] AS [PeriodEnd],
	SUM(E.[Direction] * E.[Quantity]) AS [PeriodUsage],
	SUM(E.[Direction] * E.[MonetaryValue]) AS [PeriodDepreciation],
	E.[CenterId], E.[AgentId], E.[NotedAgentId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
	JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
	WHERE L.[State] = 4
	AND LD.[Code] <> N'ToIncomeStatementAbstractFromRetainedEarnings'
	AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
	AND E.[ResourceId] IN (SELECT [FixedAssetId] FROM @FixedAssetsDepreciations)
	AND E.[EntryTypeId] IN (SELECT [Id] FROM @AccumulatedDepreciationEntryTypeIds)
	GROUP BY E.[ResourceId], E.[Time1], E.[Time2], E.[CenterId], E.[AgentId], E.[NotedAgentId] 
--75s
-- The variance to be posted
DECLARE @VarianceDepreciations TABLE  (
	[FixedAssetId]				INT,
	[PeriodStart]				DATE, 
	[PeriodEnd]					DATE,
	[UsageVariance]				DECIMAL (19, 6), -- Period Start to Period End, both inclusive
	[DepreciationVariance]		DECIMAL (19, 6), -- for straight line, [BookMinusResidual] * [Period Usage] / [Remaining Life Time]
	[DepreciationCenterId]		INT, -- As of Period Start, exclusive
	[DepreciationAgentId]		INT, -- As of Period Start, exclusive
	[DepreciationNotedAgentId]	INT, -- As of Period Start, exclusive
	[DepreciationEntryTypeId]	INT
	INDEX IX ([FixedAssetId], [PeriodStart], [PeriodEnd])
);
INSERT INTO @VarianceDepreciations([FixedAssetId], [PeriodStart], [PeriodEnd], [UsageVariance], [DepreciationVariance],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId])
SELECT  --N'@VarianceDepreciations' AS [Table], 
[FixedAssetId], [PeriodStart], [PeriodEnd], SUM([PeriodUsage]) AS [UsageVariance], SUM([PeriodDepreciation]) AS [DepreciationVariance],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId] 
FROM (
SELECT [FixedAssetId], [PeriodStart], [PeriodEnd], [PeriodUsage], [PeriodDepreciation],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
	FROM @FixedAssetsDepreciations
UNION ALL
SELECT [FixedAssetId], [PeriodStart], [PeriodEnd], [PeriodUsage], [PeriodDepreciation],
	[DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
	FROM @PostedDepreciations
) T
GROUP BY [FixedAssetId], [PeriodStart], [PeriodEnd], [DepreciationCenterId], [DepreciationAgentId], [DepreciationNotedAgentId]
HAVING SUM([PeriodUsage]) <> 0 OR 
ABS(SUM([PeriodDepreciation])) > 0.1;
--78
DECLARE @DepreciationPropertyPlantAndEquipment INT = dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment');
DECLARE @DepreciationInvestmentProperty INT = dal.fn_EntryTypeConcept__Id(N'DepreciationInvestmentProperty')
DECLARE @AmortisationIntangibleAssetsOtherThanGoodwill INT = dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')

INSERT INTO @Widelines([Index], [DocumentIndex], [PostingDate],
	[Direction0], [CenterId0], [AgentId0], [ResourceId0], [NotedAgentId0], [Quantity0], [UnitId0], [MonetaryValue0], [CurrencyId0], [Time10], [Time20], [EntryTypeId0],
	[Direction1], [CenterId1], [AgentId1], [ResourceId1], [NotedAgentId1], [Quantity1], [UnitId1], [MonetaryValue1], [CurrencyId1], [Time11], [Time21], [NotedDate1], [EntryTypeId1])
SELECT ROW_NUMBER() OVER(ORDER BY [FixedAssetId], [PeriodStart], [PeriodEnd]) - 1 AS [Index], 0 AS [DocumentIndex], [PeriodEnd] AS [PostingDate],
+1, [DepreciationCenterId], [DepreciationAgentId], [FixedAssetId], [DepreciationNotedAgentId], [UsageVariance], R.[UnitId], [DepreciationVariance], R.[CurrencyId], [PeriodStart], [PeriodEnd], bll.fn_Center__EntryType([DepreciationCenterId], NULL) AS [EntryTypeId0],
-1, [DepreciationCenterId], [DepreciationAgentId], [FixedAssetId], [DepreciationNotedAgentId], [UsageVariance], R.[UnitId], [DepreciationVariance], R.[CurrencyId], [PeriodStart], [PeriodEnd], [PeriodEnd] AS [NotedDate1], 
CASE
	WHEN RD.[ResourceDefinitionType] = N'PropertyPlantAndEquipment' THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
	WHEN RD.[ResourceDefinitionType] = N'InvestmentProperty' THEN  dal.fn_EntryTypeConcept__Id(N'DepreciationInvestmentProperty')
	WHEN RD.[ResourceDefinitionType] = N'IntangibleAssetsOtherThanGoodwill' THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
END AS [EntryTypeId1]
FROM @VarianceDepreciations VD
JOIN dbo.Resources R ON VD.[FixedAssetId] = R.[Id]
JOIN dbo.ResourceDefinitions RD on R.[DefinitionId] = RD.[Id] 
WHERE [PeriodEnd] >= @StartDate 
OPTION (RECOMPILE);
--86
-- Table var: 158s with INCLUDE indices, 162 without INCLUDE indices without recompile, 141 without Include with Recompile
-- Temp Tables: WITHOUT INCLUDE, 146 Without Recompile, 133s With Recompile, 127 using IsValidPeriod persisted:
-- 163s Moving function fn_EntryTypeConcept__Id outside the loop, 150 put them back!!!
-- Back to using Temp variables for tables: 160 s. I will stick with those

	RETURN
END
