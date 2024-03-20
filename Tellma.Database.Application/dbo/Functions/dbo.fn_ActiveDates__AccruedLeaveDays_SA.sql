CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_SA]
(
	@FromDate DATE,
	@ToDate DATE,
	@YearlyAccrual INT = 21
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
	DECLARE @YearsInPhase1 INT = 5, @DaysInPhase2 INT = 30;
	DECLARE @FullYears INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
	SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
	DECLARE @FullMonths INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
	SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
	DECLARE @FullDays INT =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 
	RETURN
	IIF(
	-- If employee has been with company more than 5 years
		@FullYears > = @YearsInPhase1,
	-- he deserves @YearlyAccrual days per year, for the first @YearsInPhase1, then @DaysInPhase2 for each additional
		@DaysInPhase2 * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0) - (@DaysInPhase2 - @YearlyAccrual) * @YearsInPhase1,
		@YearlyAccrual * (@FullYears + @FullMonths / 12.0 + @FullDays / 360.0)
	)
END
GO