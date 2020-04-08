CREATE PROCEDURE [dal].[Documents__Open]
	@DocumentId INT,
	@CreatedAt DATETIMEOFFSET(7),
	@OpenedAt DATETIMEOFFSET(7)
AS
BEGIN
	UPDATE [dbo].[DocumentAssignments] SET [OpenedAt] = @OpenedAt
	WHERE [DocumentId] = @DocumentId AND [CreatedAt] = @CreatedAt;

	-- Create a singleton containing the current user
	DECLARE @AffectedUsers [dbo].[IdList];
	INSERT INTO @AffectedUsers (Id) VALUES (CONVERT(INT, SESSION_CONTEXT(N'UserId')))

	-- Return the new assignment counts for the current user
	EXEC [dal].[InboxCounts__Load] @UserIds = @AffectedUsers
END;