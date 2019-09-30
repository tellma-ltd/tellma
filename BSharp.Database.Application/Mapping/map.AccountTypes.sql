CREATE FUNCTION [map].[AccountTypes]()
RETURNS TABLE AS 
RETURN (
	SELECT
		[AC].*,
		(SELECT [Id] FROM dbo.[AccountTypes] WHERE [Node] = AC.[ParentNode]) AS ParentId,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [IsActive] = 1 AND [Node].IsDescendantOf([AC].[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[AccountTypes] WHERE [Node].IsDescendantOf([AC].[Node]) = 1) As [ChildCount]
	FROM dbo.[AccountTypes] AC
);