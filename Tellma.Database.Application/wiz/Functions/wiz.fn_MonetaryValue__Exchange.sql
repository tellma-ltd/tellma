CREATE FUNCTION [wiz].[fn_ConvertToFunctional]
(
	@Date DATE,
	@CurrencyId NCHAR (3),
	@Amount DECIMAL (19,4)
)
RETURNS DECIMAL (19,4)
AS
BEGIN
	DECLARE @E INT;
	SELECT @E = E FROM dbo.Currencies WHERE [Id] = dbo.fn_FunctionalCurrencyId();
	RETURN (
		SELECT ROUND(@Amount * [Rate], @E)
		FROM [map].[ExchangeRates]()
		WHERE CurrencyId = @CurrencyId
		AND @Date >= ValidAsOf
		AND @Date < ValidTill
	)
END;