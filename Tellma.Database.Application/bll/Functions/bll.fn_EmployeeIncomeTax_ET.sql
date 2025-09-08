CREATE FUNCTION bll.[fn_EmployeeIncomeTax_ET](@MonetaryAmount DECIMAL (19, 4))
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN
		ROUND(
		CASE
			/* Until 2025-07-07
			WHEN @MonetaryAmount <= 600		THEN 0
			WHEN @MonetaryAmount <= 1650	THEN 0.10 * @MonetaryAmount - 60
			WHEN @MonetaryAmount <= 3200	THEN 0.15 * @MonetaryAmount - 142.5
			WHEN @MonetaryAmount <= 5250	THEN 0.20 * @MonetaryAmount - 302.5
			WHEN @MonetaryAmount <= 7800	THEN 0.25 * @MonetaryAmount - 565
			WHEN @MonetaryAmount <= 10900	THEN 0.30 * @MonetaryAmount - 955
			ELSE 0.35 * @MonetaryAmount - 1500
			*/
			-- Starting 2025-07-08
			WHEN @MonetaryAmount <= 2000	THEN 0
			WHEN @MonetaryAmount <= 4000	THEN 0.15 * @MonetaryAmount - 300
			WHEN @MonetaryAmount <= 7000	THEN 0.20 * @MonetaryAmount - 500
			WHEN @MonetaryAmount <= 10000	THEN 0.25 * @MonetaryAmount - 850
			WHEN @MonetaryAmount <= 14000	THEN 0.30 * @MonetaryAmount - 1350
			ELSE 0.35 * @MonetaryAmount - 2050
		END, 2)
END