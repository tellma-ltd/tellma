CREATE FUNCTION [dbo].[fi_Document__Assignments] (
	@DocumentId INT
)
RETURNS TABLE
AS
RETURN
	SELECT 	[AssigneeId], [Comment], [CreatedById], [CreatedAt], [OpenedAt]
	FROM [dbo].[DocumentAssignments]
	WHERE DocumentId = @DocumentId;