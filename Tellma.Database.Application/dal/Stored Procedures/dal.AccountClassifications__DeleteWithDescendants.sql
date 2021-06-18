CREATE PROCEDURE [dal].[AccountClassifications__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	IF NOT EXISTS(SELECT * FROM @Ids) RETURN;

	-- Delete the entites and their children
	WITH EntitiesWithDescendants
	AS (
		SELECT T2.[Id]
		FROM [dbo].[AccountClassifications] T1
		JOIN [dbo].[AccountClassifications] T2
		ON T2.[Node].IsDescendantOf(T1.[Node]) = 1
		WHERE T1.[Id] IN (SELECT [Id] FROM @Ids)
	)
	DELETE FROM [dbo].[AccountClassifications]
	WHERE [Id] IN (SELECT [Id] FROM EntitiesWithDescendants);
END;
