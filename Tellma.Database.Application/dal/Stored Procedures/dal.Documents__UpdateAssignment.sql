CREATE PROCEDURE [dal].[Documents__UpdateAssignment]
	@AssignmentId INT,
	@Comment NVARCHAR(1024) = NULL,
	@UserId INT,
	@DocumentId INT OUTPUT
AS
BEGIN
	DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();
	DECLARE @CreatedAt DATETIMEOFFSET(7);
	DECLARE @AssigneeId INT;

	-- Update the history table and grab the docment Id and the assignment CreatedAt
	UPDATE [dbo].[DocumentAssignmentsHistory]
	SET [Comment] = @Comment, [ModifiedAt] = @Now, @DocumentId = [DocumentId], @CreatedAt = [CreatedAt]
	WHERE [Id] = @AssignmentId;

	-- Update it in [DocumentAssignments] too if CreatedAt matches the one from the history table. And grab the assigneeId
	UPDATE [dbo].[DocumentAssignments]
	SET [Comment] = @Comment, [ModifiedAt] = @Now, @AssigneeId = [AssigneeId]
	WHERE [DocumentId] = @DocumentId AND [CreatedAt] = @CreatedAt;

	-- Put the assigneeId (if any) in a singleton
	DECLARE @AffectedUsers [dbo].[IdList];
	IF @AssigneeId IS NOT NULL
		INSERT INTO @AffectedUsers ([Id]) VALUES (@AssigneeId);
		
	-- Return Notification info
	EXEC [dal].[InboxCounts__Load] @UserIds = @AffectedUsers;
END;
