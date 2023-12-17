CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_V2_SA]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE,
	@YearsInPhase1 INT, -- 5
	@DaysInPhase1 INT, -- 21
	@DaysInPhase2 INT, -- 30,
	@GapDays INT = 0 -- 0
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
		@FullYears > = @YearsInPhase1,
	-- he deserves @DaysInPhase1 days per year, for the first @@YearsInPhase1, then @DaysInPhase2 for each additional
		@DaysInPhase2 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0) - (@DaysInPhase2 - @DaysInPhase1) * @YearsInPhase1,
		@DaysInPhase1 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0)
	)
END
GO