CREATE FUNCTION [bll].[ft_Employees__Overtimes](
	@EmployeesOvertimesDates AgentIdResourceIdDateList READONLY
)
RETURNS @HourlyRates TABLE (
	[EmployeeId]		INT,
	[ResourceId]		INT,
	[AsOfDate]			DATE,
	[CenterId]			INT,
	[AgentId]			INT,
	[NotedResourceId]	INT,
	[EntryTypeId]		INT,
	[CurrencyId]		NCHAR (3),
	[HourlyRate]		DECIMAL (19, 6),
	PRIMARY KEY ([EmployeeId], [ResourceId], [AsOfDate])
)
AS BEGIN
	DECLARE @Country NCHAR (2) = dal.fn_Settings__Country();
	IF @Country = N'ET'
		INSERT INTO @HourlyRates
		SELECT * FROM bll.ft_Employees__Overtimes_ET(@EmployeesOvertimesDates);
	ELSE IF @Country = N'SA'
		INSERT INTO @HourlyRates
		SELECT * FROM bll.ft_Employees__Overtimes_SA(@EmployeesOvertimesDates);
	
	RETURN
END