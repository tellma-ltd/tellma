CREATE PROCEDURE [bll].[LineDefinitions_Validate__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryRelationDefinitions LineDefinitionEntryRelationDefinitionList READONLY,
	--@LineDefinitionEntryCustodianDefinitions LineDefinitionEntryCustodianDefinitionList READONLY,
	@LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList READONLY,
	@LineDefinitionEntryNotedRelationDefinitions LineDefinitionEntryNotedRelationDefinitionList READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionGenerateParameters [LineDefinitionGenerateParameterList] READONLY,
	@LineDefinitionStateReasons [LineDefinitionStateReasonList] READONLY,
	@Workflows [WorkflowList] READONLY,
	@WorkflowSignatures [WorkflowSignatureList] READONLY,
	@Top INT = 10
AS
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

	SELECT TOP(@Top) * FROM @ValidationErrors;