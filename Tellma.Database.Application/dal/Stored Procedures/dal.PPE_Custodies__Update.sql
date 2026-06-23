CREATE PROCEDURE [dal].[PPE_Custodies__Update] AS
BEGIN
	DECLARE @FA TABLE ([ResourceId] INT PRIMARY KEY, [Profile_CustodianId] INT, [Entries_CustodianId] INT);

	INSERT INTO @FA([ResourceId], [Profile_CustodianId])
	SELECT R.Id, R.Agent1Id AS CustodianId --, rd.id, rd.TitleSingular
	FROM dbo.Resources R
	JOIN dbo.ResourceDefinitions rd on rd.Id = r.definitionId
	LEFT JOIN dbo.Agents AG on AG.Id =r.Agent1Id
	where rd.Code like N'%Member'
	AND rd.Id <> 138
	and R.Id in (SELECT E.ResourceId FROM dbo.Entries E JOIN dbo.EntryTypes ET ON ET.Id = E.EntryTypeId
		WHERE ET.[Concept] LIKE N'AdditionsOtherThanThroughBusinessCombinations%')
	;
	
	WITH LatestEntries AS (
			SELECT 
			E.[ResourceId],
			E.[NotedAgentId],
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
	LatestCustodies AS (
	SELECT 	[ResourceId], [NotedAgentId]
	FROM 
		LatestEntries
	WHERE 
		RowNum = 1
	)
	UPDATE FA
	SET
		Agent1Id = LL.[NotedAgentId]
	FROM dbo.Resources FA
	JOIN LatestCustodies LL ON FA.[Id] = LL.[ResourceId]
	WHERE (ISNULL(Agent1Id, 0) <> ISNULL(LL.[NotedAgentId], 0));
--		select Agent1Id from Resources where Id = 1608

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
		Agent1Id = NULL
	FROM dbo.Resources FA
	JOIN Disposed D ON FA.[Id] = D.[ResourceId]
	WHERE Agent1Id IS NOT NULL
END
GO