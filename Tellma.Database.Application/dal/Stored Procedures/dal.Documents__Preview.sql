﻿CREATE PROCEDURE [dal].[Documents__Preview]
	@DocumentId INT,
	@CreatedAt DATETIMEOFFSET(7),
	@OpenedAt DATETIMEOFFSET(7),
	@UserId INT
AS
BEGIN
	UPDATE [dbo].[DocumentAssignments] SET [OpenedAt] = @OpenedAt
	WHERE [DocumentId] = @DocumentId AND [CreatedAt] = @CreatedAt;

	UPDATE dbo.[DocumentAssignmentsHistory]
	SET OpenedAt = @OpenedAt
	WHERE [Id] = (
		SELECT MAX([Id]) FROM dbo.[DocumentAssignmentsHistory]
		WHERE DocumentId = @DocumentId
		AND AssigneeId = (
			SELECT [AssigneeId]
			FROM [dbo].[DocumentAssignments]
			WHERE [DocumentId] = @DocumentId
		)
	)

	-- Create a singleton containing the current user
	DECLARE @AffectedUsers [dbo].[IdList];
	INSERT INTO @AffectedUsers (Id) VALUES (@UserId)

	-- Return the new assignment counts for the current user
	EXEC [dal].[InboxCounts__Load] @UserIds = @AffectedUsers
END;