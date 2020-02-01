DECLARE @LineDefinitions dbo.LineDefinitionList;
DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];

INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES
(0,N'ManualLine', N'Adjustment',		N'تسوية',		N'Adjustments',	N'تسويات');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],		[Label],		[Label2],		[IsRequiredForStateId],
																[IsReadOnlyFromStateId]) VALUES
(0,0,0,		N'Line.Memo',		N'Memo',		N'البيان',		5,4), -- only if it appears,
(1,0,1,		N'Entry[0].Account',N'Account',		N'الحساب',		3,4),
(2,0,2,		N'Entry[0].Value',	N'Debit',		N'مدين',		3,4), -- see special case
(3,0,3,		N'Entry[0].Value',	N'Credit',		N'دائن',		3,4),
(4,0,5,		N'Entry[0].Dynamic',N'Properties',	N'الواصفات',	3,4);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');

INSERT @LineDefinitions([Index],
[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2],		[AgentDefinitionId]) VALUES
(1,N'PurchaseInvoice',	N'Purchase Invoice',	N'فاتورة مشتريات',	N'Purchase Invoices',	N'فواتير مشتريات',	N'suppliers');

UPDATE @LineDefinitions
SET [Script] = N'
	SET NOCOUNT ON
	DECLARE @ProcessedWideLines WideLineList;

	INSERT INTO @ProcessedWideLines
	SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[Value0] = [Value1] * 0.15,
		[Value2] = [Value1] * 1.15,
		[NotedAmount0] = [Value1]
	-----
	SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;
-- Source = -1 (n/a), 1 (get from line), 2 (get from entry), 4-7 (from other entry data), 8 (from balancing), 9 (from bll script)
-- 4: from resource/agent/currency etc./5 from (Resource, Account Type), 6: from Counter/Contra/Noted in Line, 7:
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],[AccountTypeParentCode],		[AgentDefinitionList],[ResponsibilityCenterSource], [AgentSource],	[CurrencySource],		[NotedAgentSource]) VALUES
(0,1,0,+1,	N'ValueAddedTaxPayables',				NULL,			N'Line.ResponsibilityCenterId',	NULL,			N'FunctionalCurrencyId',N'Line.AgentId'),
(1,1,1,+1,	N'Accruals',							N'suppliers',	N'Line.ResponsibilityCenterId',	N'Line.AgentId',N'FunctionalCurrencyId',NULL),
(2,1,2,-1,	N'TradeAndOtherPayablesToTradeSuppliers',N'suppliers',	N'Line.ResponsibilityCenterId',	N'Line.AgentId',N'FunctionalCurrencyId',NULL);

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],				[Label],				[Label2],				[IsRequiredForStateId],
																						[IsReadOnlyFromStateId]) VALUES
(0,1,0,	N'Line.Memo',					N'Memo',				N'البيان',				1,5), 
(1,1,1,	N'Entry[0].ExternalReference',	N'Invoice #',			N'رقم الفاتورة',		3,5), 
(2,1,2,	N'Line.AgentId',				N'Supplier',			N'المورد',				3,4),
(3,1,3,	N'Entry[1].Value',				N'Price Excl. VAT',		N'المبلغ قبل الضريية',	1,4),
(4,1,4,	N'Entry[0].Value',				N'VAT',					N'القيمة المضافة',		1,1),
(5,1,5,	N'Entry[2].Value',				N'Total',				N'المبلغ بعد الضريبة',	1,1),-- script is needed to find sum
(6,1,6,	N'Entry[2].DueDate',			N'Due Date',			N'تاريخ الاستحقاق',		1,4),
(7,1,7,	N'Line.ResponsibilityCenterId',	N'Responsibility Center',N'مركز المسؤولية',	0,4)
;

-- NB: requisitions could be for payment towards something approved. Or it could be for a new purchase
-- when it is for a new purchase, the document must have two tabs: payment details, and purchase details
-- AgentDefinition is filtered by AccountType.AgentDefinitionList

INSERT @LineDefinitions([Index],
[Id],				[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
2,N'CashPayment',	N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionList], [CurrencySource]) VALUES
(0,2,0,	-1,		N'CashAndCashEquivalents',	N'banks,employees',	N'FunctionalCurrencyId');

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],					[Label],				[Label2],					[IsRequiredForStateId],
																								[IsReadOnlyFromStateId]) VALUES
(0,2,0,		N'Line.Memo',					N'Memo',				N'البيان',					1,2), 
(1,2,1,		N'Entry[0].MonetaryValue',		N'Pay Amount',			N'المبلغ',					1,2), 
(2,2,2,		N'Entry[0].NotedAgentName',		N'Beneficiary',			N'المستفيد',				3,4),
(3,2,3,		N'Entry[0].EntryTypeId',		N'Purpose',				N'الغرض',					1,2),
(4,2,4,		N'Entry[0].AgentId',			N'Bank/Cashier',		N'البنك/الخزنة',			3,4),
(5,2,5,		N'Entry[0].ExternalReference',	N'Check #/Receipt #',	N'رقم الشيك/رقم الإيصال',	3,4),
(6,2,6,		N'Entry[0].NotedDate'	,		N'Check Date',			N'تاريخ الشيك',				3,4),
(7,2,7,		N'Entry[0].ResponsibilityCenterId',N'Responsibility Center',N'مركز المسؤولية',		1,4)
;

INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,2,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,2,-3,	N'Other reasons',		N'أسباب أخرى');

-- Petty cash payment
-- Stock issue
-- Stock receipt
-- LC open
-- Shipping Docs Receipt

-- NB: We defined a Pettycash payment to separate the business rules
INSERT @LineDefinitions([Index],
[Id],				[TitleSingular],			[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
3,N'PettyCashPayment',	N'Petty Cash Payment',	N'دفعية نثرية',		N'Petty Cash Payments',	N'دفعيات النثرية');

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionList], [CurrencySource]) VALUES
(0,3,0,-1,		N'CashAndCashEquivalents',	N'employees',			N'FunctionalCurrencyId');

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],					[Label],					[Label2],			[IsRequiredForStateId],
																							[IsReadOnlyFromStateId]) VALUES
(0,3,0,		N'Line.Memo',					N'Memo',					N'البيان',			1,4), 
(1,3,1,		N'Entry[0].MonetaryValue',		N'Pay Amount',				N'المبلغ',			1,2), 
(2,3,2,		N'Entry[0].NotedAgentName',		N'Beneficiary',				N'المستفيد',		1,2),
(3,3,3,		N'Entry[0].EntryTypeId',		N'Purpose',					N'الغرض',			4,4),
(4,3,4,		N'Entry[0].AgentId',			N'Petty Cash Custodian',	N'أمين العهدة',		3,4),
(5,3,5,		N'Entry[0].ExternalReference',	N'Receipt #',				N'رقم الإيصال',		3,4),
(6,3,7,		N'Entry[0].ResponsibilityCenterId',N'Responsibility Center',N'مركز المسؤولية',	4,4)
;
/*
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