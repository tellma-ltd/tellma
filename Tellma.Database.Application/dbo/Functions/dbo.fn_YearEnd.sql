CREATE FUNCTION [dbo].[fn_YearEnd] (
-- To replace occurrences of fn_PeriodEnd...
	@Date DATE
) RETURNS DATE
AS BEGIN
DECLARE @Calendar NCHAR (2) = dal.fn_Settings__Calendar();
RETURN
	CASE		
		WHEN @Calendar = 'ET'  THEN dbo.fn_EOMONTH_12ET(@Date)
		WHEN @Calendar = 'UQ' THEN dbo.fn_UmAlQura_DateAdd('y', 1, dbo.fn_UmAlQura_StartOfMonth(@Date))
		ELSE DATEFROMPARTS(YEAR(@Date), 12, 31)
	END
END
GO