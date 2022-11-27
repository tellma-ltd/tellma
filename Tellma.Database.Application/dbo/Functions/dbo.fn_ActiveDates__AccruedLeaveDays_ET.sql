CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_ET]
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
	IIF (@FullYears > = 1,
		(16 + (@FullYears - 1) / 2) * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0),
		16 *  (@FullMonths / 12.0 + @FullDays / 360.0)
		);
END
GO