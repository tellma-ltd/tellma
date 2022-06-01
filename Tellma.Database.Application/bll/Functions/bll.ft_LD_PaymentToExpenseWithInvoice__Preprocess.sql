CREATE FUNCTION [bll].[ft_LD_PaymentToExpenseWithInvoice__Preprocess] (
	@WideLines [WidelineList] READONLY
)
/*
Used in C/S Exp., Emp. Ben. - Kind, C/S Exp. (AP). In tenants: 100, 101
*/
--DECLARE @ProcessedWidelines WidelineList;
--INSERT @ProcessedWidelines([Index], [DocumentIndex],
--[PostingDate], [CenterId1], [Memo], [AgentId2], [InternalReference2], [AgentId3], 
--[ExternalReference1], [ResourceId0], [MonetaryValue0], [MonetaryValue1], [MonetaryValue2],
--[AgentId0], [CenterId0], [Quantity0], [UnitId0], [AccountId0]
--) VALUES
--(0, 0,
--N'2022-02-21',6, N'Test',44, N'999', 356,
--N'FS1001',132, 100, 150, 1150,  -- 270: board <=> 270, 271: brochure <=> Account: 472, Elec - Customer <=>412
--96,7, 1, NULL, NULL -- then try 412
--)
RETURNS @ProcessedWidelines TABLE
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
	INSERT INTO @ProcessedWidelines SELECT * FROM @WideLines;
	DECLARE @FunctionalCurrencyId NCHAR (30) = dal.fn_FunctionalCurrencyId();

	DECLARE @BusinessUnitId INT;
	IF dal.fn_FeatureCode__IsEnabled(N'BusinessUnitGoneWithTheWind') = 0
		SELECT @BusinessUnitId = [Id] FROM dbo.Centers WHERE CenterType = N'BusinessUnit' AND IsActive = 1;

	DECLARE @VATDepartment INT = dal.fn_AgentDefinition_Code__Id(N'TaxDepartment', N'VAT');
	DECLARE @ExpenseByNatureNode HIERARCHYID = dal.fn_AccountTypeConcept__Node(N'ExpenseByNature');

	UPDATE @ProcessedWidelines
	SET
		-- The expense currency is guessed from the resource, then the cash account, then functional
		[CurrencyId0] = COALESCE(
					dal.fn_Resource__CurrencyId([ResourceId0]),
					dal.fn_Agent__CurrencyId([AgentId2]),
					@FunctionalCurrencyId
				),
		[UnitId0] = ISNULL(dal.fn_Resource__UnitId([ResourceId0]), [UnitId0]);

	IF dal.fn_FeatureCode__IsEnabled(N'BusinessUnitGoneWithTheWind') = 0
		UPDATE @ProcessedWidelines
		SET 
			-- Center1, Center2, and Center3 must be equal. This is checked in Validation logic
			-- Center0 must be descendant of Center1
			[CenterId2] = ISNULL([dal].[fn_Agent__CenterId]([AgentId2]), [CenterId1]),
			[CenterId3] = ISNULL([dal].[fn_Agent__CenterId]([AgentId3]), [CenterId1]),
			-- Currency1 and Currency2 must be equal. This is checked in Validation logic
			[CurrencyId1] = ISNULL(dal.fn_Agent__CurrencyId([AgentId1]), [CurrencyId0]),
			[CurrencyId2] = ISNULL(dal.fn_Agent__CurrencyId([AgentId2]), [CurrencyId0]),
			[CurrencyId3] = ISNULL(dal.fn_Agent__CurrencyId([AgentId3]), [CurrencyId0]),
			[MonetaryValue2] = [NotedAmount1] + [MonetaryValue1],
			[MonetaryValue3] = 0,
			[NotedDate1] = EOMONTH([PostingDate]),
			[NotedDate3] = [PostingDate],
			[AgentId1] = @VATDepartment,
			[NotedAgentId1] = NULL,
			[ExternalReference2] = [ExternalReference1],	[ExternalReference3] = [ExternalReference1],
			[NotedAgentName2] = dbo.fn_Localize(
						dal.fn_Agent__Name([AgentId3]),
						dal.fn_Agent__Name2([AgentId3]),
						dal.fn_Agent__Name3([AgentId3])
						),
			[ResourceId1] = [ResourceId0], [Quantity1] = [Quantity0], [UnitId1] = [UnitId0],
			[CenterId0] = CASE
					WHEN dal.fn_Agent__AgentDefinitionCode([AgentId0]) IN (N'Project') THEN dal.fn_BusinessUnit__CIPCenter([CenterId1]) -- select dal.fn_BusinessUnit__CIPCenter(1)
					WHEN dal.fn_Agent__AgentDefinitionCode([AgentId0]) IN (N'TradeReceivableAccount') THEN dal.fn_BusinessUnit__SaleCenter([CenterId1])
					ELSE [CenterId0]
				END;

	-- Note: We enter requesting dept in header, but we can have it isCommon = false, and we can still read it, like Posting Date
	IF dal.fn_FeatureCode__IsEnabled(N'BusinessUnitGoneWithTheWind') = 1
	BEGIN
		UPDATE @ProcessedWidelines
		SET 
			[CenterId0] = COALESCE(
							[dal].[fn_Agent__CenterId]([AgentId0]), -- of PUC, IPUCD, incoming shipment, Production Order, Customer Project...
							[CenterId0], -- requesting dept
							[dal].[fn_Agent__CenterId]([AgentId3]), -- supplier
							[dal].[fn_Agent__CenterId]([AgentId2]), -- of cash account
							[dal].[fn_Agent__CenterId]([AgentId1]) -- of VAT account
						),
			-- Currency1 and Currency2 must be equal. This is checked in Validation logic
			[CurrencyId1] = ISNULL(dal.fn_Agent__CurrencyId([AgentId1]), [CurrencyId0]),
			[CurrencyId2] = ISNULL(dal.fn_Agent__CurrencyId([AgentId2]), [CurrencyId0]),
			[CurrencyId3] = ISNULL(dal.fn_Agent__CurrencyId([AgentId3]), [CurrencyId0]),
			[MonetaryValue2] = [NotedAmount1] + [MonetaryValue1],
			[MonetaryValue3] = 0,
			[NotedDate1] = EOMONTH([PostingDate]),
			[NotedDate3] = [PostingDate],
			[AgentId1] = @VATDepartment,
			[NotedAgentId1] = NULL,
			[ExternalReference2] = [ExternalReference1],	[ExternalReference3] = [ExternalReference1],
			[NotedAgentName2] = dbo.fn_Localize(
						dal.fn_Agent__Name([AgentId3]),
						dal.fn_Agent__Name2([AgentId3]),
						dal.fn_Agent__Name3([AgentId3])
						),
			[ResourceId1] = [ResourceId0], [Quantity1] = [Quantity0], [UnitId1] = [UnitId0];

		UPDATE @ProcessedWidelines
		SET 
			[CenterId1] = COALESCE(
								[dal].[fn_Agent__CenterId]([AgentId1]), -- of VAT account
								[dal].[fn_Agent__CenterId]([AgentId3]), -- supplier
								[CenterId0], -- requesting dept
								[dal].[fn_Agent__CenterId]([AgentId2]) -- of cash account
							),
			[CenterId2] = COALESCE(
								[dal].[fn_Agent__CenterId]([AgentId2]), -- of cash account
								[CenterId0], -- requesting dept
								[dal].[fn_Agent__CenterId]([AgentId3]), -- supplier
								[dal].[fn_Agent__CenterId]([AgentId1]) -- of VAT account
							),
			[CenterId3] = COALESCE(
								[dal].[fn_Agent__CenterId]([AgentId3]), -- of supplier
								[CenterId0], -- requesting dept
								[dal].[fn_Agent__CenterId]([AgentId2]), -- of cash account
								[dal].[fn_Agent__CenterId]([AgentId1]) -- of VAT account
							)

	END


	UPDATE @ProcessedWidelines
	SET 
		[NotedAgentName2] = dbo.fn_Localize(dal.fn_Agent__Name([AgentId3]), dal.fn_Agent__Name2([AgentId3]), dal.fn_Agent__Name3([AgentId3])), 
		[Value0] = bll.fn_ConvertToFunctional([PostingDate], [CurrencyId1], ISNULL([NotedAmount1], 0)),
		[Value1] = bll.fn_ConvertToFunctional([PostingDate], [CurrencyId1], ISNULL([MonetaryValue1], 0));

	UPDATE @ProcessedWidelines
	SET
		[Value2] = [Value0] + [Value1],
		[MonetaryValue0] =  IIF([CurrencyId1] = [CurrencyId0] AND [CurrencyId2] = [CurrencyId0],
								[NotedAmount1],
								bll.fn_ConvertFromFunctional([PostingDate], [CurrencyId0], [Value0])
							);
	--select * from @ProcessedWidelines;

	-- TODO: The following logic is to be moved to bll.Document__Preprocess
	-- since it is about account detection, which is universal
	WITH AccountScores AS (
	SELECT PWL.[Index], PWL.[DocumentIndex], A.[Id],
		IIF(A.[CenterId] IS NULL, 0, 1) + IIF(A.[CurrencyId] IS NULL, 0, 1) + 
			IIF(A.[AgentId] IS NULL, 0, 1) + IIF(A.[ResourceId] IS NULL, 0, 1) +
			IIF(A.[NotedAgentId] IS NULL, 0, 1) + IIF(A.[NotedResourceId] IS NULL, 0, 1) AS Score
	FROM @ProcessedWidelines PWL
	JOIN dbo.Resources R
		ON R.[Id] = PWL.[ResourceId0]
	LEFT JOIN dbo.Accounts A
		ON (A.[CenterId] IS NULL OR A.[CenterId] = PWL.[CenterId0])
		AND (A.[CurrencyId] IS NULL OR A.[CurrencyId] = PWL.[CurrencyId0])
		AND A.[ResourceDefinitionId] = R.[DefinitionId]
		AND (A.[ResourceId] IS NULL OR A.[ResourceId] = R.[Id])
		AND (PWL.[AgentId0] IS NULL AND A.[AgentDefinitionId] IS NULL OR
			A.[AgentDefinitionId] =  dal.fn_Agent__DefinitionId(PWL.[AgentId0]))
		AND (PWL.[NotedAgentId0] IS NULL AND A.[NotedAgentDefinitionId] IS NULL OR
			A.[NotedAgentDefinitionId] = dal.fn_Agent__DefinitionId(PWL.[NotedAgentId0]))
		AND (PWL.[NotedResourceId0] IS NULL AND A.[NotedResourceDefinitionId] IS NULL OR
			A.[NotedResourceDefinitionId] = dal.fn_Resource__DefinitionId(PWL.[NotedResourceId0]))
		AND A.AccountTypeId IN (SELECT [Id] FROM dbo.AccountTypes WHERE [Node].IsDescendantOf(@ExpenseByNatureNode) = 1)
		AND A.[ResourceDefinitionId] IS NOT NULL
	LEFT JOIN dbo.Agents AG	ON AG.[Id] = PWL.[AgentId0]
	LEFT JOIN dbo.Agents NAG ON NAG.[Id] = PWL.[NotedAgentId0]
	LEFT JOIN dbo.Resources NR ON NR.[Id] = PWL.[NotedResourceId0]
	) 	--select * from AccountScores
	,
	TopScores AS (
		SELECT A.[Index], A.[DocumentIndex], A.[Id], [Score]
		FROM AccountScores A
		JOIN (
			SELECT [Index], [DocumentIndex], MAX([Score]) AS MaxScore
			FROM AccountScores
			GROUP BY [Index], [DocumentIndex]
		) T ON A.[Index] = T.[Index] AND A.[DocumentIndex] = T.[DocumentIndex] AND A.[Score] = T.[MaxScore]
		LEFT JOIN  @ProcessedWidelines PWL
		ON A.[Index] = PWL.[Index]
		AND A.[DocumentIndex] = PWL.[DocumentIndex]
		AND A.[Id] = PWL.[AccountId0]
		WHERE PWL.[AccountId0] IS NULL
	) --select * from TopScores
	,
	UniqueScores AS (
		SELECT A.[Index], A.[DocumentIndex], IIF(T.[HitCount] = 1, A.[Id], NULL) AS [Id], [Score]
		FROM TopScores A
		JOIN (
			SELECT [Index], [DocumentIndex], COUNT([Score]) As HitCount
			FROM TopScores
			GROUP BY [Index], [DocumentIndex]
		) T ON A.[Index] = T.[Index] AND A.[DocumentIndex] = T.[DocumentIndex]

	) --select * from UniqueScores -- 0	0	270	1
	UPDATE PWL
	SET [AccountId0] = US.[Id]

	FROM @ProcessedWidelines PWL
	JOIN UniqueScores US 
		ON US.[Index] = PWL.[Index]
		AND US.[DocumentIndex] = PWL.[DocumentIndex]

	--select * from @ProcessedWidelines;
	RETURN
END