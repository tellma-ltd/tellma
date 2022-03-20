CREATE PROCEDURE [wiz].[AssetsAmortization__Populate]
-- [wiz].[AssetsAmortization__Populate] @AmortizationPeriodEnds = N'2021.08.07'
	@DocumentIndex	INT = 0,
	@AmortizationPeriodEnds	DATE = '2021.09.10'
AS
	-- Return the list of assets that have depreciable life, with Time1= last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines [WidelineList];
	DECLARE @IAOGNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');
	DECLARE @PureUnitId INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'Pure');

	WITH IAOGAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsActive] = 1
		AND [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@IAOGNode) = 1
		)
	),
	FirstAmortizationDates AS (
		SELECT E.[AgentId], E.[ResourceId], MIN(E.[Time1]) AS FirstAmortizationDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN IAOGAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4 AND L.PostingDate <= @AmortizationPeriodEnds
		AND E.UnitId <> @PureUnitId
		AND E.EntryTypeId = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill')
		GROUP BY E.[AgentId], E.[ResourceId]
	),
	LastAmortizationDates AS (
		SELECT E.[AgentId], E.[ResourceId], MAX(E.[Time2]) AS LastAmortizationDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN IAOGAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4 AND L.PostingDate <= @AmortizationPeriodEnds
		AND E.UnitId <> @PureUnitId
		AND E.EntryTypeId = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AmortisationIntangibleAssetsOtherThanGoodwill')
		GROUP BY E.[AgentId], E.[ResourceId]
	),
	LastCostCenters AS (
		SELECT E.[AgentId], E.[ResourceId], MAX(CenterId) AS [CostCenter]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		JOIN LastAmortizationDates LDD
			ON E.[AgentId] = LDD.[AgentId] 
			AND ISNULL(E.[ResourceId], -1) = ISNULL(LDD.[ResourceId], -1)
			AND E.[Time2] = LDD.[LastAmortizationDate]
		WHERE C.[IsLeaf] = 1
		AND L.[State] = 4 AND L.PostingDate <= @AmortizationPeriodEnds
		AND E.UnitId <> @PureUnitId
		GROUP BY E.[AgentId], E.[ResourceId]
	),
	DepreciableIAOGs AS (
		SELECT E.[AgentId], E.[ResourceId], FDD.[FirstAmortizationDate], LDD.[LastAmortizationDate]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN IAOGAccountIds A ON E.AccountId = A.[Id]
		JOIN FirstAmortizationDates FDD ON E.[AgentId] = FDD.[AgentId] AND ISNULL(E.[ResourceId], -1) = ISNULL(FDD.[ResourceId], -1)
		LEFT JOIN LastAmortizationDates LDD ON E.[AgentId] = LDD.[AgentId] AND ISNULL(E.[ResourceId], -1) = ISNULL(LDD.[ResourceId], -1)
		WHERE L.[State] = 4 AND L.PostingDate <= @AmortizationPeriodEnds
		AND E.UnitId <> @PureUnitId
		-- never depreciated for the period
		AND (LDD.LastAmortizationDate IS NULL OR LDD.LastAmortizationDate < @AmortizationPeriodEnds)
		-- Amortization date start has passed
		AND FDD.FirstAmortizationDate <= @AmortizationPeriodEnds
		GROUP BY E.[AgentId], E.[ResourceId], FDD.[FirstAmortizationDate], LDD.[LastAmortizationDate]
		-- there is value to depreciate, and a life to allocate the cost to
		HAVING SUM(E.[Direction] * E.[Quantity]) > 0
		AND SUM(E.[Direction] * E.[MonetaryValue]) > 0
	)
	INSERT INTO @WideLines([Index],
		[DocumentIndex], [AgentId1], [ResourceId1],
		[Time10],
		[CurrencyId0], [CurrencyId1], [CenterId0]
		)
	SELECT ROW_NUMBER() OVER(ORDER BY DIAOG.[AgentId], DIAOG.[ResourceId]) - 1,
			@DocumentIndex, DIAOG.[AgentId], DIAOG.[ResourceId],
			ISNULL(DATEADD(DAY, 1,LDD.LastAmortizationDate), DIAOG.FirstAmortizationDate),
			RL.[CurrencyId], RL.[CurrencyId], LCC.[CostCenter]
	FROM DepreciableIAOGs DIAOG
	JOIN dbo.[Agents] RL ON RL.[Id] = DIAOG.[AgentId]
	LEFT JOIN dbo.[Resources] R ON R.[Id] = DIAOG.[ResourceId]
	LEFT JOIN LastAmortizationDates LDD
		ON DIAOG.[AgentId] = LDD.[AgentId]
		AND ISNULL(DIAOG.[ResourceId], -1) = ISNULL(LDD.[ResourceId], -1)
	LEFT JOIN LastCostCenters LCC
		ON DIAOG.[AgentId] = LCC.[AgentId]
		AND ISNULL(DIAOG.[ResourceId], -1) =  ISNULL(LCC.[ResourceId], -1)
	SELECT * FROM @WideLines;
GO