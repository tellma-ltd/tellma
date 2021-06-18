﻿UPDATE dbo.Accounts SET CurrencyId = N'SDG' WHERE CurrencyId = N'XXX'
UPDATE dbo.Accounts SET [Name] = N'Architectural fees', [Name2] = N'رسوم معماريين' WHERE [Code] = N'43030201'
DELETE FROM @AccountTypesIndexedIds;
INSERT INTO @AccountTypesIndexedIds ([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]  FROM dbo.AccountTypes
WHERE [Concept] IN (
(N'LandAndBuildings'),
(N'Land'),
(N'Buildings'),
(N'Machinery'),
(N'Vehicles'),
--(N'Ships'),
--(N'Aircraft'),
(N'MotorVehicles'),
(N'FixturesAndFittings'),
(N'BearerPlants'),
--(N'TangibleExplorationAndEvaluationAssets'),
--(N'MiningAssets'),
--(N'OilAndGasAssets'),
(N'ConstructionInProgress'),
(N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel'),
(N'OtherPropertyPlantAndEquipment'),
(N'InvestmentProperty'),
(N'InvestmentPropertyCompleted'),
(N'InvestmentPropertyUnderConstructionOrDevelopment'),
--(N'Goodwill'),
(N'IntangibleAssetsOtherThanGoodwill'),
(N'InvestmentAccountedForUsingEquityMethod'),
(N'InvestmentsInAssociatesAccountedForUsingEquityMethod'),
(N'InvestmentsInJointVenturesAccountedForUsingEquityMethod'),
(N'InvestmentsInSubsidiariesJointVenturesAndAssociates'),
(N'InvestmentsInSubsidiaries'),
(N'InvestmentsInJointVentures'),
(N'InvestmentsInAssociates'),
(N'NoncurrentBiologicalAssets'),
--(N'NoncurrentValueAddedTaxReceivables'),
(N'NoncurrentReceivablesFromSaleOfProperties'),
(N'NoncurrentReceivablesFromRentalOfProperties'),
--(N'NoncurrentInventories'),
(N'DeferredTaxAssets'),
(N'CurrentTaxAssetsNoncurrent'),
(N'OtherNoncurrentFinancialAssets'),
(N'OtherNoncurrentNonfinancialAssets'),
--(N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
(N'CurrentInventoriesHeldForSale'),
(N'Merchandise'),
(N'CurrentFoodAndBeverage'),
(N'CurrentAgriculturalProduce'),
(N'FinishedGoods'),
(N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness'),
(N'CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices'),
(N'CurrentRawMaterialsAndCurrentProductionSupplies'),
(N'RawMaterials'),
(N'ProductionSupplies'),
(N'CurrentPackagingAndStorageMaterials'),
(N'SpareParts'),
(N'CurrentFuel'),
--(N'WithholdingTaxReceivablesExtension'),
(N'CurrentReceivablesFromRentalOfProperties'),
(N'AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension'),
(N'CurrentTaxAssetsCurrent'),
(N'CurrentBiologicalAssets'),
(N'LoansExtension'),
(N'OtherCurrentNonfinancialAssets'),
(N'CashEquivalents'),
--(N'ShorttermDepositsClassifiedAsCashEquivalents'),
--(N'ShorttermInvestmentsClassifiedAsCashEquivalents'),
--(N'BankingArrangementsClassifiedAsCashEquivalents'),
--(N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral'),
(N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners'),
(N'AllowanceAccountForCreditLossesOfFinancialAssets'),
(N'AllowanceAccountForCreditLossesOfTradeAndOtherReceivablesExtension'),
(N'AllowanceAccountForCreditLossesOfOtherFinancialAssetsExtension'),
--(N'SharePremium'),
--(N'TreasuryShares'),
(N'OtherEquityInterest'),
(N'ReserveOfExchangeDifferencesOnTranslation'),
(N'ReserveOfCashFlowHedges'),
--(N'ReserveOfGainsAndLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments'),
--(N'ReserveOfChangeInValueOfTimeValueOfOptions'),
--(N'ReserveOfChangeInValueOfForwardElementsOfForwardContracts'),
--(N'ReserveOfChangeInValueOfForeignCurrencyBasisSpreads'),
(N'ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome'),
--(N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillBeReclassifiedToProfitOrLoss'),
--(N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss'),
--(N'ReserveOfFinanceIncomeExpensesFromReinsuranceContractsHeldExcludedFromProfitOrLoss'),
--(N'ReserveOfGainsAndLossesOnRemeasuringAvailableforsaleFinancialAssets'),
--(N'ReserveOfSharebasedPayments'),
--(N'ReserveOfRemeasurementsOfDefinedBenefitPlans'),
(N'AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale'),
(N'ReserveOfGainsAndLossesFromInvestmentsInEquityInstruments'),
--(N'ReserveOfChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability'),
(N'ReserveForCatastrophe'),
--(N'ReserveForEqualisation'),
--(N'ReserveOfDiscretionaryParticipationFeatures'),
--(N'ReserveOfEquityComponentOfConvertibleInstruments'),
(N'CapitalRedemptionReserve'),
(N'MergerReserve'),
(N'LongtermWarrantyProvision'),
(N'LongtermRestructuringProvision'),
(N'LongtermLegalProceedingsProvision'),
--(N'NoncurrentRefundsProvision'),
--(N'LongtermOnerousContractsProvision'),
(N'LongtermProvisionForDecommissioningRestorationAndRehabilitationCosts'),
(N'LongtermMiscellaneousOtherProvisions'),
(N'DeferredTaxLiabilities'),
(N'CurrentTaxLiabilitiesNoncurrent'),
--(N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLoss'),
--(N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading'),
--(N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition'),
--(N'NoncurrentFinancialLiabilitiesAtAmortisedCost'),
--(N'OtherNoncurrentNonfinancialLiabilities'),
(N'ShorttermWarrantyProvision'),
(N'ShorttermRestructuringProvision'),
(N'ShorttermLegalProceedingsProvision'),
(N'CurrentRefundsProvision'),
--(N'ShorttermOnerousContractsProvision'),
(N'ShorttermProvisionForDecommissioningRestorationAndRehabilitationCosts'),
(N'ShorttermMiscellaneousOtherProvisions'),
(N'CurrentExciseTaxPayables'),
(N'CurrentZakatPayablesExtension'),
(N'CurrentEmployeeStampTaxPayablesExtension'),
--(N'ProvidentFundPayableExtension'),
--(N'WithholdingTaxPayableExtension'),
--(N'CostSharingPayableExtension'),
(N'CurrentRetentionPayables'),
(N'CurrentTaxLiabilitiesCurrent'),
(N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossAbstract'),
(N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading'),
(N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition'),
(N'CurrentFinancialLiabilitiesAtAmortisedCost'),
(N'OtherCurrentNonfinancialLiabilities'),
(N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale'),
--(N'RevenueFromConstructionContracts'),
--(N'RevenueFromRoyalties'),
--(N'LicenceFeeIncome'),
--(N'FranchiseFeeIncome'),
--(N'RevenueFromInterest'),
(N'ChangesInInventoriesOfFinishedGoodsAndWorkInProgress'),
(N'OtherWorkPerformedByEntityAndCapitalised'),
(N'WritedownsReversalsOfInventories'),
(N'GainsLossesOnDisposalsOfInvestmentProperties'),
(N'GainsLossesOnDisposalsOfInvestments'),
(N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost'),
(N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9'),
(N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod'),
(N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates'),
(N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue'),
(N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory'),
--(N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions'),
(N'IncomeTaxExpenseContinuingOperations'),
(N'ProfitLossFromDiscontinuedOperations'),
(N'OtherComprehensiveIncome'),
(N'ComponentsOfOtherComprehensiveIncomeThatWillNotBeReclassifiedToProfitOrLossBeforeTax'),
(N'OtherComprehensiveIncomeBeforeTaxGainsLossesFromInvestmentsInEquityInstruments'),
(N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnRevaluation'),
--(N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnRemeasurementsOfDefinedBenefitPlans'),
--(N'OtherComprehensiveIncomeBeforeTaxChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability'),
--(N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments'),
--(N'OtherComprehensiveIncomeBeforeTaxInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss'),
(N'ShareOfOtherComprehensiveIncomeOfAssociatesAndJointVenturesAccountedForUsingEquityMethodThatWillNotBeReclassifiedToProfitOrLossBeforeTax'),
(N'ComponentsOfOtherComprehensiveIncomeThatWillBeReclassifiedToProfitOrLossBeforeTax'),
(N'OtherComprehensiveIncomeBeforeTaxExchangeDifferencesOnTranslation'),
(N'GainsLossesOnExchangeDifferencesOnTranslationBeforeTax'),
(N'ReclassificationAdjustmentsOnExchangeDifferencesOnTranslationBeforeTax'),
(N'OtherComprehensiveIncomeBeforeTaxAvailableforsaleFinancialAssets'),
(N'GainsLossesOnRemeasuringAvailableforsaleFinancialAssetsBeforeTax'),
(N'ReclassificationAdjustmentsOnAvailableforsaleFinancialAssetsBeforeTax')
)

INSERT INTO @ValidationErrors
EXEC [api].[AccountTypes__Activate]
	@Ids = @AccountTypesIndexedIds,
	@IsActive = 1,
	@UserId = @AdminUserId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Account Types: Error Activating'
	GOTO Err_Label;
END;
	
DELETE FROM @AccountClassificationsIndexedIds;
INSERT INTO @AccountClassificationsIndexedIds ([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]  FROM dbo.AccountClassifications
WHERE AccountTypeParentId IN (SELECT [Id] FROM @AccountTypesIndexedIds);

INSERT INTO @ValidationErrors
EXEC [api].[AccountClassifications__Activate]
	@Ids = @AccountClassificationsIndexedIds,
	@IsActive = 1,
	@UserId = @AdminUserId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Account Classifications: Error Activating'
	GOTO Err_Label;
END;

DELETE FROM @AccountsIndexedIds;
INSERT INTO @AccountsIndexedIds([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]  FROM dbo.Accounts
WHERE AccountTypeId IN (SELECT [Id] FROM @AccountTypesIndexedIds);

INSERT INTO @ValidationErrors
EXEC [api].[Accounts__Activate]
	@Ids = @AccountsIndexedIds,
	@IsActive = 1,
	@UserId = @AdminUserId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'Accounts: Error Activating'
	GOTO Err_Label;
END;
