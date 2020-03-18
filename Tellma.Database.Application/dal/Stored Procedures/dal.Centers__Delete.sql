CREATE PROCEDURE [dal].[Centers__Delete]
	@Ids [dbo].[IdList] READONLY
AS
SET NOCOUNT ON;
	DECLARE @BeforeCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE IsLeaf = 1 AND IsActive = 1);
	DELETE [dbo].[Centers] WHERE [Id] IN (SELECT [Id] FROM @Ids);
		
	-- reorganize the nodes
	WITH Children ([Id], [ParentId], [Num]) AS (
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])
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
	USING Paths As s ON (t.[Id] = s.[Id] AND t.[Node] <> s.[Node])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
	
	-- Whether there are multiple centers is an important settings value
	DECLARE @AfterCount INT = (SELECT COUNT(*) FROM [dbo].[Centers] WHERE IsLeaf = 1 AND IsActive = 1);
	IF (@BeforeCount <= 1 AND @AfterCount > 1) OR (@BeforeCount > 1 AND @AfterCount <= 1) 
		UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();