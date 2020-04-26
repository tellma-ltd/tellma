CREATE PROCEDURE [bll].[RelationDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that RelationDefinitionId is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheRelationDefinitionIsUsedInAccountDefinition0',
		dbo.fn_Localize(AD.[Name], AD.[Name2], AD.[Name3]) AS [Account]
	FROM @Ids FE
	JOIN dbo.AccountDefinitionRelationDefinitions ADRD ON ADRD.[RelationDefinitionId] = FE.[Id]
	JOIN dbo.AccountDefinitions AD ON AD.[Id] = ADRD.[AccountDefinitionId]

	SELECT TOP(@Top) * FROM @ValidationErrors;