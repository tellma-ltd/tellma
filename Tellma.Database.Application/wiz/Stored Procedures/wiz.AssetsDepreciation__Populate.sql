CREATE PROCEDURE [wiz].[AssetsDepreciation__Populate]
-- [wiz].[AssetsDepreciation__Populate] @DepreciationPeriodEnds = N'2021.08.07'
	@DocumentIndex	INT = 0,
	@DepreciationPeriodEnds	DATE
AS
BEGIN
	-- Return the list of assets that have depreciable life, with Time1= last depreciable date + 1
	-- Time2 is decided by posting date
	DECLARE @WideLines WideLineList;
	DECLARE @PPENode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
	DECLARE @PureUnitId INT = (SELECT [Id] FROM dbo.Units WHERE [Code] = N'Pure');

	WITH PPEAccountIds AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsActive] = 1
		AND [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@PPENode) = 1
		)
	),
	FirstDepreciationDates AS (
		SELECT E.[AgentId], E.[ResourceId], MIN(E.[Time1]) AS FirstDepreciationDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4
		AND E.UnitId <> @PureUnitId
		AND E.EntryTypeId = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment')
		GROUP BY E.[AgentId], E.[ResourceId]
	),
	LastDepreciationDates AS (
		SELECT E.[AgentId], E.[ResourceId], MAX(E.[Time2]) AS LastDepreciationDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		WHERE L.[State] = 4
		AND E.UnitId <> @PureUnitId
		AND E.EntryTypeId = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DepreciationPropertyPlantAndEquipment')
		GROUP BY E.[AgentId], E.[ResourceId]
	),
	LastCostCenters AS (
		SELECT E.[AgentId], E.[ResourceId], MAX(CenterId) AS [CostCenter]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
		JOIN LastDepreciationDates LDD
			ON E.[AgentId] = LDD.[AgentId] 
			AND E.[ResourceId] = LDD.[ResourceId]
			AND E.[Time2] = LDD.[LastDepreciationDate]
		WHERE C.[IsLeaf] = 1
		AND L.[State] = 4
		AND E.UnitId <> @PureUnitId
		GROUP BY E.[AgentId], E.[ResourceId]
	),
	DepreciablePPEs AS (
		SELECT E.[AgentId], E.[ResourceId], FDD.[FirstDepreciationDate], LDD.[LastDepreciationDate]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN PPEAccountIds A ON E.AccountId = A.[Id]
		JOIN FirstDepreciationDates FDD ON E.[AgentId] = FDD.[AgentId] AND E.[ResourceId] = FDD.[ResourceId]
		LEFT JOIN LastDepreciationDates LDD ON E.[AgentId] = LDD.[AgentId] AND E.[ResourceId] = LDD.[ResourceId]
		WHERE L.[State] = 4
		AND E.UnitId <> @PureUnitId
		-- never depreciated for the period
		AND (LDD.LastDepreciationDate IS NULL OR LDD.LastDepreciationDate < @DepreciationPeriodEnds)
		-- depreciation date start has passed
		AND FDD.FirstDepreciationDate <= @DepreciationPeriodEnds
		GROUP BY E.[AgentId], E.[ResourceId], FDD.[FirstDepreciationDate], LDD.[LastDepreciationDate]
		-- there is value to depreciate
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) > 0
	)
	INSERT INTO @WideLines([Index],
		[DocumentIndex], [AgentId1], [ResourceId1],
		[Time10],
		[CurrencyId0], [CurrencyId1], [CenterId0]
		)
	SELECT ROW_NUMBER() OVER(ORDER BY DPPE.[AgentId], DPPE.[ResourceId]) - 1,
			@DocumentIndex, DPPE.[AgentId], DPPE.[ResourceId],
			ISNULL(DATEADD(DAY, 1,LDD.LastDepreciationDate), DPPE.FirstDepreciationDate),
			RL.[CurrencyId], RL.[CurrencyId], LCC.[CostCenter]
	FROM DepreciablePPEs DPPE
	JOIN dbo.[Agents] RL ON RL.[Id] = DPPE.[AgentId]
	JOIN dbo.[Resources] R ON R.[Id] = DPPE.[ResourceId]
	LEFT JOIN LastDepreciationDates LDD ON DPPE.[AgentId] = LDD.[AgentId] AND DPPE.[ResourceId] = LDD.[ResourceId]
	LEFT JOIN LastCostCenters LCC ON DPPE.[AgentId] = LCC.[AgentId] AND DPPE.[ResourceId] =  LCC.[ResourceId] 

	SELECT * FROM @WideLines;
END
GO