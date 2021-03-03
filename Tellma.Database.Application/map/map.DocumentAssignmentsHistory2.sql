CREATE FUNCTION [map].[DocumentAssignmentsHistory2]()
-- It is useful as FACT Table to check users efficiency
RETURNS TABLE
AS
RETURN (
	WITH Circulation AS (
		SELECT DAH.[DocumentId], DAH.[AssigneeId], DAH.[CreatedById] AS AssignedById,
		DAH.[CreatedAt] AS [Time1],
		ISNULL(
			LEAD(DAH.[CreatedAt]) OVER(PARTITION BY DAH.[DocumentId] ORDER BY DAH.[Id]),
			IIF(D.[State] = 0, GETDATE(), [StateAt])
		) AS [Time2]
		FROM dbo.DocumentAssignmentsHistory DAH
		LEFT JOIN dbo.Documents D ON DAH.[DocumentId] = D.[Id]
	)
	SELECT *,
	-- The following can probably be replaced by Tellma function
	DATEDIFF(SECOND, [Time1], [Time2])/86400.0 AS [DelayInDays]
	FROM Circulation
);