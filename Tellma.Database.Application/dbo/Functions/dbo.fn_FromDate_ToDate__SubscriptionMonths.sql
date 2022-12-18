CREATE FUNCTION [dbo].[fn_FromDate_ToDate__SubscriptionMonths]
(
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS   DECIMAL (19, 6)
AS
BEGIN
	DECLARE @month INT = dal.fn_UnitCode__Id(N'mo')

	DECLARE @FullDays INT = 0, @FullMonths INT = 0, @FullYears INT = 0;
	IF @FromDate <> dbo.fn_PeriodStart(@month, @FromDate)
	BEGIN
		SET @FullDays = @FullDays + DATEDIFF(DAY, @FromDate, dbo.fn_PeriodEnd(@month, @FromDate)) + 1;
		SET @FromDate = DATEADD(DAY, 1, dbo.fn_PeriodEnd(@month, @FromDate))
	END
	IF @ToDate <> dbo.fn_PeriodEnd(@month, @ToDate)
	BEGIN
		SET @FullDays = @FullDays + DATEDIFF(DAY, dbo.fn_PeriodStart(@month, @ToDate), @ToDate) + 1;
		SET @ToDate = DATEADD(DAY, -1, dbo.fn_PeriodStart(@month, @ToDate))
	END
	IF @FromDate < @ToDate
	BEGIN
		SET @FullYears = dbo.fn_FromDate_ToDate__FullYears(@Calendar, @FromDate, @ToDate);   
		SET @FromDate = DATEADD(YEAR, @FullYears, @FromDate);
		SET @FullMonths = dbo.fn_FromDate_ToDate__FullMonths(@Calendar, @FromDate, @ToDate); 
	END
	RETURN --@FullYears * 10000 + @FullMonths* 100 + @FullDays--	
		(@FullYears * 12.0 + @FullMonths + @FullDays / 28.0)
END
GO