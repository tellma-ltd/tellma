	CREATE FUNCTION [rpt].[fi_JournalSummary] (
	--TODO: make the filtering of these parameters from NormalizedJournal
	@OperatingSegmentId INT,
	@AccountTypeId NVARCHAR(1024) = NULL,
	@AgentDefinitionId NVARCHAR(50),
	@ResourceClassificationCode NVARCHAR (255),
	@FromDate Date = '01.01.2019',
	@ToDate Date = '01.01.2020',
	@CountUnitId INT = NULL,
	@MassUnitId INT = NULL,
	@VolumeUnitId INT = NULL
)
RETURNS TABLE AS
RETURN
	WITH ResourceClassifications AS (
		SELECT Id FROM dbo.[ResourceClassifications]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[ResourceClassifications] WHERE [Code] = @ResourceClassificationCode)
		) = 1
	),
	ReportAccounts AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE (@AccountTypeId IS NULL OR [AccountTypeId]= @AccountTypeId)
		AND (@AccountTypeId IS NULL OR [AccountTypeId]= @AccountTypeId)
		AND (@AgentDefinitionId IS NULL OR [AgentDefinitionId]= @AgentDefinitionId)
		AND (@ResourceClassificationCode IS NULL OR [ResourceClassificationId] IN (SELECT [Id] FROM ResourceClassifications))
	),
	OpeningBalances AS (
		SELECT
			[AccountId],
			[CurrencyId],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue],
			SUM([Direction] * [Count]) AS [Count],
			SUM([Direction] * [Mass]) AS [Mass],
			SUM([Direction] * [Volume]) AS [Volume],
			SUM([Direction] * [Time]) AS [Time],
			SUM([Direction] * [Value]) AS [Opening]
		FROM [dbo].[fi_NormalizedJournal](NULL, DATEADD(DAY, -1, @FromDate),  @CountUnitId, @MassUnitId, @VolumeUnitId)
		WHERE AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY AccountId, [CurrencyId]
	),
	Movements AS (
		SELECT
			[AccountId], [EntryClassificationId], [CurrencyId],
			SUM(CASE WHEN [Direction] > 0 THEN [MonetaryValue] ELSE 0 END) AS MonetaryValueIn,
			SUM(CASE WHEN [Direction] < 0 THEN [MonetaryValue] ELSE 0 END) AS MonetaryValueOut,

			SUM(CASE WHEN [Direction] > 0 THEN [Count] ELSE 0 END) AS CountIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Count] ELSE 0 END) AS CountOut,
			SUM(CASE WHEN [Direction] > 0 THEN [Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Mass] ELSE 0 END) AS MassOut,
			SUM(CASE WHEN [Direction] > 0 THEN [Volume] ELSE 0 END) AS VolumeIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Volume] ELSE 0 END) AS VolumeOut,

			SUM(CASE WHEN [Direction] > 0 THEN [Value] ELSE 0 END) AS [Debit],
			SUM(CASE WHEN [Direction] < 0 THEN [Value] ELSE 0 END) AS [Credit]
		FROM [dbo].[fi_NormalizedJournal](@FromDate, @ToDate, @CountUnitId, @MassUnitId, @VolumeUnitId)
		WHERE AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY AccountId, [EntryClassificationId], [CurrencyId]
	),
	Register AS (
		SELECT
			COALESCE(OpeningBalances.AccountId, Movements.AccountId) AS AccountId, [EntryClassificationId],
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
		AccountId, R.[EntryClassificationId], A.[AccountTypeId], A.[AccountClassificationId], A.[ResourceId], A.[AgentId],-- A.PartyReference,
		OpeningCount, CountIn, CountOut, EndingCount,
		OpeningMass, MassIn, MassOut, EndingMass,
		[Opening], [Debit], [Credit], [Closing]
	FROM Register R
	JOIN dbo.Accounts A ON R.[AccountId] = A.[Id]
;