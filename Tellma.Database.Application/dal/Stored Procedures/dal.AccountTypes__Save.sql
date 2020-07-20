CREATE PROCEDURE [dal].[AccountTypes__Save]
	@Entities [AccountTypeList] READONLY,
	@AccountTypeResourceDefinitions AccountTypeResourceDefinitionList READONLY,
	@AccountTypeContractDefinitions AccountTypeContractDefinitionList READONLY,
	@AccountTypeNotedContractDefinitions AccountTypeNotedContractDefinitionList READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[AccountTypes] AS t
		USING (
			SELECT
				E.[Index], E.[Id], E.[ParentId], E.[Code], E.[Concept],
				hierarchyid::Parse('/' + CAST(-ABS(CHECKSUM(NewId()) % 2147483648) AS VARCHAR(30)) + '/') AS [Node],
				E.[Name], E.[Name2], E.[Name3], E.[Description], E.[Description2], E.[Description3],
				E.[IsMonetary],
				E.[IsAssignable],
				E.[AllowsPureUnit],
				E.[EntryTypeParentId],
				E.[DueDateLabel],
				E.[DueDateLabel2],
				E.[DueDateLabel3],
				E.[Time1Label],
				E.[Time1Label2],
				E.[Time1Label3],
				E.[Time2Label],
				E.[Time2Label2],
				E.[Time2Label3],
				E.[ExternalReferenceLabel],
				E.[ExternalReferenceLabel2],
				E.[ExternalReferenceLabel3],
				E.[AdditionalReferenceLabel],
				E.[AdditionalReferenceLabel2],
				E.[AdditionalReferenceLabel3],
				E.[NotedAgentNameLabel],
				E.[NotedAgentNameLabel2],
				E.[NotedAgentNameLabel3],
				E.[NotedAmountLabel],
				E.[NotedAmountLabel2],
				E.[NotedAmountLabel3],
				E.[NotedDateLabel],
				E.[NotedDateLabel2],
				E.[NotedDateLabel3]
			FROM @Entities E
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[ParentId]				= IIF(t.[IsSystem]=0,s.[ParentId],t.[ParentId]),
				t.[Code]					= IIF(t.[IsSystem]=0,s.[Code],t.[Code]),
				t.[Concept]					= IIF(t.[IsSystem]=0,s.[Concept],t.[Concept]),
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
				t.[IsMonetary]				= IIF(t.[IsMonetary]=0,s.[IsMonetary],t.[IsMonetary]),
				t.[IsAssignable]			= IIF(t.[IsSystem]=0,s.[IsAssignable],t.[IsAssignable]),
				t.[AllowsPureUnit]			= IIF(t.[IsSystem]=0,s.[AllowsPureUnit],t.[AllowsPureUnit]),
				t.[EntryTypeParentId]		= IIF(t.[IsSystem]=0,s.[EntryTypeParentId],t.[EntryTypeParentId]),
				t.[DueDateLabel]			= s.[DueDateLabel],
				t.[DueDateLabel2]			= s.[DueDateLabel2],
				t.[DueDateLabel3]			= s.[DueDateLabel3],
				t.[Time1Label]				= s.[Time1Label],
				t.[Time1Label2]				= s.[Time1Label2],
				t.[Time1Label3]				= s.[Time1Label3],
				t.[Time2Label]				= s.[Time2Label],
				t.[Time2Label2]				= s.[Time2Label2],
				t.[Time2Label3]				= s.[Time2Label3],
				t.[ExternalReferenceLabel]	= s.[ExternalReferenceLabel],
				t.[ExternalReferenceLabel2]	= s.[ExternalReferenceLabel2],
				t.[ExternalReferenceLabel3]	= s.[ExternalReferenceLabel3],
				t.[AdditionalReferenceLabel]= s.[AdditionalReferenceLabel],
				t.[AdditionalReferenceLabel2]= s.[AdditionalReferenceLabel2],
				t.[AdditionalReferenceLabel3]= s.[AdditionalReferenceLabel3],
				t.[NotedAgentNameLabel]		= s.[NotedAgentNameLabel],
				t.[NotedAgentNameLabel2]	= s.[NotedAgentNameLabel2],
				t.[NotedAgentNameLabel3]	= s.[NotedAgentNameLabel3],
				t.[NotedAmountLabel]		= s.[NotedAmountLabel],
				t.[NotedAmountLabel2]		= s.[NotedAmountLabel2],
				t.[NotedAmountLabel3]		= s.[NotedAmountLabel3],
				t.[NotedDateLabel]			= s.[NotedDateLabel],
				t.[NotedDateLabel2]			= s.[NotedDateLabel2],
				t.[NotedDateLabel3]			= s.[NotedDateLabel3],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([ParentId],[Code],[Concept],
					[Name],[Name2],[Name3],
					[Description],	[Description2], [Description3],
					[Node],
					[IsMonetary],
					[IsAssignable],
					[AllowsPureUnit],
					[EntryTypeParentId],
					[DueDateLabel],
					[DueDateLabel2],
					[DueDateLabel3],
					[Time1Label],
					[Time1Label2],
					[Time1Label3],
					[Time2Label],
					[Time2Label2],
					[Time2Label3],
					[ExternalReferenceLabel],
					[ExternalReferenceLabel2],
					[ExternalReferenceLabel3],
					[AdditionalReferenceLabel],
					[AdditionalReferenceLabel2],
					[AdditionalReferenceLabel3],
					[NotedAgentNameLabel],
					[NotedAgentNameLabel2],
					[NotedAgentNameLabel3],
					[NotedAmountLabel],
					[NotedAmountLabel2],
					[NotedAmountLabel3],
					[NotedDateLabel],
					[NotedDateLabel2],
					[NotedDateLabel3]
					)
			VALUES (s.[ParentId],s.[Code],s.[Concept],
					s.[Name], s.[Name2], s.[Name3],
					s.[Description], s.[Description2], s.[Description3],
					s.[Node],
					s.[IsMonetary],
					s.[IsAssignable],
					s.[AllowsPureUnit],
					s.[EntryTypeParentId],
					s.[DueDateLabel],
					s.[DueDateLabel2],
					s.[DueDateLabel3],
					s.[Time1Label],
					s.[Time1Label2],
					s.[Time1Label3],
					s.[Time2Label],
					s.[Time2Label2],
					s.[Time2Label3],
					s.[ExternalReferenceLabel],
					s.[ExternalReferenceLabel2],
					s.[ExternalReferenceLabel3],
					s.[AdditionalReferenceLabel],
					s.[AdditionalReferenceLabel2],
					s.[AdditionalReferenceLabel3],
					s.[NotedAgentNameLabel],
					s.[NotedAgentNameLabel2],
					s.[NotedAgentNameLabel3],
					s.[NotedAmountLabel],
					s.[NotedAmountLabel2],
					s.[NotedAmountLabel3],
					s.[NotedDateLabel],
					s.[NotedDateLabel2],
					s.[NotedDateLabel3]
					)
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

	-- AccountTypeResourceDefinitions
	WITH BEATRD AS (
		SELECT * FROM dbo.[AccountTypeResourceDefinitions]
		WHERE [AccountTypeId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BEATRD AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [AccountTypeId], L.[ResourceDefinitionId]
		FROM @AccountTypeResourceDefinitions L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[ResourceDefinitionId]		= s.[ResourceDefinitionId], 
			t.[SavedById]					= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([AccountTypeId],	[ResourceDefinitionId])
		VALUES (s.[AccountTypeId], s.[ResourceDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- AccountTypeContractDefinitions
	WITH BEATCD AS (
		SELECT * FROM dbo.[AccountTypeContractDefinitions]
		WHERE [AccountTypeId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BEATCD AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [AccountTypeId], L.[ContractDefinitionId]
		FROM @AccountTypeContractDefinitions L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[ContractDefinitionId]		= s.[ContractDefinitionId], 
			t.[SavedById]					= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([AccountTypeId],	[ContractDefinitionId])
		VALUES (s.[AccountTypeId], s.[ContractDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- AccountTypeNotedContractDefinitions
	WITH BEATNCD AS (
		SELECT * FROM dbo.[AccountTypeNotedContractDefinitions]
		WHERE [AccountTypeId] IN (SELECT [Id] FROM @IndexedIds)
	)
	MERGE INTO BEATNCD AS t
	USING (
		SELECT L.[Index], L.[Id], H.[Id] AS [AccountTypeId], L.[NotedContractDefinitionId]
		FROM @AccountTypeNotedContractDefinitions L
		JOIN @IndexedIds H ON L.[HeaderIndex] = H.[Index]
	) AS s ON t.Id = s.Id
	WHEN MATCHED THEN
		UPDATE SET 
			t.[NotedContractDefinitionId]		= s.[NotedContractDefinitionId], 
			t.[SavedById]					= @UserId
	WHEN NOT MATCHED THEN
		INSERT ([AccountTypeId],	[NotedContractDefinitionId])
		VALUES (s.[AccountTypeId], s.[NotedContractDefinitionId])
	WHEN NOT MATCHED BY SOURCE THEN
		DELETE;

	-- The following code is needed for bulk import, when the reliance is on Parent Index
	MERGE [dbo].[AccountTypes] As t
	USING (
		SELECT II.[Id], IIParent.[Id] As ParentId
		FROM @Entities O
		JOIN @IndexedIds IIParent ON IIParent.[Index] = O.ParentIndex
		JOIN @IndexedIds II ON II.[Index] = O.[Index]
	) As s
	ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[ParentId] = s.[ParentId];

	-- reorganize the nodes
	WITH Children ([Id], [ParentId], [Num]) AS (
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E.[Code])
		FROM [dbo].[AccountTypes] E
		LEFT JOIN [dbo].[AccountTypes] E2 ON E.[ParentId] = E2.[Id]
	),
	Paths ([Node], [Id]) AS (  
		-- This section provides the value for the roots of the hierarchy  
		SELECT CAST(('/'  + CAST(C.Num AS VARCHAR(30)) + '/') AS HIERARCHYID) AS [Node], [Id]
		FROM Children AS C   
		WHERE [ParentId] IS NULL
		UNION ALL   
		-- This section provides values for all nodes except the root  
		SELECT CAST(P.[Node].ToString() + CAST(C.Num AS VARCHAR(30)) + '/' AS HIERARCHYID), C.[Id]
		FROM Children C
		JOIN Paths P ON C.[ParentId] = P.[Id]
	)
	MERGE INTO [dbo].[AccountTypes] As t
	USING Paths As s ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];

	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;