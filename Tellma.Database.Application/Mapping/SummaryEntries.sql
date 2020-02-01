	CREATE FUNCTION [map].[SummaryEntries] (
	@FromDate Date = '01.01.2018',
	@ToDate Date = '01.01.2019',
	@ResponsibilityCenterId INT = NULL,
	@AgentDefinitionId NVARCHAR(50) = NULL,
	@AccountTypeCode NVARCHAR (255) = NULL,
	@CountUnitId INT = NULL,
	@MassUnitId INT = NULL,
	@VolumeUnitId INT = NULL
)
RETURNS TABLE AS
RETURN
	WITH AccountTypesSubtree AS (
		SELECT Id FROM dbo.[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[AccountTypes] WHERE [Code] = @AccountTypeCode)
		) = 1
	),
	ReportAccounts AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE
			(@ResponsibilityCenterId IS NULL OR [ResponsibilityCenterId] = @ResponsibilityCenterId)
		AND (@AgentDefinitionId IS NULL OR [AgentDefinitionId]= @AgentDefinitionId)
		AND (@AccountTypeCode IS NULL OR [AccountTypeId] IN (SELECT [Id] FROM AccountTypesSubtree))
	),
	OpeningBalances AS (
		SELECT
			E.[AccountId],
			E.[CurrencyId],
			SUM(E.[AlgebraicMonetaryValue]) AS [MonetaryValue],
			SUM(E.[AlgebraicCount]) AS [Count],
			SUM(E.[AlgebraicMass]) AS [Mass],
			SUM(E.[AlgebraicVolume]) AS [Volume],
			SUM(E.[AlgebraicTime]) AS [Time],
			SUM(E.[AlgebraicValue]) AS [Opening]
		FROM [map].[DetailsEntries](@CountUnitId, @MassUnitId, @VolumeUnitId) E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		WHERE 
			(@fromDate IS NOT NULL AND D.[DocumentDate] < @fromDate)
		AND E.AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY E.AccountId, E.[CurrencyId]
	),
	Movements AS (
		SELECT
			E.[AccountId], E.[EntryTypeId], E.[CurrencyId],
			SUM(CASE WHEN [Direction] > 0 THEN E.[MonetaryValue] ELSE 0 END) AS MonetaryValueIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[MonetaryValue] ELSE 0 END) AS MonetaryValueOut,

			SUM(CASE WHEN [Direction] > 0 THEN E.[Count] ELSE 0 END) AS CountIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Count] ELSE 0 END) AS CountOut,
			SUM(CASE WHEN [Direction] > 0 THEN E.[Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Mass] ELSE 0 END) AS MassOut,
			SUM(CASE WHEN [Direction] > 0 THEN E.[Volume] ELSE 0 END) AS VolumeIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Volume] ELSE 0 END) AS VolumeOut,

			SUM(CASE WHEN [Direction] > 0 THEN E.[Value] ELSE 0 END) AS [Debit],
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Value] ELSE 0 END) AS [Credit]
		FROM [map].[DetailsEntries](@CountUnitId, @MassUnitId, @VolumeUnitId) E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		JOIN [dbo].[Documents] D ON L.[DocumentId] = D.[Id]
		WHERE 
			(@fromDate IS NULL OR D.[DocumentDate] >= @fromDate)
		AND (@toDate IS NULL OR D.[DocumentDate] < DATEADD(DAY, 1, @toDate))
		AND E.AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY E.AccountId, E.[EntryTypeId], E.[CurrencyId]
	),
	Register AS (
		SELECT
			COALESCE(OpeningBalances.AccountId, Movements.AccountId) AS AccountId, [EntryTypeId],
			ISNULL(OpeningBalances.[Count],0) AS OpeningCount, ISNULL(OpeningBalances.[Mass],0) AS OpeningMass, ISNULL(OpeningBalances.[Volume],0) AS OpeningVolume ,ISNULL(OpeningBalances.[Opening],0) AS Opening,
			ISNULL(Movements.[CountIn],0) AS CountIn, ISNULL(Movements.[CountOut],0) AS CountOut,
			ISNULL(Movements.[MassIn],0) AS MassIn, ISNULL(Movements.[MassOut],0) AS MassOut,
			ISNULL(Movements.[VolumeIn],0) AS VolumeIn, ISNULL(Movements.[VolumeOut],0) AS VolumeOut,
			ISNULL(Movements.[Debit], 0) AS [Debit], ISNULL(Movements.[Credit], 0) AS [Credit],
			ISNULL(OpeningBalances.[Count], 0) + ISNULL(Movements.[CountIn], 0) - ISNULL(Movements.[CountOut],0) AS EndingCount,
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS EndingMass,
			ISNULL(OpeningBalances.[Volume], 0) + ISNULL(Movements.[VolumeIn], 0) - ISNULL(Movements.[VolumeOut],0) AS EndingVolume,
			ISNULL(OpeningBalances.[Opening], 0) + ISNULL(Movements.[Debit], 0) - ISNULL(Movements.[Credit],0) AS [Closing]
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.AccountId = Movements.AccountId
	)
	SELECT
		AccountId, R.[EntryTypeId], A.[AccountTypeId], A.[LegacyClassificationId], A.[ResourceId], A.[AgentId],-- A.PartyReference,
		OpeningCount, CountIn, CountOut, EndingCount,
		OpeningMass, MassIn, MassOut, EndingMass,
		[Opening], [Debit], [Credit], [Closing]
	FROM Register R
	JOIN dbo.Accounts A ON R.[AccountId] = A.[Id]
	WHERE A.[Id] IN (SELECT Id FROM ReportAccounts)
;