CREATE FUNCTION [bll].[fn_Employee_EOS__Provision_AE]
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
	[Salary] DECIMAL (19, 6), -- Basic
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
	@NotedAbsenceDays INT = 20; -- Number of absence days which disrupt service continuity. This KSA value. Maybe it is the same for UAE
	
	INSERT INTO @Result([EmployeeId], [FromDate], [ToDate], [ServiceDaysLost], [Provision])
	SELECT [Id], [FromDate], @AsOfDate, 0, 0
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD
	AND [IsActive] = 1
	AND (NOT EXISTS (SELECT * FROM @EmployeeIds) OR [Id] IN (SELECT [Id] FROM @EmployeeIds));

	UPDATE R
	SET
		R.[BasicCurrencyId] = S.[CurrencyId0],
		R.[Salary] = S.[MonetaryValue0],
		R.[CenterId] = S.[CenterId0],
		R.[AgentId] = S.[AgentId0],
		R.[NotedResourceId] = S.[NotedResourceId0],
		R.[EntryTypeId] = S.[EntryTypeId0]
	FROM @Result R
	CROSS APPLY [bll].[ft_Employees_Period_EventFromModel_Salaries__Generate](@AsOfDate, @AsOfDate, @BasicSalaryRS, @EmployeeIds) S
	WHERE R.[EmployeeId] = S.[NotedAgentId0];

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
	
	UPDATE @Result
	SET	[Provision] = CASE
			WHEN LK2.[Code] = N'ARE' THEN -- Domestic
				14.0 / 30.0 * Salary * (Years + Months / 12.0 + [Days] / 360.0)
			WHEN Years >= 5 THEN -- Expat, two cases, 
			-- 3 weeks per year for the first 5 years -- add to it one month per year for the additional period
				21.0/30.0 * Salary * 5 + Salary * ([Years] - 5 + [Months] / 12.0 + [Days] / 360.0)
			ELSE
				21.0/30.0 * Salary * (Years + Months / 12.0 + [Days] / 360.0)
	END
	FROM @Result T
	JOIN dbo.Agents AG ON AG.[Id] = T.[EmployeeId]
	JOIN dbo.Lookups LK2 ON LK2.[Id] = AG.[Lookup2Id]

	UPDATE @Result SET [Provision] = ROUND([Provision], 2);

	RETURN
END
GO