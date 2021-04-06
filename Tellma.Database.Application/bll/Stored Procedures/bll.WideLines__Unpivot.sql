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
			[Index],[LineIndex],[DocumentIndex],[Id],[Direction],[AccountId],[CurrencyId],[CustodianId],[CustodyId],[ParticipantId],[ResourceId],[CenterId],
			[EntryTypeId],[MonetaryValue],[Quantity],[UnitId],[Value],[Time1],[Time2],[ExternalReference],[InternalReference],[NotedAgentName],[NotedAmount],[NotedDate]
		)
	SELECT	0,		WL.[Index],	[DocumentIndex],[Id0],[Direction0],[AccountId0],[CurrencyId0],[CustodianId0],[CustodyId0],[ParticipantId0],[ResourceId0],[CenterId0],
			[EntryTypeId0],[MonetaryValue0],[Quantity0],[UnitId0],[Value0],[Time10],[Time20],[ExternalReference0],[InternalReference0],[NotedAgentName0],[NotedAmount0],[NotedDate0]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 1
	UNION
	SELECT	1,		WL.[Index],	[DocumentIndex],[Id1],[Direction1],[AccountId1],[CurrencyId1],[CustodianId1],[CustodyId1],[ParticipantId1],[ResourceId1],[CenterId1],
			[EntryTypeId1],[MonetaryValue1],[Quantity1],[UnitId1],[Value1],[Time11],[Time21],[ExternalReference1],[InternalReference1],[NotedAgentName1],[NotedAmount1],[NotedDate1]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 2
	UNION
	SELECT	2,		WL.[Index],	[DocumentIndex],[Id2],[Direction2],[AccountId2],[CurrencyId2],[CustodianId2],[CustodyId2],[ParticipantId2],[ResourceId2],[CenterId2],
			[EntryTypeId2],[MonetaryValue2],[Quantity2],[UnitId2],[Value2],[Time12],[Time22],[ExternalReference2],[InternalReference2], [NotedAgentName2],[NotedAmount2],[NotedDate2]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 3
	UNION
	SELECT	3,		WL.[Index],	[DocumentIndex],[Id3],[Direction3],[AccountId3],[CurrencyId3],[CustodianId3],[CustodyId3],[ParticipantId3],[ResourceId3],[CenterId3],
			[EntryTypeId3],[MonetaryValue3],[Quantity3],[UnitId3],[Value3],[Time13],[Time23],[ExternalReference3],[InternalReference3], [NotedAgentName3],[NotedAmount3],[NotedDate3]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 4
	UNION
	SELECT	4,		WL.[Index],	[DocumentIndex],[Id4],[Direction4],[AccountId4],[CurrencyId4],[CustodianId4],[CustodyId4],[ParticipantId4],[ResourceId4],[CenterId4],
			[EntryTypeId4],[MonetaryValue4],[Quantity4],[UnitId4],[Value4],[Time14],[Time24],[ExternalReference4],[InternalReference4],[NotedAgentName4],[NotedAmount4],[NotedDate4]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 5
	UNION
	SELECT	5,		WL.[Index],	[DocumentIndex],[Id5],[Direction5],[AccountId5],[CurrencyId5],[CustodianId5],[CustodyId5],[ParticipantId5],[ResourceId5],[CenterId5],
			[EntryTypeId5],[MonetaryValue5],[Quantity5],[UnitId5],[Value5],[Time15],[Time25],[ExternalReference5],[InternalReference5],[NotedAgentName5],[NotedAmount5],[NotedDate5]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 6
	UNION
	SELECT	6,		WL.[Index],	[DocumentIndex],[Id6],[Direction6],[AccountId6],[CurrencyId6],[CustodianId6],[CustodyId6],[ParticipantId6],[ResourceId6],[CenterId6],
			[EntryTypeId6],[MonetaryValue6],[Quantity6],[UnitId6],[Value6],[Time16],[Time26],[ExternalReference6],[InternalReference6],[NotedAgentName6],[NotedAmount6],[NotedDate6]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 7
	UNION
	SELECT	7,		WL.[Index],	[DocumentIndex],[Id7],[Direction7],[AccountId7],[CurrencyId7],[CustodianId7],[CustodyId7],[ParticipantId7],[ResourceId7],[CenterId7],
			[EntryTypeId7],[MonetaryValue7],[Quantity7],[UnitId7],[Value7],[Time17],[Time27],[ExternalReference7],[InternalReference7],[NotedAgentName7],[NotedAmount7],[NotedDate7]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 8
	UNION
	SELECT	8,		WL.[Index],	[DocumentIndex],[Id8],[Direction8],[AccountId8],[CurrencyId8],[CustodianId8],[CustodyId8],[ParticipantId8],[ResourceId8],[CenterId8],
			[EntryTypeId8],[MonetaryValue8],[Quantity8],[UnitId8],[Value8],[Time18],[Time28],[ExternalReference8],[InternalReference8],[NotedAgentName8],[NotedAmount8],[NotedDate8]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 9
	UNION
	SELECT	9,		WL.[Index],	[DocumentIndex],[Id9],[Direction9],[AccountId9],[CurrencyId9],[CustodianId9],[CustodyId9],[ParticipantId9],[ResourceId9],[CenterId9],
			[EntryTypeId9],[MonetaryValue9],[Quantity9],[UnitId9],[Value9],[Time19],[Time29],[ExternalReference9],[InternalReference9],[NotedAgentName9],[NotedAmount9],[NotedDate9]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 10
	UNION
	SELECT	10,		WL.[Index],	[DocumentIndex],[Id10],[Direction10],[AccountId10],[CurrencyId10],[CustodianId10],[CustodyId10],[ParticipantId10],[ResourceId10],[CenterId10],
			[EntryTypeId10],[MonetaryValue10],[Quantity10],[UnitId10],[Value10],[Time110],[Time210],[ExternalReference10],[InternalReference10],[NotedAgentName10],[NotedAmount10],[NotedDate10]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 11
	UNION
	SELECT	11,		WL.[Index],	[DocumentIndex],[Id11],[Direction11],[AccountId11],[CurrencyId11],[CustodianId11],[CustodyId11],[ParticipantId11],[ResourceId11],[CenterId11],
			[EntryTypeId11],[MonetaryValue11],[Quantity11],[UnitId11],[Value11],[Time111],[Time211],[ExternalReference11],[InternalReference11],[NotedAgentName11],[NotedAmount11],[NotedDate11]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 12
	UNION
	SELECT	12,		WL.[Index],	[DocumentIndex],[Id12],[Direction12],[AccountId12],[CurrencyId12],[CustodianId12],[CustodyId12],[ParticipantId12],[ResourceId12],[CenterId12],
			[EntryTypeId12],[MonetaryValue12],[Quantity12],[UnitId12],[Value12],[Time112],[Time212],[ExternalReference12],[InternalReference12],[NotedAgentName12],[NotedAmount12],[NotedDate12]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 13
	UNION
	SELECT	13,		WL.[Index],	[DocumentIndex],[Id13],[Direction13],[AccountId13],[CurrencyId13],[CustodianId13],[CustodyId13],[ParticipantId13],[ResourceId13],[CenterId13],
			[EntryTypeId13],[MonetaryValue13],[Quantity13],[UnitId13],[Value13],[Time113],[Time213],[ExternalReference13],[InternalReference13],[NotedAgentName13],[NotedAmount13],[NotedDate13]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 14
	UNION
	SELECT	14,		WL.[Index],	[DocumentIndex],[Id14],[Direction14],[AccountId14],[CurrencyId14],[CustodianId14],[CustodyId14],[ParticipantId14],[ResourceId14],[CenterId14],
			[EntryTypeId14],[MonetaryValue14],[Quantity14],[UnitId14],[Value14],[Time114],[Time214],[ExternalReference14],[InternalReference14],[NotedAgentName14],[NotedAmount14],[NotedDate14]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 15
	UNION
	SELECT	15,		WL.[Index],	[DocumentIndex],[Id15],[Direction15],[AccountId15],[CurrencyId15],[CustodianId15],[CustodyId15],[ParticipantId15],[ResourceId15],[CenterId15],
			[EntryTypeId15],[MonetaryValue15],[Quantity15],[UnitId15],[Value15],[Time115],[Time215],[ExternalReference15],[InternalReference15],[NotedAgentName15],[NotedAmount15],[NotedDate15]
	FROM @WideLines WL JOIN @LD LD ON WL.DefinitionId = LD.LineDefinitionId
	WHERE LD.EntryCount >= 16;

	SELECT * FROM @AllEntries;