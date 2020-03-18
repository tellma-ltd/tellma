CREATE PROCEDURE [bll].[LineDefinitions_Validate__Save]
	@Entities [LineDefinitionList] READONLY,
	@LineDefinitionColumns [LineDefinitionColumnList] READONLY,
	@LineDefinitionEntries [LineDefinitionEntryList] READONLY,
	@LineDefinitionStateReasons [LineDefinitionStateReasonList] READONLY,
	@Workflows [WorkflowList] READONLY,
	@WorkflowSignatures [WorkflowSignatureList] READONLY,
	@Top INT = 10
AS
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- If IsCurrent is specified in Account types and Line Definition Entries=> must equal.
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].LineDefinitionEntries[' + CAST(LDE.[Index]  AS NVARCHAR (255)) + '].IsCurrent',
		N'Error_IsCurrent0IsIncompatibleWithAccountType1',
		dbo.fn_Localize(AC.[Name], AC.Name2, AC.Name3) AS AccountType
	FROM @Entities LD 
	JOIN @LineDefinitionEntries LDE ON LD.[Index] = LDE.[HeaderIndex]
	JOIN dbo.AccountTypes AC ON LDE.[AccountTypeParentCode] = AC.[Code]
	WHERE AC.IsCurrent IS NOT NULL AND LDE.IsCurrent IS NOT NULL
	AND AC.IsCurrent <> LDE.IsCurrent;

	SELECT TOP(@Top) * FROM @ValidationErrors;