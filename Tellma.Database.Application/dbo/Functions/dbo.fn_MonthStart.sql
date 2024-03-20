CREATE FUNCTION [dbo].[fn_MonthStart] (
-- To replace occurrences of fn_PeriodStart...
	@Date DATE
) RETURNS DATE
AS BEGIN
DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
RETURN
	CASE
		WHEN @Calendar = 'ET' THEN dbo.fn_Ethiopian_StartOfMonth(@Date)
		WHEN @Calendar = 'UQ' THEN dbo.fn_UmAlQura_StartOfMonth(@Date)
		ELSE DATEFROMPARTS(YEAR(@Date), MONTH(@Date), 1) -- assume Gregorian Calendar
	END
END
GO