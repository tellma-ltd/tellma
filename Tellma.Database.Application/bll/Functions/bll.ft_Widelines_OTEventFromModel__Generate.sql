﻿CREATE FUNCTION [bll].[ft_Widelines_OTEventFromModel__Generate]
(-- If a model expires during the period, it still generates an event
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT,
	@ContractTerminationLineDefinitionId INT,	
	@PeriodEnd DATE,
	@DurationUnitId INT,
	@EntryIndex	INT,
	@AgentId INT = NULL,
	@ResourceId INT = NULL,
	@NotedAgentId INT = NULL,
	@NotedResourceId INT = NULL,
	@CenterId INT = NULL
)
RETURNS @Widelines TABLE
(
	[Index]						INT	,
	[DocumentIndex]				INT				NOT NULL DEFAULT 0,-- INDEX IX_WideLineList_DocumentIndex ([DocumentIndex]),
	PRIMARY KEY ([Index], [DocumentIndex]),
	[Id]						INT				NOT NULL DEFAULT 0,
	[DefinitionId]				INT,
	[PostingDate]				DATE,
	[Memo]						NVARCHAR (255),
	[Boolean1]					BIT,
	[Decimal1]					DECIMAL (19,6),
	[Text1]						NVARCHAR(10),
	
	[Id0]						INT				NOT NULL DEFAULT 0,
	[Direction0]				SMALLINT,
	[AccountId0]				INT,
	[AgentId0]					INT,
	[NotedAgentId0]				INT,
	[ResourceId0]				INT,
	[CenterId0]					INT,
	[CurrencyId0]				NCHAR (3),
	[EntryTypeId0]				INT,
	[MonetaryValue0]			DECIMAL (19,6),
	[Quantity0]					DECIMAL (19,6),
	[UnitId0]					INT,
	[Value0]					DECIMAL (19,6),		
	[Time10]					DATETIME2 (2),
	[Duration0]					DECIMAL (19,6),
	[DurationUnitId0]			INT,
	[Time20]					DATETIME2 (2),
	[ExternalReference0]		NVARCHAR (50),
	[ReferenceSourceId0]		INT,
	[InternalReference0]		NVARCHAR (50),
	[NotedAgentName0]			NVARCHAR (50),
	[NotedAmount0]				DECIMAL (19,6),
	[NotedDate0]				DATE,
	[NotedResourceId0]			INT,

	[Id1]						INT				NULL DEFAULT 0,
	[Direction1]				SMALLINT,
	[AccountId1]				INT,
	[AgentId1]					INT,
	[NotedAgentId1]				INT,
	[ResourceId1]				INT,
	[CenterId1]					INT,
	[CurrencyId1]				NCHAR (3),
	[EntryTypeId1]				INT,
	[MonetaryValue1]			DECIMAL (19,6),
	[Quantity1]					DECIMAL (19,6),
	[UnitId1]					INT,
	[Value1]					DECIMAL (19,6),		
	[Time11]					DATETIME2 (2),
	[Duration1]					DECIMAL (19,6),
	[DurationUnitId1]			INT,
	[Time21]					DATETIME2 (2),
	[ExternalReference1]		NVARCHAR (50),
	[ReferenceSourceId1]		INT,
	[InternalReference1]		NVARCHAR (50),
	[NotedAgentName1]			NVARCHAR (50),
	[NotedAmount1]				DECIMAL (19,6),
	[NotedDate1]				DATE,
	[NotedResourceId1]			INT,

	[Id2]						INT				NULL DEFAULT 0, -- since a wide line may be two entries only
	[Direction2]				SMALLINT,
	[AccountId2]				INT,
	[AgentId2]					INT,
	[NotedAgentId2]				INT,
	[ResourceId2]				INT,
	[CenterId2]					INT,
	[CurrencyId2]				NCHAR (3),
	[EntryTypeId2]				INT,
	[MonetaryValue2]			DECIMAL (19,6),
	[Quantity2]					DECIMAL (19,6),
	[UnitId2]					INT,
	[Value2]					DECIMAL (19,6),
	[Time12]					DATETIME2 (2),
	[Duration2]					DECIMAL (19,6),
	[DurationUnitId2]			INT,
	[Time22]					DATETIME2 (2),
	[ExternalReference2]		NVARCHAR (50),
	[ReferenceSourceId2]		INT,
	[InternalReference2]		NVARCHAR (50),
	[NotedAgentName2]			NVARCHAR (50),
	[NotedAmount2]				DECIMAL (19,6),
	[NotedDate2]				DATE,
	[NotedResourceId2]			INT,

	[Id3]						INT				NULL DEFAULT 0,
	[Direction3]				SMALLINT,
	[AccountId3]				INT,
	[AgentId3]					INT,
	[NotedAgentId3]				INT,
	[ResourceId3]				INT,
	[CenterId3]					INT,
	[CurrencyId3]				NCHAR (3),
	[EntryTypeId3]				INT,
	[MonetaryValue3]			DECIMAL (19,6),
	[Quantity3]					DECIMAL (19,6),
	[UnitId3]					INT,
	[Value3]					DECIMAL (19,6),		
	[Time13]					DATETIME2 (2),
	[Duration3]					DECIMAL (19,6),
	[DurationUnitId3]			INT,
	[Time23]					DATETIME2 (2),
	[ExternalReference3]		NVARCHAR (50),
	[ReferenceSourceId3]		INT,
	[InternalReference3]		NVARCHAR (50),
	[NotedAgentName3]			NVARCHAR (50),
	[NotedAmount3]				DECIMAL (19,6),
	[NotedDate3]				DATE,
	[NotedResourceId3]			INT,

	[Id4]						INT				NULL DEFAULT 0,
	[Direction4]				SMALLINT,
	[AccountId4]				INT,
	[AgentId4]					INT,
	[NotedAgentId4]				INT,
	[ResourceId4]				INT,
	[CenterId4]					INT,
	[CurrencyId4]				NCHAR (3),
	[EntryTypeId4]				INT,
	[MonetaryValue4]			DECIMAL (19,6),
	[Quantity4]					DECIMAL (19,6),
	[UnitId4]					INT,
	[Value4]					DECIMAL (19,6),		
	[Time14]					DATETIME2 (2),
	[Duration4]					DECIMAL (19,6),
	[DurationUnitId4]			INT,
	[Time24]					DATETIME2 (2),
	[ExternalReference4]		NVARCHAR (50),
	[ReferenceSourceId4]		INT,
	[InternalReference4]		NVARCHAR (50),
	[NotedAgentName4]			NVARCHAR (50),
	[NotedAmount4]				DECIMAL (19,6),
	[NotedDate4]				DATE,
	[NotedResourceId4]			INT,

	[Id5]						INT				NULL DEFAULT 0,
	[Direction5]				SMALLINT,
	[AccountId5]				INT,
	[AgentId5]					INT,
	[NotedAgentId5]				INT,
	[ResourceId5]				INT,
	[CenterId5]					INT,
	[CurrencyId5]				NCHAR (3),
	[EntryTypeId5]				INT,
	[MonetaryValue5]			DECIMAL (19,6),
	[Quantity5]					DECIMAL (19,6),
	[UnitId5]					INT,
	[Value5]					DECIMAL (19,6),		
	[Time15]					DATETIME2 (2),
	[Duration5]					DECIMAL (19,6),
	[DurationUnitId5]			INT,
	[Time25]					DATETIME2 (2),
	[ExternalReference5]		NVARCHAR (50),
	[ReferenceSourceId5]		INT,
	[InternalReference5]		NVARCHAR (50),
	[NotedAgentName5]			NVARCHAR (50),
	[NotedAmount5]				DECIMAL (19,6),
	[NotedDate5]				DATE,
	[NotedResourceId5]			INT,

	[Id6]						INT				NULL DEFAULT 0,
	[Direction6]				SMALLINT,
	[AccountId6]				INT,
	[AgentId6]					INT,
	[NotedAgentId6]				INT,
	[ResourceId6]				INT,
	[CenterId6]					INT,
	[CurrencyId6]				NCHAR (3),
	[EntryTypeId6]				INT,
	[MonetaryValue6]			DECIMAL (19,6),
	[Quantity6]					DECIMAL (19,6),
	[UnitId6]					INT,
	[Value6]					DECIMAL (19,6),		
	[Time16]					DATETIME2 (2),
	[Duration6]					DECIMAL (19,6),
	[DurationUnitId6]			INT,
	[Time26]					DATETIME2 (2),
	[ExternalReference6]		NVARCHAR (50),
	[ReferenceSourceId6]		INT,
	[InternalReference6]		NVARCHAR (50),
	[NotedAgentName6]			NVARCHAR (50),
	[NotedAmount6]				DECIMAL (19,6),
	[NotedDate6]				DATE,
	[NotedResourceId6]			INT,

	[Id7]						INT				NULL DEFAULT 0,
	[Direction7]				SMALLINT,
	[AccountId7]				INT,
	[AgentId7]					INT,
	[NotedAgentId7]				INT,
	[ResourceId7]				INT,
	[CenterId7]					INT,
	[CurrencyId7]				NCHAR (3),
	[EntryTypeId7]				INT,
	[MonetaryValue7]			DECIMAL (19,6),
	[Quantity7]					DECIMAL (19,6),
	[UnitId7]					INT,
	[Value7]					DECIMAL (19,6),		
	[Time17]					DATETIME2 (2),
	[Duration7]					DECIMAL (19,6),
	[DurationUnitId7]			INT,
	[Time27]					DATETIME2 (2),
	[ExternalReference7]		NVARCHAR (50),
	[ReferenceSourceId7]		INT,
	[InternalReference7]		NVARCHAR (50),
	[NotedAgentName7]			NVARCHAR (50),
	[NotedAmount7]				DECIMAL (19,6),
	[NotedDate7]				DATE,
	[NotedResourceId7]			INT,

	[Id8]						INT				NULL DEFAULT 0,
	[Direction8]				SMALLINT,
	[AccountId8]				INT,
	[AgentId8]					INT,
	[NotedAgentId8]				INT,
	[ResourceId8]				INT,
	[CenterId8]					INT,
	[CurrencyId8]				NCHAR (3),
	[EntryTypeId8]				INT,
	[MonetaryValue8]			DECIMAL (19,6),
	[Quantity8]					DECIMAL (19,6),
	[UnitId8]					INT,
	[Value8]					DECIMAL (19,6),		
	[Time18]					DATETIME2 (2),
	[Duration8]					DECIMAL (19,6),
	[DurationUnitId8]			INT,
	[Time28]					DATETIME2 (2),
	[ExternalReference8]		NVARCHAR (50),
	[ReferenceSourceId8]		INT,
	[InternalReference8]		NVARCHAR (50),
	[NotedAgentName8]			NVARCHAR (50),
	[NotedAmount8]				DECIMAL (19,6),
	[NotedDate8]				DATE,
	[NotedResourceId8]			INT,

	[Id9]						INT				NULL DEFAULT 0,
	[Direction9]				SMALLINT,
	[AccountId9]				INT,
	[AgentId9]					INT,
	[NotedAgentId9]				INT,
	[ResourceId9]				INT,
	[CenterId9]					INT,
	[CurrencyId9]				NCHAR (3),
	[EntryTypeId9]				INT,
	[MonetaryValue9]			DECIMAL (19,6),
	[Quantity9]					DECIMAL (19,6),
	[UnitId9]					INT,
	[Value9]					DECIMAL (19,6),		
	[Time19]					DATETIME2 (2),
	[Duration9]					DECIMAL (19,6),
	[DurationUnitId9]			INT,
	[Time29]					DATETIME2 (2),
	[ExternalReference9]		NVARCHAR (50),
	[ReferenceSourceId9]		INT,
	[InternalReference9]		NVARCHAR (50),
	[NotedAgentName9]			NVARCHAR (50),
	[NotedAmount9]				DECIMAL (19,6),
	[NotedDate9]				DATE,
	[NotedResourceId9]			INT,

	[Id10]						INT				NULL DEFAULT 0,
	[Direction10]				SMALLINT,
	[AccountId10]				INT,
	[AgentId10]					INT,
	[NotedAgentId10]			INT,
	[ResourceId10]				INT,
	[CenterId10]				INT,
	[CurrencyId10]				NCHAR (3),
	[EntryTypeId10]				INT,
	[MonetaryValue10]			DECIMAL (19,6),
	[Quantity10]				DECIMAL (19,6),
	[UnitId10]					INT,
	[Value10]					DECIMAL (19,6),		
	[Time110]					DATETIME2 (2),
	[Duration10]				DECIMAL (19,6),
	[DurationUnitId10]			INT,
	[Time210]					DATETIME2 (2),
	[ExternalReference10]		NVARCHAR (50),
	[ReferenceSourceId10]		INT,
	[InternalReference10]		NVARCHAR (50),
	[NotedAgentName10]			NVARCHAR (50),
	[NotedAmount10]				DECIMAL (19,6),
	[NotedDate10]				DATE,
	[NotedResourceId10]			INT,

	[Id11]						INT				NULL DEFAULT 0,
	[Direction11]				SMALLINT,
	[AccountId11]				INT,
	[AgentId11]					INT,
	[NotedAgentId11]			INT,
	[ResourceId11]				INT,
	[CenterId11]				INT,
	[CurrencyId11]				NCHAR (3),
	[EntryTypeId11]				INT,
	[MonetaryValue11]			DECIMAL (19,6),
	[Quantity11]				DECIMAL (19,6),
	[UnitId11]					INT,
	[Value11]					DECIMAL (19,6),		
	[Time111]					DATETIME2 (2),
	[Duration11]				DECIMAL (19,6),
	[DurationUnitId11]			INT,
	[Time211]					DATETIME2 (2),
	[ExternalReference11]		NVARCHAR (50),
	[ReferenceSourceId11]		INT,
	[InternalReference11]		NVARCHAR (50),
	[NotedAgentName11]			NVARCHAR (50),
	[NotedAmount11]				DECIMAL (19,6),
	[NotedDate11]				DATE,
	[NotedResourceId11]			INT,

	[Id12]						INT				NULL DEFAULT 0,
	[Direction12]				SMALLINT,
	[AccountId12]				INT,
	[AgentId12]					INT,
	[NotedAgentId12]			INT,
	[ResourceId12]				INT,
	[CenterId12]				INT,
	[CurrencyId12]				NCHAR (3),
	[EntryTypeId12]				INT,
	[MonetaryValue12]			DECIMAL (19,6),
	[Quantity12]				DECIMAL (19,6),
	[UnitId12]					INT,
	[Value12]					DECIMAL (19,6),		
	[Time112]					DATETIME2 (2),
	[Duration12]				DECIMAL (19,6),
	[DurationUnitId12]			INT,
	[Time212]					DATETIME2 (2),
	[ExternalReference12]		NVARCHAR (50),
	[ReferenceSourceId12]		INT,
	[InternalReference12]		NVARCHAR (50),
	[NotedAgentName12]			NVARCHAR (50),
	[NotedAmount12]				DECIMAL (19,6),
	[NotedDate12]				DATE,
	[NotedResourceId12]			INT,

	[Id13]						INT				NULL DEFAULT 0,
	[Direction13]				SMALLINT,
	[AccountId13]				INT,
	[AgentId13]					INT,
	[NotedAgentId13]			INT,
	[ResourceId13]				INT,
	[CenterId13]				INT,
	[CurrencyId13]				NCHAR (3),
	[EntryTypeId13]				INT,
	[MonetaryValue13]			DECIMAL (19,6),
	[Quantity13]				DECIMAL (19,6),
	[UnitId13]					INT,
	[Value13]					DECIMAL (19,6),		
	[Time113]					DATETIME2 (2),
	[Duration13]				DECIMAL (19,6),
	[DurationUnitId13]			INT,
	[Time213]					DATETIME2 (2),
	[ExternalReference13]		NVARCHAR (50),
	[ReferenceSourceId13]		INT,
	[InternalReference13]		NVARCHAR (50),
	[NotedAgentName13]			NVARCHAR (50),
	[NotedAmount13]				DECIMAL (19,6),
	[NotedDate13]				DATE,
	[NotedResourceId13]			INT,

	[Id14]						INT				NULL DEFAULT 0,
	[Direction14]				SMALLINT,
	[AccountId14]				INT,
	[AgentId14]					INT,
	[NotedAgentId14]			INT,
	[ResourceId14]				INT,
	[CenterId14]				INT,
	[CurrencyId14]				NCHAR (3),
	[EntryTypeId14]				INT,
	[MonetaryValue14]			DECIMAL (19,6),
	[Quantity14]				DECIMAL (19,6),
	[UnitId14]					INT,
	[Value14]					DECIMAL (19,6),		
	[Time114]					DATETIME2 (2),
	[Duration14]				DECIMAL (19,6),
	[DurationUnitId14]			INT,
	[Time214]					DATETIME2 (2),
	[ExternalReference14]		NVARCHAR (50),
	[ReferenceSourceId14]		INT,
	[InternalReference14]		NVARCHAR (50),
	[NotedAgentName14]			NVARCHAR (50),
	[NotedAmount14]				DECIMAL (19,6),
	[NotedDate14]				DATE,
	[NotedResourceId14]			INT,

	[Id15]						INT				NULL DEFAULT 0,
	[Direction15]				SMALLINT,
	[AccountId15]				INT,
	[AgentId15]					INT,
	[NotedAgentId15]			INT,
	[ResourceId15]				INT,
	[CenterId15]				INT,
	[CurrencyId15]				NCHAR (3),
	[EntryTypeId15]				INT,
	[MonetaryValue15]			DECIMAL (19,6),
	[Quantity15]				DECIMAL (19,6),
	[UnitId15]					INT,
	[Value15]					DECIMAL (19,6),		
	[Time115]					DATETIME2 (2),
	[Duration15]				DECIMAL (19,6),
	[DurationUnitId15]			INT,
	[Time215]					DATETIME2 (2),
	[ExternalReference15]		NVARCHAR (50),
	[ReferenceSourceId15]		INT,
	[InternalReference15]		NVARCHAR (50),
	[NotedAgentName15]			NVARCHAR (50),
	[NotedAmount15]				DECIMAL (19,6),
	[NotedDate15]				DATE,
	[NotedResourceId15]			INT
)

AS
BEGIN
	DECLARE @PeriodStart DATE = dbo.fn_PeriodStart(@DurationUnitId, @PeriodEnd);
	SET @ContractAmendmentLineDefinitionId = ISNULL(@ContractAmendmentLineDefinitionId, 0);
	SET @ContractTerminationLineDefinitionId = ISNULL(@ContractTerminationLineDefinitionId, 0);

	DECLARE @Hour INT = dal.fn_UnitCode__Id(N'hr'), @Day INT = dal.fn_UnitCode__Id(N'd');

	DECLARE @T TABLE (
		[LineKey] INT, [Index] INT, [DurationUnitId] INT, [Decimal1] DECIMAL (19, 6), [PeriodIndex] INT, [Time1] DATE, [Time2] DATE,
		[AccountId] INT, [Direction] SMALLINT, [CenterId] INT, [AgentId] INT, [ResourceId] INT, [UnitId] INT, [Quantity] DECIMAL (19,4), [CurrencyId] NCHAR (3),
		[MonetaryValue] DECIMAL (19,4), [Value] DECIMAL (19,4), [NotedAmount] DECIMAL (19,4), [NotedAgentId] INT, [NotedResourceId] INT, [EntryTypeId] INT
		);

	WITH FilteredLines AS (
		SELECT DISTINCT L.Id, L.LineKey, L.[Decimal1]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId)
		AND L.[State] = 2
		AND E.[DurationUnitId] = @DurationUnitId -- Should be moved to the line level
		AND (E.[UnitId] IS NULL OR E.[UnitId] NOT IN (@Hour, @Day)) -- Since we are dealing with OT, a period must be at least one week, one month, one quarter or one year
		AND E.[Index] = @EntryIndex -- Primary entry whose data needs to be filtered
		AND (@AgentId IS NULL OR AgentId = @AgentId)
		AND (@ResourceId IS NULL OR ResourceId = @ResourceId)
		AND (@NotedAgentId IS NULL OR NotedAgentId = @NotedAgentId)
		AND (@NotedResourceId IS NULL OR NotedResourceId = @NotedResourceId)
		AND (@CenterId IS NULL OR CenterId = @CenterId)
	)
	INSERT INTO @T([LineKey], [Index], [DurationUnitId], [Decimal1], [Time1], [Time2],
		[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[Quantity], [MonetaryValue], [Value], [NotedAmount])
	SELECT L.[LineKey], E.[Index], [DurationUnitId], [Decimal1], E.[Time1], NULL, --E.[Time1], E.[Time2],  MA: 2022.08.13
		[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[Quantity], [MonetaryValue], [Value], [NotedAmount]
	FROM dbo.Entries E
	JOIN FilteredLines L ON L.[Id] = E.[LineId]
	WHERE E.[Time1] <= @PeriodEnd
	UNION ALL
	SELECT L.[LineKey], E.[Index], [DurationUnitId], [Decimal1], DATEADD(DAY, 1, E.[Time2]), NULL, --E.[Time1], E.[Time2], MA: 2022.08.13
		[AccountId], [Direction], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		-[Quantity], -[MonetaryValue], -[Value], -[NotedAmount]
	FROM dbo.Entries E
	JOIN FilteredLines L ON L.[Id] = E.[LineId]
	WHERE E.[Time2] < @PeriodEnd

	UPDATE @T SET [PeriodIndex] = dbo.fn_CommencementDate_DurationUnit_PeriodEnd__PeriodIndex([Time1], [DurationUnitId], @PeriodEnd)
	UPDATE @T SET [Time1] = IIF([PeriodIndex] = 0 , [Time1], @PeriodStart);
	UPDATE @T SET [Time2] = IIF(ISNULL([Time2], @PeriodEnd) < @PeriodEnd, [Time2], @PeriodEnd);
	UPDATE @T SET [Decimal1] = [Decimal1] * (DATEDIFF(DAY, [Time1], [Time2]) + 1) / (DATEDIFF(DAY, @PeriodStart, @PeriodEnd) + 1);

	UPDATE @T
	SET
		[Quantity] = [Decimal1] * [Quantity],
		[MonetaryValue] = [Decimal1] * [MonetaryValue],
		[Value] = [Decimal1] * [Value]
	--SELECT * FROM @T;

	DECLARE @T2 TABLE (
		[LineKey] INT, [Index] INT, [Time1] DATE, [Time2] DATE,
		[AccountId] INT, [Direction] SMALLINT, [CenterId] INT, [AgentId] INT, [ResourceId] INT, [UnitId] INT, [Quantity] DECIMAL (19,4), [CurrencyId] NCHAR (3),
		[MonetaryValue] DECIMAL (19,4), [Value] DECIMAL (19,4), [NotedAmount] DECIMAL (19,4), [NotedAgentId] INT, [NotedResourceId] INT, [EntryTypeId] INT
		);
	INSERT INTO @T2	([LineKey], [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId],[CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		 [Quantity], [MonetaryValue], [Value], [NotedAmount], [Time1], [Time2])
	SELECT [LineKey], [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId],  [NotedAgentId], [NotedResourceId], [EntryTypeId],
		SUM([Quantity]) AS [Quantity], SUM([MonetaryValue]) AS [MonetaryValue], SUM([Value]) AS [Value], SUM([NotedAmount]) AS [NotedAmount], MIN([Time1]) AS [Time1],  MIN([Time2]) AS [Time2]
	FROM @T
	GROUP BY [LineKey], [Index], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId]
	HAVING SUM([Value]) <> 0
	--SELECT * FROM @T2;

	DECLARE @Lines LineList, @Entries EntryList;

	INSERT INTO @Entries([LineIndex], [Index], [DocumentIndex], [Id], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId], [Quantity], [MonetaryValue], [Value], [NotedAmount], [Time1], [Time2])
	SELECT
		ROW_NUMBER () OVER(PARTITION BY [Index] ORDER BY [LineKey], [Index] ASC) - 1 AS [LineIndex],
		[Index], 0 AS [DocumentIndex],  0 AS [Id], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId],[CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[Quantity], [MonetaryValue], [Value], [NotedAmount], [Time1], [Time2]
	FROM @T2
	ORDER BY [LineKey], [Index];

	INSERT INTO @Lines([Index], [DocumentIndex], [Id])
	SELECT DISTINCT [LineIndex], 0, 0
	FROM @Entries;

	INSERT INTO @Widelines
	SELECT * FROM bll.fi_Lines__Pivot(@Lines, @Entries);

	RETURN
END