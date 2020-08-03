INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(0, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0),
(71, N'ProjectCompletionToPropertyPlantAndEquipment', N'real estate project turning into properties to use', N'For Use', N'For Use', 0, 0),
(72, N'ProjectCompletionToInventory', N'real estate project turning into properties for sale', N'For Sale', N'For Sale', 0, 0),
(73, N'ProjectCompletionToInvestmentProperty', N'real estate project turning into properties for rent', N'For Rent', N'For Rent', 0, 0),
(81, N'PPEDepreciation', N'Depreciating assets that are time based, and using the number of days as criteria', N'Depreciation', N'Assets Depreciation', 0, 0),
(82, N'IntangibleAmortization', N'', N'Amortization', N'Amortization', 0, 0),
(83, N'ExchangeVariance', N'', N'Exchange Variance', N'Exchange Variances', 0, 0),
(84, N'TradeSettlement', N'Adjusting trade payables and trade receivables balances', N'Settlement', N'Settlements', 0, 0),
(85, N'Hyperinflation', N'Adjusting according to IAS 29', N'Hyperinflation', N'Hyperinflation', 0, 0),
(91, N'CostReallocationToConstructionInProgress', N'Capitalization of a project expenditures', N'Project', N'Projects', 0, 0),
(92, N'CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment', N'Capitalization of an investment property expenditures ', N'Investment Property', N'Investment Properties', 0, 0),
(93, N'CostReallocationToCurrentInventoriesInTransit', N'Capitalization of expenditures on inventories in transit', N'Goods In Transit', N'Goods In Transit', 0, 0),
(100, N'CashTransferExchange', N'cash transfer and currency exchange', N'Transfer & Exchange', N'Cash Transfers', 0, 1),
(101, N'CashTransfer', N'cash transfer, same currency', N'Transfer', N'Transfers', 0, 1),
(102, N'CashExchange', N'currency exchange, same account', N'Exchange', N'Exchanges', 0, 1),
(110, N'DepositCashToBank', N'deposit cash in bank', N'Cash Deposit', N'Cash Deposits', 0, 1),
(111, N'DepositCheckToBank', N'deposit checks in bank', N'Check Deposit', N'Check Deposits', 0, 0),
(120, N'CashReceiptFromOtherToCashier', N'cash receipt by cashier or bank from other than customers, suppliers or employees', N'Cash Payment', N'Cash Payments', 0, 1),
(121, N'CheckReceiptFromOtherToCashier', N'check receipt by cashier from other than customers, suppliers or employees', N'Check Payment', N'Check Payments', 0, 1),
(130, N'CashPaymentToOther', N'cash payment to other than suppliers, customers, and employees', N'Payment to Other', N'Payments to Others', 0, 1),
(300, N'CashPaymentToTradePayable', N'issuing Payment to supplier/lessor/..', N'Payment', N'Payments', 0, 1),
(301, N'CashPaymentToTradePayableWithInvoice', N'issuing Payment to supplier/lessor/.. against invoice', N'Payment (Invoice)', N'Payments (Invoices)', 0, 1),
(302, N'StockReceiptFromTradePayable', N'Receiving goods to inventory from supplier/contractor', N'Stock', N'Stock', 0, 0),
(303, N'PPEReceiptFromTradePayable', N'Receiving property, plant and equipment from supplier/contractor', N'Fixed Asset', N'Fixed Assets', 0, 1),
(304, N'ConsumableServiceReceiptFromTradePayable', N'Receiving services/consumables from supplier/lessor/consultant, ...', N'Consumable - Service', N'Consumables - Services', 0, 1),
(305, N'RentalReceiptFromTradePayable', N'Receiving rental service from lessor', N'Rental', N'Rentals', 0, 1),
(306, N'WithholdingTaxFromTradePayable', N'Withholding tax from payment to supplier', N'WT', N'WT', 0, 1),
(310, N'CashPaymentFromTradePayable', N'refund', N'Refund', N'Refunds', 0, 1),
(400, N'CashReceiptFromTradeReceivable', N'Receiving cash payment from customer/lessee', N'Cash', N'Cash', 0, 1),
(401, N'CheckReceiptFromTradeReceivable', N'Receiving check payment from customer/lessee', N'check', N'Checks', 0, 1),
(402, N'InvoiceToTradeReceivable', N'Issuing invoice to customer/lessee', N'Invoice', N'Invoices', 0, 1),
(403, N'StockIssueToTradeReceivable', N'Issuing stock to customer', N'Stock', N'Stock', 0, 0),
(404, N'ServiceDeliveryToTradeReceivable', N'Delivering service to customer', N'Service', N'Services', 0, 0);
--0: ManualLine
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction], [AccountTypeId]) VALUES (0,0,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,0,	N'Account',		0,			N'Account',		4,4,0), -- together with properties
(1,0,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,0,	N'Value',		0,			N'Credit',		4,4,0),
(3,0,	N'Memo',		0,			N'Memo',		4,4,1);
--130:CashPaymentToOther
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET

		[CurrencyId1]		= [CurrencyId0],
		[CenterId1]			= COALESCE([CenterId1], [CenterId0]),
		[MonetaryValue1]	= ISNULL([MonetaryValue0], 0),
		[NotedAgentName1]	= [NotedAgentName0]
'
WHERE [Index] = 130;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,130,+1,		@CashPaymentsToOthersControlExtension), -- Item price
(1,130,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,130,	N'Memo',				1,	N'Memo',			1,4,1),
(1,130,	N'NotedAgentName',		0,	N'Beneficiary',		1,4,1),
(2,130,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,130,	N'MonetaryValue',		0,	N'Amount',			1,2,0), -- 
(8,130,	N'ExternalReference',	1,	N'Check #',			4,4,0),
(9,130,	N'CustodyId',			1,	N'Cash/Bank Acct',	4,4,0), -- Entryztype
(10,130,N'PostingDate',			0,	N'Payment Date',	1,2,1),
(11,130, N'CenterId',			0,	N'Business Unit',	1,4,1);
--300:CashPaymentToTradePayable without Invoice
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[MonetaryValue1]	= [MonetaryValue0] - ISNULL([NotedAmount0], 0),
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0])
'
WHERE [Index] = 300;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [AccountTypeId],[EntryTypeId]) VALUES
(0,300,+1,	@CashPaymentsToSuppliersControlExtension, NULL),
(1,300,-1,	@CashAndCashEquivalents, @PaymentsToSuppliersForGoodsAndServices);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,300,	N'Memo',				1,	N'Memo',			1,4,1),
(1,300,	N'NotedRelationId',		0,	N'Supplier',		3,4,1),
(2,300,	N'CurrencyId',			0,	N'Invoice Currency',1,2,1),
(3,300,	N'MonetaryValue',		0,	N'Invoice Amount',	1,2,0),
(4,300,	N'NotedAmount',			0,	N'Withholding',		1,4,0),
(5,300,	N'CustodyId',			1,	N'Safe/Bank Account',3,4,0),
(6,300,	N'ExternalReference',	1,	N'Check/Receipt #',	3,4,0),
(7,300,	N'NotedDate',			1,	N'Check/Receipt Date',4,4,0),
(8,300,	N'PostingDate',			1,	N'Paid On',			1,4,1),
(9,300,	N'CenterId',			0,	N'Business Unit',	1,4,1);
--301:CashPaymentToTradePayableWithInvoice: (basically, it is the VAT) -- assume all in same currency
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CurrencyId2]		= [CurrencyId0],
		[CurrencyId3]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[CenterId2]			= [CenterId0],
		[CenterId3]			= COALESCE([CenterId3], [CenterId0]),
		[MonetaryValue0]	= ISNULL([MonetaryValue0], 0),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue2]	= IIF(ISNUMERIC([ExternalReference2]) = 1, 0.02 * [MonetaryValue0], 0),
		[MonetaryValue3]	= ISNULL([MonetaryValue0], 0) + ISNULL([MonetaryValue1], 0) - 
								IIF(ISNUMERIC([ExternalReference2]) = 1, 0.02 * [MonetaryValue0], 0),
		[ExternalReference2]= ISNULL([ExternalReference2], N''--''),
		[NotedAmount1]		= ISNULL([MonetaryValue0], 0),
		[NotedAmount2]		= ISNULL([MonetaryValue0], 0),
		[NotedRelationId1]	= [NotedRelationId0],
		[NotedRelationId2]	= [NotedRelationId0],
		-- Entry Type may change depending on nature of items
		[EntryTypeId3]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''PaymentsToSuppliersForGoodsAndServices''),
		[NotedAgentName3]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0])
'
WHERE [Index] = 301;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,301,+1,		@GoodsAndServicesReceivedFromSuppliersControlExtensions), -- Item price
(1,301,+1,		@CurrentValueAddedTaxReceivables), -- VAT, Taxamble Amount
(2,301,-1,		@WithholdingTaxPayableExtension), -- Amount paid, Equivalent Actual amount to be paid. Noted Currency Id
(3,301,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,301,	N'Memo',				1,	N'Memo',			1,4,1),
(1,301,	N'NotedRelationId',		0,	N'Supplier',		1,4,1),
(2,301,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,301,	N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0), -- 
(4,301,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,301,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(6,301,	N'MonetaryValue',		2,	N'Amount Withheld',	4,4,0),
(7,301,	N'ExternalReference',	2,	N'WT Voucher #',	4,4,0),
(8,301,	N'ExternalReference',	3,	N'Check #',			4,4,0),
(9,301,	N'CustodyId',			3,	N'Cash/Bank Acct',	4,4,0),
(10,301,N'PostingDate',			0,	N'Payment Date',	1,2,1),
(11,301, N'CenterId',			0,	N'Business Unit',	1,4,1);
--302:StockReceiptFromTradePayable: (This is the Cash purchase version, we still need credit purchase versions)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue0]	= [MonetaryValue1],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 302;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],										[EntryTypeId]) VALUES
(0,302,+1,	@Inventories,											@ReceiptsReturnsThroughPurchaseExtension),
(1,302,-1,	@GoodsAndServicesReceivedFromSuppliersControlExtensions,NULL);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,0,302,@MerchandiseRD),
(1,0,302,@CurrentFoodAndBeverageRD),
(2,0,302,@CurrentAgriculturalProduceRD),
(3,0,302,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(4,0,302,@RawMaterialsRD),
(5,0,302,@ProductionSuppliesRD),
(6,0,302,@CurrentPackagingAndStorageMaterialsRD),
(7,0,302,@SparePartsRD),
(8,0,302,@CurrentFuelRD),
(9,0,302,@OtherInventoriesRD),
(10,0,302,@TradeMedicineRD),
(11,0,302,@TradeConstructionMaterialRD),
(12,0,302,@TradeSparePartRD),
(13,0,302,@RawGrainRD),
(14,0,302,@RawVehicleRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,302,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,302,	N'Memo',				1,	N'Memo',			1,4,1),
(1,302,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,302,	N'CustodyId',			0,	N'Warehouse',		3,4,1),
(3,302,	N'ResourceId',			0,	N'Item',			2,4,0),
(4,302,	N'Quantity',			0,	N'Qty',				2,4,0),
(5,302,	N'UnitId',				0,	N'Unit',			2,4,0),
(6,302,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(7,302,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0),
(10,302,N'PostingDate',			1,	N'Received On',		1,4,1),
(11,302,N'CenterId',			1,	N'Business Unit',	1,4,1);
--303:PPEReceiptFromTradePayable: (This is the Cash purchase version, we still need credit purchase versions)
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId2],
		[CurrencyId1]		= [CurrencyId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[CenterId1]			= COALESCE([CenterId1], [CenterId2]),
		[CustodyId1]		= [CustodyId0],
		[MonetaryValue1]	= ISNULL([MonetaryValue2],0) - ISNULL([MonetaryValue0],0),
		[ResourceId1]		= [ResourceId0],
		[Quantity0]			= 1,
		[UnitId0]			= (SELECT [Id] FROM dbo.Units WHERE Code = N''pure''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 303;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],			[EntryTypeId]) VALUES
(0,303,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(1,303,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(2,303,-1,	@GoodsAndServicesReceivedFromSuppliersControlExtensions,NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,303,@PPECustodyCD),
(0,1,303,@PPECustodyCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,303,	N'Memo',				1,	N'Memo',			1,4,1),
(1,303,	N'NotedRelationId',		2,	N'Supplier',		3,4,1),
(2,303,	N'CustodyId',			0,	N'Custody',			5,5,0),
(3,303,	N'ResourceId',			0,	N'Fixed Asset',		2,4,0),
(4,303,	N'Quantity',			1,	N'Life/Usage',		2,4,0),
(5,303,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,303,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,303,	N'MonetaryValue',		2,	N'Cost (VAT Excl.)',1,2,0),
(8,303,	N'MonetaryValue',		0,	N'Residual Value',	1,2,0),
(10,303,N'PostingDate',			1,	N'Acquired On',		1,4,1),
(11,303,N'CenterId',			2,	N'Business Unit',	1,4,1);
--306:WithholdingTaxReceivablesExtension: Do we have it separate or part of Payment line???
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue0]	= [MonetaryValue1],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 306;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],										[EntryTypeId]) VALUES
(0,306,+1,	@GoodsAndServicesReceivedFromSuppliersControlExtensions,NULL),
(1,306,-1,	@WithholdingTaxPayableExtension,NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,306,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,306,	N'Memo',				1,	N'Memo',			1,4,1),
(1,306,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,306,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,306,	N'NotedAmount',			1,	N'Amount (VAT Excl.)',3,3,0),
(4,306,	N'MonetaryValue',		1,	N'Amount Withheld',	1,2,0),
(9,306,N'ExternalReference',	1,	N'Voucher #',		1,4,1),
(10,306,N'PostingDate',			1,	N'Voucher Date',	1,4,1),
(11,306,N'CenterId',			1,	N'Business Unit',	1,4,1);

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryCustodyDefinitions = @LineDefinitionEntryCustodyDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedRelationDefinitions = @LineDefinitionEntryNotedRelationDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
-- Declarations
DECLARE @ManualLineLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ManualLine');
DECLARE @ProjectCompletionToPropertyPlantAndEquipmentLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ProjectCompletionToPropertyPlantAndEquipment');
DECLARE @ProjectCompletionToInventoryLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ProjectCompletionToInventory');
DECLARE @ProjectCompletionToInvestmentPropertyLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ProjectCompletionToInvestmentProperty');
DECLARE @PPEDepreciationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEDepreciation');
DECLARE @IntangibleAmortizationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'IntangibleAmortization');
DECLARE @ExchangeVarianceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ExchangeVariance');
DECLARE @TradeSettlementLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'TradeSettlement');
DECLARE @HyperinflationLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'Hyperinflation');
DECLARE @CostReallocationToConstructionInProgressLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToConstructionInProgress');
DECLARE @CostReallocationToInvestmentPropertyUnderConstructionOrDevelopmentLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToInvestmentPropertyUnderConstructionOrDevelopment');
DECLARE @CostReallocationToCurrentInventoriesInTransitLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CostReallocationToCurrentInventoriesInTransit');
DECLARE @CashTransferExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransferExchange');
DECLARE @CashTransferLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashTransfer');
DECLARE @CashExchangeLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashExchange');
DECLARE @DepositCashToBankLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepositCashToBank');
DECLARE @DepositCheckToBankLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'DepositCheckToBank');
DECLARE @CashReceiptFromOtherToCashierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashReceiptFromOtherToCashier');
DECLARE @CheckReceiptFromOtherToCashierLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CheckReceiptFromOtherToCashier');
DECLARE @CashPaymentToOtherLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToOther');
DECLARE @CashPaymentToTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToTradePayable');
DECLARE @CashPaymentToTradePayableWithInvoiceLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentToTradePayableWithInvoice');
DECLARE @StockReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockReceiptFromTradePayable');
DECLARE @PPEReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'PPEReceiptFromTradePayable');
DECLARE @ConsumableServiceReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ConsumableServiceReceiptFromTradePayable');
DECLARE @RentalReceiptFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'RentalReceiptFromTradePayable');
DECLARE @WithholdingTaxFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'WithholdingTaxFromTradePayable');
DECLARE @CashPaymentFromTradePayableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashPaymentFromTradePayable');
DECLARE @CashReceiptFromTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CashReceiptFromTradeReceivable');
DECLARE @CheckReceiptFromTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'CheckReceiptFromTradeReceivable');
DECLARE @InvoiceToTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'InvoiceToTradeReceivable');
DECLARE @StockIssueToTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'StockIssueToTradeReceivable');
DECLARE @ServiceDeliveryToTradeReceivableLD INT = (SELECT [Id] FROM dbo.LineDefinitions WHERE [Code] = N'ServiceDeliveryToTradeReceivable');

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
