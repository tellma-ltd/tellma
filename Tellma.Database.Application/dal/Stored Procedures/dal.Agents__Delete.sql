CREATE PROCEDURE [dal].[Agents__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	-- TODO restrict action to @DefinitionId
	
	-- So they can be removed from blob storage
	SELECT [ImageId] FROM [dbo].[Agents] 
	WHERE [ImageId] IS NOT NULL AND [Id] IN (SELECT [Id] FROM @Ids);
	
	-- So they can also be removed from blob storage
	SELECT [FileId] FROM [dbo].[AgentAttachments]
	WHERE [AgentId] IN (SELECT [Id] FROM @Ids);

	-- Finally delete
	DELETE FROM [dbo].[Agents] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;