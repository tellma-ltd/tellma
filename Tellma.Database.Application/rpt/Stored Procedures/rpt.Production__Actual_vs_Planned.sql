CREATE PROCEDURE [rpt].[Production__Actual_vs_Planned]
	@fromDate Date,
	@toDate Date
	-- TODO: rewrite using summary entries
AS
BEGIN
	WITH FinishedGoodsAccountTypes AS (
		SELECT Id FROM dbo.[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[AccountTypes] WHERE [Code] = N'FinishedGoods')
		) = 1
	),
	Actual([ResourceLookup1Id], [ResponsibleActorId], [Mass], [Count]) AS (
		SELECT 
			R.[Lookup1Id], J.[RelationId],
			SUM(J.[AlgebraicMass]) AS [Mass],
			SUM(J.[AlgebraicCount]) AS [Count]
		FROM [map].[DetailsEntries]() J --(@FromDate, @ToDate) J
		JOIN dbo.Resources R ON J.ResourceId = R.Id
		LEFT JOIN dbo.[AccountTypes] RC ON R.[AssetTypeId] = RC.Id
		WHERE J.[EntryTypeId] = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'ProductionOfGoods') -- assuming that inventory entries require IfrsNoteExtension
		-- TODO: we need a way to separate finished goods from the rest
		AND R.[AssetTypeId] IN (SELECT [Id] FROM FinishedGoodsAccountTypes)
		GROUP BY J.[RelationId], R.[Lookup1Id]
	),
	PlannedDetails([ResourceLookup1Id], [Mass], [MassUnitId], [Count], [CountUnitId]) AS (
		SELECT 
		ResourceLookup1Id,
		SUM([Mass]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @toDate THEN ToDate Else @toDate END)
			) + 1
		) As [Mass],
		[MassUnitId],
		SUM([Count]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @toDate THEN ToDate Else @toDate END)
			) + 1
		) As [Count],
		[CountUnitId]
		FROM dbo.Plans
		WHERE (ToDate >= @fromDate AND FromDate <= @ToDate)
		AND Activity = N'Production'
		GROUP BY ResourceLookup1Id, [MassUnitId], [CountUnitId], [FromDate], [ToDate]
	),
	Planned([ResourceLookup1Id], [Mass], [Count])	AS (
		SELECT ResourceLookup1Id, 
		SUM([Mass]) AS [Mass], 
		SUM([Count]) AS [Count]
		FROM PlannedDetails P
		GROUP BY ResourceLookup1Id
	)
	SELECT RL.Id, RL.SortKey, RL.[Name],
		A.[Mass] AS MassActual, P.Mass As MassPlanned, A.Mass/P.Mass * 100 As [PercentOfMassPlanned],
		A.[Count] AS CountActual, P.[Count] AS CountPlanned, A.[Count]/P.[Count] * 100 As [PercentOfCountPlanned]
	FROM dbo.[Lookups] RL
	LEFT JOIN Actual A ON RL.Id = A.ResourceLookup1Id
	LEFT JOIN Planned P ON RL.Id = P.ResourceLookup1Id
	AND 
	(
		(A.Mass IS NOT NULL AND A.Mass <> 0) OR 
		(P.Mass IS NOT NULL AND P.Mass <> 0)
	);
END;