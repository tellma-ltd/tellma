CREATE PROCEDURE [api].[LineDefinitions__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryRelationDefinitions LineDefinitionEntryRelationDefinitionList READONLY,
	@LineDefinitionEntryCustodianDefinitions LineDefinitionEntryCustodianDefinitionList READONLY,
	@LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList READONLY,
	@LineDefinitionEntryNotedRelationDefinitions LineDefinitionEntryNotedRelationDefinitionList READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionGenerateParameters [LineDefinitionGenerateParameterList] READONLY,
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
		@LineDefinitionEntryRelationDefinitions = @LineDefinitionEntryRelationDefinitions,
		@LineDefinitionEntryCustodianDefinitions = @LineDefinitionEntryCustodianDefinitions,
		@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
		@LineDefinitionEntryNotedRelationDefinitions = @LineDefinitionEntryNotedRelationDefinitions,
		@LineDefinitionColumns = @LineDefinitionColumns,
		@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
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
		@LineDefinitionEntryRelationDefinitions = @LineDefinitionEntryRelationDefinitions,
		@LineDefinitionEntryCustodianDefinitions = @LineDefinitionEntryCustodianDefinitions,
		@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
		@LineDefinitionEntryNotedRelationDefinitions = @LineDefinitionEntryNotedRelationDefinitions,
		@LineDefinitionColumns = @LineDefinitionColumns,
		@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
		@LineDefinitionEntries = @LineDefinitionEntries,
		@LineDefinitionStateReasons = @LineDefinitionStateReasons,
		@Workflows = @Workflows,
		@WorkflowSignatures = @WorkflowSignatures;
END;