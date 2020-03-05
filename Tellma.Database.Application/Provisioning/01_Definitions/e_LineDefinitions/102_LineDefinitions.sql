IF @DB = N'102'
BEGIN
	INSERT @LineDefinitions([Index],
	[Id],			[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES
	(0,N'ManualLine', N'Adjustment',		N'تسوية',		N'Adjustments',	N'تسويات');
	INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
	[SortKey],	[ColumnName],		[Label],		[Label2],		[RequiredState],
																	[ReadOnlyState]) VALUES
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
			[MonetaryValue2] = [NotedAmount0] + [MonetaryValue0]
		-----
		--SELECT * FROM @ProcessedWideLines;'
	WHERE [Index] = 1;

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
		[MonetaryValue2] = [NotedAmount0] + [MonetaryValue0]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
[Direction],[AccountTypeParentCode],	[AccountTagId]) VALUES
(0,1,0,+1,	N'TradeAndOtherPayables',	N'VATX'),
(1,1,1,+1,	N'Accruals',				N'SACR'),
(2,1,2,-1,	N'TradeAndOtherPayables',	N'TPBL');
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
[SortKey],	[ColumnName],				[Label],				[Label2],				[RequiredState],
																						[ReadOnlyState]) VALUES
(0,1,0,	N'Line.Memo',					N'Memo',				N'البيان',				1,5), 
(1,1,1,	N'Entry[0].NotedDate',			N'Invoice Date',		N'تاريخ الفاتورة',		3,5), 
(2,1,2,	N'Entry[0].ExternalReference',	N'Invoice #',			N'رقم الفاتورة',		3,5), 
(3,1,3,	N'Entry[1].AgentId',			N'Supplier',			N'المورد',				3,4),
(4,1,4,	N'Entry[0].CurrencyId',			N'Currency',			N'العملة',				1,4),
(5,1,5,	N'Entry[0].NotedAmount',		N'Price Excl. VAT',		N'المبلغ قبل الضريية',	1,4),
(6,1,6,	N'Entry[0].MonetaryValue',		N'VAT',					N'القيمة المضافة',		1,4),
(7,1,7,	N'Entry[2].MonetaryValue',		N'Total',				N'المبلغ بعد الضريبة',	1,1),
(8,1,8,	N'Entry[2].DueDate',			N'Due Date',			N'تاريخ الاستحقاق',		1,4),
(9,1,9,	N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',N'مركز المسؤولية',0,4);

	-- NB: requisitions could be for payment towards something approved. Or it could be for a new purchase
	-- when it is for a new purchase, the document must have two tabs: payment details, and purchase details

	INSERT @LineDefinitions([Index],
	[Id],				[TitleSingular], [TitleSingular2],	[TitlePlural], [TitlePlural2]) VALUES (
	2,N'CashPayment',	N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');

	UPDATE @LineDefinitions
	SET [Script] = N'
		SET NOCOUNT ON
		DECLARE @ProcessedWideLines WideLineList;

		INSERT INTO @ProcessedWideLines
		SELECT * FROM @WideLines;
		-----
		UPDATE @ProcessedWideLines
		SET
			[CurrencyId0] = dbo.fn_FunctionalCurrencyId()
		-----
		SELECT * FROM @ProcessedWideLines;'
	WHERE [Index] = 2;

	INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
	[Direction],	[AccountTypeParentCode]) VALUES
	(0,2,0,	-1,		N'CashAndCashEquivalents');

	INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
	[SortKey],	[ColumnName],						[Label],					[Label2],					[RequiredState],
																											[ReadOnlyState]) VALUES
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

	-- Petty cash payment
	-- Stock issue
	-- Stock receipt
	-- LC open
	-- Shipping Docs Receipt

	-- NB: We defined a Pettycash payment to separate the business rules
	INSERT @LineDefinitions([Index],
	[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
	3,N'PettyCashPayment',	N'Petty Cash Payment',	N'دفعية نثرية',		N'Petty Cash Payments',	N'دفعيات النثرية');
	--UPDATE @LineDefinitions
	--SET [Script] = N'
	--	SET NOCOUNT ON
	--	DECLARE @ProcessedWideLines WideLineList;

	--	INSERT INTO @ProcessedWideLines
	--	SELECT * FROM @WideLines;
	--	-----
	--	UPDATE @ProcessedWideLines
	--	SET
	--		[CurrencyId0] = dbo.fn_FunctionalCurrencyId()
	--	-----
	--	SELECT * FROM @ProcessedWideLines;'
	--WHERE [Index] = 3;
	INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
	[Direction],	[AccountTypeParentCode],	[AccountTagId]) VALUES
	(0,3,0,-1,		N'CashAndCashEquivalents',	N'CASH');
	INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
	[SortKey],	[ColumnName],						[Label],					[Label2],			[RequiredState],
																									[ReadOnlyState]) VALUES
	(0,3,0,		N'Entry[0].NotedDate',				N'Date',					N'التاريخ',			1,4), 
	(1,3,1,		N'Line.Memo',						N'Memo',					N'البيان',			1,4),
	(2,3,2,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',			1,2), 
	(3,3,3,		N'Entry[0].MonetaryValue',			N'Pay Amount',				N'المبلغ',			1,2), 
	(4,3,4,		N'Entry[0].NotedAgentName',			N'Beneficiary',				N'المستفيد',		1,2),
	(5,3,5,		N'Entry[0].EntryTypeId',			N'Purpose',					N'الغرض',			4,4),
	(6,3,6,		N'Entry[0].AgentId',				N'Petty Cash Custodian',	N'أمين العهدة',		3,4),
	(7,3,7,		N'Entry[0].ExternalReference',		N'Receipt #',				N'رقم الإيصال',		3,4),
	(8,3,8,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',	4,4);

	-- Withholding Tax Payable
	INSERT @LineDefinitions([Index],
	[Id],						[TitleSingular],			[TitleSingular2],		[TitlePlural],					[TitlePlural2],			[AgentDefinitionList], [ResponsibilityTypeList]) VALUES (
	4,N'WithholdingTaxPayable',	N'Withholding Tax Payable',	N'ضريبة خصم مشتريات',	N'Withholding Taxes Payable',	N'ضرائب خصم مشتريات',	N'suppliers',			N'Investment');
	UPDATE @LineDefinitions
	SET [Script] = N'
		SET NOCOUNT ON
		DECLARE @ProcessedWideLines WideLineList;

		INSERT INTO @ProcessedWideLines
		SELECT * FROM @WideLines;
		-----
		UPDATE @ProcessedWideLines
		SET
			[NotedAgentId0]				= [AgentId1],
			[MonetaryValue0]			= 0.02 * [NotedAmount0],
			[ResponsibilityCenterId1]	= [ResponsibilityCenterId0],
			[ExternalReference1]		= [ExternalReference0],
			[CurrencyId1]				= [CurrencyId0]
		-----
		SELECT * FROM @ProcessedWideLines;'
	WHERE [Index] = 4;
	INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
	[Direction],	[AccountTypeParentCode],	[AccountTagId]) VALUES
	(0,4,0,+1,		N'TradeAndOtherPayables',	N'WHTX'),
	(1,4,1,-1,		N'TradeAndOtherPayables',	N'TPBL');
	INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
	[SortKey],	[ColumnName],						[Label],					[Label2],				[RequiredState],
																										[ReadOnlyState]) VALUES
	(0,4,0,		N'Entry[0].NotedDate',				N'Date',					N'التاريخ',				1,4), 
	(1,4,1,		N'Line.Memo',						N'Memo',					N'البيان',				1,4),
	(2,4,2,		N'Entry[0].CurrencyId',				N'Currency',				N'العملة',				1,2), 
	(3,4,3,		N'Entry[1].AgentId',				N'Supplier',				N'المورد',				1,2), 
	(4,4,4,		N'Entry[0].NotedAmount',			N'TaxableAmount',			N'المبلغ الخاضع للخصم',1,2),
	(5,4,5,		N'Entry[0].MonetaryValue',			N'Withtholding Tax',		N'الخصم الضريبي',		0,4),
	(6,4,6,		N'Entry[0].ExternalReference',		N'Voucher #',				N'رقم الإيصال',			3,4),
	(7,4,7,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',	N'مركز المسؤولية',		4,4);

	-- Goods Receipts Note
	INSERT @LineDefinitions([Index],
	[Id],					[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2],		[AgentDefinitionList], [ResponsibilityTypeList]) VALUES (
	5,N'GoodsReceiptNote',	N'Goods Receipt Note',	N'استلام مخزن',		N'Goods Receipt Notes',	N'استلامات مخازن',	N'suppliers',			N'Investment');
	UPDATE @LineDefinitions
	SET [Script] = N'
		SET NOCOUNT ON
		DECLARE @ProcessedWideLines WideLineList;

		INSERT INTO @ProcessedWideLines
		SELECT * FROM @WideLines;
		-----
		UPDATE @ProcessedWideLines
		SET
			[NotedAgentId0]				= [AgentId1],
			[MonetaryValue1]			= [MonetaryValue0],
			[ResponsibilityCenterId1]	= [ResponsibilityCenterId0],
			[CurrencyId1]				= [CurrencyId0]
		-----
		SELECT * FROM @ProcessedWideLines;'
	WHERE [Index] = 5;
	INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
	[Direction],	[AccountTypeParentCode],	[AccountTagId]) VALUES
	(0,5,0,+1,		N'TotalInventories',		N'STCK'),
	(1,5,1,-1,		N'TradeAndOtherPayables',	N'SACR');
	INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
	[SortKey],	[ColumnName],						[Label],				[Label2],				[RequiredState],
																									[ReadOnlyState]) VALUES
	(0,5,0,		N'Line.Memo',						N'Memo',				N'البيان',				1,5), 
	(1,5,1,		N'Entry[0].AgentId',				N'Supplier',			N'المورد',				3,4),
	(2,5,2,		N'Entry[0].NotedAgentId',			N'Beneficiary',			N'المستفيد',			3,4),
	(3,5,3,		N'Entry[0].ResourceId',				N'Item',				N'الصنف',				1,4),
	(4,5,4,		N'Entry[0].Quantity',				N'Quantity',			N'الكمية',				1,4),
	(5,5,5,		N'Entry[0].UnitId',					N'Unit',				N'الوحدة',				1,4),
	(6,5,6,		N'Entry[0].CurrencyId',				N'Currency',			N'العملة',				4,4),
	(7,5,7,		N'Entry[0].MonetaryValue',			N'Price Excl. VAT',		N'المبلغ قبل الضريية',	4,4),
	(8,5,8,		N'Entry[0].ResponsibilityCenterId',	N'Responsibility Center',N'مركز المسؤولية',	0,4);

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
			[NotedAgentId0]				= [AgentId1],
			[MonetaryValue1]			= [MonetaryValue0],
			[ResponsibilityCenterId1]	= [ResponsibilityCenterId0],
			[CurrencyId1]				= [CurrencyId0]
		-----
		SELECT * FROM @ProcessedWideLines;'
	WHERE [Index] = 6;
	INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Index],
	[Direction],	[AccountTypeParentCode],	[AccountTagId]) VALUES
	(0,6,0,-1,		N'TradeAndOtherPayables',	N'SACR'); -- we need functionality to fill one tab based on info in the other tab
END