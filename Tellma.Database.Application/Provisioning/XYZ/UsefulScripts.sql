-- For auto-generating salaries:

DECLARE @GenerateArguments GenerateArgumentList;
INSERT INTO @GenerateArguments([Key], [Value]) VALUES
(N'MonthEnding', N'2021.07.24'),
(N'CenterId', N'2');

DECLARE @MonthEnding DATE = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N'MonthEnding') AS DATE);
DECLARE @CenterId INT = CAST((SELECT [Value] FROM @GenerateArguments WHERE [Key] = N'CenterId') AS INT);

DECLARE @CenterNode HIERARCHYID = (SELECT [Node] FROM dbo.Centers WHERE [Id] = @CenterId);
DECLARE @WideLines WideLineList;

WITH ActiveEmployees AS (
	SELECT E.[AgentId] AS [EmployeeId], E.[CenterId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	WHERE AC.[Concept] = N'CostCenterAssignmentExtension' -- HRM
	AND L.[State] = 4
	AND E.[ResourceId] IS NULL
	AND E.[Time1] <= @MonthEnding
	GROUP BY E.[AgentId], E.[CenterId]
	HAVING SUM([Direction] * [Quantity]) <> 0
)
INSERT INTO @WideLines([Index], [DocumentIndex], [PostingDate], [AgentId0])
SELECT ROW_NUMBER() OVER (Order By EmployeeId) - 1 AS [Index], 0 AS [DocumentIndex], @MonthEnding AS [PostingDate], EmployeeId
FROM ActiveEmployees E
JOIN dbo.Centers C ON E.[CenterId] = C.[Id]
WHERE (@CenterId IS NULL OR C.[Node].IsDescendantOf(@CenterNode) = 1);

SELECT * FROM @WideLines;
GO