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
	SET @Years =
		CASE 
			WHEN
				DATEPART(YEAR, @FromDate) < DATEPART(YEAR, @ToDate)
			AND (DATEPART(MONTH, @FromDate) < DATEPART(MONTH, @ToDate)
				OR (DATEPART(MONTH, @FromDate) = DATEPART(MONTH, @ToDate) 
					AND	DATEPART(DAY, @FromDate) <= DATEPART(DAY, @ToDate))
				)
				THEN DATEDIFF(YEAR, @FromDate, @ToDate)
            WHEN DATEPART(YEAR, @FromDate) < DATEPART(YEAR, @ToDate)
				THEN DATEDIFF(YEAR, @FromDate, @ToDate) - 1
            WHEN DATEPART(YEAR, @FromDate) = DATEPART(YEAR, @ToDate)
				THEN 0
        END; --print N'Years = ' + cast(@years as nvarchar(50))
	SET @FromDate = DATEADD(YEAR, @Years, @FromDate); --print @fromDate
	SET @Months =
		CASE 
            WHEN DATEPART(DAY, @FromDate) > DATEPART(DAY, @ToDate)
				THEN DATEDIFF(MONTH, @FromDate, @ToDate) - 1
            ELSE DATEDIFF(MONTH, @FromDate, @ToDate)
		END; --print N'Months = ' + cast(@Months as nvarchar(50))
	SET @FromDate = DATEADD(MONTH, @Months, @FromDate); --print @fromDate
	SET @Days = DATEDIFF(DAY, @FromDate, @ToDate); --print N'Days = ' + cast(@Days as nvarchar(50))
	
	RETURN CAST(@Years AS NVARCHAR (50)) + N'Y-' + CAST(@Months AS NVARCHAR (50)) + N'M-' + CAST(@Days AS NVARCHAR (50)) + N'D'
END
GO