CREATE FUNCTION [rpt].[ProductCategories] ()
RETURNS TABLE
AS
RETURN (
	SELECT [Q].*, [Q].[Node].GetLevel() AS [Level],
	(SELECT COUNT(*) FROM [dbo].[ProductCategories] WHERE [IsActive] = 1 AND [Node].IsDescendantOf([Q].[Node]) = 1) As [ActiveChildCount],
    (SELECT COUNT(*) FROM [dbo].[ProductCategories] WHERE [Node].IsDescendantOf([Q].[Node]) = 1) As [ChildCount]
	FROM [dbo].[ProductCategories] As [Q]
);
