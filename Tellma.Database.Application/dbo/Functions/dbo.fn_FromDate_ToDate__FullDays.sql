CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullDays]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @FullDays INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		SET @FullDays = DATEDIFF(DAY, @FromDate, @ToDate);
	END
	ELSE IF @Calendar = N'ET'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		
		DECLARE @FromMonth INT = dbo.fn_Ethiopian_DatePart('m', @FromDate);
		DECLARE @TotalDays INT = DATEDIFF(DAY, @FromDate, @ToDate);
		
		IF @FromMonth = 13
		BEGIN
			-- Skip remaining Pagume days: advance to Meskerem 1
			DECLARE @FromDay INT = dbo.fn_Ethiopian_DatePart('d', @FromDate);
			DECLARE @FromYear INT = dbo.fn_Ethiopian_DatePart('y', @FromDate);
			-- Pagume has 6 days in leap year (year % 4 = 3), otherwise 5
			DECLARE @PagumeLength INT = CASE WHEN @FromYear % 4 = 3 THEN 6 ELSE 5 END;
			DECLARE @PagumeDaysToSkip INT = @PagumeLength - @FromDay + 1;
			
			SET @TotalDays = @TotalDays - @PagumeDaysToSkip;
			
			IF @TotalDays < 0
				SET @FullDays = 0;
			ELSE
				SET @FullDays = @TotalDays % 30;
		END
		ELSE
			SET @FullDays = @TotalDays % 30;
	END
	RETURN @FullDays;
END
GO