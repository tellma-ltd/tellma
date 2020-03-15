CREATE VIEW [dbo].[ExchangeRatesView]
AS
SELECT 
	[Id],
	[CurrencyId],
	[ValidAsOf],
	(
		SELECT ISNULL(MIN([ValidAsOf]), GETDATE())
		FROM dbo.ExchangeRates
		WHERE [CurrencyId] = ER.[CurrencyId] AND [ValidAsOf] > ER.[ValidAsOf]
	) AS [ValidTill],
	[AmountInCurrency],
	[AmountInFunctional]
FROM dbo.ExchangeRates ER;