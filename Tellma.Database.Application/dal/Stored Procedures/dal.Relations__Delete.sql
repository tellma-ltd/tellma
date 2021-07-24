CREATE PROCEDURE [dal].[Relations__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	-- So they can be removed from blob storage
	SELECT [ImageId] FROM [dbo].[Relations] 
	WHERE [ImageId] IS NOT NULL AND [Id] IN (SELECT [Id] FROM @Ids);
	
	-- So they can also be removed from blob storage
	SELECT [FileId] FROM [dbo].[RelationAttachments]
	WHERE [RelationId] IN (SELECT [Id] FROM @Ids);

	-- Finally delete
	DELETE FROM [dbo].[Relations] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids) AND [DefinitionId] = @DefinitionId;
END;