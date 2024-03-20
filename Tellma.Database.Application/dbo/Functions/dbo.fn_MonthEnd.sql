CREATE FUNCTION [dbo].[fn_MonthEnd] (
-- To replace occurrences of fn_PeriodEnd...
	@Date DATE
) RETURNS DATE
AS BEGIN
DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
RETURN
	CASE		
		WHEN @Calendar = 'ET'  THEN dbo.fn_EOMONTH_ET(@Date)
		WHEN @Calendar = 'UQ' THEN dbo.fn_UmAlQura_DateAdd('m', 1, dbo.fn_UmAlQura_StartOfMonth(@Date))
		ELSE EOMONTH(@Date)
	END
END
GO