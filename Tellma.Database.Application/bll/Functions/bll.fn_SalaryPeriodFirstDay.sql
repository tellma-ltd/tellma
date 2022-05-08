CREATE FUNCTION bll.fn_SalaryPeriodFirstDay()
RETURNS TINYINT
AS
BEGIN
	RETURN (
		SELECT [FirstDayOfPeriod]
		FROM dbo.Settings
	)
END