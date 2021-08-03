CREATE PROCEDURE [dal].[Users__Invite]
	@Ids [dbo].[IndexedIdList] READONLY,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE [dbo].[Users]
	SET [InvitedAt] = @Now 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);

	-- Return information about the invited users
	SELECT [Id], [Email], [Name], [Name2], [Name3], [PreferredLanguage]
	FROM [dbo].[Users] 
	WHERE [Id] IN (SELECT [Id] FROM @Ids);
END;