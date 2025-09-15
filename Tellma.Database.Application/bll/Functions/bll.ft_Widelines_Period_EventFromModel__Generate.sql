﻿CREATE FUNCTION [bll].[ft_Widelines_Period_EventFromModel__Generate]
(
--DECLARE
	@ContractLineDefinitionId INT,
	@ContractAmendmentLineDefinitionId INT, -- This was a mistake. Need to be removed.
	@ContractTerminationLineDefinitionId INT,
	@FromDate DATE,
	@ToDate DATE,
	@DurationUnitId INT,
	@EntryIndex	INT,
	@AgentId INT = NULL,
	@ResourceId INT = NULL,
	@NotedAgentId INT = NULL,
	@NotedResourceId INT = NULL,
	@CenterId INT = NULL
	-- Should be adding also
	-- Quantity DECIMAL (19, 6) = 1, -- to work as multiplying factor for PIT
	-- CurrencyId NCHAR (3) = NULL
	-- EntryTypeId INT = NULL
	--SELECT @FromDate = '2022-11-01', @ToDate = '2022-11-30', @DurationUnitId = dal.fn_UnitCode__Id('mo');
	--SET @ContractLineDefinitionId  = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccruals.M');
	--SET @ContractAmendmentLineDefinitionId  = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsAmended.M');
	--SET @ContractTerminationLineDefinitionId  = dal.fn_LineDefinitionCode__Id(N'ToEmployeeBenefitsExpenseFromAccrualsTerminated.M');
	--SELECT @EntryIndex = 0, @NotedAgentId = 16; SET @ResourceId = null;
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
		[Decimal2]					DECIMAL (19,6),
		[Text1]						NVARCHAR(50),
		[Text2]						NVARCHAR(50),
	
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
	SET @ContractAmendmentLineDefinitionId = ISNULL(@ContractAmendmentLineDefinitionId, 0);
	SET @ContractTerminationLineDefinitionId = ISNULL(@ContractTerminationLineDefinitionId, 0);
--	IF ISNULL(@ToDate,  N'9999-12-31') = N'9999-12-31' SET @ToDate = N'9999-12-30';
	SET @ToDate = ISNULL(@ToDate,  N'9999-12-31');
	DECLARE @OldContractAmendmentLineDefinitionId INT;
	IF @ContractAmendmentLineDefinitionId <> 0
	BEGIN
		DECLARE @ContractAmendmentLineDefinitionCode NVARCHAR (255) = dal.fn_LineDefinition__Code(@ContractAmendmentLineDefinitionId);
		SET @OldContractAmendmentLineDefinitionId = ISNULL(dal.fn_LineDefinitionCode__Id(N'(Old)' + @ContractAmendmentLineDefinitionCode), 0);
	END

	DECLARE @Hour INT = dal.fn_UnitCode__Id(N'hr'), @Day INT = dal.fn_UnitCode__Id(N'd');
	DECLARE @LdEntryCount INT = (SELECT COUNT(*) FROM LineDefinitionEntries WHERE [LineDefinitionId] = @ContractLineDefinitionId);

	-- @T splits any time limited line into two lines
	DECLARE @T TABLE (
		[Id]	INT IDENTITY PRIMARY KEY,
		[LineKey] INT, [EntryIndex] INT, 
		[DurationUnitId] INT, --[Decimal1] DECIMAL (19, 6), 
		[Time1] DATE, [Time2] DATE,
		[Direction] SMALLINT, [AccountId] INT, [CenterId] INT, [AgentId] INT, [ResourceId] INT, [UnitId] INT, [CurrencyId] NCHAR (3),
		[NotedAgentId] INT, [NotedResourceId] INT, [EntryTypeId] INT,
		[Quantity] DECIMAL (19,4), [MonetaryValue] DECIMAL (19,4), [Value] DECIMAL (19,4), [NotedAmount] DECIMAL (19,4)
	--	INDEX ([LineKey], [Time1])
		--UNIQUE ([LineKey], [EntryIndex], [Time1], [CenterId], [AgentId], [NotedResourceId], [EntryTypeId]) -- MA: added 2023-03-24 because they were changing after contract termination
	);
	WITH FilteredLines AS (
		SELECT DISTINCT L.[LineKey]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId,
															@OldContractAmendmentLineDefinitionId)
		AND (L.[State] = 2)
		AND (@DurationUnitId IS NULL OR E.[DurationUnitId] = @DurationUnitId) -- Should be moved to the line level, and renamed to @FrequencyId
		-- next line is needed when terminating contracts but not when ag salaries
		AND (E.[Index] = @EntryIndex) -- Primary entry whose data needs to be filtered
		AND (E.[Time1] <= @ToDate)
		AND (ISNULL(E.[Time2], '9999-12-31') >= @FromDate)
		AND (@AgentId IS NULL OR AgentId = @AgentId)
		AND (@ResourceId IS NULL OR ResourceId = @ResourceId)
		AND (@NotedAgentId IS NULL OR NotedAgentId = @NotedAgentId)
		AND (@NotedResourceId IS NULL OR NotedResourceId = @NotedResourceId)
		AND (@CenterId IS NULL OR CenterId = @CenterId)
	),--select * from FilteredLines
	FilteredEntries AS  (
		SELECT FL.[LineKey], E.[Index] % @LdEntryCount AS [Index] , E.[DurationUnitId], --FL.[Decimal1],
			IIF(E.[Time1]<= @FromDate, @FromDate, E.[Time1]) AS [Time1], '9999-12-31' AS [Time2],
			[Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
			[Quantity], [MonetaryValue], [Value], [NotedAmount]
		FROM dbo.Entries E -- MA: Should it be BaseQuantity, to allow entering templates of different unit?
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN FilteredLines FL ON FL.[LineKey] = L.[LineKey]
		WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId,
															@OldContractAmendmentLineDefinitionId)
		AND L.[State] = 2 AND E.[Time1] <= @ToDate AND ISNULL(E.[Time2], '9999-12-31') >= @FromDate
--		AND (@ResourceId IS NULL OR E.ResourceId = @ResourceId)-- caused empty for SS
		UNION ALL
		SELECT FL.[LineKey], E.[Index] % @LdEntryCount AS [Index], E.[DurationUnitId], --FL.[Decimal1],
			DATEADD(DAY, 1, E.[Time2]) AS [Time1], '9999-12-31' AS [Time2],
			[Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
			- [Quantity] AS [Quantity], - [MonetaryValue] AS [MonetaryValue], - [Value] AS [Value], - [NotedAmount] AS [NotedAmount]
		FROM dbo.Entries E
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN FilteredLines FL ON FL.[LineKey] = L.[LineKey]
		WHERE L.DefinitionId IN (@ContractLineDefinitionId, @ContractAmendmentLineDefinitionId, @ContractTerminationLineDefinitionId,
															@OldContractAmendmentLineDefinitionId)
		AND L.[State] = 2 AND ISNULL(E.[Time2], '9999-12-31') < @ToDate AND ISNULL(E.[Time2], '9999-12-31') >= @FromDate
--		AND (@ResourceId IS NULL OR E.ResourceId = @ResourceId)
	) --  select * from FilteredEntries 
	INSERT INTO @T([LineKey], [EntryIndex], [DurationUnitId], --[Decimal1], 
		[Time1],[Direction],[AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[Time2], [Quantity], [MonetaryValue], [Value], [NotedAmount])
	SELECT [LineKey], [Index], [DurationUnitId], --MAX([Decimal1]) AS [Decimal1], 
		[Time1], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		DATEADD(DAY, -1, LEAD([Time1]) OVER (PARTITION BY [LineKey], [Index], [DurationUnitId], [Direction], [AccountId], [CenterId], [AgentId],
											[ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] 
						ORDER BY [Time1])) AS [Time2],
		SUM([Quantity]) OVER (PARTITION BY [LineKey], [Index], [DurationUnitId], [Direction], [AccountId], [CenterId], [AgentId],
											[ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] 
						ORDER BY [Time1]) AS [Quantity],
		SUM([MonetaryValue]) OVER (PARTITION BY [LineKey], [Index], [DurationUnitId], [Direction], [AccountId], [CenterId], [AgentId],
											[ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] 
						ORDER BY [Time1]) AS [MonetaryValue], 
		SUM([Value]) OVER (PARTITION BY [LineKey], [Index], [DurationUnitId], [Direction], [AccountId], [CenterId], [AgentId],
											[ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] 
						ORDER BY [Time1]) AS [Value], 
		SUM([NotedAmount]) OVER (PARTITION BY [LineKey], [Index], [DurationUnitId], [Direction], [AccountId], [CenterId], [AgentId],
											[ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId] 
						ORDER BY [Time1]) AS [NotedAmount]
	FROM (
		SELECT
			[LineKey], [Index] , [DurationUnitId], [Time1], [Time2], [Direction], [AccountId], [CenterId], [AgentId],
			[ResourceId], [UnitId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
			SUM([Quantity]) AS [Quantity], SUM([MonetaryValue]) AS [MonetaryValue], SUM([Value]) AS [Value], SUM([NotedAmount]) AS [NotedAmount]
		FROM FilteredEntries
		GROUP BY
			[LineKey], [Index] , [DurationUnitId], [Time1], [Time2], Direction, AccountId, CenterId, AgentId, ResourceId, UnitId, [CurrencyId], NotedAgentId, NotedResourceId, EntryTypeId
	) AFE -- Aggregated filtered entries. To act on the total resulting entries
	-- MA: next line is helpful to know which employees are causing anmalies
	-- select ISNULL(NotedAgentId, AgentId), sum(Direction*[Value]) from @T  group by ISNULL(NotedAgentId, AgentId) Having Sum([Direction]*[Value]) <> 0
	DELETE FROM @T
	WHERE [Time2] < [Time1]
	OR [Id] IN ( -- If a workflow (LineKey) adds up to zero, for a period [Time1, *], remove ALL entries
		SELECT [Id]
		FROM @T T1
		JOIN (
			SELECT [LineKey], [Time1], [Time2]
			FROM @T
			GROUP BY [LineKey], [Time1], [Time2]
			HAVING SUM([MonetaryValue]) = 0
		) T2 ON T2.[LineKey] = T1.[LineKey] AND T2.[Time1] = T1.[Time1] AND ISNULL(T2.[Time2], '99991231') = ISNULL(T1.[Time2], '99991231')
	)
	-- MA: 2023-07-20, commented because it caused errors in July SS Contribution for Tenant (303)
	-- MA: 2023-09-01, uncommented because it is needed to remove 0 results for amendments starting beginning of month
	OR [Id] IN (
		SELECT [Id]
		FROM @T T1
		JOIN (
			SELECT [LineKey], [Time1], [Time2], [EntryIndex] --  MA: 2023-09-01 Added Entry Index 
			FROM @T
			WHERE [MonetaryValue] <> 0
		) T2 ON T2.[LineKey] = T1.[LineKey] AND T2.[Time1] = T1.[Time1] AND ISNULL(T2.[Time2], '99991231') = ISNULL(T1.[Time2], '99991231')
			AND T2.[EntryIndex] = T1.[EntryIndex] --  MA: 2023-09-01 Added to mitigate the SS Contribution 
		WHERE [MonetaryValue] = 0
	);
--	select * from @T   order by LineKey, time1, entryIndex; 

	-- MA 2023-11-03. The following garbage collection was added to handle a bug resulting from transfer of centers with zero Monetary Value
	IF @LdEntryCount= 3
	BEGIN -- If a center appears once, delete it.
		DELETE FROM @T
		WHERE [Id] IN (
			SELECT [Id]
			FROM @T T1
			INNER JOIN (
				SELECT [CenterId], [LineKey]
				FROM @T
				GROUP BY [CenterId], [LineKey]
				HAVING COUNT(*) = 1
			) T2 ON T1.[CenterId] = T2.[CenterId] AND T1.[LineKey] = T2.LineKey
		)
		AND [LineKey] IN (
			SELECT [LineKey]
			FROM @T
			GROUP BY CenterId, [LineKey]
			HAVING COUNT(*) = 3
		)
	END;

	DECLARE @Lines LineList, @Entries EntryList;

	INSERT INTO @Entries([LineIndex], [Index], [DocumentIndex], [Id], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		[Quantity], [UnitId], [MonetaryValue], [Value], [NotedAmount], [Time1], [Time2], [DurationUnitId])
	SELECT
		ROW_NUMBER () OVER(PARTITION BY [EntryIndex]
			ORDER BY [Time1], [LineKey], [EntryIndex]
		) - 1 AS [LineIndex],
		[EntryIndex] AS [Index], 0 AS [DocumentIndex],  0 AS [Id], [Direction], [AccountId], [CenterId], [AgentId], [ResourceId], [CurrencyId], [NotedAgentId], [NotedResourceId], [EntryTypeId],
		1, [UnitId], [MonetaryValue], [Value], [NotedAmount], [Time1], ISNULL([Time2], @ToDate) AS [Time2], [DurationUnitId]
	FROM @T

	INSERT INTO @Lines([Index], [DocumentIndex], [Id], [Decimal1])
	SELECT DISTINCT
		ROW_NUMBER () OVER(--PARTITION BY T2.[EntryIndex]
			ORDER BY T.[Time1], T.[LineKey] --, T2.[EntryIndex]
		) - 1 AS [LineIndex], 0 AS [DocumentIndex],  0 AS [Id], LDLK.[Decimal1]
	FROM @T T
	JOIN dbo.[LineDefinitionLineKeys] LDLK ON LDLK.[Id] = T.[LineKey]
	WHERE T.[EntryIndex] = @EntryIndex
--	select * from @lines
	INSERT INTO @Widelines
	SELECT * FROM bll.fi_Lines__Pivot(@Lines, @Entries)
--	select * from @widelines

	DECLARE @WidelinesSequencing TABLE ([Index] INT PRIMARY KEY IDENTITY (0, 1), [WLIndex] INT, UNIQUE([WLIndex]))
	INSERT INTO @WidelinesSequencing ([WLIndex]) SELECT [Index] FROM @Widelines;
	--select * from @WidelinesSequencing

	UPDATE WL
	SET WL.[Index] = WS.[Index]
	FROM @Widelines WL
	JOIN @WidelinesSequencing WS ON WS.[WLIndex] = WL.[Index]

	RETURN
END
GO
