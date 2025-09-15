CREATE PROCEDURE [dal].[PPE_Locations__Update] AS
BEGIN
	DECLARE @FA TABLE ([ResourceId] INT PRIMARY KEY, [Profile_DutyStationId] INT, [Entries_DutyStationId] INT);

	INSERT INTO @FA([ResourceId], [Profile_DutyStationId])
	SELECT R.Id, R.Agent2Id AS DS
	FROM dbo.Resources R
	JOIN dbo.ResourceDefinitions rd on rd.Id = r.definitionId
	JOIN dbo.Agents AG on AG.Id =r.Agent2Id
	where rd.Code like N'%Member';

	WITH LatestEntries AS (
			SELECT 
			E.[ResourceId],
			E.[ReferenceSourceId],
			L.[PostingDate],
			ROW_NUMBER() OVER (PARTITION BY E.[ResourceId] ORDER BY L.[PostingDate] DESC, E.[Id] DESC) AS RowNum
		FROM 
			dbo.Entries E
			JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
			JOIN dbo.Lines L ON L.[Id] = E.[LineId]
			JOIN @FA FA ON FA.[ResourceId] = E.[ResourceId]
		WHERE L.[State] = 4
		AND E.[Direction] = +1
		AND (ET.[Concept] LIKE N'AdditionsOtherThanThroughBusinessCombinations%'
			OR ET.[Concept] LIKE N'Disposals%'
			OR ET.[Concept] LIKE N'Retirements%'
			OR ET.[Concept] LIKE N'DecreaseThroughClassifiedAsHeldForSale%'
			OR ET.[Concept] LIKE N'DecreaseThroughLossOfControlOfSubsidiary%'
			OR ET.[Concept] LIKE N'InternalTransfer%'
		)
	),
	LatestLocations AS (
	SELECT 	[ResourceId], [ReferenceSourceId]
	FROM 
		LatestEntries
	WHERE 
		RowNum = 1
	)
	UPDATE FA
	SET
		Agent2Id = LL.ReferenceSourceId
	FROM dbo.Resources FA
	JOIN LatestLocations LL ON FA.[Id] = LL.[ResourceId]
	WHERE (Agent2Id <> LL.ReferenceSourceId);
--		select Agent2Id from Resources where Id = 1608
	WITH Disposed AS (
		SELECT E.[ResourceId]
		FROM dbo.Entries E
		JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN @FA FA ON FA.[ResourceId] = E.[ResourceId]
		WHERE L.[State] = 4
		AND E.[Direction] = -1
		AND (
			ET.[Concept] LIKE N'Disposals%'
			)
	)
	UPDATE FA
	SET
		Agent2Id = NULL
	FROM dbo.Resources FA
	JOIN Disposed D ON FA.[Id] = D.[ResourceId]
	WHERE Agent2Id IS NOT NULL
END
GO