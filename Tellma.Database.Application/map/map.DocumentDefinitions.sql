CREATE FUNCTION [map].[DocumentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT
		DD.[Id],
		DD.[Code],
		DD.[IsOriginalDocument],
		DD.[DocumentType],
		DD.[Description]	,
		DD.[Description2],
		DD.[Description3],
		DD.[TitleSingular],
		DD.[TitleSingular2]	,
		DD.[TitleSingular3],
		DD.[TitlePlural]	,
		DD.[TitlePlural2]		,
		DD.[TitlePlural3]	,
		DD.[SortKey]	,
		DD.[Prefix]	,
		DD.[CodeWidth]	,
	
		DD.[PostingDateVisibility]	,
		DD.[CenterVisibility]	,

		DD.[ClearanceVisibility],
		DD.[MemoVisibility],

		DD.[HasAttachments],
		DD.[HasBookkeeping]	,

		DD.[State],
		DD.[MainMenuIcon],
		DD.[MainMenuSection],
		DD.[MainMenuSortKey],
		DD.[SavedById],
		TODATETIMEOFFSET([ValidFrom], '+00:00') AS [SavedAt],
		DD.[ValidFrom],
		DD.[ValidTo],
		CAST(IIF([1] > 0, 1, 0) AS BIT) AS [CanReachState1],
		CAST(IIF([2] > 0, 1, 0) AS BIT) AS [CanReachState2],
		CAST(IIF([3] > 0, 1, 0) AS BIT) AS [CanReachState3],
		CAST(IIF([1]+[2]+[3]+[4] > 0, 1, 0) AS BIT) AS [HasWorkflow]
	FROM dbo.DocumentDefinitions DD
	LEFT JOIN
	(
		SELECT DDLD.DocumentDefinitionId, W.ToState 
		FROM dbo.DocumentDefinitionLineDefinitions DDLD
		JOIN dbo.Workflows W ON DDLD.LineDefinitionId = W.LineDefinitionId
	) AS ST
	PIVOT  (
		COUNT([ToState]) FOR [ToState] IN ([1], [2], [3], [4])
	) AS PT  ON DD.[Id] = PT.DocumentDefinitionId
);