CREATE FUNCTION [map].[DocumentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT DD.*, 
		IIF([1] > 0, 1, 0) AS Has_1,
		IIF([2] > 1, 1, 0) AS Has_2,
		IIF([3] > 0, 1, 0) AS Has_3,
		IIF([4] > 0, 1, 0) AS Has_4
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