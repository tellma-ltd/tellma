CREATE FUNCTION [map].[Inbox]()
RETURNS TABLE
AS
RETURN (
	SELECT [DocumentId] AS [Id], * FROM [dbo].[DocumentAssignments]
	WHERE [AssigneeId] = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
);
