CREATE PROCEDURE [dal].[Currencies__Delete]
	@Ids [dbo].[IndexedStringList] READONLY
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM [dbo].[Currencies] 
	WHERE Id IN (SELECT Id FROM @Ids);

	UPDATE [dbo].[Settings] SET [SettingsVersion] = NEWID();
END