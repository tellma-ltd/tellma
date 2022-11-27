CREATE FUNCTION bll.[fn_EmployeeIncomeTax_LB](@MonetaryAmount DECIMAL (19, 4))
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN
		ROUND(
		CASE
			WHEN @MonetaryAmount <= 6000000		THEN 0.02 -- 120,000
			WHEN @MonetaryAmount <= 15000000	THEN 0.04 * @MonetaryAmount - 120000 -- 480,000
			WHEN @MonetaryAmount <= 30000000	THEN 0.07 * @MonetaryAmount - 570000 -- 1,530,000
			WHEN @MonetaryAmount <= 60000000	THEN 0.11 * @MonetaryAmount - 1770000 -- 4,830,000
			WHEN @MonetaryAmount <= 120000000	THEN 0.15 * @MonetaryAmount - 4170000 -- 13,830,000
			WHEN @MonetaryAmount <= 225000000	THEN 0.20 * @MonetaryAmount - 10170000 -- 34,830,000
			ELSE 0.25 * @MonetaryAmount - 21420000
		END, 2)
END