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
	@BasicSalaryRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'BasicSalary'),
	@DailyWageRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'DailyWage'),
	@HourlyWageRS INT = dal.fn_ResourceDefinition_Code__Id(N'EmployeeBenefits', N'HourlyWage');

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
		@BasicSalaryRS,
		@EmployeeId, -- @NotedAgentId,
		@NotedResourceId,
		@CenterId
	);
	IF @@ROWCOUNT = 0
	BEGIN
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
			@DailyWageRS,
			@EmployeeId, -- @NotedAgentId,
			@NotedResourceId,
			@CenterId
		);
		IF @@ROWCOUNT = 0
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
			@HourlyWageRS,
			@EmployeeId, -- @NotedAgentId,
			@NotedResourceId,
			@CenterId
		);
	END
	RETURN
END
GO