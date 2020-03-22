CREATE PROCEDURE [api].[GainLossOnExchange__Prepare]
@Documents dbo.DocumentList READONLY, -- only one item in the list
@LineDefinitionId NVARCHAR (50) = N'ManualLine'
AS
BEGIN
	DECLARE	@Lines [dbo].[LineList], @Entries [dbo].EntryList;
	DECLARE @PostingDate DATE;
	SELECT @PostingDate = [PostingDate] FROM @Documents;
	IF @PostingDate IS NULL 
	BEGIN
		RAISERROR (N'Posting Date is Required', 16, 1);
		RETURN
	END

	DECLARE @CashAndCashEquivalentsNode HIERARCHYID = (
		SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'CashAndCashEquivalents'
	);
	DECLARE @EffectOfExchangeRateChangesOnCashAndCashEquivalents INT = (
		SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'EffectOfExchangeRateChangesOnCashAndCashEquivalents'
	);
	DECLARE @ForeignExchangeGainLossAccount INT = (
		SELECT MIN([Id]) FROM dbo.Accounts
		WHERE AccountTypeId = (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'GainLossOnForeignExchangeExtension'
		)
	);

	WITH 
	ExchangeVarianceAccountTypes AS
	(
		SELECT T2.[Id]
		FROM [dbo].[AccountTypes] T1
		JOIN [dbo].[AccountTypes] T2
		ON T2.[Node].IsDescendantOf(T1.[Node]) = 1
		WHERE T1.[Code] IN (
			N'TradeAndOtherReceivables',
			N'CashAndCashEquivalents',
			N'TradeAndOtherPayables',
			N'OtherFinancialAssets',
			N'OtherFinancialLiabilities')
		AND (T2.IsCurrent = 1 OR T2.IsCurrent IS NULL)
	),
	CashAndCashEquivalentsAccounts AS
	(
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@CashAndCashEquivalentsNode) = 1
		)
	),
	ExchangeVarianceAccounts AS 
	(
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsCurrent] = 1
		AND [AccountTypeId] IN (SELECT [Id] FROM ExchangeVarianceAccountTypes)
	),
	ExchangeVarianceEntries AS (
		SELECT ROW_NUMBER() OVER (ORDER BY E.[AccountId]) AS [Index],
		E.[AccountId], E.[AgentId], E.[ResourceId], E.[CurrencyId],
		ER.[Rate] * SUM(E.[Direction] * E.[MonetaryValue]) - SUM(E.[Direction] * E.[Value]) AS [NetGainLoss]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
		JOIN dbo.ExchangeRatesView ER ON E.CurrencyId = ER.CurrencyId AND @PostingDate >= ER.ValidAsOf AND @PostingDate < ER.ValidTill
		AND E.[AccountId] IN (SELECT [Id] FROM ExchangeVarianceAccounts)
		AND L.[State] = 4 AND D.[PostingState] = 1
		AND D.[PostingDate] <= @PostingDate
		GROUP BY E.[AccountId], E.[AgentId], E.[ResourceId], E.[CurrencyId], ER.Rate
		HAVING SUM(E.[Direction] * E.[Value]) * ER.Rate <> SUM(E.[Direction] * E.[MonetaryValue])
	),
	GainLossEntry AS (
		SELECT
			(SELECT MAX([Index]) + 1 FROM ExchangeVarianceEntries) AS [Index],
			@ForeignExchangeGainLossAccount AS [AccountId],
			-SIGN(SUM([NetGainLoss])) AS [Direction],
			ABS(SUM([NetGainLoss])) AS [Value]
		FROM ExchangeVarianceEntries
		WHERE ABS([NetGainLoss]) <> 0
	)
	INSERT INTO @Entries([Index], [LineIndex],
		[Direction],
		[AccountId], [AgentId], [ResourceId], [Quantity],
		[Value],
		[EntryTypeId])
	SELECT
		[Index],
		[Index] AS [LineIndex],
		SIGN([NetgainLoss]) AS [Direction],
		[AccountId], [AgentId], [ResourceId], 0,
		ABS([NetgainLoss]) AS [Value],
		IIF([AccountId] IN (SELECT [Id] FROM CashAndCashEquivalentsAccounts),
			@EffectOfExchangeRateChangesOnCashAndCashEquivalents, NULL) AS [EntryTypeId]
	FROM ExchangeVarianceEntries
	UNION
	SELECT
		[Index],
		[Index] AS [LineIndex],
		[Direction],
		[AccountId], NULL, NULL, NULL,
		[Value],
		NULL
	FROM GainLossEntry

	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	DISTINCT [LineIndex], [DocumentIndex]
	FROM @Entries;

	SELECT * FROM @Lines; SELECT * FROM @Entries;
END
