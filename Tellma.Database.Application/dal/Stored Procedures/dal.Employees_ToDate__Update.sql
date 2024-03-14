CREATE PROCEDURE [dal].[Employees_ToDate__Update]
@EmployeeIds IdList READONLY
AS
BEGIN
	SET NOCOUNT OFF;
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');
	DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id('Employee');
--	DECLARE @EmployeeCount INT = (SELECT COUNT(*) FROM @EmployeeIds);
	DECLARE @FromDate DATE = DATEADD(MONTH, -3, GETDATE()); -- Go back 3 months
	-- Update Termination Date
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
--	WHERE @EmployeeCount = 0 OR [NotedAgentId0] IN (SELECT [Id] FROM @EmployeeIds)
	GROUP BY [NotedAgentId0];

	UPDATE AG
	SET
		AG.ToDate = ISNULL(TD.[TerminationDate], AG.ToDate)
	FROM dbo.Agents AG
	LEFT JOIN @TerminationDates TD
	ON TD.EmployeeId = AG.[Id]
	WHERE AG.DefinitionId = @EmployeeAD
	AND (
		(AG.ToDate IS NULL AND TD.[TerminationDate] IS NOT NULL) OR
		(AG.ToDate IS NOT NULL AND TD.[TerminationDate] IS NULL) OR
		(AG.[ToDate] <> TD.[TerminationDate])
	);
--	PRINT @@ROWCOUNT;
	
	UPDATE dbo.Agents
	SET ToDate = NULL
	WHERE ToDate >= '9999-12-30'; -- to handle a bug. Can be removed later
	
	-- UPDATE expected termination date
	UPDATE dbo.Agents
	SET Date4 = CASE
		WHEN [Date4] IS NULL THEN DATEADD(DAY, -1, DATEADD(MONTH, ISNULL([Decimal1], 12), [FromDate]))
		-- if expected termination date is bypassed, the contract is not terminated, extend by contract period
		WHEN [Date4] < GETDATE() AND [ToDate] IS NULL THEN DATEADD(MONTH, ISNULL([Decimal1], 12), [Date4])
		ELSE [Date4]
	END
	WHERE DefinitionId = @EmployeeAD

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
	--PRINT @@ROWCOUNT;

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