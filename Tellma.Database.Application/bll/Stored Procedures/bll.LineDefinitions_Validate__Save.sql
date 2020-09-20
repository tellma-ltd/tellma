CREATE PROCEDURE [bll].[LineDefinitions_Validate__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryCustodyDefinitions [LineDefinitionEntryCustodyDefinitionList] READONLY,
	@LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList READONLY,
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

	WITH LineDefinitionNotedRelations AS (
		SELECT LD.[Index]
		FROM @Entities LD 
		JOIN dbo.LineDefinitionEntries LDE ON LDE.[LineDefinitionId] = LD.[Id]
		JOIN dbo.AccountTypes ATP ON LDE.[ParentAccountTypeId] = ATP.[Id]
		JOIN dbo.AccountTypes ATC ON (ATC.[Node].IsDescendantOf(ATP.[Node]) = 1)
		JOIN dbo.LineDefinitionColumns LDC ON LDC.LineDefinitionId = LD.[Id]
		WHERE ATC.[NotedRelationDefinitionId] IS NOT NULL
		AND LDC.ColumnName = N'NotedRelationId'
		AND LDC.[InheritsFromHeader] = 2
		GROUP BY LD.[Index]
		HAVING COUNT(DISTINCT ATC.[NotedRelationDefinitionId]) > 1
	)
	INSERT INTO @ValidationErrors([Key], [ErrorName])
	SELECT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +	']',
		N'localize:Error__DistinctRelationDefinitionInheritFromDocumentHeader'
	FROM LineDefinitionNotedRelations LD 

	SELECT TOP(@Top) * FROM @ValidationErrors;