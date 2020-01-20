CREATE PROCEDURE [dal].[AccountTypes__Save]
	@Entities [AccountTypeList] READONLY,
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
				E.[Index], E.[Id], E.[ParentId],
				-- TODO: use index and last node number to add a tree that is pre-sorted
				hierarchyid::Parse('/' + CAST(-ABS(CHECKSUM(NewId()) % 2147483648) AS VARCHAR(30)) + '/') AS [Node],
				E.[Name], E.[Name2], E.[Name3], E.[Description], E.[Description2], E.[Description3], E.[Code], E.[IsAssignable],
				E.[IsCurrent],
				E.[IsReal],
				E.[IsResourceClassification],
				E.[IsPersonal],
				E.[EntryTypeParentId]
			FROM @Entities E
		) AS s ON (t.[Code] = s.[Code])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[ParentId]				= s.[ParentId],
				t.[Name]					= s.[Name],
				t.[Name2]					= s.[Name2],
				t.[Name3]					= s.[Name3],
				t.[Description]				= s.[Description],
				t.[Description2]			= s.[Description2],
				t.[Description3]			= s.[Description3],
				t.[Code]					= s.[Code],
				t.[IsAssignable]			= s.[IsAssignable],
				t.[IsCurrent]				= s.[IsCurrent],
				t.[IsReal]					= s.[IsReal],
				t.[IsResourceClassification]= s.[IsResourceClassification],
				t.[IsPersonal]				= s.[IsPersonal],
				t.[EntryTypeParentId]		= s.[EntryTypeParentId],
				t.[ModifiedAt]				= @Now,
				t.[ModifiedById]			= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([ParentId],		[Name],		[Name2], [Name3], [Description], [Description2], [Description3],		[Code],	[Node],	[IsAssignable],
					[IsCurrent],
					[IsReal],
					[IsResourceClassification],
					[IsPersonal],
					[EntryTypeParentId])
			VALUES (s.[ParentId], s.[Name], s.[Name2], s.[Name3], s.[Description], s.[Description2], s.[Description3], s.[Code], s.[Node],  s.[IsAssignable],
					s.[IsCurrent],
					s.[IsReal],
					s.[IsResourceClassification],
					s.[IsPersonal],
					s.[EntryTypeParentId])
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

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
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])
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

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;