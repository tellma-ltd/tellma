CREATE FUNCTION [bll].[fn_WorkDays]
(
	@StartDate DATETIME,
	@EndDate DATETIME
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT
		   (DATEDIFF(dd, @StartDate, @EndDate) + 1)
		  -(DATEDIFF(wk, @StartDate, @EndDate) * 2)
		  -IIF(DATENAME(dw, @StartDate) = 'Sunday', 1, 0)
		  -IIF(DATENAME(dw, @EndDate) = 'Saturday', 1, 0)
	)
END