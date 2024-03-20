CREATE FUNCTION [bll].[ft_Employees__Overtimes_SA](
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
	DECLARE @HoursInMonth DECIMAL (19, 6) = 240.0, @MonthUnitId INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');

	INSERT INTO @HourlyRates([EmployeeId], [ResourceId], [AsOfDate], [CenterId], [AgentId], [NotedResourceId], [EntryTypeId], [CurrencyId], [HourlyRate])
	SELECT E.[NotedAgentId], ED.[ResourceId], ED.[Date], E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[EntryTypeId], E.[CurrencyId],
		SUM(E.[Direction] * E.[MonetaryValue]) / @HoursInMonth / 2 -- 50% of Basic
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN @EmployeesOvertimesDates ED ON ED.[AgentId] = E.[NotedAgentId]
	WHERE E.[Time1] <= ED.[Date]
	AND (E.[Time2] IS NULL OR E.[Time2] >= ED.[Date])
	AND L.[State] = 2
	AND AC.[Concept] = N'WagesAndSalaries'
	AND E.[ResourceId] = @BasicSalaryRS
	GROUP BY E.[NotedAgentId], ED.[ResourceId], ED.[Date], E.[CenterId], E.[AgentId], E.[NotedResourceId], E.[EntryTypeId], E.[CurrencyId]
	HAVING  SUM(E.[Direction] * E.[MonetaryValue]) <> 0;

	-- Assuming all benefits are the same currency
	WITH GrossSalaries AS (
		SELECT E.[NotedAgentId] AS [EmployeeId], ED.[Date] AS [AsOfDate], E.[CurrencyId],
			SUM(E.[Direction] * E.[MonetaryValue]) / @HoursInMonth AS GrossComponent -- 100% of Gross
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN @EmployeesOvertimesDates ED ON ED.[AgentId] = E.[NotedAgentId]
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
		WHERE E.[Time1] <= ED.[Date]
		AND (E.[Time2] IS NULL OR E.[Time2] >= ED.[Date])
		AND L.[State] = 2
		AND AC.[Concept] = N'WagesAndSalaries'
		AND RD.[Code] = N'EmployeeBenefits'
		AND R.[UnitId] = @MonthUnitId
		GROUP BY E.[NotedAgentId], ED.[Date], E.[CurrencyId]
		HAVING  SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	)
	UPDATE HR
	SET HR.[HourlyRate] = HR.HourlyRate + GS.[GrossComponent]
	FROM @HourlyRates HR
	JOIN GrossSalaries GS ON HR.[EmployeeId] = GS.[EmployeeId] AND HR.[AsOfDate] = GS.[AsOfDate]

	RETURN

END
GO
