IF @DB = N'101' -- Banan SD, USD, en
BEGIN
INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitlePlural]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],		[Label],		[IsRequiredForStateId],
												[IsReadOnlyFromStateId]) VALUES
(0,0,0,		N'Line.Memo',		N'Memo',		5,4), -- only if it appears,
(1,0,1,		N'Entry[0].Account',N'Account',		3,4),
(2,0,2,		N'Entry[0].Value',	N'Debit',		3,4), -- see special case
(3,0,3,		N'Entry[0].Value',	N'Credit',		3,4),
(4,0,5,		N'Entry[0].Dynamic',N'Properties',	3,4);

INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');

INSERT @LineDefinitions([Index],
[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES
(1,N'PurchaseInvoice',	N'Purchase Invoice',	N'فاتورة مشتريات',	N'Purchase Invoices',	N'فواتير مشتريات');

UPDATE @LineDefinitions
SET [Script] = N'
	SET NOCOUNT ON
	DECLARE @ProcessedWideLines WideLineList;

	INSERT INTO @ProcessedWideLines
	SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[ResponsibilityCenterId1] = [ResponsibilityCenterId0],
		[ResponsibilityCenterId2] = [ResponsibilityCenterId0],
		[AgentId2]	= [AgentId1],
		[CurrencyId1] = [CurrencyId0],
		[CurrencyId2] = [CurrencyId0],
		[NotedAgentSource0] = [AgentId1],
		[AccountIdentifier1] = [ExternalReference0],
		[MonetaryValue1] = [NotedAmount0],
		[MonetaryValue2] = [NotedAmount0] + [MonetaryAmount0]
	-----
	SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],[AccountTypeParentCode],					[AgentDefinitionList],	[ResponsibilityTypeList]) VALUES
(0,1,0,+1,	N'ValueAddedTaxPayables',					NULL,					N'Investment'),
(1,1,1,+1,	N'Accruals',								N'suppliers',			NULL),
(2,1,2,-1,	N'TradeAndOtherPayablesToTradeSuppliers',	NULL,					NULL);

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],				[Label],				[Label2],				[IsRequiredForStateId],
																						[IsReadOnlyFromStateId]) VALUES
(0,1,0,	N'Line.Memo',					N'Memo',				N'البيان',				1,5), 
(1,1,1,	N'Entry[0].NotedDate',			N'Invoice Date',		N'تاريخ الفاتورة',		3,5), 
(2,1,2,	N'Entry[0].ExternalReference',	N'Invoice #',			N'رقم الفاتورة',		3,5), 
(3,1,3,	N'Entry[1].AgentId',			N'Supplier',			N'المورد',				3,4),
(4,1,4,	N'Entry[0].CurrencyId',			N'Currency',			N'العملة',				1,4),
(5,1,5,	N'Entry[0].NotedAmount',		N'Price Excl. VAT',		N'المبلغ قبل الضريية',	1,4),
(6,1,6,	N'Entry[0].MonetaryValue',		N'VAT',					N'القيمة المضافة',		1,4),
(7,1,7,	N'Entry[2].MonetaryValue',		N'Total',				N'المبلغ بعد الضريبة',	1,1),
(8,1,8,	N'Entry[2].DueDate',			N'Due Date',			N'تاريخ الاستحقاق',		1,4),
(9,1,9,	N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',N'مركز المسؤولية',0,4)
;

-- NB: requisitions could be for payment towards something approved. Or it could be for a new purchase
-- when it is for a new purchase, the document must have two tabs: payment details, and purchase details

INSERT @LineDefinitions([Index],
[Id],				[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
2,N'CashPayment',	N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionList], [ResponsibilityTypeList]) VALUES
(0,2,0,	-1,		N'CashAndCashEquivalents',	N'banks,employees',		N'Investment');

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],						[Label],					[Label2],					[IsRequiredForStateId],
																										[IsReadOnlyFromStateId]) VALUES
(0,2,0,		N'Line.Memo',						N'Memo',					N'البيان',					1,2),
(1,2,1,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',					1,2),
(2,2,2,		N'Entry[0].MonetaryValue',			N'Pay Amount',				N'المبلغ',					1,2), 
(3,2,3,		N'Entry[0].NotedAgentName',			N'Beneficiary',				N'المستفيد',				3,4),
(4,2,4,		N'Entry[0].EntryTypeId',			N'Purpose',					N'الغرض',					1,4),
(5,2,5,		N'Entry[0].AgentId',				N'Bank/Cashier',			N'البنك/الخزنة',			3,4),
(6,2,6,		N'Entry[0].AccountIdentifier',		N'Account Identifier',		N'تمييز الحساب',			3,4),
(7,2,7,		N'Entry[0].ExternalReference',		N'Check #/Receipt #',		N'رقم الشيك/رقم الإيصال',	3,4),
(8,2,8,		N'Entry[0].NotedDate'	,			N'Check Date',				N'تاريخ الشيك',				3,4),
(9,2,9,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',			1,4)
;

INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,2,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,2,-3,	N'Other reasons',		N'أسباب أخرى');

INSERT @LineDefinitions([Index],
[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
3,N'PettyCashPayment',	N'Petty Cash Payment',	N'دفعية نثرية',		N'Petty Cash Payments',	N'دفعيات النثرية');

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionList], [ResponsibilityTypeList]) VALUES
(0,3,0,-1,		N'CashAndCashEquivalents',	N'custodies',			N'Investment');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],						[Label],					[Label2],			[IsRequiredForStateId],
																								[IsReadOnlyFromStateId]) VALUES
(0,3,0,		N'Entry[0].NotedDate',				N'Date',					N'التاريخ',			1,4), 
(1,3,1,		N'Line.Memo',						N'Memo',					N'البيان',			1,4),
(2,3,2,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',			1,2), 
(3,3,3,		N'Entry[0].MonetaryValue',			N'Pay Amount',				N'المبلغ',			1,2), 
(4,3,4,		N'Entry[0].NotedAgentName',			N'Beneficiary',				N'المستفيد',		1,2),
(5,3,5,		N'Entry[0].EntryTypeId',			N'Purpose',					N'الغرض',			4,4),
(6,3,6,		N'Entry[0].AgentId',				N'Petty Cash Custodian',	N'أمين العهدة',		3,4),
(7,3,7,		N'Entry[0].ExternalReference',		N'Receipt #',				N'رقم الإيصال',		3,4),
(8,3,8,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',	4,4);

-- GRIV
INSERT @LineDefinitions([Index],
[Id],							[TitleSingular],				[TitleSingular2],	[TitlePlural],					[TitlePlural2],			[AgentDefinitionList], [ResponsibilityTypeList]) VALUES (
6,N'GoodsReceiptIssueVoucher',	N'Goods Receipt/Issue Voucher',	N'استلام مستخدم',	N'Goods Receipt/Issue Voucher',	N'استلامات مستخدمين',	N'suppliers',			N'Investment');
UPDATE @LineDefinitions
SET [Script] = N'
	SET NOCOUNT ON
	DECLARE @ProcessedWideLines WideLineList;

	INSERT INTO @ProcessedWideLines
	SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[EntryTypeId0]				= (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = ''InventoryTransferExtension''),
		[MonetaryAmount1]			= [MonetaryAmount0],
		[ResponsibilityCenterId1]	= [ResponsibilityCenterId0],
		[CurrencyId1]				= [CurrencyId0]
	-----
	SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 4;

INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionList], [ResponsibilityTypeList]) VALUES
(0,6,0,+1,		N'OtherInventories',		NULL,					NULL), -- We may need to add GRIV Inventory underneath, or instead
(1,6,1,-1,		N'Accruals',				NULL,					NULL); -- we need functionality to fill one tab based on info in the other tab

INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],						[Label],				[Label2],				[IsRequiredForStateId],
																								[IsReadOnlyFromStateId]) VALUES
(0,5,0,		N'Line.Memo',						N'Memo',				N'البيان',				1,5), 
(1,5,1,		N'Entry[1].AgentId',				N'Supplier',			N'المورد',				3,4),
(2,5,2,		N'Entry[0].AgentId',				N'Beneficiary',			N'المستفيد',			3,4),
(3,5,3,		N'Entry[0].ResourceId',				N'Item',				N'الصنف',				1,4),
(4,5,4,		N'Entry[0].Quantity',				N'Quantity',			N'الكمية',				1,4),
(5,5,5,		N'Entry[0].UnitId',					N'Unit',				N'الوحدة',				1,4),
(6,5,6,		N'Entry[0].CurrencyId',				N'Currency',			N'العملة',				4,4),
(7,5,7,		N'Entry[0].MonetaryAmount',			N'Price Excl. VAT',		N'المبلغ قبل الضريية',	4,4),
(8,5,8,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',N'مركز المسؤولية',	0,4);

END