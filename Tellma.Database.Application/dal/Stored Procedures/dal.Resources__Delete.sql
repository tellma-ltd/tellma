CREATE PROCEDURE [dal].[Resources__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	-- So they can be removed from blob storage
	SELECT [ImageId] FROM [dbo].[Resources] 
	WHERE [ImageId] IS NOT NULL AND [Id] IN (SELECT [Id] FROM @Ids) AND [DefinitionId] = @DefinitionId;

	-- Delete resources
	DELETE FROM [dbo].[Resources]
	WHERE [Id] IN (SELECT [Id] FROM @Ids) AND [DefinitionId] = @DefinitionId;
END;