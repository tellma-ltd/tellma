INSERT INTO @AccountClassifications([Index], [ParentIndex], [Code], [Name], [AccountTypeParentId]) VALUES
(1, NULL, N'1', N'Assets', @Assets),
(11, 1, N'11', N'Current assets', @CurrentAssets),
(111, 11, N'111', N'Cash and cash equivalents', @CashAndCashEquivalents),
(1111, 111, N'1111', N'Cash on hand', @CashOnHand),
(1112, 111, N'1112', N'Balances with banks', @BalancesWithBanks),
(1113, 111, N'1113', N'Cash equivalents', @CashEquivalents),
(112, 11, N'112', N'Trade and other current receivables', @TradeAndOtherCurrentReceivables),
(1121, 112, N'1121', N'Current trade receivables', @CurrentTradeReceivables),
(1122, 112, N'1122', N'Current receivables due from related parties', @TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(1123, 112, N'1123', N'Current prepayments', @CurrentPrepayments),
(1124, 112, N'1124', N'Current accrued income', @CurrentAccruedIncome),
(1125, 112, N'1125', N'Current billed but not received', @CurrentBilledButNotReceivedExtension),
(1126, 112, N'1126', N'Current receivables from taxes other than income tax', @CurrentReceivablesFromTaxesOtherThanIncomeTax),
(1127, 112, N'1127', N'Current receivables from rental of properties', @CurrentReceivablesFromRentalOfProperties),
(1129, 112, N'1129', N'Other current receivables', @OtherCurrentReceivables),
(113, 11, N'113', N'Other current non-financial assets', @OtherCurrentNonfinancialAssets),
(114, 11, N'114', N'Other current financial assets', @OtherCurrentFinancialAssets),
(1141, 114, N'1141', N'Staff Debtors', @StaffDebtorsExtension),
(1142, 114, N'1142', N'Sundry Debtors', @SundryDebtorsExtension),
(116, 11, N'116', N'Current tax assets, current', @CurrentTaxAssetsCurrent),
(117, 11, N'117', N'Current inventories', @Inventories),
(1171, 117, N'1171', N'Current raw materials and current production supplies', @CurrentRawMaterialsAndCurrentProductionSupplies),
(1172, 117, N'1172', N'Current merchandise', @Merchandise),
(1173, 117, N'1173', N'Current work in progress', @WorkInProgress),
(1174, 117, N'1174', N'Current finished goods', @FinishedGoods),
(1175, 117, N'1175', N'Current packaging and storage materials', @CurrentPackagingAndStorageMaterials),
(1176, 117, N'1176', N'Current spare parts', @SpareParts),
(1177, 117, N'1177', N'Current inventories in transit', @CurrentInventoriesInTransit),
(1179, 117, N'1179', N'Other current inventories', @OtherInventories),
(119, 11, N'119', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners', @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners),
(12, 1, N'12', N'Non-current assets', @NoncurrentAssets),
(121, 12, N'121', N'Property, plant and equipment', @PropertyPlantAndEquipment),
(1211, 121, N'1211', N'Land and buildings', @LandAndBuildings),
(1212, 121, N'1212', N'Machinery', @Machinery),
(1213, 121, N'1213', N'Vehicles', @Vehicles),
(1214, 121, N'1214', N'Fixtures and fittings', @FixturesAndFittings),
(1215, 121, N'1215', N'Office equipment', @OfficeEquipment),
(1217, 121, N'1217', N'Construction in progress', @ConstructionInProgress),
(1218, 121, N'1218', N'Owner-occupied property measured using investment property fair value model', @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel),
(1219, 121, N'1219', N'Other property, plant and equipment', @OtherPropertyPlantAndEquipment),
(122, 12, N'122', N'Investment property', @InvestmentProperty),
(123, 12, N'123', N'Investments accounted for using equity method', @InvestmentAccountedForUsingEquityMethod),
(124, 12, N'124', N'Investments in subsidiaries, joint ventures and associates', @InvestmentsInSubsidiariesJointVenturesAndAssociates),
(125, 12, N'125', N'Trade and other non-current receivables', @NoncurrentReceivables),
(1251, 125, N'1251', N'Non-current trade receivables', @NoncurrentTradeReceivables),
(1252, 125, N'1252', N'Non-current receivables due from related parties', @NoncurrentReceivablesDueFromRelatedParties),
(1253, 125, N'1253', N'Non-current prepayments', @NoncurrentPrepayments),
(1254, 125, N'1254', N'Non-current accrued income', @NoncurrentAccruedIncome),
(1255, 125, N'1255', N'Non-current receivables from taxes other than income tax', @NoncurrentReceivablesFromTaxesOtherThanIncomeTax),
(1256, 125, N'1256', N'Non-current receivables from rental of properties', @NoncurrentReceivablesFromRentalOfProperties),
(1257, 125, N'1257', N'Other non-current receivables', @OtherNoncurrentReceivables),
(126, 12, N'126', N'Deferred tax assets', @DeferredTaxAssets),
(127, 12, N'127', N'Other non-current financial assets', @OtherNoncurrentFinancialAssets),
(128, 12, N'128', N'Other non-current non-financial assets', @OtherNoncurrentNonfinancialAssets),
(2, NULL, N'2', N'Liabilities', @Liabilities),
(21, 2, N'21', N'Current liabilities', @CurrentLiabilities),
(211, 21, N'211', N'Current provisions', @CurrentProvisions),
(2111, 211, N'2111', N'Current provisions for employee benefits', @CurrentProvisionsForEmployeeBenefits),
(2112, 211, N'2112', N'Other current provisions', @OtherShorttermProvisions),
(212, 21, N'212', N'Trade and other current payables', @TradeAndOtherCurrentPayables),
(2121, 212, N'2121', N'Current trade payables', @TradeAndOtherCurrentPayablesToTradeSuppliers),
(2122, 212, N'2122', N'Current payables to related parties', @TradeAndOtherCurrentPayablesToRelatedParties),
(2123, 212, N'2123', N'Deferred income classified as current', @DeferredIncomeClassifiedAsCurrent),
(2124, 212, N'2124', N'Accruals classified as current', @AccrualsClassifiedAsCurrent),
(2125, 212, N'2125', N'Short-term employee benefits accruals', @ShorttermEmployeeBenefitsAccruals),
(2126, 212, N'2126', N'Current payables on social security and taxes other than income tax', @CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax),
(2127, 212, N'2127', N'Current retention payables', @CurrentRetentionPayables),
(2128, 212, N'2128', N'Other current payables', @OtherCurrentPayables),
(2130, 21, N'2130', N'Current tax liabilities, current', @CurrentTaxLiabilitiesCurrent),
(2140, 21, N'2140', N'Other current financial liabilities', @OtherCurrentFinancialLiabilities),
(2150, 21, N'2150', N'Other current non-financial liabilities', @OtherCurrentNonfinancialLiabilities),
(2160, 21, N'2160', N'Liabilities included in disposal groups classified as held for sale', @LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale),
(22, 2, N'22', N'Non-current liabilities', @NoncurrentLiabilities),
(221, 22, N'221', N'Non-current provisions', @NoncurrentProvisions),
(2211, 221, N'2211', N'Non-current provisions for employee benefits', @NoncurrentProvisionsForEmployeeBenefits),
(2212, 221, N'2212', N'Other non-current provisions', @OtherLongtermProvisions),
(222, 22, N'222', N'Trade and other non-current payables', @NoncurrentPayables),
(2221, 222, N'2221', N'Deferred tax liabilities', @DeferredTaxLiabilities),
(2222, 222, N'2222', N'Current tax liabilities, non-current', @CurrentTaxLiabilitiesNoncurrent),
(2223, 222, N'2223', N'Other non-current financial liabilities', @OtherNoncurrentFinancialLiabilities),
(2224, 222, N'2224', N'Other non-current non-financial liabilities', @OtherNoncurrentNonfinancialLiabilities),
(3, NULL, N'3', N'Equity', @Equity),
(3100, 3, N'3100', N'Issued capital', @IssuedCapital),
(3200, 3, N'3200', N'Retained earnings', @RetainedEarnings),
(3400, 3, N'3400', N'Other reserves', @OtherReserves),
(4, NULL, N'4', N'Revenue', @Revenue),
(4110, 4, N'4110', N'Revenue from sale of goods', @RevenueFromSaleOfGoods),
(4120, 4, N'4120', N'Revenue from rendering of services', @RevenueFromRenderingOfServices),
(4130, 4, N'4130', N'Interest income', @RevenueFromInterest),
(4140, 4, N'4140', N'Dividend income', @RevenueFromDividends),
(4190, 4, N'4190', N'Other revenue', @OtherRevenue),
(4200, 4, N'4200', N'Other income', @OtherIncome),
(5, NULL, N'5', N'Expenses', NULL),
(51, 5, N'51', N'Expenses by nature', @ExpenseByNature),
(5110, 51, N'5110', N'Raw materials and consumables used', @RawMaterialsAndConsumablesUsed),
(5120, 51, N'5120', N'Cost of merchandise sold', @CostOfMerchandiseSold),
(513, 51, N'513', N'Services expense', @ServicesExpense),
(5131, 513, N'5131', N'Insurance expense', @InsuranceExpense),
(5132, 513, N'5132', N'Professional fees expense', @ProfessionalFeesExpense),
(5133, 513, N'5133', N'Transportation expense', @TransportationExpense),
(5134, 513, N'5134', N'Bank and similar charges', @BankAndSimilarCharges),
(5135, 513, N'5135', N'Travel expense', @TravelExpense),
(5136, 513, N'5136', N'Communication expense', @CommunicationExpense),
(5137, 513, N'5137', N'Utilities expense', @UtilitiesExpense),
(5138, 513, N'5138', N'Advertising expense', @AdvertisingExpense),
(514, 51, N'514', N'Employee benefits expense', @EmployeeBenefitsExpense),
(5141, 514, N'5141', N'Wages and salaries', @WagesAndSalaries),
(5142, 514, N'5142', N'Social security contributions', @SocialSecurityContributions),
(5143, 514, N'5143', N'Other short-term employee benefits', @OtherShorttermEmployeeBenefits),
(515, 51, N'515', N'Depreciation and amortisation expense', @DepreciationAndAmortisationExpense),
(5151, 515, N'5151', N'Depreciation expense', @DepreciationExpense),
(5152, 515, N'5152', N'Amortisation expense', @AmortisationExpense),
(516, 51, N'516', N'Reversal of impairment loss (impairment loss) recognised in profit or loss', @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss),
(519, 51, N'519', N'Other expenses', @OtherExpenseByNature),
(52, 5, N'52', N'Other gains (losses)', @OtherGainsLosses),
(521, 52, N'521', N'Gain (loss) on disposal of property, plant and equipment', @GainLossOnDisposalOfPropertyPlantAndEquipmentExtension),
(522, 52, N'522', N'Gain (loss) on foreign exchange', @GainLossOnForeignExchangeExtension),
(7, NULL, N'7', N'Control Accounts', @ControlAccountsExtension),
(7100, 7, N'7100', N'Document Control', @DocumentControlExtension),
(7200, 7, N'7200', N'Final account control', @FinalAccountsControlExtension)

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
DECLARE @AC1 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1');
DECLARE @AC11 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'11');
DECLARE @AC111 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'111');
DECLARE @AC1111 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1111');
DECLARE @AC1112 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1112');
DECLARE @AC1113 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1113');
DECLARE @AC112 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'112');
DECLARE @AC1121 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1121');
DECLARE @AC1122 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1122');
DECLARE @AC1123 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1123');
DECLARE @AC1124 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1124');
DECLARE @AC1125 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1125');
DECLARE @AC1126 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1126');
DECLARE @AC1127 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1127');
DECLARE @AC1129 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1129');
DECLARE @AC113 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'113');
DECLARE @AC114 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'114');
DECLARE @AC1141 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1141');
DECLARE @AC1142 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1142');
DECLARE @AC116 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'116');
DECLARE @AC117 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'117');
DECLARE @AC1171 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1171');
DECLARE @AC1172 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1172');
DECLARE @AC1173 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1173');
DECLARE @AC1174 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1174');
DECLARE @AC1175 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1175');
DECLARE @AC1176 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1176');
DECLARE @AC1177 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1177');
DECLARE @AC1179 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1179');
DECLARE @AC119 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'119');
DECLARE @AC12 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'12');
DECLARE @AC121 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'121');
DECLARE @AC1211 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1211');
DECLARE @AC1212 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1212');
DECLARE @AC1213 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1213');
DECLARE @AC1214 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1214');
DECLARE @AC1215 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1215');
DECLARE @AC1217 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1217');
DECLARE @AC1218 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1218');
DECLARE @AC1219 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1219');
DECLARE @AC122 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'122');
DECLARE @AC123 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'123');
DECLARE @AC124 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'124');
DECLARE @AC125 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'125');
DECLARE @AC1251 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1251');
DECLARE @AC1252 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1252');
DECLARE @AC1253 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1253');
DECLARE @AC1254 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1254');
DECLARE @AC1255 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1255');
DECLARE @AC1256 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1256');
DECLARE @AC1257 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1257');
DECLARE @AC126 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'126');
DECLARE @AC127 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'127');
DECLARE @AC128 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'128');
DECLARE @AC2 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2');
DECLARE @AC21 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'21');
DECLARE @AC211 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'211');
DECLARE @AC2111 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2111');
DECLARE @AC2112 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2112');
DECLARE @AC212 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'212');
DECLARE @AC2121 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2121');
DECLARE @AC2122 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2122');
DECLARE @AC2123 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2123');
DECLARE @AC2124 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2124');
DECLARE @AC2125 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2125');
DECLARE @AC2126 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2126');
DECLARE @AC2127 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2127');
DECLARE @AC2128 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2128');
DECLARE @AC2130 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2130');
DECLARE @AC2140 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2140');
DECLARE @AC2150 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2150');
DECLARE @AC2160 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2160');
DECLARE @AC22 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'22');
DECLARE @AC221 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'221');
DECLARE @AC2211 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2211');
DECLARE @AC2212 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2212');
DECLARE @AC222 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'222');
DECLARE @AC2221 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2221');
DECLARE @AC2222 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2222');
DECLARE @AC2223 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2223');
DECLARE @AC2224 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2224');
DECLARE @AC3 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3');
DECLARE @AC3100 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3100');
DECLARE @AC3200 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3200');
DECLARE @AC3400 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3400');
DECLARE @AC4 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4');
DECLARE @AC4110 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4110');
DECLARE @AC4120 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4120');
DECLARE @AC4130 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4130');
DECLARE @AC4140 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4140');
DECLARE @AC4190 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4190');
DECLARE @AC4200 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4200');
DECLARE @AC5 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5');
DECLARE @AC51 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'51');
DECLARE @AC5110 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5110');
DECLARE @AC5120 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5120');
DECLARE @AC513 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'513');
DECLARE @AC5131 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5131');
DECLARE @AC5132 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5132');
DECLARE @AC5133 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5133');
DECLARE @AC5134 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5134');
DECLARE @AC5135 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5135');
DECLARE @AC5136 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5136');
DECLARE @AC5137 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5137');
DECLARE @AC5138 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5138');
DECLARE @AC514 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'514');
DECLARE @AC5141 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5141');
DECLARE @AC5142 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5142');
DECLARE @AC5143 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5143');
DECLARE @AC515 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'515');
DECLARE @AC5151 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5151');
DECLARE @AC5152 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5152');
DECLARE @AC516 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'516');
DECLARE @AC519 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'519');
DECLARE @AC52 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'52');
DECLARE @AC521 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'521');
DECLARE @AC522 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'522');
DECLARE @AC7 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7');
DECLARE @AC7100 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7100');
DECLARE @AC7200 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'7200');