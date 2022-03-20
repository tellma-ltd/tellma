CREATE PROCEDURE [dal].[EmailTemplates__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[EmailTemplates] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
	
	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();
END;