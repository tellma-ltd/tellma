CREATE FUNCTION [dbo].[fn_Time1_Time2__VacationDays](
@FromDate DATE,
@ToDate DATE
) RETURNS INT
AS BEGIN
	DECLARE @Result INT;
	
	SELECT @Result = COUNT(*)
	FROM dbo.Lookups
	WHERE DefinitionId = dal.fn_LookupDefinitionCode__Id(N'CalendarDay')
	AND [IsActive] = 0
	AND CAST([Code] AS DATE) BETWEEN @FromDate AND @ToDate;

	RETURN @Result;
END
GO