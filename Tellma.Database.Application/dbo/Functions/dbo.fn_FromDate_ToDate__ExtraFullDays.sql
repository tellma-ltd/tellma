CREATE FUNCTION [dbo].[fn_FromDate_ToDate__ExtraFullDays]
(
	@Calendar NCHAR (2),-- = N'GC',
	@FromDate DATE,-- = N'2011-01-01',
	@ToDate DATE -- = N'2020-12-31',
)
RETURNS INT
AS
BEGIN
	DECLARE @FullYears INT, @ExtraFullMonths INT, @FullDays INT;
	IF @Calendar = N'GC'
	BEGIN
		SET @FullYears= [dbo].[fn_FromDate_ToDate__FullYears](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
		SET @ExtraFullMonths = [dbo].[fn_FromDate_ToDate__ExtraFullMonths](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(MONTH, @ExtraFullMonths, @FromDate);
	END
	RETURN  [dbo].[fn_FromDate_ToDate__FullDays](@Calendar, @FromDate, @ToDate);
END
GO