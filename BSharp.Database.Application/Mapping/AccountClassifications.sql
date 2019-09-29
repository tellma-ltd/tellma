CREATE FUNCTION [map].[AccountClassifications] ()
RETURNS TABLE AS 
RETURN (
	SELECT
		[AC].*,
		(SELECT [Id] FROM dbo.[AccountClassifications] WHERE [Node] = AC.[ParentNode]) AS ParentId,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[AccountClassifications] WHERE [IsDeprecated] = 0 AND [Node].IsDescendantOf([AC].[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[AccountClassifications] WHERE [Node].IsDescendantOf([AC].[Node]) = 1) As [ChildCount]
	FROM dbo.[AccountClassifications] AC
);