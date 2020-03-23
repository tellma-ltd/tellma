CREATE VIEW [map].[ExchangeRates]
AS
SELECT 
	ER.*,
	(
		SELECT ISNULL(MIN([ValidAsOf]), DATEADD(DAY, 1, GETDATE()))
		FROM dbo.ExchangeRates
		WHERE [CurrencyId] = ER.[CurrencyId] AND [ValidAsOf] > ER.[ValidAsOf]
	) AS [ValidTill]
FROM dbo.ExchangeRates ER;