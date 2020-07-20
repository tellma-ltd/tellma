CREATE FUNCTION [map].[AccountTypes] ()
RETURNS TABLE
AS
RETURN (
	SELECT Q.*,
	[Node].GetAncestor(1)  AS [ParentNode],
	[Node].GetLevel() AS [Level],
	[Node].ToString() AS [Path],
	(SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(Q.[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [Node].IsDescendantOf(Q.[Node]) = 1) As [ChildCount]
	FROM [dbo].[AccountTypes] Q
);
