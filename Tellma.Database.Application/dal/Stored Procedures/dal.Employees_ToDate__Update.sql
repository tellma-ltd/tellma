CREATE PROCEDURE [dal].[Employees_ToDate__Update]
@EmployeeIds IdList READONLY
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ContractLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	DECLARE @ContractAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	DECLARE @ContractTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');
	DECLARE @DurationUnitId INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');
	DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id('Employee');
	DECLARE @EmployeeCount INT = (SELECT COUNT(*) FROM @EmployeeIds);

	-- Update Termination Date
	DECLARE @TerminationDates TABLE (
		EmployeeId INT PRIMARY KEY,
		TerminationDate DATE
	);
	INSERT INTO @TerminationDates(EmployeeId, TerminationDate)
	SELECT [NotedAgentId0] AS EmployeeId, MAX([Time20]) AS TerminationDate
	FROM [bll].[ft_Widelines_Period_EventFromModel__Generate]
	(
		@ContractLineDefinitionId,
		@ContractAmendmentLineDefinitionId,
		@ContractTerminationLineDefinitionId,
		GETDATE(), '9999-12-31',
		@DurationUnitId,
		0,
		NULL, --@AgentId INT = NULL,
		@BasicSalaryRS, -- @ResourceId
		NULL, --@NotedAgentId INT = NULL,
		NULL, --@NotedResourceId INT = NULL,
		NULL --@CenterId INT = NULL
	)
	WHERE @EmployeeCount = 0 OR [NotedAgentId0] IN (SELECT [Id] FROM @EmployeeIds)
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
	PRINT @@ROWCOUNT;
	UPDATE dbo.Agents
	SET ToDate = NULL
	WHERE ToDate >= '9999-12-30'; -- to handle a bug. Can be removed later

	-- Update Centers
	DECLARE @EmployeeCenters TABLE (
		EmployeeId INT PRIMARY KEY,
		CenterId INT
	);
	INSERT INTO @EmployeeCenters(EmployeeId, CenterId)
	SELECT [NotedAgentId0] AS EmployeeId, [CenterId0] AS CenterId
	FROM [bll].[ft_Widelines_Period_EventFromModel__Generate]
	(
		@ContractLineDefinitionId,
		@ContractAmendmentLineDefinitionId,
		@ContractTerminationLineDefinitionId,
		GETDATE(), GETDATE(),
		@DurationUnitId,
		0,
		NULL, --@AgentId INT = NULL,
		@BasicSalaryRS, -- @ResourceId
		NULL, --@NotedAgentId INT = NULL,
		NULL, --@NotedResourceId INT = NULL,
		NULL --@CenterId INT = NULL
	)
	WHERE @EmployeeCount = 0 OR [NotedAgentId0] IN (SELECT [Id] FROM @EmployeeIds);
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
END
GO