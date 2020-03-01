CREATE PROCEDURE [dal].[Documents_State__Refresh]
	@Ids dbo.IdList READONLY
AS
WITH Docs_MinPosStates AS
(
	SELECT DocumentId As [Id], MIN([State]) As [State] FROM dbo.Lines
	WHERE DocumentId IN (SELECT [Id] FROM @Ids)
	AND [State] >= 0
	GROUP BY DocumentId
),
Docs_MinNegStates AS
(
	SELECT DocumentId As [Id], MIN([State]) As [State] FROM dbo.Lines
	WHERE DocumentId IN (SELECT [Id] FROM @Ids)
	AND [State] < 0
	GROUP BY DocumentId
),
Docs__NewStates AS (
	SELECT COALESCE(P.[Id], N.[Id]) AS [Id],
	COALESCE(P.[State], N.[State]) AS [State]
	FROM Docs_MinPosStates P
	FULL OUTER JOIN Docs_MinNegStates N ON P.[Id] = N.[Id]
)
UPDATE D
SET
	D.[State]	= DN.[State],
	D.[PostingStateAt] = SYSDATETIMEOFFSET()
FROM dbo.Documents D
JOIN Docs__NewStates DN ON D.[Id] = DN.[Id]
WHERE D.[State] <> DN.[State];