CREATE FUNCTION [dbo].[fn_DateFromParts]
(
	@CalendarYear INT,
	@CalendarMonth INT,
	@CalendarDay INT
)
RETURNS DATETIME
AS BEGIN
DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
RETURN
	CASE		
		WHEN @Calendar = 'ET' THEN dbo.fn_Ethiopian_DateFromParts(@CalendarYear, @CalendarMonth, @CalendarDay)
		WHEN @Calendar = 'UQ' THEN dbo.fn_UmAlQura_DateFromParts(@CalendarYear, @CalendarMonth, @CalendarDay)
		WHEN @Calendar = 'GC' THEN DATEFROMPARTS(@CalendarYear, @CalendarMonth, @CalendarDay)
	END
END
GO