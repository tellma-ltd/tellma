CREATE TABLE [dbo].[AccountTypePaths] (
	[ChildId] INT,
	[ParentId] INT PRIMARY KEY ([ChildId], [ParentId])
)
/*
INSERT INTO dbo.AccountTypePaths([ChildId], [ParentId])
SELECT DISTINCT T1.[Id] As [ChildId], T2.[Id] AS [ParentId]
FROM dbo.AccountTypes T1
JOIN dbo.AccountTypes T2
ON T1.[Node].IsDescendantOf(T2.[Node]) = 1 AND T1.[Node] <> T2.[Node]
*/