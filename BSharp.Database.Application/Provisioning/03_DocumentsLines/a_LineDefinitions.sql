DECLARE @LineDefinitions TABLE (
	[Id]						NVARCHAR (50)			PRIMARY KEY,
	[TitleSingular]				NVARCHAR (255),
	[TitleSingular2]			NVARCHAR (255),
	[TitleSingular3]			NVARCHAR (255),
	[TitlePlural]				NVARCHAR (255),
	[TitlePlural2]				NVARCHAR (255),
	[TitlePlural3]				NVARCHAR (255)
);

INSERT @LineDefinitions([Id]) VALUES
(N'ManualLine'),
(N'PettyCashPayment')
--,
----------------------------------------------------
--(N'CashIssue'),
--(N'VATInvoiceWithGoodReceipt'),
--(N'VATInvoiceWithoutGoodReceipt'),
--(N'PaysheetLine')
;

MERGE [dbo].[LineDefinitions] AS t
USING @LineDefinitions AS s
ON s.Id = t.Id
WHEN MATCHED THEN
	UPDATE SET
		t.[TitleSingular]	= s.[TitleSingular],
		t.[TitleSingular2]	= s.[TitleSingular2],
		t.[TitleSingular3]	= s.[TitleSingular3],
		t.[TitlePlural]		= s.[TitlePlural],
		t.[TitlePlural2]	= s.[TitlePlural2],
		t.[TitlePlural3]	= s.[TitlePlural3]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3])
    VALUES (s.[Id], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3]);

DECLARE @LineDefinitionEntries TABLE (
	[LineDefinitionId]				NVARCHAR (50),
	[EntryNumber]					INT,

	[DirectionIsVisible]			BIT				NOT NULL DEFAULT 0,
	[DirectionIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Reviewed',
	[DirectionExpression]			NVARCHAR (255),
	[DirectionEntryNumber]			INT,
	[Direction]						SMALLINT,

	[AccountIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[AccountIsEditableTill]			NVARCHAR (255)	NOT NULL DEFAULT N'Reviewed',
	[AccountDefinitionList]			NVARCHAR (1024),
	[AccountTypeList]				NVARCHAR (1024),
	[AccountIdExpression]			NVARCHAR (255),
	[AccountIdEntryNumber]			INT,
	[AccountId]						INT,

	[EntryTypeIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[EntryTypeIdIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Reviewed',
	[EntryTypeIdExpression]			NVARCHAR (255),
	[EntryTypeIdEntryNumber]		INT,
	[EntryTypeId]					NVARCHAR (255),

	[ResourceIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[ResourceIdIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Reviewed',
	[ResourceDefinitionList]		NVARCHAR (1024),
	[ResourceTypeList]				NVARCHAR (1024),
	[ResourceIdEntryNumber]			INT,
	[ResourceId]					INT,

	[LocationIdIsVisible]			BIT				NOT NULL DEFAULT 0,
	[LocationIdIsEditableTill]		NVARCHAR (255)	NOT NULL DEFAULT N'Reviewed',
	[LocationDefinitionList]		NVARCHAR (1024),
	[LocationTypeList]				NVARCHAR (1024),
	[LocationIdEntryNumber]			INT,
	[LocationId]					INT,

	[AgentIdIsVisible]				BIT				NOT NULL DEFAULT 0,
	[AgentIdIsEditableTill]			NVARCHAR (255)	NOT NULL DEFAULT N'Reviewed',
	[AgentDefinitionList]			NVARCHAR (1024),
	[AgentTypeList]					NVARCHAR (1024),
	[AgentIdEntryNumber]			INT,
	[AgentId]						INT,
	
	[BatchCodeIsVisible]			BIT				NOT NULL DEFAULT 0,
	[BatchCodeExpression]			NVARCHAR (255),
	[BatchCodeEntryNumber]			INT,

	[DueDateIsVisible]				BIT				NOT NULL DEFAULT 0,
	[DueDateExpression]				NVARCHAR (255),
	[DueDateEntryNumber]			INT,

	[QuantityIsVisible]				BIT				NOT NULL DEFAULT 0,
	[QuantityExpression]			NVARCHAR (255),
	[QuantityEntryNumber]			INT,
	[Quantity]						VTYPE,

	[MoneyAmountIsVisible]			BIT				NOT NULL DEFAULT 0,
	[MoneyAmountExpression]			NVARCHAR (255),
	[MoneyAmountEntryNumber]		INT,

	[MassIsVisible]					BIT				NOT NULL DEFAULT 0,
	[MassExpression]				NVARCHAR (255),
	[MassEntryNumber]				INT,

	[VolumeIsVisible]				BIT				NOT NULL DEFAULT 0,
	[VolumeExpression]				NVARCHAR (255),
	[VolumeEntryNumber]				INT,

	[AreaIsVisible]					BIT				NOT NULL DEFAULT 0,
	[AreaExpression]				NVARCHAR (255),
	[AreaEntryNumber]				INT,

	[LengthIsVisible]				BIT				NOT NULL DEFAULT 0,
	[LengthExpression]				NVARCHAR (255),
	[LengthEntryNumber]				INT,

	[TimeIsVisible]					BIT				NOT NULL DEFAULT 0,
	[TimeExpression]				NVARCHAR (255),
	[TimeEntryNumber]				INT,

	[CountIsVisible]				BIT				NOT NULL DEFAULT 0,
	[CountExpression]				NVARCHAR (255),
	[CountEntryNumber]				INT,
	[Count]							INT,
	
	[ValueIsVisible]				BIT				NOT NULL DEFAULT 0,
	[ValueExpression]				NVARCHAR (255),
	[ValueEntryNumber]				INT,

	[MemoIsVisible]					BIT				NOT NULL DEFAULT 0,
	[MemoExpression]				NVARCHAR (255),
	[MemoEntryNumber]				INT,

	[ExternalReferenceIsVisible]	BIT				NOT NULL DEFAULT 0,
	[ExternalReferenceExpression]	NVARCHAR (255),
	[ExternalReferenceEntryNumber]	INT,

	[AdditionalReferenceIsVisible]	BIT				NOT NULL DEFAULT 0,
	[AdditionalReferenceExpression]	NVARCHAR (255),
	[AdditionalReferenceEntryNumber]INT,

	[RelatedResourceId]				INT, -- Good, Service, Labor, Machine usage

	[RelatedAgentIsVisible]			BIT				NOT NULL DEFAULT 0,
	[RelatedAgentExpression]		NVARCHAR (255),
	[RelatedAgentEntryNumber]		INT,
		
	[RelatedQuantity]				MONEY ,			-- used in Tax accounts, to store the quantiy of taxable item
	[RelatedMoneyAmount]			MONEY 				NOT NULL DEFAULT 0 -- e.g., amount subject to tax
);

INSERT INTO @LineDefinitionEntries
-- Note: CurrentValueAddedTaxReceivables is not an account type for now
([LineDefinitionId], [EntryNumber], [Direction],	[AccountTypeList]) VALUES
(N'PettyCashPayment', 1,			+1,				NULL),
(N'PettyCashPayment', 2,			+1,				N'CurrentValueAddedTaxPayables,CurrentValueAddedTaxReceivables'),
(N'PettyCashPayment', 3,			-1,				N'CashOnHand');

MERGE [dbo].[LineDefinitionEntries] AS t
USING @LineDefinitionEntries AS s
ON s.[LineDefinitionId] = t.[LineDefinitionId] AND s.[EntryNumber] = t.[EntryNumber]
WHEN MATCHED THEN
	UPDATE SET
		t.[Direction]		= s.[Direction],
		t.[AccountTypeList]	= s.[AccountTypeList]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([LineDefinitionId], [EntryNumber],		[Direction], [AccountTypeList])
    VALUES (s.[LineDefinitionId], s.[EntryNumber], s.[Direction], s.[AccountTypeList]);