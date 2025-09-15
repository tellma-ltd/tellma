CREATE PROCEDURE [wiz].[FixedAssets__Depreciate_V2]
-- Better version, allows correct depreciation even when the asset changes center during the month
	@PostingDate DATE,
	@ResourceId INT = NULL,
	@ExcludePostedOnOrAfter DATE = '9999-12-31' -- to allow entering and depreciating old assets or leases
AS
BEGIN
SET NOCOUNT ON;
-- The first two lines are in preparation for depreciation in ET calendar. However, we still need to update the 
-- rest of the script
DECLARE @PeriodStart DATE = dbo.fn_MonthStart(@PostingDate); -- seed value changes in the loop
DECLARE @PeriodEnd DATE = dbo.fn_MonthEnd(@PostingDate); -- fixed

DECLARE @MonthUnit INT = dal.fn_UnitCode__Id(N'mo');

DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
DECLARE @ROUNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'RightofuseAssets');
DECLARE @IANode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');
DECLARE @FunctionalCurrencyId NCHAR (3) = dal.fn_FunctionalCurrencyId();

DECLARE @FAAccountIds TABLE ([Id] INT PRIMARY KEY, [EntryTypeId] INT)
INSERT INTO @FAAccountIds([Id] , [EntryTypeId])
SELECT A.[Id], 
CASE
		WHEN AC.[Node].IsDescendantOf(@PPENode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN AC.[Node].IsDescendantOf(@ROUNode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'DepreciationPropertyPlantAndEquipment')
		WHEN AC.[Node].IsDescendantOf(@IANode) = 1 THEN dal.fn_EntryTypeConcept__Id(N'AmortisationIntangibleAssetsOtherThanGoodwill')
		ELSE NULL
END AS [EntryTypeId]
FROM dbo.[Accounts] A
JOIN dbo.[AccountTypes] AC ON AC.[Id] = A.[AccountTypeId]
WHERE A.[IsActive] = 1
AND (
	AC.[Node].IsDescendantOf(@PPENode) = 1 OR
	AC.[Node].IsDescendantOf(@ROUNode) = 1 OR
	AC.[Node].IsDescendantOf(@IANode) = 1
);

--select * from @FAAccountIds;

DECLARE @Widelines WidelineList, @LastIndex INT;
DECLARE @FixedAssetsDepreciations TABLE (
	[ResourceId]				INT PRIMARY KEY,
	[BookMinusResidual]			DECIMAL (19, 6), -- till period start
	[RemainingLifeTime]			DECIMAL (19, 6), -- till period start
	[PeriodUsage]				DECIMAL (19, 6), -- from period start till next activity which is NOT depreciation
	[PeriodStart]				DATE,
	[PeriodEnd]					DATE,
	[CorrectPeriodDepreciation]	DECIMAL (19, 6),
	[PostedPeriodDepreciation]	DECIMAL (19, 6), -- from period start to period end, inclusive
	[VariancePeriodDepreciation]DECIMAL (19, 6),
	[CenterId]					INT,
	[AgentId]					INT,
	[NotedResourceId]			INT,
	[NotedAgentId]				INT,
	[EntryTypeId]				INT
);
DECLARE @DepreciationEntryTypes StringList;
INSERT INTO @DepreciationEntryTypes VALUES (N'DepreciationPropertyPlantAndEquipment'), (N'DepreciationInvestmentProperty'), (N'AmortisationIntangibleAssetsOtherThanGoodwill');
WHILE @PeriodStart IS NOT NULL
BEGIN
	DELETE FROM @FixedAssetsDepreciations; 
	INSERT INTO @FixedAssetsDepreciations([ResourceId], [BookMinusResidual], [RemainingLifeTime], [PeriodUsage], [PeriodStart], [PeriodEnd], [PostedPeriodDepreciation],
											[CenterId], [AgentId], [NotedResourceId], [NotedAgentId], [EntryTypeId])
	SELECT E.[ResourceId], SUM(E.[Direction] * E.[MonetaryValue] - E.[Direction] * ISNULL(E.[NotedAmount], 0)) AS [BookMinusResidual], 
			SUM(E.[Direction] * E.[Quantity]) AS [RemainingLifeTime], dbo.fn_DateDiffWithPrecision_V2(@MonthUnit, @PeriodStart, @PeriodEnd), @PeriodStart, @PeriodEnd, 0,
			E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[NotedAgentId], A.[EntryTypeId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
	JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
	WHERE L.[State] = 4 
	AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
	AND R.[IsActive] = 1 AND R.[Code] <> N'0'
	AND L.[PostingDate] < @ExcludePostedOnOrAfter
	AND (E.[Time1] <= @PeriodStart AND ET.[Concept] NOT IN (SELECT [Id] FROM @DepreciationEntryTypes)
		OR E.[Time2] < @PeriodStart AND ET.[Concept] IN (SELECT [Id] FROM @DepreciationEntryTypes)
		OR E.[Time1] >= @PeriodStart AND E.[Time2] <= @PeriodEnd -- MA:2024-05-16, to include minor assets
			AND ET.[Concept] NOT IN (SELECT [Id] FROM @DepreciationEntryTypes)
	)
	AND (@ResourceId IS NULL OR E.[ResourceId] = @ResourceId)
	GROUP BY E.[ResourceId], E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[NotedAgentId], A.[EntryTypeId]
	HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0;

	-- The next noted date is the date before the next activity
	WITH UsageTill AS (
		SELECT E.[ResourceId], DATEADD(DAY, -1, MIN(E.[Time1])) AS [NotedDate]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4 
		AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
		AND E.[Time1] > @PeriodStart
		AND E.[Time1] <= @PeriodEnd
		AND E.[ResourceId] IN (SELECT [ResourceId] FROM @FixedAssetsDepreciations)
		AND ET.[Concept] NOT IN (SELECT [Id] FROM @DepreciationEntryTypes)
		GROUP BY E.[ResourceId]
	)
	UPDATE T
	SET -- 
		[PeriodUsage] = dbo.fn_DateDiffWithPrecision_V2(@MonthUnit, @PeriodStart, UT.[NotedDate]),
		[PeriodEnd]	= UT.[NotedDate]
	FROM @FixedAssetsDepreciations T
	JOIN UsageTill UT ON UT.ResourceId = T.[ResourceId];

	UPDATE @FixedAssetsDepreciations
	SET
		-- We still need to update Period End, in case it is fully depreciated?
		[CorrectPeriodDepreciation] = IIF([RemainingLifeTime] < [PeriodUsage], [BookMinusResidual], [BookMinusResidual] / [RemainingLifeTime] * [PeriodUsage]),
		[PeriodUsage] = IIF([RemainingLifeTime] < [PeriodUsage], [RemainingLifeTime], [PeriodUsage]);

	WITH PeriodDepreciations AS (
		SELECT E.[ResourceId], SUM(E.[Direction] * E.[MonetaryValue]) AS Amount
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4
		AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
		AND L.[PostingDate] < @ExcludePostedOnOrAfter
		AND E.[Time2] >= @PeriodStart
		AND E.[Time2] <= @PeriodEnd
		AND E.[ResourceId] IN (SELECT [ResourceId] FROM @FixedAssetsDepreciations)
		AND ET.[Concept] IN (SELECT [Id] FROM @DepreciationEntryTypes)
		GROUP BY E.[ResourceId]
	)
	UPDATE T
		SET T.[PostedPeriodDepreciation] = PD.[Amount]
	FROM @FixedAssetsDepreciations T
	JOIN PeriodDepreciations PD ON PD.ResourceId = T.[ResourceId];

	UPDATE @FixedAssetsDepreciations SET [VariancePeriodDepreciation] = [CorrectPeriodDepreciation] - [PostedPeriodDepreciation];
--	select * from @FixedAssetsDepreciations;
	SET @LastIndex = ISNULL((SELECT MAX([Index]) FROM @Widelines), -1);
	INSERT INTO @Widelines([Index],
	[DocumentIndex], 
	[CenterId0], [CurrencyId0], [AgentId0], [ResourceId0], [NotedAgentId0], [NotedResourceId0], [Quantity0], [UnitId0], [MonetaryValue0], [Value0], [Time10], [Time20],[EntryTypeId0],
	[CenterId1], [CurrencyId1], [AgentId1], [ResourceId1], [NotedAgentId1], [NotedResourceId1], [Quantity1], [UnitId1], [MonetaryValue1], [Value1], [Time11], [Time21],[EntryTypeId1]
	)
	SELECT ROW_NUMBER() OVER(ORDER BY T.[ResourceId]) + @LastIndex,
			0,
			T.[CenterId] AS [CenterId0], ISNULL(R.[CurrencyId], @FunctionalCurrencyId), T.[AgentId] AS [AgentId0], T.[ResourceId], T.[NotedAgentId] AS [NotedAgentId0], T.[NotedResourceId] AS [NotedResourceId0],
			T.[PeriodUsage], R.[UnitId], T.[VariancePeriodDepreciation], NULL AS [NetValue], T.PeriodStart, T.PeriodEnd, bll.fn_Center__EntryType(T.[CenterId], NULL) AS [EntryTypeId0],
			T.[CenterId] AS [CenterId1], ISNULL(R.[CurrencyId], @FunctionalCurrencyId), T.[AgentId] AS  [AgentId], T.[ResourceId], T.[NotedAgentId] AS [NotedAgentId1], T.[NotedResourceId] AS [NotedResourceId1],
			T.[PeriodUsage], R.[UnitId], T.[VariancePeriodDepreciation], NULL AS [NetValue], T.PeriodStart, T.PeriodEnd, T.[EntryTypeId] AS [EntryTypeId1]
	FROM @FixedAssetsDepreciations T
	JOIN dbo.Resources R ON R.[Id] = T.[ResourceId]
	WHERE R.[Code] <> '0';

	SET @PeriodStart = (
		SELECT MIN(E.[Time1])
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4
		AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
		AND L.[PostingDate] < @ExcludePostedOnOrAfter
		AND E.[Time1] > @PeriodStart
		AND E.[Time1] <= @PeriodEnd
		AND E.[ResourceId] IN (SELECT [ResourceId] FROM @FixedAssetsDepreciations)
		AND ET.[Concept] NOT IN (SELECT [Id] FROM @DepreciationEntryTypes)
	)
END

-- Add assets acquired during the period. MA: 2024-06-18
SET @PeriodStart = dbo.fn_MonthStart(@PostingDate);
DELETE FROM @FixedAssetsDepreciations; 
INSERT INTO @FixedAssetsDepreciations([ResourceId], [BookMinusResidual], [RemainingLifeTime], [PeriodUsage], [PeriodEnd], [PostedPeriodDepreciation],
										[Centerid], [AgentId], [NotedResourceId], [NotedAgentId], [EntryTypeId])
SELECT E.[ResourceId], SUM(E.[Direction] * E.[MonetaryValue] - E.[Direction] * ISNULL(E.[NotedAmount], 0)) AS [BookMinusResidual], 
		SUM(E.[Direction] * E.[Quantity]) AS [RemainingLifeTime], dbo.fn_DateDiffWithPrecision_V2(@MonthUnit, @PeriodStart, @PeriodEnd), @PeriodEnd, 0,
		E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[NotedAgentId], A.[EntryTypeId]
FROM dbo.Entries E
JOIN dbo.Lines L ON L.[Id] = E.[LineId]
JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
WHERE L.[State] = 4 
AND L.[PostingDate] <= @PostingDate
AND R.[IsActive] = 1 AND R.[Code] <> N'0'
AND L.[PostingDate] < @ExcludePostedOnOrAfter
AND (E.[Time1] >= @PeriodStart AND E.[Time1] <= @PeriodEnd) -- acquired during the period
AND (R.[Id] NOT IN (SELECT [ResourceId0] FROM @Widelines)) -- and not included above already
AND (@ResourceId IS NULL OR E.[ResourceId] = @ResourceId)
GROUP BY E.[ResourceId], E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[NotedAgentId], A.[EntryTypeId]
HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0;

WITH UsageTill AS (
	SELECT E.[ResourceId], E.[Time1], @PeriodEnd AS PeriodEnd
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
	JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
	WHERE L.[State] = 4 
	AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
	AND E.[Time1] > @PeriodStart
	AND E.[Time1] <= @PeriodEnd
	AND E.[ResourceId] IN (SELECT [ResourceId] FROM @FixedAssetsDepreciations)
	AND ET.[Concept] NOT IN (SELECT [Id] FROM @DepreciationEntryTypes)
	GROUP BY E.[ResourceId], E.[Time1]
)
UPDATE T
SET -- 
--	[PeriodUsage] = 1.0 * DATEDIFF(DAY, UT.[Time1], @PeriodEnd) / DATEDIFF(DAY, @PeriodStart, @PeriodEnd),
	[PeriodUsage] = 1.0 * dbo.fn_DateDiffWithPrecision_V2(@MonthUnit, UT.[Time1], @PeriodEnd),
	[PeriodStart] = UT.[Time1],
	[PeriodEnd]	= @PeriodEnd
FROM @FixedAssetsDepreciations T
JOIN UsageTill UT ON UT.ResourceId = T.[ResourceId];

UPDATE @FixedAssetsDepreciations
SET [CorrectPeriodDepreciation] = IIF([RemainingLifeTime] = 0, 0, [BookMinusResidual] / [RemainingLifeTime] * [PeriodUsage]);

WITH PeriodDepreciations AS (
	SELECT E.[ResourceId], SUM(E.[Direction] * E.[MonetaryValue]) AS Amount
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN @FAAccountIds A ON A.[Id] = E.[AccountId]
	JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
	WHERE L.[State] = 4
	AND L.[PostingDate] <= @PostingDate -- MA:2024-05-05
	AND L.[PostingDate] < @ExcludePostedOnOrAfter
	AND E.[Time2] >= @PeriodStart
	AND E.[Time2] <= @PeriodEnd
	AND E.[ResourceId] IN (SELECT [ResourceId] FROM @FixedAssetsDepreciations)
	AND ET.[Concept] IN (SELECT [Id] FROM @DepreciationEntryTypes)
	GROUP BY E.[ResourceId]
)
UPDATE T
	SET T.[PostedPeriodDepreciation] = PD.[Amount]
FROM @FixedAssetsDepreciations T
JOIN PeriodDepreciations PD ON PD.ResourceId = T.[ResourceId];

UPDATE @FixedAssetsDepreciations SET [VariancePeriodDepreciation] = [CorrectPeriodDepreciation] - [PostedPeriodDepreciation];
--	select * from @FixedAssetsDepreciations;
SET @LastIndex = ISNULL((SELECT MAX([Index]) FROM @Widelines), -1);
INSERT INTO @Widelines([Index],
[DocumentIndex], 
[CenterId0], [CurrencyId0], [AgentId0], [ResourceId0], [NotedAgentId0], [NotedResourceId0], [Quantity0], [UnitId0], [MonetaryValue0], [Value0], [Time10], [Time20],[EntryTypeId0],
[CenterId1], [CurrencyId1], [AgentId1], [ResourceId1], [NotedAgentId1], [NotedResourceId1], [Quantity1], [UnitId1], [MonetaryValue1], [Value1], [Time11], [Time21],[EntryTypeId1]
)
SELECT ROW_NUMBER() OVER(ORDER BY T.[ResourceId]) + @LastIndex,
		0,
		T.[CenterId] AS [CenterId0], ISNULL(R.[CurrencyId], @FunctionalCurrencyId), T.[AgentId] AS [AgentId0], T.[ResourceId], T.[NotedAgentId] AS [NotedAgentId0], T.[NotedResourceId] AS [NotedResourceId0],
		T.[PeriodUsage], R.[UnitId], T.[VariancePeriodDepreciation], NULL AS [NetValue], T.PeriodStart, T.PeriodEnd, bll.fn_Center__EntryType(T.[CenterId], NULL) AS [EntryTypeId0],
		T.[CenterId] AS [CenterId1], ISNULL(R.[CurrencyId], @FunctionalCurrencyId), T.[AgentId] AS  [AgentId], T.[ResourceId], T.[NotedAgentId] AS [NotedAgentId1], T.[NotedResourceId] AS [NotedResourceId1],
		T.[PeriodUsage], R.[UnitId], T.[VariancePeriodDepreciation], NULL AS [NetValue], T.PeriodStart, T.PeriodEnd, T.[EntryTypeId] AS [EntryTypeId1]
FROM @FixedAssetsDepreciations T
JOIN dbo.Resources R ON R.[Id] = T.[ResourceId]
WHERE R.[Code] <> '0';

SELECT * FROM @Widelines;
END
GO