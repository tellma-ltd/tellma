CREATE PROCEDURE [rpt].[Rental__KPIs]
	@AsOfDate DATE
AS
	SELECT R.[Name] AS [Space], R.Decimal1 AS Area, C.[Name] AS Tenant, E.Time1, E.Time2
	FROM dbo.Entries E
	Join dbo.Lines L ON E.LineId = L.[Id]
	JOIN dbo.Documents D On L.[DocumentId] = D.[Id]
	JOIN dbo.DocumentDefinitions DD ON D.[DefinitionId] = DD.Id
	JOIN dbo.Resources R ON R.[Id] = E.[ResourceId]
	JOIN dbo.Relations C ON C.[Id] = E.[ParticipantId]
	WHERE E.[Index]  = 2
	and L.[State] = 2
	AND @AsOfDate Between E.Time1 And E.Time2
	ORDER BY R.[Code]