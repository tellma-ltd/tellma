CREATE FUNCTION [bll].[fn_ConvertCurrencies]
(
	@Date DATE,
	@FromCurrencyId NCHAR (3),
	@ToCurrencyId NCHAR (3),
	@FromAmount DECIMAL (19,4)
)
RETURNS DECIMAL (19,4)
AS
BEGIN
	DECLARE @FunctionalAmount  DECIMAL (19,4), @Result  DECIMAL (19,4);

	IF @FromCurrencyId = @ToCurrencyId
		SET @Result = @FromAmount;
	ELSE BEGIN
		SET @FunctionalAmount = [bll].[fn_ConvertToFunctional](@Date, @FromCurrencyId, @FromAmount);
		SET @Result = [bll].[fn_ConvertFromFunctional](@Date, @ToCurrencyId, @FunctionalAmount);
	END

	RETURN @Result;
END;