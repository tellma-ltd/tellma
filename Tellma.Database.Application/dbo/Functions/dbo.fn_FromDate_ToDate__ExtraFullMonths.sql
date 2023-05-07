CREATE FUNCTION [dbo].[fn_FromDate_ToDate__ExtraFullMonths]
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
		SET @FullYears= [dbo].[fn_FromDate_ToDate__FullYears](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
--		SET @ToDate = DATEADD(DAY, 1, @ToDate); Commented, MA 2023-04-29
	END
	RETURN [dbo].[fn_FromDate_ToDate__FullMonths](@Calendar, @FromDate, @ToDate)
END
GO