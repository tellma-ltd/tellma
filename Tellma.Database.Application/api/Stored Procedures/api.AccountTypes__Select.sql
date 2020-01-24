CREATE PROCEDURE [api].[AccountTypes__Select]
AS
BEGIN
	SELECT [Id], [Name], [Code], [Node].ToString() As NodePath
	FROM dbo.[AccountTypes]
	ORDER BY [Node];
END;