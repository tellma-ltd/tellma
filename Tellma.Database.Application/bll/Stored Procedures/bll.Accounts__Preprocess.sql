CREATE PROCEDURE [bll].[Accounts__Preprocess]
	@Entities [dbo].[AccountList] READONLY,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	--=-=-=-=-=-=- [C# Preprocess - Before SQL]
	/* 
		[✓] If AgentDefinitionId set AgentId = null
		[✓] If ResourceDefinitionId set ResourceId = null
		[✓] If NotedAgentDefinitionId set NotedAgentId = null
	*/

	DECLARE @ProcessedEntities [dbo].[AccountList];
	INSERT INTO @ProcessedEntities SELECT * FROM @Entities;

	-- If Agent has a Currency or Center copy it to Account
	UPDATE A
	SET A.[CurrencyId] = COALESCE(R.[CurrencyId], A.[CurrencyId]),
		A.[CenterId] = COALESCE(R.[CenterId], A.[CenterId])
	FROM @ProcessedEntities A JOIN dbo.[Agents] R ON A.[AgentId] = R.Id;

	-- If Resource has a CurrencyId Or Center, copy it to Account
	/*
	UPDATE A
	SET
		A.[CurrencyId] = R.[CurrencyId],
		A.[CenterId] = COALESCE(
			IIF(AC.[IsBusinessUnit] = 1, R.[CenterId], R.[CostCenterId]),
			A.[CenterId])
	FROM @ProcessedEntities A JOIN dbo.[Resources] R ON A.[ResourceId] = R.Id
	JOIN map.AccountTypes() AC ON A.[AccountTypeId] = AC.[Id];
	*/
	-- Account Type/Agent Definition = null => Account/Agent is null
	UPDATE A
	SET [AgentId] = NULL, [AgentDefinitionId] = NULL 
	FROM  @ProcessedEntities A
	LEFT JOIN dbo.[AccountTypeAgentDefinitions] ATRD ON A.[AccountTypeId] = ATRD.[AccountTypeId] AND A.[AgentDefinitionId] = ATRD.[AgentDefinitionId]
	WHERE A.[AgentDefinitionId] IS NOT NULL AND ATRD.[AgentDefinitionId] IS NULL

	UPDATE A
	SET [ResourceId] = NULL, [ResourceDefinitionId] = NULL 
	FROM  @ProcessedEntities A
	LEFT JOIN dbo.[AccountTypeResourceDefinitions] ATRD ON A.[AccountTypeId] = ATRD.[AccountTypeId] AND A.[ResourceDefinitionId] = ATRD.[ResourceDefinitionId]
	WHERE A.[ResourceDefinitionId] IS NOT NULL AND ATRD.[ResourceDefinitionId] IS NULL

	UPDATE A
	SET [NotedAgentId] = NULL, [NotedAgentDefinitionId] = NULL 
	FROM  @ProcessedEntities A
	LEFT JOIN dbo.[AccountTypeNotedAgentDefinitions] ATRD ON A.[AccountTypeId] = ATRD.[AccountTypeId] AND A.[NotedAgentDefinitionId] = ATRD.[NotedAgentDefinitionId]
	WHERE A.[NotedAgentDefinitionId] IS NOT NULL AND ATRD.[NotedAgentDefinitionId] IS NULL

	UPDATE A
	SET [ResourceId] = NULL
	FROM  @ProcessedEntities A
	WHERE [ResourceDefinitionId] IS NULL 

	-- Return the result
	SELECT * FROM @ProcessedEntities;
END;