CREATE PROCEDURE [dal].[Centers__Save]
	@Entities [CenterList] READONLY,
	@ReturnIds BIT = 0,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @BeforeBuCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1);

	DECLARE @IndexedIds [dbo].[IndexedIdList];
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	INSERT INTO @IndexedIds([Index], [Id])
	SELECT x.[Index], x.[Id]
	FROM
	(
		MERGE INTO [dbo].[Centers] AS t
		USING (
			SELECT
				E.[Index], E.[Id],
				E.[ParentId], [CenterType],
				hierarchyid::Parse('/' + CAST(-ABS(CHECKSUM(NewId()) % 2147483648) AS VARCHAR(30)) + '/') AS [Node],
				E.[Name], E.[Name2], E.[Name3], E.[Code]
			FROM @Entities E
		) AS s ON (t.[Id] = s.[Id])
		WHEN MATCHED 
		THEN
			UPDATE SET
				t.[ParentId]			= s.[ParentId],
				t.[CenterType]			= s.[CenterType],
				t.[Name]				= s.[Name],
				t.[Name2]				= s.[Name2],
				t.[Name3]				= s.[Name3],
				t.[Code]				= s.[Code],
				t.[ModifiedAt]			= @Now,
				t.[ModifiedById]		= @UserId
		WHEN NOT MATCHED THEN
			INSERT ([ParentId],	[CenterType], [Node], [Name], [Name2], [Name3], [Code], [CreatedById], [CreatedAt], [ModifiedById], [ModifiedAt])
			VALUES (s.[ParentId],s.[CenterType],s.[Node],s.[Name],s.[Name2],s.[Name3],s.[Code], @UserId, @Now, @UserId, @Now)
			OUTPUT s.[Index], inserted.[Id] 
	) As x;

	-- The following code is needed for bulk import, when the reliance is on Parent Index
	MERGE [dbo].[Centers] As t
	USING (
		SELECT II.[Id], IIParent.[Id] As [ParentId]
		FROM @Entities O
		JOIN @IndexedIds IIParent ON IIParent.[Index] = O.[ParentIndex]
		JOIN @IndexedIds II ON II.[Index] = O.[Index]
	) As s
	ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[ParentId] = s.[ParentId];

	-- reorganize the nodes
	WITH Children ([Id], [ParentId], [Num]) AS (
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E.[Code])
		FROM [dbo].[Centers] E
		LEFT JOIN [dbo].[Centers] E2 ON E.[ParentId] = E2.[Id]
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
	MERGE INTO [dbo].[Centers] As t
	USING Paths As s ON (t.[Id] = s.[Id])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
	
	-- Whether there are multiple active business units is an important cached value of the settings
	DECLARE @AfterBuCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [CenterType] = N'BusinessUnit' AND [IsActive] = 1);
	
	-- BUG: filling Maximus centers did not update the business unit count
	IF (@BeforeBuCount <= 1 AND @AfterBuCount > 1) OR (@BeforeBuCount > 1 AND @AfterBuCount <= 1)
		UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();

	IF @ReturnIds = 1
		SELECT * FROM @IndexedIds;
END;