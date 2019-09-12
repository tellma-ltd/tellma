CREATE FUNCTION [dbo].[fi_MyWorkspace] ()
RETURNS TABLE
AS
RETURN
	SELECT A.Comment, A.[CreatedById], A.[CreatedAt], D.[DocumentDefinitionId], D.SerialNumber
	FROM [dbo].Documents D
	JOIN dbo.[DocumentAssignments] A ON A.DocumentId = D.Id
	WHERE A.AssigneeId = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
