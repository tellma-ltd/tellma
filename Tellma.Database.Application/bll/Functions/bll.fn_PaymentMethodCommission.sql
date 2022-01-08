CREATE FUNCTION [bll].[fn_PaymentMethodCommission] (
	@PaymentMethodId INT,
	@MonetaryValue DECIMAL (19, 4)
)
RETURNS DECIMAL (19, 4)
AS
BEGIN
-- Assumptions: Decimal1: Commission Percentage, Decimal2: Commission Amount
	DECLARE @Result DECIMAL (19, 4);
	SELECT @Result = 
		CASE
			WHEN LK.[Code] = N'HPA' THEN -- Higher Percentage or Amount
				IIF(AG.[Decimal1] * @MonetaryValue / 100 > AG.[Decimal2], AG.[Decimal1] * @MonetaryValue / 100, AG.[Decimal2])
			WHEN LK.[Code] = N'LPA' THEN -- Lower  Percentage or Amount
				IIF(AG.[Decimal1] * @MonetaryValue / 100 < AG.[Decimal2], AG.[Decimal1] * @MonetaryValue / 100, AG.[Decimal2])
			ELSE
				AG.[Decimal1] * @MonetaryValue / 100 
		END
	FROM dbo.Agents AG
	JOIN dbo.Lookups LK ON LK.[Id] = AG.[Lookup1Id]
	WHERE AG.[Id] = @PaymentMethodId

	RETURN ROUND(@Result, 2);
END
GO