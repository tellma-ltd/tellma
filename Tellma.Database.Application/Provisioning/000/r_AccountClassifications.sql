INSERT INTO @AccountClassifications([Index],[ParentIndex],[Code],[Name],[AccountTypeParentId]) VALUES
(1,NULL, N'1',N'Assets',@Assets),
(11,1, N'11',N'Non-current assets',@NoncurrentAssets),
(1101,11, N'1101',N'Property, plant and equipment',@PropertyPlantAndEquipment),
(110101,1101, N'110101',N'Land',@Land),
(110102,1101, N'110102',N'Buildings',@Buildings),
(110103,1101, N'110103',N'Machinery',@Machinery),
(110106,1101, N'110106',N'Motor Vehicles',@MotorVehicles),
(110107,1101, N'110107',N'Fixtures and fittings',@FixturesAndFittings),
(110108,1101, N'110108',N'Office equipment',@OfficeEquipment),
(110109,1101, N'110109',N'Bearer plants',@BearerPlants),
(110110,1101, N'110110',N'Tangible exploration and evaluation assets',@TangibleExplorationAndEvaluationAssets),
(110111,1101, N'110111',N'Mining assets',@MiningAssets),
(110112,1101, N'110112',N'Oil and gas assets',@OilAndGasAssets),
(110113,1101, N'110113',N'Construction in progress',@ConstructionInProgress),
(110114,1101, N'110114',N'Owner-occupied property measured using investment property fair value model',@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel),
(110199,1101, N'110199',N'Other property, plant and equipment',@OtherPropertyPlantAndEquipment),
(1102,11, N'1102',N'Investment property',@InvestmentProperty),
(110201,1102, N'110201',N'Investment property completed',@InvestmentPropertyCompleted),
(110202,1102, N'110202',N'Investment property under construction or development',@InvestmentPropertyUnderConstructionOrDevelopment),
(1103,11, N'1103',N'Goodwill',@Goodwill),
(1104,11, N'1104',N'Intangible assets other than goodwill',@IntangibleAssetsOtherThanGoodwill),
(1105,11, N'1105',N'Investments accounted for using equity method',@InvestmentAccountedForUsingEquityMethod),
(110501,1105, N'110501',N'Investments in associates accounted for using equity method',@InvestmentsInAssociatesAccountedForUsingEquityMethod),
(110502,1105, N'110502',N'Investments in joint ventures accounted for using equity method',@InvestmentsInJointVenturesAccountedForUsingEquityMethod),
(1106,11, N'1106',N'Investments in subsidiaries, joint ventures and associates',@InvestmentsInSubsidiariesJointVenturesAndAssociates),
(110601,1106, N'110601',N'Investments in subsidiaries',@InvestmentsInSubsidiaries),
(110602,1106, N'110602',N'Investments in joint ventures',@InvestmentsInJointVentures),
(110603,1106, N'110603',N'Investments in associates',@InvestmentsInAssociates),
(1107,11, N'1107',N'Non-current biological assets',@NoncurrentBiologicalAssets),
(1108,11, N'1108',N'Trade and other non-current receivables',@NoncurrentReceivables),
(110801,1108, N'110801',N'Non-current trade receivables',@NoncurrentTradeReceivables),
(110802,1108, N'110802',N'Non-current receivables due from related parties',@NoncurrentReceivablesDueFromRelatedParties),
(110803,1108, N'110803',N'Non-current prepayments',@NoncurrentPrepayments),
(110804,1108, N'110804',N'Non-current accrued income',@NoncurrentAccruedIncome),
(110805,1108, N'110805',N'Non-current receivables from taxes other than income tax',@NoncurrentReceivablesFromTaxesOtherThanIncomeTax),
(110806,1108, N'110806',N'Non-current receivables from sale of properties',@NoncurrentReceivablesFromSaleOfProperties),
(110807,1108, N'110807',N'Non-current receivables from rental of properties',@NoncurrentReceivablesFromRentalOfProperties),
(110899,1108, N'110899',N'Other non-current receivables',@OtherNoncurrentReceivables),
(1109,11, N'1109',N'Non-current inventories',@NoncurrentInventories),
(1110,11, N'1110',N'Deferred tax assets',@DeferredTaxAssets),
(1111,11, N'1111',N'Current tax assets, non-current',@CurrentTaxAssetsNoncurrent),
(1112,11, N'1112',N'Other non-current financial assets',@OtherNoncurrentFinancialAssets),
(1113,11, N'1113',N'Other non-current non-financial assets',@OtherNoncurrentNonfinancialAssets),
(1114,11, N'1114',N'Non-current non-cash assets pledged as collateral for which transferee has right by contract or cust',@NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral),
(12,1, N'12',N'Current assets',@CurrentAssets),
(1201,12, N'1201',N'Current inventories',@Inventories),
(120101,1201, N'120101',N'Current inventories held for sale',@CurrentInventoriesHeldForSale),
(120102,1201, N'120102',N'Current work in progress',@WorkInProgress),
(120103,1201, N'120103',N'Current materials and supplies to be consumed in production process or rendering services',@CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices),
(120104,1201, N'120104',N'Current inventories in transit',@CurrentInventoriesInTransit),
(120199,1201, N'120199',N'Other current inventories',@OtherInventories),
(1202,12, N'1202',N'Trade and other current receivables',@TradeAndOtherCurrentReceivables),
(120201,1202, N'120201',N'Current trade receivables',@CurrentTradeReceivables),
(120202,1202, N'120202',N'Current receivables due from related parties',@TradeAndOtherCurrentReceivablesDueFromRelatedParties),
(120203,1202, N'120203',N'Current prepayments',@CurrentPrepayments),
(120204,1202, N'120204',N'Current accrued income',@CurrentAccruedIncome),
(120205,1202, N'120205',N'Current billed but not received',@CurrentBilledButNotReceivedExtension),
(120206,1202, N'120206',N'Current receivables from taxes other than income tax',@CurrentReceivablesFromTaxesOtherThanIncomeTax),
(120207,1202, N'120207',N'Current receivables from rental of properties',@CurrentReceivablesFromRentalOfProperties),
(120298,1202, N'120298',N'Other current receivables',@OtherCurrentReceivables),
(120299,1202, N'120299',N'Allowance account for credit losses of trade and other current receivables',@AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension),
(1203,12, N'1203',N'Current tax assets, current',@CurrentTaxAssetsCurrent),
(1204,12, N'1204',N'Current biological assets',@CurrentBiologicalAssets),
(1205,12, N'1205',N'Other current financial assets',@OtherCurrentFinancialAssets),
(120501,1205, N'120501',N'Staff Debtors',@LoansExtension),
(120502,1205, N'120502',N'Sundry debtors',@LoansExtension),
(120503,1205, N'120503',N'Collection Guarantee',@CollectionGuaranteeExtension),
(120504,1205, N'120504',N'Dishonoured Guarantee',@DishonouredGuaranteeExtension),
(1206,12, N'1206',N'Other current non-financial assets',@OtherCurrentNonfinancialAssets),
(1207,12, N'1207',N'Cash and cash equivalents',@CashAndCashEquivalents),
(120701,1207, N'120701',N'Cash',@Cash),
(120702,1207, N'120702',N'Cash equivalents',@CashEquivalents),
(120703,1207, N'120703',N'Other cash and cash equivalents',@OtherCashAndCashEquivalents),
(1208,12, N'1208',N'Current non-cash assets pledged as collateral for which transferee has right by contract or custom t',@CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral),
(1209,12, N'1209',N'Non-current assets or disposal groups classified as held for sale or as held for distribution to own',@NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners),
(2,NULL, N'2',N'Equity',@Equity),
(20,2, N'20',N'Equity',NULL),
(2000,20, N'2000',N'Equity',NULL),
(200001,2000, N'200001',N'Issued capital',@IssuedCapital),
(200002,2000, N'200002',N'Retained earnings',@RetainedEarnings),
(200003,2000, N'200003',N'Share premium',@SharePremium),
(200004,2000, N'200004',N'Treasury shares',@TreasuryShares),
(200005,2000, N'200005',N'Other equity interest',@OtherEquityInterest),
(200006,2000, N'200006',N'Other reserves',@OtherReserves),
(3,NULL, N'3',N'Liabilities',@Liabilities),
(31,3, N'31',N'Non-current liabilities',@NoncurrentLiabilities),
(3101,31, N'3101',N'Non-current provisions',@NoncurrentProvisions),
(310101,3101, N'310101',N'Non-current provisions for employee benefits',@NoncurrentProvisionsForEmployeeBenefits),
(310102,3101, N'310102',N'Other non-current provisions',@OtherLongtermProvisions),
(3102,31, N'3102',N'Trade and other non-current payables',@NoncurrentPayables),
(3103,31, N'3103',N'Deferred tax liabilities',@DeferredTaxLiabilities),
(3104,31, N'3104',N'Current tax liabilities, non-current',@CurrentTaxLiabilitiesNoncurrent),
(3105,31, N'3105',N'Other non-current financial liabilities',@OtherNoncurrentFinancialLiabilities),
(310501,3105, N'310501',N'Non-current financial liabilities at fair value through profit or loss, classified as held for tradi',@NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading),
(310502,3105, N'310502',N'Non-current financial liabilities at fair value through profit or loss, designated upon initial reco',@NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition),
(310503,3105, N'310503',N'Non-current financial liabilities at amortised cost',@NoncurrentFinancialLiabilitiesAtAmortisedCost),
(3106,31, N'3106',N'Other non-current non-financial liabilities',@OtherNoncurrentNonfinancialLiabilities),
(32,3, N'32',N'Current liabilities',@CurrentLiabilities),
(3201,32, N'3201',N'Current provisions',@CurrentProvisions),
(320101,3201, N'320101',N'Current provisions for employee benefits',@CurrentProvisionsForEmployeeBenefits),
(320102,3201, N'320102',N'Other current provisions',@OtherShorttermProvisions),
(3202,32, N'3202',N'Trade and other current payables',@TradeAndOtherCurrentPayables),
(320201,3202, N'320201',N'Current trade payables',@TradeAndOtherCurrentPayablesToTradeSuppliers),
(320202,3202, N'320202',N'Current payables to related parties',@TradeAndOtherCurrentPayablesToRelatedParties),
(320203,3202, N'320203',N'Deferred income classified as current',@DeferredIncomeClassifiedAsCurrent),
(320204,3202, N'320204',N'Accruals classified as current',@AccrualsClassifiedAsCurrent),
(320205,3202, N'320205',N'Current payables on social security and taxes other than income tax',@CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax),
(320206,3202, N'320206',N'Current retention payables',@CurrentRetentionPayables),
(320299,3202, N'320299',N'Other current payables',@OtherCurrentPayables),
(3203,32, N'3203',N'Current tax liabilities, current',@CurrentTaxLiabilitiesCurrent),
(3204,32, N'3204',N'Other current financial liabilities',@OtherCurrentFinancialLiabilities),
(320401,3204, N'320401',N'Current financial liabilities at fair value through profit or loss, classified as held for trading',@CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading),
(320402,3204, N'320402',N'Current financial liabilities at fair value through profit or loss, designated upon initial recognit',@CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition),
(320403,3204, N'320403',N'Current financial liabilities at amortised cost',@CurrentFinancialLiabilitiesAtAmortisedCost),
(3205,32, N'3205',N'Other current non-financial liabilities',@OtherCurrentNonfinancialLiabilities),
(3206,32, N'3206',N'Liabilities included in disposal groups classified as held for sale',@LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale),
(4,NULL, N'4',N'Profit (loss) from operating activities',@ProfitLossFromOperatingActivities),
(41,4, N'41',N'Revenue',@Revenue),
(4101,41, N'4101',N'Revenue from sale of goods',@RevenueFromSaleOfGoods),
(410101,4101, N'410101',N'Revenue from sale of goods (by product type)',@RevenueFromSaleOfGoods),
(4102,41, N'4102',N'Revenue from rendering of services',@RevenueFromRenderingOfServices),
(410201,4102, N'410201',N'Revenue from rendering of services (by service type)',@RevenueFromRenderingOfServices),
(4103,41, N'4103',N'Revenue from construction contracts',@RevenueFromConstructionContracts),
(4104,41, N'4104',N'Royalty income',@RevenueFromRoyalties),
(4105,41, N'4105',N'Licence fee income',@LicenceFeeIncome),
(4106,41, N'4106',N'Franchise fee income',@FranchiseFeeIncome),
(4107,41, N'4107',N'Interest income',@RevenueFromInterest),
(4108,41, N'4108',N'Dividend income',@RevenueFromDividends),
(4199,41, N'4199',N'Other revenue',@OtherRevenue),
(42,4, N'42',N'Other income',@OtherIncome),
(43,4, N'43',N'Expenses by nature',@ExpenseByNature),
(4301,43, N'4301',N'Raw materials and consumables used',@RawMaterialsAndConsumablesUsed),
(4302,43, N'4302',N'Cost of merchandise sold',@CostOfMerchandiseSold),
(4303,43, N'4303',N'Services expense',@ServicesExpense),
(430301,4303, N'430301',N'Insurance expense',@InsuranceExpense),
(430302,4303, N'430302',N'Professional fees expense',@ProfessionalFeesExpense),
(430303,4303, N'430303',N'Transportation expense',@TransportationExpense),
(430304,4303, N'430304',N'Bank and similar charges',@BankAndSimilarCharges),
(430305,4303, N'430305',N'Travel expense',@TravelExpense),
(430306,4303, N'430306',N'Communication expense',@CommunicationExpense),
(430307,4303, N'430307',N'Utilities expense',@UtilitiesExpense),
(430308,4303, N'430308',N'Advertising expense',@AdvertisingExpense),
(4304,43, N'4304',N'Employee benefits expense',@EmployeeBenefitsExpense),
(430401,4304, N'430401',N'Wages and salaries',@WagesAndSalaries),
(430402,4304, N'430402',N'Social security contributions',@SocialSecurityContributions),
(430403,4304, N'430403',N'Other short-term employee benefits',@OtherShorttermEmployeeBenefits),
(430404,4304, N'430404',N'Post-employment benefit expense, defined contribution plans',@PostemploymentBenefitExpenseDefinedContributionPlans),
(430405,4304, N'430405',N'Post-employment benefit expense, defined benefit plans',@PostemploymentBenefitExpenseDefinedBenefitPlans),
(430406,4304, N'430406',N'Termination benefits expense',@TerminationBenefitsExpense),
(430407,4304, N'430407',N'Other long-term employee benefits',@OtherLongtermBenefits),
(430499,4304, N'430499',N'Other employee expense',@OtherEmployeeExpense),
(4305,43, N'4305',N'Depreciation and amortisation expense',@DepreciationAndAmortisationExpense),
(430501,4305, N'430501',N'Depreciation expense',@DepreciationExpense),
(430502,4305, N'430502',N'Amortisation expense',@AmortisationExpense),
(4306,43, N'4306',N'Reversal of impairment loss (impairment loss) recognised in profit or loss',@ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss),
(430601,4306, N'430601',N'Write-downs (reversals of write-downs) of inventories',@WritedownsReversalsOfInventories),
(430602,4306, N'430602',N'Write-downs (reversals of write-downs) of property, plant and equipment',@WritedownsReversalsOfPropertyPlantAndEquipment),
(430603,4306, N'430603',N'Impairment loss (reversal of impairment loss) recognised in profit or loss, trade receivables',@ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossTradeReceivables),
(430604,4306, N'430604',N'Impairment loss (reversal of impairment loss) recognised in profit or loss, loans and advances',@ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossLoansAndAdvances),
(4307,43, N'4307',N'Tax expense other than income tax expense',@TaxExpenseOtherThanIncomeTaxExpense),
(4399,43, N'4399',N'Other expenses',@OtherExpenseByNature),

(44,4, N'44',N'Other gains (losses)',@OtherGainsLosses),
(4401,44, N'4401',N'Gain (loss) on disposal of property, plant and equipment',@GainLossOnDisposalOfPropertyPlantAndEquipmentExtension),
(4402,44, N'4402',N'Gain (loss) on foreign exchange',@GainLossOnForeignExchangeExtension),
(5,NULL, N'5',N'Other profit (loss)',NULL),
(51,5, N'51',N'Other profit (loss) from continuing operation',NULL),
(5101,51, N'5101',N'Gains (losses) on net monetary position',@GainsLossesOnNetMonetaryPosition),
(5102,51, N'5102',N'Gain (loss) arising from derecognition of financial assets measured at amortised cost',@GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost),
(5103,51, N'5103',N'Finance income',@FinanceIncome),
(5104,51, N'5104',N'Finance costs',@FinanceCosts),
(5105,51, N'5105',N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS',@ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9),
(5106,51, N'5106',N'Share of profit (loss) of associates and joint ventures accounted for using equity method',@ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod),
(5107,51, N'5107',N'Other income (expense) from subsidiaries, jointly controlled entities and associates',@OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates),
(5108,51, N'5108',N'Gains (losses) arising from difference between previous amortised cost and fair value of financial a',@GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue),
(5109,51, N'5109',N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassifica',@CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThrou),
(5110,51, N'5110',N'Hedging gains (losses) for hedge of group of items with offsetting risk positions',@HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions),
(52,5, N'52',N'Tax income (expense)',@IncomeTaxExpenseContinuingOperations),
(53,5, N'53',N'Profit (loss) from discontinued operations',@ProfitLossFromDiscontinuedOperations)


EXEC [api].[AccountClassifications__Save] --  N'cash-and-cash-equivalents',
	@Entities = @AccountClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting AccountClassifications: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
--Declarations
DECLARE @AC1 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1');
DECLARE @AC11 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'11');
DECLARE @AC1101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1101');
DECLARE @AC110101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110101');
DECLARE @AC110102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110102');
DECLARE @AC110103 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110103');
DECLARE @AC110106 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110106');
DECLARE @AC110107 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110107');
DECLARE @AC110108 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110108');
DECLARE @AC110109 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110109');
DECLARE @AC110110 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110110');
DECLARE @AC110111 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110111');
DECLARE @AC110112 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110112');
DECLARE @AC110113 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110113');
DECLARE @AC110114 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110114');
DECLARE @AC110199 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110199');
DECLARE @AC1102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1102');
DECLARE @AC110201 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110201');
DECLARE @AC110202 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110202');
DECLARE @AC1103 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1103');
DECLARE @AC1104 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1104');
DECLARE @AC1105 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1105');
DECLARE @AC110501 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110501');
DECLARE @AC110502 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110502');
DECLARE @AC1106 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1106');
DECLARE @AC110601 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110601');
DECLARE @AC110602 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110602');
DECLARE @AC110603 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110603');
DECLARE @AC1107 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1107');
DECLARE @AC1108 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1108');
DECLARE @AC110801 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110801');
DECLARE @AC110802 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110802');
DECLARE @AC110803 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110803');
DECLARE @AC110804 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110804');
DECLARE @AC110805 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110805');
DECLARE @AC110806 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110806');
DECLARE @AC110807 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110807');
DECLARE @AC110899 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'110899');
DECLARE @AC1109 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1109');
DECLARE @AC1110 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1110');
DECLARE @AC1111 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1111');
DECLARE @AC1112 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1112');
DECLARE @AC1113 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1113');
DECLARE @AC1114 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1114');
DECLARE @AC12 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'12');
DECLARE @AC1201 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1201');
DECLARE @AC120101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120101');
DECLARE @AC120102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120102');
DECLARE @AC120103 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120103');
DECLARE @AC120104 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120104');
DECLARE @AC120199 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120199');
DECLARE @AC1202 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1202');
DECLARE @AC120201 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120201');
DECLARE @AC120202 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120202');
DECLARE @AC120203 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120203');
DECLARE @AC120204 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120204');
DECLARE @AC120205 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120205');
DECLARE @AC120206 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120206');
DECLARE @AC120207 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120207');
DECLARE @AC120298 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120298');
DECLARE @AC120299 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120299');
DECLARE @AC1203 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1203');
DECLARE @AC1204 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1204');
DECLARE @AC1205 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1205');
DECLARE @AC120501 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120501');
DECLARE @AC120502 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120502');
DECLARE @AC120503 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120503');
DECLARE @AC120504 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120504');
DECLARE @AC1206 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1206');
DECLARE @AC1207 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1207');
DECLARE @AC120701 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120701');
DECLARE @AC120702 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120702');
DECLARE @AC120703 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'120703');
DECLARE @AC1208 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1208');
DECLARE @AC1209 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'1209');
DECLARE @AC2 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2');
DECLARE @AC20 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'20');
DECLARE @AC2000 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'2000');
DECLARE @AC200001 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'200001');
DECLARE @AC200002 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'200002');
DECLARE @AC200003 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'200003');
DECLARE @AC200004 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'200004');
DECLARE @AC200005 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'200005');
DECLARE @AC200006 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'200006');
DECLARE @AC3 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3');
DECLARE @AC31 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'31');
DECLARE @AC3101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3101');
DECLARE @AC310101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'310101');
DECLARE @AC310102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'310102');
DECLARE @AC3102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3102');
DECLARE @AC3103 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3103');
DECLARE @AC3104 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3104');
DECLARE @AC3105 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3105');
DECLARE @AC310501 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'310501');
DECLARE @AC310502 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'310502');
DECLARE @AC310503 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'310503');
DECLARE @AC3106 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3106');
DECLARE @AC32 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'32');
DECLARE @AC3201 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3201');
DECLARE @AC320101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320101');
DECLARE @AC320102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320102');
DECLARE @AC3202 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3202');
DECLARE @AC320201 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320201');
DECLARE @AC320202 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320202');
DECLARE @AC320203 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320203');
DECLARE @AC320204 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320204');
DECLARE @AC320205 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320205');
DECLARE @AC320206 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320206');
DECLARE @AC320299 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320299');
DECLARE @AC3203 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3203');
DECLARE @AC3204 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3204');
DECLARE @AC320401 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320401');
DECLARE @AC320402 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320402');
DECLARE @AC320403 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'320403');
DECLARE @AC3205 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3205');
DECLARE @AC3206 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'3206');
DECLARE @AC4 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4');
DECLARE @AC41 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'41');
DECLARE @AC4101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4101');
DECLARE @AC410101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'410101');
DECLARE @AC4102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4102');
DECLARE @AC410201 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'410201');
DECLARE @AC4103 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4103');
DECLARE @AC4104 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4104');
DECLARE @AC4105 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4105');
DECLARE @AC4106 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4106');
DECLARE @AC4107 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4107');
DECLARE @AC4108 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4108');
DECLARE @AC4199 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4199');
DECLARE @AC42 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'42');
DECLARE @AC43 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'43');
DECLARE @AC4301 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4301');
DECLARE @AC4302 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4302');
DECLARE @AC4303 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4303');
DECLARE @AC430301 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430301');
DECLARE @AC430302 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430302');
DECLARE @AC430303 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430303');
DECLARE @AC430304 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430304');
DECLARE @AC430305 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430305');
DECLARE @AC430306 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430306');
DECLARE @AC430307 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430307');
DECLARE @AC430308 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430308');
DECLARE @AC4304 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4304');
DECLARE @AC430401 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430401');
DECLARE @AC430402 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430402');
DECLARE @AC430403 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430403');
DECLARE @AC430404 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430404');
DECLARE @AC430405 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430405');
DECLARE @AC430406 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430406');
DECLARE @AC430407 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430407');
DECLARE @AC430499 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430499');
DECLARE @AC4305 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4305');
DECLARE @AC430501 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430501');
DECLARE @AC430502 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430502');
DECLARE @AC4306 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4306');
DECLARE @AC430601 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430601');
DECLARE @AC430602 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430602');
DECLARE @AC430603 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430603');
DECLARE @AC430604 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'430604');
DECLARE @AC4307 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4307');
DECLARE @AC4399 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4399');

DECLARE @AC44 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'44');
DECLARE @AC4401 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4401');
DECLARE @AC4402 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'4402');
DECLARE @AC5 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5');
DECLARE @AC51 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'51');
DECLARE @AC5101 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5101');
DECLARE @AC5102 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5102');
DECLARE @AC5103 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5103');
DECLARE @AC5104 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5104');
DECLARE @AC5105 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5105');
DECLARE @AC5106 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5106');
DECLARE @AC5107 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5107');
DECLARE @AC5108 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5108');
DECLARE @AC5109 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5109');
DECLARE @AC5110 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'5110');
DECLARE @AC52 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'52');
DECLARE @AC53 INT = (SELECT [Id] FROM dbo.AccountClassifications WHERE [Code] = N'53');