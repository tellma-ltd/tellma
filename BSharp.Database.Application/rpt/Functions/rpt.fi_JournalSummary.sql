CREATE FUNCTION [rpt].[fi_JournalSummary] (
	@AccountDefinitionId NVARCHAR(50) = NULL,
	@GLAccountsCode NVARCHAR(50) = NULL,
	@FromDate Date = '01.01.2019',
	@ToDate Date = '01.01.2020',
	@MassUnitId INT = NULL,
	@CountUnitId INT = NULL
)
RETURNS TABLE AS
RETURN
	WITH
	ReportAccounts AS (
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountDefinitionId] = @AccountDefinitionId
		OR [GLAccountId] IN (
			SELECT [Id] FROM dbo.[AccountClassifications]
			WHERE [Code] LIKE @GLAccountsCode + '%'
		)
	),
	OpeningBalances AS (
		SELECT
			AccountId,
			SUM([Direction] * [Mass]) AS [Mass],
			SUM([Direction] * [Volume]) AS [Volume],
			SUM([Direction] * [Area]) AS [Area],
			SUM([Direction] * [MonetaryValue]) AS [MonetaryValue],
			SUM([Direction] * [Count]) AS [Count],
			SUM([Direction] * [Value]) AS [Opening]
		FROM [dbo].[fi_NormalizedJournal](NULL, DATEADD(DAY, -1, @FromDate), NULL, NULL)
		WHERE AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY AccountId
	),
	Movements AS (
		SELECT
			AccountId, IfrsEntryClassificationId,
			SUM(CASE WHEN [Direction] > 0 THEN [Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Mass] ELSE 0 END) AS MassOut,
			SUM(CASE WHEN [Direction] > 0 THEN [Count] ELSE 0 END) AS CountIn,
			SUM(CASE WHEN [Direction] < 0 THEN [Count] ELSE 0 END) AS CountOut,
			SUM(CASE WHEN [Direction] > 0 THEN [MonetaryValue] ELSE 0 END) AS MonetaryValueIn,
			SUM(CASE WHEN [Direction] < 0 THEN [MonetaryValue] ELSE 0 END) AS MonetaryValueOut,
			SUM(CASE WHEN [Direction] > 0 THEN [Value] ELSE 0 END) AS [Debit],
			SUM(CASE WHEN [Direction] < 0 THEN [Value] ELSE 0 END) AS [Credit]
		FROM [dbo].[fi_NormalizedJournal](@FromDate, @ToDate, @MassUnitId, @CountUnitId)
		WHERE AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY AccountId, IfrsEntryClassificationId
	),
	Register AS (
		SELECT
			COALESCE(OpeningBalances.AccountId, Movements.AccountId) AS AccountId, IfrsEntryClassificationId,
			ISNULL(OpeningBalances.[Count],0) AS OpeningCount, ISNULL(OpeningBalances.[Mass],0) AS OpeningMass, ISNULL(OpeningBalances.[Opening],0) AS Opening,
			ISNULL(Movements.[CountIn],0) AS CountIn, ISNULL(Movements.[CountOut],0) AS CountOut,
			ISNULL(Movements.[MassIn],0) AS MassIn, ISNULL(Movements.[MassOut],0) AS MassOut,
			ISNULL(Movements.[Debit], 0) AS [Debit], ISNULL(Movements.[Credit], 0) AS [Credit],
			ISNULL(OpeningBalances.[Count], 0) + ISNULL(Movements.[CountIn], 0) - ISNULL(Movements.[CountOut],0) AS EndingCount,
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS EndingMass,
			ISNULL(OpeningBalances.[Opening], 0) + ISNULL(Movements.[Debit], 0) - ISNULL(Movements.[Credit],0) AS [Closing]
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.AccountId = Movements.AccountId
	)
	SELECT
		AccountId, R.IfrsEntryClassificationId, A.GLAccountId, A.ResourceId, A.CustodianActorId, A.ResponsibleActorId, A.LocationId, A.SubAccountId,
		OpeningCount, CountIn, CountOut, EndingCount,
		OpeningMass, MassIn, MassOut, EndingMass,
		[Opening], [Debit], [Credit], [Closing]
	FROM Register R
	JOIN dbo.Accounts A ON R.[AccountId] = A.[Id]
;