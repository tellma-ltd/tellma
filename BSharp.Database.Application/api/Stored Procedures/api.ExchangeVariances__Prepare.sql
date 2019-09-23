CREATE PROCEDURE [api].[ExchangeVariances__Prepare]
@DocumentDate DATE,
@CurrencyId NCHAR(3),
@Rate FLOAT(53)
AS
-- TODO: required testing
BEGIN
	DECLARE	@Documents [dbo].DocumentList, @Lines [dbo].[DocumentLineList], @Entries [dbo].[DocumentLineEntryList];
	DECLARE @SalariesAccrualsTaxable INT, @SalariesAccrualsNonTaxable INT, @EmployeesIncomeTaxPayable INT;
	WITH IfrsAccountClassifications AS
	(
		SELECT [Id] FROM dbo.AccountClassifications -- where current assets, etc...
	),
	ExchangeVarianceAccounts AS 
	(
		SELECT [Id] FROM dbo.Accounts
	),
	ExchangeVarianceEntries AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AgentId], [ResourceId], [AccountId]) AS [Index],
			ROW_NUMBER() OVER (PARTITION BY [AgentId] ORDER BY [ResourceId], [AccountId]) AS [EntryNumber],
		[AccountId], -SUM([Direction] * E.[Value]) AS ValueBalance, SUM([Direction] * E.[MonetaryValue]) AS FXBalance, [ResourceId], [AgentId]
		FROM dbo.DocumentLineEntries E
		JOIN dbo.Resources R ON E.ResourceId = R.Id
		WHERE R.CurrencyId = @CurrencyId
		AND [AccountId] IN (SELECT [Id] FROM ExchangeVarianceAccounts)
		GROUP BY [AccountId], [ResourceId], [AgentId]
		HAVING SUM([Direction] * E.[Value]) * @Rate <> SUM([Direction] * E.[MonetaryValue])
	),
	LineIndices AS (
		SELECT ROW_NUMBER() OVER (ORDER BY [AccountId], [AgentId], [ResourceId]) AS [DocumentLineIndex],[AccountId], [AgentId], [ResourceId]
		FROM ExchangeVarianceEntries
		GROUP BY [AccountId], [AgentId], [ResourceId]
	),
	GainLossEntry AS (
		SELECT [AccountId], [AgentId], [ResourceId], ABS(ValueBalance - FXBalance) AS [Value], SIGN(ValueBalance - FXBalance) AS [Direction]
		FROM ExchangeVarianceEntries
	)
	INSERT INTO @Entries([Index], [DocumentLineIndex], [EntryNumber], [Direction],[AccountId], [Value], [ResourceId], [AgentId])
	SELECT [Index], [DocumentLineIndex], [EntryNumber],
		CAST(SIGN([ValueBalance]) AS SMALLINT) AS [Direction], E.[AccountId], CAST(ABS([ValueBalance]) AS MONEY) AS [ValueBalance], E.[ResourceId], E.[AgentId]
	FROM ExchangeVarianceEntries E 
	JOIN LineIndices L ON E.AgentId = L.AgentId AND E.AccountId = L.AccountId AND E.ResourceId = L.ResourceId

	INSERT INTO @Lines ([Index], [DocumentIndex])
	SELECT	[DocumentLineIndex], 0 AS [DocumentIndex]
	FROM @Entries
	GROUP BY [DocumentLineIndex];

	INSERT INTO @Documents([DocumentDate]) VALUES(@DocumentDate);
	SELECT * FROM @Documents; SELECT *, N'ManualLine' As [LineDefinitionId] FROM @Lines; SELECT * FROM @Entries;
END
