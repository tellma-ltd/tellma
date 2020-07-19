CREATE PROCEDURE [rpt].[Production__Actual_vs_Planned]
	@fromDate Date,
	@toDate Date
	-- TODO: rewrite using summary entries
AS
BEGIN
	DECLARE @InventoryProductionExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InventoryProductionExtension');
	DECLARE @FinishedGoods INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FinishedGoods');
	WITH
	Actual([ResourceId], [Mass], [Quantity]) AS (
		SELECT
			J.[ResourceId],
			SUM(J.[AlgebraicMass]) AS [Mass],
			SUM(J.[AlgebraicQuantity]) AS [Quantity]
		FROM [map].[DetailsEntries2](NULL) J
		JOIN dbo.Lines L ON J.LineId = L.Id
		JOIN dbo.[Accounts] A ON J.AccountId = A.[Id]
		WHERE J.[EntryTypeId] = @InventoryProductionExtension 
		AND A.[AccountTypeId] = @FinishedGoods
		AND L.[State] = 4
		AND L.PostingDate Between @fromDate AND @toDate
		GROUP BY J.[ResourceId]
	),
	-- TODO: use map.DetailsBudgetEntries
	Planned([ResourceId], [Mass], [Quantity]) AS (
		SELECT 
		ResourceId,
		SUM([Mass]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @toDate THEN ToDate Else @toDate END)
			) + 1
		) As [Mass],
		SUM([Quantity]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @toDate THEN ToDate Else @toDate END)
			) + 1
		) As [Quantity]
		FROM dbo.[BudgetEntries] BE
		JOIN dbo.[Budgets] B ON B.[Id] = BE.[BudgetId]
		WHERE (ToDate >= @fromDate AND FromDate <= @toDate)
		AND [EntryTypeId] = @InventoryProductionExtension
		GROUP BY [ResourceId], [FromDate], [ToDate]
	)
	SELECT RL.Id, RL.[Name],
		A.[Mass] AS MassActual, P.Mass As MassPlanned, A.Mass/P.Mass * 100 As [PercentOfMassPlanned],
		A.[Quantity] AS QuantityActual, P.[Quantity] AS QuantityPlanned, A.[Quantity]/P.[Quantity] * 100 As [PercentOfQuantityPlanned]
	FROM dbo.[Resources] RL
	LEFT JOIN Actual A ON RL.Id = A.ResourceId
	LEFT JOIN Planned P ON RL.Id = P.ResourceId
	AND 
	(
		(A.Mass IS NOT NULL AND A.Mass <> 0) OR 
		(P.Mass IS NOT NULL AND P.Mass <> 0)
	);
END;