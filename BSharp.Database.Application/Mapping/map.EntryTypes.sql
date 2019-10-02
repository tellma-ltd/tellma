CREATE FUNCTION [map].[EntryTypes]()
RETURNS TABLE AS 
RETURN (
	SELECT
		ET.*,
		(SELECT [Id] FROM dbo.[EntryTypes] WHERE [Node] = ET.[ParentNode]) AS ParentId,
		[Node].GetLevel() AS [Level],
		(SELECT COUNT(*) FROM [dbo].[EntryTypes] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(ET.[Node]) = 1) As [ActiveChildCount],
		(SELECT COUNT(*) FROM [dbo].[EntryTypes] WHERE [Node].IsDescendantOf(ET.[Node]) = 1) As [ChildCount]
	FROM dbo.[EntryTypes] ET
);