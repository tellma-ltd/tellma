CREATE FUNCTION [map].[ResourceClassifications] ()
RETURNS TABLE
AS
RETURN (
	SELECT Q.*,
	Q.[Node].GetLevel() AS [Level],
	(SELECT COUNT(*) FROM [dbo].[ResourceClassifications] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(Q.[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[ResourceClassifications] WHERE [Node].IsDescendantOf(Q.[Node]) = 1) As [ChildCount],
	Q.Node.ToString() AS [Path]
	FROM [dbo].[ResourceClassifications] Q
);
