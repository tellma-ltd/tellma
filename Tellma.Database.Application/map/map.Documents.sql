CREATE FUNCTION [map].[Documents]()
RETURNS TABLE
AS
RETURN (
	SELECT D.*, IIF(DB.Balance = 0, 1, 0) AS IsBalanced,
	A.[Comment], A.[AssigneeId], A.[CreatedAt] AS [AssignedAt], A.[CreatedById] AS [AssignedById], A.[OpenedAt]
	FROM [dbo].[Documents] D
	LEFT JOIN [dbo].[DocumentAssignments] A ON D.[Id] = A.[DocumentId]
	LEFT JOIN (
		SELECT L.[DocumentId], SUM(E.[Direction] * E.[Value]) AS Balance
		FROM dbo.Lines L
		JOIN dbo.Entries E ON L.[Id] = E.[LineId]
		WHERE (L.[State] = 4)
		GROUP BY L.[DocumentId]
	) DB ON D.[Id] = DB.[DocumentId]
);
