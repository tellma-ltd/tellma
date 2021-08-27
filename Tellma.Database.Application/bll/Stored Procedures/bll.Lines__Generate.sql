CREATE PROCEDURE [bll].[Lines__Generate]
	@LineDefinitionId INT,
	@GenerateArguments [GenerateArgumentList] READONLY
AS
	SET NOCOUNT ON;
	DECLARE @Script NVARCHAR (MAX);
	DECLARE @WideLines WideLineList;
	DECLARE @Lines LineList, @Entries EntryList;
	SELECT @Script = [GenerateScript] FROM dbo.LineDefinitions WHERE [Id] = @LineDefinitionId;

	INSERT INTO @WideLines
	EXECUTE	sp_executesql @Script, N'@GenerateArguments [GenerateArgumentList] READONLY',
			@GenerateArguments = @GenerateArguments;

	UPDATE @WideLines SET DefinitionId =  @LineDefinitionId
	
	INSERT INTO @Lines([Index], [DocumentIndex], [DefinitionId], [PostingDate], [Memo], [Boolean1], [Decimal1], [Text1])
	SELECT [Index], 0, @LineDefinitionId, [PostingDate], [Memo], [Boolean1], [Decimal1], [Text1] FROM @WideLines

	INSERT INTO @Entries
	EXEC [bll].[WideLines__Unpivot] @WideLines;

	SELECT
		[L].[DefinitionId],
		[L].[PostingDate],
		[L].[Memo],
		[L].[Boolean1],
		[L].[Decimal1],
		[L].[Text1],
		[L].[Index]
	FROM @Lines AS [L] -- LineList
	ORDER BY [L].[Index] ASC

	SELECT
		E.[AccountId],
		E.[CurrencyId],
		E.[RelationId],
		E.[NotedRelationId],
		E.[ResourceId],
		E.[EntryTypeId],
		E.[CenterId],
		E.[UnitId],
		E.[Direction],
		E.[MonetaryValue],
		E.[Quantity],
		E.[Value],
		E.[Time1],
		E.[Time2],
		E.[ExternalReference],
		E.[ReferenceSourceId],
		E.[InternalReference],
		E.[NotedAgentName],
		E.[NotedAmount],
		E.[NotedDate],
		E.[LineIndex]
	FROM @Entries AS E
	ORDER BY E.[Index] ASC
	
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
	WHERE [Id] IN (SELECT [ResourceId] FROM @Entries);

	-- Relation (From 3 places)
	SELECT 
		R.[Id], 
		R.[Name],
		R.[Name2],
		R.[Name3],
		R.[DefinitionId]
	FROM [map].[Relations]() R 
	WHERE [Id] IN (SELECT [RelationId] FROM @Entries)
	OR [Id] IN  (SELECT [NotedRelationId] FROM @Entries);

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