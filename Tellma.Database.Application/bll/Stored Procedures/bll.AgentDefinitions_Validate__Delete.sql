CREATE PROCEDURE [bll].[AgentDefinitions_Validate__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];	

	-- Check that Definition is not used
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0AlreadyContainsData',
		[dbo].[fn_Localize](D.[TitlePlural], D.[TitlePlural2], D.[TitlePlural3]) AS [AgentDefinition]
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] D ON D.[Id] = FE.[Id]
	JOIN [dbo].[Agents] R ON R.[DefinitionId] = FE.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [AgentDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AccountTypeAgentDefinitions] ATRD ON ATRD.[AgentDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON ATRD.[AccountTypeId] = AC.[Id]
	JOIN [dbo].[AgentDefinitions] RD ON ATRD.[AgentDefinitionId] = RD.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [NotedAgentDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AccountTypeNotedAgentDefinitions] ATRD ON ATRD.[NotedAgentDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON ATRD.[AccountTypeId] = AC.[Id]
	JOIN [dbo].[AgentDefinitions] RD ON ATRD.[NotedAgentDefinitionId] = RD.[Id]

		-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](RD.[TitlePlural], RD.[TitlePlural2], RD.[TitlePlural3]) AS [NotedResourceDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AccountTypeNotedResourceDefinitions] ATRD ON ATRD.[NotedResourceDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON ATRD.[AccountTypeId] = AC.[Id]
	JOIN [dbo].[AgentDefinitions] RD ON ATRD.[NotedResourceDefinitionId] = RD.[Id]

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;