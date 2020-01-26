CREATE VIEW [dbo].[AccountsBalancesView]
AS
	SELECT
		[AccountId],
		SUM(DLE.[Direction] * DLE.[MonetaryValue]) AS [MonetaryValue],
		SUM(DLE.[Direction] * DLE.[Mass]) AS [Mass],
		SUM(DLE.[Direction] * DLE.[Volume]) AS [Volume],
		SUM(DLE.[Direction] * DLE.[Count]) AS [Count],
		SUM(DLE.[Direction] * DLE.[Time]) AS [Time],
		SUM(DLE.[Direction] * DLE.[Value]) AS [Value]
	FROM dbo.[Entries] DLE
	JOIN dbo.[Lines] DL ON DLE.[LineId] = DL.[Id]
	JOIN dbo.[Documents] D ON DL.[DocumentId] = D.[Id]
	JOIN dbo.[DocumentDefinitions] DT ON D.[DefinitionId] = DT.[Id]
	WHERE D.[State] = 5 --N'Closed'
	AND DL.[State] = +4 -- N'Reviewed'
	GROUP BY
		[AccountId]
	HAVING
		SUM(DLE.[Direction] * DLE.[MonetaryValue]) <> 0 OR
		SUM(DLE.[Direction] * DLE.[Mass]) <> 0 OR 
		SUM(DLE.[Direction] * DLE.[Volume]) <> 0 OR
		SUM(DLE.[Direction] * DLE.[Count]) <> 0 OR
		SUM(DLE.[Direction] * DLE.[Time]) <> 0 OR
		SUM(DLE.[Direction] * DLE.[Value]) <> 0;
GO;