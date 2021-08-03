CREATE PROCEDURE [dal].[Inbox__Check]
	@Now DATETIMEOFFSET(7),
	@UserId INT
AS
BEGIN
	SET NOCOUNT OFF;

	UPDATE [dbo].[Users] SET [LastInboxCheck] = @Now
	WHERE [Id] = @UserId;

	-- Create a singleton containing the current user
	DECLARE @AffectedUsers [dbo].[IdList];
	INSERT INTO @AffectedUsers (Id) VALUES (@UserId);

	-- Return the new assignment counts for the current user
	EXEC [dal].[InboxCounts__Load] @UserIds = @AffectedUsers;
END;