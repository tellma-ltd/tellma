CREATE PROCEDURE [bll].[LineDefinitions_Validate__Save]
	@Entities [dbo].[LineDefinitionList] READONLY,
	@LineDefinitionEntries [dbo].[LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryRelationDefinitions [dbo].[LineDefinitionEntryRelationDefinitionList] READONLY,
	@LineDefinitionEntryResourceDefinitions [dbo].[LineDefinitionEntryResourceDefinitionList] READONLY,
	@LineDefinitionEntryNotedRelationDefinitions [dbo].[LineDefinitionEntryNotedRelationDefinitionList] READONLY,
	@LineDefinitionColumns [dbo].[LineDefinitionColumnList] READONLY,
	@LineDefinitionGenerateParameters [dbo].[LineDefinitionGenerateParameterList] READONLY,
	@LineDefinitionStateReasons [dbo].[LineDefinitionStateReasonList] READONLY,
	@Workflows [dbo].[WorkflowList] READONLY,
	@WorkflowSignatures [dbo].[WorkflowSignatureList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Center and currency, if any, must be required from draft state, to make error user friendly
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].Columns[' + CAST(LDC.[Index]  AS NVARCHAR (255)) + '].RequiredState',
		N'localize:Error_Column0_RequiredState_Draft',
		LDC.[ColumnName]
	FROM @Entities LD 
	JOIN @LineDefinitionColumns LDC ON LD.[Index] = LDC.[HeaderIndex]
	WHERE LDC.[ColumnName] IN (N'CurrencyId', N'CenterId')  AND LDC.RequiredState <> 0;

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;