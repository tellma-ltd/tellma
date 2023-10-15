CREATE FUNCTION [dbo].[fn_IsWeekendOrHoliday](
@Date DATE
) RETURNS BIT
AS BEGIN
	DECLARE @Result BIT;
	SELECT @Result = (1 - [IsActive])
	FROM dbo.Lookups
	WHERE DefinitionId = dal.fn_LookupDefinitionCode__Id(N'CalendarDay')
	AND CAST([Code] AS DATE)  = @Date;

	RETURN @Result;
END