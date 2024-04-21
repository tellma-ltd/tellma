CREATE FUNCTION [bll].[fn_Employee_AnnualLeave__Provision_AE]
(
	@EmployeeIds IdList READONLY,
	@AsOfDate DATE
)
RETURNS @Result TABLE (
	[EmployeeId] INT PRIMARY KEY,
	[BasicCurrencyId] NCHAR (3),
	[FromDate]	DATE,
	[ToDate] DATE,
	[ServiceDaysLost] INT,
	[Provision] DECIMAL (19, 6),
	[Quantity] DECIMAL (19, 6),
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
	@MonthUnitId INT = dal.fn_UnitCode__Id(N'mo'),
	@EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee'),
	@BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary'),
	@NotedAbsenceDays INT = 30, -- Number of absence days which disrupt service continuity. This KSA value. Maybe it is the same for UAE
	@YearlyAccrual INT = 24; -- number of days deserved int2. Default should be 30
	
	INSERT INTO @Result([EmployeeId], [FromDate], [ToDate], [ServiceDaysLost], [Provision])
	SELECT [Id], [FromDate], @AsOfDate, 0, 0
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD
--	AND [IsActive] = 1
	AND (NOT EXISTS (SELECT * FROM @EmployeeIds) OR [Id] IN (SELECT [Id] FROM @EmployeeIds));

	UPDATE R
	SET
		R.[BasicCurrencyId] = S.[CurrencyId0],
		R.[CenterId] = S.[CenterId0],
		R.[AgentId] = S.[AgentId0],
		R.[NotedResourceId] = S.[NotedResourceId0],
		R.[EntryTypeId] = S.[EntryTypeId0]
	FROM @Result R
	CROSS APPLY [bll].[ft_Employees_Period_EventFromModel_Salaries__Generate](@AsOfDate, @AsOfDate, @BasicSalaryRS, @EmployeeIds) S
	WHERE R.[EmployeeId] = S.[NotedAgentId0];

	DECLARE @GrossSalariesBenefits TABLE (
		EmployeeId	INT ,
		ResourceId INT,
		CurrencyId NCHAR (3),
		Benefit DECIMAL (19, 6)
		PRIMARY KEY(EmployeeId, ResourceId)
	);
	INSERT INTO @GrossSalariesBenefits
	SELECT [NotedAgentId0], [ResourceId0], [CurrencyId1], SUM([MonetaryValue1]) AS [Benefit]
	FROM [bll].[ft_Employees_Period_EventFromModel_Salaries__Generate](@AsOfDate, @AsOfDate, NULL, @EmployeeIds) SS
	JOIN dbo.Resources R ON R.[Id] = SS.[ResourceId0]
	WHERE R.[UnitId] = @MonthUnitId
	-- Ideally, an employee benefit need to have a lookup to select if included in Annual Leave provision
	-- AND R.[Lookup8Id] = @Yes
	GROUP BY [NotedAgentId0],  [ResourceId0], [CurrencyId1];

	DECLARE @GrossSalaries TABLE (
		EmployeeId	INT PRIMARY KEY,
		AmountInBasicCurrency DECIMAL (19, 6)
	);
	INSERT INTO @GrossSalaries(EmployeeId, AmountInBasicCurrency)
	SELECT R.[EmployeeId], SUM(bll.fn_ConvertCurrencies(@AsOfDate, T.[CurrencyId], R.[BasicCurrencyId], T.[Benefit]))
	FROM @GrossSalariesBenefits T
	JOIN @Result R ON R.[EmployeeId] = T.[EmployeeId]
	GROUP BY R.[EmployeeId];

	UPDATE R
	SET
		R.[Salary] = G.AmountInBasicCurrency
	FROM @Result R
	JOIN @GrossSalaries G ON G.[EmployeeId] = R.[EmployeeId];

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
		[Years] = dbo.fn_FromDate_ToDate__FullYears(@Calendar, FromDate, ToDate), 
		[Months] = dbo.fn_FromDate_ToDate__ExtraFullMonths(@Calendar, FromDate, ToDate), 
		[Days] = dbo.fn_FromDate_ToDate__ExtraFullDays(@Calendar, FromDate, ToDate);
	
	UPDATE R
	SET
		[Quantity] = ISNULL(AG.Int2, @YearlyAccrual) * ([Years] + [Months] / 12.0 + [Days] / 360.0)
	FROM @Result R
	JOIN dbo.Agents AG ON AG.[Id] = R.[EmployeeId]


	DECLARE @AnnualLeaveBenefitRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'AnnualLeave');
	DECLARE @Usage TABLE (
		[EmployeeId]	INT PRIMARY KEY,
		[Quantity]		DECIMAL (19, 6)
	)
	INSERT INTO @Usage
	SELECT E.[AgentId], SUM(E.[Direction] * E.[Quantity])
	FROM dbo.Entries E
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	WHERE L.[State] = 4
	AND E.[Direction] = 1 -- for usage
	AND L.[PostingDate] <= @AsOfDate
	AND AC.[Concept] = N'CurrentProvisionsForEmployeeBenefits'
	AND E.[ResourceId] = @AnnualLeaveBenefitRS
	AND E.[AgentId] IN (SELECT [EmployeeId] FROM @Result)
	GROUP BY E.[AgentId];

	UPDATE R
	SET  [Quantity] = ROUND(R.[Quantity] - ISNULL(U.[Quantity], 0), 4)
	FROM @Result R
	LEFT JOIN @Usage U ON U.[EmployeeId] = R.[EmployeeId];

	UPDATE @Result SET [Provision] = ROUND([Salary] * [Quantity] / 30.0, 2);

	RETURN
END
GO