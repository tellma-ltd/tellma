CREATE PROCEDURE [dal].[Employees_ToDate__Update]
@EmployeeIds IdList READONLY
AS
BEGIN
	DECLARE @ContractLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	DECLARE @ContractAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	DECLARE @ContractTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');
	DECLARE @DurationUnitId INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');
	DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id('Employee');

	UPDATE AG
	SET AG.ToDate = ISNULL(SS.[Time20], AG.ToDate)
	FROM dbo.Agents AG
	OUTER APPLY
	[bll].[ft_Widelines_Period_EventFromModel__Generate]
		(
			@ContractLineDefinitionId,
			@ContractAmendmentLineDefinitionId,
			@ContractTerminationLineDefinitionId,
			'0001-01-01', '9999-12-31',
			@DurationUnitId,
			0,
			NULL, --@AgentId INT = NULL,
			@BasicSalaryRS, -- @ResourceId
			AG.[Id], --@NotedAgentId INT = NULL,
			NULL, --@NotedResourceId INT = NULL,
			NULL --@CenterId INT = NULL
		) SS
	WHERE AG.DefinitionId = @EmployeeAD
END
GO