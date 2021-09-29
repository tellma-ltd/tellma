CREATE FUNCTION [bll].[fn_ConvertToFunctional]
(
	@Date DATE,
	@CurrencyId NCHAR (3),
	@Amount DECIMAL (19,4)
)
RETURNS DECIMAL (19,4)
AS
BEGIN
	DECLARE @E INT, @FunctionalCurrencyId NCHAR (3) = [dal].fn_FunctionalCurrencyId();

	SELECT @E = E FROM dbo.Currencies WHERE [Id] = @FunctionalCurrencyId;
	IF ISNULL(@Amount, 0) = 0 RETURN 0;
	RETURN
		IIF(@CurrencyId = @FunctionalCurrencyId, 
			@Amount,
			(
				SELECT ROUND(@Amount * [Rate], @E)
				FROM [map].[ExchangeRates]()
				WHERE CurrencyId = @CurrencyId
				AND @Date >= ValidAsOf
				AND @Date < ValidTill
			));
END;