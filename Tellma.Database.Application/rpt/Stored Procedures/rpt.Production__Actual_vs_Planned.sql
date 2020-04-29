CREATE PROCEDURE [rpt].[Production__Actual_vs_Planned]
	@fromDate Date,
	@toDate Date
	-- TODO: rewrite using summary entries
AS
BEGIN
	WITH
	Actual([ResourceLookup1Id], [ResponsibleActorId], [Mass], [Count]) AS (
		SELECT 
			R.[Lookup1Id], J.[ContractId],
			SUM(J.[AlgebraicMass]) AS [Mass],
			SUM(J.[AlgebraicCount]) AS [Count]
		FROM [map].[DetailsEntries]() J --(@FromDate, @ToDate) J
		JOIN dbo.Resources R ON J.ResourceId = R.Id
		JOIN dbo.[Accounts] A ON J.AccountId = A.[Id]
		JOIN dbo.[AccountDefinitions] AD ON A.[DefinitionId] = AD.[Id]
		-- assuming that inventory entries require IfrsNoteExtension
		WHERE J.[EntryTypeId] = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'ProductionOfGoods') 
		AND AD.[Code] = N'Inventory'
		GROUP BY J.[ContractId], R.[Lookup1Id]
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