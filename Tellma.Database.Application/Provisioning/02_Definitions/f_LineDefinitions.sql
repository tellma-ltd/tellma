-- Suppliers(inv-gs,cash | inv-cash-gs | inv-cash,gs | gs,inv-cash)
-- Customers(inv-gs,cash | inv-cash-gs | inv-Cash,gs | gs,inv-cash)
--	credit purchase/sale | cash pur/sal| Prep/Unearn | Postpayment

-- Payment to Suppliers
-- credit purchase: Dr. A/P, Cr. Cash
-- cash purchase: Dr. Cash Purchase Doc control, Dr. VAT Receivable, Cr. Cash
-- Prepayment: Dr. Prepayment, Dr. VAT Receivable, Cr. Cash
-- postinvoice: Dr. Accrued Expense, Dr. VAT receivable, Cr. Cash

-- Stock Receipts from Suppliers
-- credit purchase: Dr. Expense, Dr. VAT receivable, Cr. A/P
-- cash purchase: Dr. Expense, Cr. Cash Purchase Doc control
-- prepaid: Dr. Expenses, Cr. Prepayment
-- post invoiced: Dr. Expenses, Cr. Accrued Expense

-- Payment from Customers
-- credit sale: Dr. Cash, Cr. A/R
-- cash sale: Dr. Cash, Cr. Cash sale Doc control
-- prepayment: Dr. Cash, Cr. VAT Payable, Cr. Unearned Revenues
-- post pay accrual: Dr. Cash, Cr. VAT Payable, Cr. Accrued income

-- G/S Delivered to Customers
-- credit sale: Dr. A/R, Cr. VAT payable, Cr. Revenues
-- cash sale: Dr. Cash sale Doc control, Cr. VAT payable, Cr. Revenues
-- prepaid: Dr. Unearned Revenues, Cr. Revenues
-- post invoiced: Dr. Accrued income, Cr. Revenues

--0:ManualLine 
INSERT @LineDefinitions([Index],
[Code],			[TitleSingular], [TitlePlural]) VALUES
(0,N'ManualLine', N'Adjustment', N'Adjustments');
INSERT INTO @LineDefinitionVariants([Index], [HeaderIndex], [Name]) VALUES
(0,0,N'Default');
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[VariantIndex],
[Direction], [AccountTypeId]) VALUES
(0,0,0,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,0,	N'Account',		0,			N'Account',		4,4,0), -- together with properties
(1,0,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,0,	N'Value',		0,			N'Credit',		4,4,0),
(3,0,	N'Memo',		0,			N'Memo',		5,4,1);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name]) VALUES
(0,0,-4,	N'Duplicate Line'),
(1,0,-4,	N'Incorrect Analysis'),
(2,0,-4,	N'Other reasons');
--1:PaymentToSupplierCreditPurchase (inv-gs,cash) [for suppliers w/ credit line, rarely used in ET]
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm], [Code],			[TitleSingular],				[TitlePlural]) VALUES
(1,1,N'PaymentToSupplierCreditPurchase',N'Payment (Credit Purchase)',	N'Payments (Credit Purchases)');
INSERT INTO @LineDefinitionVariants([Index], [HeaderIndex], [Name]) VALUES
(0,1,N'Cash'),
(1,1,N'Cheque')
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId0]			= [CenterId1]
'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex], [VariantIndex],
[Direction],[AccountTypeId],							[EntryTypeId]) VALUES
(0,1,0,+1,	@TradeAndOtherCurrentPayablesToTradeSuppliers,	NULL),
(1,1,0,-1,	@CashOnHand,						@PaymentsToSuppliersForGoodsAndServices),

(0,1,1,+1,	@TradeAndOtherCurrentPayablesToTradeSuppliers,	NULL),
(1,1,1,-1,	@BalancesWithBanks,						@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1,	N'ContractId',			0,	N'Supplier',		3,4,1),
(2,1,	N'CurrencyId',			0,	N'Invoice Currency',1,2,1),
(3,1,	N'MonetaryValue',		0,	N'Invoice Amount',	1,2,0),
(4,1,	N'ContractId',			1,	N'Bank/Cashier',	3,4,0),
(5,1,	N'ExternalReference',	1,	N'Check/Receipt #',	3,4,0),
(6,1,	N'NotedDate',			1,	N'Check Date',		5,4,0),
(7,1,	N'CenterId',			1,	N'Inv. Ctr',		4,4,1);
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
(0,2,1,N'ByAgent',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,1,N'ByRole',	@1Comptroller,		NULL,			NULL);
--2:PaymentToSupplierCashPurchase (inv-cash-gs)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],		[TitlePlural]) VALUES
(2,1,N'PaymentToSupplierCashPurchase',	N'Payment (Cash Purchase)',	N'Payments (Cash Purchases)');
INSERT INTO @LineDefinitionVariants([Index], [HeaderIndex], [Name]) VALUES
(0,2,N'Cash'),
(1,2,N'Cheque')
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]	= [CurrencyId2],
		[CurrencyId1]	= [CurrencyId2],

		[NotedContractId1]	= [ContractId0],
		[NotedAgentName2]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[NotedAmount1]		= [MonetaryValue0],
		[MonetaryValue2]	= ISNULL([MonetaryValue0],0) + ISNULL([MonetaryValue1],0),
		
		[CenterId0]		= [CenterId2],
		[CenterId1]		= [CenterId2]
'
WHERE [Index] = 2;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex], [VariantIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,2,0,+1,	@CashPurchaseDocumentControlExtension,	NULL),
(1,2,0,+1,	@CurrentValueAddedTaxReceivables,		NULL),
(2,2,0,-1,	@CashOnHand,							@PaymentsToSuppliersForGoodsAndServices),

(0,2,1,+1,	@CashPurchaseDocumentControlExtension,	NULL),
(1,2,1,+1,	@CurrentValueAddedTaxReceivables,		NULL),
(2,2,1,-1,	@BalancesWithBanks,						@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,2,	N'Memo',				1,	N'Memo',			1,4,1),
(1,2,	N'NotedDate',			1,	N'Invoice Date',	3,4,0), 
(2,2,	N'ExternalReference',	1,	N'Invoice #',		3,4,0), 
(3,2,	N'ContractId',			0,	N'Supplier',		3,4,1),
(4,2,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(5,2,	N'MonetaryValue',		0,	N'Price Excl. VAT',	1,2,0),
(6,2,	N'MonetaryValue',		1,	N'VAT',				3,4,0),
(7,2,	N'MonetaryValue',		2,	N'Total',			3,0,0),
(8,2,	N'ContractId',			2,	N'Bank/Cashier',	3,4,0),
(9,2,	N'ExternalReference',	2,	N'Check/Receipt #',	3,4,0),
(10,2,	N'NotedDate',			2,	N'Check Date',		5,4,0),
(11,2,	N'CenterId',			2,	N'Inv. Ctr',		4,4,1);
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
GOTO ENOUGH_LD
--3:PrepaymentToSupplier (inv-cash,gs)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],			[TitlePlural]) VALUES
(3,1,N'PrepaymentToSupplier',	N'Prepayment to Supplier',	N'Prepayments to Suppliers');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]	= [CurrencyId2],
		[CurrencyId1]	= [CurrencyId2],

		[NotedContractId1] = [ContractId0],
		[NotedAgentName2] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[NotedAmount1]		= [MonetaryValue0],
		[MonetaryValue2]	= ISNULL([MonetaryValue0],0) + ISNULL([MonetaryValue1],0),
		
		[CenterId0]		= [CenterId2],
		[CenterId1]		= [CenterId2]
'
WHERE [Index] = 3;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,3,+1,	@CurrentPrepayments,				NULL),
(1,3,+1,	@CurrentValueAddedTaxReceivables,	NULL),
(2,3,-1,	@CashAndCashEquivalents,			@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,3,	N'Memo',				1,	N'Memo',			1,4,1),
(1,3,	N'NotedDate',			1,	N'Invoice Date',	3,4,0), 
(2,3,	N'ExternalReference',	1,	N'Invoice #',		3,4,0), 
(3,3,	N'ContractId',			0,	N'Supplier',		3,4,1),
(4,3,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(5,3,	N'MonetaryValue',		0,	N'Price Excl. VAT',	1,2,0),
(6,3,	N'MonetaryValue',		1,	N'VAT',				3,4,0),
(7,3,	N'MonetaryValue',		2,	N'Total',			3,0,0),
(8,3,	N'ContractId',			2,	N'Bank/Cashier',	3,4,0),
(9,3,	N'ExternalReference',	2,	N'Check/Receipt #',	3,4,0),
(10,3,	N'NotedDate',			2,	N'Check Date',		5,4,0),
(11,3,	N'CenterId',			2,	N'Inv. Ctr',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,3,+1),
(1,3,+2),
(2,3,+3),
(3,3,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,3,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,3,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,3,N'ByAgent',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,3,N'ByRole',	@1Comptroller,		NULL,			NULL);
--4:PaymentToSupplierAccrual (gs,inv-cash) [for utilities, rarely used, mostly treated as cash purchase]
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],		[TitleSingular],			[TitlePlural]) VALUES
(4,1,N'PaymentToSupplierAccrual',	N'Postpayment to Supplier',	N'Postpayments to Suppliers');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]	= [CurrencyId2],
		[CurrencyId1]	= [CurrencyId2],

		[NotedContractId1] = [ContractId0],
		[NotedAgentName2] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[MonetaryValue0]= ISNULL([NotedAmount1],0),
		[MonetaryValue2]= ISNULL([MonetaryValue1],0) + ISNULL([NotedAmount1],0),
		
		[CenterId0]		= [CenterId2],
		[CenterId1]		= [CenterId2]
'
WHERE [Index] = 4;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,4,+1,	@AccrualsClassifiedAsCurrent,		NULL),
(1,4,+1,	@CurrentValueAddedTaxReceivables,	NULL),
(2,4,-1,	@CashAndCashEquivalents,			@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,4,	N'Memo',				1,	N'Memo',			1,4,1),
(1,4,	N'NotedDate',			1,	N'Invoice Date',	3,4,0), 
(2,4,	N'ExternalReference',	1,	N'Invoice #',		3,4,0), 
(3,4,	N'ContractId',			0,	N'Supplier',		3,4,1),
(4,4,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(5,4,	N'NotedAmount',			1,	N'Price Excl. VAT',	1,2,0),
(6,4,	N'MonetaryValue',		1,	N'VAT',				3,4,0),
(7,4,	N'MonetaryValue',		2,	N'Total',			3,0,0),
(8,4,	N'ContractId',			2,	N'Bank/Cashier',	3,4,0),
(9,4,	N'ExternalReference',	2,	N'Check/Receipt #',	3,4,0),
(10,4,	N'NotedDate',			2,	N'Check Date',		5,4,0),
(11,4,	N'CenterId',			2,	N'Inv. Ctr',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,4,+1),
(1,4,+2),
(2,4,+3),
(3,4,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,4,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,4,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,4,N'ByAgent',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,4,N'ByRole',	@1Comptroller,		NULL,			NULL);
--8:PaymentToEmployee (used in a payroll voucher)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],			[TitlePlural]) VALUES
(8,1,N'PaymentToEmployee',	N'Payment Employee Benefit',N'Payments Employee Benefits');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]	= [CurrencyId0],
		[NotedContractId1] = [ContractId0],
		[MonetaryValue1]= [MonetaryValue0],
		[CenterId0]		= [CenterId1]
'
WHERE [Index] = 8;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
-- We might better add ContractDefinitionId and limit it to employees for this case
[Direction],[AccountTypeId],		[EntryTypeId]) VALUES
(0,8,+1,	@OtherCurrentPayables,		NULL),
(1,8,-1,	@CashAndCashEquivalents,	@PaymentsToAndOnBehalfOfEmployees);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,8,	N'Memo',				0,	N'Memo',			1,4,1),
(1,8,	N'ContractId',			0,	N'Employee',		3,4,1),
(2,8,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,8,	N'MonetaryValue',		0,	N'Amount',			1,2,0),
(4,8,	N'ContractId',			1,	N'Bank/Cashier',	3,4,0),
(5,8,	N'ExternalReference',	1,	N'Check/Receipt #',	3,4,0),
(6,8,	N'NotedDate',			1,	N'Check Date',		5,4,0),
(7,8,	N'CenterId',			1,	N'Inv. Ctr',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,8,+1),
(1,8,+2),
(2,8,+3),
(3,8,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,8,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,8,N'ByRole',	@1GeneralManager,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,8,N'ByAgent',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,8,N'ByRole',	@1Comptroller,		NULL,			NULL);
--9:PaymentToOther
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],[TitleSingular],	[TitlePlural]) VALUES (
9,1,N'PaymentToOther',		N'Other Payment',	N'Other Payments');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1],
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 9;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId]) VALUES
(0,9,	+1,	@OtherDocumentControlExtension),
(1,9,	-1,	@CashAndCashEquivalents);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,9,	N'Memo',				1,	N'Memo',				1,2,1),
(1,9,	N'CurrencyId',			1,	N'Currency',			1,2,1),
(2,9,	N'MonetaryValue',		1,	N'Pay Amount',			1,2,0),
(3,9,	N'NotedAgentName',		1,	N'Beneficiary',			3,3,0),
(4,9,	N'ContractId',			1,	N'Bank/Cashier',		3,3,0),
(5,9,	N'ExternalReference',	1,	N'Check #/Receipt #',	3,3,0),
(6,9,	N'NotedDate',			1,	N'Check Date',			5,3,0),
(7,9,	N'CenterId',			1,	N'Inv. Ctr',			4,4,1),
(8,9,	N'EntryTypeId',			1,	N'Purpose',				4,4,0);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name]) VALUES
(0,9,-3,	N'Insufficient Balance'),
(1,9,-3,	N'Other reasons');
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
(0,2,9,N'ByAgent',	NULL,				1,				NULL), -- cash/check custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,9,N'ByRole',	@1Comptroller,		NULL,			NULL);
--10:CashTransferExchange
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],	[TitleSingular],		[TitlePlural]) VALUES (
10,1,N'CashTransferExchange',	N'Transfer/Exchange',	N'Transfers/Exchanges');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId1] = [CenterId0],
		[CenterId2] = [CenterId0],
		[CurrencyId2] = dbo.fn_FunctionalCurrencyId(),
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0]),
		[MonetaryValue2] = wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId1], [MonetaryValue1])
							- wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId0], [MonetaryValue0]) 
'
WHERE [Index] = 10;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,10,+1,	@CashAndCashEquivalents,			@InternalCashTransferExtension),
(1,10,-1,	@CashAndCashEquivalents,			@InternalCashTransferExtension),
(2,10,+1,	@GainLossOnForeignExchangeExtension,NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,10,	N'ContractId',			1,	N'From Account',	1,2,0),
(1,10,	N'ContractId',			0,	N'To Account',		1,2,0),
(2,10,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,10,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,10,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,10,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,10,	N'CenterId',			0,	N'Invest. Ctr',		4,4,1),
(7,10,	N'Memo',				0,	N'Memo',			1,2,1);
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
--11:StockReceiptCreditPurchase (inv-gs,cash) [rarely used in ET]
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],							[TitleSingular],					[TitlePlural]) VALUES
(11,0,N'StockReceiptCreditPurchase',N'Stock Receipt (Credit Purchase)',	N'Stocks Receipts (Credit Purchases)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 11;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],								[EntryTypeId]) VALUES
(0,11,+1,	@Inventories,									@ReceiptsReturnsThroughPurchaseExtension),
(1,11,-1,	@CurrentValueAddedTaxReceivables,				NULL),
(2,11,-1,	@TradeAndOtherCurrentPayablesToTradeSuppliers,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,11,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1),
(1,11,	N'ResourceId',			0,	N'Item',			N'الصنف',			3,4,0),
(2,11,	N'Quantity',			0,	N'Quantity',		N'الكمية',			1,2,0),
(3,11,	N'UnitId',				0,	N'Unit',			N'الوحدة',			1,2,0),
(4,11,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	N'السعر (بلا ق.م.)',1,2,0),
(5,11,	N'MonetaryValue',		1,	N'VAT',				N'ق.م.',			1,2,0),
(6,11,	N'MonetaryValue',		2,	N'Price (w/ VAT)',	N'السعر (مع ق.م.)',1,2,0),
(7,11,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(8,11,	N'ContractId',			0,	N'Warehouse',		N'المخزن',			3,3,1),
(9,11,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(10,11,	N'NotedContractId',		0,	N'Supplier',		N'المورد',			3,3,1);
--12:StockReceiptCashPurchase (inv-cash-GS)
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],							[TitleSingular],					[TitlePlural]) VALUES
(12,0,N'StockReceiptCashPurchase',	N'Stock Receipt (Cash Purchase)',	N'Stocks Receipts (Cash Purchases)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 12;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],						[EntryTypeId]) VALUES
(0,12,+1,	@Inventories,							@ReceiptsReturnsThroughPurchaseExtension),
(1,12,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,12,	N'Memo',				0,	N'Memo',			1,2,1),
(1,12,	N'ResourceId',			0,	N'Item',			3,4,0),
(2,12,	N'Quantity',			0,	N'Quantity',		1,2,0),
(3,12,	N'UnitId',				0,	N'Unit',			1,2,0),
(4,12,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(5,12,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(6,12,	N'ContractId',			0,	N'Warehouse',		3,3,1),
(7,12,	N'CenterId',			0,	N'Invest. Ctr',		4,4,1),
(8,12,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--13:StockReceiptPrepaid (inv-cash,gs)
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],						[TitleSingular],			[TitlePlural]) VALUES
(13,0,N'StockReceiptPrepaid',N'Stock Receipt (Prepaid)',	N'Stocks Receipts (Prepaid)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 13;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],		[EntryTypeId]) VALUES
(0,13,+1,	@Inventories,			@ReceiptsReturnsThroughPurchaseExtension),
(1,13,-1,	@CurrentPrepayments,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,13,	N'Memo',				0,	N'Memo',			1,2,1),
(1,13,	N'ResourceId',			0,	N'Item',			3,4,0),
(2,13,	N'Quantity',			0,	N'Quantity',		1,2,0),
(3,13,	N'UnitId',				0,	N'Unit',			1,2,0),
(4,13,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(5,13,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(6,13,	N'ContractId',			0,	N'Warehouse',		3,3,1),
(7,13,	N'CenterId',			0,	N'Invest. Ctr',		4,4,1),
(8,13,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--14:StockReceiptPostInvoiced (gs,inv-cash) [rarely used]
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],							[TitleSingular],					[TitlePlural]) VALUES
(14,0,N'StockReceiptPostInvoiced',	N'Stock Receipt (Post Invoiced)',	N'Stocks Receipts (Post Invoiced)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 14;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,14,+1,	@Inventories,					@ReceiptsReturnsThroughPurchaseExtension),
(1,14,-1,	@AccrualsClassifiedAsCurrent,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,14,	N'Memo',				0,	N'Memo',			1,2,1),
(1,14,	N'ResourceId',			0,	N'Item',			3,4,0),
(2,14,	N'Quantity',			0,	N'Quantity',		1,2,0),
(3,14,	N'UnitId',				0,	N'Unit',			1,2,0),
(4,14,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(5,14,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(6,14,	N'ContractId',			0,	N'Warehouse',		3,3,1),
(7,14,	N'CenterId',			0,	N'Invest. Ctr',		4,4,1),
(8,14,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--15:ConsumableServiceReceiptCreditPurchase (inv-gs,cash) [rarely used, applies to travel expenses]
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],										[TitleSingular],					[TitlePlural]) VALUES
(15,0,N'ConsumableServiceReceiptCreditPurchase',N'C/S Receipt (Credit Purchase)',	N'C/S Receipts (Credit Purchases)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 15;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,15,+1,	@ExpenseByNature,						NULL),
(1,15,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,15,	N'Memo',				0,	N'Memo',			1,2,1),
(1,15,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,15,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,15,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,15,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,15,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--16:ConsumableServiceReceiptCashPurchase (inv-cash-gs)
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],										[TitleSingular],				[TitlePlural]) VALUES
(16,0,N'ConsumableServiceReceiptCashPurchase',	N'C/S Receipt (Cash Purchase)',	N'C/S Receipts (Cash Purchases)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 16;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,16,+1,	@ExpenseByNature,						NULL),
(1,16,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,16,	N'Memo',				0,	N'Memo',			1,2,1),
(1,16,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,16,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,16,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,16,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,16,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--17:ConsumableServiceReceiptPrepaid (inv-cash,gs)
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],										[TitleSingular],			[TitlePlural]) VALUES
(17,0,N'ConsumableServiceReceiptPrepaid',	N'C/S Receipt (Prepaid)',	N'C/S Receipts (Prepaid)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 17;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,17,+1,	@ExpenseByNature,						NULL),
(1,17,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,17,	N'Memo',				0,	N'Memo',			1,2,1),
(1,17,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,17,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,17,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,17,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,17,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--18:ConsumableServiceReceiptPostInvoiced (gs,inv-cash) [rarely used]
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
	[Code],										[TitleSingular],				[TitlePlural]) VALUES
(18,0,N'ConsumableServiceReceiptPostInvoiced',	N'C/S Receipt (Post Invoiced)',	N'C/S Receipts (Post Invoiced)');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 18;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,18,+1,	@ExpenseByNature,						NULL),
(1,18,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,18,	N'Memo',				0,	N'Memo',			1,2,1),
(1,18,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,18,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,18,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,18,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,18,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--19:LeaseInPrepaid (inv-cash,gs) -- most common
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
[Code],[TitleSingular],	[TitlePlural], [Description]) VALUES (
19,0,N'LeaseInPrepaid',	N'Lease In/Subscription (Prepaid)',	N'Leases In/Subscriptions (Prepaid)',
N'For lease in of properties or software subscriptions. Indicates the rendering of service');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 19;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,19,+1,	@ExpenseByNature,						NULL),
(1,19,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,19,	N'ContractId',			1,	N'Lessor',		1,4,0),
(1,19,	N'CenterId',			0,	N'Cost Center',	4,4,0),
(2,19,	N'ResourceId',			1,	N'Service',		1,4,0),
(3,19,	N'Quantity',			1,	N'Duration',	1,4,1),
(4,19,	N'UnitId',				1,	N'',			1,4,1),
(5,19,	N'Time1',				1,	N'From',		1,4,1),
(6,19,	N'Time2',				1,	N'Till',		1,1,1),
(7,19,	N'CurrencyId',			1,	N'Currency',	1,4,0),
(8,19,	N'MonetaryValue',		1,	N'Amount',		1,4,0),
(9,19,	N'CenterId',			1,	N'Inv. Ctr',	4,4,1);
--20:LeaseInPostinvoiced (gs,inv-cash) [rarely used, may be in hotels where you can see transient bill]
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
[Code],[TitleSingular],	[TitlePlural], [Description]) VALUES (
20,0,N'LeaseInPostinvoiced',	N'Lease In/Subscription (Post Invoiced)',	N'Leases In/Subscriptions (Post Invoiced)',
N'For lease in of properties or software subscriptions. Indicates the rendering of service');
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 20;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],					[EntryTypeId]) VALUES
(0,20,+1,	@ExpenseByNature,						NULL),
(1,20,-1,	@CashPurchaseDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,20,	N'ContractId',			1,	N'Lessor',		1,4,0),
(1,20,	N'CenterId',			0,	N'Cost Center',	4,4,0),
(2,20,	N'ResourceId',			1,	N'Service',		1,4,0),
(3,20,	N'Quantity',			1,	N'Duration',	1,4,1),
(4,20,	N'UnitId',				1,	N'',			1,4,1),
(5,20,	N'Time1',				1,	N'From',		1,4,1),
(6,20,	N'Time2',				1,	N'Till',		1,1,1),
(7,20,	N'CurrencyId',			1,	N'Currency',	1,4,0),
(8,20,	N'MonetaryValue',		1,	N'Amount',		1,4,0),
(9,20,	N'CenterId',			1,	N'Inv. Ctr',	4,4,1);
 --41:PaymentFromCustomerCreditSale (inv-gs,cash) 21,22,23,24:PPE, 25,26,27,28 Biological, 29,30,31,32 IP, ...
-- credit sale: Dr. Cash, Cr. A/R
-- cash sale: Dr. Cash, Cr. Cash sale Doc control
-- prepayment: Dr. Cash, Cr. VAT Payable, Cr. Unearned Revenues
-- post pay accrual: Dr. Cash, Cr. VAT Payable, Cr. Accrued income
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],			[TitleSingular],					[TitlePlural]) VALUES (
41,1,N'PaymentFromCustomerCreditSale',	N'Customer Payment (Credit Sale)',	N'Customer Payments (Credit Sale)');
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[MonetaryValue1]	= [MonetaryValue0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1])
	-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 41;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],			[EntryTypeId]) VALUES
(0,41,+1,	@CashAndCashEquivalents,		@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,41,-1,	@CurrentTradeReceivables,		NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,41,	N'Memo',				1,	N'Memo',			1,5,1),
(1,41,	N'ContractId',			1,	N'Customer',		1,4,1),
(2,41,	N'CurrencyId',			0,	N'Currency',		2,0,0),
(3,41,	N'MonetaryValue',		0,	N'Amount',			2,0,0),
(4,41,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(5,41,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--42:PaymentFromCustomerCashSale (inv-cash-gs)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],		[TitleSingular],				[TitlePlural]) VALUES (
42,1,N'PaymentFromCustomerCashSale',N'Customer Payment (Cash Sale)',N'Customer Payments (Cash Sale)');
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
WHERE [Index] = 42;
INSERT INTO @LineDefinitionEntries([Index],[HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,42,+1,	@CashAndCashEquivalents,			@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,42,-1,	@CurrentValueAddedTaxPayables,		NULL),
(2,42,-1,	@CashSaleDocumentControlExtension,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,42,	N'Memo',				1,	N'Memo',			1,5,1),
(1,42,	N'NotedContractId',		1,	N'Customer',		1,4,1),
(2,42,	N'CurrencyId',			2,	N'Contract Currency',1,2,1),
(3,42,	N'MonetaryValue',		2,	N'Price Excl. VAT',	1,2,0),
(4,42,	N'MonetaryValue',		1,	N'VAT',				1,2,0),
(5,42,	N'NotedAmount',			0,	N'Total',			2,0,0),
(6,42,	N'NotedDate',			2,	N'Due Date',		3,4,0),
(7,42,	N'NotedDate',			1,	N'Payment Date',	3,5,0),
(8,42,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(9,42,	N'CurrencyId',			0,	N'Rcvd. Currency',	3,4,0),
(10,42,	N'MonetaryValue',		0,	N'Rcvd. Amount',	3,4,0),
(11,42,	N'ExternalReference',	1,	N'Invoice #',		3,5,0),
(42,42,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--43:PrepaymentFromCustomer (inv-Cash,gs)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],		[TitleSingular],				[TitlePlural]) VALUES (
43,1,N'PrepaymentFromCustomer',	N'Customer Prepayment',	N'Customer Prepayments');
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
WHERE [Index] = 43;
INSERT INTO @LineDefinitionEntries([Index],[HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,43,+1,	@CashAndCashEquivalents,			@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,43,-1,	@CurrentValueAddedTaxPayables,		NULL),
(2,43,-1,	@DeferredIncomeClassifiedAsCurrent,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,43,	N'Memo',				1,	N'Memo',			1,5,1),
(1,43,	N'NotedContractId',		1,	N'Customer',		1,4,1),
(2,43,	N'CurrencyId',			2,	N'Contract Currency',1,2,1),
(3,43,	N'MonetaryValue',		2,	N'Price Excl. VAT',	1,2,0),
(4,43,	N'MonetaryValue',		1,	N'VAT',				1,2,0),
(5,43,	N'NotedAmount',			0,	N'Total',			2,0,0),
(6,43,	N'NotedDate',			2,	N'Due Date',		3,4,0),
(7,43,	N'NotedDate',			1,	N'Payment Date',	3,5,0),
(8,43,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(9,43,	N'CurrencyId',			0,	N'Rcvd. Currency',	3,4,0),
(10,43,	N'MonetaryValue',		0,	N'Rcvd. Amount',	3,4,0),
(11,43,	N'ExternalReference',	1,	N'Invoice #',		3,5,0),
(43,43,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--44:PaymentFromCustomerAccrual (gs,inv-cash)
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],		[TitleSingular],		[TitlePlural]) VALUES (
44,1,N'PaymentFromCustomerAccrual',	N'Customer Postpayment',N'Customer Postpayments');
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
WHERE [Index] = 44;
INSERT INTO @LineDefinitionEntries([Index],[HeaderIndex],
[Direction],[AccountTypeId],				[EntryTypeId]) VALUES
(0,44,+1,	@CashAndCashEquivalents,			@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,44,-1,	@CurrentValueAddedTaxPayables,		NULL),
(2,44,-1,	@DeferredIncomeClassifiedAsCurrent,	NULL);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,44,	N'Memo',				1,	N'Memo',			1,5,1),
(1,44,	N'NotedContractId',		1,	N'Customer',		1,4,1),
(2,44,	N'CurrencyId',			2,	N'Contract Currency',1,2,1),
(3,44,	N'MonetaryValue',		2,	N'Price Excl. VAT',	1,2,0),
(4,44,	N'MonetaryValue',		1,	N'VAT',				1,2,0),
(5,44,	N'NotedAmount',			0,	N'Total',			2,0,0),
(6,44,	N'NotedDate',			2,	N'Due Date',		3,4,0),
(7,44,	N'NotedDate',			1,	N'Payment Date',	3,5,0),
(8,44,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(9,44,	N'CurrencyId',			0,	N'Rcvd. Currency',	3,4,0),
(10,44,	N'MonetaryValue',		0,	N'Rcvd. Amount',	3,4,0),
(11,44,	N'ExternalReference',	1,	N'Invoice #',		3,5,0),
(44,44,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--49:PaymentFromOther, 47:RefundFromSupplier, 48:PaymentFromEmployee
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
[Direction],[AccountTypeId]) VALUES
(0,49,+1,	@CashAndCashEquivalents),
(1,49,-1,	@OtherDocumentControlExtension);
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

--51:G/S Delivered to Customers
-- credit sale: Dr. A/R, Cr. VAT payable, Cr. Revenues
-- cash sale: Dr. Cash sale Doc control, Cr. VAT payable, Cr. Revenues
-- prepaid: Dr. Unearned Revenues, Cr. Revenues
-- post invoiced: Dr. Accrued income, Cr. Revenues
--53:@StockIssueCreditSaleLD
--@StockIssueCashSaleLD
--@StockIssuePrepaidLD
--@StockIssuePostInvoicedLD
--@ServiceIssueCreditSaleLD
--@ServiceIssueCashSaleLD
--@ServiceIssuePrepaidLD
--@ServiceIssuePostInvoicedLD
--@LeaseOutPrepaidLD
--@LeaseOutPostinvoicedLD
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
[Direction],[AccountTypeId]) VALUES
(0,51,+1,	@CurrentTradeReceivables), -- @CurrentAccruedIncome
(1,51,-1,	@RevenueFromRenderingOfServices);
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
--52:LeaseOutPrepaid
INSERT @LineDefinitions([Index],[ViewDefaultsToForm],
[Code],				[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2],
[Description]) VALUES (
52,0,N'LeaseOutPrepaid',	N'Rent/Sub',	N'تأجير',			N'Rents/Subs',	N'تأجيرات',
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
		[Time21]			= DATEADD(DAY, -1, dbo.fn_DateAdd([UnitId1],[Quantity1],[Time11])),
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 52;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId]) VALUES
(0,52,+1,	@CurrentTradeReceivables), -- @CurrentAccruedIncome
(1,52,-1,	@RevenueFromRenderingOfServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],				[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader],
																			[IsVisibleInTemplate]) VALUES
(0,52,	N'ContractId',		0,	N'Customer',		N'الزبون',				1,4,0,1),
(1,52,	N'CenterId',		1,	N'Profit Center',	N'مركز الربح',			1,4,0,1),
(2,52,	N'ResourceId',		1,	N'Service',			N'الخدمة',				1,4,0,1),
(3,52,	N'Quantity',		1,	N'Duration',		N'الفترة',				1,4,1,1),
(4,52,	N'UnitId',			1,	N'',				N'',					1,4,1,1),
(5,52,	N'Time1',			1,	N'From',			N'ابتداء من',			3,4,1,0),
(6,52,	N'Time2',			1,	N'Till',			N'حتى',					3,0,0,0),
(7,52,	N'CurrencyId',		0,	N'Currency',		N'العملة',				1,4,0,1),
(8,52,	N'MonetaryValue',	0,	N'Due Excl. VAT',	N'المطالبة بدون ق.م',	1,4,0,1),
(9,52,	N'CenterId',		0,	N'Inv. Ctr',		N'مركز الاستثمار',		4,4,1,1);

--91:PPEDepreciation
INSERT @LineDefinitions([Index],
[ViewDefaultsToForm],[Code],			[TitleSingular],		[TitleSingular2],		[TitlePlural],				[TitlePlural2],
[Description]) VALUES (
91,0,N'PPEDepreciation',				N'Asset Depreciation',	N'إهلاك أصل',			N'Assets Depreciation',		N'إهلاكات أصول',
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
WHERE [Index] = 91;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],		[EntryTypeId]) VALUES
(0,91,+1,	@DepreciationExpense,	NULL),
(1,91,-1,	@PropertyPlantAndEquipment,	@DepreciationPropertyPlantAndEquipment);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[Label2],		[RequiredState],
																	[ReadOnlyState],
																	[InheritsFromHeader]) VALUES
(0,91,	N'ResourceId',			1,	N'Asset',		N'الأصل',		1,4,0),
(1,91,	N'Quantity',			1,	N'Usage',		N'الاستخدام',	1,4,1),
(2,91,	N'UnitId',				1,	N'',			N'',			1,4,1),
(3,91,	N'CenterId',			0,	N'Cost Ctr',	N'مركز التكلفة',1,4,0),
(4,91,	N'EntryTypeId',			0,	N'Purpose',		N'الغرض',		1,4,0),
(5,91,	N'Time1',				1,	N'From',		N'ابتداء من',	1,4,1),
(6,91,	N'Time2',				1,	N'Till',		N'حتى',			1,0,1),
(7,91,	N'MonetaryValue',		1,	N'Depreciation',N'الإهلاك',		1,0,0);

DECLARE @Translations TABLE (
	[Word] NVARCHAR (50),
	[Lang] NVARCHAR (5),
	PRIMARY KEY ([Word], [Lang]),
	[Translated] NVARCHAR (50)
)
INSERT INTO @Translations 
([Word],				[Lang], [Translated]) VALUES
(N'Adjustment',			N'ar',	N'تسوية'),
(N'Adjustments',		N'ar',	N'تسويات'),
(N'Other Payment',		N'ar',	N'دفعية أخرى'),
(N'Other Payments',		N'ar',	N'دفعيات أخرى'),
(N'Transfer/Exchange',	N'ar',	N'تحويل\صرف'),		
(N'Transfers/Exchanges',N'ar',	N'تحويلات\صرف'),
(N'Duplicate Line',		N'ar',	N'تسوية'),
(N'Incorrect Analysis', N'ar',	N'تسوية'),
(N'Other reasons',		N'ar',	N'تسوية'),
(N'Payment to Supplier',N'ar',	N'دفعية لمورد'),
(N'Payments to Suppliers',N'ar',N'دفعيات لموردين'),
(N'Memo',				N'ar',	N'البيان'),
(N'Supplier',			N'ar',	N'المورد'),
(N'Employee',			N'ar',	N'الموظف'),
(N'Beneficiary',		N'ar',	N'المستفيد'),
(N'Due Currency',		N'ar',	N'عملة الاستحقاق'),
(N'Total Due',			N'ar',	N'جملة الاستحقاق'),
(N'From Account',		N'ar',	N'من حساب'),
(N'To Account',			N'ar',	N'إلى حساب'),
(N'From Currency',		N'ar',	N'من عملة'),
(N'To Currency',		N'ar',	N'إلى عملة'),
(N'From Amount',		N'ar',	N'من مبلغ'),
(N'To Amount',			N'ar',	N'إلى مبلغ'),
(N'Due Amount',			N'ar',	N'القسط الحالي'),
(N'Pay Currency',		N'ar',	N'عملة الدفع'),
(N'Pay Amount',			N'ar',	N'المبلغ المدفوع'),
(N'Bank/Cashier',		N'ar',	N'البنك\الخزنة'),
(N'Check/Receipt #',	N'ar',	N'رقم الشيك\الإيصال'),
(N'Check Date',			N'ar',	N'تاريخ الشيك'),
(N'Inv. Ctr',			N'ar',	N'مركز الاستثمار');
--(0,9,-3,	N'Insufficient Balance',N'الرصيد غير كاف'),
--(1,9,-3,	N'Other reasons',		N'أسباب أخرى');
--N'Payment Employee Benefit',N'دفعية لصالح موظف',	

DECLARE @Lang2 NVARCHAR (5), @Lang3 NVARCHAR (5);
SELECT @Lang2 = SecondaryLanguageId, @Lang3 = TernaryLanguageId FROM dbo.Settings

UPDATE LD
SET LD.[TitleSingular2] = T.[Translated]
FROM @LineDefinitions LD JOIN @Translations T ON LD.[TitleSingular] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LD
SET	LD.[TitlePlural2] = T.[Translated]
FROM @LineDefinitions LD JOIN @Translations T ON LD.[TitlePlural] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDSR
SET LDSR.[Name2] = T.[Translated]
FROM @LineDefinitionStateReasons LDSR JOIN @Translations T ON LDSR.[Name] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDC
SET	LDC.[Label2] = T.[Translated]
FROM @LineDefinitionColumns LDC JOIN @Translations T ON LDC.[Label] = T.[Word] WHERE T.[Lang] = @Lang2
ENOUGH_LD:
EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionVariants = @LineDefinitionVariants,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @PaymentToSupplierCreditPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToSupplierCreditPurchase');
DECLARE @PaymentToSupplierCashPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToSupplierCashPurchase');
DECLARE @PrepaymentToSupplierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PrepaymentToSupplier');
DECLARE @PaymentToSupplierAccrualLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToSupplierAccrual');
DECLARE @PaymentToEmployeeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToEmployee');
DECLARE @PaymentToOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToOther');
DECLARE @CashTransferExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');
DECLARE @StockReceiptCreditPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptCreditPurchase');
DECLARE @StockReceiptCashPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptCashPurchase');
DECLARE @StockReceiptPrepaidLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptPrepaid');
DECLARE @StockReceiptPostInvoicedLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptPostInvoiced');
DECLARE @ConsumableServiceReceiptCreditPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptCreditPurchase');
DECLARE @ConsumableServiceReceiptCashPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptCashPurchase');
DECLARE @ConsumableServiceReceiptPrepaidLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptPrepaid');
DECLARE @ConsumableServiceReceiptPostInvoicedLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptPostInvoiced');
DECLARE @LeaseInPrepaidLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'LeaseInPrepaid');
DECLARE @LeaseInPostinvoicedLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'LeaseInPostinvoiced');

DECLARE @PaymentFromCustomerCreditSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromCustomerCreditSale');
DECLARE @PaymentFromCustomerCashSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromCustomerCashSale');
DECLARE @PrepaymentFromCustomerLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PrepaymentFromCustomer');
DECLARE @PaymentFromCustomerAccrualLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromCustomerAccrual');
DECLARE @PaymentFromEmployeeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromEmployee');
DECLARE @PaymentFromOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromOther');

DECLARE @StockIssueCreditSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockIssueCreditSale');
DECLARE @StockIssueCashSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockIssueCashSale');
DECLARE @StockIssuePrepaidLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockIssuePrepaid');
DECLARE @StockIssuePostInvoicedLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockIssuePostInvoiced');
DECLARE @ServiceIssueCreditSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceIssueCreditSale');
DECLARE @ServiceIssueCashSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceIssueCashSale');
DECLARE @ServiceIssuePrepaidLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceIssuePrepaid');
DECLARE @ServiceIssuePostInvoicedLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceIssuePostInvoiced');
DECLARE @LeaseOutPrepaidLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'LeaseOutPrepaid');
DECLARE @LeaseOutPostinvoicedLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'LeaseOutPostinvoiced');


/*
61-69: employees payroll/
71-79: machines
*/
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Line Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;