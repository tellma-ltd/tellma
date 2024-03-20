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
	DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();

	IF @UnitId = @DayUnit
		RETURN DATEDIFF(DAY, @From, DATEADD(DAY, 1, @To));
	
	IF @UnitId = @WeekUnit
		RETURN DATEDIFF(DAY, @From, DATEADD(DAY, 1, @To)) / 7.0

	DECLARE @RemainingDays DECIMAL (19, 6) = 0;
	DECLARE @MonthsIncluded INT = 0;
	IF @Calendar = 'GC'
	BEGIN
		IF (DAY(@From) = 1 AND @To = EOMONTH(@To))
			SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To) + 1;
		ELSE IF DAY(@From) = 1 AND @To < EOMONTH(@To)
		BEGIN
			SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To);
			SET @RemainingDays = 1.0 * DAY(@To) / DAY(EOMONTH(@To))
		END
		ELSE IF DAY(@From) > 1 AND @To = EOMONTH(@To)
		BEGIN
			SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To);
			SET @RemainingDays = (DATEDIFF(DAY, @From, EOMONTH(@From)) + 1.0) / DAY(EOMONTH(@From));
		END
		ELSE IF DAY(@From) > 1 AND @To < EOMONTH(@To)
		BEGIN
			SET @MonthsIncluded = DATEDIFF(MONTH, @From, @To) - 1;
			SET @RemainingDays = DAY(@To) * 1.0/DAY(EOMONTH(@To)) + (DATEDIFF(DAY, @From, EOMONTH(@From)) + 1.0) / DAY(EOMONTH(@From));
		END
		SET @Result = @MonthsIncluded + @RemainingDays;
		RETURN 
		CASE 
			WHEN @UnitId = @MonthUnit THEN @Result
			WHEN @UnitId = @YearUnit THEN @Result / 12.0
		END
	END
	ELSE IF @Calendar = 'ET'
	BEGIN
		DECLARE @TotalDays INT = DATEDIFF(DAY, @From, @To) + 1;
		DECLARE @TotalPagumeDays INT = 0;
		DECLARE @PagumeMonths TABLE (FromDate DATE, ToDate DATE);
		DECLARE @ET_FirstYear INT = dbo.[fn_Ethiopian_DatePart]('yr', @From)
		DECLARE @ET_LastYear INT = dbo.[fn_Ethiopian_DatePart]('yr', @To)
		DECLARE @Yr INT = @ET_FirstYear;
		WHILE @Yr <= @ET_LastYear
		BEGIN
			INSERT INTO @PagumeMonths(FromDate, ToDate)
			SELECT
				[dbo].[fn_Ethiopian_DateFromParts](@Yr, 13, 1) AS FromDate,
				DATEADD(DAY, -1, [dbo].[fn_Ethiopian_DateFromParts](@Yr + 1, 1, 1)) AS ToDate
			SET @Yr = @Yr + 1
		END
		DELETE FROM @PagumeMonths WHERE [FromDate] > @To OR [ToDate] < @From;
		UPDATE @PagumeMonths SET FromDate = @From WHERE @From BETWEEN FromDate AND ToDate;
		UPDATE @PagumeMonths SET ToDate = @To WHERE @To BETWEEN FromDate AND ToDate;
		SET @TotalPagumeDays = ISNULL((SELECT SUM(DATEDIFF(DAY, [FromDate], [ToDate]) + 1) FROM @PagumeMonths), 0);

		SET @MonthsIncluded = (@TotalDays - @TotalPagumeDays) / 30;
		SET @RemainingDays = ((@TotalDays - @TotalPagumeDays) % 30) / 30.0;
		SET @Result = @MonthsIncluded + @RemainingDays;
		RETURN 
		CASE 
			WHEN @UnitId = @MonthUnit THEN @Result
			WHEN @UnitId = @YearUnit THEN @Result / 12.0
		END
	END
	RETURN 0;
END
GO