CREATE FUNCTION [map].[AccountClassifications] ()
RETURNS TABLE AS 
RETURN (
	-- TODO, persist ChildCount and ActiveChildCount similar to Account Types, and expand this
	SELECT
		[AC].*,
		[Node].GetLevel() AS [Level],
		CC.[ActiveChildCount],
		CC.ChildCount
	FROM dbo.[AccountClassifications] AC
	CROSS APPLY (
		SELECT COUNT(*) AS [ChildCount],
		SUM(IIF([IsActive]=1,1,0)) AS  [ActiveChildCount]	
		FROM [dbo].[AccountClassifications]
		WHERE [Node].IsDescendantOf(AC.[Node]) = 1
	) CC 
);