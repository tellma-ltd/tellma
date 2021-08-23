CREATE PROCEDURE [dal].[IdentityServerClients__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;

	-- Delete
	DELETE FROM [dbo].[IdentityServerClients] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END
