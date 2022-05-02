CREATE PROCEDURE [bll].[ResourceDefinitions_Validate__Delete]
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
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition]
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] RD ON RD.[Id] = FE.[Id]
	WHERE FE.[Id] IN (SELECT DISTINCT [DefinitionId] FROM dbo.Resources)

-- Check that Definition is not used in Account Resource Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccount1',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] RD ON RD.[Id] = FE.[Id]
	JOIN [dbo].[Accounts] A ON A.[ResourceDefinitionId] = FE.[Id]

-- Check that Definition is not used in Account Noted Resource Definition
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccount1',
		[dbo].[fn_Localize](NRD.[TitleSingular], NRD.[TitleSingular2], NRD.[TitleSingular3]) AS [NotedResourceDefinition],
		[dbo].[fn_Localize](A.[Name], A.[Name2], A.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] NRD ON NRD.[Id] = FE.[Id]
	JOIN [dbo].[Accounts] A ON A.[NotedResourceDefinitionId] = FE.[Id]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] RD ON RD.[Id] = FE.[Id]
	JOIN [dbo].[AccountTypeResourceDefinitions] ATRD ON ATRD.[ResourceDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON AC.[Id] = ATRD.[AccountTypeId]

	-- Check that Definition is not used in Account Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInAccountType1',
		[dbo].[fn_Localize](NRD.[TitleSingular], NRD.[TitleSingular2], NRD.[TitleSingular3]) AS [NotedResourceDefinition],
		[dbo].[fn_Localize](AC.[Name], AC.[Name2], AC.[Name3]) AS AccountType
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] NRD ON NRD.[Id] = FE.[Id]
	JOIN [dbo].[AccountTypeNotedResourceDefinitions] ATNRD ON ATNRD.[NotedResourceDefinitionId] = FE.[Id]
	JOIN [dbo].[AccountTypes] AC ON AC.[Id] = ATNRD.[AccountTypeId]

	-- Check that Definition is not used in Line Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInLineDefinition1',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition],
		[dbo].[fn_Localize](LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS LineDefinition
	FROM @Ids FE
	JOIN [dbo].[ResourceDefinitions] RD ON RD.[Id] = FE.[Id]
	JOIN [dbo].[LineDefinitionEntryResourceDefinitions] LDERD ON LDERD.[ResourceDefinitionId] = FE.[Id]
	JOIN [dbo].[LineDefinitionEntries] LDE ON LDE.[Id] = LDERD.[LineDefinitionEntryId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]
	
	-- Check that Definition is not used in Line Definition Filters
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP(@Top)
		 '[' + CAST(FE.[Index] AS NVARCHAR (255)) + ']',
		N'Error_Definition0IsUsedInLineDefinition1',
		dbo.[fn_Localize](NRD.[TitleSingular], NRD.[TitleSingular2], NRD.[TitleSingular3]) AS [ResourceDefinition],
		dbo.[fn_Localize](LD.[TitleSingular], LD.[TitleSingular2], LD.[TitleSingular3]) AS LineDefinition
	FROM @Ids FE
	JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = FE.[Id]
	JOIN dbo.LineDefinitionEntryNotedResourceDefinitions LDENRD ON LDENRD.[NotedResourceDefinitionId] = FE.[Id]
	JOIN dbo.LineDefinitionEntries LDE ON LDE.[Id] = LDENRD.[LineDefinitionEntryId]
	JOIN dbo.LineDefinitions LD ON LD.[Id] = LDE.[LineDefinitionId]

	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP(@Top) * FROM @ValidationErrors;
END;