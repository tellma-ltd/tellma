CREATE PROCEDURE [bll].[WideLines__Unpivot]
		@WideLines dbo.[WideLineList] READONLY
AS
	DECLARE @AllEntries dbo.EntryList;

	WITH LD AS (
		SELECT LineDefinitionId, COUNT(*) AS EntryCount FROM dbo.LineDefinitionEntries GROUP BY LineDefinitionId
	)
	INSERT INTO @AllEntries
	(
			[Index], [LineIndex], [DocumentIndex], [Id], [EntryNumber], [Direction],[AccountId],[AgentId],[ResourceId],[ResponsibilityCenterId],
			--[AccountIdentifier],[ResourceIdentifier],
			[CurrencyId],[EntryTypeId],	[DueDate],	[MonetaryValue])
	SELECT 3*[Index], [Index],	[DocumentIndex], [Id],		0,		[Direction0],[AccountId0],[AgentId0],[ResourceId0],[ResponsibilityCenterId0],
			--[AccountIdentifier0],[ResourceIdentifier0],
			[CurrencyId0],[EntryTypeId0],	[DueDate0],	[MonetaryValue0]
	FROM @WideLines WL JOIN LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 1
	UNION
	SELECT 3*[Index] + 1, [Index],	[DocumentIndex], [Id],		1,	[Direction1],[AccountId1],[AgentId1],[ResourceId1],[ResponsibilityCenterId1],
			--[AccountIdentifier1],[ResourceIdentifier1],
			[CurrencyId1],[EntryTypeId1],	[DueDate1],	[MonetaryValue1]
	FROM @WideLines WL JOIN LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 2
	UNION
	SELECT 3*[Index] + 2, [Index],	[DocumentIndex], [Id],		2,	[Direction2],[AccountId2],[AgentId2],[ResourceId2],[ResponsibilityCenterId2],
			--[AccountIdentifier2],[ResourceIdentifier2],
			[CurrencyId2],[EntryTypeId2],	[DueDate2],	[MonetaryValue2]
	FROM @WideLines WL JOIN LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 3

	SELECT * FROM @AllEntries;