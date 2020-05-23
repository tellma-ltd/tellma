CREATE PROCEDURE [wiz].[GainLossOnExchange__Prepare]
@Documents dbo.DocumentList READONLY, -- only one item in the list
@LineDefinitionId INT = NULL
AS
BEGIN
	IF @LineDefinitionId = NULL 
		SELECT @LineDefinitionId = [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine';
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
		WHERE [AccountTypeId] = (
			SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'exchange-variance'
		)
	);

	DECLARE @E INT;
	SELECT @E = E FROM dbo.Currencies WHERE [Id] = dbo.fn_FunctionalCurrencyId();
	
	WITH 
	ExchangeVarianceAccountTypes AS
	(
		SELECT T2.[Id]
		FROM [dbo].[AccountTypes] T1
		JOIN [dbo].[AccountTypes] T2
		ON T2.[Node].IsDescendantOf(T1.[Node]) = 1
		WHERE T1.[Code] IN (
			N'TradeAndOtherCurrentReceivables',
			N'CashAndCashEquivalents',
			N'TradeAndOtherCurrentPayables',
			N'OtherCurrentFinancialAssets',
			N'OtherCurrentFinancialLiabilities')
	),
	CashAndCashEquivalentsAccounts AS
	(
		SELECT [Id] FROM dbo.Accounts
		WHERE [AccountTypeId] IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Node].IsDescendantOf(@CashAndCashEquivalentsNode) = 1
		)
	),
	ExchangeVarianceAccounts AS 
	(
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [AccountTypeId] IN (SELECT [Id] FROM ExchangeVarianceAccountTypes)
	),
	ExchangeVarianceEntries AS (
		SELECT ROW_NUMBER() OVER (ORDER BY E.[AccountId]) AS [Index],
		E.[AccountId], E.[ContractId], E.[ResourceId], E.[CurrencyId],
		ROUND(ER.[Rate] * SUM(E.[Direction] * E.[MonetaryValue]) - SUM(E.[Direction] * E.[Value]), @E) AS [NetGainLoss]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.LineId = L.Id
		JOIN [map].[ExchangeRates]() ER ON E.CurrencyId = ER.CurrencyId AND @PostingDate >= ER.ValidAsOf AND @PostingDate < ER.ValidTill
		AND E.[AccountId] IN (SELECT [Id] FROM ExchangeVarianceAccounts)
		AND L.[State] = 4
		AND L.[PostingDate] <= @PostingDate
		GROUP BY E.[AccountId], E.[ContractId], E.[ResourceId], E.[CurrencyId], ER.Rate
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
		[AccountId], [ContractId], [ResourceId], [Quantity],
		[Value],
		[EntryTypeId])
	SELECT
		[Index],
		0 AS [LineIndex],
		SIGN([NetgainLoss]) AS [Direction],
		[AccountId], [ContractId], [ResourceId], 0,
		ABS([NetgainLoss]) AS [Value],
		IIF([AccountId] IN (SELECT [Id] FROM CashAndCashEquivalentsAccounts),
			@EffectOfExchangeRateChangesOnCashAndCashEquivalents, NULL) AS [EntryTypeId]
	FROM ExchangeVarianceEntries
	UNION
	SELECT
		[Index],
		0 AS [LineIndex],
		[Direction],
		[AccountId], NULL,			NULL,		NULL,
		[Value],
		NULL
	FROM GainLossEntry

	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	DISTINCT [LineIndex], [DocumentIndex]
	FROM @Entries;

	SELECT * FROM @Lines; SELECT * FROM @Entries;
END