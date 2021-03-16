CREATE PROCEDURE [dal].[DashboardDefinitions__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].[DashboardDefinitions] 
	WHERE Id IN (SELECT Id FROM @Ids);

	-- Signal clients to refresh their cache
	UPDATE [dbo].[Settings] SET [DefinitionsVersion] = NEWID();