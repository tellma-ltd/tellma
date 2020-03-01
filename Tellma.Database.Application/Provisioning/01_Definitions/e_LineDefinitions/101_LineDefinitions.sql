-- for account types parent code, we use the IAS 1 level. 
-- together with that, we use:
-- AccountTagId, Currency, Responsibility Center, IsCurrent, Resource.AccountType, AgentDefinitionList to figure out the account
-- 
IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--ManualLine 
INSERT @LineDefinitions([Index],
[Id],			[TitleSingular], [TitlePlural]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryNumber],	[Label],		[RequiredState],
																[ReadOnlyState]) VALUES
(0,0,	N'Lines',	N'Line.Memo',	0,			N'Memo',		5,4), -- only if it appears,
(1,0,	N'Entries',	N'Account',		0,			N'Account',		3,4),
(2,0,	N'Entries',	N'Value',		0,			N'Debit',		3,4), -- see special case
(3,0,	N'Entries',	N'Value',		0,			N'Credit',		3,4),
(4,0,	N'Entries',	N'Dynamic',		0,			N'Properties',	3,4);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');
--PurchaseInvoice
INSERT @LineDefinitions([Index],
[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES
(1,N'PurchaseInvoice',	N'Purchase Invoice',	N'فاتورة مشتريات',	N'Purchase Invoices',	N'فواتير مشتريات');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
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
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],[AccountTypeParentCode],	[AccountTagId], [AgentDefinitionId]) VALUES
(0,1,0,+1,	N'TradeAndOtherPayables',	N'VATX'	,		NULL),
(1,1,1,+1,	N'Accruals',				N'SACR',		N'suppliers'),
(2,1,2,-1,	N'TradeAndOtherPayables',	N'TPBL',		NULL);-- <== AgentDefinitionId is irrelevant here
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryNumber],	[Label],				[Label2],				[RequiredState],
																								[ReadOnlyState]) VALUES
(0,1,	N'Lines',	N'Memo',				0,	N'Memo',				N'البيان',				1,5), 
(1,1,	N'Entries',	N'NotedDate',			0,	N'Invoice Date',		N'تاريخ الفاتورة',		3,5), 
(2,1,	N'Entries',	N'ExternalReference',	0,	N'Invoice #',			N'رقم الفاتورة',		3,5), 
(3,1,	N'Entries',	N'AgentId',				1,	N'Supplier',			N'المورد',				3,4),
(4,1,	N'Entries',	N'CurrencyId',			0,	N'Currency',			N'العملة',				1,4),
(5,1,	N'Entries',	N'NotedAmount',			0,	N'Price Excl. VAT',		N'المبلغ قبل الضريية',	1,4),
(6,1,	N'Entries',	N'MonetaryValue',		0,	N'VAT',					N'القيمة المضافة',		1,4),
(7,1,	N'Entries',	N'MonetaryValue',		2,	N'Total',				N'المبلغ بعد الضريبة',	1,1),
(8,1,	N'Entries',	N'DueDate',				2,	N'Due Date',			N'تاريخ الاستحقاق',		1,4),
(9,1,	N'Entries',	N'ResponsibilityCenterId',0,N'Responsibility Center',N'مركز المسؤولية',	0,4);
--CashPayment
INSERT @LineDefinitions([Index],
[Id],				[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
2,N'CashPayment',	N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AccountTagId], [AgentDefinitionId]) VALUES
(0,2,0,	-1,		N'CashAndCashEquivalents',	NULL,			N'cash-custodians');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryNumber],	[Label],					[Label2],				[RequiredState],
																									[ReadOnlyState]) VALUES
(0,2,	N'Lines',	N'Memo',				0,	N'Memo',					N'البيان',				1,2),
(1,2,	N'Entries',	N'CurrencyId',			0,	N'Currency',				N'العملة',				1,2),
(2,2,	N'Entries',	N'MonetaryValue',		0,	N'Pay Amount',				N'المبلغ',				1,2),
(3,2,	N'Entries',	N'Value',				0,	N'Equiv Amt ($)',			N'($) المعادل',			4,4), 
(4,2,	N'Entries',	N'NotedAgentName',		0,	N'Beneficiary',				N'المستفيد',			3,4),
(5,2,	N'Entries',	N'EntriesTypeId',		0,	N'Purpose',					N'الغرض',				1,4),
(6,2,	N'Entries',	N'AgentId',				0,	N'Bank/Cashier',			N'البنك/الخزنة',		3,4),
(7,2,	N'Entries',	N'AccountIdentifier',	0,	N'Account Identifier',		N'تمييز الحساب',		3,4),
(8,2,	N'Entries',	N'ExternalReference',	0,	N'Check #/Receipt #',		N'رقم الشيك/الإيصال',	3,4),
(9,2,	N'Entries',	N'NotedDate',			0,	N'Check Date',				N'تاريخ الشيك',			3,4),
(10,2,	N'Entries',	N'ResponsibilityCenterId',0,N'Responsibility Center',	N'مركز المسؤولية',		1,4);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[StateId], [Name],					[Name2]) VALUES
(0,2,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,2,-3,	N'Other reasons',		N'أسباب أخرى');
--PettyCashPayment
INSERT @LineDefinitions([Index],
[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
3,N'PettyCashPayment',	N'Petty Cash Payment',	N'دفعية نثرية',		N'Petty Cash Payments',	N'دفعيات النثرية');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],[AccountTypeParentCode],	[AccountTagId]) VALUES
(0,3,0,-1,	N'CashAndCashEquivalents',	N'CASH');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryNumber],	[Label],					[Label2],			[RequiredState],
																								[ReadOnlyState]) VALUES
(0,3,	N'Entries', N'NotedDate',			0,	N'Date',					N'التاريخ',			1,4), 
(1,3,	N'Lines',	N'Memo',				0,	N'Memo',					N'البيان',			1,4),
(2,3,	N'Entries', N'CurrencyId',			0,	N'Currency',				N'العملة',			1,2), 
(3,3,	N'Entries', N'MonetaryValue',		0,	N'Pay Amount',				N'المبلغ',			1,2), 
(4,3,	N'Entries', N'Value',				0,	N'Equiv Amt ($)',			N'($) المعادل',		4,4), 
(5,3,	N'Entries', N'NotedAgentName',		0,	N'Beneficiary',				N'المستفيد',		1,2),
(6,3,	N'Entries', N'EntryTypeId',			0,	N'Purpose',					N'الغرض',			4,4),
(7,3,	N'Entries', N'AgentId',				0,	N'Petty Cash Custodian',	N'أمين العهدة',		3,4),
(8,3,	N'Entries', N'AccountIdentifier',	0,	N'Account Identifier',		N'تمييزالعهدة',	3,4),
(9,3,	N'Entries', N'ExternalReference',	0,	N'Receipt #',				N'رقم الإيصال',		3,4),
(10,3,	N'Entries', N'ResponsibilityCenterId',0,N'Responsibility Center',	N'مركز المسؤولية',	4,4);  
--GoodsReceiptIssue
INSERT @LineDefinitions([Index],
[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
6,N'GoodsReceiptIssue',	N'Goods Receipt/Issue',	N'استلام مستخدم',	N'Goods Receipt/Issue',	N'استلامات مستخدمين');
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
WHERE [Index] = 6;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AgentDefinitionId], [AccountTagId]) VALUES
(0,6,0,-1,		N'TradeAndOtherPayables',	N'suppliers',		N'SACR'); -- We may need to add GRIV Inventory underneath, or instead
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[TableName],[ColumnName],[EntryNumber],	[Label],				[Label2],				[RequiredState],
																								[ReadOnlyState]) VALUES
(0,6,	N'Lines',	N'Memo',				0,	N'Memo',				N'البيان',				1,5), 
(1,6,	N'Entries', N'AgentId',				1,	N'Supplier',			N'المورد',				3,4),
(2,6,	N'Entries', N'AgentId',				0,	N'Beneficiary',			N'المستفيد',			3,4),
(3,6,	N'Entries', N'ResourceId',			0,	N'Item',				N'الصنف',				1,4),
(4,6,	N'Entries', N'Quantity',			0,	N'Quantity',			N'الكمية',				1,4),
(5,6,	N'Entries', N'UnitId',				0,	N'Unit',				N'الوحدة',				1,4),
(6,6,	N'Entries', N'CurrencyId',			0,	N'Currency',			N'العملة',				4,4),
(7,6,	N'Entries', N'MonetaryAmount',		0,	N'Price Excl. VAT',		N'المبلغ قبل الضريية',	4,4),
(8,6,	N'Entries', N'ResponsibilityCenterId',0,N'Responsibility Center',N'مركز المسؤولية',	0,4);
--DomesticSubscriptions
INSERT @LineDefinitions([Index],
[Id],						[TitleSingular],			[TitleSingular2],	[TitlePlural],				[TitlePlural2],		[AgentDefinitionList], [ResponsibilityTypeList]) VALUES (
7,N'DomesticSubscriptions',	N'Domestic Subscription',	N'اشتراك محلي',		N'Domestic Subscriptions',	N'اشتراكات محلية',	NULL,					N'Investment');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[EntryNumber],
[Direction],	[AccountTypeParentCode],	[AccountTagId]) VALUES
(0,7,0,+1,		N'TradeAndOtherReceivables',N'TPBL'), 
(1,7,1,-1,		N'Revenue',					NULL);
END