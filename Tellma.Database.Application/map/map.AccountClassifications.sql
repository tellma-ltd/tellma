CREATE FUNCTION [map].[AccountClassifications] ()
RETURNS TABLE AS 
RETURN (
	SELECT
		[AC].*, ~[AC].[IsDeprecated] AS [IsActive],
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[AccountClassifications] WHERE [IsDeprecated] = 0 AND [Node].IsDescendantOf([AC].[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[AccountClassifications] WHERE [Node].IsDescendantOf([AC].[Node]) = 1) As [ChildCount]
	FROM dbo.[AccountClassifications] AC
);