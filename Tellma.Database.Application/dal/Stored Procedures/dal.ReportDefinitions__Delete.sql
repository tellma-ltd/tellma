CREATE PROCEDURE [dal].[ReportDefinitions__Delete]
	@Ids [dbo].[StringList] READONLY
AS
	DELETE FROM [dbo].[ReportDefinitions] 
	WHERE Id IN (SELECT Id FROM @Ids);

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();