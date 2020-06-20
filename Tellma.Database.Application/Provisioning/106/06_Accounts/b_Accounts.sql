INSERT INTO @Accounts([Index], [Name], [Code], [AccountTypeId], [ClassificationId]) VALUES
(1111001, N'Cash Account 1', N'1111-001', @CashOnHand, @AC1111000),
(1112001, N'Bank Account 1', N'1112-001', @BalancesWithBanks, @AC1112000),
(1112002, N'Bank Account 2', N'1112-002', @BalancesWithBanks, @AC1112000),
(1121000, N'Current trade receivable - SP', N'1121-000', @CurrentTradeReceivables, @AC1121000),
(1121001, N'A/R, Customer 1', N'1121-001', @CurrentTradeReceivables, @AC1121000),
(1121002, N'A/R, Customer 2', N'1121-002', @CurrentTradeReceivables, @AC1121000),
(1121999, N'ECL Allowance, Current trade receivable', N'1121-999', @CurrentTradeReceivables, @AC1121000),
(1122000, N'Current receivable from related parties - SP', N'1122-000', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122000),
(1122001, N'Related Party 1', N'1122-001', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122000),
(1122002, N'Related Party 2', N'1122-002', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122000),
(1122999, N'ECL Allowance, Related Parties receivable', N'1122-999', @TradeAndOtherCurrentReceivablesDueFromRelatedParties, @AC1122000),
(1123000, N'Current prepayments - SP', N'1123-000', @CurrentPrepayments, @AC1123000),
(1123001, N'Current prepayments - Supplier 1', N'1123-001', @CurrentPrepayments, @AC1123000),
(1123002, N'Current prepayments - Supplier 2', N'1123-002', @CurrentPrepayments, @AC1123000),
(1124000, N'Current accrued income - SP', N'1124-000', @CurrentAccruedIncome, @AC1124000),
(1124001, N'Current accrued income - Customer 1', N'1124-001', @CurrentAccruedIncome, @AC1124000),
(1124002, N'Current accrued income - Customer 2', N'1124-002', @CurrentAccruedIncome, @AC1124000),
(1125000, N'Current billed but not received - SP', N'1125-000', @CurrentBilledButNotReceivedExtension, @AC1125000),
(1125001, N'Current billed but not received - Supplier 1', N'1125-001', @CurrentBilledButNotReceivedExtension, @AC1125000),
(1125002, N'Current billed but not received - Supplier 2', N'1125-002', @CurrentBilledButNotReceivedExtension, @AC1125000),
(1126001, N'VAT Receivable', N'1126-001', @CurrentValueAddedTaxReceivables, @AC1126000),
(1126002, N'WT Receivable', N'1126-002', @WithholdingTaxReceivablesExtension, @AC1126000),
(1127000, N'Rental Receivables - SP', N'1127-000', @CurrentReceivablesFromRentalOfProperties, @AC1127000),
(1127001, N'Rental Receivable - Tenant 1', N'1127-001', @CurrentReceivablesFromRentalOfProperties, @AC1127000),
(1127002, N'Rental Receivable - Tenant 2', N'1127-002', @CurrentReceivablesFromRentalOfProperties, @AC1127000),
(1141000, N'Staff Debtors - SP', N'1141-000', @StaffDebtorsExtension, @AC1141000),
(1141001, N'Employee 1', N'1141-001', @StaffDebtorsExtension, @AC1141000),
(1141002, N'Employee 2', N'1141-002', @StaffDebtorsExtension, @AC1141000),
(1141999, N'Allowance for ECL, Staff debtors', N'1141-999', @StaffDebtorsExtension, @AC1141000),
(1142000, N'Sundry Debtors - SP', N'1142-000', @SundryDebtorsExtension, @AC1142000),
(1142001, N'Sundry 1', N'1142-001', @SundryDebtorsExtension, @AC1142000),
(1142002, N'Sundry 2', N'1142-002', @SundryDebtorsExtension, @AC1142000),
(1142999, N'Allowance for ECL, Sundry debtors', N'1142-999', @SundryDebtorsExtension, @AC1142000),
(1171100, N'Raw Grains - SP', N'1171-100', @RawMaterials, @AC1171000),
(1171101, N'Raw Grains - Type 1', N'1171-101', @RawMaterials, @AC1171000),
(1171102, N'Raw Grains - Type 2', N'1171-102', @RawMaterials, @AC1171000),
(1171200, N'Vehicle Components - SP', N'1171-200', @RawMaterials, @AC1171000),
(1171201, N'Vehicle Components - Type 1', N'1171-201', @RawMaterials, @AC1171000),
(1171202, N'Vehicle Components - Type 2', N'1171-202', @RawMaterials, @AC1171000),
(1172000, N'Merchandise - SP', N'1172-000', @Merchandise, @AC1172000),
(1172001, N'Medicine', N'1172-001', @Merchandise, @AC1172000),
(1172002, N'Construction Materials', N'1172-002', @Merchandise, @AC1172000),
(1172003, N'Spare Parts (for Sale)', N'1172-003', @Merchandise, @AC1172000),
(1173000, N'Work In Progress - SP', N'1173-000', @WorkInProgress, @AC1173000),
(1173001, N'Grains in Process', N'1173-001', @WorkInProgress, @AC1173000),
(1173002, N'Oils in Process', N'1173-002', @WorkInProgress, @AC1173000),
(1173003, N'Vehicles in Process', N'1173-003', @WorkInProgress, @AC1173000),
(1174000, N'Finished Goods - SP', N'1174-000', @FinishedGoods, @AC1174000),
(1174001, N'Cleaned Grains', N'1174-001', @FinishedGoods, @AC1174000),
(1174002, N'Processed Oil', N'1174-002', @FinishedGoods, @AC1174000),
(1174003, N'Assembled Vehicles', N'1174-003', @FinishedGoods, @AC1174000),
(1175000, N'Packaging and Storage Materials - SP', N'1175-000', @CurrentPackagingAndStorageMaterials, @AC1175000),
(1176000, N'Spare Parts - SP', N'1176-000', @SpareParts, @AC1176000),
(1176001, N'Spare Parts (Not for sale)', N'1176-001', @SpareParts, @AC1176000),
(1177100, N'Import shipments - SP', N'1177-100', @CurrentInventoriesInTransit, @AC1177000),
(1177101, N'Import shipment - LC#1', N'1177-101', @CurrentInventoriesInTransit, @AC1177000),
(1177102, N'Import shipment - LC#2', N'1177-102', @CurrentInventoriesInTransit, @AC1177000),
(1177200, N'Export shipments - SP', N'1177-200', @CurrentInventoriesInTransit, @AC1177000),
(1177201, N'Export Shipment - Permit #1', N'1177-201', @CurrentInventoriesInTransit, @AC1177000),
(1177202, N'Export Shipment - Permit #2', N'1177-202', @CurrentInventoriesInTransit, @AC1177000),
(1211100, N'Land - SP', N'1211-100', @Land, @AC1211000),
(1211101, N'Land 1', N'1211-101', @Land, @AC1211000),
(1211102, N'Land 2', N'1211-102', @Land, @AC1211000),
(1211200, N'Buildings - SP', N'1211-200', @Buildings, @AC1211000),
(1211201, N'Building 1', N'1211-201', @Buildings, @AC1211000),
(1211202, N'Building 2', N'1211-202', @Buildings, @AC1211000),
(1212001, N'Machineries', N'1212-001', @Machinery, @AC1212000),
(1212002, N'Machineries, acc. depreciation', N'1212-002', @Machinery, @AC1212000),
(2121000, N'Current Trade Payables - SP', N'2121-000', @TradeAndOtherCurrentPayablesToTradeSuppliers, @AC2121000),
(2121001, N'A/P - Supplier 1', N'2121-001', @TradeAndOtherCurrentPayablesToTradeSuppliers, @AC2121000),
(2121002, N'A/P - Supplier 2', N'2121-002', @TradeAndOtherCurrentPayablesToTradeSuppliers, @AC2121000),
(2122000, N'Current payables to related parties - SP', N'2122-000', @TradeAndOtherCurrentPayablesToRelatedParties, @AC2122000),
(2122001, N'Related Party 1', N'2122-001', @TradeAndOtherCurrentPayablesToRelatedParties, @AC2122000),
(2122002, N'Related Party 2', N'2122-002', @TradeAndOtherCurrentPayablesToRelatedParties, @AC2122000),
(2123000, N'Current Deferred Income - SP', N'2123-000', @DeferredIncomeClassifiedAsCurrent, @AC2123000),
(2123001, N'Deferred Income - Customer 1', N'2123-001', @DeferredIncomeClassifiedAsCurrent, @AC2123000),
(2123002, N'Deferred Income - Customer 2', N'2123-002', @DeferredIncomeClassifiedAsCurrent, @AC2123000),
(2124000, N'Accrued Expenses - SP', N'2124-000', @AccrualsClassifiedAsCurrent, @AC2124000),
(2124001, N'Accrued Expenses - Supplier 1', N'2124-001', @AccrualsClassifiedAsCurrent, @AC2124000),
(2124002, N'Accrued Expenses - Supplier 2', N'2124-002', @AccrualsClassifiedAsCurrent, @AC2124000),
(2125000, N'Employee Benefit Accruals - SP', N'2125-000', @ShorttermEmployeeBenefitsAccruals, @AC2125000),
(2126001, N'Current value added tax payables', N'2126-001', @CurrentValueAddedTaxPayables, @AC2126000),
(2126002, N'Current excise tax payables', N'2126-002', @CurrentExciseTaxPayables, @AC2126000),
(2126003, N'Current Social Security payables', N'2126-003', @CurrentSocialSecurityPayablesExtension, @AC2126000),
(2126004, N'Provident fund payable', N'2126-004', @ProvidentFundPayableExtension, @AC2126000),
(2126005, N'Employee Income Tax Payable', N'2126-005', @CurrentEmployeeIncomeTaxPayablesExtension, @AC2126000),
(2126006, N'Witholding Tax Payable', N'2126-006', @WithholdingTaxPayableExtension, @AC2126000),
(2126007, N'Cost Sharing Payable', N'2126-007', @CostSharingPayableExtension, @AC2126000),
(2126008, N'Dividend Tax Payable', N'2126-008', @DividendTaxPayableExtension, @AC2126000),
(2127001, N'Retention Payable - Contractor 1', N'2127-001', @CurrentRetentionPayables, @AC2127000),
(2127002, N'Retention Payable - Contractor 2', N'2127-002', @CurrentRetentionPayables, @AC2127000),
(2130001, N'Profit tax payable', N'2130-001', @CurrentTaxLiabilitiesCurrent, @AC2130000),
(3100001, N'Paid up capital', N'3100-001', @IssuedCapital, @AC3100000),
(3200001, N'Retained Earning - Soreti', N'3200-001', @RetainedEarnings, @AC3200000),
(3400001, N'Revaluation surplus', N'3400-001', @RevaluationSurplus, @AC3400000),
(3400008, N'Reserve of gains and losses on financial assets measured at fair value through other comprehensive income', N'3400-008', @ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome, @AC3400000),
(3400015, N'Reserve for non-current assets or disposal groups held for sale', N'3400-015', @AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale, @AC3400000),
(3400018, N'Reserve for catastrophe', N'3400-018', @ReserveForCatastrophe, @AC3400000),
(3400024, N'Statutory reserve', N'3400-024', @StatutoryReserve, @AC3400000),
(4110111, N'Revenue from Exported Grains - SP', N'4110-111', @RevenueFromSaleOfGoods, @AC4110000),
(4110112, N'Revenue from Processed Oil - SP', N'4110-112', @RevenueFromSaleOfGoods, @AC4110000),
(4110113, N'Revenue from Assembled Minidor - SP', N'4110-113', @RevenueFromSaleOfGoods, @AC4110000),
(4110114, N'Revenue from Imported Merchandise - SP', N'4110-114', @RevenueFromSaleOfGoods, @AC4110000),
(4110115, N'Revenue from Local Grains - SP', N'4110-115', @RevenueFromSaleOfGoods, @AC4110000),
(4110119, N'Revenue from other Trading Activities', N'4110-119', @RevenueFromSaleOfGoods, @AC4110000),
(4120121, N'Revenue from Soreti Mall - SP', N'4120-121', @RevenueFromRenderingOfServices, @AC4120000),
(4120122, N'Revenue from A.A. Building - SP', N'4120-122', @RevenueFromRenderingOfServices, @AC4120000),
(4130001, N'Interest Income - Source 1', N'4130-001', @RevenueFromInterest, @AC4130000),
(4140001, N'Dividend Income - Source 1', N'4140-001', @RevenueFromDividends, @AC4140000),
(4190001, N'Other revenue - Source 1', N'4190-001', @OtherRevenue, @AC4190000),
(4200001, N'Other income - Source 1', N'4200-001', @OtherIncome, @AC4200000),
(5110301, N'RM - Adama factory', N'5110-301', @RawMaterialsAndConsumablesUsed, @AC5110000),
(5131121, N'Insurance Expense - Soreti Mall', N'5131-121', @InsuranceExpense, @AC5131000),
(5131122, N'Insurance Expense - Bole HQ', N'5131-122', @InsuranceExpense, @AC5131000),
(5131123, N'Insurance Expense - Adama Factory', N'5131-123', @InsuranceExpense, @AC5131000),
(5131124, N'Insurance Expense - Minidor Factory', N'5131-124', @InsuranceExpense, @AC5131000),
(5132121, N'Professional Fees Expense - Soreti Mall', N'5132-121', @ProfessionalFeesExpense, @AC5132000),
(5132122, N'Professional Fees Expense - Bole HQ', N'5132-122', @ProfessionalFeesExpense, @AC5132000),
(5132123, N'Professional Fees Expense - Adama Factory', N'5132-123', @ProfessionalFeesExpense, @AC5132000),
(5132124, N'Professional Fees Expense - Minidor Factory', N'5132-124', @ProfessionalFeesExpense, @AC5132000),
(5141111, N'Wages and Salaries - Exported Grains', N'5141-111', @WagesAndSalaries, @AC5141000),
(5141112, N'Wages and Salaries - Processed Oil', N'5141-112', @WagesAndSalaries, @AC5141000),
(5141113, N'Wages And Salaries - Assembled Minidor', N'5141-113', @WagesAndSalaries, @AC5141000),
(5141114, N'Wages And Salaries - Imported Merchandise', N'5141-114', @WagesAndSalaries, @AC5141000),
(5141115, N'Wages and Salaries - Local Grains', N'5141-115', @WagesAndSalaries, @AC5141000),
(5141119, N'Wages and Salaries - other Trading Activities', N'5141-119', @WagesAndSalaries, @AC5141000),
(5141121, N'Wages and Salaries - Soreti Mall', N'5141-121', @WagesAndSalaries, @AC5141000),
(5141122, N'Wages and Salaries - Addis Ababa Building', N'5141-122', @WagesAndSalaries, @AC5141000),
(5141201, N'Wages and Salaries - Bole HQ - SGNA', N'5141-201', @WagesAndSalaries, @AC5141000),
(5141301, N'Wages and Salaries - Adama Factory', N'5141-301', @WagesAndSalaries, @AC5141000),
(5141302, N'Wages and Salaries - Minidor Factory', N'5141-302', @WagesAndSalaries, @AC5141000),
(5143001, N'Employee bonus', N'5143-001', @EmployeeBonusExtension, @AC5143000),
(7100001, N'Cash control', N'7100-001', @CashControlExtension, @AC7100000),
(7100002, N'Trading control', N'7100-002', @TradingControlExtension, @AC7100000),
(7100003, N'Payroll control', N'7100-003', @PayrollControlExtension, @AC7100000),
(7100004, N'Other document control', N'7100-004', @OtherControlExtension, @AC7100000)

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