CREATE FUNCTION [map].[LegacyClassifications] ()
RETURNS TABLE AS 
RETURN (
	SELECT
		[AC].*, ~[AC].[IsDeprecated] AS [IsActive],
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[CustomClassifications] WHERE [IsDeprecated] = 0 AND [Node].IsDescendantOf([AC].[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[CustomClassifications] WHERE [Node].IsDescendantOf([AC].[Node]) = 1) As [ChildCount]
	FROM dbo.[CustomClassifications] AC
);