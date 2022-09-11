CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullYears]
(
	@Calendar NCHAR (2),-- = N'GC',
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS INT
AS
BEGIN
	DECLARE @Years INT;
	IF @Calendar = N'GC'
	BEGIN
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
	END
	RETURN @Years;
END
GO