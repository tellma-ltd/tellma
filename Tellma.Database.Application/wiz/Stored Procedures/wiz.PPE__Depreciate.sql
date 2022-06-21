CREATE PROCEDURE [wiz].[PPE__Depreciate]
	@DocumentIndex	INT = 0,
	@DepreciationPeriodStarts DATE =  N'2022.06.01',
	@DepreciationPeriodEnds DATE =  N'2022.06.30',
	@LineType TINYINT = 100 -- 100: Normal, 120: Regulatory
AS
	-- Return the list of assets that have depreciable life, with Time1 = last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines [WidelineList];
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @DepreciatedLife SMALLINT = 1 + DATEDIFF(MONTH, @DepreciationPeriodStarts, @DepreciationPeriodEnds);

	WITH PPEAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsActive] = 1
		AND [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
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
		HAVING SUM(E.[Direction] * E.[BaseQuantity]) <> 0
		OR SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	),--select * from OpeningBalances
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
		AND ET.[Concept] IN (N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment')
		GROUP BY E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId]
	),--select * from PeriodAdditions
	PostedPeriodDepreciation AS (
		SELECT E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId],
			SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
			SUM(E.[Direction] * E.[Value]) AS [NetValue],
			0  AS [NetResidualMonetaryValue],
			0  AS [NetResidualValue]
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND L.PostingDate Between @DepreciationPeriodStarts AND @DepreciationPeriodEnds
		AND ET.[Concept] IN (N'DepreciationPropertyPlantAndEquipment')
		GROUP BY E.[ResourceId], E.[BaseUnitId], E.[CurrencyId], E.[CenterId], E.[AgentId], E.[NotedAgentId], E.[NotedResourceId]
	), --select * from PostedPeriodDepreciation
	TargetPeriodDepreciation AS (
		SELECT OB.[ResourceId], OB.[BaseUnitId], OB.[CurrencyId], OB.[CenterId], OB.[AgentId], OB.[NotedAgentId], OB.[NotedResourceId],
			@DepreciatedLife AS [NetLife],
			@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(LK.[Code],
				OB.[NetMonetaryValue] - OB.[NetResidualMonetaryValue], OB.[NetResidualMonetaryValue], OB.[NetLife]
			) AS [NetMonetaryValue],
			@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(LK.[Code],
				OB.[NetValue] - OB.[NetResidualValue], OB.[NetResidualValue], OB.[NetLife]
			) AS [NetValue]
		FROM OpeningBalances OB
		JOIN dbo.Resources PPE ON PPE.[Id] = OB.[ResourceId]
		LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup4Id] -- assuming we use last lookup for dep methods
		UNION
		SELECT PA.[ResourceId], PA.[BaseUnitId], PA.[CurrencyId], PA.[CenterId], PA.[AgentId], PA.[NotedAgentId], PA.[NotedResourceId],	
			PA.[DepreciableLife] AS [NetLife],
			PA.[DepreciableLife] * bll.fn_BookValue_Residual_LifeTime__Depreciation(LK.[Code],
				PA.[NetMonetaryValue] - PA.[NetResidualMonetaryValue], PA.[NetResidualMonetaryValue], PA.[NetLife]
			) AS [NetMonetaryValue],
			PA.[DepreciableLife] * bll.fn_BookValue_Residual_LifeTime__Depreciation(LK.[Code],
				PA.[NetValue] - PA.[NetResidualValue], PA.[NetResidualValue], PA.[NetLife]
			) AS [NetValue]
		FROM PeriodAdditions PA
		JOIN dbo.Resources PPE ON PPE.[Id] = PA.[ResourceId]
		LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup4Id] -- assuming we use last lookup for dep methods
	), --select * from TargetPeriodDepreciation
	NetPeriodDepreciation AS (
		SELECT TPD.[ResourceId], TPD.[BaseUnitId], TPD.[CurrencyId], TPD.[CenterId], TPD.[AgentId], TPD.[NotedAgentId], TPD.[NotedResourceId],
			TPD.[NetLife] - ISNULL(PPD.[NetLife], 0) AS [NetLife],
			TPD.[NetMonetaryValue] - ISNULL(PPD.[NetMonetaryValue], 0) AS [NetMonetaryValue],
			TPD.[NetValue] - ISNULL(PPD.[NetValue], 0) AS [NetValue]
		FROM TargetPeriodDepreciation TPD
		LEFT JOIN PostedPeriodDepreciation PPD
		ON TPD.[ResourceId] = PPD.[ResourceId]
		AND TPD.[BaseUnitId] = PPD.[BaseUnitId]
		AND TPD.[CurrencyId] = PPD.[CurrencyId]
		AND TPD.[CenterId] = PPD.[CenterId]
		AND TPD.[AgentId] = PPD.[AgentId]
		AND TPD.[NotedAgentId] = PPD.[NotedAgentId]
		AND TPD.[NotedResourceId] = PPD.[NotedResourceId]
		WHERE TPD.[NetLife] - ISNULL(PPD.[NetLife], 0) <> 0
		OR TPD.[NetMonetaryValue] - ISNULL(PPD.[NetMonetaryValue], 0) <> 0
		OR TPD.[NetValue] - ISNULL(PPD.[NetValue], 0) <> 0
	)--	select * from NetPeriodDepreciation
	INSERT INTO @WideLines([Index],
		[DocumentIndex], 
		[CenterId0], [CurrencyId0], [AgentId0], [ResourceId0], [NotedAgentId0], [NotedResourceId0], [Quantity0], [UnitId0], [MonetaryValue0], [Value0], [Time10], [Time20],
		[CenterId1], [CurrencyId1], [AgentId1], [ResourceId1], [NotedAgentId1], [NotedResourceId1], [Quantity1], [UnitId1], [MonetaryValue1], [Value1], [Time11], [Time21]
		
		)
	SELECT ROW_NUMBER() OVER(ORDER BY NPD.[ResourceId]) - 1,
			@DocumentIndex,
			NPD.[CenterId], NPD.[CurrencyId], NULL,		[ResourceId],	[NotedAgentId], NULL,
			[NetLife], R.[UnitId], [NetMonetaryValue], [NetValue], @DepreciationPeriodStarts, @DepreciationPeriodEnds,
			NPD.[CenterId], NPD.[CurrencyId], [AgentId], [ResourceId], [NotedAgentId], [NotedResourceId],
			[NetLife], R.[UnitId], [NetMonetaryValue], [NetValue], @DepreciationPeriodStarts, @DepreciationPeriodEnds
	FROM NetPeriodDepreciation NPD
	JOIN dbo.Resources R ON R.[Id] = NPD.[ResourceId]

	SELECT * FROM @WideLines;
GO