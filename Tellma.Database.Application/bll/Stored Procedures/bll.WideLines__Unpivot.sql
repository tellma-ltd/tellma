CREATE PROCEDURE [bll].[WideLines__Unpivot]
	@WideLines dbo.[WideLineList] READONLY
AS
	DECLARE @AllEntries dbo.EntryList;

	WITH LD AS (
		SELECT LineDefinitionId, COUNT(*) AS EntryCount FROM dbo.LineDefinitionEntries GROUP BY LineDefinitionId
	)
	INSERT INTO @AllEntries
	(
			[Index],[LineIndex],[DocumentIndex],[Id],[Direction],[AccountId],[CurrencyId],[CustodyId],[ResourceId],[CenterId],
			[EntryTypeId],[MonetaryValue],[Quantity],[UnitId],[Value],[Time1],[Time2],[ExternalReference],[AdditionalReference],[NotedRelationId],[NotedAgentName],[NotedAmount],[NotedDate]
		)
	SELECT	0,		WL.[Index],	[DocumentIndex],[Id0],[Direction0],[AccountId0],[CurrencyId0],[CustodyId0],[ResourceId0],[CenterId0],
			[EntryTypeId0],[MonetaryValue0],[Quantity0],[UnitId0],[Value0],[Time10],[Time20],[ExternalReference0],[AdditionalReference0],[NotedRelationId0],[NotedAgentName0],[NotedAmount0],[NotedDate0]
	FROM @WideLines WL JOIN LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 1
	UNION
	SELECT	1,		WL.[Index],	[DocumentIndex],[Id1],[Direction1],[AccountId1],[CurrencyId1],[CustodyId1],[ResourceId1],[CenterId1],
			[EntryTypeId1],[MonetaryValue1],[Quantity1],[UnitId1],[Value1],[Time11],[Time21],[ExternalReference1],[AdditionalReference1],[NotedRelationId1],[NotedAgentName1],[NotedAmount1],[NotedDate1]
	FROM @WideLines WL JOIN LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 2
	UNION
	SELECT	2,		WL.[Index],	[DocumentIndex],[Id2],[Direction2],[AccountId2],[CurrencyId2],[CustodyId2],[ResourceId2],[CenterId2],
			[EntryTypeId2],[MonetaryValue2],[Quantity2],[UnitId2],[Value2],[Time12],[Time22],[ExternalReference2],[AdditionalReference2],[NotedRelationId2],[NotedAgentName2],[NotedAmount2],[NotedDate2]
	FROM @WideLines WL JOIN LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 3;

	SELECT * FROM @AllEntries;