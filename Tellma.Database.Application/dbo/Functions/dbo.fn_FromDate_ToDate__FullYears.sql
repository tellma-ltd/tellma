CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullYears]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @FullYears INT;
	IF @Calendar IN (N'GC', N'ET')  -- Changed here
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		SET @FullYears =
			CASE 
				WHEN DATEPART(YEAR, @FromDate) < DATEPART(YEAR, @ToDate)
				AND (DATEPART(MONTH, @FromDate) < DATEPART(MONTH, @ToDate)
					OR (DATEPART(MONTH, @FromDate) = DATEPART(MONTH, @ToDate) 
						AND DATEPART(DAY, @FromDate) <= DATEPART(DAY, @ToDate)))
					THEN DATEDIFF(YEAR, @FromDate, @ToDate)
				WHEN DATEPART(YEAR, @FromDate) < DATEPART(YEAR, @ToDate)
					THEN DATEDIFF(YEAR, @FromDate, @ToDate) - 1
				WHEN DATEPART(YEAR, @FromDate) = DATEPART(YEAR, @ToDate)
					THEN 0
			END;
	END
	RETURN @FullYears;
END
GO