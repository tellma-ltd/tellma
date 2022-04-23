CREATE FUNCTION [dbo].[fn_CenterType__ExpenseByFunction] (
	@CenterType NVARCHAR (255)
)
RETURNS NVARCHAR (255)
AS
BEGIN
	RETURN CASE
		WHEN @CenterType = N'Administration' THEN N'AdministrativeExpense'
		WHEN @CenterType = N'Service' THEN N'OtherExpenseByFunction'
		ELSE NULL
	END
END