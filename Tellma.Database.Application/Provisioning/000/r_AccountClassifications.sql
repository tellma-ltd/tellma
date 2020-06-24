INSERT INTO @AccountClassifications([Index], [ParentIndex], [Code], [Name], [AccountTypeParentId]) VALUES
(1000000, NULL, N'1', N'Assets', @Assets),
(1100000, 1000000, N'11', N'Current assets', @CurrentAssets),
(1110000, 1100000, N'111', N'Cash and cash equivalents', @CashAndCashEquivalents),
(1111000, 1110000, N'1111', N'Cash on hand', @CashOnHand),
(1112000, 1110000, N'1112', N'Balances with banks', @BalancesWithBanks),
(1113000, 1110000, N'1113', N'Cash equivalents', @CashEquivalents),
(1120000, 1100000, N'112', N'Trade and other current receivables', @TradeAndOtherCurrentReceivables),
(1121000, 1120000, N'1121', N'Current trade receivables', @CurrentTradeReceivables),
(1122000, 1120000, N'1122', N'Current receivables due from related parties', @TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(1123000, 1120000, N'1123', N'Current prepayments', @CurrentPrepayments),
(1124000, 1120000, N'1124', N'Current accrued income', @CurrentAccruedIncome),
(1125000, 1120000, N'1125', N'Current billed but not received', @CurrentBilledButNotReceivedExtension),
(1126000, 1120000, N'1126', N'Current receivables from taxes other than income tax', @CurrentReceivablesFromTaxesOtherThanIncomeTax),
(1127000, 1120000, N'1127', N'Current receivables from rental of properties', @CurrentReceivablesFromRentalOfProperties),
(1129000, 1120000, N'1129', N'Other current receivables', @OtherCurrentReceivables),
(1130000, 1100000, N'113', N'Other current non-financial assets', @OtherCurrentNonfinancialAssets),
(1140000, 1100000, N'114', N'Other current financial assets', @OtherCurrentFinancialAssets),
(1141000, 1140000, N'1141', N'Staff Debtors', @StaffDebtorsExtension),
(1142000, 1140000, N'1142', N'Sundry Debtors', @SundryDebtorsExtension),
(1160000, 1100000, N'116', N'Current tax assets, current', @CurrentTaxAssetsCurrent),
(1170000, 1100000, N'117', N'Current inventories', @Inventories),
(1171000, 1170000, N'1171', N'Current raw materials and current production supplies', @CurrentRawMaterialsAndCurrentProductionSupplies),
(1172000, 1170000, N'1172', N'Current merchandise', @Merchandise),
(1173000, 1170000, N'1173', N'Current work in progress', @WorkInProgress),
(1174000, 1170000, N'1174', N'Current finished goods', @FinishedGoods),
(1175000, 1170000, N'1175', N'Current packaging and storage materials', @CurrentPackagingAndStorageMaterials),
(1176000, 1170000, N'1176', N'Current spare parts', @SpareParts),
(1177000, 1170000, N'1177', N'Current inventories in transit', @CurrentInventoriesInTransit),
(1179000, 1170000, N'1179', N'Other current inventories', @OtherInventories),
(1190000, 1100000, N'119', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners', @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners),
(1200000, 1000000, N'12', N'Non-current assets', @NoncurrentAssets),
(1210000, 1200000, N'121', N'Property, plant and equipment', @PropertyPlantAndEquipment),
(1211000, 1210000, N'1211', N'Land and buildings', @LandAndBuildings),
(1212000, 1210000, N'1212', N'Machinery', @Machinery),
(1213000, 1210000, N'1213', N'Vehicles', @Vehicles),
(1214000, 1210000, N'1214', N'Fixtures and fittings', @FixturesAndFittings),
(1215000, 1210000, N'1215', N'Office equipment', @OfficeEquipment),
(1217000, 1210000, N'1217', N'Construction in progress', @ConstructionInProgress),
(1218000, 1210000, N'1218', N'Owner-occupied property measured using investment property fair value model', @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel),
(1219000, 1210000, N'1219', N'Other property, plant and equipment', @OtherPropertyPlantAndEquipment),
(1220000, 1200000, N'122', N'Investment property', @InvestmentProperty),
(1230000, 1200000, N'123', N'Investments accounted for using equity method', @InvestmentAccountedForUsingEquityMethod),
(1240000, 1200000, N'124', N'Investments in subsidiaries, joint ventures and associates', @InvestmentsInSubsidiariesJointVenturesAndAssociates),
(1250000, 1200000, N'125', N'Trade and other non-current receivables', @NoncurrentReceivables),
(1251000, 1250000, N'1251', N'Non-current trade receivables', @NoncurrentTradeReceivables),
(1252000, 1250000, N'1252', N'Non-current receivables due from related parties', @NoncurrentReceivablesDueFromRelatedParties),
(1253000, 1250000, N'1253', N'Non-current prepayments', @NoncurrentPrepayments),
(1254000, 1250000, N'1254', N'Non-current accrued income', @NoncurrentAccruedIncome),
(1255000, 1250000, N'1255', N'Non-current receivables from taxes other than income tax', @NoncurrentReceivablesFromTaxesOtherThanIncomeTax),
(1256000, 1250000, N'1256', N'Non-current receivables from rental of properties', @NoncurrentReceivablesFromRentalOfProperties),
(1257000, 1250000, N'1257', N'Other non-current receivables', @OtherNoncurrentReceivables),
(1260000, 1200000, N'126', N'Deferred tax assets', @DeferredTaxAssets),
(1270000, 1200000, N'127', N'Other non-current financial assets', @OtherNoncurrentFinancialAssets),
(1280000, 1200000, N'128', N'Other non-current non-financial assets', @OtherNoncurrentNonfinancialAssets),
(2000000, NULL, N'2', N'Liabilities', @Liabilities),
(2100000, 2000000, N'21', N'Current liabilities', @CurrentLiabilities),
(2110000, 2100000, N'211', N'Current provisions', @CurrentProvisions),
(2111000, 2110000, N'2111', N'Current provisions for employee benefits', @CurrentProvisionsForEmployeeBenefits),
(2112000, 2110000, N'2112', N'Other current provisions', @OtherShorttermProvisions),
(2120000, 2100000, N'212', N'Trade and other current payables', @TradeAndOtherCurrentPayables),
(2121000, 2120000, N'2121', N'Current trade payables', @TradeAndOtherCurrentPayablesToTradeSuppliers),
(2122000, 2120000, N'2122', N'Current payables to related parties', @TradeAndOtherCurrentPayablesToRelatedParties),
(2123000, 2120000, N'2123', N'Deferred income classified as current', @DeferredIncomeClassifiedAsCurrent),
(2124000, 2120000, N'2124', N'Accruals classified as current', @AccrualsClassifiedAsCurrent),
(2125000, 2120000, N'2125', N'Short-term employee benefits accruals', @ShorttermEmployeeBenefitsAccruals),
(2126000, 2120000, N'2126', N'Current payables on social security and taxes other than income tax', @CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax),
(2127000, 2120000, N'2127', N'Current retention payables', @CurrentRetentionPayables),
(2128000, 2120000, N'2128', N'Other current payables', @OtherCurrentPayables),
(2130000, 2100000, N'2130', N'Current tax liabilities, current', @CurrentTaxLiabilitiesCurrent),
(2140000, 2100000, N'2140', N'Other current financial liabilities', @OtherCurrentFinancialLiabilities),
(2150000, 2100000, N'2150', N'Other current non-financial liabilities', @OtherCurrentNonfinancialLiabilities),
(2160000, 2100000, N'2160', N'Liabilities included in disposal groups classified as held for sale', @LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale),
(2200000, 2000000, N'22', N'Non-current liabilities', @NoncurrentLiabilities),
(2210000, 2200000, N'221', N'Non-current provisions', @NoncurrentProvisions),
(2211000, 2210000, N'2211', N'Non-current provisions for employee benefits', @NoncurrentProvisionsForEmployeeBenefits),
(2212000, 2210000, N'2212', N'Other non-current provisions', @OtherLongtermProvisions),
(2220000, 2200000, N'222', N'Trade and other non-current payables', @NoncurrentPayables),
(2221000, 2220000, N'2221', N'Deferred tax liabilities', @DeferredTaxLiabilities),
(2222000, 2220000, N'2222', N'Current tax liabilities, non-current', @CurrentTaxLiabilitiesNoncurrent),
(2223000, 2220000, N'2223', N'Other non-current financial liabilities', @OtherNoncurrentFinancialLiabilities),
(2224000, 2220000, N'2224', N'Other non-current non-financial liabilities', @OtherNoncurrentNonfinancialLiabilities),
(3000000, NULL, N'3', N'Equity', @Equity),
(3100000, 3000000, N'3100', N'Issued capital', @IssuedCapital),
(3200000, 3000000, N'3200', N'Retained earnings', @RetainedEarnings),
(3400000, 3000000, N'3400', N'Other reserves', @OtherReserves),
(4100000, NULL, N'41', N'Revenue', @Revenue),
(4110000, 4100000, N'4110', N'Revenue from sale of goods', @RevenueFromSaleOfGoods),
(4120000, 4100000, N'4120', N'Revenue from rendering of services', @RevenueFromRenderingOfServices),
(4130000, 4100000, N'4130', N'Interest income', @RevenueFromInterest),
(4140000, 4100000, N'4140', N'Dividend income', @RevenueFromDividends),
(4190000, 4100000, N'4190', N'Other revenue', @OtherRevenue),
(4200000, 4100000, N'4200', N'Other income', @OtherIncome),
(5100000, 5000000, N'51', N'Expenses by nature', @ExpenseByNature),
(5110000, 5000000, N'5110', N'Raw materials and consumables used', @RawMaterialsAndConsumablesUsed),
(5120000, 5000000, N'512', N'Cost of merchandise sold', @CostOfMerchandiseSold),
(5130000, 5000000, N'513', N'Services expense', @ServicesExpense),
(5131000, 5300000, N'5131', N'Insurance expense', @InsuranceExpense),
(5132000, 5300000, N'5132', N'Professional fees expense', @ProfessionalFeesExpense),
(5133000, 5300000, N'5133', N'Transportation expense', @TransportationExpense),
(5134000, 5300000, N'5134', N'Bank and similar charges', @BankAndSimilarCharges),
(5135000, 5300000, N'5135', N'Travel expense', @TravelExpense),
(5136000, 5300000, N'5136', N'Communication expense', @CommunicationExpense),
(5137000, 5300000, N'5137', N'Utilities expense', @UtilitiesExpense),
(5138000, 5300000, N'5138', N'Advertising expense', @AdvertisingExpense),
(5140000, 5000000, N'514', N'Employee benefits expense', @EmployeeBenefitsExpense),
(5141000, 5410000, N'5141', N'Wages and salaries', @WagesAndSalaries),
(5142000, 5410000, N'5142', N'Social security contributions', @SocialSecurityContributions),
(5143000, 5410000, N'5143', N'Other short-term employee benefits', @OtherShorttermEmployeeBenefits),
(5150000, 5100000, N'515', N'Depreciation and amortisation expense', @DepreciationAndAmortisationExpense),
(5151000, 5150000, N'5151', N'Depreciation expense', @DepreciationExpense),
(5152000, 5150000, N'5152', N'Amortisation expense', @AmortisationExpense),
(5160000, 5100000, N'516', N'Reversal of impairment loss (impairment loss) recognised in profit or loss', @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss),
(5190000, 5100000, N'519', N'Other expenses', @OtherExpenseByNature),
(5200000, 5000000, N'52', N'Other gains (losses)', @OtherGainsLosses),
(5210000, 6000000, N'521', N'Gain (loss) on disposal of property, plant and equipment', @GainLossOnDisposalOfPropertyPlantAndEquipmentExtension),
(5220000, 6000000, N'522', N'Gain (loss) on foreign exchange', @GainLossOnForeignExchangeExtension),
(7000000, NULL, N'7', N'Control Accounts', @ControlAccountsExtension),
(7100000, 7000000, N'7100', N'Document Control', @DocumentControlExtension),
(7200000, 7000000, N'7200', N'Final account control', @FinalAccountsControlExtension)

EXEC [api].[AccountClassifications__Save] --  N'cash-and-cash-equivalents',
	@Entities = @AccountClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting AccountClassifications: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

DECLARE @AC INT = NULL;

-- DECLARATIONS
DECLARE @AC1000000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1');
DECLARE @AC1100000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'11');
DECLARE @AC1110000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'111');
DECLARE @AC1111000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1111');
DECLARE @AC1112000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1112');
DECLARE @AC1113000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1113');
DECLARE @AC1120000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'112');
DECLARE @AC1121000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1121');
DECLARE @AC1122000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1122');
DECLARE @AC1123000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1123');
DECLARE @AC1124000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1124');
DECLARE @AC1125000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1125');
DECLARE @AC1126000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1126');
DECLARE @AC1127000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1127');
DECLARE @AC1129000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1129');
DECLARE @AC1130000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'113');
DECLARE @AC1140000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'114');
DECLARE @AC1141000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1141');
DECLARE @AC1142000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1142');
DECLARE @AC1160000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'116');
DECLARE @AC1170000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'117');
DECLARE @AC1171000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1171');
DECLARE @AC1172000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1172');
DECLARE @AC1173000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1173');
DECLARE @AC1174000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1174');
DECLARE @AC1175000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1175');
DECLARE @AC1176000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1176');
DECLARE @AC1177000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1177');
DECLARE @AC1179000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1179');
DECLARE @AC1190000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'119');
DECLARE @AC1200000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'12');
DECLARE @AC1210000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'121');
DECLARE @AC1211000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1211');
DECLARE @AC1212000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1212');
DECLARE @AC1213000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1213');
DECLARE @AC1214000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1214');
DECLARE @AC1215000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1215');
DECLARE @AC1217000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1217');
DECLARE @AC1218000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1218');
DECLARE @AC1219000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1219');
DECLARE @AC1220000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'122');
DECLARE @AC1230000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'123');
DECLARE @AC1240000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'124');
DECLARE @AC1250000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'125');
DECLARE @AC1251000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1251');
DECLARE @AC1252000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1252');
DECLARE @AC1253000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1253');
DECLARE @AC1254000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1254');
DECLARE @AC1255000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1255');
DECLARE @AC1256000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1256');
DECLARE @AC1257000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1257');
DECLARE @AC1260000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'126');
DECLARE @AC1270000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'127');
DECLARE @AC1280000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'128');
DECLARE @AC2000000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2');
DECLARE @AC2100000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'21');
DECLARE @AC2110000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'211');
DECLARE @AC2111000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2111');
DECLARE @AC2112000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2112');
DECLARE @AC2120000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'212');
DECLARE @AC2121000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2121');
DECLARE @AC2122000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2122');
DECLARE @AC2123000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2123');
DECLARE @AC2124000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2124');
DECLARE @AC2125000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2125');
DECLARE @AC2126000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2126');
DECLARE @AC2127000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2127');
DECLARE @AC2128000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2128');
DECLARE @AC2130000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2130');
DECLARE @AC2140000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2140');
DECLARE @AC2150000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2150');
DECLARE @AC2160000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2160');
DECLARE @AC2200000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'22');
DECLARE @AC2210000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'221');
DECLARE @AC2211000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2211');
DECLARE @AC2212000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2212');
DECLARE @AC2220000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'222');
DECLARE @AC2221000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2221');
DECLARE @AC2222000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2222');
DECLARE @AC2223000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2223');
DECLARE @AC2224000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2224');
DECLARE @AC3000000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3');
DECLARE @AC3100000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3100');
DECLARE @AC3200000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3200');
DECLARE @AC3400000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3400');
DECLARE @AC4100000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'41');
DECLARE @AC4110000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4110');
DECLARE @AC4120000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4120');
DECLARE @AC4130000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4130');
DECLARE @AC4140000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4140');
DECLARE @AC4190000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4190');
DECLARE @AC4200000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4200');
DECLARE @AC5100000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'51');
DECLARE @AC5110000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5110');
DECLARE @AC5120000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'512');
DECLARE @AC5130000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'513');
DECLARE @AC5131000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5131');
DECLARE @AC5132000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5132');
DECLARE @AC5133000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5133');
DECLARE @AC5134000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5134');
DECLARE @AC5135000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5135');
DECLARE @AC5136000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5136');
DECLARE @AC5137000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5137');
DECLARE @AC5138000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5138');
DECLARE @AC5140000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'514');
DECLARE @AC5141000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5141');
DECLARE @AC5142000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5142');
DECLARE @AC5143000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5143');
DECLARE @AC5150000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'515');
DECLARE @AC5151000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5151');
DECLARE @AC5152000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5152');
DECLARE @AC5160000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'516');
DECLARE @AC5190000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'519');
DECLARE @AC5200000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'52');
DECLARE @AC5210000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'521');
DECLARE @AC5220000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'522');
DECLARE @AC7000000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7');
DECLARE @AC7100000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7100');
DECLARE @AC7200000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7200');