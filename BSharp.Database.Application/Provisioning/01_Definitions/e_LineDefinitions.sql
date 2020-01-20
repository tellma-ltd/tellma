DECLARE @LineDefinitions dbo.LineDefinitionList;
DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];

INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
0,N'ManualLine', N'Adjustment',		N'تسوية',		N'Adjustments',	N'تسويات');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],		[Label],		[Label2]) VALUES
(0,0,0,		N'Line.Memo',		N'Memo',		N'البيان'), -- only if it appears,
(1,0,1,		N'Entry[0].Account',N'Account',		N'الحساب'),
(2,0,2,		N'Entry[0].Value',	N'Debit',		N'مدين'), -- see special case
(3,0,3,		N'Entry[0].Value',	N'Credit',		N'دائن'),
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
(0,2,1,	-1,		N'CashAndCashEquivalents',	N'banks,cashiers',		2,				-1,				1,				2,						2,								2);

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

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'AgentDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF @DebugLineDefinitions = 1
BEGIN
	SELECT * FROM dbo.LineDefinitions;
	SELECT * FROM dbo.LineDefinitionEntries;
END