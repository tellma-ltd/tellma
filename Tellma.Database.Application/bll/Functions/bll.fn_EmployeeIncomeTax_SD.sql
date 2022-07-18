CREATE FUNCTION bll.[fn_EmployeeIncomeTax_SD](@MonetaryAmount DECIMAL (19, 4))
-- OBSOLETE, REPLACE WITH bll.[fn_PostingDate_Earning_SD__EmployeeIncomeTax]
RETURNS DECIMAL (19, 4)
AS
BEGIN
	RETURN
-- starting 2021 onwards
	ROUND(
		CASE
			WHEN @MonetaryAmount >= 27000	THEN 0.20 * (@MonetaryAmount - 27000) + 3300
			WHEN @MonetaryAmount >= 7000	THEN 0.15 * (@MonetaryAmount - 7000) + 300
			WHEN @MonetaryAmount >= 5000	THEN 0.10 * (@MonetaryAmount - 5000) + 100
			WHEN @MonetaryAmount >= 3000	THEN 0.05 * (@MonetaryAmount - 3000)
			ELSE 0
		END, 2)
END