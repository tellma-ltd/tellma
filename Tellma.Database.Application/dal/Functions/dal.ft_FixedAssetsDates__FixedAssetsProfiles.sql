CREATE FUNCTION [dal].[ft_FixedAssetsDates__FixedAssetsProfiles] (
	@FixedAssetsDates dbo.IdDateList READONLY
)
RETURNS @FixedAssetProfiles TABLE
(
	[FixedAssetId]		INT,
	[AsOfDate]			DATE,
	[CenterId]			INT,
	[AgentId]			INT,
	[NotedAgentId]		INT,
	[ReferenceSourceId] INT
	PRIMARY KEY ([FixedAssetId], [AsOfDate])
)
AS
BEGIN
	WITH FAEntries AS (
		  SELECT 
			E.[ResourceId],
			E.[CenterId], E.[AgentId], E.[NotedAgentId],
			E.[ReferenceSourceId],
			L.[PostingDate],
			ROW_NUMBER() OVER (PARTITION BY E.[ResourceId] ORDER BY L.[PostingDate] DESC, E.[Id] DESC) AS RowNum
		FROM 
			dbo.Entries E
			JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
			JOIN dbo.Lines L ON L.[Id] = E.[LineId]
			JOIN @FixedAssetsDates FA ON FA.[Id] = E.[ResourceId]
		WHERE 
			L.[PostingDate] <= FA.[Date]
		AND L.[State] = 4
		AND E.[Direction] = +1
		AND (ET.[Concept] LIKE N'AdditionsOtherThanThroughBusinessCombinations%'
			OR ET.[Concept] LIKE N'Disposals%'
			OR ET.[Concept] LIKE N'Retirements%'
			OR ET.[Concept] LIKE N'DecreaseThroughClassifiedAsHeldForSale%'
			OR ET.[Concept] LIKE N'DecreaseThroughLossOfControlOfSubsidiary%'
			OR ET.[Concept] LIKE N'InternalTransfer%'
		)
	),
	LatestEntries AS (
	SELECT 	[ResourceId], [CenterId], [AgentId], [NotedAgentId], [ReferenceSourceId]
	FROM 
		FAEntries
	WHERE 
		RowNum = 1
	)
	INSERT INTO @FixedAssetProfiles([FixedAssetId],	[AsOfDate], [CenterId],	[AgentId], [NotedAgentId], [ReferenceSourceId])
	SELECT FAD.[Id], FAD.[Date], LE.[CenterId],	LE.[AgentId], LE.[NotedAgentId], LE.[ReferenceSourceId]
	FROM @FixedAssetsDates FAD
	JOIN LatestEntries LE ON FAD.[Id] = LE.[ResourceId];

	RETURN
END
GO
