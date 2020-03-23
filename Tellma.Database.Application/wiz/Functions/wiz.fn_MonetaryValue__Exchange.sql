CREATE FUNCTION [wiz].[fn_MonetaryValue__Exchange]
(
	@PostingDate DATE,
	@CurrencyId NCHAR (3),
	@MonetaryValue DECIMAL (19,4)
)
RETURNS DECIMAL (19,4)
AS
BEGIN RETURN (
			SELECT @MonetaryValue * [Rate]
			FROM [map].[ExchangeRates]
			WHERE CurrencyId = @CurrencyId
			AND @PostingDate >= ValidAsOf
			AND @PostingDate < ValidTill
			)
END;