CREATE FUNCTION [map].[SummaryEntries] (
	@fromDate Date = '01.01.2018',
	@toDate Date = '01.01.2019',
	@CenterId INT = NULL,
	@AccountTypeConcept NVARCHAR (255) = NULL
)
RETURNS TABLE AS
RETURN
	WITH AccountTypesSubtree AS (
		SELECT Id FROM dbo.[AccountTypes]
		WHERE [Node].IsDescendantOf(
			(SELECT [Node] FROM dbo.[AccountTypes] WHERE [Concept] = @AccountTypeConcept)
		) = 1
	),
	ReportAccounts AS (
		SELECT A.[Id] FROM dbo.[Accounts] A
		JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.Id
		WHERE
			(@CenterId IS NULL OR [CenterId] = @CenterId)
		AND (@AccountTypeConcept IS NULL OR [AccountTypeId] IN (SELECT [Id] FROM AccountTypesSubtree))
	),
	OpeningBalances AS (
		SELECT
			E.[AccountId],
			E.[CurrencyId],
			SUM(E.[AlgebraicMonetaryValue]) AS [MonetaryValue],
			SUM(E.[AlgebraicQuantity]) AS [Quantity],
			SUM(E.[AlgebraicMass]) AS [Mass],
			SUM(E.[AlgebraicValue]) AS [Opening]
		FROM [map].[DetailsEntries]() E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		WHERE 
			(@fromDate IS NOT NULL AND L.[PostingDate] < @fromDate)
		AND E.AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY E.AccountId, E.[CurrencyId]
	),
	Movements AS (
		SELECT
			E.[AccountId], E.[EntryTypeId], E.[CurrencyId],
			SUM(CASE WHEN [Direction] > 0 THEN E.[MonetaryValue] ELSE 0 END) AS MonetaryValueIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[MonetaryValue] ELSE 0 END) AS MonetaryValueOut,

			SUM(CASE WHEN [Direction] > 0 THEN E.[Quantity] ELSE 0 END) AS QuantityIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Quantity] ELSE 0 END) AS QuantityOut,
			SUM(CASE WHEN [Direction] > 0 THEN E.[Mass] ELSE 0 END) AS MassIn,
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Mass] ELSE 0 END) AS MassOut,

			SUM(CASE WHEN [Direction] > 0 THEN E.[Value] ELSE 0 END) AS [Debit],
			SUM(CASE WHEN [Direction] < 0 THEN -E.[Value] ELSE 0 END) AS [Credit]
		FROM [map].[DetailsEntries]() E
		JOIN [dbo].[Lines] L ON E.[LineId] = L.Id
		WHERE 
			(@fromDate IS NULL OR L.[PostingDate] >= @fromDate)
		AND (@toDate IS NULL OR L.[PostingDate] < DATEADD(DAY, 1, @toDate))
		AND E.AccountId IN (SELECT Id FROM ReportAccounts)
		GROUP BY E.AccountId, E.[EntryTypeId], E.[CurrencyId]
	),
	Register AS (
		SELECT
			COALESCE(OpeningBalances.AccountId, Movements.AccountId) AS AccountId, [EntryTypeId],
			ISNULL(OpeningBalances.[Quantity],0) AS OpeningQuantity, ISNULL(OpeningBalances.[Mass],0) AS OpeningMass, ISNULL(OpeningBalances.[Opening],0) AS Opening,
			ISNULL(Movements.[QuantityIn],0) AS QuantityIn, ISNULL(Movements.[QuantityOut],0) AS QuantityOut,
			ISNULL(Movements.[MassIn],0) AS MassIn, ISNULL(Movements.[MassOut],0) AS MassOut,
			ISNULL(Movements.[Debit], 0) AS [Debit], ISNULL(Movements.[Credit], 0) AS [Credit],
			ISNULL(OpeningBalances.[Quantity], 0) + ISNULL(Movements.[QuantityIn], 0) - ISNULL(Movements.[QuantityOut],0) AS EndingQuantity,
			ISNULL(OpeningBalances.[Mass], 0) + ISNULL(Movements.[MassIn], 0) - ISNULL(Movements.[MassOut],0) AS EndingMass,
			ISNULL(OpeningBalances.[Opening], 0) + ISNULL(Movements.[Debit], 0) - ISNULL(Movements.[Credit],0) AS [Closing]
		FROM OpeningBalances
		FULL OUTER JOIN Movements ON OpeningBalances.AccountId = Movements.AccountId
	)
	SELECT
		AccountId, R.[EntryTypeId], A.[AccountTypeId], A.[ClassificationId], A.[ResourceId], A.[ContractId],-- A.PartyReference,
		OpeningQuantity, QuantityIn, QuantityOut, EndingQuantity,
		OpeningMass, MassIn, MassOut, EndingMass,
		[Opening], [Debit], [Credit], [Closing]
	FROM Register R
	JOIN dbo.Accounts A ON R.[AccountId] = A.[Id]
	WHERE A.[Id] IN (SELECT Id FROM ReportAccounts)
;