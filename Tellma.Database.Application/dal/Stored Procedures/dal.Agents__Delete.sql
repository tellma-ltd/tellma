CREATE PROCEDURE [dal].[Agents__Delete]
	@DefinitionId INT,
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;
	
	-- So they can be removed from blob storage
	SELECT [ImageId] FROM [dbo].[Agents] 
	WHERE [ImageId] IS NOT NULL AND [Id] IN (SELECT [Id] FROM @Ids)
	AND [DefinitionId] = @DefinitionId; -- MA: Added 2022.05.03
	
	-- So they can also be removed from blob storage
	SELECT [FileId] FROM [dbo].[AgentAttachments]
	WHERE [AgentId] IN (SELECT [Id] FROM @Ids)
	AND [AgentId] IN (
		SELECT [Id] FROM dbo.Agents WHERE [DefinitionId] = @DefinitionId
	); -- MA: Added 2022.05.03, to restrict action to @Definition Id

	-- Finally delete
	DELETE FROM [dbo].[Agents] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids)
	AND [DefinitionId] = @DefinitionId; -- MA: Added 2022.05.03
END;