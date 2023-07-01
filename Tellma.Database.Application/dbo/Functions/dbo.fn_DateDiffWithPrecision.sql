CREATE FUNCTION [dbo].[fn_DateDiffWithPrecision](
-- This version calculates the elapsed years, then months from the start date.
	@UnitId INT,
	@From DATE,
	@To DATE
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @Result DECIMAL (19, 6)
	DECLARE @DayUnit INT = dal.fn_UnitCode__Id(N'd');
	DECLARE @WeekUnit INT = dal.fn_UnitCode__Id(N'wk');
	DECLARE @MonthUnit INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @YearUnit INT = dal.fn_UnitCode__Id(N'yr');
	DECLARE @Calendar NCHAR (2) = N'GC';
--	DECLARE @UnitId INT = @YearUnit, @From DATE = '20220313', @To DATE = '20221231';

	IF @UnitId = @DayUnit
		RETURN DATEDIFF(DAY, @From, DATEADD(DAY, 1, @To));
	
	IF @UnitId = @WeekUnit
		RETURN DATEDIFF(DAY, @From, DATEADD(DAY, 1, @To)) / 7.0

	DECLARE @FromDate DATE = @From, @ToDate DATE = @To;
	DECLARE @YearsIncluded INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
	SET @FromDate = DATEADD(YEAR, @YearsIncluded, @FromDate);
	DECLARE @MonthsIncluded INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
	SET @FromDate = DATEADD(MONTH, @MonthsIncluded, @FromDate);
	DECLARE @RemainingDays INT =  dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate); 
	
	SET @Result = @YearsIncluded * 360 + @MonthsIncluded * 30 + @RemainingDays;
	RETURN CASE 
		WHEN @UnitId = @MonthUnit THEN @Result / 30.0
		WHEN @UnitId = @YearUnit THEN @Result / 360.0
	END
END