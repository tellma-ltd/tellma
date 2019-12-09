CREATE VIEW [dbo].[AccountsBalancesView]
AS
	SELECT
		[AccountId],
		SUM([Direction] * [MonetaryValue]) AS [MonetaryValue],
		SUM([Direction] * [Mass]) AS [Mass],
		SUM([Direction] * [Volume]) AS [Volume],
		SUM([Direction] * [Count]) AS [Count],
		SUM([Direction] * [Time]) AS [Time],
		SUM([Direction] * [Value]) AS [Value]
	FROM dbo.[Entries] DLE
	JOIN dbo.[Lines] DL ON DLE.[LineId] = DL.[Id]
	JOIN dbo.[Documents] D ON DL.[DocumentId] = D.[Id]
	JOIN dbo.[DocumentDefinitions] DT ON D.[DefinitionId] = DT.[Id]
	WHERE D.[State] = 5 --N'Closed'
	AND DL.[State] = +4 -- N'Reviewed'
	GROUP BY
		[AccountId]
	HAVING
		SUM([Direction] * [MonetaryValue]) <> 0 OR
		SUM([Direction] * [Mass]) <> 0 OR 
		SUM([Direction] * [Volume]) <> 0 OR
		SUM([Direction] * [Count]) <> 0 OR
		SUM([Direction] * [Time]) <> 0 OR
		SUM([Direction] * [Value]) <> 0;
GO;