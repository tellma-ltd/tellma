CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullMonths]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @FullMonths INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		SET @FullMonths =
			CASE 
				WHEN DATEPART(DAY, @FromDate) > DATEPART(DAY, @ToDate)
					THEN DATEDIFF(MONTH, @FromDate, @ToDate) - 1
				ELSE DATEDIFF(MONTH, @FromDate, @ToDate)
			END;
	END
	ELSE IF @Calendar = N'ET'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		DECLARE @TotalDays INT = DATEDIFF(DAY, @FromDate, @ToDate);
		
		DECLARE @FromMonth INT = dbo.fn_Ethiopian_DatePart('m', @FromDate);
		
		IF @FromMonth = 13
		BEGIN
			-- Starting in Pagume - skip to Meskerem 1
			DECLARE @FromDay INT = dbo.fn_Ethiopian_DatePart('d', @FromDate);
			DECLARE @FromYear INT = dbo.fn_Ethiopian_DatePart('y', @FromDate);
			DECLARE @PagumeLength INT = CASE WHEN @FromYear % 4 = 3 THEN 6 ELSE 5 END;
			DECLARE @PagumeDaysToSkip INT = @PagumeLength - @FromDay + 1;
			SET @TotalDays = @TotalDays - @PagumeDaysToSkip;
		END
		
		IF @TotalDays < 0
			SET @FullMonths = 0;
		ELSE
			SET @FullMonths = @TotalDays / 30;
	END
	RETURN @FullMonths;
END
GO