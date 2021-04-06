CREATE FUNCTION bll.[fn_EmployeeIncomeTax_ET](@MonetaryAmount DECIMAL (19, 4))
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN
		ROUND(
		CASE
			WHEN @MonetaryAmount <= 600		THEN 0
			WHEN @MonetaryAmount <= 1650	THEN 0.1 * @MonetaryAmount - 60
			WHEN @MonetaryAmount <= 3200	THEN 0.15 * @MonetaryAmount - 142.5
			WHEN @MonetaryAmount <= 5250	THEN 0.2 * @MonetaryAmount - 302.5
			WHEN @MonetaryAmount <= 7800	THEN 0.25 * @MonetaryAmount - 565
			WHEN @MonetaryAmount <= 10900	THEN 0.3 * @MonetaryAmount - 955
			ELSE 0.35 * @MonetaryAmount - 1500
		END, 2)
END