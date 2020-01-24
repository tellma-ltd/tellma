CREATE PROCEDURE [dal].[ResponsibilityCenters__Save]
	@Entities [ResponsibilityCenterList] READONLY,
	@ReturnIds BIT = 0
AS
SET NOCOUNT ON;
	DECLARE @BeforeCount INT = (SELECT COUNT(*) FROM [dbo].[ResponsibilityCenters] WHERE IsLeaf = 1 AND IsActive = 1);

	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[ResponsibilityCenters] AS t
		USING (
			SELECT
				--E.[Index], E.[Id], E.[ParentId], [IsOperatingSegment], [ManagerId], [ResponsibilityType],
				E.[Index], E.[Id], E.[ParentId], [ManagerId], [ResponsibilityType],
				hierarchyid::Parse('/' + CAST(-ABS(CHECKSUM(NewId()) % 2147483648) AS VARCHAR(30)) + '/') AS [Node],
				E.[Name], E.[Name2], E.[Name3], E.[Code], E.[IsLeaf]
			FROM @Entities E
		) AS s ON (t.Id = s.Id)
		WHEN MATCHED 
		THEN
			UPDATE SET
				--t.[IsOperatingSegment]	= s.[IsOperatingSegment],
				t.[ManagerId]			= s.[ManagerId],
				t.[ParentId]			= s.[ParentId],
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[Code]				= s.[Code],
				t.[IsLeaf]				= s.[IsLeaf],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([ResponsibilityType], [ParentId], [Node], [Name],	[Name2], [Name3], [Code], [IsLeaf], [ManagerId])
			VALUES (s.[ResponsibilityType], s.[Parentid], s.[Node], s.[Name], s.[Name2], s.[Name3], s.[Code], s.[IsLeaf], s.[ManagerId])
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

	-- The following code is needed for bulk import, when the reliance is on Parent Index
	MERGE [dbo].[ResponsibilityCenters] As t
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
		FROM [dbo].[ResponsibilityCenters] E
		LEFT JOIN [dbo].[ResponsibilityCenters] E2 ON E.[ParentId] = E2.[Id]
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
	MERGE INTO [dbo].[ResponsibilityCenters] As t
	USING Paths As s ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];

	-- Whether there are multiple responsibility centers is an important settings value
	DECLARE @AfterCount INT = (SELECT COUNT(*) FROM [dbo].[ResponsibilityCenters] WHERE IsLeaf = 1 AND IsActive = 1);
	IF (@BeforeCount <= 1 AND @AfterCount > 1) OR (@BeforeCount > 1 AND @AfterCount <= 1) 
		UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;