CREATE FUNCTION [bll].[fn_WorkDays] (
	@StartDate DATETIME,
	@EndDate DATETIME
)
RETURNS INT
AS
BEGIN
	RETURN (
		SELECT COUNT(*) FROM dbo.Lookups
		WHERE [DefinitionId] = dal.fn_LookupDefinitionCode__Id(N'CalendarDay')
		AND [Code] BETWEEN FORMAT(@StartDate, 'yyyyMMdd') AND FORMAT(@EndDate, 'yyyyMMdd')
		AND [IsActive] = 1
	)
END
GO