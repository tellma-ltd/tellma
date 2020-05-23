CREATE FUNCTION [map].[AccountClassifications] ()
RETURNS TABLE AS 
RETURN (
	SELECT
		[AC].*,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[AccountClassifications] WHERE [IsActive] = 0 AND [Node].IsDescendantOf([AC].[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[AccountClassifications] WHERE [Node].IsDescendantOf([AC].[Node]) = 1) As [ChildCount]
	FROM dbo.[AccountClassifications] AC
);