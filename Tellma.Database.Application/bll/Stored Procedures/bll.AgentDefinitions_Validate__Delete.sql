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
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS [AgentDefinition]
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] AD ON AD.[Id] = FE.[Id]
	WHERE FE.[Id] IN (SELECT DISTINCT [DefinitionId] FROM dbo.Agents)

-- Check that Definition is not used in Account Agent Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccount1',
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS [AgentDefinition],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] AD ON AD.[Id] = FE.[Id]
	JOIN [dbo].[Accounts] A ON A.[AgentDefinitionId] = FE.[Id]

-- Check that Definition is not used in Account Noted Agent Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccount1',
		[dbo].[fn_Localize](NAD.[TitleSingular], NAD.[TitleSingular2], NAD.[TitleSingular3]) AS [NotedAgentDefinition],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] NAD ON NAD.[Id] = FE.[Id]
	JOIN [dbo].[Accounts] A ON A.[NotedAgentDefinitionId] = FE.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS [AgentDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] AD ON AD.[Id] = FE.[Id]
	JOIN [dbo].[AccountTypeAgentDefinitions] ATAD ON ATAD.[AgentDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON AC.[Id] = ATAD.[AccountTypeId]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](NAD.[TitleSingular], NAD.[TitleSingular2], NAD.[TitleSingular3]) AS [NotedAgentDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] NAD ON NAD.[Id] = FE.[Id]
	JOIN [dbo].[AccountTypeNotedAgentDefinitions] ATNAD ON ATNAD.[NotedAgentDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON AC.[Id] = ATNAD.[AccountTypeId]

	-- Check that Definition is not used in Line Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInLineDefinition1',
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS [AgentDefinition],
		[dbo].[fn_Localize](LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS LineDefinition
	FROM @Ids FE
	JOIN [dbo].[AgentDefinitions] AD ON AD.[Id] = FE.[Id]
	JOIN [dbo].[LineDefinitionEntryAgentDefinitions] LDEAD ON LDEAD.[AgentDefinitionId] = FE.[Id]
	JOIN [dbo].[LineDefinitionEntries] LDE ON LDE.[Id] = LDEAD.[LineDefinitionEntryId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]
	
	-- Check that Definition is not used in Line Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInLineDefinition1',
		dbo.[fn_Localize](NAD.[TitleSingular], NAD.[TitleSingular2], NAD.[TitleSingular3]) AS [AgentDefinition],
		dbo.[fn_Localize](LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS LineDefinition
	FROM @Ids FE
	JOIN dbo.AgentDefinitions NAD ON NAD.[Id] = FE.[Id]
	JOIN dbo.LineDefinitionEntryNotedAgentDefinitions LDENAD ON LDENAD.[NotedAgentDefinitionId] = FE.[Id]
	JOIN dbo.LineDefinitionEntries LDE ON LDE.[Id] = LDENAD.[LineDefinitionEntryId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;