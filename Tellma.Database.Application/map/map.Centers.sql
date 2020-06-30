CREATE FUNCTION [map].[Centers] ()
RETURNS TABLE
AS
RETURN (
	SELECT [Q].*,
	(SELECT COUNT(*) FROM [dbo].[Centers] WHERE [IsActive] = 1 AND [Node].IsDescendantOf([Q].[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[Centers] WHERE [Node].IsDescendantOf([Q].[Node]) = 1) As [ChildCount]
	FROM [dbo].[Centers] As [Q]
);