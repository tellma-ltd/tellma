CREATE FUNCTION [dal].[ft_FixedAssetsDates__FixedAssetsProfiles] (
	@FixedAssetsDates dbo.IdDateList READONLY
)
RETURNS @FixedAssetProfiles TABLE
(
	[FixedAssetId]		INT,
	[AsOfDate]			DATE,
	[CenterId]			INT,
	[AgentId]			INT,
	[NotedAgentId]		INT
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
	WHERE E.[Time1] <= FAD.[Date]
--	AND (E.[Time2] IS NULL OR E.[Time2] >= FAD.[Date])
	AND L.[State] = 4
	AND (ET.[Concept] LIKE N'AdditionsOtherThanThroughBusinessCombinations%'
		OR ET.[Concept] LIKE N'Disposals%'
		OR ET.[Concept] LIKE N'Retirements%'
		OR ET.[Concept] LIKE N'DecreaseThroughClassifiedAsHeldForSale%'
		OR ET.[Concept] LIKE N'DecreaseThroughLossOfControlOfSubsidiary%'
		OR ET.[Concept] LIKE N'InternalTransfer%'
	)
	GROUP BY E.[ResourceId], FAD.[Date], E.[CenterId], E.[AgentId], E.[NotedAgentId]
	HAVING SUM(E.[Direction]) <> 0
	RETURN
END
GO
