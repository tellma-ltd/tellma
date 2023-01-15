CREATE FUNCTION [bll].[ft_EmployeeProfile_Period_EventFromModel__Generate]
(
	@EmployeeId INT,
	@AsOfDate DATE
)
	RETURNS @EmployeeProfiles TABLE
	(
		[EmployeeId]		INT,
		[CenterId]			INT,
		[AgentId]			INT,
		[NotedResourceId]	INT,
		[EntryTypeId]		INT,
		[CurrencyId]		NCHAR (3)
	)
	AS
	BEGIN
	DECLARE
	@ContractLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M'),
	@ContractAmendmentLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M'),
	@ContractTerminationLineDefinitionId INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M'),
	@DurationUnitId INT = dal.fn_UnitCode__Id('mo'),
	@EntryIndex INT = 0, @AgentId INT = NULL, @NotedResourceId INT = NULL, @CenterId INT = NULL,
	@ResourceId INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary');

	INSERT INTO @EmployeeProfiles([EmployeeId], [CenterId], [AgentId], [NotedResourceId], [EntryTypeId], [CurrencyId])
	SELECT [NotedAgentId0], [CenterId0], [AgentId0], [NotedResourceId0], [EntryTypeId0], [CurrencyId1]
	FROM  [bll].[ft_Widelines_Period_EventFromModel__Generate] (
		@ContractLineDefinitionId,
		@ContractAmendmentLineDefinitionId,
		@ContractTerminationLineDefinitionId,
		@AsOfDate,
		@AsOfDate,
		@DurationUnitId,
		@EntryIndex,
		@AgentId,
		@ResourceId,
		@EmployeeId, -- @NotedAgentId,
		@NotedResourceId,
		@CenterId
	);

	RETURN
END
GO