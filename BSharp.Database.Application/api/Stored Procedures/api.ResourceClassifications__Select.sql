CREATE PROCEDURE [api].[ResourceClassifications__Select]
AS
BEGIN
	SELECT [Id], [Name], [Code], [Node].ToString() As NodePath
	FROM dbo.[ResourceClassifications]
	ORDER BY [Node];
END;