CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullMonths]
(
	@Calendar NCHAR (2),-- = N'GC',
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS INT
AS
BEGIN
	DECLARE @Months INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		DECLARE @Years INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate)
		SET @FromDate = DATEADD(YEAR, @Years, @FromDate); --print @fromDate
		SET @Months =
			CASE 
				WHEN DATEPART(DAY, @FromDate) > DATEPART(DAY, @ToDate)
					THEN DATEDIFF(MONTH, @FromDate, @ToDate) - 1
				ELSE DATEDIFF(MONTH, @FromDate, @ToDate)
			END; --print N'Months = ' + cast(@Months as nvarchar(50))
	END
	RETURN @Months;
END
GO