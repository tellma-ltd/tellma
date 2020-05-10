CREATE FUNCTION [map].[Documents]()
RETURNS TABLE
AS
RETURN (
	SELECT D.*,
	A.[Comment], A.[AssigneeId], A.[CreatedAt] AS [AssignedAt], A.[CreatedById] AS [AssignedById], A.[OpenedAt]
	FROM [dbo].[Documents] D
	LEFT JOIN [dbo].[DocumentAssignments] A ON D.[Id] = A.[DocumentId]
);
