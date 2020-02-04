CREATE PROCEDURE [api].[ExchangeVariances__Prepare]
@DocumentDate DATE,
-- TODO: Allow a list of currencies and their rates?
@CurrencyId NCHAR(3),
@Rate FLOAT(53),
@ExchangeVarianceAccount INT
AS
-- TODO: review Logic
BEGIN
	DECLARE	@Documents [dbo].DocumentList, @Lines [dbo].[LineList], @Entries [dbo].EntryList;
	
	WITH 
	ExchangeVarianceAccounts AS 
	(
		SELECT [Id] FROM dbo.[Accounts]
		WHERE [IsCurrent] = 1
		AND [CurrencyId] = @CurrencyId
	),
	ExchangeVarianceEntries AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AccountId]) AS [Index],
		[AccountId], [AgentId], -SUM(E.[AlgebraicValue]) AS ValueBalance, SUM(E.[AlgebraicMonetaryValue]) AS FXBalance
		FROM [rpt].[Entries](NULL, @DocumentDate) E
		WHERE E.[CurrencyId] = @CurrencyId
		AND [AccountId] IN (SELECT [Id] FROM ExchangeVarianceAccounts)
		GROUP BY [AccountId], [AgentId]
		HAVING SUM(E.[AlgebraicValue]) * @Rate <> SUM(E.[AlgebraicMonetaryValue])
	),
	GainLossEntry AS (
		SELECT [AccountId], [AgentId], ABS(ValueBalance - FXBalance) AS [Value], SIGN(ValueBalance - FXBalance) AS [Direction]
		FROM ExchangeVarianceEntries
	)
	INSERT INTO @Entries([Index], [LineIndex], [EntryNumber], [Direction],[AccountId], [AgentId], [Value])
	SELECT [Index], [Index], 1,
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], [AccountId], [AgentId], CAST(ABS([ValueBalance]) AS DECIMAL (19,4)) AS [ValueBalance]
	FROM ExchangeVarianceEntries

	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	[LineIndex], 0 AS [DocumentIndex]
	FROM @Entries
	GROUP BY [LineIndex];

	INSERT INTO @Documents([DocumentDate]) VALUES(@DocumentDate);
	SELECT * FROM @Documents; SELECT *, N'ManualLine' As [LineDefinitionId] FROM @Lines; SELECT * FROM @Entries;
END
