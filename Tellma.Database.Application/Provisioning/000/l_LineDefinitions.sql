-- Suppliers(inv-gs,cash | inv-cash-gs | inv-cash,gs | gs,inv-cash): CreditPurchase,	Purchase,	Purchase,	Purchase
-- Customers(inv-gs,cash | inv-cash-gs | inv-Cash,gs | gs,inv-cash): CreditSale,		Sale,		Sale,		Sale

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
INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(0, N'ManualLine', N'Making any adjustment', N'Adjustment', N'Adjustments', 0, 0),
(1, N'PaymentToSupplierCreditPurchase', N'Payment done some time after having received Goods and services with invoice. ', N'Payment (Credit Purchase)', N'Payments (Credit Purchases)', 0, 1),
(2, N'PaymentToSupplierPurchase', N'Invoice received with payment (which could be before, during, or after receipt of good or service)', N'Payment (Purchase)', N'Payments (Purchases)', 0, 1),
(3, N'PaymentToEmployee', N'Payment to or on behalf of employee', N'Payment (Employee Benefit)', N'Payments Employee Benefits', 0, 1),
(9, N'PaymentToOther', N'Payment to other than employee or supplier', N'Payment (Other)', N'Other Payments', 0, 1),
(10, N'CashTransferExchange', N'Cash transfer between two accounts and/or exchange between two different currencies', N'Transfer/Exchange', N'Transfers/Exchanges', 0, 1),
(11, N'StockReceiptCreditPurchase', N'Receiving goods with invoice. Payment is to be done later', N'Stock Receipt (On Account)', N'Stocks Receipts (Credit Purchases)', 0, 0),
(12, N'StockReceiptPurchase', N'Receiving goods without invoice. Invoice is received with Payment', N'Stock Receipt (in Cash)', N'Stocks Receipts (Purchases)', 0, 0),
(13, N'ConsumableServiceReceiptCreditPurchase', N'Receiving services/consumables with invoice. Payment is to be done later', N'C/S Receipt (On Account)', N'C/S Receipts (Credit Purchases)', 0, 0),
(14, N'ConsumableServiceReceiptPurchase', N'Receiving services/consumables without invoice. Invoice is received with Payment', N'C/S Receipt (Cash)', N'C/S Receipts (Purchases)', 0, 0),
(21, N'PaymentFromCustomerCreditSale', N'Payment collected some time after having issuing Goods and services with invoice. ', N'Payment (Credit Sale)', N'Payments (Credit Sale)', 0, 1),
(22, N'PaymentFromCustomerSale', N'Invoice issued  with payment collection (which could be before, during, or after delivery of good or rendering of service)', N'Payment (Sale)', N'Customer Payments (Sale)', 0, 1),
(29, N'PaymentFromOther', N'Payment collected from other than customer', N'Payment from Others', N'Payments from Others', 0, 1),
(33, N'ServiceIssueCreditSale', N'To recgonize revenues from delivering services on credit', N'Service (Credit Sale)', N'Services (Credit Sale)', 0, 1),
(34, N'ServiceIssueSale', N'To recgonize revenues from delivering services', N'Service (Sale)', N'Services (Sale)', 0, 1),
(91, N'PPEDepreciation', N'For depreciation of assets that are time based, and using the number of days as criteria', N'Asset Depreciation', N'Assets Depreciation', 0, 1);

--0:ManualLine 
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction]) VALUES (0,0,+1);
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
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId0]			= [CenterId1]
'
WHERE [Index] = 1;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,1,+1,	NULL),
(1,1,-1,	@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,1,		@TradeAndOtherCurrentPayablesToTradeSuppliers),
(0,1,1,		@CashAndCashEquivalents); -- ContractDefinition Limits the Contract. But Contract+Currency specifies the account
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,1,		@suppliersCD),
--(1,0,1,	@RelatedSuppliersCD),
(0,1,1,		@petty_cash_fundsCD),
(1,1,1,		@bank_accountsCD);
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
[RuleType],				[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,1,N'Public',		NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,1,N'ByRole',		@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,1,N'ByContract',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,1,N'ByRole',		@ComptrollerRL,		NULL,			NULL);
--2:PaymentToSupplierPurchase (inv-cash-gs), (inv-cash,gs), (gs,inv-cash)
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
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,2,+1,	NULL),
(1,2,+1,	NULL),
(2,2,-1,	@PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,2,		@CashPurchaseDocumentControlExtension),
(1,0,2,		@CurrentPrepayments),
(2,0,2,		@AccrualsClassifiedAsCurrent),
(0,1,2,		@CurrentValueAddedTaxReceivables),
(0,2,2,		@CashOnHand),
(1,2,2,		@BalancesWithBanks);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,2,		@suppliersCD),
(0,2,2,		@petty_cash_fundsCD),
(1,2,2,		@bank_accountsCD);
INSERT INTO @LineDefinitionEntryNotedContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[NotedContractDefinitionId]) VALUES
(0,1,2,		@suppliersCD);
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
(0,1,2,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,2,N'ByContract',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,2,N'ByRole',	@ComptrollerRL,		NULL,			NULL);
--3:PaymentToEmployee (used in a payroll voucher)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]	= [CurrencyId0],
		[NotedContractId1] = [ContractId0],
		[MonetaryValue1]= [MonetaryValue0],
		[CenterId0]		= [CenterId1]
'
WHERE [Index] = 3;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
-- We might better add ContractDefinitionId and limit it to employees for this case
[Direction],[EntryTypeId]) VALUES
(0,3,+1,	NULL),
(1,3,-1,	@PaymentsToAndOnBehalfOfEmployees);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,3,		@OtherCurrentPayables),
(0,1,3,		@CashOnHand),
(1,1,3,		@BalancesWithBanks);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,3,		@employeesCD),
(0,1,3,		@petty_cash_fundsCD),
(1,1,3,		@bank_accountsCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,3,	N'Memo',				0,	N'Memo',			1,4,1),
(1,3,	N'ContractId',			0,	N'Employee',		3,4,1),
(2,3,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,3,	N'MonetaryValue',		0,	N'Amount',			1,2,0),
(4,3,	N'ContractId',			1,	N'Bank/Cashier',	3,4,0),
(5,3,	N'ExternalReference',	1,	N'Check/Receipt #',	3,4,0),
(6,3,	N'NotedDate',			1,	N'Check Date',		5,4,0),
(7,3,	N'CenterId',			1,	N'Inv. Ctr',		4,4,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,3,+1),
(1,3,+2),
(2,3,+3),
(3,3,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,3,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,3,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,3,N'ByContract',	NULL,				2,				NULL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,3,N'ByRole',	@ComptrollerRL,		NULL,			NULL);
--9:PaymentToOther
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
[Direction]) VALUES
(0,9,	+1),
(1,9,	-1);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,9,		@OtherDocumentControlExtension),
(0,1,9,		@CashOnHand),
(1,1,9,		@BalancesWithBanks);
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
(0,1,9,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,9,N'ByContract',	NULL,				1,				NULL), -- cash/check custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,9,N'ByRole',	@ComptrollerRL,	NULL,			NULL);
--10:CashTransferExchange
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
[Direction],[EntryTypeId]) VALUES
(0,10,+1,	@InternalCashTransferExtension),
(1,10,-1,	@InternalCashTransferExtension),
(2,10,+1,	NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,10,		@CashOnHand),
(1,0,10,		@BalancesWithBanks),
(0,1,10,		@CashOnHand),
(1,1,10,		@BalancesWithBanks),
(0,2,10,		@GainLossOnForeignExchangeExtension);
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
(0,1,10,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,10,N'ByContract',	NULL,				0,				@ComptrollerRL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(1,2,10,N'ByContract',	NULL,				1,				@ComptrollerRL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,10,N'ByRole',	@ComptrollerRL,		NULL,		NULL);
--11:StockReceiptCreditPurchase (inv-gs,cash) [rarely used in ET]
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 11;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,11,+1,	@ReceiptsReturnsThroughPurchaseExtension), -- @Inventories
(1,11,-1,	NULL), -- @CurrentValueAddedTaxReceivables
(2,11,-1,	NULL); -- @TradeAndOtherCurrentPayablesToTradeSuppliers
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,11,	@RawMaterials),
(1,0,11,	@ProductionSupplies),
(2,0,11,	@Merchandise),
(3,0,11,	@CurrentFoodAndBeverage),
(4,0,11,	@CurrentAgriculturalProduce),
(5,0,11,	@FinishedGoods),
(6,0,11,	@CurrentPackagingAndStorageMaterials),
(7,0,11,	@SpareParts),
(8,0,11,	@CurrentFuel),
(9,0,11,	@PropertyIntendedForSaleInOrdinaryCourseOfBusiness),
(10,0,11,	@OtherInventories),
(0,1,11,	@CurrentValueAddedTaxReceivables),
(0,2,11,	@TradeAndOtherCurrentPayablesToTradeSuppliers);
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
--12:StockReceiptPurchase (inv-cash-gs),  (inv-cash,gs), (gs,inv-cash)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 12;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,12,+1,	@ReceiptsReturnsThroughPurchaseExtension), -- @Inventories
(1,12,-1,	NULL); -- @CashPurchaseDocumentControlExtension, @CurrentPrepayments, @AccrualsClassifiedAsCurrent
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,12,	@RawMaterials),
(1,0,12,	@ProductionSupplies),
(2,0,12,	@Merchandise),
(3,0,12,	@CurrentFoodAndBeverage),
(4,0,12,	@CurrentAgriculturalProduce),
(5,0,12,	@FinishedGoods),
(6,0,12,	@CurrentPackagingAndStorageMaterials),
(7,0,12,	@SpareParts),
(8,0,12,	@CurrentFuel),
(9,0,12,	@PropertyIntendedForSaleInOrdinaryCourseOfBusiness),
(10,0,12,	@OtherInventories),
(0,1,12,	@CashPurchaseDocumentControlExtension),
(1,1,12,	@CurrentPrepayments),
(2,1,12,	@AccrualsClassifiedAsCurrent);
--13:ConsumableServiceReceiptCreditPurchase (inv-gs,cash) [rarely used, applies to travel expenses]
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 13;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,13,+1,	NULL), -- @ExpenseByNature
(1,13,-1,	NULL), -- @CurrentValueAddedTaxReceivables
(2,13,-1,	NULL); -- @TradeAndOtherCurrentPayablesToTradeSuppliers
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,13,	@RawMaterialsAndConsumablesUsed),
(1,0,13,	@CostOfMerchandiseSold),
(2,0,13,	@InsuranceExpense),
(3,0,13,	@ProfessionalFeesExpense),
(4,0,13,	@TransportationExpense),
(5,0,13,	@BankAndSimilarCharges),
(6,0,13,	@TravelExpense),
(7,0,13,	@CommunicationExpense),
(8,0,13,	@UtilitiesExpense),
(9,0,13,	@AdvertisingExpense),
(10,0,13,	@WagesAndSalaries),
(11,0,13,	@SocialSecurityContributions),
(12,0,13,	@OtherShorttermEmployeeBenefits),
(13,0,13,	@EmployeeBonusExtension),
(14,0,13,	@PostemploymentBenefitExpenseDefinedContributionPlans),
(15,0,13,	@PostemploymentBenefitExpenseDefinedBenefitPlans),
(16,0,13,	@TerminationBenefitsExpense),
(17,0,13,	@OtherLongtermBenefits),
(18,0,13,	@OtherEmployeeExpense),
(19,0,13,	@OtherExpenseByNature),
(0,1,13,	@CurrentValueAddedTaxReceivables),
(0,2,13,	@TradeAndOtherCurrentPayablesToTradeSuppliers);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,13,	N'Memo',				0,	N'Memo',			1,2,1),
(1,13,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,13,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,13,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,13,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,13,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
--14:ConsumableServiceReceiptPurchase (inv-cash-gs) (inv-cash,gs) (gs,inv-cash), can used for LeaseIn as well...
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1] = [CurrencyId0],
		[MonetaryValue1] = [MonetaryValue0]
'
WHERE [Index] = 14;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,14,+1,	NULL), -- @ExpenseByNature
(1,14,-1,	NULL); -- @CashPurchaseDocumentControlExtension, @CurrentPrepayments @AccrualsClassifiedAsCurrent
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,14,	@RawMaterialsAndConsumablesUsed),
(1,0,14,	@CostOfMerchandiseSold),
(2,0,14,	@InsuranceExpense),
(3,0,14,	@ProfessionalFeesExpense),
(4,0,14,	@TransportationExpense),
(5,0,14,	@BankAndSimilarCharges),
(6,0,14,	@TravelExpense),
(7,0,14,	@CommunicationExpense),
(8,0,14,	@UtilitiesExpense),
(9,0,14,	@AdvertisingExpense),
(10,0,14,	@WagesAndSalaries),
(11,0,14,	@SocialSecurityContributions),
(12,0,14,	@OtherShorttermEmployeeBenefits),
(13,0,14,	@EmployeeBonusExtension),
(14,0,14,	@PostemploymentBenefitExpenseDefinedContributionPlans),
(15,0,14,	@PostemploymentBenefitExpenseDefinedBenefitPlans),
(16,0,14,	@TerminationBenefitsExpense),
(17,0,14,	@OtherLongtermBenefits),
(18,0,14,	@OtherEmployeeExpense),
(19,0,14,	@OtherExpenseByNature),
(0,1,14,	@CashPurchaseDocumentControlExtension),
(1,1,14,	@CurrentPrepayments),
(2,1,14,	@AccrualsClassifiedAsCurrent);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
-- Budget determines: Account, Entry Type, Center, Resource, Contract
(0,14,	N'Memo',				0,	N'Memo',			1,2,1),
(1,14,	N'BudgetId',			0,	N'Budget',			3,4,0),
(2,14,	N'MonetaryValue',		0,	N'Price (b/f VAT)',	1,2,0),
(3,14,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(4,14,	N'CenterId',			0,	N'Cost. Ctr',		4,4,1),
(5,14,	N'NotedContractId',		0,	N'Supplier',		3,3,1);
 --21:PaymentFromCustomerCreditSale (inv-gs,cash)
-- credit sale: Dr. Cash, Cr. A/R
-- cash sale: Dr. Cash, Cr. Cash sale Doc control
-- prepayment: Dr. Cash, Cr. VAT Payable, Cr. Unearned Revenues
-- post pay accrual: Dr. Cash, Cr. VAT Payable, Cr. Accrued income
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[MonetaryValue1]	= [MonetaryValue0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1])
'
WHERE [Index] = 21;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,21,+1,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices), -- @CashAndCashEquivalents
(1,21,-1,	NULL); -- @CurrentTradeReceivables
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,21,	@CashOnHand),
(1,0,21,	@BalancesWithBanks),
(0,1,21,	@CurrentTradeReceivables);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,21,	N'Memo',				1,	N'Memo',			1,5,1),
(1,21,	N'ContractId',			1,	N'Customer',		1,4,1),
(2,21,	N'CurrencyId',			0,	N'Currency',		2,0,0),
(3,21,	N'MonetaryValue',		0,	N'Amount',			2,0,0),
(4,21,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(5,21,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--22:PaymentFromCustomerSale (inv-cash-gs) (inv-Cash,gs) (gs,inv-cash)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CenterId1]			= [CenterId0],
		[CenterId2]			= [CenterId0],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [NotedContractId1]),
		[ContractId2]		= [ContractId1],
		[NotedAmount1]		= ISNULL([MonetaryValue2],0)
'
WHERE [Index] = 22;
INSERT INTO @LineDefinitionEntries([Index],[HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,22,+1,	@ReceiptsFromSalesOfGoodsAndRenderingOfServices), -- @CashAndCashEquivalents
(1,22,-1,	NULL), -- @CurrentValueAddedTaxPayables
(2,22,-1,	NULL); -- @CashSaleDocumentControlExtension, @DeferredIncomeClassifiedAsCurrent, @CurrentAccruedIncome
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,22,	@CashOnHand),
(1,0,22,	@BalancesWithBanks),
(0,1,22,	@CurrentValueAddedTaxPayables),
(0,2,22,	@CashSaleDocumentControlExtension),
(1,2,22,	@DeferredIncomeClassifiedAsCurrent),
(2,2,22,	@CurrentAccruedIncome);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,22,	N'Memo',				1,	N'Memo',			1,5,1),
(1,22,	N'NotedContractId',		1,	N'Customer',		1,4,1),
(2,22,	N'CurrencyId',			2,	N'Contract Currency',1,2,1),
(3,22,	N'MonetaryValue',		2,	N'Price Excl. VAT',	1,2,0),
(4,22,	N'MonetaryValue',		1,	N'VAT',				1,2,0),
(5,22,	N'NotedAmount',			0,	N'Total',			2,0,0),
(6,22,	N'NotedDate',			2,	N'Due Date',		3,4,0),
(7,22,	N'NotedDate',			1,	N'Payment Date',	3,5,0),
(8,22,	N'ContractId',			0,	N'Bank/Cashier',	3,4,1),
(9,22,	N'CurrencyId',			0,	N'Rcvd. Currency',	3,4,0),
(10,22,	N'MonetaryValue',		0,	N'Rcvd. Amount',	3,4,0),
(11,22,	N'ExternalReference',	1,	N'Invoice #',		3,5,0),
(22,22,	N'CenterId',			0,	N'Inv. Ctr',		4,4,1);
--29:PaymentFromOther, 27:RefundFromSupplier, 28:PaymentFromEmployee
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
WHERE [Index] = 29;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction]) VALUES
(0,29,+1),-- @CashAndCashEquivalents
(1,29,-1); -- @OtherDocumentControlExtension
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,29,	@CashOnHand), -- 
(1,0,29,	@BalancesWithBanks),
(0,1,29,	@OtherDocumentControlExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[Label2],		[RequiredState],
																			[ReadOnlyState],
																			[InheritsFromHeader]) VALUES
(0,29,	N'CurrencyId',			0,	N'Currency',		N'العملة',			1,2,1),
(1,29,	N'MonetaryValue',		0,	N'Amount',			N'المبلغ',			1,2,0),
(2,29,	N'NotedAgentName',		0,	N'Received from',	N'مستلم من',		3,4,0),
(3,29,	N'ContractId',			0,	N'Bank/Cashier',	N'البنك/الخزنة',	3,4,1),
(4,29,	N'ExternalReference',	0,	N'Check/Receipt #',	N'رقم الشيك/الإيصال',5,4,0),
(5,29,	N'NotedDate',			0,	N'Check Date',		N'تاريخ الشيك',		5,4,0),
(6,29,	N'CenterId',			0,	N'Invest. Ctr',		N'مركز الاستثمار',	4,4,1),
(7,29,	N'EntryTypeId',			0,	N'Purpose',			N'الغرض',			4,4,0),
(8,29,	N'Memo',				0,	N'Memo',			N'البيان',			1,2,1);
--33:Service Issue Credit Sale (inv-gs,cash) --31:Stock Issue Credit Sale --32:Stock Issue Sale
UPDATE @LineDefinitions
SET [Script] = N'
	--SET NOCOUNT ON
	--DECLARE @ProcessedWideLines WideLineList;

	--INSERT INTO @ProcessedWideLines
	--SELECT * FROM @WideLines;
	-----
	UPDATE @ProcessedWideLines
	SET
		[MonetaryValue0]	= ISNULL([MonetaryValue1],0) + ISNULL([MonetaryValue2],0),
		[CurrencyId2]		= [CurrencyId0],
		[CurrencyId1]		= [CurrencyId0],
		[NotedContractId1]	= [ContractId0],
		[NotedContractId2]	= [ContractId0]
		-----
	--SELECT * FROM @ProcessedWideLines;'
WHERE [Index] = 33;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction]) VALUES
(0,33,+1), -- @CashSaleDocumentControlExtension, @DeferredIncomeClassifiedAsCurrent, @CurrentAccruedIncome
(1,33,-1), -- @CurrentValueAddedTaxPayables
(2,33,-1); -- @RevenueFromRenderingOfServices
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,33,	@CashSaleDocumentControlExtension),
(1,0,33,	@DeferredIncomeClassifiedAsCurrent),
(2,0,33,	@CurrentAccruedIncome),
(0,1,33,	@CurrentValueAddedTaxPayables),
(0,2,33,	@RevenueFromRenderingOfServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,33,	N'ContractId',		0,	N'Customer',		1,4,1),
(1,33,	N'CenterId',		2,	N'Profit Center',	1,4,0),
(2,33,	N'ResourceId',		2,	N'Service',			1,4,0),
(3,33,	N'Quantity',		2,	N'Quantity',		1,3,0),
(4,33,	N'UnitId',			2,	N'',				1,3,0),
(5,33,	N'Time1',			2,	N'From',			3,3,1),
(6,33,	N'Time2',			2,	N'Till',			3,3,0),
(7,33,	N'CurrencyId',		0,	N'Currency',		1,4,1),
(8,33,	N'MonetaryValue',	2,	N'Price Excl. VAT',	1,4,0),
(9,33,	N'MonetaryValue',	1,	N'VAT',				1,4,0),
(10,33,	N'MonetaryValue',	0,	N'Price Incl. VAT',	1,0,0),
(11,33,	N'CenterId',		0,	N'Segment',			4,4,1);
--34:Service Issue Sale,  (inv-cash-gs) (inv-Cash,gs) (gs,inv-cash)
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
WHERE [Index] = 34;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction]) VALUES
(0,34,+1), -- @CurrentTradeReceivables
(1,34,-1); -- @RevenueFromRenderingOfServices
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,34,	@CashSaleDocumentControlExtension),
(1,0,34,	@DeferredIncomeClassifiedAsCurrent),
(2,0,34,	@CurrentAccruedIncome),
(0,1,34,	@RevenueFromRenderingOfServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,34,	N'ContractId',		0,	N'Customer',		1,4,1),
(1,34,	N'CenterId',		1,	N'Profit Center',	1,4,0),
(2,34,	N'ResourceId',		1,	N'Service',			1,4,0),
(3,34,	N'Quantity',		1,	N'Quantity',		1,4,0),
(4,34,	N'UnitId',			1,	N'',				1,4,0),
(5,34,	N'Time1',			1,	N'From',			3,4,1),
(6,34,	N'Time2',			1,	N'Till',			3,0,0),
(7,34,	N'CurrencyId',		0,	N'Currency',		1,4,1),
(8,34,	N'MonetaryValue',	0,	N'Price Excl. VAT',	1,4,0),
(9,34,	N'CenterId',		0,	N'Segment',			4,4,1);
--91:PPEDepreciation
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
[Direction],[EntryTypeId]) VALUES
(0,91,+1,	NULL), -- @DepreciationExpense
(1,91,-1,	@DepreciationPropertyPlantAndEquipment); -- @PropertyPlantAndEquipment
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,91,	@DepreciationExpense),
(1,1,91,	@Buildings),
(2,1,91,	@Machinery),
(3,1,91,	@Vehicles),
(4,1,91,	@FixturesAndFittings),
(5,1,91,	@OfficeEquipment),
(6,1,91,	@TangibleExplorationAndEvaluationAssets),
(7,1,91,	@MiningAssets),
(8,1,91,	@OilAndGasAssets),
--(9,1,91,	@ConstructionInProgress),
(10,1,91,	@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel),
(11,1,91,	@OtherPropertyPlantAndEquipment);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,91,	N'ResourceId',			1,	N'Asset',		1,4,0),
(1,91,	N'Quantity',			1,	N'Usage',		1,4,1),
(2,91,	N'UnitId',				1,	N'',			1,4,1),
(3,91,	N'CenterId',			0,	N'Cost Ctr',	1,4,0),
(4,91,	N'EntryTypeId',			0,	N'Purpose',		1,4,0),
(5,91,	N'Time1',				1,	N'From',		1,4,1),
(6,91,	N'Time2',				1,	N'Till',		1,0,1),
(7,91,	N'MonetaryValue',		1,	N'Depreciation',1,0,0);
/*
DECLARE @TranslationsLD TABLE (
	[Word] NVARCHAR (50),
	[Lang] NVARCHAR (5),
	PRIMARY KEY ([Word], [Lang]),
	[Translated] NVARCHAR (50)
)
INSERT INTO @TranslationsLD 
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
FROM @LineDefinitions LD JOIN @TranslationsLD T ON LD.[TitleSingular] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LD
SET	LD.[TitlePlural2] = T.[Translated]
FROM @LineDefinitions LD JOIN @TranslationsLD T ON LD.[TitlePlural] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDSR
SET LDSR.[Name2] = T.[Translated]
FROM @LineDefinitionStateReasons LDSR JOIN @TranslationsLD T ON LDSR.[Name] = T.[Word] WHERE T.[Lang] = @Lang2

UPDATE LDC
SET	LDC.[Label2] = T.[Translated]
FROM @LineDefinitionColumns LDC JOIN @TranslationsLD T ON LDC.[Label] = T.[Word] WHERE T.[Lang] = @Lang2
*/

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryAccountTypes = @LineDefinitionEntryAccountTypes,
	@LineDefinitionEntryContractDefinitions = @LineDefinitionEntryContractDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedContractDefinitions = @LineDefinitionEntryNotedContractDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

-- Declarations
DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @PaymentToSupplierCreditPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToSupplierCreditPurchase');
DECLARE @PaymentToSupplierPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToSupplierPurchase');
DECLARE @PaymentToEmployeeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToEmployee');
DECLARE @PaymentToOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentToOther');
DECLARE @CashTransferExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');
DECLARE @StockReceiptCreditPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptCreditPurchase');
DECLARE @StockReceiptPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptPurchase');
DECLARE @ConsumableServiceReceiptCreditPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptCreditPurchase');
DECLARE @ConsumableServiceReceiptPurchaseLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptPurchase');
DECLARE @PaymentFromCustomerCreditSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromCustomerCreditSale');
DECLARE @PaymentFromCustomerSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromCustomerSale');
DECLARE @PaymentFromOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PaymentFromOther');
DECLARE @ServiceIssueCreditSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceIssueCreditSale');
DECLARE @ServiceIssueSaleLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceIssueSale');
DECLARE @PPEDepreciationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEDepreciation');

/*
61-69: employees payroll/
71-79: machines
*/