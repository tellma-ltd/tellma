CREATE FUNCTION [bll].[fn_Employee_EOS__Provision_SA]
(
	@EmployeeIds IdList READONLY,
	@AsOfDate DATE
)
RETURNS @Result TABLE (
	[EmployeeId] INT,
	[BasicCurrencyId] NCHAR (3),
	[FromDate]	DATE,
	[ToDate] DATE,
	[ServiceDaysLost] INT,
	[Provision] DECIMAL (19, 6),
	[Years] INT,
	[Months] INT,
	[Days] INT,
	[Salary] DECIMAL (19, 6), -- Gross
	[CenterId] INT,
	[AgentId] INT,
	[NotedResourceId] INT,
	[EntryTypeId] INT
)
AS
BEGIN
	DECLARE
	@EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee'),
	@BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary'),
	@NotedAbsenceDays INT = 20; -- Number of absence days which disrupt service continuity
	
	INSERT INTO @Result([EmployeeId], [FromDate], [ToDate], [ServiceDaysLost], [Provision])
	SELECT [Id], [FromDate], @AsOfDate, 0, 0
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD
	AND [IsActive] = 1
	AND (NOT EXISTS (SELECT * FROM @EmployeeIds) OR [Id] IN (SELECT [Id] FROM @EmployeeIds));

	UPDATE R
	SET 
		R.[BasicCurrencyId] = S.[CurrencyId0],
		R.[CenterId] = S.[CenterId0],
		R.[AgentId] = S.[AgentId0],
		R.[NotedResourceId] = S.[NotedResourceId0],
		R.[EntryTypeId] = S.[EntryTypeId0]
	FROM @Result R
	CROSS APPLY [bll].[ft_Employees_Period_EventFromModel_Salaries__Generate](@AsOfDate, @AsOfDate, @BasicSalaryRS, @EmployeeIds) S;

	-- Service discontinuity is noted if unpaid or breach for 20 days or more (got it from SA law)
	WITH ServiceDaysLost AS (
		SELECT E.[AgentId], 
			SUM(E.[Direction] * (DATEDIFF(DAY, E.[Time1], IIF(E.[Time2] <= @AsOfDate, E.[Time2], @AsOfDate)) + 1)) AS Quantity
		FROM dbo.Entries E
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		JOIN dbo.ResourceDefinitions RD ON RD.[Id] = R.[DefinitionId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.[State] = 4
		AND RD.[Code] = N'LeaveTypes'
		AND R.[Code] IN (N'Breach', N'UnpaidLeave')
		AND AC.[Concept] = N'HRExtension'
		AND E.[Time1] <= @AsOfDate
		AND DATEDIFF(DAY, E.[Time1], IIF(E.[Time2] <= @AsOfDate, E.[Time2], @AsOfDate)) + 1 >= @NotedAbsenceDays
		GROUP BY E.[AgentId]
	)
	UPDATE R
	SET
		R.[ServiceDaysLost] = SDL.Quantity,
		R.[FromDate] = DATEADD(DAY, SDL.[Quantity], R.FromDate)
	FROM @Result R
	JOIN ServiceDaysLost SDL ON SDL.AgentId = R.[EmployeeId];

	DECLARE	@Calendar NCHAR (2) = dal.fn_Settings__Calendar();
	
	UPDATE @Result
	SET
		Years = dbo.fn_FromDate_ToDate__FullYears(@Calendar, FromDate, ToDate), 
		Months = dbo.fn_FromDate_ToDate__ExtraFullMonths(@Calendar, FromDate, ToDate), 
		[Days] = dbo.fn_FromDate_ToDate__ExtraFullDays(@Calendar, FromDate, ToDate);
	
	-- Get the gross salary		
	DECLARE	@Monthly INT = dal.fn_UnitCode__Id(N'mo');

	WITH GrossSalaries AS (
		SELECT E.[NotedAgentId], SUM(
			bll.fn_ConvertCurrencies(@AsOfDate, 
				E.[CurrencyId], T.[BasicCurrencyId], E.[Direction] * E.[MonetaryValue])) AS [Amount]
		FROM dbo.Entries E
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.LineDefinitions LD ON LD.[Id] = L.[DefinitionId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		JOIN @Result T ON T.[EmployeeId] = E.[NotedAgentId]
		WHERE E.[Time1] <= @AsOfDate
		AND (E.[Time2] IS NULL OR E.[Time2] >= @AsOfDate)
		AND L.[State] = 2
		AND LD.[LineType] = 80
		AND AC.[Concept] = N'WagesAndSalaries'
		AND E.[DurationUnitId] = @Monthly AND R.[UnitId] = @Monthly
		GROUP BY E.[NotedAgentId]
		HAVING  SUM(E.[Direction] * E.[MonetaryValue]) <> 0
	)
	UPDATE R
	SET
		R.[Salary] = GS.[Amount]
	FROM @Result R
	JOIN GrossSalaries GS ON GS.[NotedAgentId] = R.[EmployeeId];

	UPDATE @Result
	SET	[Provision] = CASE
			WHEN Years >= 5 THEN
				0.5 * Salary * 5 + Salary * (Years - 5 + Months / 12.0 + [Days] / 360.0)
			ELSE
				0.5 * Salary * (Years + Months / 12.0 + [Days] / 360.0)
			END;

	UPDATE @Result SET [Provision] = ROUND([Provision], 2);

	RETURN
END
GO