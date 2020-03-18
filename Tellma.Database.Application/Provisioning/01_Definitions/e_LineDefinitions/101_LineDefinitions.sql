IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--ManualLine 
INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitlePlural]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode]) VALUES
(0,0,+1,	N'StatementOfFinancialPositionAbstract');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],		[RequiredState],
																[ReadOnlyState],
																[InheritsFromHeader]) VALUES
(0,0,	N'Lines',	N'Memo',		0,			N'Memo',		5,4,1), -- only if it appears,
(1,0,	N'Entries',	N'Account',		0,			N'Account',		3,4,0),
(2,0,	N'Entries',	N'Value',		0,			N'Debit',		3,4,0), -- see special case
(3,0,	N'Entries',	N'Value',		0,			N'Credit',		3,4,0),
(4,0,	N'Entries',	N'Dynamic',		0,			N'Properties',	3,4,0);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');
--CashPaymentToSupplierAndPurchaseInvoiceVAT
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],[TitleSingular],		[TitleSingular2],			[TitlePlural],			[TitlePlural2]) VALUES
(1,1,N'CashPaymentToSupplierAndPurchaseInvoiceVAT',
						N'Cash Purchase',	N'شراء نقدي',	N'Cash Purchases',N'مشتريات نقدية');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = ISNULL([MonetaryValue0],0) + ISNULL([NotedAmount0],0),
		[CenterId0] = [CenterId1]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId],[NotedAgentDefinitionId],[EntryTypeCode]) VALUES
(0,1,+1,	N'ValueAddedTaxReceivables',1,		NULL,				N'suppliers',			NULL),
(1,1,-1,	N'CashAndCashEquivalents',	1,		N'cash-custodians',	NULL,					N'PaymentsToSuppliersForGoodsAndServices');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																								[ReadOnlyState],
																								[InheritsFromHeader]) VALUES
(0,1,	N'Lines',	N'Memo',				0,	N'Memo',				N'البيان',				1,5,1),
(1,1,	N'Entries',	N'NotedDate',			0,	N'Invoice Date',		N'تاريخ الفاتورة',		3,5,0), 
(2,1,	N'Entries',	N'ExternalReference',	0,	N'Invoice #',			N'رقم الفاتورة',		3,5,0), 
(3,1,	N'Entries',	N'NotedAgentId',		0,	N'Supplier',			N'المورد',				3,4,0),-- may inherit
(4,1,	N'Entries',	N'CurrencyId',			0,	N'Currency',			N'العملة',				1,2,1),
(5,1,	N'Entries',	N'NotedAmount',			0,	N'Price Excl. VAT',		N'المبلغ بدون ق.م.',	1,2,0),
(6,1,	N'Entries',	N'MonetaryValue',		0,	N'VAT',					N'ق.م.',				3,4,0),
(7,1,	N'Entries',	N'MonetaryValue',		1,	N'Total',				N'الإجمالي',				3,0,0),
(8,1,	N'Entries',	N'AgentId',				1,	N'Bank/Cashier',		N'البنك\الخزنة',		3,4,0),
(9,1,	N'Entries',	N'ExternalReference',	1,	N'Check/Receipt #',		N'رقم الشيك\الإيصال',	3,4,0),
(10,1,	N'Entries',	N'NotedDate',			1,	N'Check Date',			N'تاريخ الشيك',			5,5,0),
(11,1,	N'Entries',	N'CenterId',			1,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1);
--CashPaymentToOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],	[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES (
2,1,N'CashPaymentToOther',	N'Other Payment',	N'دفعية أخرى',		N'Other Payments',	N'دفعيات أخرى');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId]) VALUES
(0,2,	-1,	N'CashAndCashEquivalents',	1,		N'cash-custodians');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																								[ReadOnlyState],
																								[InheritsFromHeader]) VALUES
(0,2,	N'Lines',	N'Memo',				0,	N'Memo',				N'البيان',				1,2,1),
(1,2,	N'Entries',	N'CurrencyId',			0,	N'Currency',			N'العملة',				1,2,1),
(2,2,	N'Entries',	N'MonetaryValue',		0,	N'Pay Amount',			N'المبلغ',				1,2,0),
(3,2,	N'Entries',	N'NotedAgentName',		0,	N'Beneficiary',			N'المستفيد',			3,4,0),
(4,2,	N'Entries',	N'EntryTypeId',			0,	N'Purpose',				N'الغرض',				4,4,0),
(5,2,	N'Entries',	N'AgentId',				0,	N'Bank/Cashier',		N'البنك/الخزنة',		3,4,0),
(6,2,	N'Entries',	N'ExternalReference',	0,	N'Check #/Receipt #',	N'رقم الشيك/الإيصال',	3,4,0),
(7,2,	N'Entries',	N'NotedDate',			0,	N'Check Date',			N'تاريخ الشيك',			5,5,0),
(8,2,	N'Entries',	N'CenterId',			0,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1)
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,2,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,2,-3,	N'Other reasons',		N'أسباب أخرى');
--CashTransferExchange
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],		[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
103,1,N'CashTransferExchange',	N'Transfer/Exchange',	N'تحويل\صرف',		N'Transfers/Exchanges',	N'تحويلات\صرف');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentId0]	= [AgentId1],
		[NotedAgentId1]	= [AgentId0],
		[CenterId1] = [CenterId0]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 103;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId], [EntryTypeCode]) VALUES
(0,103,+1,	N'CashAndCashEquivalents',	1,		N'cash-custodians',	N'InternalCashTransferExtension'),
(1,103,-1,	N'CashAndCashEquivalents',	1,		N'cash-custodians',	N'InternalCashTransferExtension');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],			[Label2],			[RequiredState],
																						[ReadOnlyState],
																						[InheritsFromHeader]) VALUES
(0,103,	N'Lines',	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1),
(1,103,	N'Entries',	N'AgentId',				1,	N'From Account',	N'من حساب',			1,2,0),
(2,103,	N'Entries',	N'CurrencyId',			1,	N'From Currency',	N'من عملة',			1,2,0),
(3,103,	N'Entries',	N'MonetaryValue',		1,	N'From Amount',		N'من مبلغ',			1,3,0),
(4,103,	N'Entries',	N'AgentId',				0,	N'To Account',		N'إلى حساب',		1,2,0),
(5,103,	N'Entries',	N'CurrencyId',			0,	N'To Currency',		N'إلى عملة',		1,2,0),
(6,103,	N'Entries',	N'MonetaryValue',		0,	N'To Amount',		N'إلى مبلغ',		1,3,0),
(7,103,	N'Entries',	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1);
--CashReceiptFromOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],	[TitleSingular],	[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
3,1,N'CashReceiptFromOther',	N'Other Cash Receipt',	N'توريد آخر',	N'Other Cash Receipts',	N'توريدات أخرى');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId]) VALUES
(0,3,-1,	N'CashAndCashEquivalents',	1,		 N'cash-custodians');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																						[ReadOnlyState],
																						[InheritsFromHeader]) VALUES
(0,3,	N'Lines',	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1),
(1,3,	N'Entries',	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(2,3,	N'Entries',	N'MonetaryValue',		0,	N'Amount',			N'المبلغ',			1,2,0),
(3,3,	N'Entries',	N'NotedAgentName',		0,	N'Received from',	N'مستلم من',		3,4,0),
(4,3,	N'Entries',	N'EntryTypeId',			0,	N'Purpose',			N'الغرض',			4,4,0),
(5,3,	N'Entries',	N'AgentId',				0,	N'Bank/Cashier',	N'البنك/الخزنة',	3,4,1),
(6,3,	N'Entries',	N'ExternalReference',	0,	N'Check/Receipt #',	N'رقم الشيك/الإيصال',3,4,0),
(7,3,	N'Entries',	N'NotedDate',			0,	N'Check Date',		N'تاريخ الشيك',		5,5,0),
(8,3,	N'Entries',	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1);
--CashReceiptFromCustomerAndSalesInvoiceVAT
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],[TitleSingular],		[TitleSingular2],			[TitlePlural],	[TitlePlural2]) VALUES (
4,1,N'CashReceiptFromCustomerAndSalesInvoiceVAT',	
						N'Cash Sale w/VAT',	N'بيع نقدي + قيمة مضافة',	N'Cash Sales w/VAT',N'مبيعات نقدية + قيمة مضافة');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentId1]	= [AgentId2],
		[CurrencyId1] = [CurrencyId2],
		[NotedAmount0] = [MonetaryValue1] + [MonetaryValue2],
		[CenterId1] = [CenterId0],
		[CenterId2] = [CenterId0]
	-----
--	SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 4;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId],[EntryTypeCode]) VALUES
(0,4,+1,	N'CashAndCashEquivalents',	1,		N'cash-custodians',	N'ReceiptsFromSalesOfGoodsAndRenderingOfServices'),
(1,4,-1,	N'ValueAddedTaxPayables',	1,		NULL,				NULL),
(2,4,-1,	N'AccruedIncome',			1,		N'customers',		NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],			[Label2],				[RequiredState],
																							[ReadOnlyState],
																							[InheritsFromHeader]) VALUES
(0,4,	N'Lines',	N'Memo',				0,	N'Memo',			N'البيان',				1,5,1), 
(1,4,	N'Entries',	N'NotedDate',			1,	N'Invoice Date',	N'تاريخ الفاتورة',		3,5,0), 
(2,4,	N'Entries',	N'ExternalReference',	1,	N'Invoice #',		N'رقم الفاتورة',		3,5,0), 
(3,4,	N'Entries',	N'AgentId',				2,	N'Customer',		N'الزبون',				3,4,1),
(4,4,	N'Entries',	N'CurrencyId',			2,	N'Currency',		N'العملة',				1,2,1),
(5,4,	N'Entries',	N'MonetaryValue',		2,	N'Price Excl. VAT',	N'المبلغ بدون ق.م.',	1,2,0),
(6,4,	N'Entries',	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',				3,4,0),
(7,4,	N'Entries',	N'NotedAmount',			0,	N'Total',			N'الإجمالي',				3,4,0),
(8,4,	N'Entries',	N'AgentId',				0,	N'Bank/Cashier',	N'البنك\الخزنة',		3,4,1),
(9,4,	N'Entries',	N'CurrencyId',			0,	N'Currency Rcvd',	N'العملة المستلمة',	3,4,1),
(10,4,	N'Entries',	N'MonetaryValue',		0,	N'Amount Rcvd',		N'المبلغ المستلم',		3,4,0),
(11,4,	N'Entries',	N'ExternalReference',	0,	N'Check/Receipt #',	N'رقم الشيك\الإيصال',	3,4,0),
(12,4,	N'Entries',	N'NotedDate',			0,	N'Check Date',		N'تاريخ الشيك',			5,5,0),
(13,4,	N'Entries',	N'CenterId',			0,N'Invest. Ctr',		N'مركز الاستثمار',		4,4,1)
--CashReceiptFromCustomer
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],		[TitleSingular],		[TitleSingular2],	[TitlePlural],		[TitlePlural2],
[Description]) VALUES (
5,1,N'CashReceiptFromCustomer',	N'Customer Payment',	N'دفعية زبون',	N'Customer Payments',	N'دفعية زبائن',
N'For cash receipt from customers who are not paying VAT');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CenterId1] = [CenterId0]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 5;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId], [EntryTypeCode]) VALUES
(0,5,+1,	N'CashAndCashEquivalents',	1,		N'cash-custodians',	N'ReceiptsFromSalesOfGoodsAndRenderingOfServices'),
(1,5,-1,	N'TradeReceivables',		1,		N'customers',		NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																								[ReadOnlyState],
																								[InheritsFromHeader]) VALUES
(0,5,	N'Lines',	N'Memo',				0,	N'Memo',				N'البيان',				1,5,1), 
(1,5,	N'Entries',	N'AgentId',				1,	N'Customer',			N'الزبون',				3,4,1),
(2,5,	N'Entries',	N'MonetaryValue',		1,	N'Amount Due',			N'المطلوب',				3,4,0),
(3,5,	N'Entries',	N'CurrencyId',			1,	N'Currency Due',		N'عملة المطالبة',		3,4,0),
(4,5,	N'Entries',	N'MonetaryValue',		0,	N'Amount Paid',			N'المستلم',				3,4,0),
(5,5,	N'Entries',	N'CurrencyId',			0,	N'Currency Paid',		N'عملة الاستلام',			3,4,0),
(6,5,	N'Entries',	N'AgentId',				0,	N'Bank/Cashier',		N'البنك\الخزنة',		3,4,1),
(7,5,	N'Entries',	N'ExternalReference',	0,	N'Check/Receipt #',		N'رقم الشيك\الإيصال',	3,4,0),
(8,5,	N'Entries',	N'NotedDate',			0,	N'Check Date',			N'تاريخ الشيك',			5,5,0),
(9,5,	N'Entries',	N'CenterId',			0,	N'Invest. Ctr',			N'مركز الاستثمار',		4,4,0)
--LeaseOutIssue. TODO: Auto calculate Revenue based on AgentRates
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],[TitleSingular],			[TitleSingular2],		[TitlePlural],				[TitlePlural2],
[Description]) VALUES (
6,0,N'LeaseOutIssue',	N'Lease/Subscription V.',	N'إيجار -اشتراك ق.م',	N'Leases/Subscriptions V.',	N'إيجارات -اشتراكات ق.م',
N'For lease out of properties or software subscriptions with VAT. Indicates the rendering of service');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue1]	= [MonetaryValue0],
		[Time20]			= dbo.fn_DateAdd([UnitId0],[Quantity0],[Time10]) 
		[CurrencyId1]		= [CurrencyId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 6;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId]) VALUES
(0,6,+1,	N'AccruedIncome',			1,		N'customers'),
(1,6,-1,	N'Revenue',					1,		N'cost-objects');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,6,	N'Entries', N'AgentId',				0,	N'Customer',	N'الزبون',		1,4,0),
(1,6,	N'Entries', N'AgentId',				1,	N'System',		N'النظام',		1,4,0),
(2,6,	N'Entries', N'ResourceId',			0,	N'Service',		N'الخدمة',		1,4,0),
(3,6,	N'Entries', N'Quantity',			0,	N'Duration',	N'الفترة',		1,4,1),
(4,6,	N'Entries', N'UnitId',				0,	N'',			N'',			1,4,1),
(5,6,	N'Entries', N'Time1',				0,	N'From',		N'ابتداء من',	1,4,1),
(6,6,	N'Entries', N'Time2',				0,	N'Till',		N'حتى',			1,1,1),
(7,6,	N'Entries', N'CurrencyId',			0,	N'Currency',	N'العملة',		1,4,0),
(8,6,	N'Entries', N'MonetaryValue',		0,	N'Amount',		N'المطالبة',	1,4,0),
(9,6,	N'Entries',	N'CenterId',			0,N'Inv. Ctr',	N'مركز الاستثمار',4,4,1),
(10,6,	N'Entries',	N'CenterId',			1,N'Rev./Profit Ctr',	N'مركز الإيراد\الربح',4,4,0);
--LeaseOutIssueAndSalesInvoiceNoVAT.  TODO: Auto calculate Time2 and Revenue, based on AgentRates
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],				[TitleSingular],		[TitleSingular2],	[TitlePlural],				[TitlePlural2],
[Description]) VALUES (
7,0,N'LeaseOutIssueAndSalesInvoiceNoVAT',N'Lease/Subscription N.V.',	N'إيجار -اشتراك لا ق.م',	N'Leases/Subscriptions N.V.',	N'إيجارات -اشتراكات لا ق.م',
N'For lease out of properties or software subscriptions No VAT. Indicates both the rendering of service and issue of invoice.');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue1]	= [MonetaryValue0],
		[Time20]			= dbo.fn_DateAdd([UnitId0],[Quantity0],[Time10]) 
		[CurrencyId1]		= [CurrencyId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 7;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],[IsCurrent],[AgentDefinitionId]) VALUES
(0,7,+1,	N'TradeReceivables',		1,		N'customers'),
(1,7,-1,	N'Revenue',					1,		N'cost-objects');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,7,	N'Entries', N'AgentId',				0,	N'Customer',	N'الزبون',		1,4,0),
(1,7,	N'Entries', N'AgentId',				1,	N'System',		N'النظام',		1,4,0),
(2,7,	N'Entries', N'ResourceId',			0,	N'Service',		N'الخدمة',		1,4,0),
(3,7,	N'Entries', N'Quantity',			0,	N'Duration',	N'الفترة',		1,4,1),
(4,7,	N'Entries', N'UnitId',				0,	N'',			N'',			1,4,1),
(5,7,	N'Entries', N'Time1',				0,	N'From',		N'ابتداء من',	1,4,1),
(6,7,	N'Entries', N'Time2',				0,	N'Till',		N'حتى',			1,4,1),
(7,7,	N'Entries', N'CurrencyId',			0,	N'Currency',	N'العملة',		1,4,0),
(8,7,	N'Entries', N'MonetaryValue',		0,	N'Amount',		N'المطالبة',	1,4,0),
(9,7,	N'Entries',	N'CenterId',			0,	N'Inv. Ctr',	N'مركز الاستثمار',4,4,1),
(10,7,	N'Entries',	N'CenterId',			1,	N'Rev./Profit Ctr',	N'مركز الإيراد\الربح',4,4,0);
--PPEDepreciation
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Id],			[TitleSingular],		[TitleSingular2],		[TitlePlural],				[TitlePlural2],
[Description]) VALUES (
8,0,N'PPEDepreciation',				N'Asset Depreciation',	N'إهلاك أصل',			N'Assets Depreciation',		N'إهلاكات أصول',
N'For depreciation of assets that are time based, and using the number of days as criteria');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[AgentId1]				= [AgentId0],
		[MonetaryValue0]		= [MonetaryValue1]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 8;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentCode],	[IsCurrent],[AgentDefinitionId],	[EntryTypeCode]) VALUES
(0,8,+1,	N'DepreciationExpense',			1,		N'cost-objects',		NULL),
(1,8,-1,	N'PropertyPlantAndEquipment',	0,		N'cost-objects',		N'DepreciationPropertyPlantAndEquipment');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,8,	N'Entries', N'ResourceId',			1,	N'Asset',		N'الأصل',		1,4,0),
(1,8,	N'Entries', N'Quantity',			1,	N'Usage',		N'الاستخدام',	1,4,1),
(2,8,	N'Entries', N'UnitId',				1,	N'',			N'',			1,4,1),
(3,8,	N'Entries', N'AgentId',				0,	N'For',			N'لصالح',		1,4,0),
(4,8,	N'Entries', N'EntryTypeId',			0,	N'Purpose',		N'الغرض',		1,4,0),
(5,8,	N'Entries', N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
(6,8,	N'Entries', N'Time2',				1,	N'Till',		N'حتى',			1,0,1),
(7,8,	N'Entries', N'MonetaryValue',		1,	N'Depreciation',N'الإهلاك',		1,0,0),
(8,8,	N'Entries',	N'CenterId',			1,	N'Inv. Ctr',	N'مركز الاستثمار',4,4,1),
(9,8,	N'Entries',	N'CenterId',			0,	N'Cost Ctr',	N'مركز التكلفة',4,4,0);
END