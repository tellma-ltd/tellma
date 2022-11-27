CREATE FUNCTION [dbo].[fn_ActiveDates__AccruedLeaveDays]
(
	@CountryId NCHAR (2),
	@Calendar NCHAR (2),
	@FromDate DATE,
	@ToDate DATE
)
RETURNS DECIMAL (19, 6)
AS
BEGIN
	RETURN
	CASE
		WHEN @CountryId = N'ET' THEN dbo.fn_ActiveDates__AccruedLeaveDays_ET(@Calendar, @FromDate, @ToDate)
		WHEN @CountryId = N'LB' THEN dbo.fn_ActiveDates__AccruedLeaveDays_LB(@Calendar, @FromDate, @ToDate)
		WHEN @CountryId = N'SA' THEN dbo.fn_ActiveDates__AccruedLeaveDays_SA(@Calendar, @FromDate, @ToDate)
		WHEN @CountryId = N'SD' THEN dbo.fn_ActiveDates__AccruedLeaveDays_SA(@Calendar, @FromDate, @ToDate)
	END
END
GO