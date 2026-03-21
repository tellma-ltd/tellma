CREATE FUNCTION [dbo].[fn_Employee_Date__IsWeekendOrHoliday](
-- Not used. Though generally correct. A tabular function exists
	@EmployeeId INT,
	@Date DATE
) RETURNS BIT
AS BEGIN
	DECLARE @IsHoliday BIT =  dbo.fn_IsWeekendOrHoliday(@Date) ;
	DECLARE @Weekday TINYINT =  DATEPART(WEEKDAY, @Date);

	DECLARE @DateBit NCHAR(1);
	-- Using Lookup7 to define the work days, starting Sunday:1 to Satuday:7.
	-- So, working from Mon - Fri is 0111110
	SELECT @DateBit = SUBSTRING([dal].[fn_Lookup__Code] (Lookup7Id), @Weekday, 1)
	FROM dbo.Agents
	WHERE Id = @EmployeeId;

	RETURN CASE
		WHEN @IsHoliday = 1 THEN 0
		WHEN @DateBit = '0' THEN 0
		ELSE 1
	END
END