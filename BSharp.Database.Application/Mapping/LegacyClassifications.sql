CREATE FUNCTION [map].[LegacyClassifications] ()
RETURNS TABLE AS 
RETURN (
	SELECT
		[AC].*, ~[AC].[IsDeprecated] AS [IsActive],
		(SELECT [Id] FROM dbo.[LegacyClassifications] WHERE [Node] = AC.[ParentNode]) AS ParentId,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[LegacyClassifications] WHERE [IsDeprecated] = 0 AND [Node].IsDescendantOf([AC].[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[LegacyClassifications] WHERE [Node].IsDescendantOf([AC].[Node]) = 1) As [ChildCount]
	FROM dbo.[LegacyClassifications] AC
);