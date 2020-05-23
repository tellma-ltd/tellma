CREATE PROCEDURE [rpt].[Production__Actual_vs_Planned]
	@fromDate Date,
	@toDate Date
	-- TODO: rewrite using summary entries
AS
BEGIN
	DECLARE @InventoryProductionExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'InventoryProductionExtension');
	DECLARE @FinishedGoods INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'FinishedGoods');
	WITH
	Actual([ResourceId], [Mass], [Count]) AS (
		SELECT
			J.[ResourceId],
			SUM(J.[AlgebraicMass]) AS [Mass],
			SUM(J.[AlgebraicCount]) AS [Count]
		FROM [map].[DetailsEntries]() J
		JOIN dbo.Lines L ON J.LineId = L.Id
		JOIN dbo.[Accounts] A ON J.AccountId = A.[Id]
		WHERE J.[EntryTypeId] = @InventoryProductionExtension 
		AND A.[AccountTypeId] = @FinishedGoods
		AND L.[State] = 4
		AND L.PostingDate Between @fromDate AND @toDate
		GROUP BY J.[ResourceId]
	),
	-- TODO: use map.DetailsBudgetEntries
	Planned([ResourceId], [Mass], [Count]) AS (
		SELECT 
		ResourceId,
		SUM([Mass]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @toDate THEN ToDate Else @toDate END)
			) + 1
		) As [Mass],
		SUM([Count]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @toDate THEN ToDate Else @toDate END)
			) + 1
		) As [Count]
		FROM dbo.[BudgetEntries] BE
		JOIN dbo.[Budgets] B ON B.[Id] = BE.[BudgetId]
		WHERE (ToDate >= @fromDate AND FromDate <= @ToDate)
		AND [EntryTypeId] = @InventoryProductionExtension
		GROUP BY [ResourceId], [FromDate], [ToDate]
	)
	SELECT RL.Id, RL.[Name],
		A.[Mass] AS MassActual, P.Mass As MassPlanned, A.Mass/P.Mass * 100 As [PercentOfMassPlanned],
		A.[Count] AS CountActual, P.[Count] AS CountPlanned, A.[Count]/P.[Count] * 100 As [PercentOfCountPlanned]
	FROM dbo.[Resources] RL
	LEFT JOIN Actual A ON RL.Id = A.ResourceId
	LEFT JOIN Planned P ON RL.Id = P.ResourceId
	AND 
	(
		(A.Mass IS NOT NULL AND A.Mass <> 0) OR 
		(P.Mass IS NOT NULL AND P.Mass <> 0)
	);
END;