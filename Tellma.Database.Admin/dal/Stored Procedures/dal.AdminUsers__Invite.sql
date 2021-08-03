CREATE PROCEDURE [dal].[AdminUsers__Invite]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE [dbo].[AdminUsers]
	SET [InvitedAt] = @Now 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	-- Return information about the invited users
	SELECT [Id], [Email], [Name]
	FROM [dbo].[AdminUsers] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;