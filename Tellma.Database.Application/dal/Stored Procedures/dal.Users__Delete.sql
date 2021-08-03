CREATE PROCEDURE [dal].[Users__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
BEGIN
	SET NOCOUNT ON;

	-- First Return the image ids of the deleted users
	SELECT [ImageId] FROM [dbo].[Users] 
	WHERE [ImageId] IS NOT NULL AND [Id] IN (SELECT [Id] FROM @Ids);

	DELETE FROM [dbo].[Users] 
	OUTPUT DELETED.[Email] -- Returns the deleted emails
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;
