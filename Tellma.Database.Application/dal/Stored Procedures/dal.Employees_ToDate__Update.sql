CREATE PROCEDURE [dal].[Employees_ToDate__Update]
@EmployeeIds IdList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');
	DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id('Employee');
	DECLARE @FromDate DATE = DATEADD(YEAR, -1, GETDATE()); -- Go back 1 Year

	DECLARE @TerminationDates TABLE (
		EmployeeId INT PRIMARY KEY,
		TerminationDate DATE
	);
	INSERT INTO @TerminationDates(EmployeeId, TerminationDate)
	SELECT DISTINCT [NotedAgentId0] AS EmployeeId, MAX([Time20]) AS TerminationDate
	FROM bll.ft_Employees_Period_EventFromModel_Salaries__Generate --[bll].[ft_Widelines_Period_EventFromModel__Generate]
	(
		@FromDate, '9999-12-31', -- assuming retoractive termination of 3 months back only
		@BasicSalaryRS, -- @ResourceId
		@EmployeeIds --@NotedAgentId INT = NULL,
	)
	GROUP BY [NotedAgentId0];

-- UPDATE actual termination date
	UPDATE AG
	SET
		AG.ToDate = IIF(TD.[TerminationDate] = '9999-12-31', NULL, TD.[TerminationDate])
	--select AG.Todate,TD.TerminationDate
	FROM dbo.Agents AG
	JOIN @TerminationDates TD
	ON TD.EmployeeId = AG.[Id]
	WHERE (AG.ToDate IS NULL AND TD.[TerminationDate] < '9999-12-31')
	OR (AG.ToDate IS NOT NULL AND TD.[TerminationDate] = '9999-12-31' OR (TD.[TerminationDate] <> AG.ToDate))
	PRINT @@ROWCOUNT;
	
	-- UPDATE expected termination date
	UPDATE dbo.Agents
	SET Date4 = CASE
		WHEN [Date4] IS NULL THEN DATEADD(DAY, -1, DATEADD(MONTH, ISNULL([Decimal1], 12), [FromDate]))
		-- if expected termination date is bypassed, the contract is not terminated, extend by contract period
		WHEN [Date4] < GETDATE() AND [ToDate] IS NULL THEN DATEADD(MONTH, ISNULL([Decimal1], 12), [Date4])
		ELSE [Date4]
	END
	WHERE DefinitionId = @EmployeeAD
	AND Date4 <> CASE
		WHEN [Date4] IS NULL THEN DATEADD(DAY, -1, DATEADD(MONTH, ISNULL([Decimal1], 12), [FromDate]))
		-- if expected termination date is bypassed, the contract is not terminated, extend by contract period
		WHEN [Date4] < GETDATE() AND [ToDate] IS NULL THEN DATEADD(MONTH, ISNULL([Decimal1], 12), [Date4])
		ELSE [Date4]
	END

	-- Update Centers
	DECLARE @EmployeeCenters TABLE (
		EmployeeId INT PRIMARY KEY,
		CenterId INT
	);
	INSERT INTO @EmployeeCenters(EmployeeId, CenterId)
	SELECT [NotedAgentId0] AS EmployeeId, MIN([CenterId0]) AS CenterId
	FROM  bll.ft_Employees_Period_EventFromModel_Salaries__Generate
	(
		GETDATE(), GETDATE(),
		@BasicSalaryRS, -- @ResourceId
		@EmployeeIds --@NotedAgentId INT = NULL,
	)
	--WHERE @EmployeeCount = 0 OR [NotedAgentId0] IN (SELECT [Id] FROM @EmployeeIds)
	GROUP BY [NotedAgentId0]
	HAVING MIN([CenterId0]) = MAX([CenterId0]);

	UPDATE AG
	SET
		AG.[CenterId] = ISNULL(EC.[CenterId], AG.[CenterId])
	FROM dbo.Agents AG
	JOIN @EmployeeCenters EC
	ON EC.EmployeeId = AG.[Id]
	WHERE AG.DefinitionId = @EmployeeAD
	AND (
		(AG.[CenterId] IS NULL AND EC.[CenterId] IS NOT NULL) OR
		( AG.[CenterId] IS NOT NULL AND EC.[CenterId] IS NULL) OR
		( AG.[CenterId] <> EC.CenterId)
	);
	PRINT @@ROWCOUNT;

	-- The following logic works irrespective of the inpute list
	DECLARE @TerminatedAndActive IdList, @EOSVoucherAndActive IdList, @NoAccrualAndActive IdList, @AccrualAndInActive Idlist;
	DECLARE @EOSVoucherDD INT = dal.fn_DocumentDefinitionCode__Id(N'EndOfServiceVoucher');
	
	INSERT INTO @TerminatedAndActive 
	SELECT [Id]
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD AND [IsActive] = 1
	AND [ToDate] IS NOT NULL 
	
	INSERT INTO @EOSVoucherAndActive
	SELECT [Id]
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD AND [IsActive] = 1
	AND [Id] IN (SELECT [AgentId] FROM dbo.Documents WHERE DefinitionId = @EOSVoucherDD AND [State] = 1);

	WITH NoAccrual AS (
		SELECT E.[AgentId], E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS Balance
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE L.[State] = 4
		AND AC.[Concept] = N'ShorttermEmployeeBenefitsAccruals'
		GROUP BY E.[AgentId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) = 0

	)
	INSERT INTO @NoAccrualAndActive
	SELECT [Id]
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD AND [IsActive] = 1
	AND Id IN (SELECT [AgentId] FROM NoAccrual)
	-- Deactivate if Terminated and EOS Voucher and No Accrual
	UPDATE AG
	SET AG.[IsActive] = 0
	FROM dbo.Agents AG
	JOIN @TerminatedAndActive TA ON TA.[Id] = AG.[Id]
	JOIN @EOSVoucherAndActive EA ON EA.[Id] = AG.[Id]
	JOIN @NoAccrualAndActive NA ON NA.[Id] = AG.[Id]
	WHERE AG.[DefinitionId] = @EmployeeAD
	PRINT @@ROWCOUNT;

	WITH Accrual AS (
		SELECT E.[AgentId], E.[CurrencyId], SUM(E.[Direction] * E.[MonetaryValue]) AS Balance
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
		JOIN dbo.AccountTypes AC ON AC.[Id] = A.[AccountTypeId]
		WHERE L.[State] = 4
		AND AC.[Concept] = N'ShorttermEmployeeBenefitsAccruals'
		GROUP BY E.[AgentId], E.[CurrencyId]
		HAVING SUM(E.[Direction] * E.[MonetaryValue]) <> 0

	)
	INSERT INTO @AccrualAndInActive
	SELECT [Id]
	FROM dbo.Agents
	WHERE [DefinitionId] = @EmployeeAD AND [IsActive] = 0
	AND Id IN (SELECT [AgentId] FROM Accrual)
	-- Activate if Accrual
	UPDATE AG
	SET AG.[IsActive] = 1
	FROM dbo.Agents AG
	JOIN @AccrualAndInActive NA ON NA.[Id] = AG.[Id]
	WHERE AG.[DefinitionId] = @EmployeeAD
	PRINT @@ROWCOUNT;

	--INSERT 

	-- Move to a separate job and raise error if it returns such employees
	--SELECT [NotedAgentId0] AS EmployeeId_WithMultipleCenters, MIN([CenterId0]) AS Center1Id, MAX([CenterId0]) AS Center2Id
	--FROM bll.ft_Employees_Period_EventFromModel_Salaries__Generate
	--(
	--	GETDATE(), GETDATE(),
	--	@BasicSalaryRS, -- @ResourceId
	--	@EmployeeIds --@NotedAgentId INT = NULL,
	--)
	--GROUP BY [NotedAgentId0]
	--HAVING MIN([CenterId0]) <> MAX([CenterId0]);
	
END
GO
