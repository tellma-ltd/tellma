CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullDays]
(
	@Calendar NCHAR (2),-- = N'GC',
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS INT
AS
BEGIN
	DECLARE @Days INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		DECLARE @Years INT = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(YEAR, @Years, @FromDate); --print @fromDate
		DECLARE @Months INT = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(MONTH, @Months, @FromDate); --print @fromDate
--		SET @ToDate = DATEADD(DAY, -1, @ToDate);
		SET @Days = DATEDIFF(DAY, @FromDate, @ToDate); --print N'Days = ' + cast(@Days as nvarchar(50))
	END
	RETURN @Days;
END
GO
