CREATE FUNCTION [bll].[fn_WorkDays] (
-- This function is not currently used anywhere in Tellma, but it is added for convenience.
-- It excludes Saturdays and Sundays, nbut not holidans
-- TODO: Find a more generic one, taking holidays into consideration.
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