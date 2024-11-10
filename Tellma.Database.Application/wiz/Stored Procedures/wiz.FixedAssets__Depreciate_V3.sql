CREATE PROCEDURE [wiz].[FixedAssets__Depreciate_V3]
-- Using Claude.AI
-- DECLARE
-- Even Better version than V2, allows correct depreciation even when the asset changes center during the month
	@PostingDate DATE, -- depreciate till Posting Date. This is usually date for transfer, disposal, or end of month
	@StartDate DATE = N'1753-01-01', -- Typically from archive datae
	@ResourceDefinitionId INT = NULL,
	@ResourceId INT = NULL,
	@Debug BIT = 0
AS
BEGIN
SET NOCOUNT ON;
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
IF @Debug=1 select  N'@SummarizedFixedAssets' AS [Table], * from @SummarizedFixedAssets;

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
IF @Debug=1 select N'@FixedAssetsJournal_1' AS [Table],* from @FixedAssetsJournal order by FixedAssetId, PeriodStart

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
IF @Debug=1 select N'@AssetDateRanges' AS [Table],* from @AssetDateRanges order by FixedAssetId

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
IF @Debug=1 select N'@LastCenterPerDate' AS [Table], * from @LastCenterPerDate order by fixedAssetId
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
IF @Debug=1 select N'@LastCenterLookup' AS [Table], * from @LastCenterLookup order by fixedAssetId
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
IF @Debug=1 select N'@FixedAssetsJournal_2' AS [Table], * from @FixedAssetsJournal order by FixedAssetId, PeriodStart;
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
IF @Debug=1 select N'@PeriodData' AS [Table], [RowNum], [FixedAssetId], [PeriodStart], [PeriodEnd], [Amount], [Quantity], [PeriodUsage], [IsValidPeriod] from @PeriodData order by [FixedAssetId], RowNum ;
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
IF @Debug=1 select N'@FixedAssetsDepreciations' AS [Table], * from @FixedAssetsDepreciations order by FixedAssetId, RowNum;

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
	JOIN @FixedAssetsAccountIds A ON A.[FixedAssetAccountId] = E.[AccountId]
	WHERE L.[State] = 4
	AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
	AND E.[ResourceId] IN (SELECT [FixedAssetId] FROM @FixedAssetsDepreciations)
	AND E.[EntryTypeId] IN (SELECT [Id] FROM @AccumulatedDepreciationEntryTypeIds)
	GROUP BY E.[ResourceId], E.[Time1], E.[Time2], E.[CenterId], E.[AgentId], E.[NotedAgentId] 
IF @Debug=1 select N'@PostedDepreciations' AS [Table], * from @PostedDepreciations order by FixedAssetId, PeriodStart;
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
IF @Debug=1 select N'@VarianceDepreciations' AS [Table], vd.*, r.[code] as FA_Code, r.[name] as FA_Name from @VarianceDepreciations VD join dbo.resources R on r.id = vd.fixedassetid order by FixedAssetId, PeriodStart, PeriodEnd;
--78
DECLARE @Widelines WidelineList;
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
IF @Debug=0 SELECT * from @Widelines;
-- Table var: 158s with INCLUDE indices, 162 without INCLUDE indices without recompile, 141 without Include with Recompile
-- Temp Tables: WITHOUT INCLUDE, 146 Without Recompile, 133s With Recompile, 127 using IsValidPeriod persisted:
-- 163s Moving function fn_EntryTypeConcept__Id outside the loop, 150 put them back!!!
-- Back to using Temp variables for tables: 160 s. I will stick with those
END