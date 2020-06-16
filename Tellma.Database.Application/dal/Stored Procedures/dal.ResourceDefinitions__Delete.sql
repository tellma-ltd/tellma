CREATE PROCEDURE [dal].[ResourceDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE [dbo].[ResourceDefinitions]
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
	
	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();