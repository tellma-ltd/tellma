CREATE FUNCTION [dbo].[fn_FromDate_ToDate__WorkdaysText]
(
	@Calendar NCHAR (2),
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31'
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @Years INT, @Months INT, @Days INT, @YDate DATE, @MDate DATE;
	SET @Years =  dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate)
	SET @FromDate = DATEADD(YEAR, @Years, @FromDate);
	SET @Months = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate);
	SET @FromDate = DATEADD(MONTH, @Months, @FromDate);
	SET @Days = dbo.fn_FromDate_ToDate__FullDays(@Calendar, @FromDate, @ToDate);
	
	RETURN CAST(@Years AS NVARCHAR (50)) + N'Y-' + CAST(@Months AS NVARCHAR (50)) + N'M-' + CAST(@Days AS NVARCHAR (50)) + N'D';
END