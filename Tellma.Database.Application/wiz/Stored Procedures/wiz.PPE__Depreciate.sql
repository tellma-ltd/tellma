CREATE PROCEDURE [wiz].[PPE__Depreciate]
-- [wiz].[PPE__Depreciate] @DepreciationPeriodStarts = N'2022.01.01'
	@DocumentIndex	INT = 0,
	@DepreciationPeriodStarts DATE =  N'2022.01.01',
	@PostingDate DATE, -- Assume monthly depreciation
	@LineType TINYINT -- 100: Normal, 120: Regulatory
AS
	-- Return the list of assets that have depreciable life, with Time1 = last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines WideLineList;
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @PureUnitId INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'Pure');
	DECLARE @DepreciationPeriodEnds DATE = EOMONTH(@PostingDate);
	DECLARE @DepreciatedLife SMALLINT = 1 + DATEDIFF(MONTH, @DepreciationPeriodStarts, @DepreciationPeriodEnds);

	WITH PPEAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsActive] = 1
		AND [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
		)
	),
	OpeningBookMinusResidalBalances AS (
		SELECT E.[AgentId], E.[BaseUnitId], E.[CurrencyId],
			SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
			SUM(E.[Direction] * E.[MonetaryValue]) AS NetMonetaryValue,
			SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND L.PostingDate < @DepreciationPeriodStarts
		AND E.UnitId <> @PureUnitId
		GROUP BY E.[AgentId], E.[BaseUnitId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[BaseQuantity]) <> 0
		OR SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	),
	OpeningResidualBalances AS (
		SELECT E.[AgentId], E.[CurrencyId],
			SUM(E.[Direction] * E.[MonetaryValue]) AS NetMonetaryValue,
			SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND L.PostingDate < @DepreciationPeriodStarts
		AND E.UnitId = @PureUnitId
		GROUP BY E.[AgentId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	), /* revisit after knowing MHR policies
	-- As it is now, it is run every month, and if you want to include the items purchased before mid month, simply adjust
	-- Date 1: Purchase Date, Date 2: Avaiable for itended use, From Date: Depreciation Starts, To Date: Depreciation Ends
	PeriodAdditions AS (
		SELECT E.[AgentId], E.[BaseUnitId], E.[CurrencyId],
			SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
			SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND L.PostingDate Between @DepreciationPeriodStarts AND @DepreciationPeriodEnds
		AND ET.[Concept] IN (N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment')
		GROUP BY E.[AgentId], E.[BaseUnitId], E.[CurrencyId]
	), */
	PostedPeriodDepreciation AS (
		SELECT E.[AgentId], E.[BaseUnitId], E.[CurrencyId],
			SUM(E.[Direction] * E.[BaseQuantity]) AS [NetLife],
			SUM(E.[Direction] * E.[MonetaryValue]) AS [NetMonetaryValue],
			SUM(E.[Direction] * E.[Value]) AS NetValue
		FROM map.DetailsEntries() E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		WHERE L.[State] = 4 AND LD.[LineType] BETWEEN 100 AND @LineType
		AND L.PostingDate Between @DepreciationPeriodStarts AND @DepreciationPeriodEnds
		AND ET.[Concept] IN (N'DepreciationPropertyPlantAndEquipment')
		GROUP BY E.[AgentId], E.[BaseUnitId], E.[CurrencyId]
	),
	TargetPeriodDepreciation AS (
		SELECT OBMRB.[AgentId], OBMRB.[BaseUnitId], OBMRB.[CurrencyId],
			@DepreciatedLife AS [NetLife],
			@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(LK.[Code],
				OBMRB.[NetMonetaryValue], ORB.[NetMonetaryValue], OBMRB.[NetLife]
			) AS [NetMonetaryValue],
			@DepreciatedLife * bll.fn_BookValue_Residual_LifeTime__Depreciation(LK.[Code],
				OBMRB.[NetValue], ORB.[NetValue], OBMRB.[NetLife]
			) AS [NetValue]
		FROM OpeningBookMinusResidalBalances OBMRB
		JOIN dbo.Agents PPE ON PPE.[Id] = OBMRB.[AgentId]
		LEFT JOIN dbo.Lookups LK ON LK.Id = PPE.[Lookup8Id] -- assuming we use last lookup for dep methods
		LEFT JOIN OpeningResidualBalances ORB
		ON OBMRB.[AgentId] = ORB.[AgentId]
		AND OBMRB.[CurrencyId] = ORB.[CurrencyId]
	),
	NetPeriodDepreciation AS (
		SELECT TPD.[AgentId], TPD.[BaseUnitId], TPD.[CurrencyId], 
			TPD.[NetLife] - ISNULL(PPD.[NetLife], 0) AS [NetLife],
			TPD.[NetMonetaryValue] - ISNULL(PPD.[NetMonetaryValue], 0) AS [NetMonetaryValue],
			TPD.[NetValue] - ISNULL(PPD.[NetValue], 0) AS [NetValue]
		FROM TargetPeriodDepreciation TPD
		LEFT JOIN PostedPeriodDepreciation PPD
		ON TPD.[AgentId] = PPD.[AgentId]
		AND TPD.[BaseUnitId] = PPD.[BaseUnitId]
		AND TPD.[CurrencyId] = PPD.[CurrencyId]
		WHERE TPD.[NetLife] - ISNULL(PPD.[NetLife], 0) <> 0
		OR TPD.[NetMonetaryValue] - ISNULL(PPD.[NetMonetaryValue], 0) <> 0
		OR TPD.[NetValue] - ISNULL(PPD.[NetValue], 0) <> 0
	)
	INSERT INTO @WideLines([Index],
		[DocumentIndex], [AgentId1], 
		[CurrencyId0], [CurrencyId1], [CenterId0], [Quantity0], [MonetaryValue0], [Value0],
		[Time10], [Time20]
		)
	SELECT ROW_NUMBER() OVER(ORDER BY NPD.[AgentId]) - 1,
			@DocumentIndex, NPD.[AgentId],
			NPD.[CurrencyId], NPD.[CurrencyId], AG.[CenterId], NPD.[NetLife], NPD.[NetMonetaryValue], NPD.[NetValue],
			@DepreciationPeriodStarts, @DepreciationPeriodEnds
	FROM NetPeriodDepreciation NPD
	JOIN dbo.[Agents] AG ON AG.[Id] = NPD.[AgentId]
	SELECT * FROM @WideLines;
GO