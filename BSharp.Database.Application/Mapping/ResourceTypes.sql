CREATE FUNCTION [map].[ResourceTypes]()
RETURNS TABLE AS 
RETURN (
	SELECT
		RT.*,
		(SELECT [Id] FROM dbo.[ResourceTypes] WHERE [Node] = RT.[ParentNode]) AS ParentId,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[ResourceTypes] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(RT.[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[ResourceTypes] WHERE [Node].IsDescendantOf(RT.[Node]) = 1) As [ChildCount]
	FROM dbo.[ResourceTypes] RT
);