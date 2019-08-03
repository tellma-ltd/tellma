CREATE PROCEDURE [dbo].[api_ProductCategories__Select]
AS
BEGIN
	SELECT [Id], [ParentId], [Name], [Code], [Node].ToString() As NodePath
	FROM dbo.ProductCategories
	ORDER BY [Node];
END;