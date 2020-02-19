EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Line Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;