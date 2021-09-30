CREATE FUNCTION bll.[fn_EmployeeIncomeTax_SD](@MonetaryAmount DECIMAL (19, 4))
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN
		ROUND(
		CASE
			WHEN @MonetaryAmount <= 3000	THEN 0
			WHEN @MonetaryAmount <= 5000	THEN 0.05 * @MonetaryAmount - 150
			WHEN @MonetaryAmount <= 7000	THEN 0.10 * @MonetaryAmount - 400
			ELSE 0.15 * @MonetaryAmount - 750
		END, 2)
END