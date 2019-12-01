
DECLARE @LineDefinitions TABLE (
	[Id]								NVARCHAR (50)			PRIMARY KEY,
	[TitleSingular]						NVARCHAR (255) NOT NULL,
	[TitleSingular2]					NVARCHAR (255),
	[TitleSingular3]					NVARCHAR (255),
	[TitlePlural]						NVARCHAR (255) NOT NULL,
	[TitlePlural2]						NVARCHAR (255),
	[TitlePlural3]						NVARCHAR (255),
	[AgentDefinitionId]					NVARCHAR (50),--	REFERENCES dbo.AgentDefinitions([Id]),
	[Script]							NVARCHAR (MAX)
);

DECLARE @LineDefinitionEntries TABLE (
	[LineDefinitionId]			NVARCHAR (50),
	[EntryNumber]				INT,
	[Direction]					SMALLINT,
	-- Source = -1 (n/a), 0 (get from line def), 1 (get from Entry), 2 (get from line), 3 (from account), 4-7 (from other entry data), 8 (from balancing), 9 (from bll script)
	-- 4: from resource/agent/currency etc./5 from (Resource, Account Type), 6: from Counter/Contra/Related in Line, 7:
	-- Account is invisible in a tab, unless the source specifies it is entered by user. or in Manual line
	
	-- Account Group Properties
	[AccountId]					INT, -- invisible, except in 

	[AccountTypeId]				NVARCHAR (50), -- if account is entered by user, all the group properties get set.

	[AgentDefinitionSource]		SMALLINT			NOT NULL DEFAULT 0, --  -1: n/a, 0:set from line def, 3: from account
	[AgentDefinitionId]			NVARCHAR (50),

	[ResourceClassificationSource]	SMALLINT			NOT NULL DEFAULT 0, -- -1: n/a,  0:set from line def, 3: from account
	[ResourceClassificationCode]NVARCHAR (255),

	[LiquiditySource]			SMALLINT			NOT NULL DEFAULT -1, -- -1: n/a, 0:set from line def, 3: from account
	[IsCurrent]					BIT,

	-- Concluded from Agent. User will not figure out
--	[AgentRelatedness]			SMALLINT			NOT NULL DEFAULT 0, -- -1: n/a,  0:set from line def, 3: from account

	[EntryClassificationSource]	SMALLINT			NOT NULL DEFAULT 0,
	[EntryClassificationCode]	NVARCHAR (255),

	[RelatedAgentDefinitionSource]SMALLINT			NOT NULL DEFAULT -1, --  -1: n/a, 0:set from line def, 3: from account
	[RelatedAgentDefinitionId]	NVARCHAR (50),

	-- Account Details Properties
	[AgentSource]				SMALLINT			NOT NULL DEFAULT 1, --  -1: n/a, 3: from account
	[AgentId]					INT,--				REFERENCES dbo.Agents([Id]),	-- fixed in the case of ERCA, e.g., VAT

	[ResourceSource]			SMALLINT			NOT NULL DEFAULT 1,
	[ResourceId]				INT,--				REFERENCES dbo.Resources([Id]),	-- Fixed in the case of unallocated expense
	
	[CurrencySource]				SMALLINT			NOT NULL DEFAULT 2,
	[CurrencyId]						NCHAR (3),--		REFERENCES dbo.Currencies([Id]),	-- Fixed in the case of unallocated expense
	
	[MonetaryValueSource]		SMALLINT			NOT NULL DEFAULT 1,
	[QuantitySource]			SMALLINT			NOT NULL DEFAULT 1,
	[ExternalReferenceSource]	SMALLINT			NOT NULL DEFAULT 2,
	[AdditionalReferenceSource]	SMALLINT			NOT NULL DEFAULT 2,
	[RelatedAgentSource]		SMALLINT			NOT NULL DEFAULT 2,
	[RelatedAmountSource]		SMALLINT			NOT NULL DEFAULT 2,
	[DueDateSource]				SMALLINT			NOT NULL DEFAULT 1
);

DECLARE @LineDefinitionColumns TABLE (
	[LineDefinitionId]			NVARCHAR (50),
	[SortIndex]					TINYINT,
	[ColumnName]				NVARCHAR (50),
	[Label]						NVARCHAR (50),
	[Label2]					NVARCHAR (50),
	[Label3]					NVARCHAR (50)
);


-- The behavior of the manual line is driven by the account.
INSERT @LineDefinitions([Id], [TitleSingular], [TitlePlural]) VALUES (N'ManualLine', N'Adjustment', N'Adjustments');
-- There is a special case, where 
-- [Direction] = SIGN ([Debit]) + SIGN([Credit]), [MonetaryAmount] = [Debit]-[Credit]
-- IF [Direction] = 1 THEN [Debit] = [Direction] * SIGN([MonetaryAmount]), [Credit] = 0
-- IF [Direction] = -1 THEN [Debit] = 0, [Credit] = - [Direction] * SIGN([MonetaryAmount])
-- NB: Debit & Credit Cannot be both non-zero. If both are zero, we set direction to +1.

INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex],	[ColumnName],				[Label]) VALUES
(N'ManualLine',			0,			N'Line.Memo',				N'Memo'), -- only if it appears,
(N'ManualLine',			1,			N'Entry[0].Account',		N'Account'),
(N'ManualLine',			2,			N'Entry[0].Currency',		N'Currency'), -- only if it appears,
(N'ManualLine',			3,			N'Entry[0].MonetaryAmount',	N'Debit'), -- see special case
(N'ManualLine',			4,			N'Entry[0].MonetaryAmount',	N'Credit'),
(N'ManualLine',			5,			N'Entry[0].Dynamic',		N'Properties')
;
/*
INSERT @LineDefinitions(
[Id],					[TitleSingular],		[TitlePlural]) VALUES
(N'PurchaseInvoice',	N'Purchase Invoice',	N'Purchase Invoices');
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[AccountSource], [AccountTypeId], [AgentDefinitionId],[AgentSource],[ResourceSource], [EntryClassificationSource],[MonetaryValueSource], [QuantitySource], [RelatedAgentSource], [RelatedAmountSource]) VALUES
(N'PurchaseInvoice',	0,			0,				N'TaxPayable',		N'TaxAgency',				3,				-1,					-1,					1,				-1,					6,					6),
(N'PurchaseInvoice',	1,			4,				N'Accrual',			N'Supplier',				2,				-1,					-1,					2,				-1,					-1,					-1),
(N'PurchaseInvoice',	2,			4,				N'Payable',			N'Supplier',				2,				-1,					-1,					8,				-1,					-1,					-1);
INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex], [ColumnName],					[Label]) VALUES
(N'PurchaseInvoice',	0,			N'Line.Description',		N'Description'), 
(N'PurchaseInvoice',	1,			N'Line.ExternalReference',	N'Invoice #'), 
(N'PurchaseInvoice',	2,			N'Line.AgentId',			N'Supplier'),
(N'PurchaseInvoice',	3,			N'Line.Currency',			N'Currency'),
(N'PurchaseInvoice',	4,			N'Line.MonetaryAmount',		N'Price Excl. VAT'),
(N'PurchaseInvoice',	5,			N'Entry[0].MonetaryAmount',	N'VAT'),
(N'PurchaseInvoice',	6,			N'Entry[2].MonetaryAmount',	N'Total'),
(N'PurchaseInvoice',	7,			N'Entry[2].DueDate',		N'Due Date')
;
-- NB: requisitions could be for payment towards something approved. Or it could be for a new purchase
-- when it is for a new purchase, the document must have two tabs: payment details, and purchase details
-- AgentDefinition is filtered by AccountType.AgentDefinitionList
INSERT @LineDefinitions(
[Id],				[TitleSingular],	[TitlePlural]) VALUES
(N'CashPayment',	N'Payment',			N'Payments')
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[AccountSource], [AccountTypeId], [AgentDefinitionId], [ResourceSource], [EntryTypeSource],[MonetaryValueSource], [QuantitySource], [RelatedAgentSource], [RelatedAmountSource]) VALUES
(N'CashPayment',		0,			4,				N'Cash',			N'PettyCashCustody',		-1,					1,				1,						-1,					-1,						-1);
INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex], [ColumnName],								[Label]) VALUES
(N'CashPayment',		0,			N'Line.Description',					N'Description'), 
(N'CashPayment',		1,			N'Entry[1].MonetaryAmount',				N'Pay Amount'), 
(N'CashPayment',		2,			N'Entry[1].CurrencyId',					N'Pay Currency'),
(N'CashPayment',		3,			N'Entry[1].AdditionalReference',		N'Beneficiary'),
(N'CashPayment',		4,			N'Entry[1].EntryClassification',		N'Purpose'),
(N'CashPayment',		5,			N'Entry[1].AgentDefinitionId',	N'Payment From'),
(N'CashPayment',		6,			N'Entry[1].AgentId',					N'Bank/Cashier'),
(N'CashPayment',		7,			N'Entry[1].ExternalReference',			N'Check #/Receipt #'),
(N'CashPayment',		8,			N'Entry[1].DueDate',					N'Check Date')
;

-- NB: We defined a Pettycash payment to separate the business rules
INSERT @LineDefinitions(
[Id],					[TitleSingular],	[TitlePlural]) VALUES
(N'PettyCashPayment',	N'Petty Cash Payment',		N'Petty Cash Payments')
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[AccountSource], [AccountTypeId], [AgentDefinitionSource], [ResourceSource], [EntryClassificationSource],[MonetaryValueSource], [QuantitySource], [RelatedAgentSource], [RelatedAmountSource]) VALUES
(N'PettyCashPayment',		0,			4,				N'Cash',			1,								-1,					1,					1,				-1,					-1,						-1);
INSERT INTO @LineDefinitionColumns
([LineDefinitionId], [SortIndex], [ColumnName],								[Label]) VALUES
(N'PettyCashPayment',		0,		N'Line.Description',					N'Description'), 
(N'PettyCashPayment',		1,		N'Entry[1].MonetaryAmount',				N'Pay Amount'), 
(N'PettyCashPayment',		2,		N'Entry[1].CurrencyId',					N'Pay Currency'),
(N'PettyCashPayment',		3,		N'Entry[1].AdditionalReference',		N'Beneficiary'),
(N'PettyCashPayment',		4,		N'Entry[1].EntryClassification',					N'Purpose'),
(N'CashPayment',			5,		N'Entry[1].AgentId',					N'Cashier'), -- TODO: Read it from document
(N'PettyCashPayment',		6,		N'Entry[1].ExternalReference',			N'Receipt #')
;

-- TODO: this is still unfinished
INSERT @LineDefinitions(
[Id],				[TitleSingular],	[TitlePlural]) VALUES
(N'TaxWithholding',	N'Tax Withholding',	N'Tax Withholdings');
INSERT INTO @LineDefinitionEntries
([LineDefinitionId], [EntryNumber],[AccountSource], [AccountTypeId], [AgentDefinitionId],[AgentSource],[ResourceSource], [EntryClassificationSource],[MonetaryValueSource], [QuantitySource], [RelatedAgentSource], [RelatedAmountSource]) VALUES
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