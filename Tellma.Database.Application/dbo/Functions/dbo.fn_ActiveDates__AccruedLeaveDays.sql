CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays]
(
	@FromDate DATE,
	@ToDate DATE,
	@YearlyAccrual INT = 21,
	@InactiveDays INT = 0
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	DECLARE @CountryId NCHAR (2) = dal.fn_Settings__Country();
	SET @FromDate = DATEADD(DAY, @InactiveDays, @FromDate);	
	RETURN
	CASE
		WHEN @CountryId = N'AE' THEN dbo.fn_ActiveDates__AccruedLeaveDays_AE(@FromDate, @ToDate, @YearlyAccrual)
		WHEN @CountryId = N'ET' THEN dbo.fn_ActiveDates__AccruedLeaveDays_ET(@FromDate, @ToDate, 16)
		WHEN @CountryId = N'LB' THEN dbo.fn_ActiveDates__AccruedLeaveDays_LB(@FromDate, @ToDate, 15)
		WHEN @CountryId = N'SA' THEN dbo.fn_ActiveDates__AccruedLeaveDays_SA(@FromDate, @ToDate, @YearlyAccrual)
		WHEN @CountryId = N'SD' THEN dbo.fn_ActiveDates__AccruedLeaveDays_SD(@FromDate, @ToDate, @YearlyAccrual)
	END
END
GO