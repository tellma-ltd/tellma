CREATE PROCEDURE [bll].[Agents_Validate__Delete]	
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@UserId INT,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

-- Check that Agent is not used in Entries
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Agent0IsUsedInDocument1',
		[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3]) AS [AgentName],
		D.[Code]
	FROM @Ids FE
	JOIN [dbo].[Agents] AG ON AG.[Id] = FE.[Id]
	JOIN [dbo].[Entries] E ON E.[AgentId] = FE.[Id] OR E.[NotedAgentId] = FE.[Id]
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN map.Documents() D ON D.[Id] = L.[DocumentId]

-- Check that Agent is not used in Agent1Id or Agent2Id of Agents
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1], [Argument2])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Agent0IsUsedInAgent12',
		[dbo].[fn_Localize](AG.[Name], AG.[Name2], AG.[Name3]) AS [AgentName],
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS [AgentDefinition],
		[dbo].[fn_Localize](AGP.[Name], AGP.[Name2], AGP.[Name3]) AS [AgentParentName]
	FROM @Ids FE
	JOIN [dbo].[Agents] AG ON AG.[Id] = FE.[Id]
	JOIN [dbo].[Agents] AGP ON AGP.[Agent1Id] = FE.[Id] OR AGP.[Agent2Id] = FE.[Id]
	JOIN dbo.[AgentDefinitions] AD ON AD.[Id] = AGP.[DefinitionId]

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;