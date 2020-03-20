CREATE FUNCTION [map].[DocumentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT DD.*, 
		CAST(IIF([1] > 0, 1, 0) AS BIT) AS [CanReachState1],
		CAST(IIF([2] > 1, 1, 0) AS BIT) AS [CanReachState2],
		CAST(IIF([3] > 0, 1, 0) AS BIT) AS [CanReachState3],
		CAST(IIF([4] > 0, 1, 0) AS BIT) AS [CanReachState4]
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