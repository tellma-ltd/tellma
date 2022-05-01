CREATE PROCEDURE [bll].[LineDefinitions_Validate__Save]
	@Entities [dbo].[LineDefinitionList] READONLY,
	@LineDefinitionEntries [dbo].[LineDefinitionEntryList] READONLY,
	@LineDefinitionEntryAgentDefinitions [dbo].[LineDefinitionEntryAgentDefinitionList] READONLY,
	@LineDefinitionEntryResourceDefinitions [dbo].[LineDefinitionEntryResourceDefinitionList] READONLY,
	@LineDefinitionEntryNotedAgentDefinitions [dbo].[LineDefinitionEntryNotedAgentDefinitionList] READONLY,
	@LineDefinitionEntryNotedResourceDefinitions [dbo].[LineDefinitionEntryNotedResourceDefinitionList] READONLY,
	@LineDefinitionColumns [dbo].[LineDefinitionColumnList] READONLY,
	@LineDefinitionGenerateParameters [dbo].[LineDefinitionGenerateParameterList] READONLY,
	@LineDefinitionStateReasons [dbo].[LineDefinitionStateReasonList] READONLY,
	@Workflows [dbo].[WorkflowList] READONLY,
	@WorkflowSignatures [dbo].[WorkflowSignatureList] READONLY,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	-- Center and currency, if any, must be required from draft state, to make error user friendly
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].Columns[' + CAST(LDC.[Index]  AS NVARCHAR (255)) + '].RequiredState',
		N'localize:Error_Column0_RequiredState_Draft',
		LDC.[ColumnName]
	FROM @Entities LD 
	JOIN @LineDefinitionColumns LDC ON LD.[Index] = LDC.[HeaderIndex]
	WHERE LDC.[ColumnName] IN (N'CurrencyId', N'CenterId')  AND LDC.RequiredState <> 0;

	-- Account Agent Definition must be compatible with at least one of the descendants of the parent account type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].Entries[' + CAST(LDE.[Index]  AS NVARCHAR (255)) + 
			'].AgentDefinitions[' + CAST(LDEAD.[Index]  AS NVARCHAR (255)) + '].AgentDefinitionId',
		N'Error_AgentDefinition0IsNotUsedInAccountType1OrItsDescendents',
		[dbo].[fn_Localize](AD.[TitleSingular], AD.[TitleSingular2], AD.[TitleSingular3]) AS [AgentDefinition],
		[dbo].[fn_Localize](PAC.[Name], PAC.[Name2], PAC.[Name3]) AS AccountType
	FROM @Entities LD
	JOIN @LineDefinitionEntries LDE ON LDE.[HeaderIndex] = LD.[Index]
	JOIN dbo.[AccountTypes] PAC ON PAC.[Id] = LDE.[ParentAccountTypeId]
	JOIN @LineDefinitionEntryAgentDefinitions LDEAD ON LDEAD.[LineDefinitionEntryIndex] = LDE.[Index] AND LDEAD.[LineDefinitionIndex] = LD.[Index] 
	JOIN dbo.AgentDefinitions AD ON AD.[Id] = LDEAD.[AgentDefinitionId]
	WHERE LDEAD.[AgentDefinitionId] NOT IN (
		SELECT ATAD.AgentDefinitionId
		FROM dbo.AccountTypeAgentDefinitions ATAD
		JOIN dbo.AccountTypes CAC ON CAC.[Id] = ATAD.AccountTypeId
		WHERE CAC.[Node].IsDescendantOf(PAC.[Node]) = 1
	)

	-- Account Resource Definition must be compatible with at least one of the descendants of the parent account type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].Entries[' + CAST(LDE.[Index]  AS NVARCHAR (255)) + 
			'].ResourceDefinitions[' + CAST(LDERD.[Index]  AS NVARCHAR (255)) + '].ResourceDefinitionId',
		N'Error_ResourceDefinition0IsNotUsedInAccountType1OrItsDescendents',
		[dbo].[fn_Localize](RD.[TitleSingular], RD.[TitleSingular2], RD.[TitleSingular3]) AS [ResourceDefinition],
		[dbo].[fn_Localize](PAC.[Name], PAC.[Name2], PAC.[Name3]) AS AccountType
	FROM @Entities LD
	JOIN @LineDefinitionEntries LDE ON LDE.[HeaderIndex] = LD.[Index]
	JOIN dbo.[AccountTypes] PAC ON PAC.[Id] = LDE.[ParentAccountTypeId]
	JOIN @LineDefinitionEntryResourceDefinitions LDERD ON LDERD.[LineDefinitionEntryIndex] = LDE.[Index] AND LDERD.[LineDefinitionIndex] = LD.[Index] 
	JOIN dbo.ResourceDefinitions RD ON RD.[Id] = LDERD.[ResourceDefinitionId]
	WHERE LDERD.[ResourceDefinitionId] NOT IN (
		SELECT ATRD.ResourceDefinitionId
		FROM dbo.AccountTypeResourceDefinitions ATRD
		JOIN dbo.AccountTypes CAC ON CAC.[Id] = ATRD.AccountTypeId
		WHERE CAC.[Node].IsDescendantOf(PAC.[Node]) = 1
	)

	-- Account NotedAgent Definition must be compatible with at least one of the descendants of the parent account type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].Entries[' + CAST(LDE.[Index]  AS NVARCHAR (255)) + 
			'].NotedAgentDefinitions[' + CAST(LDENAD.[Index]  AS NVARCHAR (255)) + '].NotedAgentDefinitionId',
		N'Error_NotedAgentDefinition0IsNotUsedInAccountType1OrItsDescendents',
		[dbo].[fn_Localize](NAD.[TitleSingular], NAD.[TitleSingular2], NAD.[TitleSingular3]) AS [NotedAgentDefinition],
		[dbo].[fn_Localize](PAC.[Name], PAC.[Name2], PAC.[Name3]) AS AccountType
	FROM @Entities LD
	JOIN @LineDefinitionEntries LDE ON LDE.[HeaderIndex] = LD.[Index]
	JOIN dbo.[AccountTypes] PAC ON PAC.[Id] = LDE.[ParentAccountTypeId]
	JOIN @LineDefinitionEntryNotedAgentDefinitions LDENAD ON LDENAD.[LineDefinitionEntryIndex] = LDE.[Index] AND LDENAD.[LineDefinitionIndex] = LD.[Index] 
	JOIN dbo.AgentDefinitions NAD ON NAD.[Id] = LDENAD.[NotedAgentDefinitionId]
	WHERE LDENAD.[NotedAgentDefinitionId] NOT IN (
		SELECT ATNAD.NotedAgentDefinitionId
		FROM dbo.AccountTypeNotedAgentDefinitions ATNAD
		JOIN dbo.AccountTypes CAC ON CAC.[Id] = ATNAD.AccountTypeId
		WHERE CAC.[Node].IsDescendantOf(PAC.[Node]) = 1
	)

	-- Account NotedResource Definition must be compatible with at least one of the descendants of the parent account type
	INSERT INTO @ValidationErrors([Key], [ErrorName], [Argument0], [Argument1])
	SELECT DISTINCT TOP (@Top)
		'[' + CAST(LD.[Index] AS NVARCHAR (255)) +
			'].Entries[' + CAST(LDE.[Index]  AS NVARCHAR (255)) + 
			'].NotedResourceDefinitions[' + CAST(LDENRD.[Index]  AS NVARCHAR (255)) + '].NotedResourceDefinitionId',
		N'Error_NotedResourceDefinition0IsNotUsedInAccountType1OrItsDescendents',
		[dbo].[fn_Localize](NRD.[TitleSingular], NRD.[TitleSingular2], NRD.[TitleSingular3]) AS [NotedResourceDefinition],
		[dbo].[fn_Localize](PAC.[Name], PAC.[Name2], PAC.[Name3]) AS AccountType
	FROM @Entities LD
	JOIN @LineDefinitionEntries LDE ON LDE.[HeaderIndex] = LD.[Index]
	JOIN dbo.[AccountTypes] PAC ON PAC.[Id] = LDE.[ParentAccountTypeId]
	JOIN @LineDefinitionEntryNotedResourceDefinitions LDENRD ON LDENRD.[LineDefinitionEntryIndex] = LDE.[Index] AND LDENRD.[LineDefinitionIndex] = LD.[Index] 
	JOIN dbo.ResourceDefinitions NRD ON NRD.[Id] = LDENRD.[NotedResourceDefinitionId]
	WHERE LDENRD.[NotedResourceDefinitionId] NOT IN (
		SELECT ATNRD.NotedResourceDefinitionId
		FROM dbo.AccountTypeNotedResourceDefinitions ATNRD
		JOIN dbo.AccountTypes CAC ON CAC.[Id] = ATNRD.AccountTypeId
		WHERE CAC.[Node].IsDescendantOf(PAC.[Node]) = 1
	)


	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;