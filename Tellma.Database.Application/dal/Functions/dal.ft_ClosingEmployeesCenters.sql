CREATE FUNCTION [dal].[ft_ClosingEmployeesCenters] (
	@MonthEnding DATE
)
RETURNS @returntable TABLE
(
	[EmployeeId]	INT,
	[AsOf]			DATE,
	[CenterId]		INT,
	[CenterNode]	HIERARCHYID
)
AS
BEGIN
	INSERT @returntable
	SELECT E.[AgentId] AS [EmployeeId], E.[Time1] AS AsOf, E.[CenterId], C.[Node]
	FROM dbo.Entries E
	JOIN dbo.Centers C ON C.[Id] = E.[CenterId]
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	WHERE AC.[Concept] = N'HRExtension'
	AND L.[State] = 2
	AND E.[Time1] <= @MonthEnding
	AND (C.[IsLeaf] = 1 AND C.[CenterType] <> 'Sale')
	GROUP BY E.[AgentId], E.[Time1], E.[CenterId], C.[Node]
	HAVING SUM(E.[Direction] * E.[Quantity]) <> 0
	RETURN
END