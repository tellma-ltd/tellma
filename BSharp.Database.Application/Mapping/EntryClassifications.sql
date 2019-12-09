CREATE FUNCTION [map].[EntryClassifications]()
RETURNS TABLE
AS
RETURN (
	SELECT Q.*,
	Q.[Node].GetLevel() AS [Level],
	(SELECT COUNT(*) FROM [dbo].[EntryClassifications] WHERE [IsActive] = 1 AND [Node].IsDescendantOf(Q.[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[EntryClassifications] WHERE [Node].IsDescendantOf(Q.[Node]) = 1) As [ChildCount]
	FROM [dbo].[EntryClassifications] Q
);
