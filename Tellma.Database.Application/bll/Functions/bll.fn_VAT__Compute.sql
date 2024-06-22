CREATE FUNCTION [bll].[fn_VAT__Compute] (
	@AmountBeforeVAT DECIMAL (19, 6),
	@VAT			 DECIMAL (19, 6),
	@VATRate		 DECIMAL (19, 6)
) RETURNS DECIMAL (19, 6) AS
BEGIN
	RETURN CASE
		WHEN @VAT = 0 OR @AmountBeforeVAT = 0 THEN 0
		WHEN @VAT IS NULL THEN	ROUND(@AmountBeforeVAT * @VATRate, 2)
		WHEN ABS(@VAT - @AmountBeforeVAT * @VATRate) > 0.99
			OR ABS(@VAT - @AmountBeforeVAT * @VATRate) > 0.01 * @AmountBeforeVAT
			THEN ROUND(@AmountBeforeVAT * @VATRate, 2)
		ELSE @VAT
	END
END
GO
