CREATE PROCEDURE [dbo].[rpt_Production__Actual_vs_Planned]
	@FromDate Date,
	@ToDate Date,
	@MassUnitId INT,
	@CountUnitId INT
AS
BEGIN
	-- Code commented since we are using ResourceType to distinguish
	--WITH IfrsFinishedGoodsAccounts	AS (
	--	SELECT Id FROM dbo.[IfrsAccounts]
	--	WHERE [Node].IsDescendantOf(
	--		(SELECT [Node] FROM dbo.IfrsAccounts WHERE Id = N'FinishedGoods')
	--	) = 1
	--),
	--FinishedGoodsAccounts AS (
	--	SELECT Id FROM dbo.[Accounts]
	--	WHERE IfrsAccountId IN
	--		(SELECT [Id] FROM IfrsFinishedGoodsAccounts)
	--),
	WITH UnitConversionRates([Id], [ConversionRate]) AS (
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @MassUnitId)) As [ConversionRate]
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Mass'
		UNION
		SELECT [Id], [UnitAmount] * (SELECT [BaseAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @CountUnitId)
		/ ([BaseAmount] * (SELECT [UnitAmount] FROM  dbo.MeasurementUnits WHERE [Id] = @CountUnitId))
		FROM dbo.MeasurementUnits
		WHERE UnitType = N'Count'
	),
	Actual([ResourceLookup1Id], [ResponsibleActorId], [Mass], [Count]) AS (
		SELECT 
			R.[ResourceLookup1Id], J.[ResponsibleActorId],
			SUM(J.Direction * J.[Mass]) AS [Mass],
			SUM(J.Direction * J.[Count]) AS [Count]
		FROM [fi_NormalizedJournal](@FromDate, @ToDate, @MassUnitId, @CountUnitId) J
		JOIN dbo.Resources R ON J.ResourceId = R.Id
		LEFT JOIN dbo.ResourceClassifications RC ON R.ResourceClassificationId = RC.Id
		WHERE J.[IfrsEntryClassificationId] = N'ProductionOfGoods' -- assuming that inventory entries require IfrsNoteExtension
		-- TODO: we need a way to separate finished goods from the rest
		AND RC.[ResourceDefinitionId] = N'finished-goods'
		GROUP BY J.[ResponsibleActorId], R.ResourceLookup1Id
	),
	PlannedDetails([ResourceLookup1Id], [Mass], [MassUnitId], [Count], [CountUnitId]) AS (
		SELECT 
		ResourceLookup1Id,
		SUM([Mass]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @ToDate THEN ToDate Else @ToDate END)
			) + 1
		) As [Mass],
		[MassUnitId],
		SUM([Count]) * (
			DATEDIFF(
				DAY,
				(CASE WHEN FromDate > @fromDate THEN FromDate ELSE @fromDate END),
				(CASE WHEN ToDate < @ToDate THEN ToDate Else @ToDate END)
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
		SUM([Mass] * ISNULL(MR.[ConversionRate], 0)) AS [Mass], 
		SUM([Count] * ISNULL(CR.[ConversionRate], 0)) AS [Count]
		FROM PlannedDetails P
		LEFT JOIN UnitConversionRates MR ON P.MassUnitId = MR.Id
		LEFT JOIN UnitConversionRates CR ON P.CountUnitId = CR.Id
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