CREATE PROCEDURE [bll].[WideLines__Unpivot]
	@WideLines dbo.[WideLineList] READONLY
AS
	DECLARE @AllEntries dbo.EntryList;
	DECLARE @LD TABLE (LineDefinitionId INT PRIMARY KEY, EntryCount INT);

	INSERT INTO @LD(LineDefinitionId, EntryCount)
	SELECT LineDefinitionId, COUNT(*) AS EntryCount
	FROM dbo.LineDefinitionEntries
	WHERE LineDefinitionId IN (SELECT DISTINCT LineDefinitionId FROM @WideLines)
	GROUP BY LineDefinitionId;
	
	INSERT INTO @AllEntries
	(
			[Index],[LineIndex],[DocumentIndex],[Id],[Direction],[AccountId],[CurrencyId],[AgentId],[NotedAgentId],[ResourceId],[CenterId],
			[EntryTypeId],[MonetaryValue],[Quantity],[UnitId],[Value],[Time1],[Duration],[DurationUnitId],[Time2],[ExternalReference],[ReferenceSourceId],[InternalReference],[NotedAgentName],[NotedAmount],[NotedDate]
		)
	SELECT	0,		WL.[Index],	[DocumentIndex],[Id0],[Direction0],[AccountId0],[CurrencyId0],[AgentId0],[NotedAgentId0],[ResourceId0],[CenterId0],
			[EntryTypeId0],[MonetaryValue0],[Quantity0],[UnitId0],[Value0],[Time10],[Duration0],[DurationUnitId0],[Time20],[ExternalReference0],[ReferenceSourceId0],[InternalReference0],[NotedAgentName0],[NotedAmount0],[NotedDate0]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 1
	UNION
	SELECT	1,		WL.[Index],	[DocumentIndex],[Id1],[Direction1],[AccountId1],[CurrencyId1],[AgentId1],[NotedAgentId1],[ResourceId1],[CenterId1],
			[EntryTypeId1],[MonetaryValue1],[Quantity1],[UnitId1],[Value1],[Time11],[Duration1],[DurationUnitId1],[Time21],[ExternalReference1],[ReferenceSourceId1],[InternalReference1],[NotedAgentName1],[NotedAmount1],[NotedDate1]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 2
	UNION
	SELECT	2,		WL.[Index],	[DocumentIndex],[Id2],[Direction2],[AccountId2],[CurrencyId2],[AgentId2],[NotedAgentId2],[ResourceId2],[CenterId2],
			[EntryTypeId2],[MonetaryValue2],[Quantity2],[UnitId2],[Value2],[Time12],[Duration2],[DurationUnitId2],[Time22],[ExternalReference2],[ReferenceSourceId2],[InternalReference2], [NotedAgentName2],[NotedAmount2],[NotedDate2]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 3
	UNION
	SELECT	3,		WL.[Index],	[DocumentIndex],[Id3],[Direction3],[AccountId3],[CurrencyId3],[AgentId3],[NotedAgentId3],[ResourceId3],[CenterId3],
			[EntryTypeId3],[MonetaryValue3],[Quantity3],[UnitId3],[Value3],[Time13],[Duration3],[DurationUnitId3],[Time23],[ExternalReference3],[ReferenceSourceId3],[InternalReference3], [NotedAgentName3],[NotedAmount3],[NotedDate3]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 4
	UNION
	SELECT	4,		WL.[Index],	[DocumentIndex],[Id4],[Direction4],[AccountId4],[CurrencyId4],[AgentId4],[NotedAgentId4],[ResourceId4],[CenterId4],
			[EntryTypeId4],[MonetaryValue4],[Quantity4],[UnitId4],[Value4],[Time14],[Duration4],[DurationUnitId4],[Time24],[ExternalReference4],[ReferenceSourceId4],[InternalReference4],[NotedAgentName4],[NotedAmount4],[NotedDate4]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 5
	UNION
	SELECT	5,		WL.[Index],	[DocumentIndex],[Id5],[Direction5],[AccountId5],[CurrencyId5],[AgentId5],[NotedAgentId5],[ResourceId5],[CenterId5],
			[EntryTypeId5],[MonetaryValue5],[Quantity5],[UnitId5],[Value5],[Time15],[Duration5],[DurationUnitId5],[Time25],[ExternalReference5],[ReferenceSourceId5],[InternalReference5],[NotedAgentName5],[NotedAmount5],[NotedDate5]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 6
	UNION
	SELECT	6,		WL.[Index],	[DocumentIndex],[Id6],[Direction6],[AccountId6],[CurrencyId6],[AgentId6],[NotedAgentId6],[ResourceId6],[CenterId6],
			[EntryTypeId6],[MonetaryValue6],[Quantity6],[UnitId6],[Value6],[Time16],[Duration6],[DurationUnitId6],[Time26],[ExternalReference6],[ReferenceSourceId6],[InternalReference6],[NotedAgentName6],[NotedAmount6],[NotedDate6]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 7
	UNION
	SELECT	7,		WL.[Index],	[DocumentIndex],[Id7],[Direction7],[AccountId7],[CurrencyId7],[AgentId7],[NotedAgentId7],[ResourceId7],[CenterId7],
			[EntryTypeId7],[MonetaryValue7],[Quantity7],[UnitId7],[Value7],[Time17],[Duration7],[DurationUnitId7],[Time27],[ExternalReference7],[ReferenceSourceId7],[InternalReference7],[NotedAgentName7],[NotedAmount7],[NotedDate7]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 8
	UNION
	SELECT	8,		WL.[Index],	[DocumentIndex],[Id8],[Direction8],[AccountId8],[CurrencyId8],[AgentId8],[NotedAgentId8],[ResourceId8],[CenterId8],
			[EntryTypeId8],[MonetaryValue8],[Quantity8],[UnitId8],[Value8],[Time18],[Duration8],[DurationUnitId8],[Time28],[ExternalReference8],[ReferenceSourceId8],[InternalReference8],[NotedAgentName8],[NotedAmount8],[NotedDate8]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 9
	UNION
	SELECT	9,		WL.[Index],	[DocumentIndex],[Id9],[Direction9],[AccountId9],[CurrencyId9],[AgentId9],[NotedAgentId9],[ResourceId9],[CenterId9],
			[EntryTypeId9],[MonetaryValue9],[Quantity9],[UnitId9],[Value9],[Time19],[Duration9],[DurationUnitId9],[Time29],[ExternalReference9],[ReferenceSourceId9],[InternalReference9],[NotedAgentName9],[NotedAmount9],[NotedDate9]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 10
	UNION
	SELECT	10,		WL.[Index],	[DocumentIndex],[Id10],[Direction10],[AccountId10],[CurrencyId10],[AgentId10],[NotedAgentId10],[ResourceId10],[CenterId10],
			[EntryTypeId10],[MonetaryValue10],[Quantity10],[UnitId10],[Value10],[Time110],[Duration10],[DurationUnitId10],[Time210],[ExternalReference10],[ReferenceSourceId10],[InternalReference10],[NotedAgentName10],[NotedAmount10],[NotedDate10]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 11
	UNION
	SELECT	11,		WL.[Index],	[DocumentIndex],[Id11],[Direction11],[AccountId11],[CurrencyId11],[AgentId11],[NotedAgentId11],[ResourceId11],[CenterId11],
			[EntryTypeId11],[MonetaryValue11],[Quantity11],[UnitId11],[Value11],[Time111],[Duration11],[DurationUnitId11],[Time211],[ExternalReference11],[ReferenceSourceId11],[InternalReference11],[NotedAgentName11],[NotedAmount11],[NotedDate11]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 12
	UNION
	SELECT	12,		WL.[Index],	[DocumentIndex],[Id12],[Direction12],[AccountId12],[CurrencyId12],[AgentId12],[NotedAgentId12],[ResourceId12],[CenterId12],
			[EntryTypeId12],[MonetaryValue12],[Quantity12],[UnitId12],[Value12],[Time112],[Duration12],[DurationUnitId12],[Time212],[ExternalReference12],[ReferenceSourceId12],[InternalReference12],[NotedAgentName12],[NotedAmount12],[NotedDate12]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 13
	UNION
	SELECT	13,		WL.[Index],	[DocumentIndex],[Id13],[Direction13],[AccountId13],[CurrencyId13],[AgentId13],[NotedAgentId13],[ResourceId13],[CenterId13],
			[EntryTypeId13],[MonetaryValue13],[Quantity13],[UnitId13],[Value13],[Time113],[Duration13],[DurationUnitId13],[Time213],[ExternalReference13],[ReferenceSourceId13],[InternalReference13],[NotedAgentName13],[NotedAmount13],[NotedDate13]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 14
	UNION
	SELECT	14,		WL.[Index],	[DocumentIndex],[Id14],[Direction14],[AccountId14],[CurrencyId14],[AgentId14],[NotedAgentId14],[ResourceId14],[CenterId14],
			[EntryTypeId14],[MonetaryValue14],[Quantity14],[UnitId14],[Value14],[Time114],[Duration14],[DurationUnitId14],[Time214],[ExternalReference14],[ReferenceSourceId14],[InternalReference14],[NotedAgentName14],[NotedAmount14],[NotedDate14]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 15
	UNION
	SELECT	15,		WL.[Index],	[DocumentIndex],[Id15],[Direction15],[AccountId15],[CurrencyId15],[AgentId15],[NotedAgentId15],[ResourceId15],[CenterId15],
			[EntryTypeId15],[MonetaryValue15],[Quantity15],[UnitId15],[Value15],[Time115],[Duration15],[DurationUnitId15],[Time215],[ExternalReference15],[ReferenceSourceId15],[InternalReference15],[NotedAgentName15],[NotedAmount15],[NotedDate15]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 16;

	SELECT * FROM @AllEntries;