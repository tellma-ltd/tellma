CREATE PROCEDURE [dal].[Document_Comment__Save]
	@DocumentId INT,
	@Comment NVARCHAR(1024) = NULL
AS
BEGIN
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @Now DATETIMEOFFSET = SYSDATETIMEOFFSET();

	UPDATE dbo.DocumentAssignments
	SET Comment = @Comment, [CreatedAt] = @Now, [CreatedById] = @UserId
	WHERE DocumentId = @DocumentId

	INSERT dbo.DocumentAssignmentsHistory([DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById])
	SELECT [DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById]
	FROM dbo.DocumentAssignments
	WHERE DocumentId = @DocumentId;
END