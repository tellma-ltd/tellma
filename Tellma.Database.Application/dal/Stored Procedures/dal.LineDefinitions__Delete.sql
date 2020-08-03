CREATE PROCEDURE [dal].[LineDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[LineDefinitions] 
	WHERE Id IN (SELECT Id FROM @Ids);
	
	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID()