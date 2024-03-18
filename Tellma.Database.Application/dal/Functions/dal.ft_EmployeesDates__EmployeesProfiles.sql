CREATE FUNCTION dal.ft_EmployeesDates__EmployeesProfiles (
	@EmployeesDates dbo.IdDateList READONLY
)
RETURNS @EmployeeProfiles TABLE
(
	[EmployeeId]		INT,
	[AsOfDate]			DATE,
	[CenterId]			INT,
	[AgentId]			INT,
	[NotedResourceId]	INT,
	[EntryTypeId]		INT,
	[CurrencyId]		NCHAR (3),
	[BasicSalary]		DECIMAL (19, 6),
	PRIMARY KEY ([EmployeeId], [AsOfDate])
)
AS
BEGIN
	DECLARE
		@BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary'),
		@DailyWageRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'DailyWage'),
		@HourlyWageRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'HourlyWage');

	INSERT INTO @EmployeeProfiles([EmployeeId], [AsOfDate],	[CenterId], [AgentId], [NotedResourceId], [EntryTypeId], [CurrencyId], [BasicSalary])
	SELECT E.[NotedAgentId], ED.[Date], E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[EntryTypeId], E.[CurrencyId],
		SUM(E.[Direction] * E.[MonetaryValue]) AS [BasicSalary]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN @EmployeesDates ED ON ED.[Id] = E.[NotedAgentId]
	WHERE E.[Time1] <= ED.[Date]
	AND (E.[Time2] IS NULL OR E.[Time2] >= ED.[Date])
	AND L.[State] = 2
	AND AC.[Concept] = N'WagesAndSalaries'
	AND E.[ResourceId] = @BasicSalaryRS
	GROUP BY E.[NotedAgentId], ED.[Date], E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[EntryTypeId], E.[CurrencyId]
	HAVING  SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	RETURN
END
GO