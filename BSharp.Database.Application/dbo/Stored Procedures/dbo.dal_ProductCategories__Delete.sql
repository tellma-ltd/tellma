CREATE PROCEDURE [dbo].[dal_ProductCategories__Delete]
	@Entities [IdList] READONLY
AS
	IF NOT EXISTS(SELECT * FROM @Entities) RETURN;

	-- Delete the entites, after setting Parent Id of children to NULL
	UPDATE [dbo].[ProductCategories]
	SET [ParentId] = NULL
	WHERE [ParentId] IN (SELECT [Id] FROM @Entities);

	DELETE FROM [dbo].[ProductCategories]
	WHERE [Id] IN (SELECT [Id] FROM @Entities);

	-- reorganize the nodes
	WITH Children ([Id], [ParentId], [Num]) AS (
		SELECT E.[Id], E2.[Id] As ParentId, ROW_NUMBER() OVER (PARTITION BY E2.[Id] ORDER BY E2.[Id])
		FROM [dbo].[ProductCategories] E
		LEFT JOIN [dbo].[ProductCategories] E2 ON E.[ParentId] = E2.[Id]
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
	MERGE INTO [dbo].[ProductCategories] As t
	USING Paths As s ON (t.[Id] = s.[Id] AND t.[Node] <> s.[Node])
	WHEN MATCHED THEN UPDATE SET t.[Node] = s.[Node];
