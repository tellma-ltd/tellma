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