CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullMonths]
(
	@Calendar NCHAR (2),-- = N'GC',
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS INT
AS
BEGIN
	DECLARE @FullYears INT, @FullMonths INT;
	IF @Calendar = N'GC'
	BEGIN
		--SET @FullYears= [dbo].[fn_FromDate_ToDate__FullYears](@Calendar, @FromDate, @ToDate);
		--SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		SET @FullMonths =
			CASE 
				WHEN DATEPART(DAY, @FromDate) > DATEPART(DAY, @ToDate)
					THEN DATEDIFF(MONTH, @FromDate, @ToDate) - 1
				ELSE DATEDIFF(MONTH, @FromDate, @ToDate)
			END;
	END
	RETURN @FullMonths;
END
GO