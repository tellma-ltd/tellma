CREATE PROCEDURE [rpt].[RentalSpaces__Areas]
@AsOfDate DATE = N'2021.02.7'
AS
	SELECT LK.[Code], R.[Code],
		SUM(R.Decimal1) AS Area,
		SUM(IIF(T.ResourceId IS NOT NULL, R.[Decimal1], NULL)) AS RentedArea
	FROM dbo.Resources R
	JOIN dbo.Lookups LK ON R.[Lookup2Id] = LK.[Id]
	JOIN dbo.ResourceDefinitions RD ON R.[DefinitionId] = RD.[Id]
	LEFT JOIN (
		SELECT E.[ResourceId] 
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Relations RL ON E.[ParticipantId] = RL.[Id]
		JOIN dbo.Lookups LK2 ON RL.[Lookup1Id] = LK2.[Id]
		WHERE E.Time1 <= @AsOfDate AND E.Time2 >= @AsOfDate
		AND LK2.[Code] = N'R'
		AND L.[State] = 2
	) T ON R.[Id] = T.[ResourceId]
	WHERE RD.[Code] = N'RentalSpace'
	GROUP BY ROLLUP( LK.[Code], R.[Code])