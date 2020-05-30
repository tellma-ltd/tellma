CREATE FUNCTION [map].[Documents]()
RETURNS TABLE
AS
RETURN (
	SELECT D.*, DD.[DocumentType], 
	IIF (DD.[DocumentType] = 2, 4, 2) AS [LastLineState],
	A.[Comment], A.[AssigneeId], A.[CreatedAt] AS [AssignedAt], A.[CreatedById] AS [AssignedById], A.[OpenedAt]
	FROM [dbo].[Documents] D
	JOIN [dbo].[DocumentDefinitions] DD ON D.[DefinitionId] = DD.[Id]
	LEFT JOIN [dbo].[DocumentAssignments] A ON D.[Id] = A.[DocumentId]
);
