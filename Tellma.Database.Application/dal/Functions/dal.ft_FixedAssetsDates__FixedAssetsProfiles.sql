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
	INSERT INTO @FixedAssetProfiles([FixedAssetId], [AsOfDate],	[CenterId], [AgentId], [NotedAgentId])
	SELECT E.[ResourceId], FAD.[Date], E.[CenterId], E.[AgentId], E.[NotedAgentId]
	FROM dbo.Entries E
	JOIN dbo.Lines L ON L.[Id] = E.[LineId]
	JOIN dbo.Accounts A ON A.[Id] = E.[AccountId]
	JOIN dbo.EntryTypes ET ON ET.[Id] = E.[EntryTypeId]
	JOIN @FixedAssetsDates FAD ON FAD.[Id] = E.[ResourceId]
	--WHERE E.[Time1] <= FAD.[Date] -- MA: 2024-07-27, commented and added line below
	WHERE L.[PostingDate] <= FAD.[Date] -- -- MA: 2024-07-27, to avoid issues with half month convention
--	AND (E.[Time2] IS NULL OR E.[Time2] >= FAD.[Date])
	AND L.[State] = 4
	AND (ET.[Concept] LIKE N'AdditionsOtherThanThroughBusinessCombinations%'
		OR ET.[Concept] LIKE N'Disposals%'
		OR ET.[Concept] LIKE N'Retirements%'
		OR ET.[Concept] LIKE N'DecreaseThroughClassifiedAsHeldForSale%'
		OR ET.[Concept] LIKE N'DecreaseThroughLossOfControlOfSubsidiary%'
		OR ET.[Concept] LIKE N'InternalTransfer%'
	)
	GROUP BY E.[ResourceId], FAD.[Date], E.[CenterId], E.[AgentId], E.[NotedAgentId] -- for location in particular, we willuse the code below
	HAVING SUM(E.[Direction]) <> 0;

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
			JOIN @FixedAssetProfiles FA ON FA.[FixedAssetId] = E.[ResourceId]
		WHERE 
			L.[PostingDate] <= FA.[AsOfDate]
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
	LatestLocations AS (
	SELECT 	[ResourceId], [ReferenceSourceId]
	FROM 
		LatestEntries
	WHERE 
		RowNum = 1
	)
	UPDATE FAP
	SET
		[ReferenceSourceId] = LL.ReferenceSourceId
	FROM @FixedAssetProfiles FAP
	JOIN LatestLocations LL ON FAP.[FixedAssetId] = LL.[ResourceId];

	RETURN
END
GO
