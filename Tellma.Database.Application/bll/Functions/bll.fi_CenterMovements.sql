CREATE FUNCTION [bll].[fi_CenterMovements]()
RETURNS TABLE AS RETURN
(
	WITH AllMovements AS (
	SELECT E.[AgentId] AS [EmployeeId], E.[Time1] AS [AsOf], E.[Direction],
	E.[CenterId],
	COALESCE(E.[Time2],
			DATEADD(DAY, -1, LEAD(E.[Time1]) OVER(PARTITION BY E.[AgentId] ORDER BY E.[Time1])),
			N'9999.12.31') AS [Till]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	-- WHERE AC.[Concept] = N'HRMExtension'
	-- AND ET.[Concept] IN (N'EmployeeCheckInExtension',N'EmployeeTransferExtension',N'EmployeeCheckOutExtension')
	WHERE AC.[Concept] = N'CostCenterAssignmentExtension'
	AND L.[State] = 4
	AND [Direction] = 1
	)
	SELECT [EmployeeId], [CenterId], [AsOf], [Till]
	FROM AllMovements
)