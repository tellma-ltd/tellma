CREATE PROCEDURE [api].[ExchangeVariances__Prepare]
@DocumentDate DATE,
-- TODO: Allow a list of currencies and their rates?
@CurrencyId NCHAR(3),
@Rate FLOAT(53),
@ExchangeVarianceAccount INT
AS
-- TODO: Add test unit
BEGIN
	DECLARE	@Documents [dbo].DocumentList, @Lines [dbo].[LineList], @Entries [dbo].EntryList;
	
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
	ExchangeVarianceAccounts AS 
	(
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsCurrent] = 1
		AND [CurrencyId] = @CurrencyId
		AND [AccountTypeId] IN (SELECT [Id] FROM ExchangeVarianceAccountTypes)
	),
	ExchangeVarianceEntries AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AccountId]) AS [Index],
		[AccountId], [AgentId], [ResourceId], -SUM(E.[AlgebraicValue]) AS ValueBalance, SUM(E.[AlgebraicMonetaryValue]) AS FXBalance
		FROM [rpt].[Entries](NULL, @DocumentDate) E
		WHERE E.[CurrencyId] = @CurrencyId
		AND [AccountId] IN (SELECT [Id] FROM ExchangeVarianceAccounts)
		GROUP BY [AccountId], [AgentId], [ResourceId]
		HAVING SUM(E.[AlgebraicValue]) * @Rate <> SUM(E.[AlgebraicMonetaryValue])
	),
	GainLossEntry AS (
		SELECT [AccountId], [AgentId], [ResourceId], ABS(ValueBalance - FXBalance) AS [Value], SIGN(ValueBalance - FXBalance) AS [Direction]
		FROM ExchangeVarianceEntries
	)
	INSERT INTO @Entries([Index], [LineIndex], [Direction],[AccountId], [AgentId], [ResourceId], [Quantity], [Value])
	SELECT [Index], [Index],
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], [AccountId], [AgentId], [ResourceId], 0, CAST(ABS([ValueBalance]) AS DECIMAL (19,4)) AS [ValueBalance]
	FROM ExchangeVarianceEntries

	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	[LineIndex], 0 AS [DocumentIndex]
	FROM @Entries
	GROUP BY [LineIndex];

	INSERT INTO @Documents([DocumentDate]) VALUES(@DocumentDate);
	SELECT * FROM @Documents; SELECT *, N'ManualLine' As [LineDefinitionId] FROM @Lines; SELECT * FROM @Entries;
END
