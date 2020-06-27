INSERT INTO @Accounts([Index], [Name], [Code], [AccountTypeId], [ClassificationId], [CurrencyId], [CenterId], [ResourceDefinitionId],[ContractDefinitionId]) VALUES
(1111101, N'Cash Account 1 - Trading', N'1111-101', @CashOnHand, @AC1111, @ETB,@106C_TradingHO,NULL,@CashOnHandAccountCD),
(1111102, N'Cash Account 2 - Trading', N'1111-102', @CashOnHand, @AC1111, @ETB,@106C_TradingHO,NULL,@CashOnHandAccountCD),
(1111201, N'Cash Account 1 - Real Estate', N'1111-201', @CashOnHand, @AC1111, @ETB,@106C_RealEstate,NULL,@CashOnHandAccountCD),
(1111202, N'Cash Account 2 - Real Estate', N'1111-202', @CashOnHand, @AC1111, @ETB,@106C_RealEstate,NULL,@CashOnHandAccountCD),
(1112101, N'Bank Account 1 - Trading', N'1112-101', @BalancesWithBanks, @AC1112, @ETB,@106C_TradingHO,NULL,@BankAccountCD),
(1112102, N'Bank Account 2 - Trading', N'1112-102', @BalancesWithBanks, @AC1112, @USD,@106C_TradingHO,NULL,@BankAccountCD),
(1112201, N'Bank Account 1 - Real Estate', N'1112-201', @BalancesWithBanks, @AC1112, @ETB,@106C_RealEstate,NULL,@BankAccountCD),
(1112202, N'Bank Account 2 - Real Estate', N'1112-202', @BalancesWithBanks, @AC1112, @ETB,@106C_RealEstate,NULL,@BankAccountCD),
(1121100, N'Current trade receivable', N'1121-100', @CurrentTradeReceivables, @AC1121, @ETB,@106C_TradingHO,NULL,@CustomerCD),
(1121199, N'ECL Allowance, Current trade receivable', N'1121-199', @CurrentTradeReceivables, @AC1121, @ETB,@106C_TradingHO,NULL,NULL),
(1122100, N'Current receivable from related parties - Trading - *', N'1122-100', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122, @ETB,@106C_TradingHO,NULL,@CustomerCD),
(1122199, N'ECL Allowance, Related Parties receivable - Trading', N'1122-199', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122, @ETB,@106C_TradingHO,NULL,NULL),
(1122200, N'Current receivable from related parties - Real Estate - *', N'1122-200', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122, @ETB,@106C_RealEstate,NULL,@CustomerCD),
(1122299, N'ECL Allowance, Related Parties receivable - Real Estate', N'1122-299', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122, @ETB,@106C_RealEstate,NULL,NULL),
(1123100, N'Current prepayments - Trading - *', N'1123-100', @CurrentPrepayments, @AC1123, @ETB,@106C_TradingHO,NULL,@SupplierCD),
(1123199, N'ECL Allowance, Prepayments - Trading', N'1123-199', @CurrentPrepayments, @AC1123, @ETB,@106C_TradingHO,NULL,NULL),
(1123200, N'Current prepayments - Real Estate - *', N'1123-200', @CurrentPrepayments, @AC1123, @ETB,@106C_RealEstate,NULL,@SupplierCD),
(1123299, N'ECL Allowance, Prepayments - Real Estate', N'1123-299', @CurrentPrepayments, @AC1123, @ETB,@106C_RealEstate,NULL,NULL),
(1124100, N'Current accrued income - Trading - *', N'1124-100', @CurrentAccruedIncome, @AC1124, @ETB,@106C_TradingHO,NULL,@CustomerCD),
(1124199, N'ECL Allowance, Accrued Income - Trading', N'1124-199', @CurrentAccruedIncome, @AC1124, @ETB,@106C_TradingHO,NULL,NULL),
(1124200, N'Current accrued income - Real Estate - *', N'1124-200', @CurrentAccruedIncome, @AC1124, @ETB,@106C_RealEstate,NULL,@CustomerCD),
(1124299, N'ECL Allowance, Accrued Income - Real Estate', N'1124-299', @CurrentAccruedIncome, @AC1124, @ETB,@106C_RealEstate,NULL,NULL),
(1125000, N'Current billed but not received - *', N'1125-000', @CurrentBilledButNotReceivedExtension, @AC1125, @ETB,NULL,NULL,@SupplierCD),
(1125999, N'ECL Allowance, Billed but not received', N'1125-999', @CurrentBilledButNotReceivedExtension, @AC1125, @ETB,NULL,NULL,NULL),
(1126101, N'VAT Receivable', N'1126-101', @CurrentValueAddedTaxReceivables, @AC1126, @ETB,NULL,NULL,NULL),
(1126102, N'WT Receivable', N'1126-102', @WithholdingTaxReceivablesExtension, @AC1126, @ETB,NULL,NULL,NULL),
(1126201, N'VAT Receivable', N'1126-201', @CurrentValueAddedTaxReceivables, @AC1126, @ETB,NULL,NULL,NULL),
(1126202, N'WT Receivable', N'1126-202', @WithholdingTaxReceivablesExtension, @AC1126, @ETB,NULL,NULL,NULL),
(1127200, N'Rental Receivables - *', N'1127-200', @CurrentReceivablesFromRentalOfProperties, @AC1127, @ETB,@106C_RealEstate,NULL,@CustomerCD),
(1127299, N'ECL Allowance, Rental Receivable', N'1127-299', @CurrentReceivablesFromRentalOfProperties, @AC1127, @ETB,NULL,NULL,NULL),
(1141010, N'Staff Loans - *', N'1141-010', @StaffDebtorsExtension, @AC1141, @ETB,NULL,NULL,@EmployeeCD),
(1141020, N'Salary Advances - *', N'1141-020', @StaffDebtorsExtension, @AC1141, @ETB,NULL,NULL,@EmployeeCD),
(1141030, N'Temporary Petty Cash - *', N'1141-030', @StaffDebtorsExtension, @AC1141, @ETB,NULL,NULL,@EmployeeCD),
(1141040, N'Travel Petty Cash - USD - *', N'1141-040', @StaffDebtorsExtension, @AC1141, @USD,NULL,NULL,@EmployeeCD),
(1141999, N'Allowance for ECL, Staff debtors', N'1141-999', @StaffDebtorsExtension, @AC1141, @ETB,NULL,NULL,NULL),
(1142100, N'Sundry Debtors - Trading - *', N'1142-100', @SundryDebtorsExtension, @AC1142, @ETB,@106C_TradingHO,NULL,@DebtorCD),
(1142199, N'Allowance for ECL, Sundry debtors - Trading', N'1142-199', @SundryDebtorsExtension, @AC1142, @ETB,@106C_TradingHO,NULL,NULL),
(1142200, N'Sundry Debtors - Real Estate - *', N'1142-200', @SundryDebtorsExtension, @AC1142, @ETB,@106C_RealEstate,NULL,@CreditorCD),
(1142299, N'Allowance for ECL, Sundry debtors - Real Estate', N'1142-299', @SundryDebtorsExtension, @AC1142, @ETB,@106C_RealEstate,NULL,NULL),
(1171101, N'Raw Grains - Site 1 - *', N'1171-101', @RawMaterials, @AC1171, @ETB,@106C_TradingHO,@RawGrainRD,@WarehouseCD),
(1171102, N'Raw Grains - Site 2 - *', N'1171-102', @RawMaterials, @AC1171, @ETB,@106C_TradingHO,@RawGrainRD,@WarehouseCD),
(1171140, N'Vehicle Components - *', N'1171-140', @RawMaterials, @AC1171, @USD,@106C_TradingHO,@RawVehicleRD,@WarehouseCD),
(1172121, N'Medicine - *', N'1172-121', @Merchandise, @AC1172, @USD,@106C_TradingHO,@MedicineRD,@WarehouseCD),
(1172122, N'Construction Materials - *', N'1172-122', @Merchandise, @AC1172, @USD,@106C_TradingHO,@ConstructionMaterialRD,@WarehouseCD),
(1172123, N'Spare Parts (for Sale) - *', N'1172-123', @Merchandise, @AC1172, @USD,@106C_TradingHO,NULL,@WarehouseCD),
(1173101, N'Grains in Process - Site 1', N'1173-101', @WorkInProgress, @AC1173, @ETB,@106C_GrainsFactoriesSite1,@WorkInProgressRD,NULL),
(1173102, N'Grains in Process - Site 2', N'1173-102', @WorkInProgress, @AC1173, @ETB,@106C_GrainsFactoriesSite2,@WorkInProgressRD,NULL),
(1173130, N'Oils in Process', N'1173-130', @WorkInProgress, @AC1173, @ETB,@106C_AgroProcessingProduction,@WorkInProgressRD,NULL),
(1173140, N'Vehicles in Process', N'1173-140', @WorkInProgress, @AC1173, @USD,NULL,@WorkInProgressRD,NULL),
(1174111, N'Cleaned Export Grains - Site 1', N'1174-111', @FinishedGoods, @AC1174, @ETB,@106C_TradingHO,@FinishedGrainRD,@WarehouseCD),
(1174112, N'Cleaned Export Grains - Site 2', N'1174-112', @FinishedGoods, @AC1174, @ETB,@106C_TradingHO,@FinishedGrainRD,@WarehouseCD),
(1174130, N'Processed Oil', N'1174-130', @FinishedGoods, @AC1174, @ETB,@106C_TradingHO,@FinishedOilRD,@WarehouseCD),
(1174140, N'Assembled Vehicles', N'1174-140', @FinishedGoods, @AC1174, @USD,@106C_TradingHO,@FinishedVehicleRD,@WarehouseCD),
(1174151, N'Cleaned Domestic Grains - Site 1', N'1174-151', @FinishedGoods, @AC1174, @ETB,@106C_TradingHO,@FinishedGrainRD,@WarehouseCD),
(1174152, N'Cleaned Domestic Grains - Site 2', N'1174-152', @FinishedGoods, @AC1174, @ETB,@106C_TradingHO,@FinishedGrainRD,@WarehouseCD),
(1175100, N'Packaging and Storage Materials - *', N'1175-100', @CurrentPackagingAndStorageMaterials, @AC1175, NULL,@106C_TradingHO,NULL,@WarehouseCD),
(1176100, N'Spare Parts (Not for sale) - Trading', N'1176-100', @SpareParts, @AC1176, NULL,@106C_TradingHO,NULL,@WarehouseCD),
(1176200, N'Spare Parts (Not for sale) - Real Estate', N'1176-200', @SpareParts, @AC1176, NULL,@106C_RealEstate,NULL,@WarehouseCD),
(1177101, N'Import shipments - Medicine', N'1177-101', @CurrentInventoriesInTransit, @AC1177, @USD,NULL,@MedicineRD,NULL),
(1177102, N'Import shipments - Construction', N'1177-102', @CurrentInventoriesInTransit, @AC1177, @USD,NULL,@ConstructionMaterialRD,NULL),
(1177103, N'Import shipment - Manufacturing', N'1177-103', @CurrentInventoriesInTransit, @AC1177, @USD,NULL,@RawVehicleRD,NULL),
(1177201, N'Import shipments - Real Estate', N'1177-201', @CurrentInventoriesInTransit, @AC1177, @USD,NULL,NULL,NULL),
(1211010, N'Land', N'1211-010', @Land, @AC1211, @ETB,NULL,NULL,NULL),
(1211020, N'Buildings', N'1211-020', @Buildings, @AC1211, @ETB,NULL,@BuildingRD,NULL),
(1212000, N'Machineries', N'1212-000', @Machinery, @AC1212, NULL,NULL,NULL,NULL),
(1213000, N'Vehicles', N'1213-000', @Vehicles, @AC1213, @ETB,NULL,@VehicleRD,NULL),
(1215010, N'Office furniture', N'1215-010', @OfficeEquipment, @AC1215, NULL,NULL,@OfficeEquipmentRD,NULL),
(1215020, N'Computer equipment', N'1215-020', @OfficeEquipment, @AC1215, NULL,NULL,@ComputerEquipmentRD,NULL),
(1217001, N'AA Building under construction', N'1217-001', @ConstructionInProgress, @AC1217, NULL,@106C_AABuildingUnderConstruction,NULL,NULL),
(2111100, N'Current provisions for employees, trading', N'2111-100', @CurrentProvisionsForEmployeeBenefits, @AC2111, NULL,NULL,NULL,NULL),
(2111200, N'Current provisions for employees, real estate', N'2111-200', @CurrentProvisionsForEmployeeBenefits, @AC2111, NULL,NULL,NULL,NULL),
(2121000, N'Current Trade Payables - *', N'2121-000', @TradeAndOtherCurrentPayablesToTradeSuppliers, @AC2121, @ETB,NULL,NULL,@SupplierCD),
(2122000, N'Current payables to related parties - *', N'2122-000', @TradeAndOtherCurrentPayablesToRelatedParties, @AC2122, @ETB,NULL,NULL,@SupplierCD),
(2122001, N'Related Party 1', N'2122-001', @TradeAndOtherCurrentPayablesToRelatedParties, @AC2122, @ETB,NULL,NULL,NULL),
(2122002, N'Related Party 2', N'2122-002', @TradeAndOtherCurrentPayablesToRelatedParties, @AC2122, @ETB,NULL,NULL,NULL),
(2123000, N'Current Deferred Income - *', N'2123-000', @DeferredIncomeClassifiedAsCurrent, @AC2123, @ETB,NULL,NULL,@CustomerCD),
(2124000, N'Accrued Expenses - *', N'2124-000', @AccrualsClassifiedAsCurrent, @AC2124, @ETB,NULL,NULL,@SupplierCD),
(2125000, N'Employee Benefit Accruals - *', N'2125-000', @ShorttermEmployeeBenefitsAccruals, @AC2125, @ETB,NULL,NULL,@EmployeeCD),
(2126101, N'Current value added tax payables', N'2126-101', @CurrentValueAddedTaxPayables, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126102, N'Current excise tax payables', N'2126-102', @CurrentExciseTaxPayables, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126103, N'Current Social Security payables', N'2126-103', @CurrentSocialSecurityPayablesExtension, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126104, N'Provident fund payable', N'2126-104', @ProvidentFundPayableExtension, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126105, N'Employee Income Tax Payable', N'2126-105', @CurrentEmployeeIncomeTaxPayablesExtension, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126106, N'Witholding Tax Payable', N'2126-106', @WithholdingTaxPayableExtension, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126107, N'Cost Sharing Payable', N'2126-107', @CostSharingPayableExtension, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2126201, N'Current value added tax payables', N'2126-201', @CurrentValueAddedTaxPayables, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126202, N'Current excise tax payables', N'2126-202', @CurrentExciseTaxPayables, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126203, N'Current Social Security payables', N'2126-203', @CurrentSocialSecurityPayablesExtension, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126204, N'Provident fund payable', N'2126-204', @ProvidentFundPayableExtension, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126205, N'Employee Income Tax Payable', N'2126-205', @CurrentEmployeeIncomeTaxPayablesExtension, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126206, N'Witholding Tax Payable', N'2126-206', @WithholdingTaxPayableExtension, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126207, N'Cost Sharing Payable', N'2126-207', @CostSharingPayableExtension, @AC2126, @ETB,@106C_RealEstate,NULL,NULL),
(2126900, N'Dividend Tax Payable', N'2126-900', @DividendTaxPayableExtension, @AC2126, @ETB,@106C_TradingHO,NULL,NULL),
(2127200, N'Retention Payable - *', N'2127-200', @CurrentRetentionPayables, @AC2127, @ETB,@106C_RealEstate,NULL,@SupplierCD),
(2130001, N'Profit tax payable', N'2130-001', @CurrentTaxLiabilitiesCurrent, @AC2130, @ETB,NULL,NULL,NULL),
(3100001, N'Paid up capital', N'3100-001', @IssuedCapital, @AC3100, @ETB,NULL,NULL,NULL),
(3200000, N'Retained Earning - Soreti', N'3200-000', @RetainedEarnings, @AC3200, @ETB,@106C_TradingHO,NULL,NULL),
(3200100, N'P/L - Export', N'3200-100', @RetainedEarnings, @AC3200, @ETB,@106C_TradingHO,NULL,NULL),
(3200200, N'P/L - Import', N'3200-200', @RetainedEarnings, @AC3200, @ETB,@106C_TradingHO,NULL,NULL),
(3200300, N'P/L - Agro Processing', N'3200-300', @RetainedEarnings, @AC3200, @ETB,@106C_TradingHO,NULL,NULL),
(3200400, N'P/L - Manufacturing', N'3200-400', @RetainedEarnings, @AC3200, @ETB,@106C_TradingHO,NULL,NULL),
(3200500, N'P/L - Local Trade', N'3200-500', @RetainedEarnings, @AC3200, @ETB,@106C_TradingHO,NULL,NULL),
(3200600, N'P/L - Real Estate', N'3200-600', @RetainedEarnings, @AC3200, @ETB,@106C_RealEstate,NULL,NULL),
(3400001, N'Revaluation surplus', N'3400-001', @RevaluationSurplus, @AC3400, @ETB,@106C_TradingHO,NULL,NULL),
(3400008, N'Reserve of gains and losses on financial assets measured at fair value through other comprehensive income', N'3400-008', @ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome, @AC3400, @ETB,@106C_TradingHO,NULL,NULL),
(3400015, N'Reserve for non-current assets or disposal groups held for sale', N'3400-015', @AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale, @AC3400, @ETB,@106C_TradingHO,NULL,NULL),
(3400018, N'Reserve for catastrophe', N'3400-018', @ReserveForCatastrophe, @AC3400, @ETB,@106C_TradingHO,NULL,NULL),
(3400024, N'Statutory reserve', N'3400-024', @StatutoryReserve, @AC3400, @ETB,@106C_TradingHO,NULL,NULL),
(4110111, N'Revenue from Exported Grains - *', N'4110-111', @RevenueFromSaleOfGoods, @AC4110, @USD,@106C_ExportCostofSales,NULL,NULL),
(4110112, N'Revenue from Imported Merchandise - *', N'4110-112', @RevenueFromSaleOfGoods, @AC4110, @ETB,@106C_ImportCostofsales,NULL,NULL),
(4110113, N'Revenue from Processed Oil - *', N'4110-113', @RevenueFromSaleOfGoods, @AC4110, @ETB,@106C_AgroProcessingCostofSales,NULL,NULL),
(4110114, N'Revenue from Assembled Minidor - *', N'4110-114', @RevenueFromSaleOfGoods, @AC4110, @ETB,@106C_ManufacturingCostofsales,NULL,NULL),
(4110115, N'Revenue from Local Grains - *', N'4110-115', @RevenueFromSaleOfGoods, @AC4110, @ETB,@106C_LocalTradeCostofSales,NULL,NULL),
(4120121, N'Revenue from Soreti Mall - *', N'4120-121', @RevenueFromRenderingOfServices, @AC4120, @ETB,@106C_SoretiMallCostofsales,NULL,NULL),
(4120122, N'Revenue from A.A. Building - *', N'4120-122', @RevenueFromRenderingOfServices, @AC4120, @ETB,@106C_AABuildingCostofsales,NULL,NULL),
(4130001, N'Interest Income - Source 1', N'4130-001', @RevenueFromInterest, @AC4130, @ETB,NULL,NULL,NULL),
(4140001, N'Dividend Income - Source 1', N'4140-001', @RevenueFromDividends, @AC4140, @ETB,NULL,NULL,NULL),
(4190001, N'Other revenue - Source 1', N'4190-001', @OtherRevenue, @AC4190, @ETB,NULL,NULL,NULL),
(4200001, N'Other income - Source 1', N'4200-001', @OtherIncome, @AC4200, @ETB,NULL,NULL,NULL),
(5110301, N'RM - Adama factory', N'5110-301', @RawMaterialsAndConsumablesUsed, @AC5110, NULL,NULL,NULL,NULL),
(5120000, N'Cost of Merchandise Sold - Imported Merchandise', N'5120-000', @CostOfMerchandiseSold, @AC5120, NULL,@106C_ImportCostofsales,NULL,NULL),
(5131000, N'Insurance Expense - *', N'5131-000', @InsuranceExpense, @AC5131, NULL,NULL,NULL,NULL),
(5132000, N'Professional Fees Expense - *', N'5132-000', @ProfessionalFeesExpense, @AC5132, NULL,NULL,NULL,NULL),
(5133000, N'Transportation expense - *', N'5133-000', @TransportationExpense, @AC5133, @ETB,NULL,NULL,NULL),
(5141000, N'Wages and Salaries - *', N'5141-000', @WagesAndSalaries, @AC5141, @ETB,NULL,NULL,NULL),
(5143000, N'Employee bonus', N'5143-000', @EmployeeBonusExtension, @AC5143, @ETB,NULL,NULL,NULL),
(7100001, N'Cash control', N'7100-001', @CashControlExtension, @AC7100, @ETB,NULL,NULL,NULL),
(7100002, N'Trading control', N'7100-002', @TradingControlExtension, @AC7100, @ETB,NULL,NULL,NULL),
(7100003, N'Payroll control', N'7100-003', @PayrollControlExtension, @AC7100, @ETB,NULL,NULL,NULL),
(7100004, N'Other document control', N'7100-004', @OtherControlExtension, @AC7100, @ETB,NULL,NULL,NULL)

UPDATE @Accounts SET CenterId = @106C_TradingHO,ContractId = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Cash 1') WHERE [Index] = 1111101;
UPDATE @Accounts SET CenterId = @106C_RealEstate, ContractId = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank 1') WHERE [Index] = 1112201;
UPDATE @Accounts SET CenterId = @106C_TradingHO, ContractId = (SELECT [Id] FROM dbo.Contracts WHERE [Name] = N'Bank 2') WHERE [Index] = 1112102;


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