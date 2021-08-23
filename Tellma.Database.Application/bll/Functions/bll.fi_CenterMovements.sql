CREATE FUNCTION [bll].[fi_CenterMovements]()
RETURNS TABLE AS RETURN
(
	WITH AllMovements AS (
	SELECT E.[RelationId] AS [EmployeeId], E.[Time1] AS [AsOf], E.[Direction],
	E.[CenterId],
	COALESCE(E.[Time2],
			DATEADD(DAY, -1, LEAD(E.[Time1]) OVER(PARTITION BY E.[RelationId] ORDER BY E.[Time1])),
			N'9999.12.31') AS [Till]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	WHERE AC.[Concept] = N'CostCenterAssignmentExtension'
	AND L.[State] = 4
	)
	SELECT [EmployeeId], [CenterId], [AsOf], [Till]
	FROM AllMovements WHERE [Direction] = 1
);