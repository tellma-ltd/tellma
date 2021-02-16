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
			SUM(J.[Direction] * J.[BaseQuantity] * 
			IIF(RBU.[UnitType] = N'Mass', RBU.[BaseAmount] / RBU.[UnitAmount] , R.[UnitMass])
			) AS [Mass],
			SUM(J.[Direction] * J.[BaseQuantity]) AS [Quantity]
		FROM [map].[DetailsEntries]() J
		JOIN dbo.Lines L ON J.LineId = L.Id
		JOIN dbo.Resources R ON J.[ResourceId] = R.[Id]
		JOIN dbo.[Units] RBU ON R.[UnitId] = RBU.[Id]
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
		SUM(BE.[Quantity] * R.[UnitMass]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN BE.[FromDate] > @fromDate THEN BE.[FromDate] ELSE @fromDate END),
				(CASE WHEN BE.[ToDate] < @toDate THEN BE.[ToDate] Else @toDate END)
			) + 1
		) As [Mass],
		SUM([Quantity]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN BE.[FromDate] > @fromDate THEN BE.[FromDate] ELSE @fromDate END),
				(CASE WHEN BE.[ToDate] < @toDate THEN BE.[ToDate] Else @toDate END)
			) + 1
		) As [Quantity]
		FROM dbo.[BudgetEntries] BE
		JOIN dbo.[Budgets] B ON B.[Id] = BE.[BudgetId]
		LEFT JOIN dbo.Resources R ON B.ResourceId = R.Id
		WHERE (BE.[ToDate] >= @fromDate AND BE.[FromDate] <= @toDate)
		AND [EntryTypeId] = @InventoryProductionExtension
		GROUP BY [ResourceId], BE.[FromDate], BE.[ToDate]
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