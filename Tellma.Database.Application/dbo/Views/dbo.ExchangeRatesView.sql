CREATE VIEW [dbo].[ExchangeRatesView]
AS
SELECT 
	[Id],
	[CurrencyId],
	[ValidAsOf],
	(
		SELECT ISNULL(MIN([ValidAsOf]), DATEADD(DAY, 1, GETDATE()))
		FROM dbo.ExchangeRates
		WHERE [CurrencyId] = ER.[CurrencyId] AND [ValidAsOf] > ER.[ValidAsOf]
	) AS [ValidTill],
	[Rate]
FROM dbo.ExchangeRates ER;