CREATE PROCEDURE [dal].[NotedAgents_ToDate__Update]
-- [dal].[Employees_ToDate__Update] used to depend on it but not anymore. So this one is not used now
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@ContractTerminationLineDefinitionId  INT,
	@DurationUnitId INT,
	@EntryIndex INT,
	@NotedAgentDefinitionId INT,
	@NotedAgentIds IdList READONLY,
	@ResourceDefinitionId INT,
	@ResourceId INT

	--@TerminatedSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');
	--@AmendedSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	--@NewSalariesLD INT = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	--@EmployeeAD INT = dal.fn_AgentDefinitionCode__Id(N'Employee');
	--@DurationUnitId INT = dal.fn_UnitCode__Id(N'mo');
AS
	DECLARE @OldContractAmendmentLineDefinitionId INT;
	IF @ContractAmendmentLineDefinitionId <> 0
	BEGIN
		DECLARE @ContractAmendmentLineDefinitionCode NVARCHAR (255) = dal.fn_LineDefinition__Code(@ContractAmendmentLineDefinitionId);
		IF @ContractAmendmentLineDefinitionCode IS NULL THROW 50000, N'New Contract Amendment Version is not deployed', 1;
		SET @OldContractAmendmentLineDefinitionId = ISNULL(dal.fn_LineDefinitionCode__Id(N'(Old)' + @ContractAmendmentLineDefinitionCode), 0);
	END;

	With NotedAgentsToDates AS (
		SELECT E.[NotedAgentId],-- dal.fn_Agent__Name(E.[NotedAgentId]) AS Employee, 
			MAX(ISNULL(E.[Time2], DATEADD(DAY, -1, E.[Time1]))) AS ToDate
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
		WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId,
														@OldContractAmendmentLineDefinitionId)
		AND (@ResourceDefinitionId IS NULL OR R.DefinitionId = @ResourceDefinitionId)
		AND (@ResourceId IS NULL OR E.[ResourceId] = @ResourceId)
		AND E.[Value] <> 0
		AND L.[State] >= 2
		AND E.[Index] = 0
		AND (
			NOT EXISTS (SELECT * FROM  @NotedAgentIds)
			OR 	E.[NotedAgentId] IN (SELECT [Id] FROM @NotedAgentIds)
		)
		GROUP BY E.[NotedAgentId]
	)
	--SELECT ELD.*, SS.[MonetaryValue0]
	UPDATE AG
	SET AG.ToDate = IIF(SS.[MonetaryValue0] IS NULL, NATD.ToDate, NULL)
	FROM Agents AG
	JOIN NotedAgentsToDates NATD ON NATD.[NotedAgentId] = AG.[Id]
	OUTER APPLY  [bll].[ft_Widelines_Period_EventFromModel__Generate]
	(
		@ContractLineDefinitionId,
		@ContractAmendmentLineDefinitionId,
		@ContractTerminationLineDefinitionId,
		'0001-01-01', '9999-12-31',
		@DurationUnitId,
		@EntryIndex,
		NULL, --@AgentId INT = NULL,
		NULL, -- @ResourceId
		NATD.[NotedAgentId], --@NotedAgentId INT = NULL,
		NULL, --@NotedResourceId INT = NULL,
		NULL --@CenterId INT = NULL
	) SS
	WHERE AG.[DefinitionId] = @NotedAgentDefinitionId;
GO