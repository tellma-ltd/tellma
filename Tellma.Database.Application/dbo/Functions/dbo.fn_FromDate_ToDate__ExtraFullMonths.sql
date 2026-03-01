CREATE FUNCTION [dbo].[fn_FromDate_ToDate__ExtraFullMonths]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS INT
AS
BEGIN
	DECLARE @FullYears INT;
	IF @Calendar IN (N'GC', N'ET')
	BEGIN
		SET @FullYears = [dbo].[fn_FromDate_ToDate__FullYears](@Calendar, @FromDate, @ToDate);
		SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
	END
	RETURN [dbo].[fn_FromDate_ToDate__FullMonths](@Calendar, @FromDate, @ToDate)
END
GO