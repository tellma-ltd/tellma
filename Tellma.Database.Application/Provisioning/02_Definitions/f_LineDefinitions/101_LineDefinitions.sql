IF @DB = N'101' -- Banan SD, USD, en
BEGIN
--0:ManualLine 
INSERT @LineDefinitions([Index],
[Code],			[TitleSingular], [TitlePlural], [TitleSingular2], [TitlePlural3]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments',N'تسوية',			N'تسويات');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId]) VALUES
(0,0,+1,	@StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,0,	N'Account',		0,			N'Account',		4,4,0), -- together with properties
(1,0,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,0,	N'Value',		0,			N'Credit',		4,4,0),
(3,0,	N'Memo',		0,			N'Memo',		5,4,1);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,0,-4,	N'Duplicate Line',		N'بيانات مكررة'),
(1,0,-4,	N'Incorrect Analysis',	N'تحليل خطأ'),
(2,0,-4,	N'Other reasons',		N'أسباب أخرى');
--1:C_PaymentToSupplier
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
(1,1,N'C_PaymentToSupplier',N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId2],
		[CurrencyId1] = [CurrencyId2],
		[NotedAgentName2] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[NotedAmount1] = [MonetaryValue0],
		[MonetaryValue2] = ISNULL([MonetaryValue1],0) + ISNULL([MonetaryValue0],0),
		[CenterId0]	= [CenterId2],
		[CenterId1] = [CenterId2]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,1,+1,	@document_controlADef,	NULL),
(1,1,+1,	@vat_receivableADef,	NULL),
(2,1,-1,	@cashADef,				@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																					[ReadOnlyState],
																					[InheritsFromHeader]) VALUES
(0,1,	N'Memo',				1,	N'Memo',				N'البيان',				1,4,1),
(1,1,	N'NotedDate',			1,	N'Invoice Date',		N'تاريخ الفاتورة',		4,4,0), 
(2,1,	N'ExternalReference',	1,	N'Invoice #',			N'رقم الفاتورة',		4,4,0), 
(3,1,	N'NotedContractId',		1,	N'Supplier',			N'المورد',				3,4,1),
(4,1,	N'CurrencyId',			2,	N'Currency',			N'العملة',				1,2,1),
(5,1,	N'MonetaryValue',		0,	N'Price Excl. VAT',		N'المبلغ بدون ق.م.',	1,2,0),
(6,1,	N'MonetaryValue',		1,	N'VAT',					N'ق.م.',				3,4,0),
(7,1,	N'MonetaryValue',		2,	N'Total',				N'الإجمالي',				3,0,0),
(8,1,	N'ContractId',			2,	N'Bank/Cashier',		N'البنك\الخزنة',		3,4,0),
(9,1,	N'ExternalReference',	2,	N'Check/Receipt #',		N'رقم الشيك\الإيصال',	3,4,0),
(10,1,	N'NotedDate',			2,	N'Check Date',			N'تاريخ الشيك',			5,4,0),
(11,1,	N'CenterId',			2,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1);
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
--2:C_GoodReceipt
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],				[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES
(2,0,N'C_GoodReceipt',	N'Stock Receipt',	N'استلام مخزن',		N'Stocks Receipts',	N'استلام مخازن');
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
		[MonetaryValue1] = [MonetaryValue0]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 2;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,2,+1,	@inventoryADef,			@InventoryPurchaseExtension),
(1,2,-1,	@document_controlADef,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,2,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1),
(1,2,	N'ResourceId',			0,	N'Item',			N'الصنف',			3,4,0),
(2,2,	N'Quantity',			0,	N'Quantity',		N'الكمية',			1,2,0),
(3,2,	N'UnitId',				0,	N'Unit',			N'الوحدة',			1,2,0),
(4,2,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	N'السعر (بلا ق.م.)',1,2,0),
(5,2,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(6,2,	N'ContractId',			0,	N'Warehouse',		N'المخزن',			3,3,1),
(7,2,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(8,2,	N'NotedContractId',		0,	N'Supplier',		N'المورد',			3,3,1);
--3:C_PurchaseExpense
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],					[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
(3,0,N'C_PurchaseExpense',	N'Expense',		N'بند صرف',			N'Expenses',	N'بنود صرف');
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
		[MonetaryValue1] = [MonetaryValue0]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 3;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,3,+1,	@purchase_expenseADef,	NULL),
(1,3,-1,	@document_controlADef,	NULL);
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
--10:PaymentToSupplier (against invoice)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],		[TitleSingular2],	[TitlePlural],				[TitlePlural2]) VALUES
(10,1,N'PaymentToSupplier',	N'Payment to Supplier',	N'دفعية لمورد',		N'Payments to Suppliers',	N'دفعيات لموردين');
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
WHERE [Index] = 10;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,10,+1,	@supplierADef,			NULL),
(1,10,+1,	@vat_receivableADef,	NULL),
(2,10,-1,	@cashADef,				@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],				[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,10,	N'Memo',				1,	N'Memo',			N'البيان',				1,4,1),
(1,10,	N'NotedDate',			1,	N'Invoice Date',	N'تاريخ الفاتورة',		3,4,0), 
(2,10,	N'ExternalReference',	1,	N'Invoice #',		N'رقم الفاتورة',		3,4,0), 
(3,10,	N'NotedContractId',		1,	N'Supplier',		N'المورد',				3,4,1),
(4,10,	N'CurrencyId',			2,	N'Currency',		N'العملة',				1,2,1),
(5,10,	N'NotedAmount',			1,	N'Price Excl. VAT',	N'المبلغ بدون ق.م.',	1,2,0),
(6,10,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',				3,4,0),
(7,10,	N'MonetaryValue',		2,	N'Total',			N'الإجمالي',				3,0,0),
(8,10,	N'ContractId',			2,	N'Bank/Cashier',	N'البنك\الخزنة',		3,4,0),
(9,10,	N'ExternalReference',	2,	N'Check/Receipt #',	N'رقم الشيك\الإيصال',	3,4,0),
(10,10,	N'NotedDate',			2,	N'Check Date',		N'تاريخ الشيك',			5,4,0),
(11,10,	N'CenterId',			2,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,10,+1),
(1,10,+2),
(2,10,+3),
(3,10,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,10,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,10,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,10,N'ByAgent',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,10,N'ByRole',	@1Comptroller,		NULL,			NULL);
--11:PaymentToEmployee (in Banan SD, we will have a dedicated voucher for that)
--12:PaymentToPartner
--15:RefundToCustomer.
--19:PaymentToOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES (
19,1,N'PaymentToOther',		N'Other Payment',	N'دفعية أخرى',		N'Other Payments',	N'دفعيات أخرى');
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
		[MonetaryValue0] = [MonetaryValue1],
		[CenterId0] = [CenterId1]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 19;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId]) VALUES
(0,19,	+1,	@document_controlADef),
(1,19,	-1,	@cashADef);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																					[ReadOnlyState],
																					[InheritsFromHeader]) VALUES
(0,19,	N'Memo',				1,	N'Memo',				N'البيان',				1,2,1),
(1,19,	N'CurrencyId',			1,	N'Currency',			N'العملة',				1,2,1),
(2,19,	N'MonetaryValue',		1,	N'Pay Amount',			N'المبلغ',				1,2,0),
(3,19,	N'NotedAgentName',		1,	N'Beneficiary',			N'المستفيد',			3,3,0),
(4,19,	N'ContractId',			1,	N'Bank/Cashier',		N'البنك/الخزنة',		3,3,0),
(5,19,	N'ExternalReference',	1,	N'Check #/Receipt #',	N'رقم الشيك/الإيصال',	3,3,0),
(6,19,	N'NotedDate',			1,	N'Check Date',			N'تاريخ الشيك',			5,3,0),
(7,19,	N'CenterId',			1,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1),
(8,19,	N'EntryTypeId',			1,	N'Purpose',				N'الغرض',				4,4,0);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name],					[Name2]) VALUES
(0,19,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
(1,19,-3,	N'Other reasons',		N'أسباب أخرى');
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,19,+1),
(1,19,+2),
(2,19,+3),
(3,19,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,19,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,19,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,19,N'ByAgent',	NULL,				1,				NULL), -- cash/check custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,19,N'ByRole',	@1Comptroller,		NULL,			NULL);
--20:CashTransferExchange
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],		[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
20,1,N'CashTransferExchange',	N'Transfer/Exchange',	N'تحويل\صرف',		N'Transfers/Exchanges',	N'تحويلات\صرف');
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
WHERE [Index] = 20;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,20,+1,	@cashADef,				@InternalCashTransferExtension),
(1,20,-1,	@cashADef,				@InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],			[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,20,	N'ContractId',			1,	N'From Account',	N'من حساب',			1,2,0),
(1,20,	N'ContractId',			0,	N'To Account',		N'إلى حساب',		1,2,0),
(2,20,	N'CurrencyId',			1,	N'From Currency',	N'من عملة',			1,2,0),
(3,20,	N'CurrencyId',			0,	N'To Currency',		N'إلى عملة',		1,2,0),
(4,20,	N'MonetaryValue',		1,	N'From Amount',		N'من مبلغ',			1,3,0),
(5,20,	N'MonetaryValue',		0,	N'To Amount',		N'إلى مبلغ',		1,3,0),
(6,20,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(7,20,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,20,+1),
(1,20,+2),
(2,20,+3),
(3,20,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],			[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,20,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,20,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,20,N'ByAgent',	NULL,				0,				@1Comptroller), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(1,2,20,N'ByAgent',	NULL,				1,				@1Comptroller), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,20,N'ByRole',	@1Comptroller,		NULL,			NULL);
--22:LeaseIn
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitleSingular2],	[TitlePlural],				[TitlePlural2],
[Description]) VALUES (
22,0,N'LeaseIn',	N'Lease In/Subscription',	N'إيجار - اشتراك',	N'Leases In/Subscriptions',	N'إيجارات -اشتراكات',
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
WHERE [Index] = 22;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId]) VALUES
(0,22,+1,	@purchase_expenseADef),
(1,22,-1,	@supplierADef);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																	[ReadOnlyState],
																	[InheritsFromHeader]) VALUES
(0,22,	N'ContractId',			1,	N'Lessor',		N'المؤجر',		1,4,0),
(1,22,	N'CenterId',			0,	N'Cost Center',	N'مركز التكلفة',4,4,0),
(2,22,	N'ResourceId',			1,	N'Service',		N'الخدمة',		1,4,0),
(3,22,	N'Quantity',			1,	N'Duration',	N'الفترة',		1,4,1),
(4,22,	N'UnitId',				1,	N'',			N'',			1,4,1),
(5,22,	N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
(6,22,	N'Time2',				1,	N'Till',		N'حتى',			1,1,1),
(7,22,	N'CurrencyId',			1,	N'Currency',	N'العملة',		1,4,0),
(8,22,	N'MonetaryValue',		1,	N'Amount',		N'المطالبة',	1,4,0),
(9,22,	N'CenterId',			1,	N'Inv. Ctr',	N'مركز الاستثمار',4,4,1);

--30:C_PaymentFromCustomer, No workflow
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES (
30,1,N'C_PaymentFromCustomer',	N'Payment',		N'الدفعية',			N'Payments',	N'الدفعيات');
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
		[CurrencyId2] = [CurrencyId0],
		[CenterId1]		= [CenterId0],
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[MonetaryValue0] = ISNULL([MonetaryValue1],0) + ISNULL([NotedAmount1],0),
		[MonetaryValue2] = [NotedAmount1]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 30;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,30,+1,	@cashADef,				@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,30,-1,	@vat_payableADef,		NULL),
(2,30,-1,	@document_controlADef,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],				[RequiredState],
																					[ReadOnlyState],
																					[InheritsFromHeader]) VALUES
(0,30,	N'CenterId',			0,	N'Inv. Ctr',			N'مركز الاستثمار',		4,4,1),
(1,30,	N'Memo',				1,	N'Memo',				N'البيان',				1,5,1),
(2,30,	N'NotedAgentId',		1,	N'Customer',			N'الزبون',				1,4,1),
(3,30,	N'CurrencyId',			0,	N'Currency',			N'العملة',				1,2,1),
(4,30,	N'NotedAmount',			1,	N'Price Excl. VAT',		N'المبلغ بدون ق.م.',	1,2,0),
(5,30,	N'MonetaryValue',		1,	N'VAT',					N'ق.م.',				3,4,0),
(6,30,	N'MonetaryValue',		0,	N'Total',				N'الإجمالي',				3,0,0),
(7,30,	N'NotedDate',			0,	N'Payment Date',		N'تاريخ الدفعية',		3,4,0),
(8,30,	N'ContractId',			0,	N'Bank/Cashier',		N'البنك\الخزنة',		3,4,0),

(9,30,	N'NotedDate',			1,	N'Invoice Date',		N'تاريخ الفاتورة',		3,4,0), 
(10,30,	N'ExternalReference',	1,	N'Invoice #',			N'رقم الفاتورة',		3,4,0)
--31:C_ServiceDelivery
--32:C_GoodDelivery

--40:PaymentFromCustomer (against an invoice), No workflow
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitleSingular2],	[TitlePlural],			[TitlePlural2]) VALUES (
40,1,N'PaymentFromCustomer',N'Customer Payment',N'دفعية زبون',		N'Customer Payments',	N'دفعيات زبائن');
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
		[CenterId1]			= [CenterId0],
		[CenterId2]			= [CenterId0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[ContractId2]		= [ContractId1],
		[NotedAmount1]		= ISNULL([MonetaryValue2],0)
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 40;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId],	[EntryTypeId]) VALUES
(0,40,+1,	@cashADef,				@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,40,-1,	@vat_payableADef,		NULL),
(2,40,-1,	@customerADef,			NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[Label2],				[RequiredState],
																				[ReadOnlyState],
																				[InheritsFromHeader]) VALUES
(0,40,	N'Memo',				1,	N'Memo',			N'البيان',				1,5,1),
(1,40,	N'NotedContractId',		1,	N'Customer',		N'الزبون',				1,4,1),
(2,40,	N'CurrencyId',			2,	N'Contract Currency',N'عملة العقد',			1,2,1),
(3,40,	N'MonetaryValue',		2,	N'Price Excl. VAT',	N'المطالبة بدون ق.م.',	1,2,0),
(4,40,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',				1,2,0),
(5,40,	N'NotedAmount',			0,	N'Total',			N'الإجمالي',				2,0,0),
(6,40,	N'NotedDate',			2,	N'Due Date',		N'تاريخ الاستحقاق',		3,4,0),
(7,40,	N'NotedDate',			1,	N'Payment Date',	N'تاريخ السداد',		3,5,0),
(8,40,	N'ContractId',			0,	N'Bank/Cashier',	N'البنك\الخزنة',		3,4,1),
(9,40,	N'CurrencyId',			0,	N'Rcvd. Currency',	N'عملة الاستلام',			3,4,0),
(10,40,	N'MonetaryValue',		0,	N'Rcvd. Amount',	N'الإجمالي المستلم',	3,4,0),
(11,40,	N'ExternalReference',	1,	N'Invoice #',		N'رقم الفاتورة',		3,5,0),
(40,40,	N'CenterId',			0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
--41:PaymentFromEmployee
--42:PaymentFromPartner
--45:RefundFromSupplier.
--49:PaymentFromOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],		[TitleSingular2],		[TitlePlural],			[TitlePlural2]) VALUES (
49,1,N'PaymentFromOther',	N'Payment from Others',	N'دفعية من جهة أخرى',	N'Payments from Others',N'دفعيات من جهات أخرى');
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
		[MonetaryValue1] = [MonetaryValue0]
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 49;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId]) VALUES
(0,49,+1,	@cashADef),
(1,49,-1,	@document_controlADef);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,49,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(1,49,	N'MonetaryValue',		0,	N'Amount',			N'المبلغ',			1,2,0),
(2,49,	N'NotedAgentName',		0,	N'Received from',	N'مستلم من',		3,4,0),
(3,49,	N'ContractId',			0,	N'Bank/Cashier',	N'البنك/الخزنة',	3,4,1),
(4,49,	N'ExternalReference',	0,	N'Check/Receipt #',	N'رقم الشيك/الإيصال',5,4,0),
(5,49,	N'NotedDate',			0,	N'Check Date',		N'تاريخ الشيك',		5,4,0),
(6,49,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(7,49,	N'EntryTypeId',			0,	N'Purpose',			N'الغرض',			4,4,0),
(8,49,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1);
--51:ServiceDelivery
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2],
[Description]) VALUES (
51,1,N'ServiceDelivery',	N'Service',		N'الخدمة',			N'Services',	N'الخدمات',
N'To recgonize revenues from delivering services');
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
WHERE [Index] = 51;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId]) VALUES
(0,51,+1,	@customerADef), -- @CurrentAccruedIncome
(1,51,-1,	@revenueADef);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,51,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,1),
(1,51,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,51,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
(3,51,	N'Quantity',		1,	N'Quantity',		N'الكمية',				1,4,0),
(4,51,	N'UnitId',			1,	N'',				N'',					1,4,0),
(7,51,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,1),
(8,51,	N'MonetaryValue',	0,	N'Price Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
(9,51,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
--52:LeaseOut
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
[Code],				[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2],
[Description]) VALUES (
52,0,N'LeaseOut',	N'Rent/Sub',	N'تأجير',			N'Rents/Subs',	N'تأجيرات',
N'To recgonize revenues from leasing out of properties or software subscriptions');
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
WHERE [Index] = 52;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountDefinitionId]) VALUES
(0,52,+1,	@customerADef), -- @CurrentAccruedIncome
(1,52,-1,	@revenueADef);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,52,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,0),
(1,52,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0),
(2,52,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0),
(3,52,	N'Quantity',		1,	N'Duration',		N'الفترة',				1,4,1),
(4,52,	N'UnitId',			1,	N'',				N'',					1,4,1),
(5,52,	N'Time1',			1,	N'From',			N'ابتداء من',			1,4,1),
(6,52,	N'Time2',			1,	N'Till',			N'حتى',					1,1,0),
(7,52,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,0),
(8,52,	N'MonetaryValue',	0,	N'Due Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0),
(9,52,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1);
--53:GoodDelivery

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
[Direction],[AccountDefinitionId],		[EntryTypeId]) VALUES
(0,8,+1,	@depreciation_expenseADef,	NULL),
(1,8,-1,	@ppeADef,					@DepreciationPropertyPlantAndEquipment);
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