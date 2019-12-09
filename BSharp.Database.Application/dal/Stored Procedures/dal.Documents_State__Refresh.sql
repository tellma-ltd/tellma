CREATE PROCEDURE [dal].[Documents_State__Refresh]
	@Ids dbo.IdList READONLY
AS
WITH Docs__NewStates AS (
	SELECT DocumentId As [Id], MIN([State]) As State FROM dbo.Lines
	WHERE DocumentId IN (SELECT [Id] FROM @Ids)
	GROUP BY DocumentId
)
UPDATE D
SET D.[State] = DN.[State]
FROM dbo.Documents D
JOIN Docs__NewStates DN ON D.[Id] = DN.[Id]
WHERE D.[State] <> DN.[State];