CREATE PROCEDURE [api].[LineDefinitions__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryAccountTypes LineDefinitionEntryAccountTypeList READONLY,
	@LineDefinitionEntryContractDefinitions LineDefinitionEntryContractDefinitionList READONLY,
	@LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList READONLY,
	@LineDefinitionEntryNotedContractDefinitions LineDefinitionEntryNotedContractDefinitionList READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionStateReasons [LineDefinitionStateReasonList] READONLY,
	@Workflows [WorkflowList] READONLY,
	@WorkflowSignatures [WorkflowSignatureList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[LineDefinitions_Validate__Save]
		@Entities = @Entities,
		@LineDefinitionEntries = @LineDefinitionEntries,
		@LineDefinitionEntryAccountTypes = @LineDefinitionEntryAccountTypes,
		@LineDefinitionEntryContractDefinitions = @LineDefinitionEntryContractDefinitions,
		@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
		@LineDefinitionEntryNotedContractDefinitions = @LineDefinitionEntryNotedContractDefinitions,
		@LineDefinitionColumns = @LineDefinitionColumns,
		@LineDefinitionStateReasons = @LineDefinitionStateReasons,
		@Workflows = @Workflows,
		@WorkflowSignatures = @WorkflowSignatures;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);


	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[LineDefinitions__Save]
		@Entities = @Entities,
		@LineDefinitionEntryAccountTypes = @LineDefinitionEntryAccountTypes,
		@LineDefinitionEntryContractDefinitions = @LineDefinitionEntryContractDefinitions,
		@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
		@LineDefinitionEntryNotedContractDefinitions = @LineDefinitionEntryNotedContractDefinitions,
		@LineDefinitionColumns = @LineDefinitionColumns,
		@LineDefinitionEntries = @LineDefinitionEntries,
		@LineDefinitionStateReasons = @LineDefinitionStateReasons,
		@Workflows = @Workflows,
		@WorkflowSignatures = @WorkflowSignatures;
END;