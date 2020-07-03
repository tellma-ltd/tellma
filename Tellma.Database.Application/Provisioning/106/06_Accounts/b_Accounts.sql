INSERT INTO @Accounts([Index], [Name], [Code], [AccountTypeId], [ClassificationId], [CurrencyId], [CenterId], [ResourceDefinitionId],[ContractDefinitionId]) VALUES
(11111, N'Cash Account 1', N'111-11', @CashOnHand, @106AC111, @ETB,@106C_Soreti,NULL,@CashOnHandAccountCD),
(11112, N'Cash Account 2', N'111-12', @CashOnHand, @106AC111, @ETB,@106C_Soreti,NULL,@CashOnHandAccountCD),
(11113, N'Cash Account 3', N'111-13', @CashOnHand, @106AC111, @ETB,@106C_Soreti,NULL,@CashOnHandAccountCD),
(11114, N'Cash Account 4', N'111-14', @CashOnHand, @106AC111, @ETB,@106C_Soreti,NULL,@CashOnHandAccountCD),
(11121, N'Bank Account 1', N'111-21', @BalancesWithBanks, @106AC111, @ETB,@106C_Soreti,NULL,@BankAccountCD),
(11122, N'Bank Account 2', N'111-22', @BalancesWithBanks, @106AC111, @USD,@106C_Soreti,NULL,@BankAccountCD),
(11123, N'Bank Account 3', N'111-23', @BalancesWithBanks, @106AC111, @ETB,@106C_Soreti,NULL,@BankAccountCD),
(11124, N'Bank Account 4', N'111-24', @BalancesWithBanks, @106AC111, @ETB,@106C_Soreti,NULL,@BankAccountCD),
(11210, N'Current trade receivable', N'112-10', @CurrentTradeReceivables, @106AC112, @ETB,@106C_Soreti,NULL,@CustomerCD),
(11220, N'Current receivable from related parties', N'112-20', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @106AC112, @ETB,@106C_Soreti,NULL,@CustomerCD),
(11230, N'Current prepayments', N'112-30', @CurrentPrepayments, @106AC112, @ETB,@106C_Soreti,NULL,@SupplierCD),
(11240, N'Current accrued income', N'112-40', @CurrentAccruedIncome, @106AC112, @ETB,@106C_Soreti,NULL,@CustomerCD),
(11250, N'Current billed but not received', N'112-50', @CurrentBilledButNotReceivedExtension, @106AC112, @ETB,@106C_Soreti,NULL,@SupplierCD),
(11299, N'ECL Allowance, Billed but not received', N'112-99', @CurrentBilledButNotReceivedExtension, @106AC112, @ETB,@106C_Soreti,NULL,NULL),
(11301, N'VAT Receivable', N'113-01', @CurrentValueAddedTaxReceivables, @106AC113, @ETB,@106C_Soreti,NULL,NULL),
(11302, N'WT Receivable', N'113-02', @WithholdingTaxReceivablesExtension, @106AC113, @ETB,@106C_Soreti,NULL,NULL),
(11303, N'Current tax assets, current', N'113-03', @CurrentTaxAssetsCurrent, @106AC113, @ETB,@106C_Soreti,NULL,NULL),
(11400, N'Rental Receivables', N'114-00', @CurrentReceivablesFromRentalOfProperties, @106AC114, @ETB,@106C_Soreti,NULL,@CustomerCD),
(11499, N'ECL Allowance, Rental Receivable', N'114-99', @CurrentReceivablesFromRentalOfProperties, @106AC114, @ETB,@106C_Soreti,NULL,NULL),
(11510, N'Staff Loans', N'115-10', @StaffDebtorsExtension, @106AC115, @ETB,@106C_Soreti,NULL,@EmployeeCD),
(11520, N'Salary Advances', N'115-20', @StaffDebtorsExtension, @106AC115, @ETB,@106C_Soreti,NULL,@EmployeeCD),
(11530, N'Temporary Petty Cash', N'115-30', @StaffDebtorsExtension, @106AC115, @ETB,@106C_Soreti,NULL,@EmployeeCD),
(11540, N'Travel Petty Cash - USD', N'115-40', @StaffDebtorsExtension, @106AC115, @USD,@106C_Soreti,NULL,@EmployeeCD),
(11599, N'Allowance for ECL, Staff debtors', N'115-99', @StaffDebtorsExtension, @106AC115, @ETB,@106C_Soreti,NULL,NULL),
(11600, N'Sundry Debtors', N'116-00', @SundryDebtorsExtension, @106AC116, @ETB,@106C_Soreti,NULL,@DebtorCD),
(11601, N'Sundry Debtor 1 - Trading', N'116-01', @SundryDebtorsExtension, @106AC116, @ETB,@106C_Soreti,NULL,@DebtorCD),
(11602, N'Sundry Debtor 2 - Trading', N'116-02', @SundryDebtorsExtension, @106AC116, @ETB,@106C_Soreti,NULL,@DebtorCD),
(11699, N'Allowance for ECL, Sundry debtors', N'116-99', @SundryDebtorsExtension, @106AC116, @ETB,@106C_Soreti,NULL,NULL),
(11711, N'Cleaned Grains', N'117-11', @CurrentInventoriesHeldForSale, @106AC117, @ETB,@106C_Soreti,@FinishedGrainRD,@WarehouseCD),
(11712, N'Rejected Grains', N'117-12', @CurrentInventoriesHeldForSale, @106AC117, @ETB,@106C_Soreti,@ByproductGrainRD,@WarehouseCD),
(11713, N'Medicine', N'117-13', @CurrentInventoriesHeldForSale, @106AC117, @USD,@106C_Soreti,@TradeMedicineRD,@WarehouseCD),
(11714, N'Construction Materials', N'117-14', @CurrentInventoriesHeldForSale, @106AC117, @USD,@106C_Soreti,@TradeConstructionMaterialRD,@WarehouseCD),
(11715, N'Spare Parts (for Sale)', N'117-15', @CurrentInventoriesHeldForSale, @106AC117, @USD,@106C_Soreti,@TradeSparePartRD,@WarehouseCD),
(11717, N'Processed Oil', N'117-17', @CurrentInventoriesHeldForSale, @106AC117, @ETB,@106C_Soreti,@FinishedOilRD,@WarehouseCD),
(11718, N'Oil Cake', N'117-18', @CurrentInventoriesHeldForSale, @106AC117, @ETB,@106C_Soreti,@ByproductOilRD,@WarehouseCD),
(11719, N'Assembled Vehicles', N'117-19', @CurrentInventoriesHeldForSale, @106AC117, @USD,@106C_Soreti,@FinishedVehicleRD,@WarehouseCD),
(11741, N'Oils in Process', N'117-41', @WorkInProgress, @106AC117, @ETB,@106C_OilMillingLine,@WorkInProgressRD,NULL),
(11742, N'Vehicles in Process', N'117-42', @WorkInProgress, @106AC117, @USD,@106C_MinidorLine,@WorkInProgressRD,NULL),
(11751, N'Raw Grains', N'117-51', @CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices, @106AC117, @ETB,@106C_Soreti,@RawGrainRD,@WarehouseCD),
(11752, N'Vehicle Components', N'117-52', @CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices, @106AC117, @USD,@106C_Soreti,@RawVehicleRD,@WarehouseCD),
(11753, N'Packaging and Storage Materials ', N'117-53', @CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices, @106AC117, NULL,@106C_Soreti,NULL,@WarehouseCD),
(11754, N'Spare Parts (Maintenance)', N'117-54', @CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices, @106AC117, NULL,@106C_Soreti,NULL,@WarehouseCD),
(11791, N'Import shipments', N'117-91', @CurrentInventoriesInTransit, @106AC117, @USD,NULL,NULL,NULL),
(11792, N'Export shipments', N'117-92', @CurrentInventoriesInTransit, @106AC117, @ETB,NULL,@FinishedGrainRD,NULL),
(11799, N'Other current inventories', N'117-99', @OtherInventories, @106AC117, NULL,@106C_Soreti,NULL,NULL),
(12111, N'Land', N'121-11', @Land, @106AC121, @ETB,NULL,@LandMemberRD,NULL),
(12112, N'Buildings', N'121-12', @Buildings, @106AC121, @ETB,NULL,@BuildingsMemberRD,NULL),
(12113, N'Leasehold Improvements', N'121-13', @Buildings, @106AC121, @ETB,NULL,@LeaseholdImprovementsMemberRD,NULL),
(12121, N'Machinery and Tools', N'121-21', @Machinery, @106AC121, NULL,NULL,@MachineryMemberRD,NULL),
(12122, N'Laboratory Equipment', N'121-22', @Machinery, @106AC121, NULL,NULL,NULL,NULL),
(12123, N'Power Generating Assets', N'121-23', @Machinery, @106AC121, NULL,NULL,@PowerGeneratingAssetsMemberRD,NULL),
(12124, N'Fumigation Equipment', N'121-24', @Machinery, @106AC121, NULL,NULL,NULL,NULL),
(12131, N'Vehicles', N'121-31', @Vehicles, @106AC121, @ETB,NULL,@MotorVehiclesMemberRD,NULL),
(12141, N'Fixtures and fittings', N'121-41', @FixturesAndFittings, @106AC121, NULL,NULL,NULL,NULL),
(12151, N'Office furniture', N'121-51', @OfficeEquipment, @106AC121, NULL,NULL,@OfficeEquipmentMemberRD,NULL),
(12152, N'Computer equipment', N'121-52', @OfficeEquipment, @106AC121, NULL,NULL,@ComputerEquipmentMemberRD,NULL),
(12171, N'Construction in progress', N'121-71', @ConstructionInProgress, @106AC121, NULL,NULL,NULL,NULL),
(12181, N'Owner-occupied property', N'121-81', @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel, @106AC121, NULL,NULL,NULL,NULL),
(12191, N'Other property, plant and equipment', N'121-91', @OtherPropertyPlantAndEquipment, @106AC121, NULL,NULL,NULL,NULL),
(12510, N'Non-current trade receivables', N'125-10', @NoncurrentTradeReceivables, @106AC125, NULL,@106C_Soreti,NULL,@CustomerCD),
(12520, N'Non-current receivables due from related parties', N'125-20', @NoncurrentReceivablesDueFromRelatedParties, @106AC125, NULL,@106C_Soreti,NULL,@CustomerCD),
(12530, N'Non-current prepayments', N'125-30', @NoncurrentPrepayments, @106AC125, NULL,@106C_Soreti,NULL,@SupplierCD),
(12540, N'Non-current accrued income', N'125-40', @NoncurrentAccruedIncome, @106AC125, NULL,@106C_Soreti,NULL,@CustomerCD),
(12550, N'Non-current receivables from taxes other than income tax', N'125-50', @NoncurrentReceivablesFromTaxesOtherThanIncomeTax, @106AC125, NULL,@106C_Soreti,NULL,NULL),
(12560, N'Non-current receivables from rental of properties', N'125-60', @NoncurrentReceivablesFromRentalOfProperties, @106AC125, NULL,@106C_Soreti,NULL,@CustomerCD),
(12570, N'Other non-current receivables', N'125-70', @OtherNoncurrentReceivables, @106AC125, NULL,@106C_Soreti,NULL,NULL),
(21101, N'Current provisions for employees', N'211-01', @CurrentProvisionsForEmployeeBenefits, @106AC211, NULL,@106C_Soreti,NULL,NULL),
(21111, N'Other current provisions', N'211-11', @OtherShorttermProvisions, @106AC211, NULL,@106C_Soreti,NULL,NULL),
(21210, N'Current Trade Payables', N'212-10', @TradeAndOtherCurrentPayablesToTradeSuppliers, @106AC212, @ETB,@106C_Soreti,NULL,@SupplierCD),
(21220, N'Current payables to related parties', N'212-20', @TradeAndOtherCurrentPayablesToRelatedParties, @106AC212, @ETB,@106C_Soreti,NULL,@SupplierCD),
(21230, N'Current Deferred Income', N'212-30', @DeferredIncomeClassifiedAsCurrent, @106AC212, @ETB,@106C_Soreti,NULL,@CustomerCD),
(21240, N'Accrued Expenses', N'212-40', @AccrualsClassifiedAsCurrent, @106AC212, @ETB,@106C_Soreti,NULL,@SupplierCD),
(21250, N'Employee Benefit Accruals', N'212-50', @ShorttermEmployeeBenefitsAccruals, @106AC212, @ETB,@106C_Soreti,NULL,@EmployeeCD),
(21260, N'Retention Payable', N'212-60', @CurrentRetentionPayables, @106AC212, @ETB,@106C_Soreti,NULL,@SupplierCD),
(21301, N'Current value added tax payables', N'213-01', @CurrentValueAddedTaxPayables, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21302, N'Current excise tax payables', N'213-02', @CurrentExciseTaxPayables, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21303, N'Current Social Security payables', N'213-03', @CurrentSocialSecurityPayablesExtension, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21304, N'Provident fund payable', N'213-04', @ProvidentFundPayableExtension, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21305, N'Employee Income Tax Payable', N'213-05', @CurrentEmployeeIncomeTaxPayablesExtension, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21306, N'Witholding Tax Payable', N'213-06', @WithholdingTaxPayableExtension, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21307, N'Cost Sharing Payable', N'213-07', @CostSharingPayableExtension, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21308, N'Dividend Tax Payable', N'213-08', @DividendTaxPayableExtension, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21309, N'Profit tax payable', N'213-09', @CurrentTaxLiabilitiesCurrent, @106AC213, @ETB,@106C_Soreti,NULL,NULL),
(21701, N'Sundry Creditor 1', N'217-01', @OtherCurrentFinancialLiabilities, @106AC217, @ETB,@106C_Soreti,NULL,NULL),
(21702, N'Sundry Creditor 2', N'217-02', @OtherCurrentFinancialLiabilities, @106AC217, @ETB,@106C_Soreti,NULL,NULL),
(21703, N'Short term bank loan', N'217-03', @OtherCurrentFinancialLiabilities, @106AC217, @ETB,@106C_Soreti,NULL,NULL),
(21704, N'Interest free loan', N'217-04', @OtherCurrentFinancialLiabilities, @106AC217, @ETB,@106C_Soreti,NULL,NULL),
(21705, N'Merchandise loan', N'217-05', @OtherCurrentFinancialLiabilities, @106AC217, @ETB,@106C_Soreti,NULL,NULL),
(22101, N'Non-current provisions for employee benefits', N'221-01', @NoncurrentProvisionsForEmployeeBenefits, @106AC221, NULL,@106C_Soreti,NULL,NULL),
(22102, N'Other non-current provisions', N'221-02', @OtherLongtermProvisions, @106AC221, NULL,@106C_Soreti,NULL,NULL),
(22201, N'Deferred tax liabilities', N'222-01', @DeferredTaxLiabilities, @106AC222, NULL,@106C_Soreti,NULL,NULL),
(22202, N'Current tax liabilities, non-current', N'222-02', @CurrentTaxLiabilitiesNoncurrent, @106AC222, NULL,@106C_Soreti,NULL,NULL),
(22203, N'Other non-current financial liabilities', N'222-03', @OtherNoncurrentFinancialLiabilities, @106AC222, NULL,@106C_Soreti,NULL,NULL),
(22204, N'Long term bank loan', N'222-04', @OtherNoncurrentFinancialLiabilities, @106AC222, @ETB,@106C_Soreti,NULL,NULL),
(31110, N'Paid up capital', N'311-10', @IssuedCapital, @106AC311, @ETB,@106C_Soreti,NULL,NULL),
(31210, N'Retained Earning - Soreti', N'312-10', @RetainedEarnings, @106AC312, @ETB,@106C_Soreti,NULL,NULL),
(31220, N'P/L - Export', N'312-20', @RetainedEarnings, @106AC312, @ETB,@106C_ExportCostofSales,NULL,NULL),
(31230, N'P/L - Import', N'312-30', @RetainedEarnings, @106AC312, @ETB,@106C_ImportCostofsales,NULL,NULL),
(31240, N'P/L - Agro Processing', N'312-40', @RetainedEarnings, @106AC312, @ETB,@106C_AgroProcessingCostofSales,NULL,NULL),
(31250, N'P/L - Manufacturing', N'312-50', @RetainedEarnings, @106AC312, @ETB,@106C_ManufacturingCostofsales,NULL,NULL),
(31260, N'P/L - Local Trade', N'312-60', @RetainedEarnings, @106AC312, @ETB,@106C_LocalTradeCostofSales,NULL,NULL),
(31270, N'P/L - Real Estate', N'312-70', @RetainedEarnings, @106AC312, @ETB,@106C_SoretiMallCostofsales,NULL,NULL),
(31301, N'Revaluation surplus', N'313-01', @RevaluationSurplus, @106AC313, @ETB,@106C_Soreti,NULL,NULL),
(31308, N'Reserve of gains and losses on financial assets measured at fair value through other comprehensive income', N'313-08', @ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome, @106AC313, @ETB,@106C_Soreti,NULL,NULL),
(31315, N'Reserve for non-current assets or disposal groups held for sale', N'313-15', @AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale, @106AC313, @ETB,@106C_Soreti,NULL,NULL),
(31318, N'Reserve for catastrophe', N'313-18', @ReserveForCatastrophe, @106AC313, @ETB,@106C_Soreti,NULL,NULL),
(31324, N'Statutory reserve', N'313-24', @StatutoryReserve, @106AC313, @ETB,@106C_Soreti,NULL,NULL),
(41110, N'Revenue from Cleaned Grains', N'411-10', @RevenueFromSaleOfGoods, @106AC411, @USD,@106C_ExportCostofSales,@FinishedGrainRD,NULL),
(41120, N'Revenue from Reject Grains', N'411-20', @RevenueFromSaleOfGoods, @106AC411, @USD,@106C_ExportCostofSales,@FinishedGrainRD,NULL),
(41210, N'Revenue from Imported Medicine', N'412-10', @RevenueFromSaleOfGoods, @106AC412, @ETB,@106C_ImportCostofsales,@TradeMedicineRD,NULL),
(41220, N'Revenue from Construction Materials', N'412-20', @RevenueFromSaleOfGoods, @106AC412, @ETB,@106C_ImportCostofsales,@TradeConstructionMaterialRD,NULL),
(41230, N'Revenue from Spare Parts', N'412-30', @RevenueFromSaleOfGoods, @106AC412, @ETB,@106C_ImportCostofsales,@TradeSparePartRD,NULL),
(41310, N'Revenue from Processed Oil', N'413-10', @RevenueFromSaleOfGoods, @106AC413, @ETB,@106C_AgroProcessingCostofSales,@FinishedOilRD,NULL),
(41320, N'Revenue from Oil Cake', N'413-20', @RevenueFromSaleOfGoods, @106AC413, @ETB,@106C_AgroProcessingCostofSales,@FinishedOilRD,NULL),
(41410, N'Revenue from Assembled Minidor', N'414-10', @RevenueFromSaleOfGoods, @106AC414, @ETB,@106C_ManufacturingCostofsales,@FinishedVehicleRD,NULL),
(41510, N'Revenue from Local Grains', N'415-10', @RevenueFromSaleOfGoods, @106AC415, @ETB,@106C_LocalTradeCostofSales,@FinishedGrainRD,NULL),
(42110, N'Revenue from Soreti Mall', N'421-10', @RevenueFromRenderingOfServices, @106AC421, @ETB,@106C_SoretiMallCostofsales,NULL,NULL),
(42120, N'Revenue from A.A. Building', N'421-20', @RevenueFromRenderingOfServices, @106AC421, @ETB,@106C_AABuildingCostofsales,NULL,NULL),
(43101, N'Interest Income - Source 1', N'431-01', @RevenueFromInterest, @106AC431, @ETB,@106C_Soreti,NULL,NULL),
(43102, N'Dividend Income - Source 1', N'431-02', @RevenueFromDividends, @106AC432, @ETB,@106C_Soreti,NULL,NULL),
(43103, N'Other revenue - Source 1', N'431-03', @OtherRevenue, @106AC433, @ETB,@106C_Soreti,NULL,NULL),
(43401, N'Income from Reject Grains', N'434-01', @OtherIncome, @106AC434, @ETB,NULL,@ByproductGrainRD,NULL),
(43402, N'Income from Oil Cake', N'434-02', @OtherIncome, @106AC434, @ETB,@106C_AgroProcessingCostofSales,@ByproductOilRD,NULL),
(51101, N'Raw Materials - Grain Cleaning #1', N'511-01', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site1CleaningandMilling,@RawGrainRD,NULL),
(51102, N'Raw Materials - Grain Cleaning #2', N'511-02', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site2LocalandExportGrainCleaning,@RawGrainRD,NULL),
(51103, N'PP Bags and threads - Grain Cleaning #1', N'511-03', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site1CleaningandMilling,NULL,NULL),
(51104, N'PP Bags and threads - Grain Cleaning #2', N'511-04', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site2LocalandExportGrainCleaning,NULL,NULL),
(51105, N'Fumigation - Grain Cleaning #1', N'511-05', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site1CleaningandMilling,NULL,NULL),
(51106, N'Fumigation - Grain Cleaning #2', N'511-06', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site2LocalandExportGrainCleaning,NULL,NULL),
(51107, N'Plastic Bottle - Oil Milling', N'511-07', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_OilMillingLine,NULL,NULL),
(51108, N'Other Packing Material - Oil Milling', N'511-08', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_OilMillingLine,NULL,NULL),
(51197, N'O/H Materials absorption - Grain Cleaning #1', N'511-97', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site1CleaningandMilling,NULL,NULL),
(51198, N'O/H Materials absorption - Grain Cleaning #2', N'511-98', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_Site2LocalandExportGrainCleaning,NULL,NULL),
(51199, N'O/H Materials absorption - Oil Milling', N'511-99', @RawMaterialsAndConsumablesUsed, @106AC511, @ETB,@106C_OilMillingLine,NULL,NULL),
(51210, N'Cost of Merchandise Sold - Exported Grains', N'512-10', @CostOfMerchandiseSold, @106AC512, @ETB,@106C_ExportCostofSales,@FinishedGrainRD,NULL),
(51221, N'Cost of Merchandise Sold - Medicine', N'512-21', @CostOfMerchandiseSold, @106AC512, @USD,@106C_ImportCostofsales,@TradeMedicineRD,NULL),
(51222, N'Cost of Merchandise Sold - Construction Materials', N'512-22', @CostOfMerchandiseSold, @106AC512, @USD,@106C_ImportCostofsales,@TradeConstructionMaterialRD,NULL),
(51223, N'Cost of Merchandise Sold - Spare Parts', N'512-23', @CostOfMerchandiseSold, @106AC512, @USD,@106C_ImportCostofsales,@TradeSparePartRD,NULL),
(51230, N'Cost of Merchandise Sold - Processed Oil', N'512-30', @CostOfMerchandiseSold, @106AC512, @ETB,@106C_AgroProcessingCostofSales,@FinishedOilRD,NULL),
(51240, N'Cost of Merchandise Sold - Assembled Minidor', N'512-40', @CostOfMerchandiseSold, @106AC512, @USD,@106C_ManufacturingCostofsales,@FinishedVehicleRD,NULL),
(51250, N'Cost of Merchandise Sold - Local Grains', N'512-50', @CostOfMerchandiseSold, @106AC512, @ETB,@106C_LocalTradeCostofSales,@FinishedGrainRD,NULL),
(51311, N'Insurance Expense', N'513-11', @InsuranceExpense, @106AC513, NULL,NULL,NULL,NULL),
(51312, N'(More insurance related expenses)', N'513-12', @InsuranceExpense, @106AC513, NULL,NULL,NULL,NULL),
(51321, N'Inspection and Standard', N'513-21', @ProfessionalFeesExpense, @106AC513, NULL,NULL,NULL,NULL),
(51322, N'Laboratory Service Fee', N'513-22', @ProfessionalFeesExpense, @106AC513, NULL,NULL,NULL,NULL),
(51323, N'Maintenance', N'513-23', @ProfessionalFeesExpense, @106AC513, NULL,NULL,NULL,NULL),
(51324, N'(More professional related expenses)', N'513-24', @ProfessionalFeesExpense, @106AC513, NULL,NULL,NULL,NULL),
(51331, N'Truck Rental', N'513-31', @TransportationExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51332, N'(more transportation related expenses)', N'513-32', @TransportationExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51341, N'Bank fees', N'513-41', @BankAndSimilarCharges, @106AC513, NULL,NULL,NULL,NULL),
(51342, N'(More Bank and similar charges)', N'513-42', @BankAndSimilarCharges, @106AC513, NULL,NULL,NULL,NULL),
(51351, N'Travel expense', N'513-51', @TravelExpense, @106AC513, NULL,NULL,NULL,NULL),
(51352, N'(More travel related expenses)', N'513-52', @TravelExpense, @106AC513, NULL,NULL,NULL,NULL),
(51361, N'Internet Subscription', N'513-61', @CommunicationExpense, @106AC513, NULL,NULL,NULL,NULL),
(51362, N'(More Communication related expenses)', N'513-62', @CommunicationExpense, @106AC513, NULL,NULL,NULL,NULL),
(51371, N'Electricity and Power', N'513-71', @UtilitiesExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51372, N'(More utilities expenses)', N'513-72', @UtilitiesExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51381, N'Advertisement', N'513-81', @AdvertisingExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51382, N'Promotional Cost', N'513-82', @AdvertisingExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51383, N'(More advertising expenses)', N'513-83', @AdvertisingExpense, @106AC513, @ETB,NULL,NULL,NULL),
(51411, N'Wages and Salaries', N'514-11', @WagesAndSalaries, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51412, N'Overtime', N'514-12', @WagesAndSalaries, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51413, N'Transportation Allowances', N'514-13', @WagesAndSalaries, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51414, N'Labour, Loading & Unloading', N'514-14', @WagesAndSalaries, @106AC514, NULL,NULL,@EmployeeBenefitRD,NULL),
(51415, N'(More Wages and Salaries related expenses)', N'514-15', @WagesAndSalaries, @106AC514, NULL,NULL,@EmployeeBenefitRD,NULL),
(51421, N'Provident Fund', N'514-21', @SocialSecurityContributions, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51422, N'Pension fund', N'514-22', @SocialSecurityContributions, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51423, N'(More social security contribution related expenses)', N'514-23', @SocialSecurityContributions, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51431, N'Medical Expenses', N'514-31', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51432, N'Bonus', N'514-32', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51433, N'Uniform & Outfits', N'514-33', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51434, N'Perdiem and travel costs - Local', N'514-34', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51435, N'Perdiem and travel costs - Foreign', N'514-35', @OtherShorttermEmployeeBenefits, @106AC514, @USD,NULL,@EmployeeBenefitRD,NULL),
(51436, N'Training and development', N'514-36', @OtherShorttermEmployeeBenefits, @106AC514, NULL,NULL,@EmployeeBenefitRD,NULL),
(51437, N'Compensation & Leave pay', N'514-37', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51438, N'Cash Indemnity', N'514-38', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51439, N'Employees Insurance', N'514-39', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51440, N'(More short term employee benefits related expenses)', N'514-40', @OtherShorttermEmployeeBenefits, @106AC514, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51510, N'Depreciation', N'515-10', @DepreciationExpense, @106AC515, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51520, N'Amortization', N'515-20', @AmortisationExpense, @106AC515, @ETB,NULL,@EmployeeBenefitRD,NULL),
(51901, N'Land and Building Taxes', N'519-01', @OtherExpenseByNature, @106AC519, NULL,NULL,NULL,NULL),
(51902, N'Misc. Expenses by nature', N'519-02', @OtherExpenseByNature, @106AC519, NULL,NULL,NULL,NULL),
(52110, N'Gain (loss) on disposal of property, plant and equipment', N'521-10', @GainLossOnDisposalOfPropertyPlantAndEquipmentExtension, @106AC521, NULL,NULL,NULL,NULL),
(52120, N'Gain (loss) on foreign exchange', N'521-20', @GainLossOnForeignExchangeExtension, @106AC521, NULL,NULL,NULL,NULL),
(52201, N'Gains (losses) on net monetary position', N'522-01', @GainsLossesOnNetMonetaryPosition, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52202, N'Gain (loss) arising from derecognition of financial assets measured at amortised cost', N'522-02', @GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52203, N'Finance income', N'522-03', @FinanceIncome, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52204, N'Finance costs', N'522-04', @FinanceCosts, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52205, N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9', N'522-05', @ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52206, N'Share of profit (loss) of associates and joint ventures accounted for using equity method', N'522-06', @ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52207, N'Other income (expense) from subsidiaries, jointly controlled entities and associates', N'522-07', @OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52208, N'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category', N'522-08', @GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52209, N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category', N'522-09', @CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThrou, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52210, N'Hedging gains (losses) for hedge of group of items with offsetting risk positions', N'522-10', @HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(52211, N'Tax income (expense)', N'522-11', @IncomeTaxExpenseContinuingOperations, @106AC522, NULL,@106C_Soreti,NULL,NULL),
(71101, N'Cash control', N'711-01', @CashControlExtension, @106AC711, @ETB,@106C_Soreti,NULL,NULL),
(71112, N'Trading control', N'711-12', @TradingControlExtension, @106AC711, @ETB,@106C_Soreti,NULL,NULL),
(71123, N'Payroll control', N'711-23', @PayrollControlExtension, @106AC711, @ETB,@106C_Soreti,NULL,NULL),
(71134, N'Other document control', N'711-34', @OtherControlExtension, @106AC711, @ETB,@106C_Soreti,NULL,NULL)

UPDATE @Accounts SET ContractId = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Cash 1') WHERE [Index] = 11111;
UPDATE @Accounts SET ContractId = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank 1') WHERE [Index] = 11121;
UPDATE @Accounts SET ContractId = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank 2') WHERE [Index] = 11121;


EXEC [api].[Accounts__Save]
	@Entities = @Accounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting Accounts: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF (1=1) -- Declarations
BEGIN
DECLARE @1102_101 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1102-101');
DECLARE @1103_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-001');
DECLARE @1103_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-002');
DECLARE @1103_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-003');
DECLARE @1103_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-004');
DECLARE @1103_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-005');
DECLARE @1103_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-006');
DECLARE @1103_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-007');
DECLARE @1103_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-008');
DECLARE @1103_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-009');
DECLARE @1103_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-010');
DECLARE @1103_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-011');
DECLARE @1103_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-012');
DECLARE @1103_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-013');
DECLARE @1103_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-014');
DECLARE @1103_015 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-015');
DECLARE @1103_016 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-016');
DECLARE @1103_017 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-017');
DECLARE @1103_018 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-018');
DECLARE @1103_019 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-019');
DECLARE @1103_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-020');
DECLARE @1103_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-021');
DECLARE @1103_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-022');
DECLARE @1103_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-023');
DECLARE @1103_024 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-024');
DECLARE @1103_025 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-025');
DECLARE @1103_026 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-026');
DECLARE @1103_027 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-027');
DECLARE @1103_028 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-028');
DECLARE @1103_029 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-029');
DECLARE @1103_030 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-030');
DECLARE @1103_031 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-031');
DECLARE @1103_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-032');
DECLARE @1103_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-033');
DECLARE @1103_034 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-034');
DECLARE @1103_035 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-035');
DECLARE @1103_036 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-036');
DECLARE @1103_037 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-037');
DECLARE @1103_038 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-038');
DECLARE @1103_039 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-039');
DECLARE @1103_040 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-040');
DECLARE @1103_041 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-041');
DECLARE @1103_042 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-042');
DECLARE @1103_043 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-043');
DECLARE @1103_044 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-044');
DECLARE @1103_045 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-045');
DECLARE @1103_046 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-046');
DECLARE @1103_047 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-047');
DECLARE @1103_048 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-048');
DECLARE @1103_049 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-049');
DECLARE @1103_050 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-050');
DECLARE @1103_051 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-051');
DECLARE @1103_052 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-052');
DECLARE @1103_053 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-053');
DECLARE @1103_054 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-054');
DECLARE @1103_055 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-055');
DECLARE @1103_056 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-056');
DECLARE @1103_057 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1103-057');
DECLARE @1121_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-010');
DECLARE @1121_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-020');
DECLARE @1121_030 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-030');
DECLARE @1121_040 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-040');
DECLARE @1121_050 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1121-050');
DECLARE @1206_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1206-001');
DECLARE @1206_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1206-002');
DECLARE @1204_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1204-010');
DECLARE @1202_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1202-010');
DECLARE @1205_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1205-010');
DECLARE @1209_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1209-001');
DECLARE @1209_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1209-002');
DECLARE @1401_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-001');
DECLARE @1401_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-002');
DECLARE @1401_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-003');
DECLARE @1401_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-004');
DECLARE @1401_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-005');
DECLARE @1401_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-006');
DECLARE @1401_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-007');
DECLARE @1401_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-013');
DECLARE @1401_016 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-016');
DECLARE @1401_017 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-017');
DECLARE @1401_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-020');
DECLARE @1401_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-021');
DECLARE @1401_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-022');
DECLARE @1401_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-023');
DECLARE @1401_030 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-030');
DECLARE @1401_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-032');
DECLARE @1401_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1401-033');
DECLARE @1402_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-001');
DECLARE @1402_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-002');
DECLARE @1402_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-003');
DECLARE @1402_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-004');
DECLARE @1402_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-005');
DECLARE @1402_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-006');
DECLARE @1402_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-007');
DECLARE @1402_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-008');
DECLARE @1402_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-009');
DECLARE @1402_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-010');
DECLARE @1402_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-011');
DECLARE @1402_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-021');
DECLARE @1402_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-022');
DECLARE @1402_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-023');
DECLARE @1402_024 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-024');
DECLARE @1402_031 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-031');
DECLARE @1402_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-032');
DECLARE @1402_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1402-033');
DECLARE @1403_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-001');
DECLARE @1403_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-002');
DECLARE @1403_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-003');
DECLARE @1403_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-004');
DECLARE @1403_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-005');
DECLARE @1403_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-006');
DECLARE @1403_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-007');
DECLARE @1403_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-008');
DECLARE @1403_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-009');
DECLARE @1403_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-010');
DECLARE @1403_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-011');
DECLARE @1403_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-012');
DECLARE @1403_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-013');
DECLARE @1403_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-014');
DECLARE @1403_015 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1403-015');
DECLARE @1404_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1404-001');
DECLARE @1405_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1405-001');
DECLARE @1406_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1406-001');
DECLARE @1410_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1410-001');
DECLARE @1411_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1411-001');
DECLARE @1412_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1412-001');
DECLARE @1416_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1416-001');
DECLARE @1416_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1416-002');
DECLARE @1416_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1416-003');
DECLARE @1418_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-001');
DECLARE @1418_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-002');
DECLARE @1418_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-003');
DECLARE @1418_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-004');
DECLARE @1418_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1418-005');
DECLARE @1430_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-001');
DECLARE @1430_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-002');
DECLARE @1430_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-003');
DECLARE @1430_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-004');
DECLARE @1430_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-005');
DECLARE @1430_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-006');
DECLARE @1430_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-007');
DECLARE @1430_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-008');
DECLARE @1430_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1430-009');
DECLARE @1431_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-001');
DECLARE @1431_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-002');
DECLARE @1431_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-003');
DECLARE @1431_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-004');
DECLARE @1431_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-005');
DECLARE @1431_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-006');
DECLARE @1431_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-007');
DECLARE @1431_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-008');
DECLARE @1431_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-009');
DECLARE @1431_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-010');
DECLARE @1431_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-011');
DECLARE @1431_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-021');
DECLARE @1431_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-022');
DECLARE @1431_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1431-023');
DECLARE @1601_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1601-001');
DECLARE @1601_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1601-002');
DECLARE @1619_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1619-001');
DECLARE @1619_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1619-002');
DECLARE @1619_099 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1619-099');
DECLARE @1801_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1801-010');
DECLARE @1801_020 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1801-020');
DECLARE @1802_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-001');
DECLARE @1802_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-002');
DECLARE @1802_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-003');
DECLARE @1802_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1802-004');
DECLARE @1803_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1803-010');
DECLARE @1805_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'1805-001');
DECLARE @2201_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-001');
DECLARE @2201_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-002');
DECLARE @2201_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-003');
DECLARE @2201_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2201-004');
DECLARE @2301 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301');
DECLARE @2301_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-001');
DECLARE @2301_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-002');
DECLARE @2301_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-003');
DECLARE @2301_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-004');
DECLARE @2301_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-005');
DECLARE @2301_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-006');
DECLARE @2301_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2301-007');
DECLARE @2401 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2401');
DECLARE @2402 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402');
DECLARE @2402_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-001');
DECLARE @2402_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-002');
DECLARE @2402_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-003');
DECLARE @2402_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-004');
DECLARE @2402_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-005');
DECLARE @2402_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-006');
DECLARE @2402_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-007');
DECLARE @2402_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-008');
DECLARE @2402_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-009');
DECLARE @2402_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-010');
DECLARE @2402_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-011');
DECLARE @2402_099 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2402-099');
DECLARE @2501 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2501');
DECLARE @2502 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2502');
DECLARE @2503 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2503');
DECLARE @2504 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2504');
DECLARE @2601 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2601');
DECLARE @2701 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2701');
DECLARE @2702 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2702');
DECLARE @2703  INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2703 ');
DECLARE @2801 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2801');
DECLARE @2802 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2802');
DECLARE @2803 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2803');
DECLARE @2901 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'2901');
DECLARE @3101_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3101-001');
DECLARE @3102_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3102-001');
DECLARE @3103_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3103-001');
DECLARE @3909_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'3909-001');
DECLARE @4101_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4101-001');
DECLARE @4101_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4101-002');
DECLARE @4101_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4101-003');
DECLARE @4102_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-001');
DECLARE @4102_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-002');
DECLARE @4102_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-003');
DECLARE @4102_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-004');
DECLARE @4102_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-005');
DECLARE @4102_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-006');
DECLARE @4102_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-007');
DECLARE @4102_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-008');
DECLARE @4102_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-009');
DECLARE @4102_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-010');
DECLARE @4102_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4102-011');
DECLARE @4103_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-001');
DECLARE @4103_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-002');
DECLARE @4103_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-003');
DECLARE @4103_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4103-004');
DECLARE @4104_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4104-001');
DECLARE @4104_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4104-002');
DECLARE @4105_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4105-001');
DECLARE @4105_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4105-002');
DECLARE @4201_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4201-001');
DECLARE @4201_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4201-002');
DECLARE @4909_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-001');
DECLARE @4909_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-002');
DECLARE @4909_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-003');
DECLARE @4909_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-004');
DECLARE @4909_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'4909-005');
DECLARE @5101_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-001');
DECLARE @5101_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-002');
DECLARE @5101_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-003');
DECLARE @5101_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-004');
DECLARE @5101_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-005');
DECLARE @5101_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-006');
DECLARE @5011_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5011-007');
DECLARE @5101_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-008');
DECLARE @5101_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-009');
DECLARE @5101_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-010');
DECLARE @5101_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5101-011');
DECLARE @5102_021 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-021');
DECLARE @5102_022 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-022');
DECLARE @5102_023 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-023');
DECLARE @5102_024 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5102-024');
DECLARE @5103_031 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5103-031');
DECLARE @5103_032 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5103-032');
DECLARE @5103_033 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5103-033');
DECLARE @5120_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-001');
DECLARE @5120_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-002');
DECLARE @5120_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-003');
DECLARE @5120_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-004');
DECLARE @5120_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-005');
DECLARE @5120_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-006');
DECLARE @5120_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-007');
DECLARE @5120_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-008');
DECLARE @5120_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-009');
DECLARE @5120_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-010');
DECLARE @5120_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5120-011');
DECLARE @5202_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-001');
DECLARE @5202_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-002');
DECLARE @5202_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-003');
DECLARE @5202_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-004');
DECLARE @5202_099 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5202-099');
DECLARE @5302_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-001');
DECLARE @5302_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-002');
DECLARE @5302_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-003');
DECLARE @5302_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-004');
DECLARE @5302_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-005');
DECLARE @5302_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-006');
DECLARE @5302_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-007');
DECLARE @5302_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-008');
DECLARE @5302_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-009');
DECLARE @5302_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-010');
DECLARE @5302_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-011');
DECLARE @5302_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-012');
DECLARE @5302_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-013');
DECLARE @5302_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5302-014');
DECLARE @5303_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-001');
DECLARE @5303_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-002');
DECLARE @5303_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-003');
DECLARE @5303_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-004');
DECLARE @5303_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-005');
DECLARE @5303_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-006');
DECLARE @5303_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-007');
DECLARE @5303_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5303-008');
DECLARE @5402_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-001');
DECLARE @5402_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-002');
DECLARE @5402_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-003');
DECLARE @5402_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-004');
DECLARE @5402_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-005');
DECLARE @5402_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-006');
DECLARE @5402_007 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-007');
DECLARE @5402_008 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-008');
DECLARE @5402_009 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-009');
DECLARE @5402_010 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-010');
DECLARE @5402_011 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-011');
DECLARE @5402_012 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-012');
DECLARE @5402_013 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-013');
DECLARE @5402_014 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5402-014');
DECLARE @5403_001 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-001');
DECLARE @5403_002 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-002');
DECLARE @5403_003 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-003');
DECLARE @5403_004 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-004');
DECLARE @5403_005 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-005');
DECLARE @5403_006 INT =(SELECT [Id] FROM dbo.Accounts WHERE [Code] = N'5403-006');

END