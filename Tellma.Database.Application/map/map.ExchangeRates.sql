CREATE FUNCTION [map].[ExchangeRates]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM dbo.ExchangeRates
);