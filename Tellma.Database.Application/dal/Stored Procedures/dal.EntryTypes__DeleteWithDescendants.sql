CREATE PROCEDURE [dal].[EntryTypes__DeleteWithDescendants]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;
	IF NOT EXISTS(SELECT * FROM @Ids) RETURN;

	-- Delete the entites and their children
	WITH EntitiesWithDescendants
	AS (
		SELECT T2.[Id]
		FROM [dbo].[EntryTypes] T1
		JOIN [dbo].[EntryTypes] T2
		ON T2.[Node].IsDescendantOf(T1.[Node]) = 1
		WHERE T1.[Id] IN (SELECT [Id] FROM @Ids)
	)
	DELETE FROM [dbo].[EntryTypes]
	WHERE [Id] IN (SELECT [Id] FROM EntitiesWithDescendants);
END;
