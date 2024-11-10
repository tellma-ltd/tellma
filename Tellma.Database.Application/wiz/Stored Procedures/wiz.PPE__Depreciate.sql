CREATE PROCEDURE [wiz].[PPE__Depreciate]
-- Shortcomings: 
-- Limited to PPE and ROU, no other fixed assets
-- Cannot handle minor assets
-- cannot handle assets transfers during period
	@DocumentIndex	INT = 0,
	@DepreciationPeriodStarts DATE =  N'2022.06.01',
	@DepreciationPeriodEnds DATE =  N'2022.06.30',
	@LineType TINYINT = 100 -- 100: Normal, 120: Regulatory
AS
	-- Return the list of assets that have depreciable life, with Time1 = last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines [WidelineList];
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @ROUNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'RightofuseAssets');
	DECLARE @DepreciationAndAmortisationExpenseNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationAndAmortisationExpense');
	DECLARE @DepreciatedLife DECIMAL (19, 4) = 1 + DATEDIFF(MONTH, @DepreciationPeriodStarts, @DepreciationPeriodEnds);

	WITH PPEAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsActive] = 1
		AND [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1 OR  [Node].IsDescendantOf(@ROUNode) = 1
		)
	),
	DNAEAccountId AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsActive] = 1
		AND [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@DepreciationAndAmortisationExpenseNode) = 1
		)
	),
	OpeningBalances AS (
		SELECT E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId],
			SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
			SUM(E.[Direction] * E.[Value]) AS [NetValue],
			SUM(E.[Direction] * E.[NotedAmount]) AS [NetResidualMonetaryValue],
			SUM(E.[Direction] * E.[NotedAmount]) * SUM(E.[Direction] * E.[Value]) / SUM(E.[Direction] * E.[MonetaryValue]) AS [NetResidualValue]
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND E.Time1 < @DepreciationPeriodStarts
		GROUP BY E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	),--select * from OpeningBalances
	LastDepreciationDates AS (
		SELECT E.[ResourceId], MAX(L.[PostingDate]) AS LastDepreciationDate
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN DNAEAccountId A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND L.PostingDate < @DepreciationPeriodStarts
		GROUP BY E.[ResourceId]
	),-- select * from LastDepreciationDates ,
	OpeningAgentNotedResourceNotedAgentEntryTypes AS (
		SELECT E.[ResourceId], E.[AgentId], E.[NotedResourceId], E.[NotedAgentId], E.[EntryTypeId]
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN DNAEAccountId A ON E.AccountId = A.[Id]
		JOIN LastDepreciationDates LDD ON LDD.ResourceId = E.[ResourceId] AND LDD.[LastDepreciationDate] = L.[PostingDate]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
	),-- select * from OpeningAgentNotedResourceNotedAgentEntryTypes
	-- As it is now, it is run every month, and if you want to include the items purchased before mid month, simply adjust
	-- Date 1: Purchase Date, Date 2: Available for itended use, From Date: Depreciation Starts, To Date: Depreciation Ends
	PeriodAdditions AS (
		SELECT E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId],
			(1.0 + DATEDIFF(DAY, MIN(E.[Time1]), @DepreciationPeriodEnds)) /
			(1.0 + DATEDIFF(DAY, @DepreciationPeriodStarts, @DepreciationPeriodEnds)) AS DepreciableLife,
			SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
			SUM(E.[Direction] * E.[Value]) AS [NetValue],
			SUM(E.[Direction] * E.[NotedAmount]) AS [NetResidualMonetaryValue],
			SUM(E.[Direction] * E.[NotedAmount]) * SUM(E.[Direction] * E.[Value]) / SUM(E.[Direction] * E.[MonetaryValue]) AS [NetResidualValue]
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND E.Time1 Between @DepreciationPeriodStarts AND @DepreciationPeriodEnds
		AND ET.[Concept] IN (N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment',
							N'InternalTransferPropertyPlantAndEquipmentExtension')
		GROUP BY E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId]
		HAVING SUM(E.[Direction] * E.[BaseQuantity]) <> 0
		OR SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	),--select * from PeriodAdditions
	TargetPeriodDepreciation AS (
		SELECT OB.[ResourceId], OB.[BaseUnitId], OB.[CurrencyId], OB.[CenterId], OB.[AgentId], OB.[NotedAgentId], OB.[NotedResourceId],
			IIF(OB.[NetLife] < @DepreciatedLife, OB.[NetLife], @DepreciatedLife) AS [NetLife], --<< need review
			@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
				OB.[NetMonetaryValue] - OB.[NetResidualMonetaryValue], OB.[NetResidualMonetaryValue], OB.[NetLife]
			) AS [NetMonetaryValue],
			@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
				OB.[NetValue] - OB.[NetResidualValue], OB.[NetResidualValue], OB.[NetLife]
			) AS [NetValue]
		FROM OpeningBalances OB
		JOIN dbo.Resources PPE ON PPE.[Id] = OB.[ResourceId]
		LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup4Id] -- assuming we use last lookup for dep methods
		UNION
		SELECT PA.[ResourceId], PA.[BaseUnitId], PA.[CurrencyId], PA.[CenterId], PA.[AgentId], PA.[NotedAgentId], PA.[NotedResourceId],	
			PA.[DepreciableLife] AS [NetLife],
			PA.[DepreciableLife] * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
				PA.[NetMonetaryValue] - PA.[NetResidualMonetaryValue], PA.[NetResidualMonetaryValue], PA.[NetLife]
			) AS [NetMonetaryValue],
			PA.[DepreciableLife] * bll.fn_BookValue_Residual_LifeTime__Depreciation(ISNULL(LK.[Code], N'SL'),
				PA.[NetValue] - PA.[NetResidualValue], PA.[NetResidualValue], PA.[NetLife]
			) AS [NetValue]
		FROM PeriodAdditions PA
		JOIN dbo.Resources PPE ON PPE.[Id] = PA.[ResourceId]
		LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup4Id] -- assuming we use last lookup for dep methods
	)--select * from TargetPeriodDepreciation
	INSERT INTO @WideLines([Index],
		[DocumentIndex], 
		[CenterId0], [CurrencyId0], [AgentId0], [ResourceId0], [NotedAgentId0], [NotedResourceId0], [Quantity0], [UnitId0], [MonetaryValue0], [Value0], [Time10], [Time20],[EntryTypeId0],
		[CenterId1], [CurrencyId1], [AgentId1], [ResourceId1], [NotedAgentId1], [NotedResourceId1], [Quantity1], [UnitId1], [MonetaryValue1], [Value1], [Time11], [Time21]
		)
	SELECT ROW_NUMBER() OVER(ORDER BY NPD.[ResourceId]) - 1,
			@DocumentIndex,
			NPD.[CenterId], NPD.[CurrencyId], OANRNAET.[AgentId], NPD.[ResourceId],	NPD.[NotedAgentId], OANRNAET.[NotedResourceId],
			[NetLife], R.[UnitId], [NetMonetaryValue], [NetValue], @DepreciationPeriodStarts, @DepreciationPeriodEnds, OANRNAET.[EntryTypeId],
			NPD.[CenterId], NPD.[CurrencyId], NPD.[AgentId], NPD.[ResourceId], NPD.[NotedAgentId], NPD.[NotedResourceId],
			[NetLife], R.[UnitId], [NetMonetaryValue], [NetValue], @DepreciationPeriodStarts, @DepreciationPeriodEnds
	FROM TargetPeriodDepreciation NPD
	JOIN dbo.Resources R ON R.[Id] = NPD.[ResourceId]
	LEFT JOIN OpeningAgentNotedResourceNotedAgentEntryTypes OANRNAET ON OANRNAET.[ResourceId] = NPD.[ResourceId]

	SELECT * FROM @WideLines;
GO