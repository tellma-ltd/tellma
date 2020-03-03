CREATE PROCEDURE [bll].[AgentDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that AgentDefinitionId is not used in Accounts
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinitionIsUsedInAccount0',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS [Account]
	FROM @Ids FE
	JOIN dbo.Accounts A ON A.[AgentDefinitionId] = FE.[Id]

	-- Check that AgentDefinitionId is not used in LineDefinitionEntries
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinitionIsUsedInLineDefinition0',
		dbo.fn_Localize(LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS [LineDefinition]
	FROM @Ids FE
	JOIN dbo.LineDefinitionEntries LDE ON LDE.[AgentDefinitionId] LIKE N'%' + FE.[Id] + N'%'
	JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]

	SELECT TOP(@Top) * FROM @ValidationErrors;