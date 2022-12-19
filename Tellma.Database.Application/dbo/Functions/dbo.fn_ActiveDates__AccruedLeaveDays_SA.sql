CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_SA]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @FullYears INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
	SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
	DECLARE @FullMonths INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
	SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
	DECLARE @FullDays INT =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 
	RETURN
	IIF(
	-- If employee has been with company more than 5 years
		@FullYears > = 5,
	-- he deserves 21 days per year, for the first 5, then 30 for each additional
		30 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0) - 9 * 5,
		21 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0)
	)
END
GO