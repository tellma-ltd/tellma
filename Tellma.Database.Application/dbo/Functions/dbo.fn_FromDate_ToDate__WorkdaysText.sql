CREATE FUNCTION [dbo].[fn_FromDate_ToDate__WorkdaysText]
(
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31'
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @Years INT, @Months INT, @Days INT, @YDate DATE, @MDate DATE;
	SET @ToDate = DATEADD(DAY, 1, @ToDate);
	SET @Years = DATEDIFF(YEAR, @FromDate, @ToDate); --print N'Years = ' + cast(@years as nvarchar(50))
	SET @FromDate = DATEADD(YEAR, @Years, @FromDate); --print @fromDate
	SET @Months = DATEDIFF(MONTH, @FromDate, @ToDate); --print N'Months = ' + cast(@Months as nvarchar(50))
	SET @FromDate = DATEADD(MONTH, @Months, @FromDate); --print @fromDate
	SET @Days = DATEDIFF(DAY, @FromDate, @ToDate); --print N'Days = ' + cast(@Days as nvarchar(50))
	
	RETURN CAST(@Years AS NVARCHAR (50)) + N'Y-' + CAST(@Months AS NVARCHAR (50))  + N'M--' + CAST(@Days AS NVARCHAR (50)) + N'D'
END
