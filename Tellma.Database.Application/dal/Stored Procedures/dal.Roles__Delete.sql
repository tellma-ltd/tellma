CREATE PROCEDURE [dal].[Roles__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DELETE FROM [dbo].[Roles] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
	
	UPDATE [dbo].[Users] SET [PermissionsVersion] = NEWID();
END;