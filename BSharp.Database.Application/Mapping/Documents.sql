CREATE FUNCTION [map].[Documents]()
RETURNS TABLE
AS
RETURN (
	SELECT [D].*, [A].[Comment], [A].[AssigneeId], [A].[CreatedAt] AS [AssignedAt], [A].[CreatedById] AS [AssignedById], [A].[OpenedAt] FROM [dbo].[Documents] AS [D]
	LEFT JOIN [dbo].[DocumentAssignments] AS [A]
	ON [D].[Id] = [A].[DocumentId]
);
