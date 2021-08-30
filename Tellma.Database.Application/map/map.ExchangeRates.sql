CREATE FUNCTION [map].[ExchangeRates]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[CurrencyId],
		[ValidAsOf],
		[ValidTill],
		[AmountInCurrency],
		[AmountInFunctional],
		[Rate],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM dbo.ExchangeRates
);