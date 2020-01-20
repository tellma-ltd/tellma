DECLARE @LineDefinitions dbo.LineDefinitionList;
DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];
/*
	[AgentSource]						SMALLINT			NOT NULL DEFAULT 1, --  -1: n/a, 3: from account
	[AgentId]							INT					REFERENCES dbo.Agents([Id]),	-- fixed in the case of ERCA, e.g., VAT

	[ResourceSource]					SMALLINT			NOT NULL DEFAULT 1,
	[ResourceId]						INT					REFERENCES dbo.Resources([Id]),	-- Fixed in the case of unallocated expense
	
	[CurrencySource]					SMALLINT			NOT NULL DEFAULT 2,
	[CurrencyId]						NCHAR (3)			REFERENCES dbo.Currencies([Id]),	-- Fixed in the case of unallocated expense

	[EntryClassificationSource]			SMALLINT			NOT NULL DEFAULT 0,
	[EntryClassificationCode]			NVARCHAR (255),
	
	[MonetaryValueSource]				SMALLINT			NOT NULL DEFAULT 1,
	[QuantitySource]					SMALLINT			NOT NULL DEFAULT 1,
	[ExternalReferenceSource]			SMALLINT			NOT NULL DEFAULT 2,
	[AdditionalReferenceSource]			SMALLINT			NOT NULL DEFAULT 2,
	[NotedAgentSource]					SMALLINT			NOT NULL DEFAULT 2,
	[NotedAmountSource]					SMALLINT			NOT NULL DEFAULT 2,
	[DueDateSource]						SMALLINT			NOT NULL DEFAULT 1
	*/
DECLARE @LineDefinitionEntries TABLE (
	[Index]						INT,
	[HeaderIndex]				INT,
	PRIMARY KEY ([Index], [HeaderIndex]),
	[Id]						INT		DEFAULT 0,
	[EntryNumber]				INT,
	[Direction]					SMALLINT,
	-- Source = -1 (n/a), 1 (get from line), 2 (get from entry), 4-7 (from other entry data), 8 (from balancing), 9 (from bll script)
	-- 4: from resource/agent/currency etc./5 from (Resource, Account Type), 6: from Counter/Contra/Noted in Line, 7:
	-- Account is invisible in a tab, unless the source specifies it is entered by user. or in Manual line
	
	-- The idea is to allow the user to enter enough information, so Tellma can figure out the account, or at least short list it:
	-- AccountType, which must be a child of the AccountTypeParentCode
	-- Account.CurrencyId must match that entered by user. So, Line has CurrencyId
	-- Account.IsCurrent must conform to that computed by system (from DueDate), otherwise, return all conforming Accounts
	-- Account.ResponsibilityCenter must match or be ancestor of Line.ResponsibilityCenter
	-- Account.IsNoted must match that computed by system (from Agent.IsNoted). If No agent is specified, return all
	-- Account.AgentDefinition must match that of Agent
	-- Account.Identifier might help uniquely identify, but let us postpone it

	[AccountTypeParentCode]		NVARCHAR (255)		NOT NULL,
	[AgentDefinitionList]		NVARCHAR (1024),
	[CurrencySource]			SMALLINT			NOT NULL DEFAULT -1,
	[AgentSource]				SMALLINT			NOT NULL DEFAULT -1,
	[ResourceSource]			SMALLINT			NOT NULL DEFAULT -1,
	[EntryTypeCode]				NVARCHAR (255),
--	[NotedAgentDefinitionSource]SMALLINT			NOT NULL DEFAULT -1, -- -1: n/a, 1: set from line 2: from entry
	[NotedAgentDefinitionId]	NVARCHAR (50),

	[MonetaryValueSource]		SMALLINT			NOT NULL DEFAULT 2,
	[QuantitySource]			SMALLINT			NOT NULL DEFAULT -1,
	[ExternalReferenceSource]	SMALLINT			NOT NULL DEFAULT -1,
	[AdditionalReferenceSource]	SMALLINT			NOT NULL DEFAULT -1,
	[NotedAgentSource]			SMALLINT			NOT NULL DEFAULT -1,
	[NotedAmountSource]			SMALLINT			NOT NULL DEFAULT -1,
	[DueDateSource]				SMALLINT			NOT NULL DEFAULT -1
);
-- The behavior of the manual line is driven by the account.
-- There is a special case, where 
-- [Direction] = SIGN ([Debit]) + SIGN([Credit]), [Value] = [Debit]-[Credit]
-- IF [Direction] = 1 THEN [Debit] = [Direction] * SIGN([Value]), [Credit] = 0
-- IF [Direction] = -1 THEN [Debit] = 0, [Credit] = - [Direction] * SIGN([Value])
-- NB: Debit & Credit Cannot be both non-zero. If both are zero, we set direction to +1.
INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
0,N'ManualLine', N'Adjustment',		N'تسوية',		N'Adjustments',	N'تسويات');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],		[Label],		[Label2]) VALUES
(0,0,0,		N'Line.Memo',		N'Memo',		N'البيان'), -- only if it appears,
(1,0,1,		N'Entry[0].Account',N'Account',		N'الحساب'),
(2,0,2,		N'Entry[0].Value',	N'Debit',		N'مدين'), -- see special case
(3,0,3,		N'Entry[0].Value',	N'Credit',		N'دائن'),
-- Properties shown are as follows:
-- Currency and monetary value, if Account Currency is <> functional
-- Resource if account is smart and Account.[Resource Classification] is not null
-- Agent if account is smart and Account.[Agent Definition] is not null
-- Account Identifier if Account is smart and Account.[Has Identifier] = 1
-- Based on Resource Definition: we show: Count, Mass, Volume, Time, Resource Identifier, Due Date
-- Additional dynamic properties based on the tuple (Contract Type, Agent Definition, Resource Classifitation) -- to be stored in table
(4,0,5,		N'Entry[0].Dynamic',N'Properties',	N'الواصفات');
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');
/*
INSERT @LineDefinitions(
[Id],					[TitleSingular],		[TitlePlural],		[AgentDefinitionId]) VALUES
(N'PurchaseInvoice',	N'Purchase Invoice',	N'Purchase Invoices',	N'suppliers');
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[Direction],	[AccountTypeId],		[AgentDefinitionId],[EntryClassificationSource],[NotedAgentDefinitionSource], [NotedAgentDefinitionId], [AgentSource],[ResourceSource],	[CurrencySource], [MonetaryValueSource], [ExternalReferenceSource], [AdditionalReferenceSource], [NotedAgentSource], [NotedAmountSource], [DueDateSource]) VALUES
(N'PurchaseInvoice',	0,			-1,			N'Payable',			N'tax-agencies',		-1,						0,								N'suppliers',				-1,					0,			0,					2,						1,						1,								6,					6,						1),
(N'PurchaseInvoice',	1,			+1,			N'AccruedExpense',	N'suppliers',			-1,						-1,								NULL,						1,					0,			0,					1,						1,						1,								-1,					-1,						-1),
(N'PurchaseInvoice',	2,			-1,			N'Payable',			N'suppliers',			-1,						-1,								NULL,						1,					0,			0,					8,						1,						1,								-1,					-1,						1);
INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex], [ColumnName],					[Label]) VALUES
(N'PurchaseInvoice',	0,			N'Line.Description',		N'Description'), 
(N'PurchaseInvoice',	1,			N'Line.ExternalReference',	N'Invoice #'), 
(N'PurchaseInvoice',	2,			N'Line.AgentId',			N'Supplier'),
--(N'PurchaseInvoice',	3,			N'Line.Currency',			N'Currency'),
(N'PurchaseInvoice',	4,			N'Line.MonetaryAmount',		N'Price Excl. VAT'),
(N'PurchaseInvoice',	5,			N'Entry[0].MonetaryAmount',	N'VAT'),
(N'PurchaseInvoice',	6,			N'Entry[2].MonetaryAmount',	N'Total'),
(N'PurchaseInvoice',	7,			N'Entry[2].DueDate',		N'Due Date')
;
*/
-- NB: requisitions could be for payment towards something approved. Or it could be for a new purchase
-- when it is for a new purchase, the document must have two tabs: payment details, and purchase details
-- AgentDefinition is filtered by AccountType.AgentDefinitionList

INSERT @LineDefinitions([Index],
[Id],				[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
2,N'CashPayment',	N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionList], [AgentSource],[ResourceSource],[CurrencySource], [MonetaryValueSource], [ExternalReferenceSource], [NotedAgentSource]) VALUES
(0,2,0,	-1,		N'CashAndCashEquivalents',	N'banks,cashiers',		2,				-1,				1,				2,						2,								2);

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],					[Label],				[Label2]) VALUES
(0,2,0,		N'Line.Description',			N'Description',			N'البيان'), 
(1,2,1,		N'Entry[1].MonetaryAmount',		N'Pay Amount',			N'المبلغ'), 
(2,2,2,		N'Line.CurrencyId',				N'Pay Currency',		N'العملة'),
(3,2,3,		N'Entry[1].NotedAgentName',		N'Beneficiary',			N'المستفيد'),
(4,2,4,		N'Entry[1].EntryTypeId',		N'Purpose',				N'الغرض'),
(5,2,5,		N'Entry[1].AgentId',			N'Bank/Cashier',		N'البنك/الخزنة'),
(6,2,6,		N'Entry[1].ExternalReference',	N'Check #/Receipt #',	N'رقم الشيك/رقم الإيصال'),
(7,2,7,		N'Entry[1].NotedDate'	,		N'Check Date',			N'تاريخ الشيك')
;

INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,2,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,2,-3,	N'Other reasons',		N'أسباب أخرى');
/*
-- NB: We defined a Pettycash payment to separate the business rules
INSERT @LineDefinitions(
[Id],					[TitleSingular],	[TitlePlural]) VALUES
(N'PettyCashPayment',	N'Petty Cash Payment',		N'Petty Cash Payments')
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[AccountSource], [AccountTypeId], [AgentDefinitionSource], [ResourceSource], [EntryClassificationSource],[MonetaryValueSource], [QuantitySource], [NotedAgentSource], [NotedAmountSource]) VALUES
(N'PettyCashPayment',		0,			4,				N'Cash',			1,								-1,					1,					1,				-1,					-1,						-1);
INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex], [ColumnName],								[Label]) VALUES
(N'PettyCashPayment',		0,		N'Line.Description',					N'Description'), 
(N'PettyCashPayment',		1,		N'Entry[1].MonetaryAmount',				N'Pay Amount'), 
(N'PettyCashPayment',		2,		N'Entry[1].CurrencyId',					N'Pay Currency'),
(N'PettyCashPayment',		3,		N'Entry[1].AdditionalReference',		N'Beneficiary'),
(N'PettyCashPayment',		4,		N'Entry[1].EntryClassification',		N'Purpose'),
(N'CashPayment',			5,		N'Entry[1].AgentId',					N'Cashier'), -- TODO: Read it from document
(N'PettyCashPayment',		6,		N'Entry[1].ExternalReference',			N'Receipt #')
;

-- TODO: this is still unfinished
INSERT @LineDefinitions(
[Id],				[TitleSingular],	[TitlePlural]) VALUES
(N'TaxWithholding',	N'Tax Withholding',	N'Tax Withholdings');
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[AccountSource], [AccountTypeId], [AgentDefinitionId],[AgentSource],[ResourceSource], [EntryClassificationSource],[MonetaryValueSource], [QuantitySource], [NotedAgentSource], [NotedAmountSource]) VALUES
(N'TaxWithholding',	0,			0,				N'Payable',				N'Supplier',				3,				-1,					-1,					1,				-1,					6,					6),
(N'TaxWithholding',	1,			4,				N'Payable',				N'TaxAgency',				2,				-1,					-1,					2,				-1,					-1,					-1);
INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex], [ColumnName],					[Label]) VALUES
(N'TaxWithholding',		0,			N'Line.Description',		N'Description'), 
(N'TaxWithholding',		1,			N'Line.ExternalReference',	N'Invoice #'), 
(N'TaxWithholding',		2,			N'Line.AgentId',			N'Supplier'),
(N'TaxWithholding',		3,			N'Line.Currency',			N'Currency'),
(N'TaxWithholding',		4,			N'Line.MonetaryAmount',		N'Price Excl. VAT'),
(N'TaxWithholding',		5,			N'Entry[0].MonetaryAmount',	N'VAT'),
(N'TaxWithholding',		6,			N'Entry[2].MonetaryAmount',	N'Total'),
(N'TaxWithholding',		7,			N'Entry[2].DueDate',		N'Due Date')
;
*/
-- consumable with invoice
--	durables with invoice
-- consumables without invoice
-- durables without invoice
-- Mise In use
-- inventory receipt with invoice
-- inventory receipt without invoice
-- inventory transfer
-- stock issue
-- fuel consumption
-- payroll lines (one per comlumn)
-- etc...

DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
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
		t.[TitlePlural3]	= s.[TitlePlural3],
		t.[SavedById]		= @UserId
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],	[TitleSingular],	[TitleSingular2], [TitleSingular3],		[TitlePlural],	[TitlePlural2],		[TitlePlural3])
    VALUES (s.[Id], s.[TitleSingular], s.[TitleSingular2], s.[TitleSingular3], s.[TitlePlural], s.[TitlePlural2], s.[TitlePlural3]);

MERGE [dbo].[LineDefinitionColumns] AS t
USING (
	SELECT
		LDC.[Id],
		LD.[Id] AS [LineDefinitionId],
		LDC.[SortKey],
		LDC.[ColumnName],
		LDC.[Label],
		LDC.[Label2],
		LDC.[Label3]
	FROM @LineDefinitionColumns LDC
	JOIN @LineDefinitions LD ON LDC.HeaderIndex = LD.[Index]
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED THEN
	UPDATE SET
		t.[SortKey]			= s.[SortKey],
		t.[ColumnName]		= s.[ColumnName],
		t.[Label]			= s.[Label],
		t.[Label2]			= s.[Label2],
		t.[Label3]			= s.[Label3]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([LineDefinitionId], [SortKey],	[ColumnName],	[Label],	[Label2],	[Label3])
    VALUES (s.[LineDefinitionId], s.[SortKey], s.[ColumnName], s.[Label], s.[Label2], s.[Label3]);

MERGE [dbo].[LineDefinitionEntries] AS t
USING (
	SELECT
		LDE.[Id],
		LDE.[EntryNumber],
		LDE.[Direction]	,
		LDE.[AccountTypeParentCode]	,
		LDE.[AgentDefinitionList],
		LDE.[CurrencySource],
		LDE.[AgentSource],
		LDE.[ResourceSource],
		LDE.[EntryTypeCode],
		LDE.[NotedAgentDefinitionId],
		LDE.[MonetaryValueSource],
		LDE.[QuantitySource],
		LDE.[ExternalReferenceSource],
		LDE.[AdditionalReferenceSource]	,
		LDE.[NotedAgentSource],
		LDE.[NotedAmountSource],
		LDE.[DueDateSource]	
	FROM @LineDefinitionEntries LDE
	JOIN @LineDefinitions LD ON LDE.HeaderIndex = LD.[Index]
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED THEN
	UPDATE SET
		t.[EntryNumber]				= s.[EntryNumber],
		t.[Direction]				= t.[Direction],
		t.[AccountTypeParentCode]	= t.[AccountTypeParentCode],
		t.[AgentDefinitionList]		= t.[AgentDefinitionList],
		t.[CurrencySource]			= t.[CurrencySource],
		t.[AgentSource]				= t.[AgentSource],
		t.[ResourceSource]			= t.[ResourceSource],
		t.[EntryTypeCode]			= t.[EntryTypeCode],
		t.[NotedAgentDefinitionId]	= t.[NotedAgentDefinitionId],
		t.[MonetaryValueSource]		= t.[MonetaryValueSource],
		t.[QuantitySource]			= t.[QuantitySource],
		t.[ExternalReferenceSource]	= t.[ExternalReferenceSource],
		t.[AdditionalReferenceSource]= t.[AdditionalReferenceSource],
		t.[NotedAgentSource]		= t.[NotedAgentSource],
		t.[NotedAmountSource]		= t.[NotedAmountSource],
		t.[DueDateSource]			= t.[DueDateSource]	
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT (
		[EntryNumber],
		[Direction],
		[AccountTypeParentCode]	,
		[AgentDefinitionList],
		[CurrencySource],
		[AgentSource],
		[ResourceSource],
		[EntryTypeCode],
		[NotedAgentDefinitionId],
		[MonetaryValueSource],
		[QuantitySource],
		[ExternalReferenceSource],
		[AdditionalReferenceSource]	,
		[NotedAgentSource],
		[NotedAmountSource],
		[DueDateSource]	
	)
    VALUES (
		s.[EntryNumber],
		s.[Direction],
		s.[AccountTypeParentCode],
		s.[AgentDefinitionList],
		s.[CurrencySource],
		s.[AgentSource],
		s.[ResourceSource],

		s.[EntryTypeCode],
		s.[NotedAgentDefinitionId],
		s.[MonetaryValueSource],
		s.[QuantitySource],
		s.[ExternalReferenceSource],
		s.[AdditionalReferenceSource],
		s.[NotedAgentSource],
		s.[NotedAmountSource],
		s.[DueDateSource]	
	);

MERGE [dbo].[LineDefinitionStateReasons] AS t
USING (
	SELECT
		LDSR.[Id],
		LD.[Id] AS [LineDefinitionId],
		LDSR.[StateId],
		LDSR.[Name],
		LDSR.[Name2],
		LDSR.[Name3]
	FROM @LineDefinitionStateReasons LDSR
	JOIN @LineDefinitions LD ON LDSR.HeaderIndex = LD.[Index]
)AS s
ON s.Id = t.Id
WHEN MATCHED THEN
	UPDATE SET
		t.[LineDefinitionId]= s.[LineDefinitionId],
		t.[StateId]			= s.[StateId],
		t.[Name]			= s.[Name],
		t.[Name2]			= s.[Name2],
		t.[Name3]			= s.[Name3]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([LineDefinitionId],		[StateId], [Name],		[Name2], [Name3])
    VALUES (s.[LineDefinitionId], s.[StateId], s.[Name], s.[Name2], s.[Name3]);

IF @DebugLineDefinitions = 1
BEGIN
	SELECT * FROM dbo.LineDefinitions;
	SELECT * FROM dbo.LineDefinitionEntries;
END