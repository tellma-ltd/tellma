CREATE PROCEDURE [bll].[EntryList_IncludeRelatedEntities]
	@Lines [dbo].[LineList] READONLY,
	@Entries [dbo].[EntryList] READONLY
AS
BEGIN
	-- Accounts
	SELECT 
		A.[Id], 
		A.[Name], 
		A.[Name2], 
		A.[Name3], 
		A.[Code] 
	FROM [map].[Accounts]() A 
	WHERE [Id] IN (SELECT [AccountId] FROM @Entries);

	-- Currency
	SELECT 
		C.[Id], 
		C.[Name],
		C.[Name2], 
		C.[Name3], 
		C.[E]
	FROM [map].[Currencies]() C 
	WHERE [Id] IN (SELECT [CurrencyId] FROM @Entries);

	-- Resource
	SELECT 
		R.[Id], 
		R.[Name], 
		R.[Name2], 
		R.[Name3], 
		R.[DefinitionId] 
	FROM [map].[Resources]() R 
	WHERE [Id] IN (SELECT [ResourceId] FROM @Entries)
	OR [Id] IN  (SELECT [NotedResourceId] FROM @Entries);

	-- Agent (From 3 places)
	SELECT 
		R.[Id], 
		R.[Name],
		R.[Name2],
		R.[Name3],
		R.[DefinitionId]
	FROM [map].[Agents]() R 
	WHERE [Id] IN (SELECT [AgentId] FROM @Entries)
	OR [Id] IN  (SELECT [NotedAgentId] FROM @Entries);

	-- EntryType
	SELECT 
		ET.[Id],
		ET.[Name], 
		ET.[Name2], 
		ET.[Name3]
	FROM [map].[EntryTypes]() ET
	WHERE [Id] IN (SELECT [EntryTypeId] FROM @Entries);
	
	-- Center
	SELECT 
		C.[Id], 
		C.[Name], 
		C.[Name2], 
		C.[Name3] 
	FROM [map].[Centers]() C 
	WHERE [Id] IN (SELECT [CenterId] FROM @Entries);

	-- Unit
	SELECT 
		U.[Id], 
		U.[Name], 
		U.[Name2], 
		U.[Name3] 
	FROM [map].[Units]() U 
	WHERE [Id] IN (SELECT [UnitId] FROM @Entries);
END