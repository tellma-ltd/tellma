CREATE PROCEDURE [dal].[Employees_ToDate__Update]
@EmployeeIds IdList READONLY
AS
BEGIN
	DECLARE @NewSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	DECLARE @AmendedSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	DECLARE @TerminatedSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');
	DECLARE @DurationUnitId INT = dal.fn_UnitCode__Id(N'mo');
	DECLARE @EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee');
	DECLARE @EmployeeBenefitsRD INT = dal.fn_ResourceDefinitionCode__Id(N'EmployeeBenefits');
	DECLARE @BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');

	EXEC [dal].[NotedAgents_ToDate__Update]
		@ContractLineDefinitionId = @NewSalariesLD,
		@ContractAmendmentLineDefinitionId = @AmendedSalariesLD,
		@ContractTerminationLineDefinitionId  = @TerminatedSalariesLD,
		@DurationUnitId =@DurationUnitId,
		@EntryIndex = 0,
		@NotedAgentDefinitionId = @EmployeeAD,
		@NotedAgentIds = @EmployeeIds,
		@ResourceDefinitionId = @EmployeeBenefitsRD,
		@ResourceId = @BasicSalaryRS
END
