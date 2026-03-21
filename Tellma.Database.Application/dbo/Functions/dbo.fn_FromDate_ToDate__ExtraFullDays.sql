CREATE FUNCTION [dbo].[fn_FromDate_ToDate__ExtraFullDays]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @FullYears INT, @ExtraFullMonths INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @FullYears = [dbo].[fn_FromDate_ToDate__FullYears](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
		SET @ExtraFullMonths = [dbo].[fn_FromDate_ToDate__FullMonths](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(MONTH, @ExtraFullMonths, @FromDate);
	END
	ELSE IF @Calendar = N'ET'
	BEGIN
		SET @FullYears = [dbo].[fn_FromDate_ToDate__FullYears](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
		SET @ExtraFullMonths = [dbo].[fn_FromDate_ToDate__FullMonths](@Calendar, @FromDate, @ToDate);
		
		-- Calculate days to advance
		DECLARE @DaysToAdvance INT = @ExtraFullMonths * 30;
		
		-- Check if advancement crosses Pagume (from month <= 12 to next year)
		DECLARE @FromMonth INT = dbo.fn_Ethiopian_DatePart('m', @FromDate);
		DECLARE @FromYear INT = dbo.fn_Ethiopian_DatePart('y', @FromDate);
		
		IF @FromMonth <= 12 AND @DaysToAdvance > 0
		BEGIN
			-- Days remaining until end of current ET year (including Pagume)
			DECLARE @FromDay INT = dbo.fn_Ethiopian_DatePart('d', @FromDate);
			DECLARE @PagumeLength INT = CASE WHEN @FromYear % 4 = 3 THEN 6 ELSE 5 END;
			DECLARE @DaysUntilYearEnd INT = (12 - @FromMonth) * 30 + (30 - @FromDay + 1) + @PagumeLength;
			
			-- If we're crossing into next year, add Pagume days
			IF @DaysToAdvance >= @DaysUntilYearEnd - @PagumeLength
				SET @DaysToAdvance = @DaysToAdvance + @PagumeLength;
		END
		
		SET @FromDate = DATEADD(DAY, @DaysToAdvance, @FromDate);
	END
	RETURN [dbo].[fn_FromDate_ToDate__FullDays](@Calendar, @FromDate, @ToDate);
END
GO