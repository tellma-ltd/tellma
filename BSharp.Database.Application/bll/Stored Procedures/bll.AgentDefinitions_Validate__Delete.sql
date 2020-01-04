CREATE PROCEDURE [bll].[AgentDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedStringList] READONLY,
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Check that LookupDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinitionIsUsedInAccount0',
		dbo.fn_Localize(A.[Name], A.[Name2], A.[Name3]) AS [Account]
	FROM @Ids FE
	JOIN dbo.Accounts A ON A.[AgentDefinitionId] = FE.[Id]

	-- Check that LookupDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinitionIsUsedInLineDefinition0',
		dbo.fn_Localize(LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS [LineDefinition]
	FROM @Ids FE
	JOIN dbo.LineDefinitions LD ON LD.[AgentDefinitionId] = FE.[Id]

	-- Check that LookupDefinitionId is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_TheAgentDefinitionIsUsedInLineDefinition0',
		dbo.fn_Localize(LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS [LineDefinition]
	FROM @Ids FE
	JOIN dbo.LineDefinitionEntries LDE ON LDE.[AgentDefinitionId] = FE.[Id]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]

	SELECT TOP(@Top) * FROM @ValidationErrors;