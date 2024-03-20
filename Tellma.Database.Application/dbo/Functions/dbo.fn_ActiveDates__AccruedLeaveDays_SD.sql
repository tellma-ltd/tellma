CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_SD]
(
	@FromDate DATE,
	@ToDate DATE,
	@YearlyAccrual INT = 15
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	RETURN -1 -- To be implemented
END
GO