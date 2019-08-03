CREATE PROCEDURE [dbo].[dal_Documents__Assign]
	@Documents [dbo].[UuidList] READONLY,
	@AssigneeId INT,
	@Comment NVARCHAR(1024)
AS
BEGIN
-- Assumption: When OpenedAt is updated in table DocumentAssignments, it is also updated in table
-- DocumentAssignmentsHistory
	INSERT dbo.DocumentAssignmentsHistory([DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById])
	SELECT [DocumentId], [AssigneeId], [Comment], [CreatedAt], [CreatedById]
	FROM dbo.DocumentAssignments
	WHERE DocumentId IN (SELECT [Id] FROM @Documents)

	DELETE dbo.DocumentAssignments
	WHERE DocumentId IN (SELECT [Id] FROM @Documents)

	INSERT dbo.DocumentAssignments([DocumentId], [AssigneeId], [Comment])
	SELECT [Id], @AssigneeId, @Comment
	FROM @Documents
END;