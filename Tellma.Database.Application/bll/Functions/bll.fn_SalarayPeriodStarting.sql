CREATE FUNCTION [bll].[fn_SalaryPeriodStarting] (@PeriodEnding DATE)
RETURNS DATE
AS
BEGIN
	RETURN (DATEADD(DAY, +1, DATEADD(MONTH, -1, @PeriodEnding)));
END
GO