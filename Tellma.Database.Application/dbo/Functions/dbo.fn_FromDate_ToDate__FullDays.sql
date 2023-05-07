CREATE FUNCTION [dbo].[fn_FromDate_ToDate__FullDays]
(
	@Calendar NCHAR (2),-- = N'GC',
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS INT
AS
BEGIN
	DECLARE @FullYears INT, @FullMonths INT, @FullDays INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @ToDate = DATEADD(DAY, 1, @ToDate);
		SET @FullDays = DATEDIFF(DAY, @FromDate, @ToDate);
	END
	RETURN @FullDays;
END
GO