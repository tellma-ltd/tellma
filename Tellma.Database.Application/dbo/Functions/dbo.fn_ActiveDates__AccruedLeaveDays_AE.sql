CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_AE]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
-- These are AE rules
  DECLARE @FullYears INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
  SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
  DECLARE @FullMonths INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
  SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
  DECLARE @FullDays INT =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 
  RETURN 
 -- IIf (
	--@FullYears = 0 and @FullMonths < 6,
	--0,
	--24 * @FullYears + 2 * @FullMonths + @FullDays * 2.0 / 30)
	CASE
		WHEN @FullYears = 0 and @FullMonths < 6 THEN 0
		-- Option 1A
		WHEN @FullYears = 0 and @FullMonths >= 6 THEN 2.0 * (@FullMonths - 6 + @FullDays / 30.0)
		-- Option 1B
		WHEN @FullYears = 0 and @FullMonths >= 6 THEN 2.0 * (@FullMonths + @FullDays / 30.0)
		-- Option 2A
		WHEN @FullYears > 0 THEN 12 + 30 * (@FullYears - 1) + 2.5 * (@FullMonths + @FullDays / 30.0)
		-- Option 2B
		WHEN @FullYears > 0 THEN 30 * @FullYears + 2.5 * (@FullMonths + @FullDays / 30.0)
	END

END
GO