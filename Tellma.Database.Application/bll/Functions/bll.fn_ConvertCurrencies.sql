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
	IF @FromCurrencyId = @ToCurrencyId RETURN @FromAmount;

	DECLARE @FunctionalCurrencyId NCHAR (3) = [dal].fn_FunctionalCurrencyId();
	DECLARE @FunctionalAmount  DECIMAL (19,4), @Result  DECIMAL (19,6);

	SET @FunctionalAmount= IIF(@FromCurrencyId = @FunctionalCurrencyId, 
			@FromAmount,
			(
				SELECT @FromAmount * [Rate]
				FROM [map].[ExchangeRates]()
				WHERE CurrencyId = @FromCurrencyId
				AND @Date >= ValidAsOf
				AND @Date < ValidTill
			));

	SET @Result = IIF(@ToCurrencyId = @FunctionalCurrencyId, 
			@FunctionalAmount,
			(
				SELECT @FunctionalAmount / [Rate]
				FROM [map].[ExchangeRates]()
				WHERE CurrencyId = @ToCurrencyId
				AND @Date >= ValidAsOf
				AND @Date < ValidTill
			));

	RETURN (SELECT ROUND(@Result, E) FROM dbo.Currencies WHERE [Id] = @ToCurrencyId);
END;