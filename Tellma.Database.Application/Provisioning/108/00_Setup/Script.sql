-- De-Activate Unneeded Entry Types
UPDATE dbo.EntryTypes SET IsActive = 0;
delete from DocumentDefinitionLineDefinitions where DocumentDefinitionId > 1
delete from DocumentDefinitions where Id > 1
delete from AccountTypeResourceDefinitions
delete from AccountTypeContractDefinitions
delete from AccountTypeNotedContractDefinitions
delete from LineDefinitionEntryResourceDefinitions
delete from LineDefinitionEntryContractDefinitions
delete from LineDefinitionEntryNotedContractDefinitions
delete from ResourceDefinitions
delete from ContractDefinitions
delete from WorkflowSignatures
delete from Workflows
delete from LookupDefinitions

-- Activate Currencies in Use
INSERT INTO @IndexedCurrencyIds
([Index],	[Id]) VALUES
(0,			@SDG),
(1,			@USD);

EXEC [api].[Currencies__Activate]
	@IndexedIds = @IndexedCurrencyIds,
	@IsActive = 1,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Currencies Activating: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
