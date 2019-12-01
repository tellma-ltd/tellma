CREATE FUNCTION [map].[ResourceClassifications] ()
RETURNS TABLE
AS
RETURN (
	SELECT RC.*,
	(SELECT [Code] FROM dbo.[ResourceClassifications] WHERE [Node] = RC.[ParentNode]) AS ParentId,
	RC.[Node].GetLevel() AS [Level],
	(SELECT COUNT(*) FROM [dbo].[ResourceClassifications] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(RC.[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[ResourceClassifications] WHERE [Node].IsDescendantOf(RC.[Node]) = 1) As [ChildCount]
	FROM [dbo].[ResourceClassifications] RC
);
