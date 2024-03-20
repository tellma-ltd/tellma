CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_LB]
(
	@FromDate DATE,
	@ToDate DATE,
	@YearlyAccrual INT = 15
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
	DECLARE @FullYears INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
	SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
	DECLARE @FullMonths INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
	SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
	DECLARE @FullDays INT =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 
	RETURN 
		@YearlyAccrual * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0);
END
GO