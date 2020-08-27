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

	DECLARE @E INT, @FunctionalCurrencyId NCHAR (3) = dbo.fn_FunctionalCurrencyId();
	DECLARE @FunctionalAmount  DECIMAL (19,4), @Result  DECIMAL (19,4);
	SELECT @E = E FROM dbo.Currencies WHERE [Id] = @FunctionalCurrencyId;

	SET @FunctionalAmount= IIF(@FromCurrencyId = @FunctionalCurrencyId, 
			@FromAmount,
			(
				SELECT ROUND(@FromAmount * [Rate], @E)
				FROM [map].[ExchangeRates]()
				WHERE CurrencyId = @FromCurrencyId
				AND @Date >= ValidAsOf
				AND @Date < ValidTill
			));

	SET @Result = IIF(@ToCurrencyId = @FunctionalCurrencyId, 
			@FunctionalAmount,
			(
				SELECT ROUND(@FunctionalAmount / [Rate], @E)
				FROM [map].[ExchangeRates]()
				WHERE CurrencyId = @ToCurrencyId
				AND @Date >= ValidAsOf
				AND @Date < ValidTill
			));


	RETURN @Result;
		

END;