CREATE FUNCTION [map].[EntryClassifications]()
RETURNS TABLE AS 
RETURN (
	SELECT
		EC.*,
		(SELECT [Code] FROM dbo.[EntryClassifications] WHERE [Node] = EC.[ParentNode]) AS ParentId,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[EntryClassifications] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(EC.[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[EntryClassifications] WHERE [Node].IsDescendantOf(EC.[Node]) = 1) As [ChildCount]
	FROM dbo.[EntryClassifications] EC
);