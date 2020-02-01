CREATE PROCEDURE [api].[ExchangeVariances__Prepare]
@DocumentDate DATE,
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
		SELECT [Id] FROM dbo.[LegacyClassifications]
	),
	ExchangeVarianceEntries AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AccountId]) AS [Index],
		[AccountId], -SUM([Direction] * E.[Value]) AS ValueBalance, SUM([Direction] * E.[MonetaryValue]) AS FXBalance
		FROM [rpt].[Entries](NULL, @DocumentDate, NULL, NULL, NULL) E
		WHERE E.[CurrencyId] = @CurrencyId
		AND [AccountId] IN (SELECT [Id] FROM ExchangeVarianceAccounts)
		GROUP BY [AccountId]
		HAVING SUM([Direction] * E.[Value]) * @Rate <> SUM([Direction] * E.[MonetaryValue])
	),
	GainLossEntry AS (
		SELECT [AccountId], ABS(ValueBalance - FXBalance) AS [Value], SIGN(ValueBalance - FXBalance) AS [Direction]
		FROM ExchangeVarianceEntries
	)
	INSERT INTO @Entries([Index], [LineIndex], [EntryNumber], [Direction],[AccountId], [Value])
	SELECT [Index], [Index], 1,
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], E.[AccountId], CAST(ABS([ValueBalance]) AS DECIMAL (19,4)) AS [ValueBalance]
	FROM ExchangeVarianceEntries E 

	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	[LineIndex], 0 AS [DocumentIndex]
	FROM @Entries
	GROUP BY [LineIndex];

	INSERT INTO @Documents([DocumentDate]) VALUES(@DocumentDate);
	SELECT * FROM @Documents; SELECT *, N'ManualLine' As [LineDefinitionId] FROM @Lines; SELECT * FROM @Entries;
END
