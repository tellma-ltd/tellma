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
	DECLARE @EmployeCount INT = (SELECT COUNT(*) FROM @EmployeeIds);
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
	WHERE @EmployeCount = 0 OR [NotedAgentId0] IN (SELECT [Id] FROM @EmployeeIds)
	GROUP BY [NotedAgentId0];

	UPDATE AG
	SET AG.ToDate = ISNULL(TD.[TerminationDate], AG.ToDate)
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
END
GO