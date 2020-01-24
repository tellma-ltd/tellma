CREATE FUNCTION [bll].[fn_EmployeeIncomeTax]
(
	@EmployeeId INT,
	@TaxableIncome DECIMAL (19,4)
)
RETURNS INT
AS
BEGIN
	RETURN (
		CASE 
			WHEN @TaxableIncome < 350 THEN 0 
			WHEN @TaxableIncome >= 350 AND @TaxableIncome < 700 THEN 0.2 * @TaxableIncome
			ELSE @TaxableIncome * 0.3
		END
		)
END;