INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(0, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0),
(100, N'CashPaymentToOther', N'cash payment to other than suppliers, customers, and employees', N'Payment to Other', N'Payments to Others', 0, 1),
(104, N'CashTransferExchange', N'cash transfer exchange', N'Cash Transfer', N'Cash Transfers', 0, 1),
(110, N'DepositCashToBank', N'deposit cash in bank', N'Cash Deposit', N'Cash Deposits', 0, 1),
(111, N'DepositCheckToBank', N'deposit checks in bank', N'Check Deposit', N'Check Deposits', 0, 0),
(120, N'CashReceiptFromOther', N'cash receipt by cashier or bank from other than customers, suppliers or employees', N'Cash Payment', N'Cash Payments', 0, 1),
(121, N'CheckReceiptFromOtherInCashier', N'check receipt by cashier from other than customers, suppliers or employees', N'Check Payment', N'Check Payments', 0, 1),
(300, N'CashPaymentToTradePayable', N'issuing Payment to supplier/lessor/..', N'Payment', N'Payments', 0, 1),
(301, N'InvoiceFromTradePayable', N'Receiving Invoice from supplier/lessor', N'Invoice', N'Invoices', 0, 1),
(302, N'StockReceiptFromTradePayable', N'Receiving goods to inventory from supplier/contractor', N'Stock', N'Stock', 0, 0),
(303, N'PPEReceiptFromTradePayable', N'Receiving property, plant and equipment from supplier/contractor', N'Fixed Asset', N'Fixed Assets', 0, 1),
(304, N'ConsumableServiceReceiptFromTradePayable', N'Receiving services/consumables from supplier/lessor/consultant, ...', N'Consumable/Service', N'Consumables/Services', 0, 1),
(305, N'RentalReceiptFromTradePayable', N'Receiving rental service from lessor', N'Rental', N'Rentals', 0, 1);
--0: ManualLine
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
--100:CashPaymentToOther
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1],
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 100;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction]) VALUES
(0,100,	+1),
(1,100,	-1);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,100,		@CashControlExtension),
(0,1,100,		@CashAndCashEquivalents);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,1,100,		@cashonhand_accountsCD), -- do we have to list them? They are simply the union of AccountTypeContractDefinitions
(1,1,100,		@bank_accountsCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,100,	N'Memo',				1,	N'Memo',				1,2,1),
(1,100,	N'CurrencyId',			1,	N'Currency',			1,2,1),
(2,100,	N'MonetaryValue',		1,	N'Pay Amount',			1,2,0),
(3,100,	N'NotedAgentName',		1,	N'Beneficiary',			3,3,0),
(4,100,	N'ContractId',			1,	N'Bank/Cashier',		3,3,0),
(5,100,	N'ExternalReference',	1,	N'Check #/Receipt #',	3,3,0),
(6,100,	N'NotedDate',			1,	N'Check Date',			5,3,0),
(7,100,	N'CenterId',			1,	N'Segment',				4,4,1),
(8,100,	N'EntryTypeId',			1,	N'Purpose',				4,4,0);
INSERT INTO @LineDefinitionStateReasons([Index],[HeaderIndex],
[State],	[Name]) VALUES
(0,100,-3,	N'Insufficient Balance'),
(1,100,-3,	N'Other reasons');
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,100,+1),
(1,100,+2),
(2,100,+3),
(3,100,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],	[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,100,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,100,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,100,N'ByContract',	NULL,				1,				NULL), -- cash/check custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,100,N'ByRole',	@ComptrollerRL,	NULL,			NULL);
--104:CashTransferExchange
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
WHERE [Index] = 104;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,104,+1,	@InternalCashTransferExtension),
(1,104,-1,	@InternalCashTransferExtension),
(2,104,+1,	NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,104,		@CashAndCashEquivalents),
(0,1,104,		@CashAndCashEquivalents),
(0,2,104,		@GainLossOnForeignExchangeExtension);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,104,		@cashonhand_accountsCD),
(1,0,104,		@bank_accountsCD),
(0,1,104,		@cashonhand_accountsCD),
(1,1,104,		@bank_accountsCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,104,	N'ContractId',			1,	N'From Account',	1,2,0),
(1,104,	N'ContractId',			0,	N'To Account',		1,2,0),
(2,104,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,104,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,104,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,104,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,104,	N'CenterId',			0,	N'Invest. Ctr',		4,4,1),
(7,104,	N'Memo',				0,	N'Memo',			1,2,1);
INSERT INTO @Workflows([Index],[LineDefinitionIndex],
[ToState]) Values
(0,104,+1),
(1,104,+2),
(2,104,+3),
(3,104,+4);
INSERT INTO @WorkflowSignatures([Index], [WorkflowIndex],[LineDefinitionIndex],
[RuleType],			[RoleId],			[RuleTypeEntryIndex], [ProxyRoleId]) VALUES
(0,0,104,N'Public',	NULL,				NULL,			NULL), -- anyone can request. At this stage, we can print the requisition
(0,1,104,N'ByRole',	@GeneralManagerRL,	NULL,			NULL), -- GM only can approve. At this state, we can print the payment order (check, LT, LC, ...)
(0,2,104,N'ByContract',	NULL,				0,				@ComptrollerRL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(1,2,104,N'ByContract',	NULL,				1,				@ComptrollerRL), -- custodian only can complete, or comptroller (convenient in case of Bank not having access)
(0,3,104,N'ByRole',	@ComptrollerRL,		NULL,		NULL);
--110:DepositCashToBank
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId1] = [CenterId0],
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1]
'
WHERE [Index] = 110;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,110,+1,	@InternalCashTransferExtension),
(1,110,-1,	@InternalCashTransferExtension);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,110,	@BalancesWithBanks),
(0,1,110,	@CashOnHand);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,110,		@bank_accountsCD),
(0,1,110,		@cashonhand_accountsCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,110,	N'ContractId',			1,	N'From Cash Account',1,2,1),
(1,110,	N'ContractId',			0,	N'To Bank Account',	1,2,1),
(2,110,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,110,	N'MonetaryValue',		1,	N'Amount',			1,3,0),
(4,110,	N'CenterId',			0,	N'Segment',			4,4,1),
(5,110,	N'Memo',				0,	N'Memo',			1,2,1);
--111:DepositCheckToBank
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.Contracts WHERE [Id] = [ContractId0]),
		[CenterId1] = [CenterId0],
		[CurrencyId0] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId1]),
		[CurrencyId1] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId1]),
		[MonetaryValue0] = (SELECT [MonetaryAmount] FROM dbo.Resources WHERE [Id] = [ResourceId1]),
		[MonetaryValue1] = (SELECT [MonetaryAmount] FROM dbo.Resources WHERE [Id] = [ResourceId1])
		-- Add the checkinfo to the bank line
'
WHERE [Index] = 111;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[EntryTypeId]) VALUES
(0,111,+1,	@InternalCashTransferExtension),
(1,111,-1,	@InternalCashTransferExtension);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,111,	@BalancesWithBanks),
(0,1,111,	@CashOnHand);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,111,		@bank_accountsCD),
(0,1,111,		@cashonhand_accountsCD);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ResourceDefinitionId]) VALUES
(0,1,111,		@ChecksReceivedRD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,111,	N'ContractId',			1,	N'From Cash Account',1,2,1),
(1,111,	N'ContractId',			0,	N'To Bank Account',	1,2,1),
(2,111,	N'ResourceId',			1,	N'Check Received',	1,2,1),
(4,111,	N'CenterId',			0,	N'Segment',			4,4,1),
(5,111,	N'Memo',				0,	N'Memo',			1,2,1);
--120:CashReceiptFromOther
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = [CurrencyId1],
		[MonetaryValue0] = [MonetaryValue1],
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 120;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction]) VALUES
(0,120,	+1),
(1,120,	-1);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,120,		@CashAndCashEquivalents),
(0,1,120,		@CashControlExtension);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,120,		@cashonhand_accountsCD),
(1,0,120,		@bank_accountsCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,120,	N'Memo',				0,	N'Memo',				1,2,1),
(1,120,	N'CurrencyId',			0,	N'Currency',			1,2,1),
(2,120,	N'MonetaryValue',		0,	N'Received Amount',		1,2,0),
(3,120,	N'NotedAgentName',		0,	N'Received from',		3,3,0),
(4,120,	N'ContractId',			0,	N'Bank/Cashier',		3,3,0),
(5,120,	N'ExternalReference',	0,	N'Receipt #',			3,3,0),
(7,120,	N'CenterId',			0,	N'Segment',				4,4,1),
(8,120,	N'EntryTypeId',			0,	N'Purpose',				4,4,0);

--121:CheckReceiptFromOtherInCashier
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[CurrencyId1] = (SELECT [CurrencyId] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[MonetaryValue0] = (SELECT [MonetaryAmount] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[MonetaryValue1] = (SELECT [MonetaryAmount] FROM dbo.Resources WHERE [Id] = [ResourceId0]),
		[CenterId0] = [CenterId1]
'
WHERE [Index] = 121;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction]) VALUES
(0,121,	+1),
(1,121,	-1);
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,121,		@CashOnHand),
(0,1,121,		@CashControlExtension);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,121,		@cashonhand_accountsCD);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ResourceDefinitionId]) VALUES
(0,0,121,		@ChecksReceivedRD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],				[RequiredState],
															[ReadOnlyState],
															[InheritsFromHeader]) VALUES
(0,121,	N'Memo',				0,	N'Memo',				1,2,1),
(1,121,	N'ResourceId',			0,	N'Check',				1,0,1),
(2,121,	N'CurrencyId',			0,	N'Currency',			1,0,1),
(3,121,	N'MonetaryValue',		0,	N'Received Amount',		1,0,0),
(4,121,	N'NotedAgentName',		0,	N'Received from',		3,3,0),
(5,121,	N'ContractId',			0,	N'Cashier',				3,3,0),
(6,121,	N'ExternalReference',	0,	N'Receipt #',			3,3,0),
(7,121,	N'CenterId',			0,	N'Segment',				4,4,1),
(8,121,	N'EntryTypeId',			0,	N'Purpose',				4,4,0);

--300:CashPaymentToTradePayable: Supplier (=> Cash Purchase Voucher),-- CashPaymentToEmployee (=> Employee Payment Voucher),-- CashPaymentToCustomer (=> Customer refund Voucher)
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
(0,300,+1,	NULL), -- @CashControlExtension
(1,300,-1,	@PaymentsToSuppliersForGoodsAndServices); -- @CashAndCashEquivalents
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,300,	@CashControlExtension),
(0,1,300,	@CashAndCashEquivalents);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,300,	@suppliersCD),
(0,1,300,	@cashonhand_accountsCD), -- do we have to list them? They are simply the union of AccountTypeContractDefinitions
(1,1,300,	@bank_accountsCD);
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
(7,1,	N'PostingDate',			1,	N'Paid On',			1,4,1),
(8,1,	N'CenterId',			1,	N'Segment',			4,4,1);
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
GOTO DONE_LD
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
(0,1,3,		@CashAndCashEquivalents);
INSERT INTO @LineDefinitionEntryContractDefinitions([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[ContractDefinitionId]) VALUES
(0,0,3,		@employeesCD),
(0,1,3,		@cashonhand_accountsCD),
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
(2,11,-1,	NULL); -- @TradingControlExtension
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
(0,2,11,	@TradingControlExtension);
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
(1,12,-1,	NULL); -- @TradingControlExtension
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
(0,1,12,	@TradingControlExtension);
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
(1,14,-1,	NULL); 
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
(0,1,14,	@TradingControlExtension);
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
(0,0,21,	@CashAndCashEquivalents),
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
(2,22,-1,	NULL); -- @TradingControlExtension
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,22,	@CashAndCashEquivalents),
(0,1,22,	@CurrentValueAddedTaxPayables),
(0,2,22,	@TradingControlExtension);
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
(0,33,+1), -- @TradingControlExtension
(1,33,-1), -- @CurrentValueAddedTaxPayables
(2,33,-1); -- @RevenueFromRenderingOfServices
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,33,	@TradingControlExtension),
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
(0,34,+1), -- @TradingControlExtension
(1,34,-1); -- @RevenueFromRenderingOfServices
INSERT INTO @LineDefinitionEntryAccountTypes([Index], [LineDefinitionEntryIndex],[LineDefinitionIndex],
			[AccountTypeId]) VALUES
(0,0,34,	@TradingControlExtension),
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
DONE_LD:
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
DECLARE @CashPaymentToOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToOther');
DECLARE @CashTransferExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');
DECLARE @DepositCashToBankLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepositCashToBank');
DECLARE @DepositCheckToBankLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepositCheckToBank');
DECLARE @CashReceiptFromOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashReceiptFromOther');
DECLARE @CheckReceiptFromOtherInCashierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CheckReceiptFromOtherInCashier');
DECLARE @CashPaymentToTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToTradePayable');
DECLARE @InvoiceFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InvoiceFromTradePayable');
DECLARE @StockReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptFromTradePayable');
DECLARE @PPEReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEReceiptFromTradePayable');
DECLARE @ConsumableServiceReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptFromTradePayable');
DECLARE @RentalReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RentalReceiptFromTradePayable');

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
