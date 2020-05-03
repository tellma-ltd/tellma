IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--0:ManualLine 
INSERT @LineDefinitions([Index],
[Code],			[TitleSingular], [TitlePlural], [TitleSingular2], [TitlePlural3]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments',N'تسوية',			N'تسويات');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,0,+1,	@StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,0,	N'Account',		0,			N'Account',		4,4,0), -- together with properties
(1,0,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,0,	N'Value',		0,			N'Credit',		4,4,0),
(3,0,	N'Memo',		0,			N'Memo',		5,4,1); -- only if it appears
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');
--1:CashPayment
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
(1,1,N'CashPayment',		N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId1],
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId0]),
		[MonetaryValue1] = ISNULL([MonetaryValue0],0) + ISNULL([NotedAmount0],0),
		[CenterId0] = [CenterId1]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],				[EntryTypeId]) VALUES
(0,1,+1,	@CurrentValueAddedTaxReceivables,	NULL),
(1,1,-1,	@CashAndCashEquivalents,			@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																					[ReadOnlyState],
																					[InheritsFromHeader]) VALUES
(0,1,	N'Memo',				0,	N'Memo',				N'البيان',				1,5,1),
(1,1,	N'NotedDate',			0,	N'Invoice Date',		N'تاريخ الفاتورة',		4,5,0), 
(2,1,	N'ExternalReference',	0,	N'Invoice #',			N'رقم الفاتورة',		4,5,0), 
(3,1,	N'NotedContractId',		0,	N'Supplier',			N'المورد',				3,4,1),
(4,1,	N'CurrencyId',			1,	N'Currency',			N'العملة',				1,2,1),
(5,1,	N'NotedAmount',			0,	N'Price Excl. VAT',		N'المبلغ بدون ق.م.',	1,2,0),
(6,1,	N'MonetaryValue',		0,	N'VAT',					N'ق.م.',				3,4,0),
(7,1,	N'MonetaryValue',		1,	N'Total',				N'الإجمالي',				3,0,0),
(8,1,	N'ContractId',			1,	N'Bank/Cashier',		N'البنك\الخزنة',		3,4,0),
(9,1,	N'ExternalReference',	1,	N'Check/Receipt #',		N'رقم الشيك\الإيصال',	3,4,0),
(10,1,	N'NotedDate',			1,	N'Check Date',			N'تاريخ الشيك',			5,5,0),
(11,1,	N'CenterId',			1,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,1,+1),
(1,1,+2),
(2,1,+3),
(3,1,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,1,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,1,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,1,N'ByAgent',	NULL,				1,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,1,N'ByRole',	@1Comptroller,		NULL,			NULL);
--2:AdvancedPaymentToLessor (against invoice)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],[TitleSingular2],	[TitlePlural],				[TitlePlural2]) VALUES
(2,1,N'AdvancedPaymentToLessor',N'Adv. Rent/Sub.',	N'إيجار\اشتراك مقدم',	N'Adv. Rents/Subs',	N'إيجارات\اشتراكات مقدمة');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]	= [CurrencyId2],
		[CurrencyId1]	= [CurrencyId2],

		[ContractId0]	= [NotedContractId1],
		[NotedAgentName2] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[MonetaryValue0]= ISNULL([NotedAmount1],0),
		[MonetaryValue2]= ISNULL([MonetaryValue1],0) + ISNULL([NotedAmount1],0),
		
		[CenterId0]		= [CenterId2],
		[CenterId1]		= [CenterId2]

	--UPDATE PWL -- Show the due balance for each Contract
	--	SET PWL.[NotedAmount0] = T.Balance
	--FROM @ProcessedWideLines PWL
	--JOIN (
	--	SELECT E.ContractId, E.CurrencyId, SUM(E.[Direction] * E.[MonetaryValue]) AS [Balance]
	--	FROM dbo.Entries E
	--	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	--	JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
	--	JOIN dbo.Accounts A ON E.[AccountId] = A.[Id]
	--	JOIN dbo.AccountTypes AC ON A.[AccountTypeId] = AC.[Id]
	--	WHERE AC.Code IN (N''TradeAndOtherPayablesToTradeSuppliers'') -- may be more
	--	AND D.[PostingDate] <= PWL.[NotedDate1]
	--) T ON T.ContractId = PWL.ContractId2 AND T.CurrencyId = PWL.CurrencyId2
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 2;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],			[EntryTypeId]) VALUES
(0,2,+1,	@AccrualsClassifiedAsCurrent,	NULL),
(1,2,+1,	@CurrentValueAddedTaxReceivables,NULL),
(2,2,-1,	@CashAndCashEquivalents,		@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],				[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,2,	N'Memo',				1,	N'Memo',			N'البيان',				1,5,1),
(1,2,	N'NotedDate',			1,	N'Invoice Date',	N'تاريخ الفاتورة',		3,5,0), 
(2,2,	N'ExternalReference',	1,	N'Invoice #',		N'رقم الفاتورة',		3,5,0), 
(3,2,	N'NotedContractId',		1,	N'Supplier',		N'المورد',				3,4,1),
(4,2,	N'CurrencyId',			2,	N'Currency',		N'العملة',				1,2,1),
(5,2,	N'NotedAmount',			1,	N'Price Excl. VAT',	N'المبلغ بدون ق.م.',	1,2,0),
(6,2,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',				3,4,0),
(7,2,	N'MonetaryValue',		2,	N'Total',			N'الإجمالي',				3,0,0),
(8,2,	N'ContractId',			2,	N'Bank/Cashier',	N'البنك\الخزنة',		3,4,0),
(9,2,	N'ExternalReference',	2,	N'Check/Receipt #',	N'رقم الشيك\الإيصال',	3,4,0),
(10,2,	N'NotedDate',			2,	N'Check Date',		N'تاريخ الشيك',			5,5,0),
(11,2,	N'CenterId',			2,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,2,+1),
(1,2,+2),
(2,2,+3),
(3,2,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,2,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,2,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,2,N'ByAgent',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,2,N'ByRole',	@1Comptroller,		NULL,			NULL);
--3:GoodReceiptNote, (from Cash Purchase), (from Supplier on Account in SRV)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],		[TitleSingular2],	[TitlePlural],				[TitlePlural2]) VALUES
(3,0,N'GoodReceiptNote',	N'Inventory Purchase',	N'شراء صنف',		N'Inventories Purchases',	N'مشتريات أصناف');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],	[EntryTypeId]) VALUES
(0,3,+1,	@Inventories,			@InventoryPurchaseExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,3,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1),
(1,3,	N'ResourceId',			0,	N'Item',			N'الصنف',			3,4,0),
(2,3,	N'Quantity',			0,	N'Quantity',		N'الكمية',			1,2,0),
(3,3,	N'UnitId',				0,	N'Unit',			N'الوحدة',			1,2,0),
(4,3,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	N'السعر (بلا ق.م.)',1,2,0),
(5,3,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(6,3,	N'ContractId',			0,	N'Warehouse',		N'المخزن',			3,3,1),
(7,3,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(8,3,	N'NotedContractId',		0,	N'Supplier',		N'المورد',			3,3,1);
--4:GRIV Dr. (from Cash Purchase), (from Supplier on Account in GRIV)
--5:Consumables&Services (from Cash Purchase), (from Supplier on Account in C&S RV)
--6:PaymentToEmployee (in Banan SD, we will have a dedicated voucher for that)
--7:PaymentToPartner
--8:RefundToCustomer.
--9:PaymentToOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES (
9,1,N'PaymentToOther',		N'Other Payment',	N'دفعية أخرى',		N'Other Payments',	N'دفعيات أخرى');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,9,	-1,	@CashAndCashEquivalents);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																					[ReadOnlyState],
																					[InheritsFromHeader]) VALUES
(0,9,	N'Memo',				0,	N'Memo',				N'البيان',				1,2,1),
(1,9,	N'CurrencyId',			0,	N'Currency',			N'العملة',				1,2,1),
(2,9,	N'MonetaryValue',		0,	N'Pay Amount',			N'المبلغ',				1,2,0),
(3,9,	N'NotedAgentName',		0,	N'Beneficiary',			N'المستفيد',			3,3,0),
(4,9,	N'ContractId',			0,	N'Bank/Cashier',		N'البنك/الخزنة',		3,3,0),
(5,9,	N'ExternalReference',	0,	N'Check #/Receipt #',	N'رقم الشيك/الإيصال',	3,3,0),
(6,9,	N'NotedDate',			0,	N'Check Date',			N'تاريخ الشيك',			5,3,0),
(7,9,	N'CenterId',			0,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1),
(8,9,	N'EntryTypeId',			0,	N'Purpose',				N'الغرض',				4,4,0);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,9,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,9,-3,	N'Other reasons',		N'أسباب أخرى');
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,9,+1),
(1,9,+2),
(2,9,+3),
(3,9,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,9,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,9,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,9,N'ByAgent',	NULL,				0,				NULL), -- cash/check custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,9,N'ByRole',	@1Comptroller,		NULL,			NULL);
--10:CashTransferExchange
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
10,1,N'CashTransferExchange',	N'Transfer/Exchange',	N'تحويل\صرف',		N'Transfers/Exchanges',	N'تحويلات\صرف');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId1] = [CenterId0],
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0])
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 10;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],	 [EntryTypeId]) VALUES
(0,10,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,10,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],			[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,10,	N'ContractId',			1,	N'From Account',	N'من حساب',			1,2,0),
(1,10,	N'ContractId',			0,	N'To Account',		N'إلى حساب',		1,2,0),
(2,10,	N'CurrencyId',			1,	N'From Currency',	N'من عملة',			1,2,0),
(3,10,	N'CurrencyId',			0,	N'To Currency',		N'إلى عملة',		1,2,0),
(4,10,	N'MonetaryValue',		1,	N'From Amount',		N'من مبلغ',			1,3,0),
(5,10,	N'MonetaryValue',		0,	N'To Amount',		N'إلى مبلغ',		1,3,0),
(6,10,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(7,10,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,10,+1),
(1,10,+2),
(2,10,+3),
(3,10,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],			[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,10,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,10,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,10,N'ByAgent',	NULL,				0,				@1Comptroller), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(1,2,10,N'ByAgent',	NULL,				1,				@1Comptroller), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,10,N'ByRole',	@1Comptroller,		NULL,			NULL);
--11:CashSale, No workflow
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES (
11,1,N'CashReceipt',		N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');
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
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[MonetaryValue0] = ISNULL([MonetaryValue1],0) + ISNULL([NotedAmount1],0)
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 11;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],		[EntryTypeId]) VALUES
(0,11,+1,	@CashAndCashEquivalents,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,11,-1,	@CurrentValueAddedTaxPayables,NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																					[ReadOnlyState],
																					[InheritsFromHeader]) VALUES
(0,11,	N'CenterId',			0,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1),
(1,11,	N'Memo',				1,	N'Memo',				N'البيان',				1,5,1),
(2,11,	N'NotedAgentId',		1,	N'Customer',			N'الزبون',				1,4,1),
(3,11,	N'CurrencyId',			0,	N'Currency',			N'العملة',				1,2,1),
(4,11,	N'NotedAmount',			1,	N'Price Excl. VAT',		N'المبلغ بدون ق.م.',	1,2,0),
(5,11,	N'MonetaryValue',		1,	N'VAT',					N'ق.م.',				3,4,0),
(6,11,	N'MonetaryValue',		0,	N'Total',				N'الإجمالي',				3,0,0),
(7,11,	N'NotedDate',			0,	N'Payment Date',		N'تاريخ الدفعية',		3,4,0),
(8,11,	N'ContractId',			0,	N'Bank/Cashier',		N'البنك\الخزنة',		3,4,0),

(9,11,	N'NotedDate',			1,	N'Invoice Date',		N'تاريخ الفاتورة',		3,4,0), 
(10,11,	N'ExternalReference',	1,	N'Invoice #',			N'رقم الفاتورة',		3,4,0)
--12:AdvancedReceiptFromCustomer (against an invoice), No workflow
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],		[TitleSingular],	[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES (
12,1,N'AdvancedReceiptFromCustomer',N'Prepayment',		N'دفع مقدم',		N'Prepayments',	N'دفعيات مقدمة');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[ContractId2]		= [ContractId1],
		[NotedAmount1]		= ISNULL([MonetaryValue2],0)
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 12;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],				[EntryTypeId]) VALUES
(0,12,+1,	@CashAndCashEquivalents,			@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,12,-1,	@CurrentValueAddedTaxPayables,		NULL),
(2,12,-1,	@DeferredIncomeClassifiedAsCurrent,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],				[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,12,	N'Memo',				1,	N'Memo',			N'البيان',				1,5,1),
(1,12,	N'NotedContractId',		1,	N'Customer',		N'الزبون',				1,4,1),
(2,12,	N'CurrencyId',			2,	N'Contract Currency',N'عملة العقد',			1,2,1),
(3,12,	N'MonetaryValue',		2,	N'Price Excl. VAT',	N'المطالبة بدون ق.م.',	1,2,0),
(4,12,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',				1,2,0),
(5,12,	N'NotedAmount',			0,	N'Total',			N'الإجمالي',				2,0,0),
(6,12,	N'NotedDate',			2,	N'Due Date',		N'تاريخ الاستحقاق',		3,4,0),
(7,12,	N'NotedDate',			1,	N'Payment Date',	N'تاريخ السداد',		3,5,0),
(8,12,	N'ContractId',			0,	N'Bank/Cashier',	N'البنك\الخزنة',		3,4,1),
(9,12,	N'CurrencyId',			0,	N'Rcvd. Currency',	N'عملة الاستلام',			3,4,0),
(10,12,	N'MonetaryValue',		0,	N'Rcvd. Amount',	N'الإجمالي المستلم',	3,4,0),
(11,12,	N'ExternalReference',	1,	N'Invoice #',		N'رقم الفاتورة',		3,5,0),
(12,12,	N'CenterId',			0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);

--13:DeferredReceiptFromCustomer (against an invoice), No workflow
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],		[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES (
13,1,N'DeferredReceiptFromCustomer',N'Postpayment',	N'دفع مؤخر',		N'Postpayments',N'دفعيات مؤخرة');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[ContractId2]		= [NotedContractId1],
		[NotedAmount1]		= ISNULL([MonetaryValue2],0)
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 13;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],			[EntryTypeId]) VALUES
(0,13,+1,	@CashAndCashEquivalents,		@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,13,-1,	@CurrentValueAddedTaxPayables,	NULL),
(2,13,-1,	@CurrentAccruedIncome,			NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],				[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,13,	N'Memo',				1,	N'Memo',			N'البيان',				1,5,1),
(1,13,	N'NotedContractId',		1,	N'Customer',		N'الزبون',				1,4,1),
(2,13,	N'CurrencyId',			2,	N'Contract Currency',N'عملة العقد',			1,2,1),
(3,13,	N'MonetaryValue',		2,	N'Price Excl. VAT',	N'المطالبة بدون ق.م.',	1,2,0),
(4,13,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',				1,2,0),
(5,13,	N'NotedAmount',			0,	N'Total',			N'الإجمالي',				2,0,0),
(6,13,	N'NotedDate',			2,	N'Due Date',		N'تاريخ الاستحقاق',		3,4,0),
(7,13,	N'NotedDate',			1,	N'Payment Date',	N'تاريخ السداد',		3,5,0), 
(8,13,	N'ContractId',			0,	N'Bank/Cashier',	N'البنك\الخزنة',		3,4,1),
(9,13,	N'CurrencyId',			0,	N'Rcvd. Currency',	N'عملة الاستلام',			3,4,0),
(10,13,	N'MonetaryValue',		0,	N'Rcvd. Amount',	N'الإجمالي المستلم',	3,4,0),
(11,13,	N'ExternalReference',	1,	N'Invoice #',		N'رقم الفاتورة',		3,5,0),
(12,13,	N'CenterId',			0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
--14:GoodDeliveryNote
--15:ServiceDeliveryNote (from Cash Sale). For sale to customer on account in SDN)

--17:ReceiptFromPartner
--18:RefundFromSupplier.
--19:ReceiptFromOther

--19:ReceiptFromOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
19,1,N'ReceiptFromOther',	N'Other Cash Receipt',	N'توريد آخر',	N'Other Cash Receipts',	N'توريدات أخرى');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,19,+1,	@CashAndCashEquivalents);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,19,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(1,19,	N'MonetaryValue',		0,	N'Amount',			N'المبلغ',			1,2,0),
(2,19,	N'NotedAgentName',		0,	N'Received from',	N'مستلم من',		3,4,0),
(3,19,	N'ContractId',			0,	N'Bank/Cashier',	N'البنك/الخزنة',	3,4,1),
(4,19,	N'ExternalReference',	0,	N'Check/Receipt #',	N'رقم الشيك/الإيصال',5,4,0),
(5,19,	N'NotedDate',			0,	N'Check Date',		N'تاريخ الشيك',		5,4,0),
(6,19,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(7,19,	N'EntryTypeId',			0,	N'Purpose',			N'الغرض',			4,4,0),
(8,19,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1);
--15:ServiceDeliveryNote
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2],
[Description]) VALUES (
15,0,N'ServiceDeliveryNote',	N'Service',		N'الخدمة',			N'Services',	N'الخدمات',
N'To recgonize revenues from delivering services');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,15,-1,	@Revenue);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,15,	N'NotedContractId',	0,	N'Customer',		N'الزبون',				1,4,1),
(1,15,	N'CenterId',		0,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,15,	N'ResourceId',		0,	N'Service',			N'الخدمة',				1,4,0),
(3,15,	N'Quantity',		0,	N'Quantity',		N'الكمية',				1,4,0),
(4,15,	N'UnitId',			0,	N'',				N'',					1,4,0),
(7,15,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,1),
(8,15,	N'MonetaryValue',	0,	N'Due Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0);
--21:GRN on account
--22:GRIN on account
--23:C&S RN on account
--24:PrepaidLeaseIn
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitleSingular2],	[TitlePlural],				[TitlePlural2],
[Description]) VALUES (
24,0,N'PrepaidLeaseIn',	N'Lease In/Subscription',	N'إيجار - اشتراك',	N'Leases In/Subscriptions',	N'إيجارات -اشتراكات',
N'For lease in of properties or software subscriptions with VAT. Indicates the rendering of service');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0]	= [MonetaryValue1],
		[Time20]			= dbo.fn_DateAdd([UnitId0],[Quantity0],[Time10]),
		[CurrencyId0]		= [CurrencyId1]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 24;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,24,+1,	@ServicesExpense),
(1,24,-1,	@AccrualsClassifiedAsCurrent);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,24,	N'ContractId',			1,	N'Lessor',		N'المؤجر',		1,4,0),
(1,24,	N'CenterId',			0,	N'Cost Center',	N'مركز التكلفة',4,4,0),
(2,24,	N'ResourceId',			1,	N'Service',		N'الخدمة',		1,4,0),
(3,24,	N'Quantity',			1,	N'Duration',	N'الفترة',		1,4,1),
(4,24,	N'UnitId',				1,	N'',			N'',			1,4,1),
(5,24,	N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
(6,24,	N'Time2',				1,	N'Till',		N'حتى',			1,1,1),
(7,24,	N'CurrencyId',			1,	N'Currency',	N'العملة',		1,4,0),
(8,24,	N'MonetaryValue',		1,	N'Amount',		N'المطالبة',	1,4,0),
(9,24,	N'CenterId',			1,	N'Inv. Ctr',	N'مركز الاستثمار',4,4,1);

--31:GDN on account
--32:PrepaidServiceDelivery
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],	[TitleSingular2],		[TitlePlural],			[TitlePlural2],
[Description]) VALUES (
32,1,N'PrepaidServiceDelivery',	N'Prepaid Service',	N'خدمة مدفوعة مقدما',	N'Prepaid Services',	N'خدمات مدفوعة مقدما',
N'To recgonize revenues from delivering services, for which the customer has already paid');
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
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 32;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,32,+1,	@DeferredIncomeClassifiedAsCurrent), -- @CurrentAccruedIncome
(1,32,-1,	@Revenue);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,32,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,1),
(1,32,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,32,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
(3,32,	N'Quantity',		1,	N'Quantity',		N'الكمية',				1,4,0),
(4,32,	N'UnitId',			1,	N'',				N'',					1,4,0),
(7,32,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,1),
(8,32,	N'MonetaryValue',	0,	N'Price Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
(9,32,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
--33:PostpaidServiceDelivery
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],	[TitleSingular2],		[TitlePlural],			[TitlePlural2],
[Description]) VALUES (
33,1,N'PostpaidServiceDelivery',N'Postpaid Service',N'خدمة مدفوعة لاحقا',	N'Postpaid Services',	N'خدمات مدفوعة لاحقا',
N'To recgonize revenues from delivering services, for which the customer will pay later');
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
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 33;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,33,+1,	@CurrentAccruedIncome),
(1,33,-1,	@Revenue);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,33,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,1),
(1,33,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,33,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
(3,33,	N'Quantity',		1,	N'Quantity',		N'الكمية',				1,4,0),
(4,33,	N'UnitId',			1,	N'',				N'',					1,4,0),
(7,33,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,1),
(8,33,	N'MonetaryValue',	0,	N'Price Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
(9,33,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
--34:PrepaidLeaseOut
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],		[TitleSingular2],			[TitlePlural],			[TitlePlural2],
[Description]) VALUES (
34,0,N'PrepaidLeaseOut',	N'Prepaid Rent/Sub',	N'تأجير مدفوع مقدما',	N'Prepaid Rents/Subs',	N'تأجيرات مدفوعة مقدما',
N'To recgonize revenues from leasing out of properties or software subscriptions, for which the lessee has already paid');
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
		[Time20]			= DATEADD(DAY, -1, dbo.fn_DateAdd([UnitId0],[Quantity0],[Time10])),
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 34;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,34,+1,	@DeferredIncomeClassifiedAsCurrent), -- @CurrentAccruedIncome
(1,34,-1,	@Revenue);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,34,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,0),
(1,34,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,34,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
(3,34,	N'Quantity',		1,	N'Duration',		N'الفترة',				1,4,1),
(4,34,	N'UnitId',			1,	N'',				N'',					1,4,1),
(5,34,	N'Time1',			1,	N'From',			N'ابتداء من',			1,4,1),
(6,34,	N'Time2',			1,	N'Till',			N'حتى',					1,1,0),
(7,34,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,0),
(8,34,	N'MonetaryValue',	0,	N'Due Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
(9,34,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
-- 35:PostpaidLeaseOut
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],		[TitleSingular2],			[TitlePlural],			[TitlePlural2],
[Description]) VALUES (
35,0,N'PostpaidLeaseOut',	N'Postpaid Rent/Sub',	N'تأجير مدفوع لاحقا',	N'Postpaid Rents/Subs',	N'تأجيرات مدفوعة لاحقا',
N'To recgonize revenues from leasing out of properties or software subscriptions, for which the lessee will pay later');
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
		[Time20]			= DATEADD(DAY, -1, dbo.fn_DateAdd([UnitId0],[Quantity0],[Time10])),
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 35;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId]) VALUES
(0,35,+1,	 @CurrentAccruedIncome),
(1,35,-1,	@Revenue);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,35,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,0),
(1,35,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,35,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
(3,35,	N'Quantity',		1,	N'Duration',		N'الفترة',				1,4,1),
(4,35,	N'UnitId',			1,	N'',				N'',					1,4,1),
(5,35,	N'Time1',			1,	N'From',			N'ابتداء من',			1,4,1),
(6,35,	N'Time2',			1,	N'Till',			N'حتى',					1,1,0),
(7,35,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,0),
(8,35,	N'MonetaryValue',	0,	N'Due Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
(9,35,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
-- 41:PPEDepreciation
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],			[TitleSingular],		[TitleSingular2],		[TitlePlural],				[TitlePlural2],
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
		[CenterId1]				= [CenterId0],
		[MonetaryValue0]		= [MonetaryValue1]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 8;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeParentId],		[EntryTypeId]) VALUES
(0,8,+1,	@DepreciationExpense,		NULL),
(1,8,-1,	@PropertyPlantAndEquipment,	@DepreciationPropertyPlantAndEquipment);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																	[ReadOnlyState],
																	[InheritsFromHeader]) VALUES
(0,8,	N'ResourceId',			1,	N'Asset',		N'الأصل',		1,4,0),
(1,8,	N'Quantity',			1,	N'Usage',		N'الاستخدام',	1,4,1),
(2,8,	N'UnitId',				1,	N'',			N'',			1,4,1),
(3,8,	N'CenterId',			0,	N'Cost Ctr',	N'مركز التكلفة',1,4,0),
(4,8,	N'EntryTypeId',			0,	N'Purpose',		N'الغرض',		1,4,0),
(5,8,	N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
(6,8,	N'Time2',				1,	N'Till',		N'حتى',			1,0,1),
(7,8,	N'MonetaryValue',		1,	N'Depreciation',N'الإهلاك',		1,0,0);
END