CREATE FUNCTION [map].[AccountTypes] ()
RETURNS TABLE
AS
RETURN (
	SELECT Q.*,
	(SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(Q.[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [Node].IsDescendantOf(Q.[Node]) = 1) As [ChildCount]
	FROM [dbo].[AccountTypes] Q
);
