CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays_ET]
(
	@FromDate DATE,
	@ToDate DATE,
	@YearlyAccrual INT = 16
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
	DECLARE @FullYears INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
	SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
	DECLARE @FullMonths INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
	SET @FromDate = DATEADD(MONTH, @FullMonths, @FromDate);
	DECLARE @FullDays INT = dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 
	
	DECLARE @Accrual INT = ISNULL(@YearlyAccrual, 16);
	
	-- Complete years: sum of (@Accrual + i/2) for i = 1 to @FullYears
	-- = @FullYears * @Accrual + (1/2 + 2/2 + 3/2 + ... + @FullYears/2)
	-- = @FullYears * @Accrual + (@FullYears * @FullYears) / 4
	DECLARE @CompleteYearsAccrual DECIMAL(19, 6) = 
		@FullYears * @Accrual + (@FullYears * @FullYears) / 4;
	
	-- Partial year: rate for year (@FullYears + 1)
	DECLARE @PartialYearRate DECIMAL(19, 6) = @Accrual + (@FullYears + 1) / 2;
	DECLARE @PartialYearAccrual DECIMAL(19, 6) = 
		@PartialYearRate * (@FullMonths / 12.0 + @FullDays / 360.0);
	
	RETURN @CompleteYearsAccrual + @PartialYearAccrual;
END
GO