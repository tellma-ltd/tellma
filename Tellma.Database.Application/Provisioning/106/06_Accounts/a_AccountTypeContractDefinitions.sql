INSERT INTO @AccountClassifications([Index],[ParentIndex],[Code],[Name]) VALUES
	(0, NULL, N'1', N'Assets'),
	(1, 0, N'11', N'Current Assets'),
	(2, 1, N'111', N'Cash and Cash Equivalent'),
	(3, 2, N'1111', N'Cash on Hand'),
	(5, 2, N'1112', N'Petty Cash-'),
	(7, 2, N'1113', N'Cash at Bank'),
	(65, 1, N'112', N'Current Loans and Receivables'),
	(66, 65, N'1121', N'Current Trade and Other Receivables'),
	(72, 65, N'1122', N'Tax Receivables'),
	(75, 65, N'1123', N'Deposits and Retention'),
	(77, 65, N'1124', N'Current Debtors'),
	(80, 65, N'1129', N'Impairment Loss'),
	(83, 1, N'113', N'Stock'),
	(84, 83, N'11301', N'Stock raw grains'),
	(106, 83, N'11302', N'Stock Cleaned (and Packed) Grains'),
	(128, 83, N'11303', N'Stock Rejects of grains'),
	(144, 83, N'11304', N'FINISHED GOODS Force Motor/Vehicle'),
	(146, 83, N'11305', N'Finished Goods Edible Oil'),
	(148, 83, N'11306', N'Byproducts '),
	(150, 83, N'11307', N'Work In Process Minidor/Vehicle'),
	(152, 83, N'11308', N'Work In Process Edible Oil'),
	(154, 83, N'11309', N'Raw Materials (Minidor Components)/CKD'),
	(156, 83, N'11310', N'Raw Materials Edible Oil'),
	(158, 83, N'11311', N'Stock Imported Minidor'),
	(160, 83, N'11312', N'Stock imported Oil'),
	(162, 83, N'11313', N'Imported Spare Parts'),
	(164, 83, N'11314', N'Imported Medicine'),
	(166, 83, N'11315', N'Stock- Construction Materials'),
	(168, 83, N'11316', N'Stock Packing materials'),
	(172, 83, N'11317', N'Stock Oil, fuel & lubricants'),
	(174, 83, N'11318', N'Stock Other Supplies '),
	(180, 83, N'11319', N'Stock- Medicine'),
	(184, 83, N'11320', N'Goods in Transit Import Goods'),
	(194, 83, N'11321', N'Goods in Transit Export Commodities');

EXEC [api].[AccountClassifications__Save] --  N'cash-and-cash-equivalents',
	@Entities = @AccountClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting AccountClassifications: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;


INSERT INTO dbo.[AccountTypeContractDefinitions]
([AccountTypeId],								[ContractDefinitionId]) VALUES
(@CashOnHand,									@106cashiersCD),
(@CashOnHand,									@106petty_cash_fundsCD),
(@BalancesWithBanks	,							@106bank_accountsCD),
(@RawMaterials,									@106warehousesCD),
(@ProductionSupplies,							@106warehousesCD),
(@WorkInProgress,								@106warehousesCD),
(@FinishedGoods,								@106warehousesCD),
(@CurrentInventoriesInTransit,					@foreign_importsCD),
(@CurrentInventoriesInTransit,					@foreign_exportsCD),
(@CurrentPrepayments,							@106suppliersCD),
(@OtherCurrentFinancialAssets,					@106debtorsCD), -- sundry debtor
(@OtherCurrentFinancialAssets,					@106employeesCD), -- staff debtor
(@TradeAndOtherCurrentPayablesToTradeSuppliers,	@106suppliersCD),
(@AccrualsClassifiedAsCurrent,					@106suppliersCD),
(@AccrualsClassifiedAsCurrent,					@106employeesCD), -- last  5 days unpaid
(@CashPurchaseDocumentControlExtension,			@106suppliersCD),
(@DeferredIncomeClassifiedAsCurrent,			@106customersCD),
(@CurrentTradeReceivables,						@106customersCD),
(@CurrentAccruedIncome,							@106customersCD),
(@CashSaleDocumentControlExtension,				@106customersCD),
(@OtherCurrentFinancialLiabilities,				@106creditorsCD),
(@OtherCurrentFinancialLiabilities,				@106partnersCD);
INSERT INTO dbo.[AccountTypeResourceDefinitions]
([AccountTypeId],		[ResourceDefinitionId]) VALUES
(@RawMaterials,			@106raw_grainsRD),
(@RawMaterials,			@106raw_vehiclesRD),
(@RawMaterials,			@106raw_oilsRD),

(@WorkInProgress,		@106work_in_progressRD),

(@FinishedGoods,		@106finished_grainsRD),
(@FinishedGoods,		@106finished_vehiclesRD),
(@FinishedGoods,		@106finished_oilsRD),
(@FinishedGoods,		@106byproducts_grainsRD),
(@FinishedGoods,		@106byproducts_oilsRD),

(@Merchandise,			@106medicinesRD),
(@Merchandise,			@106construction_materialsRD);