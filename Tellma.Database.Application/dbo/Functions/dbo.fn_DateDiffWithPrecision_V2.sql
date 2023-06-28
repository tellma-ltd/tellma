CREATE FUNCTION [dbo].[fn_DateDiffWithPrecision_V2](
-- This version calculates total number of calendar months between from and to
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

	IF @UnitId = @DayUnit
		RETURN DATEDIFF(DAY, @From, DATEADD(DAY, 1, @To));
	
	IF @UnitId = @WeekUnit
		RETURN DATEDIFF(DAY, @From, DATEADD(DAY, 1, @To)) / 7.0

	DECLARE @RemainingDays INT = 0;
	DECLARE @MonthsIncluded INT = 0;
	IF DAY(@From) = 1 AND @To = EOMONTH(@To)
		SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To) + 1;
	ELSE IF DAY(@From) = 1 AND @To < EOMONTH(@To)
	BEGIN
		SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To);
		SET @RemainingDays = DAY(@To)
	END
	ELSE IF DAY(@From) > 1 AND @To = EOMONTH(@To)
	BEGIN
		SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To);
		SET @RemainingDays = DATEDIFF(DAY, @From, EOMONTH(@From)) + 1;
	END
	ELSE IF DAY(@From) > 1 AND @To < EOMONTH(@To)
	BEGIN
		SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To) - 1;
		SET @RemainingDays = DAY(@To) + DATEDIFF(DAY, @From, EOMONTH(@From)) + 1;
	END
	SET @Result = @MonthsIncluded * 30 + @RemainingDays;
	RETURN CASE 
		WHEN @UnitId = @MonthUnit THEN @Result / 30.0
		WHEN @UnitId = @YearUnit THEN @Result / 360.0
	END
END
GO