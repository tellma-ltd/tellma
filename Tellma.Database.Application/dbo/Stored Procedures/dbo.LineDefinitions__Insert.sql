CREATE PROCEDURE [dbo].[LineDefinitions__Insert]
AS
--Declarations
DECLARE @StatementOfFinancialPositionAbstract INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'StatementOfFinancialPositionAbstract');
DECLARE @Assets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Assets');
DECLARE @NoncurrentAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAssets');
DECLARE @PropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyPlantAndEquipment');
DECLARE @LandAndBuildings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LandAndBuildings');
DECLARE @Land INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Land');
DECLARE @Buildings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Buildings');
DECLARE @Machinery INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Machinery');
DECLARE @Vehicles INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Vehicles');
DECLARE @Ships INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Ships');
DECLARE @Aircraft INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Aircraft');
DECLARE @MotorVehicles INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'MotorVehicles');
DECLARE @FixturesAndFittings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FixturesAndFittings');
DECLARE @OfficeEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OfficeEquipment');
DECLARE @BearerPlants INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'BearerPlants');
DECLARE @TangibleExplorationAndEvaluationAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TangibleExplorationAndEvaluationAssets');
DECLARE @MiningAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'MiningAssets');
DECLARE @OilAndGasAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OilAndGasAssets');
DECLARE @ConstructionInProgress INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ConstructionInProgress');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel');
DECLARE @OtherPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherPropertyPlantAndEquipment');
DECLARE @InvestmentProperty INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentProperty');
DECLARE @InvestmentPropertyCompleted INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyCompleted');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentPropertyUnderConstructionOrDevelopment');
DECLARE @Goodwill INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Goodwill');
DECLARE @IntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IntangibleAssetsOtherThanGoodwill');
DECLARE @InvestmentAccountedForUsingEquityMethod INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentAccountedForUsingEquityMethod');
DECLARE @InvestmentsInAssociatesAccountedForUsingEquityMethod INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInAssociatesAccountedForUsingEquityMethod');
DECLARE @InvestmentsInJointVenturesAccountedForUsingEquityMethod INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInJointVenturesAccountedForUsingEquityMethod');
DECLARE @InvestmentsInSubsidiariesJointVenturesAndAssociates INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInSubsidiariesJointVenturesAndAssociates');
DECLARE @InvestmentsInSubsidiaries INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInSubsidiaries');
DECLARE @InvestmentsInJointVentures INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInJointVentures');
DECLARE @InvestmentsInAssociates INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInAssociates');
DECLARE @NoncurrentBiologicalAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentBiologicalAssets');
DECLARE @NoncurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivables');
DECLARE @NoncurrentTradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentTradeReceivables');
DECLARE @NoncurrentReceivablesDueFromRelatedParties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesDueFromRelatedParties');
DECLARE @NoncurrentPrepaymentsAndNoncurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPrepaymentsAndNoncurrentAccruedIncome');
DECLARE @NoncurrentPrepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPrepayments');
DECLARE @NoncurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAccruedIncome');
DECLARE @NoncurrentReceivablesFromTaxesOtherThanIncomeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesFromTaxesOtherThanIncomeTax');
DECLARE @NoncurrentValueAddedTaxReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentValueAddedTaxReceivables');
DECLARE @NoncurrentReceivablesFromSaleOfProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesFromSaleOfProperties');
DECLARE @NoncurrentReceivablesFromRentalOfProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesFromRentalOfProperties');
DECLARE @OtherNoncurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentReceivables');
DECLARE @NoncurrentInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentInventories');
DECLARE @DeferredTaxAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredTaxAssets');
DECLARE @CurrentTaxAssetsNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxAssetsNoncurrent');
DECLARE @OtherNoncurrentFinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentFinancialAssets');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLoss');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMeasuredAsSuchInAccordanceWithExemptionForRepurchaseOfOwnFinancialLiabil INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMeasuredAsSuchInAccordanceWithExemptionForRepurchaseOfOwnFinancialLiabilities');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMeasuredAsSuchInAccordanceWithExemptionForReacquisitionOfOwnEquityInstru INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMeasuredAsSuchInAccordanceWithExemptionForReacquisitionOfOwnEquityInstruments');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMandatorilyMeasuredAtFairValue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMandatorilyMeasuredAtFairValue');
DECLARE @NoncurrentFinancialAssetsAvailableforsale INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAvailableforsale');
DECLARE @NoncurrentHeldtomaturityInvestments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentHeldtomaturityInvestments');
DECLARE @NoncurrentLoansAndReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentLoansAndReceivables');
DECLARE @NoncurrentFinancialAssetsAtFairValueThroughOtherComprehensiveIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtFairValueThroughOtherComprehensiveIncome');
DECLARE @NoncurrentFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome');
DECLARE @NoncurrentInvestmentsInEquityInstrumentsDesignatedAtFairValueThroughOtherComprehensiveIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentInvestmentsInEquityInstrumentsDesignatedAtFairValueThroughOtherComprehensiveIncome');
DECLARE @NoncurrentFinancialAssetsAtAmortisedCost INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialAssetsAtAmortisedCost');
DECLARE @OtherNoncurrentNonfinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentNonfinancialAssets');
DECLARE @NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral');
DECLARE @CurrentAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentAssets');
DECLARE @CurrentAssetsOtherThanAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentAssetsOtherThanAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners');
DECLARE @Inventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Inventories');
DECLARE @CurrentInventoriesHeldForSale INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentInventoriesHeldForSale');
DECLARE @Merchandise INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Merchandise');
DECLARE @CurrentFoodAndBeverage INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentFoodAndBeverage');
DECLARE @CurrentAgriculturalProduce INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentAgriculturalProduce');
DECLARE @FinishedGoods INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FinishedGoods');
DECLARE @PropertyIntendedForSaleInOrdinaryCourseOfBusiness INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness');
DECLARE @WorkInProgress INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WorkInProgress');
DECLARE @CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices');
DECLARE @CurrentRawMaterialsAndCurrentProductionSupplies INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentRawMaterialsAndCurrentProductionSupplies');
DECLARE @RawMaterials INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RawMaterials');
DECLARE @ProductionSupplies INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProductionSupplies');
DECLARE @CurrentPackagingAndStorageMaterials INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentPackagingAndStorageMaterials');
DECLARE @SpareParts INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'SpareParts');
DECLARE @CurrentFuel INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentFuel');
DECLARE @CurrentInventoriesInTransit INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentInventoriesInTransit');
DECLARE @OtherInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherInventories');
DECLARE @TradeAndOtherCurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentReceivables');
DECLARE @CurrentTradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTradeReceivables');
DECLARE @TradeAndOtherCurrentReceivablesDueFromRelatedParties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentReceivablesDueFromRelatedParties');
DECLARE @CurrentPrepaymentsAndCurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentPrepaymentsAndCurrentAccruedIncome');
DECLARE @CurrentPrepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentPrepayments');
DECLARE @CurrentAdvancesToSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentAdvancesToSuppliers');
DECLARE @CurrentPrepaidExpenses INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentPrepaidExpenses');
DECLARE @CurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentAccruedIncome');
DECLARE @CurrentBilledButNotReceivedExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentBilledButNotReceivedExtension');
DECLARE @CurrentReceivablesFromTaxesOtherThanIncomeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentReceivablesFromTaxesOtherThanIncomeTax');
DECLARE @CurrentValueAddedTaxReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentValueAddedTaxReceivables');
DECLARE @WithholdingTaxReceivablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WithholdingTaxReceivablesExtension');
DECLARE @CurrentReceivablesFromRentalOfProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentReceivablesFromRentalOfProperties');
DECLARE @OtherCurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentReceivables');
DECLARE @AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension');
DECLARE @CurrentTaxAssetsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxAssetsCurrent');
DECLARE @CurrentBiologicalAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentBiologicalAssets');
DECLARE @OtherCurrentFinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentFinancialAssets');
DECLARE @OtherCurrentNonfinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentNonfinancialAssets');
DECLARE @CashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashAndCashEquivalents');
DECLARE @Cash INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Cash');
DECLARE @CashOnHand INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashOnHand');
DECLARE @BalancesWithBanks INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'BalancesWithBanks');
DECLARE @CashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashEquivalents');
DECLARE @ShorttermDepositsClassifiedAsCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermDepositsClassifiedAsCashEquivalents');
DECLARE @ShorttermInvestmentsClassifiedAsCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermInvestmentsClassifiedAsCashEquivalents');
DECLARE @BankingArrangementsClassifiedAsCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'BankingArrangementsClassifiedAsCashEquivalents');
DECLARE @OtherCashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCashAndCashEquivalents');
DECLARE @CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral');
DECLARE @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners');
DECLARE @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSale INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSale');
DECLARE @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForDistributionToOwners INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForDistributionToOwners');
DECLARE @AllowanceAccountForCreditLossesOfFinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AllowanceAccountForCreditLossesOfFinancialAssets');
DECLARE @AllowanceAccountForCreditLossesOfTradeAndOtherReceivablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AllowanceAccountForCreditLossesOfTradeAndOtherReceivablesExtension');
DECLARE @AllowanceAccountForCreditLossesOfOtherFinancialAssetsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AllowanceAccountForCreditLossesOfOtherFinancialAssetsExtension');
DECLARE @EquityAndLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'EquityAndLiabilities');
DECLARE @Equity INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Equity');
DECLARE @IssuedCapital INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IssuedCapital');
DECLARE @RetainedEarnings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RetainedEarnings');
DECLARE @SharePremium INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'SharePremium');
DECLARE @TreasuryShares INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TreasuryShares');
DECLARE @OtherEquityInterest INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherEquityInterest');
DECLARE @OtherReserves INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherReserves');
DECLARE @RevaluationSurplus INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevaluationSurplus');
DECLARE @ReserveOfExchangeDifferencesOnTranslation INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfExchangeDifferencesOnTranslation');
DECLARE @ReserveOfCashFlowHedges INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfCashFlowHedges');
DECLARE @ReserveOfGainsAndLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfGainsAndLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments');
DECLARE @ReserveOfChangeInValueOfTimeValueOfOptions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfChangeInValueOfTimeValueOfOptions');
DECLARE @ReserveOfChangeInValueOfForwardElementsOfForwardContracts INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfChangeInValueOfForwardElementsOfForwardContracts');
DECLARE @ReserveOfChangeInValueOfForeignCurrencyBasisSpreads INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfChangeInValueOfForeignCurrencyBasisSpreads');
DECLARE @ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome');
DECLARE @ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillBeReclassifiedToProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillBeReclassifiedToProfitOrLoss');
DECLARE @ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrL INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss');
DECLARE @ReserveOfFinanceIncomeExpensesFromReinsuranceContractsHeldExcludedFromProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfFinanceIncomeExpensesFromReinsuranceContractsHeldExcludedFromProfitOrLoss');
DECLARE @ReserveOfGainsAndLossesOnRemeasuringAvailableforsaleFinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfGainsAndLossesOnRemeasuringAvailableforsaleFinancialAssets');
DECLARE @ReserveOfSharebasedPayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfSharebasedPayments');
DECLARE @ReserveOfRemeasurementsOfDefinedBenefitPlans INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfRemeasurementsOfDefinedBenefitPlans');
DECLARE @AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale');
DECLARE @ReserveOfGainsAndLossesFromInvestmentsInEquityInstruments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfGainsAndLossesFromInvestmentsInEquityInstruments');
DECLARE @ReserveOfChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability');
DECLARE @ReserveForCatastrophe INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveForCatastrophe');
DECLARE @ReserveForEqualisation INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveForEqualisation');
DECLARE @ReserveOfDiscretionaryParticipationFeatures INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfDiscretionaryParticipationFeatures');
DECLARE @ReserveOfEquityComponentOfConvertibleInstruments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReserveOfEquityComponentOfConvertibleInstruments');
DECLARE @CapitalRedemptionReserve INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CapitalRedemptionReserve');
DECLARE @MergerReserve INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'MergerReserve');
DECLARE @StatutoryReserve INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'StatutoryReserve');
DECLARE @Liabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Liabilities');
DECLARE @NoncurrentLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentLiabilities');
DECLARE @NoncurrentProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentProvisions');
DECLARE @NoncurrentProvisionsForEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentProvisionsForEmployeeBenefits');
DECLARE @OtherLongtermProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherLongtermProvisions');
DECLARE @LongtermWarrantyProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LongtermWarrantyProvision');
DECLARE @LongtermRestructuringProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LongtermRestructuringProvision');
DECLARE @LongtermLegalProceedingsProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LongtermLegalProceedingsProvision');
DECLARE @NoncurrentRefundsProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentRefundsProvision');
DECLARE @LongtermOnerousContractsProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LongtermOnerousContractsProvision');
DECLARE @LongtermProvisionForDecommissioningRestorationAndRehabilitationCosts INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LongtermProvisionForDecommissioningRestorationAndRehabilitationCosts');
DECLARE @LongtermMiscellaneousOtherProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LongtermMiscellaneousOtherProvisions');
DECLARE @NoncurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPayables');
DECLARE @NoncurrentPayablesToTradeSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPayablesToTradeSuppliers');
DECLARE @NoncurrentPayablesToRelatedParties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPayablesToRelatedParties');
DECLARE @AccrualsAndDeferredIncomeClassifiedAsNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AccrualsAndDeferredIncomeClassifiedAsNoncurrent');
DECLARE @DeferredIncomeClassifiedAsNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredIncomeClassifiedAsNoncurrent');
DECLARE @RentDeferredIncomeClassifiedAsNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RentDeferredIncomeClassifiedAsNoncurrent');
DECLARE @AccrualsClassifiedAsNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AccrualsClassifiedAsNoncurrent');
DECLARE @NoncurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax');
DECLARE @NoncurrentValueAddedTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentValueAddedTaxPayables');
DECLARE @NoncurrentExciseTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentExciseTaxPayables');
DECLARE @NoncurrentRetentionPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentRetentionPayables');
DECLARE @OtherNoncurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentPayables');
DECLARE @DeferredTaxLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredTaxLiabilities');
DECLARE @CurrentTaxLiabilitiesNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxLiabilitiesNoncurrent');
DECLARE @OtherNoncurrentFinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentFinancialLiabilities');
DECLARE @NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLoss');
DECLARE @NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading');
DECLARE @NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition');
DECLARE @NoncurrentFinancialLiabilitiesAtAmortisedCost INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentFinancialLiabilitiesAtAmortisedCost');
DECLARE @OtherNoncurrentNonfinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentNonfinancialLiabilities');
DECLARE @CurrentLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentLiabilities');
DECLARE @CurrentProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentProvisions');
DECLARE @CurrentProvisionsForEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentProvisionsForEmployeeBenefits');
DECLARE @OtherShorttermProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherShorttermProvisions');
DECLARE @ShorttermWarrantyProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermWarrantyProvision');
DECLARE @ShorttermRestructuringProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermRestructuringProvision');
DECLARE @ShorttermLegalProceedingsProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermLegalProceedingsProvision');
DECLARE @CurrentRefundsProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentRefundsProvision');
DECLARE @ShorttermOnerousContractsProvision INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermOnerousContractsProvision');
DECLARE @ShorttermProvisionForDecommissioningRestorationAndRehabilitationCosts INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermProvisionForDecommissioningRestorationAndRehabilitationCosts');
DECLARE @ShorttermMiscellaneousOtherProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermMiscellaneousOtherProvisions');
DECLARE @TradeAndOtherCurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentPayables');
DECLARE @TradeAndOtherCurrentPayablesToTradeSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentPayablesToTradeSuppliers');
DECLARE @TradeAndOtherCurrentPayablesToRelatedParties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentPayablesToRelatedParties');
DECLARE @AccrualsAndDeferredIncomeClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AccrualsAndDeferredIncomeClassifiedAsCurrent');
DECLARE @DeferredIncomeClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredIncomeClassifiedAsCurrent');
DECLARE @RentDeferredIncomeClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RentDeferredIncomeClassifiedAsCurrent');
DECLARE @AccrualsClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AccrualsClassifiedAsCurrent');
DECLARE @ShorttermEmployeeBenefitsAccruals INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermEmployeeBenefitsAccruals');
DECLARE @CurrentBilledButNotIssuedExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentBilledButNotIssuedExtension');
DECLARE @CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax');
DECLARE @CurrentValueAddedTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentValueAddedTaxPayables');
DECLARE @CurrentExciseTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentExciseTaxPayables');
DECLARE @CurrentSocialSecurityPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentSocialSecurityPayablesExtension');
DECLARE @CurrentZakatPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentZakatPayablesExtension');
DECLARE @CurrentEmployeeIncomeTaxPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentEmployeeIncomeTaxPayablesExtension');
DECLARE @CurrentEmployeeStampTaxPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentEmployeeStampTaxPayablesExtension');
DECLARE @ProvidentFundPayableExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProvidentFundPayableExtension');
DECLARE @WithholdingTaxPayableExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WithholdingTaxPayableExtension');
DECLARE @CostSharingPayableExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CostSharingPayableExtension');
DECLARE @DividendTaxPayableExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DividendTaxPayableExtension');
DECLARE @ProfitTaxPayableExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfitTaxPayableExtension');
DECLARE @CurrentRetentionPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentRetentionPayables');
DECLARE @OtherCurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentPayables');
DECLARE @CurrentTaxLiabilitiesCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxLiabilitiesCurrent');
DECLARE @OtherCurrentFinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentFinancialLiabilities');
DECLARE @CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossAbstract INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossAbstract');
DECLARE @CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading');
DECLARE @CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition');
DECLARE @CurrentFinancialLiabilitiesAtAmortisedCost INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentFinancialLiabilitiesAtAmortisedCost');
DECLARE @OtherCurrentNonfinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentNonfinancialLiabilities');
DECLARE @LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale');
DECLARE @IncomeStatementAbstract INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IncomeStatementAbstract');
DECLARE @ProfitLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfitLoss');
DECLARE @ProfitLossFromContinuingOperations INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfitLossFromContinuingOperations');
DECLARE @ProfitLossBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfitLossBeforeTax');
DECLARE @ProfitLossFromOperatingActivities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfitLossFromOperatingActivities');
DECLARE @Revenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Revenue');
DECLARE @RevenueFromSaleOfGoods INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromSaleOfGoods');
DECLARE @RevenueFromSaleOfFoodAndBeverage INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromSaleOfFoodAndBeverage');
DECLARE @RevenueFromSaleOfAgriculturalProduce INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromSaleOfAgriculturalProduce');
DECLARE @RevenueFromRenderingOfServices INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromRenderingOfServices');
DECLARE @RevenueFromConstructionContracts INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromConstructionContracts');
DECLARE @RevenueFromRoyalties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromRoyalties');
DECLARE @LicenceFeeIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LicenceFeeIncome');
DECLARE @FranchiseFeeIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FranchiseFeeIncome');
DECLARE @RevenueFromInterest INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromInterest');
DECLARE @RevenueFromDividends INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromDividends');
DECLARE @OtherRevenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherRevenue');
DECLARE @OtherIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherIncome');
DECLARE @ChangesInInventoriesOfFinishedGoodsAndWorkInProgress INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ChangesInInventoriesOfFinishedGoodsAndWorkInProgress');
DECLARE @OtherWorkPerformedByEntityAndCapitalised INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherWorkPerformedByEntityAndCapitalised');
DECLARE @ExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ExpenseByNature');
DECLARE @RawMaterialsAndConsumablesUsed INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RawMaterialsAndConsumablesUsed');
DECLARE @CostOfMerchandiseSold INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CostOfMerchandiseSold');
DECLARE @ServicesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ServicesExpense');
DECLARE @InsuranceExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InsuranceExpense');
DECLARE @ProfessionalFeesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfessionalFeesExpense');
DECLARE @TransportationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TransportationExpense');
DECLARE @BankAndSimilarCharges INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'BankAndSimilarCharges');
DECLARE @TravelExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TravelExpense');
DECLARE @CommunicationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CommunicationExpense');
DECLARE @UtilitiesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'UtilitiesExpense');
DECLARE @AdvertisingExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AdvertisingExpense');
DECLARE @EmployeeBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'EmployeeBenefitsExpense');
DECLARE @ShorttermEmployeeBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermEmployeeBenefitsExpense');
DECLARE @WagesAndSalaries INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WagesAndSalaries');
DECLARE @SocialSecurityContributions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'SocialSecurityContributions');
DECLARE @OtherShorttermEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherShorttermEmployeeBenefits');
DECLARE @PostemploymentBenefitExpenseDefinedContributionPlans INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PostemploymentBenefitExpenseDefinedContributionPlans');
DECLARE @PostemploymentBenefitExpenseDefinedBenefitPlans INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PostemploymentBenefitExpenseDefinedBenefitPlans');
DECLARE @TerminationBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TerminationBenefitsExpense');
DECLARE @OtherLongtermBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherLongtermBenefits');
DECLARE @OtherEmployeeExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherEmployeeExpense');
DECLARE @DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss');
DECLARE @DepreciationAndAmortisationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationAndAmortisationExpense');
DECLARE @DepreciationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationExpense');
DECLARE @AmortisationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AmortisationExpense');
DECLARE @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss');
DECLARE @WritedownsReversalsOfInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WritedownsReversalsOfInventories');
DECLARE @WritedownsReversalsOfPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'WritedownsReversalsOfPropertyPlantAndEquipment');
DECLARE @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossTradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossTradeReceivables');
DECLARE @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossLoansAndAdvances INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossLoansAndAdvances');
DECLARE @TaxExpenseOtherThanIncomeTaxExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TaxExpenseOtherThanIncomeTaxExpense');
DECLARE @OtherExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherExpenseByNature');
DECLARE @UnallowedExpensesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'UnallowedExpensesExtension');
DECLARE @OtherGainsLosses INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherGainsLosses');
DECLARE @GainsLossesOnDisposalsOfPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnDisposalsOfPropertyPlantAndEquipment');
DECLARE @GainsOnDisposalsOfPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsOnDisposalsOfPropertyPlantAndEquipment');
DECLARE @LossesOnDisposalsOfPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LossesOnDisposalsOfPropertyPlantAndEquipment');
DECLARE @GainsLossesOnDisposalsOfInvestmentProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnDisposalsOfInvestmentProperties');
DECLARE @GainsOnDisposalsOfInvestmentProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsOnDisposalsOfInvestmentProperties');
DECLARE @LossesOnDisposalsOfInvestmentProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LossesOnDisposalsOfInvestmentProperties');
DECLARE @GainsLossesOnDisposalsOfInvestments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnDisposalsOfInvestments');
DECLARE @GainsOnDisposalsOfInvestments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsOnDisposalsOfInvestments');
DECLARE @LossesOnDisposalsOfInvestments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LossesOnDisposalsOfInvestments');
DECLARE @GainsLossesOnExchangeDifferencesOnTranslationRecognisedInProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnExchangeDifferencesOnTranslationRecognisedInProfitOrLoss');
DECLARE @NetForeignExchangeGain INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NetForeignExchangeGain');
DECLARE @NetForeignExchangeLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NetForeignExchangeLoss');
DECLARE @GainsLossesOnNetMonetaryPosition INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnNetMonetaryPosition');
DECLARE @GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost');
DECLARE @FinanceIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FinanceIncome');
DECLARE @FinanceCosts INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FinanceCosts');
DECLARE @ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9 INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9');
DECLARE @ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod');
DECLARE @OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates');
DECLARE @GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue');
DECLARE @CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThrou INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory');
DECLARE @HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions');
DECLARE @IncomeTaxExpenseContinuingOperations INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IncomeTaxExpenseContinuingOperations');
DECLARE @ProfitLossFromDiscontinuedOperations INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ProfitLossFromDiscontinuedOperations');
DECLARE @OtherComprehensiveIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncome');
DECLARE @ComponentsOfOtherComprehensiveIncomeThatWillNotBeReclassifiedToProfitOrLossBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ComponentsOfOtherComprehensiveIncomeThatWillNotBeReclassifiedToProfitOrLossBeforeTax');
DECLARE @OtherComprehensiveIncomeBeforeTaxGainsLossesFromInvestmentsInEquityInstruments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxGainsLossesFromInvestmentsInEquityInstruments');
DECLARE @OtherComprehensiveIncomeBeforeTaxGainsLossesOnRevaluation INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnRevaluation');
DECLARE @OtherComprehensiveIncomeBeforeTaxGainsLossesOnRemeasurementsOfDefinedBenefitPlans INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnRemeasurementsOfDefinedBenefitPlans');
DECLARE @OtherComprehensiveIncomeBeforeTaxChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability');
DECLARE @OtherComprehensiveIncomeBeforeTaxGainsLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments');
DECLARE @OtherComprehensiveIncomeBeforeTaxInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotB INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss');
DECLARE @ShareOfOtherComprehensiveIncomeOfAssociatesAndJointVenturesAccountedForUsingEquityMethodThatWillNotBeReclassifiedToProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShareOfOtherComprehensiveIncomeOfAssociatesAndJointVenturesAccountedForUsingEquityMethodThatWillNotBeReclassifiedToProfitOrLossBeforeTax');
DECLARE @ComponentsOfOtherComprehensiveIncomeThatWillBeReclassifiedToProfitOrLossBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ComponentsOfOtherComprehensiveIncomeThatWillBeReclassifiedToProfitOrLossBeforeTax');
DECLARE @OtherComprehensiveIncomeBeforeTaxExchangeDifferencesOnTranslation INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxExchangeDifferencesOnTranslation');
DECLARE @GainsLossesOnExchangeDifferencesOnTranslationBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnExchangeDifferencesOnTranslationBeforeTax');
DECLARE @ReclassificationAdjustmentsOnExchangeDifferencesOnTranslationBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReclassificationAdjustmentsOnExchangeDifferencesOnTranslationBeforeTax');
DECLARE @OtherComprehensiveIncomeBeforeTaxAvailableforsaleFinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherComprehensiveIncomeBeforeTaxAvailableforsaleFinancialAssets');
DECLARE @GainsLossesOnRemeasuringAvailableforsaleFinancialAssetsBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainsLossesOnRemeasuringAvailableforsaleFinancialAssetsBeforeTax');
DECLARE @ReclassificationAdjustmentsOnAvailableforsaleFinancialAssetsBeforeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReclassificationAdjustmentsOnAvailableforsaleFinancialAssetsBeforeTax');
DECLARE @ControlAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ControlAccountsExtension');
DECLARE @TradersControlAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradersControlAccountsExtension');
DECLARE @SuppliersControlAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'SuppliersControlAccountsExtension');
DECLARE @CashPaymentsToSuppliersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashPaymentsToSuppliersControlExtension');
DECLARE @ReceiptsAtPointInTimeFromSuppliersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReceiptsAtPointInTimeFromSuppliersControlExtension');
DECLARE @ReceiptsOverPeriodOfTimeFromSuppliersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ReceiptsOverPeriodOfTimeFromSuppliersControlExtension');
DECLARE @CustomersControlAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CustomersControlAccountsExtension');
DECLARE @CashReceiptsFromCustomersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashReceiptsFromCustomersControlExtension');
DECLARE @IssuesAtPointInTimeToCustomersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IssuesAtPointInTimeToCustomersControlExtension');
DECLARE @IssuesOverPeriodOfTimeToCustomersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IssuesOverPeriodOfTimeToCustomersControlExtension');
DECLARE @PayrollControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PayrollControlExtension');
DECLARE @CashPaymentsToEmployeesControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashPaymentsToEmployeesControlExtension');
DECLARE @OthersAccountsControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OthersAccountsControlExtension');
DECLARE @CashPaymentsToOthersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashPaymentsToOthersControlExtension');
DECLARE @CashReceiptsFromOthersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashReceiptsFromOthersControlExtension');
DECLARE @GuaranteesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GuaranteesExtension');
DECLARE @CollectionGuaranteeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CollectionGuaranteeExtension');
DECLARE @DishonouredGuaranteeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DishonouredGuaranteeExtension');
DECLARE @MigrationAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'MigrationAccountsExtension');

DECLARE @ChangesInPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInPropertyPlantAndEquipment');
DECLARE @AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment');
DECLARE @AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment');
DECLARE @IncreaseDecreaseThroughNetExchangeDifferencesPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughNetExchangeDifferencesPropertyPlantAndEquipment');
DECLARE @DepreciationPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DepreciationPropertyPlantAndEquipment');
DECLARE @ImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment');
DECLARE @ReversalOfImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment');
DECLARE @RevaluationIncreaseDecreasePropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'RevaluationIncreaseDecreasePropertyPlantAndEquipment');
DECLARE @ImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment');
DECLARE @ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment');
DECLARE @IncreaseDecreaseThroughTransfersAndOtherChangesPropertyPlantAndEquipmentAbstract INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersAndOtherChangesPropertyPlantAndEquipmentAbstract');
DECLARE @IncreaseDecreaseThroughTransfersPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersPropertyPlantAndEquipment');
DECLARE @IncreaseDecreaseThroughOtherChangesPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughOtherChangesPropertyPlantAndEquipment');
DECLARE @DisposalsAndRetirementsPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DisposalsAndRetirementsPropertyPlantAndEquipment');
DECLARE @DisposalsPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DisposalsPropertyPlantAndEquipment');
DECLARE @RetirementsPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'RetirementsPropertyPlantAndEquipment');
DECLARE @DecreaseThroughClassifiedAsHeldForSalePropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughClassifiedAsHeldForSalePropertyPlantAndEquipment');
DECLARE @DecreaseThroughLossOfControlOfSubsidiaryPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughLossOfControlOfSubsidiaryPropertyPlantAndEquipment');
DECLARE @ChangesInInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInInvestmentProperty');
DECLARE @AdditionsOtherThanThroughBusinessCombinationsInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsInvestmentProperty');
DECLARE @AdditionsFromSubsequentExpenditureRecognisedAsAssetInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsFromSubsequentExpenditureRecognisedAsAssetInvestmentProperty');
DECLARE @AdditionsFromAcquisitionsInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsFromAcquisitionsInvestmentProperty');
DECLARE @AcquisitionsThroughBusinessCombinationsInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AcquisitionsThroughBusinessCombinationsInvestmentProperty');
DECLARE @IncreaseDecreaseThroughNetExchangeDifferencesInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughNetExchangeDifferencesInvestmentProperty');
DECLARE @DepreciationInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DepreciationInvestmentProperty');
DECLARE @ImpairmentLossRecognisedInProfitOrLossInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInProfitOrLossInvestmentProperty');
DECLARE @ReversalOfImpairmentLossRecognisedInProfitOrLossInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfImpairmentLossRecognisedInProfitOrLossInvestmentProperty');
DECLARE @GainsLossesOnFairValueAdjustmentInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'GainsLossesOnFairValueAdjustmentInvestmentProperty');
DECLARE @TransferFromToInventoriesAndOwnerOccupiedPropertyInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'TransferFromToInventoriesAndOwnerOccupiedPropertyInvestmentProperty');
DECLARE @TransferFromInvestmentPropertyUnderConstructionOrDevelopmentInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'TransferFromInvestmentPropertyUnderConstructionOrDevelopmentInvestmentProperty');
DECLARE @DisposalsInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DisposalsInvestmentProperty');
DECLARE @DecreaseThroughClassifiedAsHeldForSaleInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughClassifiedAsHeldForSaleInvestmentProperty');
DECLARE @IncreaseDecreaseThroughOtherChangesInvestmentProperty INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughOtherChangesInvestmentProperty');
DECLARE @ChangesInGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInGoodwill');
DECLARE @SubsequentRecognitionOfDeferredTaxAssetsGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'SubsequentRecognitionOfDeferredTaxAssetsGoodwill');
DECLARE @IncreaseDecreaseThroughTransfersAndOtherChangesGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersAndOtherChangesGoodwill');
DECLARE @AdditionalRecognitionGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionalRecognitionGoodwill');
DECLARE @DecreaseThroughClassifiedAsHeldForSaleGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughClassifiedAsHeldForSaleGoodwill');
DECLARE @GoodwillDerecognisedWithoutHavingPreviouslyBeenIncludedInDisposalGroupClassifiedAsHeldForSale INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'GoodwillDerecognisedWithoutHavingPreviouslyBeenIncludedInDisposalGroupClassifiedAsHeldForSale');
DECLARE @ImpairmentLossRecognisedInProfitOrLossGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInProfitOrLossGoodwill');
DECLARE @IncreaseDecreaseThroughNetExchangeDifferencesGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughNetExchangeDifferencesGoodwill');
DECLARE @ChangesInIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInIntangibleAssetsOtherThanGoodwill');
DECLARE @AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill');
DECLARE @AcquisitionsThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AcquisitionsThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill');
DECLARE @IncreaseDecreaseThroughNetExchangeDifferencesIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughNetExchangeDifferencesIntangibleAssetsOtherThanGoodwill');
DECLARE @AmortisationIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AmortisationIntangibleAssetsOtherThanGoodwill');
DECLARE @ImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill');
DECLARE @ReversalOfImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill');
DECLARE @RevaluationIncreaseDecreaseIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'RevaluationIncreaseDecreaseIntangibleAssetsOtherThanGoodwill');
DECLARE @ImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill');
DECLARE @ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill');
DECLARE @DecreaseThroughClassifiedAsHeldForSaleIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughClassifiedAsHeldForSaleIntangibleAssetsOtherThanGoodwill');
DECLARE @DecreaseThroughLossOfControlOfSubsidiaryIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughLossOfControlOfSubsidiaryIntangibleAssetsOtherThanGoodwill');
DECLARE @DisposalsAndRetirementsIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DisposalsAndRetirementsIntangibleAssetsOtherThanGoodwill');
DECLARE @DisposalsIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DisposalsIntangibleAssetsOtherThanGoodwill');
DECLARE @RetirementsIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'RetirementsIntangibleAssetsOtherThanGoodwill');
DECLARE @IncreaseDecreaseThroughTransfersAndOtherChangesIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersAndOtherChangesIntangibleAssetsOtherThanGoodwill');
DECLARE @IncreaseDecreaseThroughTransfersIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersIntangibleAssetsOtherThanGoodwill');
DECLARE @IncreaseDecreaseThroughOtherChangesIntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughOtherChangesIntangibleAssetsOtherThanGoodwill');
DECLARE @ChangesInBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInBiologicalAssets');
DECLARE @AdditionsOtherThanThroughBusinessCombinationsBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsOtherThanThroughBusinessCombinationsBiologicalAssets');
DECLARE @AdditionsFromSubsequentExpenditureRecognisedAsAssetBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsFromSubsequentExpenditureRecognisedAsAssetBiologicalAssets');
DECLARE @AdditionsFromPurchasesBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionsFromPurchasesBiologicalAssets');
DECLARE @AcquisitionsThroughBusinessCombinationsBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AcquisitionsThroughBusinessCombinationsBiologicalAssets');
DECLARE @IncreaseDecreaseThroughNetExchangeDifferencesBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughNetExchangeDifferencesBiologicalAssets');
DECLARE @DepreciationBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DepreciationBiologicalAssets');
DECLARE @ImpairmentLossRecognisedInProfitOrLossBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ImpairmentLossRecognisedInProfitOrLossBiologicalAssets');
DECLARE @ReversalOfImpairmentLossRecognisedInProfitOrLossBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfImpairmentLossRecognisedInProfitOrLossBiologicalAssets');
DECLARE @GainsLossesOnFairValueAdjustmentBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'GainsLossesOnFairValueAdjustmentBiologicalAssets');
DECLARE @GainsLossesOnFairValueAdjustmentAttributableToPhysicalChangesBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'GainsLossesOnFairValueAdjustmentAttributableToPhysicalChangesBiologicalAssets');
DECLARE @GainsLossesOnFairValueAdjustmentAttributableToPriceChangesBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'GainsLossesOnFairValueAdjustmentAttributableToPriceChangesBiologicalAssets');
DECLARE @IncreaseDecreaseThroughTransfersAndOtherChangesBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersAndOtherChangesBiologicalAssets');
DECLARE @DisposalsBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DisposalsBiologicalAssets');
DECLARE @DecreaseDueToHarvestBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseDueToHarvestBiologicalAssets');
DECLARE @DecreaseThroughClassifiedAsHeldForSaleBiologicalAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughClassifiedAsHeldForSaleBiologicalAssets');
DECLARE @IncreaseDecreaseInCashAndCashEquivalents INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseInCashAndCashEquivalents');
DECLARE @IncreaseDecreaseInCashAndCashEquivalentsBeforeEffectOfExchangeRateChanges INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseInCashAndCashEquivalentsBeforeEffectOfExchangeRateChanges');
DECLARE @CashFlowsFromUsedInOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashFlowsFromUsedInOperatingActivities');
DECLARE @CashFlowsFromUsedInOperations INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashFlowsFromUsedInOperations');
DECLARE @ReceiptsFromSalesOfGoodsAndRenderingOfServices INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReceiptsFromSalesOfGoodsAndRenderingOfServices');
DECLARE @ReceiptsFromRoyaltiesFeesCommissionsAndOtherRevenue INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReceiptsFromRoyaltiesFeesCommissionsAndOtherRevenue');
DECLARE @ReceiptsFromContractsHeldForDealingOrTradingPurpose INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReceiptsFromContractsHeldForDealingOrTradingPurpose');
DECLARE @ReceiptsFromPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReceiptsFromPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits');
DECLARE @ReceiptsFromRentsAndSubsequentSalesOfSuchAssets INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReceiptsFromRentsAndSubsequentSalesOfSuchAssets');
DECLARE @OtherCashReceiptsFromOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherCashReceiptsFromOperatingActivities');
DECLARE @PaymentsToSuppliersForGoodsAndServices INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsToSuppliersForGoodsAndServices');
DECLARE @PaymentsFromContractsHeldForDealingOrTradingPurpose INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsFromContractsHeldForDealingOrTradingPurpose');
DECLARE @PaymentsToAndOnBehalfOfEmployees INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsToAndOnBehalfOfEmployees');
DECLARE @PaymentsForPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsForPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits');
DECLARE @PaymentsToManufactureOrAcquireAssetsHeldForRentalToOthersAndSubsequentlyHeldForSale INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsToManufactureOrAcquireAssetsHeldForRentalToOthersAndSubsequentlyHeldForSale');
DECLARE @OtherCashPaymentsFromOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherCashPaymentsFromOperatingActivities');
DECLARE @DividendsPaidClassifiedAsOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DividendsPaidClassifiedAsOperatingActivities');
DECLARE @DividendsReceivedClassifiedAsOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DividendsReceivedClassifiedAsOperatingActivities');
DECLARE @InterestPaidClassifiedAsOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InterestPaidClassifiedAsOperatingActivities');
DECLARE @InterestReceivedClassifiedAsOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InterestReceivedClassifiedAsOperatingActivities');
DECLARE @IncomeTaxesPaidRefundClassifiedAsOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncomeTaxesPaidRefundClassifiedAsOperatingActivities');
DECLARE @OtherInflowsOutflowsOfCashClassifiedAsOperatingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherInflowsOutflowsOfCashClassifiedAsOperatingActivities');
DECLARE @CashFlowsFromUsedInInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashFlowsFromUsedInInvestingActivities');
DECLARE @CashFlowsFromLosingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashFlowsFromLosingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities');
DECLARE @CashFlowsUsedInObtainingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashFlowsUsedInObtainingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities');
DECLARE @OtherCashReceiptsFromSalesOfEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherCashReceiptsFromSalesOfEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities');
DECLARE @OtherCashPaymentsToAcquireEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherCashPaymentsToAcquireEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities');
DECLARE @OtherCashReceiptsFromSalesOfInterestsInJointVenturesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherCashReceiptsFromSalesOfInterestsInJointVenturesClassifiedAsInvestingActivities');
DECLARE @OtherCashPaymentsToAcquireInterestsInJointVenturesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherCashPaymentsToAcquireInterestsInJointVenturesClassifiedAsInvestingActivities');
DECLARE @ProceedsFromSalesOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromSalesOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities');
DECLARE @PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities');
DECLARE @ProceedsFromSalesOfIntangibleAssetsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromSalesOfIntangibleAssetsClassifiedAsInvestingActivities');
DECLARE @PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities');
DECLARE @ProceedsFromOtherLongtermAssetsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromOtherLongtermAssetsClassifiedAsInvestingActivities');
DECLARE @PurchaseOfOtherLongtermAssetsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PurchaseOfOtherLongtermAssetsClassifiedAsInvestingActivities');
DECLARE @ProceedsFromGovernmentGrantsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromGovernmentGrantsClassifiedAsInvestingActivities');
DECLARE @CashAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities');
DECLARE @CashReceiptsFromRepaymentOfAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashReceiptsFromRepaymentOfAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities');
DECLARE @CashPaymentsForFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashPaymentsForFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities');
DECLARE @CashReceiptsFromFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashReceiptsFromFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities');
DECLARE @DividendsReceivedClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DividendsReceivedClassifiedAsInvestingActivities');
DECLARE @InterestPaidClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InterestPaidClassifiedAsInvestingActivities');
DECLARE @InterestReceivedClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InterestReceivedClassifiedAsInvestingActivities');
DECLARE @IncomeTaxesPaidRefundClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncomeTaxesPaidRefundClassifiedAsInvestingActivities');
DECLARE @OtherInflowsOutflowsOfCashClassifiedAsInvestingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherInflowsOutflowsOfCashClassifiedAsInvestingActivities');
DECLARE @CashFlowsFromUsedInFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'CashFlowsFromUsedInFinancingActivities');
DECLARE @ProceedsFromChangesInOwnershipInterestsInSubsidiaries INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromChangesInOwnershipInterestsInSubsidiaries');
DECLARE @PaymentsFromChangesInOwnershipInterestsInSubsidiaries INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsFromChangesInOwnershipInterestsInSubsidiaries');
DECLARE @ProceedsFromIssuingShares INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromIssuingShares');
DECLARE @ProceedsFromIssuingOtherEquityInstruments INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromIssuingOtherEquityInstruments');
DECLARE @PaymentsToAcquireOrRedeemEntitysShares INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsToAcquireOrRedeemEntitysShares');
DECLARE @PaymentsOfOtherEquityInstruments INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsOfOtherEquityInstruments');
DECLARE @ProceedsFromBorrowingsClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromBorrowingsClassifiedAsFinancingActivities');
DECLARE @RepaymentsOfBorrowingsClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'RepaymentsOfBorrowingsClassifiedAsFinancingActivities');
DECLARE @PaymentsOfLeaseLiabilitiesClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PaymentsOfLeaseLiabilitiesClassifiedAsFinancingActivities');
DECLARE @ProceedsFromGovernmentGrantsClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProceedsFromGovernmentGrantsClassifiedAsFinancingActivities');
DECLARE @DividendsPaidClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DividendsPaidClassifiedAsFinancingActivities');
DECLARE @InterestPaidClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InterestPaidClassifiedAsFinancingActivities');
DECLARE @IncomeTaxesPaidRefundClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncomeTaxesPaidRefundClassifiedAsFinancingActivities');
DECLARE @OtherInflowsOutflowsOfCashClassifiedAsFinancingActivities INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'OtherInflowsOutflowsOfCashClassifiedAsFinancingActivities');
DECLARE @InternalCashTransferExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InternalCashTransferExtension');
DECLARE @EffectOfExchangeRateChangesOnCashAndCashEquivalents INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'EffectOfExchangeRateChangesOnCashAndCashEquivalents');
DECLARE @ChangesInEquity INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInEquity');
DECLARE @IssueOfEquity INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IssueOfEquity');
DECLARE @DividendsPaid INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DividendsPaid');
DECLARE @IncreaseDecreaseThroughOtherContributionsByOwners INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughOtherContributionsByOwners');
DECLARE @IncreaseDecreaseThroughOtherDistributionsToOwners INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughOtherDistributionsToOwners');
DECLARE @IncreaseDecreaseThroughTransfersAndOtherChangesEquity INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersAndOtherChangesEquity');
DECLARE @IncreaseDecreaseThroughTreasuryShareTransactions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTreasuryShareTransactions');
DECLARE @IncreaseDecreaseThroughChangesInOwnershipInterestsInSubsidiariesThatDoNotResultInLossOfControl INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughChangesInOwnershipInterestsInSubsidiariesThatDoNotResultInLossOfControl');
DECLARE @IncreaseDecreaseThroughSharebasedPaymentTransactions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughSharebasedPaymentTransactions');
DECLARE @AmountRemovedFromReserveOfCashFlowHedgesAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitme INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AmountRemovedFromReserveOfCashFlowHedgesAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied');
DECLARE @AmountRemovedFromReserveOfChangeInValueOfTimeValueOfOptionsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiab INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AmountRemovedFromReserveOfChangeInValueOfTimeValueOfOptionsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied');
DECLARE @AmountRemovedFromReserveOfChangeInValueOfForwardElementsOfForwardContractsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfin INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AmountRemovedFromReserveOfChangeInValueOfForwardElementsOfForwardContractsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied');
DECLARE @AmountRemovedFromReserveOfChangeInValueOfForeignCurrencyBasisSpreadsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancial INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AmountRemovedFromReserveOfChangeInValueOfForeignCurrencyBasisSpreadsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied');
DECLARE @ChangesInOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInOtherProvisions');
DECLARE @AdditionalProvisionsOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AdditionalProvisionsOtherProvisions');
DECLARE @NewProvisionsOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'NewProvisionsOtherProvisions');
DECLARE @IncreaseDecreaseInExistingProvisionsOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseInExistingProvisionsOtherProvisions');
DECLARE @AcquisitionsThroughBusinessCombinationsOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'AcquisitionsThroughBusinessCombinationsOtherProvisions');
DECLARE @ProvisionUsedOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ProvisionUsedOtherProvisions');
DECLARE @UnusedProvisionReversedOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'UnusedProvisionReversedOtherProvisions');
DECLARE @IncreaseDecreaseThroughTimeValueOfMoneyAdjustmentOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTimeValueOfMoneyAdjustmentOtherProvisions');
DECLARE @IncreaseDecreaseThroughChangeInDiscountRateOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughChangeInDiscountRateOtherProvisions');
DECLARE @IncreaseDecreaseThroughNetExchangeDifferencesOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughNetExchangeDifferencesOtherProvisions');
DECLARE @DecreaseThroughLossOfControlOfSubsidiaryOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'DecreaseThroughLossOfControlOfSubsidiaryOtherProvisions');
DECLARE @IncreaseDecreaseThroughTransfersAndOtherChangesOtherProvisions INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughTransfersAndOtherChangesOtherProvisions');
DECLARE @ChangesInInventories INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ChangesInInventories');
DECLARE @ReceiptsReturnsThroughPurchaseExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReceiptsReturnsThroughPurchaseExtension');
DECLARE @IncreaseDecreaseThroughProductionExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughProductionExtension');
DECLARE @IncreaseDecreaseThroughMaintenanceExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThroughMaintenanceExtension');
DECLARE @IncreaseDecreaseThrougConsumptionExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'IncreaseDecreaseThrougConsumptionExtension');
DECLARE @ReturnsIssuesThroughSaleExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReturnsIssuesThroughSaleExtension');
DECLARE @PropertyPlantAndEquipmentClassifiedDeclassifiedAsInventoryExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'PropertyPlantAndEquipmentClassifiedDeclassifiedAsInventoryExtension');
DECLARE @InventoryOverageShortageExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InventoryOverageShortageExtension');
DECLARE @InventoryWritedown2011 INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InventoryWritedown2011');
DECLARE @ReversalOfInventoryWritedown INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'ReversalOfInventoryWritedown');
DECLARE @InternalInventoryTransferExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N'InternalInventoryTransferExtension');
--Declarations
DECLARE @CreditorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Creditor');
DECLARE @DebtorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Debtor');
DECLARE @OwnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Owner');
DECLARE @PartnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Partner');
DECLARE @SupplierRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Supplier');
DECLARE @CustomerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Customer');
DECLARE @EmployeeRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Employee');
DECLARE @BankBranchRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BankBranch');

DECLARE @BankAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'BankAccount');
DECLARE @CashOnHandAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'CashOnHandAccount');
DECLARE @WarehouseCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Warehouse');
DECLARE @PPECustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'PPECustody');
DECLARE @RentalCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Rental');
DECLARE @TransitCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'TransitCustody');
DECLARE @TaskCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'TaskCustody');
--Declarations
DECLARE @LandMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LandMember');
DECLARE @BuildingsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BuildingsMember');
DECLARE @MachineryMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MachineryMember');
DECLARE @MotorVehiclesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MotorVehiclesMember');
DECLARE @FixturesAndFittingsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FixturesAndFittingsMember');
DECLARE @OfficeEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OfficeEquipmentMember');
DECLARE @ComputerEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ComputerEquipmentMember');
DECLARE @CommunicationAndNetworkEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CommunicationAndNetworkEquipmentMember');
DECLARE @NetworkInfrastructureMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'NetworkInfrastructureMember');
DECLARE @BearerPlantsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BearerPlantsMember');
DECLARE @TangibleExplorationAndEvaluationAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TangibleExplorationAndEvaluationAssetsMember');
DECLARE @MiningAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MiningAssetsMember');
DECLARE @OilAndGasAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OilAndGasAssetsMember');
DECLARE @PowerGeneratingAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PowerGeneratingAssetsMember');
DECLARE @LeaseholdImprovementsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LeaseholdImprovementsMember');
DECLARE @ConstructionInProgressMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ConstructionInProgressMember');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember');
DECLARE @OtherPropertyPlantAndEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OtherPropertyPlantAndEquipmentMember');
DECLARE @InvestmentPropertyCompletedMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'InvestmentPropertyCompletedMember');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'InvestmentPropertyUnderConstructionOrDevelopmentMember');
DECLARE @MerchandiseRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Merchandise');
DECLARE @CurrentFoodAndBeverageRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentFoodAndBeverage');
DECLARE @CurrentAgriculturalProduceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentAgriculturalProduce');
DECLARE @FinishedGoodsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedGoods');
DECLARE @PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness');
DECLARE @WorkInProgressRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'WorkInProgress');
DECLARE @RawMaterialsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RawMaterials');
DECLARE @ProductionSuppliesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ProductionSupplies');
DECLARE @CurrentPackagingAndStorageMaterialsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentPackagingAndStorageMaterials');
DECLARE @SparePartsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SpareParts');
DECLARE @CurrentFuelRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentFuel');
DECLARE @OtherInventoriesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OtherInventories');
DECLARE @TradeMedicineRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeMedicine');
DECLARE @TradeConstructionMaterialRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeConstructionMaterial');
DECLARE @TradeSparePartRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeSparePart');
DECLARE @FinishedGrainRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedGrain');
DECLARE @ByproductGrainRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ByproductGrain');
DECLARE @FinishedVehicleRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedVehicle');
DECLARE @FinishedOilRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedOil');
DECLARE @ByproductOilRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ByproductOil');
DECLARE @RawGrainRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RawGrain');
DECLARE @RawVehicleRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RawVehicle');
DECLARE @RevenueServiceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RevenueService');
DECLARE @EmployeeBenefitRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeBenefit');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CheckReceived');
DECLARE @AccrualsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Accruals');
DECLARE @AccruedIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'AccruedIncome');
DECLARE @EmployeeLoanRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeLoan');
DECLARE @CurrentBilledButNotIssuedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentBilledButNotIssued');
DECLARE @DeferredIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'DeferredIncome');
DECLARE @PrepaymentsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Prepayments');
DECLARE @SalaryAdvanceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SalaryAdvance');
DECLARE @ReceivablesFromRentalOfPropertiesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ReceivablesFromRentalOfProperties');
DECLARE @ReceivablesFromSaleOfPropertiesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ReceivablesFromSaleOfProperties');
DECLARE @RefundsProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RefundsProvision');
DECLARE @RentDeferredIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RentDeferredIncome');
DECLARE @RestructuringProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RestructuringProvision');
DECLARE @RetentionPayableRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RetentionPayable');
DECLARE @TradePayableRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradePayable');
DECLARE @TradeReceivableRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeReceivable');
DECLARE @WarrantyProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'WarrantyProvision');

DECLARE @LineDefinitions dbo.LineDefinitionList;
DECLARE @LineDefinitionColumns dbo.LineDefinitionColumnList;
DECLARE @LineDefinitionGenerateParameters [LineDefinitionGenerateParameterList];
DECLARE @LineDefinitionEntries dbo.LineDefinitionEntryList;
DECLARE @LineDefinitionEntryCustodyDefinitions LineDefinitionEntryCustodyDefinitionList;
DECLARE @LineDefinitionEntryResourceDefinitions LineDefinitionEntryResourceDefinitionList;
DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];
DECLARE @Workflows dbo.[WorkflowList];
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;
DECLARE @ValidationErrorsJson nvarchar(max);

INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(1000, N'ManualLine', N'Making any accounting adjustment', N'Adjustment', N'Adjustments', 0, 0),
(1060, N'PPEFromSupplier', N'Receiving property, plant and equipment from supplier, invoiced separately', N'PPE Purchase', N'PPE Purchases', 0, 1),
(1260, N'InventoryFromSupplier', N'Receiving inventory from supplier/contractor', N'Stock Purchase', N'Stock Purchases', 0, 0),
(1330, N'RevenueFromInventory', N'Issuing inventory to customer, invoiced separately', N'Inventory (Sale)', N'Inventories (Sales)', 0, 0),
--(1350, N'RevenueFromPeriodService', N'Rendering period services to customer, invoiced separately', N'Lease Out', N'Leases Out', 0, 1),
(1360, N'RevenueFromInventoryWithPointInvoice', N'Issuing inventory to customer + point invoice', N'Inventory (Sale) + Invoice', N'Inventories (Sale) + Invoices', 0, 0),
--(1370, N'RevenueFromPointServiceWithPointInvoice', N'Rendering point services to customer + point invoice', N'Service (Sale) + Invoice (Point)', N'Services (Sales) + Invoices (Point)', 0, 1),
--(1380, N'RevenueFromPeriodServiceWithPeriodInvoice', N'Rendering period services to customer + period invoice', N'Service (Sale) + Invoice (Period)', N'Services (Sale) + Invoices (Period)', 0, 1),
(1410, N'CashFromCustomer', N'Collecting cash from customer/lessee, Invoiced separately', N'Cash Receipt (Sale)', N'Cash Receipts (Sale)', 0, 1),
(1420, N'CashFromCustomerWithWT', N'Collecting cash from customer/lessee with WT, Invoiced separately', N'Cash Receipt + WT', N'Cash Receipts + WT', 0, 1),
(1430, N'CashFromCustomerWithPointInvoice', N'Collecting cash from customer + point invoice', N'Cash Receipt + Point Invoice', N'Cash Receipts + Point Invoices', 0, 1),
--(1440, N'CashFromCustomerWithPeriodInvoice', N'Collecting cash from lessee + period invoice', N'Cash Receipt + Period Invoice', N'Cash Receipts + Period Invoices', 0, 1),
(1450, N'CashFromCustomerWithWTWithPointInvoice', N'Collecting cash from customer + WT + point invoice', N'Cash Receipt + WT + Point Invoice', N'Cash Receipts + WT + Point Invoices', 0, 1),
--(1460, N'CashFromCustomerWithWTWithPeriodInvoice', N'Collecting cash from lessee + WT + period invoice', N'Cash Receipt + WT + Period Invoice', N'Cash Receipts + WT + Period Invoices', 0, 1),
(1550, N'CashTransferExchange', N'Cash transfer and currency exchange', N'Cash Transfer & Exchange', N'Cash Transfers & Exchanges', 0, 1),
(1560, N'CashTransfer', N'Cash transfer, same currency', N'Cash Transfer', N'Cash Transfers', 0, 1),
(1570, N'CashExchange', N'Currency exchange, same account', N'Cash Exchange', N'Cash Exchanges', 0, 1),
(1660, N'CashToSupplierWithPointInvoice', N'Paying cash to supplier/lessor/.. + point invoice', N'Cash Payment + Point Invoice', N'Cash Payments + Point Invoices', 0, 1),
(1680, N'CashToSupplierWithPointInvoiceWithWT', N'Paying cash to supplier/lessor/.. + point invoice + WT', N'Cash Payment + Point Invoice + WT', N'Cash Payments + Point Invoices + WT', 0, 1),
(1730, N'SupplierWT', N'WT from supplier', N'WT (Purchase)', N'WT (Purchases)', 0, 1),
(1770, N'PointExpenseFromInventory', N'Issuing inventory to cost center (maintenance, job order, production line, construction project..)', N'Stock Consumption', N'Stock Consumptions', 0, 0),
(1780, N'PointExpenseFromSupplier', N'Receiving consumables and point services from supplier, invoiced separately', N'C/S Purchase', N'C/S Purchases', 0, 0)
--1000: ManualLine
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],[Direction], [ParentAccountTypeId]) VALUES (0,1000,+1, @StatementOfFinancialPositionAbstract);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],		[RequiredState],
													[ReadOnlyState],
													[InheritsFromHeader]) VALUES
(0,1000,	N'AccountId',	0,			N'Account',		4,4,0), -- together with properties
(1,1000,	N'Value',		0,			N'Debit',		4,4,0), -- see special case
(2,1000,	N'Value',		0,			N'Credit',		4,4,0),
(3,1000,	N'Memo',		0,			N'Memo',		4,4,1);
--1060: PPEFromSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
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
WHERE [Index] = 1060;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],			[EntryTypeId]) VALUES
(0,1060,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(1,1060,+1,	@PropertyPlantAndEquipment,	@AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment),
(2,1060,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1060,@PPECustodyCD),
(0,1,1060,@PPECustodyCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1060,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1060,	N'NotedRelationId',		2,	N'Supplier',		3,4,1),
(2,1060,	N'CustodyId',			0,	N'Custody',			5,5,0),
(3,1060,	N'ResourceId',			0,	N'Fixed Asset',		2,4,0),
(4,1060,	N'Quantity',			1,	N'Life/Usage',		2,4,0),
(5,1060,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,1060,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,1060,	N'MonetaryValue',		2,	N'Cost (VAT Excl.)',1,2,0),
(8,1060,	N'MonetaryValue',		0,	N'Residual Value',	1,2,0),
(10,1060,	N'PostingDate',			1,	N'Acquired On',		1,4,1),
(11,1060,	N'CenterId',			2,	N'Business Unit',	1,4,1);
--1260:InventoryFromSupplier
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue0]	= [MonetaryValue1],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 1260;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],										[EntryTypeId]) VALUES
(0,1260,+1,	@Inventories,											@ReceiptsReturnsThroughPurchaseExtension),
(1,1260,-1,	@ReceiptsAtPointInTimeFromSuppliersControlExtension,	NULL);
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,0,1260,@MerchandiseRD),
(1,0,1260,@CurrentFoodAndBeverageRD),
(2,0,1260,@CurrentAgriculturalProduceRD),
(3,0,1260,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(4,0,1260,@RawMaterialsRD),
(5,0,1260,@ProductionSuppliesRD),
(6,0,1260,@CurrentPackagingAndStorageMaterialsRD),
(7,0,1260,@SparePartsRD),
(8,0,1260,@CurrentFuelRD),
(9,0,1260,@OtherInventoriesRD),
(10,0,1260,@TradeMedicineRD),
(11,0,1260,@TradeConstructionMaterialRD),
(12,0,1260,@TradeSparePartRD),
(13,0,1260,@RawGrainRD),
(14,0,1260,@RawVehicleRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1260,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1260,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1260,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,1260,	N'CustodyId',			0,	N'Warehouse',		3,4,1),
(3,1260,	N'ResourceId',			0,	N'Item',			2,4,0),
(4,1260,	N'Quantity',			0,	N'Qty',				2,4,0),
(5,1260,	N'UnitId',				0,	N'Unit',			2,4,0),
(6,1260,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(7,1260,	N'MonetaryValue',		1,	N'Cost (VAT Excl.)',1,2,0),
(10,1260,	N'PostingDate',			1,	N'Received On',		1,4,1),
(11,1260,	N'CenterId',			1,	N'Business Unit',	1,4,1);
--1330:RevenueFromInventory
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[ParentAccountTypeId] = ATC.[Id]
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N''Inventories''
	),
	ResourceCosts AS (
		SELECT
		PWL.PostingDate, PWL.[CustodyId1],  PWL.[ResourceId1],
			SUM(E.[AlgebraicValue]) AS NetValue,
			SUM(E.[AlgebraicQuantity]) AS NetQuantity
		FROM map.[DetailsEntries]() E
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		JOIN @ProcessedWideLines PWL ON PWL.[ResourceId1] = E.[ResourceId] AND PWL.[CustodyId1] = E.[CustodyId] AND L.PostingDate <= PWL.[PostingDate]
		WHERE E.[AccountId] IN (SELECT [Id] FROM InventoryAccounts)
		AND L.[State] = 4
		GROUP BY PWL.PostingDate, PWL.[CustodyId1],  PWL.[ResourceId1]
	)	
	UPDATE PWL
	SET
		[CustodyId0]		= PWL.[CustodyId1],
		[CustodyId3]		= PWL.[CustodyId1],
		[CurrencyId0]		= PWL.[CurrencyId2],
		[CurrencyId1]		= PWL.[CurrencyId2],
		[CurrencyId3]		= PWL.[CurrencyId2],
		[CenterId1]			= COALESCE(PWL.[CenterId1], PWL.[CenterId2]),
		[CenterId0]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[CenterId3]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = PWL.[UnitId1],
		[ResourceId3]		= PWL.[ResourceId1], [Quantity3] = PWL.[Quantity1],	[UnitId3] = PWL.[UnitId1],
		[MonetaryValue0]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue1]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue2]	= [MonetaryValue3],
		[NotedRelationId0]	= [NotedRelationId3],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId3]),
		[NotedRelationId2]	= [NotedRelationId3]
	FROM @ProcessedWideLines PWL
	LEFT JOIN ResourceCosts RC ON PWL.[ResourceId1] = RC.[ResourceId1] AND PWL.[CustodyId1] = RC.[CustodyId1] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1330;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],									[EntryTypeId]) VALUES
(0,1330,+1,	@CostOfMerchandiseSold,								NULL),
(1,1330,-1,	@Inventories,										@ReturnsIssuesThroughSaleExtension),
(2,1330,+1,	@IssuesAtPointInTimeToCustomersControlExtension,	NULL),
(3,1330,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1330,@MerchandiseRD),
(1,1,1330,@CurrentFoodAndBeverageRD),
(2,1,1330,@CurrentAgriculturalProduceRD),
(3,1,1330,@FinishedGoodsRD),
(4,1,1330,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,1330,@TradeMedicineRD),
(6,1,1330,@TradeConstructionMaterialRD),
(7,1,1330,@TradeSparePartRD),
(8,1,1330,@FinishedGrainRD),
(9,1,1330,@ByproductGrainRD),
(10,1,1330,@FinishedVehicleRD),
(11,1,1330,@FinishedOilRD),
(12,1,1330,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1330,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1330,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1330,	N'NotedRelationId',		3,	N'Customer',		3,4,1),
(2,1330,	N'CustodyId',			1,	N'Warehouse',		3,4,1),
(3,1330,	N'ResourceId',			1,	N'Item',			2,4,0),
(4,1330,	N'Quantity',			1,	N'Qty',				2,4,0),
(5,1330,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,1330,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,1330,	N'MonetaryValue',		3,	N'Cost (VAT Excl.)',1,2,0),
(10,1330,	N'PostingDate',			1,	N'Issued On',		1,4,1),
(11,1330,	N'CenterId',			2,	N'Business Unit',	1,4,1);
--1360:RevenueFromInventoryWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[ParentAccountTypeId] = ATC.[Id]
		JOIN dbo.AccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node])  = 1
		WHERE ATP.[Concept] = N''Inventories''
	),
	ResourceCosts AS (
		SELECT
		PWL.PostingDate, PWL.[CustodyId1],  PWL.[ResourceId1],
			SUM(E.[AlgebraicValue]) AS NetValue,
			SUM(E.[AlgebraicQuantity]) AS NetQuantity
		FROM map.[DetailsEntries]() E
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		JOIN @ProcessedWideLines PWL ON PWL.[ResourceId1] = E.[ResourceId] AND PWL.[CustodyId1] = E.[CustodyId] AND L.PostingDate <= PWL.[PostingDate]
		WHERE E.[AccountId] IN (SELECT [Id] FROM InventoryAccounts)
		AND L.[State] = 4
		GROUP BY PWL.PostingDate, PWL.[CustodyId1],  PWL.[ResourceId1]
	)	
	UPDATE PWL
	SET
		[CustodyId0]		= PWL.[CustodyId1],
		[CustodyId4]		= PWL.[CustodyId1],
		[CurrencyId0]		= PWL.[CurrencyId2],
		[CurrencyId1]		= PWL.[CurrencyId2],
		[CurrencyId3]		= PWL.[CurrencyId2],
		[CurrencyId4]		= PWL.[CurrencyId2],
		[CenterId1]			= COALESCE(PWL.[CenterId1], PWL.[CenterId2]),
		[CenterId0]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[CenterId3]			= PWL.[CenterId2],
		[CenterId4]			= (
								SELECT [Id]
								FROM dbo.Centers
								WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.Centers WHERE [Id] = PWL.[CenterId2])) = 1
								AND CenterType IN (N''BusinessUnit'', N''CostOfSales'') AND [IsLeaf] = 1
								),
		[ResourceId0]		= PWL.[ResourceId1], [Quantity0] = PWL.[Quantity1], [UnitId0] = PWL.[UnitId1],
		[ResourceId4]		= PWL.[ResourceId1], [Quantity4] = PWL.[Quantity1],	[UnitId4] = PWL.[UnitId1],
		[MonetaryValue0]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue1]	= IIF (
								ISNULL(RC.[NetQuantity],0) = 0,
								0,
								RC.NetValue / RC.NetQuantity * PWL.[Quantity1] * EU.[BaseAmount] / EU.[UnitAmount] * RBU.[UnitAmount] / RBU.[BaseAmount]
								),
		[MonetaryValue2]	= ISNULL([MonetaryValue3],0) + ISNULL([MonetaryValue4],0),
		[NotedAmount3]		= ISNULL([MonetaryValue3],0) + ISNULL([MonetaryValue4],0),
		[NotedRelationId0]	= [NotedRelationId4],
		[NotedAgentName1]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId4]),
		[NotedRelationId2]	= [NotedRelationId4]
	FROM @ProcessedWideLines PWL
	LEFT JOIN ResourceCosts RC ON PWL.[ResourceId1] = RC.[ResourceId1] AND PWL.[CustodyId1] = RC.[CustodyId1] AND PWL.[PostingDate] = RC.[PostingDate]
	LEFT JOIN dbo.[Resources] R ON PWL.[ResourceId1] = R.[Id]
	LEFT JOIN dbo.Units EU ON PWL.[UnitId1] = EU.[Id]
	LEFT JOIN dbo.Units RBU ON R.[UnitId] = RBU.[Id]
'
WHERE [Index] = 1360;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],									[EntryTypeId]) VALUES
(0,1360,+1,	@CostOfMerchandiseSold,								NULL),
(1,1360,-1,	@Inventories,										@ReturnsIssuesThroughSaleExtension),
(2,1360,+1,	@CashReceiptsFromCustomersControlExtension,			NULL),
(3,1360,-1,	@CurrentValueAddedTaxPayables,						NULL),
(4,1360,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,1360,@MerchandiseRD),
(1,1,1360,@CurrentFoodAndBeverageRD),
(2,1,1360,@CurrentAgriculturalProduceRD),
(3,1,1360,@FinishedGoodsRD),
(4,1,1360,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,1360,@TradeMedicineRD),
(6,1,1360,@TradeConstructionMaterialRD),
(7,1,1360,@TradeSparePartRD),
(8,1,1360,@FinishedGrainRD),
(9,1,1360,@ByproductGrainRD),
(10,1,1360,@FinishedVehicleRD),
(11,1,1360,@FinishedOilRD),
(12,1,1360,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,1360,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1360,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1360,	N'NotedRelationId',		4,	N'Customer',		3,4,1),
(2,1360,	N'CustodyId',			1,	N'Warehouse',		3,4,1),
(3,1360,	N'ResourceId',			1,	N'Item',			2,4,0),
(4,1360,	N'Quantity',			1,	N'Qty',				2,4,0),
(5,1360,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,1360,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,1360,	N'MonetaryValue',		4,	N'Cost (VAT Excl.)',1,2,0),
(8,1360,	N'MonetaryValue',		3,	N'VAT',				1,2,0),
(9,1360,	N'MonetaryValue',		2,	N'Line Total',		1,2,0),
(10,1360,	N'ExternalReference',	2,	N'Invoice #',		1,4,0),
(11,1360,	N'PostingDate',			1,	N'Issued On',		1,4,1),
(12,1360,	N'CenterId',			2,	N'Business Unit',	1,4,1);
--1410:CashFromCustomer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue1], 0),
		[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1410;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1410,+1,		@CashAndCashEquivalents),
(1,1410,-1,		@CashReceiptsFromCustomersControlExtension); 
--1420:CashFromCustomerWithWT
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CurrencyId0]		= [CurrencyId2],
		[CenterId1]			= [CenterId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * ISNULL([NotedAmount1],0), 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue2], 0) - 
								IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * ISNULL([NotedAmount1],0), 0),
		[NotedRelationId1]	= [NotedRelationId2],
		--[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId2]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1420;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId],							[EntryTypeId]) VALUES
(0,1420,+1,		@CashAndCashEquivalents,					@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1420,+1,		@WithholdingTaxReceivablesExtension,		NULL),
(2,1420,-1,		@CashReceiptsFromCustomersControlExtension,	NULL); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1420,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1420,	N'NotedRelationId',		1,	N'Customer',		1,4,1),
(2,1420,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,1420,	N'MonetaryValue',		1,	N'Amount (VAT incl.)',1,2,0), 
(8,1420,	N'MonetaryValue',		0,	N'Net To Receive',	1,1,0),
(9,1420,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1420,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1420,	N'PostingDate',			1,	N'Payment Date',	1,2,1),
(12,1420,	N'CenterId',			1,	N'Business Unit',	1,4,1),
(13,1420,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1430:CashFromCustomerWithWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId2],
		[CurrencyId0]		= [CurrencyId2],
		[CenterId1]			= [CenterId2],
		[CenterId0]			= COALESCE([CenterId0], [CenterId2]),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= 0.15 * [MonetaryValue2],
		[MonetaryValue0]	= ISNULL([MonetaryValue2], 0) + ISNULL([MonetaryValue1], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue2], 0),
		[NotedRelationId1]	= [NotedRelationId2],
	--	[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId2]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1430;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId],								[EntryTypeId]) VALUES
(0,1430,+1,		@CashAndCashEquivalents,						@ReceiptsFromSalesOfGoodsAndRenderingOfServices),
(1,1430,-1,		@CurrentValueAddedTaxPayables,					NULL),
(2,1430,-1,		@IssuesAtPointInTimeToCustomersControlExtension,NULL); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1430,	N'Memo',				0,	N'Memo',			1,4,1),
(1,1430,	N'NotedRelationId',		2,	N'Customer',		1,4,1),
(2,1430,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(3,1430,	N'MonetaryValue',		2,	N'Amount (VAT Excl)',1,2,0), -- 
(4,1430,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,1430,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(8,1430,	N'MonetaryValue',		0,	N'Net To Receive',	1,1,0),
(9,1430,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1430,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1430,	N'PostingDate',			2,	N'Payment Date',	1,2,1),
(12,1430,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(13,1430,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1450:CashFromCustomerWithWTWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId2]		= [CurrencyId3],
		[CurrencyId1]		= [CurrencyId3],
		[CurrencyId0]		= [CurrencyId3],
		[CenterId2]			= [CenterId3],
		[CenterId1]			= [CenterId3],
		[CenterId0]			= COALESCE([CenterId0], [CenterId3]),
		[MonetaryValue3]	= ISNULL([MonetaryValue3], 0),
		[MonetaryValue2]	= 0.15 * [MonetaryValue3], --ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * [MonetaryValue3], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue3], 0) + ISNULL([MonetaryValue2], 0) - 
								IIF(ISNUMERIC([ExternalReference1]) = 1 AND [ExternalReference1] <> N''-'', 0.02 * [MonetaryValue3], 0),
		--[ExternalReference1]= ISNULL([ExternalReference1], N''--''),
		[NotedAmount2]		= ISNULL([MonetaryValue3], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue3], 0),
		[NotedRelationId2]	= [NotedRelationId3],
		[NotedRelationId1]	= [NotedRelationId3],
		-- Entry Type may change depending on nature of items
		[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId3]),
		[AdditionalReference0] = IIF(ISNUMERIC([AdditionalReference0]) = 1, N''CRV'' + [AdditionalReference0], [AdditionalReference0])
'
WHERE [Index] = 1450;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1450,+1,		@CashAndCashEquivalents),
(1,1450,+1,		@WithholdingTaxReceivablesExtension),
(2,1450,-1,		@CurrentValueAddedTaxPayables),
(3,1450,-1,		@IssuesAtPointInTimeToCustomersControlExtension); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1450,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1450,	N'NotedRelationId',		3,	N'Customer',		1,4,1),
(2,1450,	N'CurrencyId',			3,	N'Currency',		1,2,1),
(3,1450,	N'MonetaryValue',		3,	N'Amount (VAT Excl)',1,2,0), -- 
(4,1450,	N'MonetaryValue',		2,	N'VAT',				1,4,0),
(5,1450,	N'ExternalReference',	2,	N'Invoice #',		1,4,0),
(6,1450,	N'MonetaryValue',		1,	N'Amount Withheld',	4,4,0),
(7,1450,	N'ExternalReference',	1,	N'WT Voucher #',	5,5,0),
(8,1450,	N'MonetaryValue',		0,	N'Net To Receive',	1,1,0),
(9,1450,	N'ExternalReference',	0,	N'Check #',			5,5,0),
(10,1450,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(11,1450,	N'PostingDate',			3,	N'Payment Date',	1,2,1),
(12,1450,	N'CenterId',			3,	N'Business Unit',	1,4,1),
(13,1450,	N'AdditionalReference',	0,	N'CRV #',			5,5,0);
--1550:CashTransferExchange
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[CenterId0] = COALESCE([CenterId0], [CenterId2]),
		[CenterId1] = COALESCE([CenterId1], [CenterId2]),
		[CurrencyId2] = dbo.fn_FunctionalCurrencyId(),
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0]),
		[MonetaryValue2] = wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId1], [MonetaryValue1])
							- wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId0], [MonetaryValue0]) 
'
WHERE [Index] = 1550;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [ParentAccountTypeId],[EntryTypeId]) VALUES
(0,1550,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1550,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(2,1550,+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax, NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1550,	N'CustodyId',			1,	N'From Account',	1,2,0),
(1,1550,	N'CustodyId',			0,	N'To Account',		1,2,0),
(2,1550,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,1550,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,1550,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,1550,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,1550,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(7,1550,	N'Memo',				0,	N'Memo',			1,2,1);
--1560:CashTransfer
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[MonetaryValue0] = ISNULL([MonetaryValue1],0)
'
WHERE [Index] = 1560;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [ParentAccountTypeId],[EntryTypeId]) VALUES
(0,1560,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1560,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1560,	N'CustodyId',			1,	N'From Account',	1,2,0),
(1,1560,	N'CustodyId',			0,	N'To Account',		1,2,0),
(4,1560,	N'MonetaryValue',		1,	N'Amount',			1,3,0),
(7,1560,	N'Memo',				0,	N'Memo',			1,2,1);
--1570:CashExchange
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[NotedAgentName0] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]),
		[NotedAgentName1] = (SELECT [Name] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]),
		[CustodyId0] = [CustodyId1],
		[CenterId0] = COALESCE((SELECT [CenterId] FROM dbo.[Custodies] WHERE [Id] = [CustodyId0]), [CenterId2]),
		[CenterId1] = COALESCE((SELECT [CenterId] FROM dbo.[Custodies] WHERE [Id] = [CustodyId1]), [CenterId2]),
		[CurrencyId2] = dbo.fn_FunctionalCurrencyId(),
		[MonetaryValue0] = IIF([CurrencyId0]=[CurrencyId1],[MonetaryValue1],[MonetaryValue0]),
		[MonetaryValue2] = wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId1], [MonetaryValue1])
							- wiz.fn_ConvertToFunctional([PostingDate], [CurrencyId0], [MonetaryValue0]) 
'
WHERE [Index] = 1570;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction], [ParentAccountTypeId],[EntryTypeId]) VALUES
(0,1570,+1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(1,1570,-1,	@CashAndCashEquivalents, @InternalCashTransferExtension),
(2,1570,+1,	@GainsLossesOnExchangeDifferencesOnTranslationBeforeTax, NULL); -- Make it an automatic system entry
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1570,	N'CustodyId',			1,	N'Account',			1,2,0),
(2,1570,	N'CurrencyId',			1,	N'From Currency',	1,2,0),
(3,1570,	N'CurrencyId',			0,	N'To Currency',		1,2,0),
(4,1570,	N'MonetaryValue',		1,	N'From Amount',		1,3,0),
(5,1570,	N'MonetaryValue',		0,	N'To Amount',		1,3,0),
(6,1570,	N'CenterId',			2,	N'Business Unit',	1,4,1),
(7,1570,	N'Memo',				0,	N'Memo',			1,2,1);
--1660:CashToSupplierWithPointInvoice
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId1]		= [CurrencyId0],
		[CurrencyId2]		= [CurrencyId0],
		[CenterId1]			= [CenterId0],
		[CenterId2]			= COALESCE([CenterId2], [CenterId0]),
		[MonetaryValue0]	= ISNULL([MonetaryValue0], 0),
		[MonetaryValue1]	= ISNULL([MonetaryValue1], 0),
		[MonetaryValue2]	= ISNULL([MonetaryValue0], 0) + ISNULL([MonetaryValue1], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue0], 0),
		[NotedRelationId1]	= [NotedRelationId0],
		-- Entry Type may change depending on nature of items
		[EntryTypeId2]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''PaymentsToSuppliersForGoodsAndServices''),
		[NotedAgentName2]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0]),
		[AdditionalReference2] = IIF(ISNUMERIC([AdditionalReference2]) = 1, N''CPV'' + [AdditionalReference2], [AdditionalReference2])
'
WHERE [Index] = 1660;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1660,+1,		@ReceiptsAtPointInTimeFromSuppliersControlExtension),
(1,1660,+1,		@CurrentValueAddedTaxReceivables),
(2,1660,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1660,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1660,	N'NotedRelationId',		0,	N'Supplier',		1,4,1),
(2,1660,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,1660,	N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0),
(4,1660,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,1660,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(6,1660,	N'MonetaryValue',		2,	N'Net To Pay',		1,1,0),
(8,1660,	N'ExternalReference',	2,	N'Check #',			5,5,0),
(9,1660,	N'CustodyId',			2,	N'Cash/Bank Acct',	4,4,0),
(10,1660,N'PostingDate',			0,	N'Payment Date',	1,2,1),
(11,1660, N'CenterId',			0,	N'Business Unit',	1,4,1),
(12,1660, N'AdditionalReference',2,	N'CPV #',			1,4,0);
--1680:CashToSupplierWithPointInvoiceWithWT CashPaymentToTradePayableWithWT: (basically, it is the VAT) -- assume all in same currency
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
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
		[MonetaryValue2]	= IIF(ISNUMERIC([ExternalReference2]) = 1 AND [ExternalReference2] <> N''-'', 0.02 * [MonetaryValue0], 0),
		[MonetaryValue3]	= ISNULL([MonetaryValue0], 0) + ISNULL([MonetaryValue1], 0) - 
								IIF(ISNUMERIC([ExternalReference2]) = 1 AND [ExternalReference2] <> N''-'', 0.02 * [MonetaryValue0], 0),
		--[ExternalReference2]= ISNULL([ExternalReference2], N''--''),
		[NotedAmount1]		= ISNULL([MonetaryValue0], 0),
		[NotedAmount2]		= ISNULL([MonetaryValue0], 0),
		[NotedRelationId1]	= [NotedRelationId0],
		[NotedRelationId2]	= [NotedRelationId0],
		-- Entry Type may change depending on nature of items
		[EntryTypeId3]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''PaymentsToSuppliersForGoodsAndServices''),
		[NotedAgentName3]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId0]),
		[AdditionalReference3] = IIF(ISNUMERIC([AdditionalReference3]) = 1, N''CPV'' + [AdditionalReference3], [AdditionalReference3])
'
WHERE [Index] = 1680;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[ParentAccountTypeId]) VALUES
(0,1680,+1,		@ReceiptsAtPointInTimeFromSuppliersControlExtension), -- Item price
(1,1680,+1,		@CurrentValueAddedTaxReceivables), -- VAT, Taxamble Amount
(2,1680,-1,		@WithholdingTaxPayableExtension), -- Amount paid, Equivalent Actual amount to be paid. Noted Currency Id
(3,1680,-1,		@CashAndCashEquivalents); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1680,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1680,	N'NotedRelationId',		0,	N'Supplier',		1,4,1),
(2,1680,	N'CurrencyId',			0,	N'Currency',		1,2,1),
(3,1680,	N'MonetaryValue',		0,	N'Amount (VAT Excl)',1,2,0),
(4,1680,	N'MonetaryValue',		1,	N'VAT',				1,4,0),
(5,1680,	N'ExternalReference',	1,	N'Invoice #',		1,4,0),
(6,1680,	N'MonetaryValue',		2,	N'Amount Withheld',	4,4,0),
(7,1680,	N'ExternalReference',	2,	N'WT Voucher #',	5,5,0),
(8,1680,	N'MonetaryValue',		3,	N'Net To Pay',		1,1,0),
(9,1680,	N'ExternalReference',	3,	N'Check #',			5,5,0),
(10,1680,N'CustodyId',			3,	N'Cash/Bank Acct',	4,4,0),
(11,1680,N'PostingDate',			0,	N'Payment Date',	1,2,1),
(12,1680, N'CenterId',			0,	N'Business Unit',	1,4,1),
(13,1680, N'AdditionalReference',3,	N'CPV #',			1,4,0);
--1730:SupplierWT
UPDATE @LineDefinitions
SET [PreprocessScript] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId0]		= [CurrencyId1],
		[CenterId0]			= COALESCE([CenterId0], [CenterId1]),
		[MonetaryValue0]	= [MonetaryValue1],
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId1])
'
WHERE [Index] = 1730;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[ParentAccountTypeId],										[EntryTypeId]) VALUES
(0,1730,+1,	@CashPaymentsToSuppliersControlExtension,NULL),
(1,1730,-1,	@WithholdingTaxPayableExtension,NULL);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,0,1730,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,1730,	N'Memo',				1,	N'Memo',			1,4,1),
(1,1730,	N'NotedRelationId',		1,	N'Supplier',		3,4,1),
(2,1730,	N'CurrencyId',			1,	N'Currency',		1,2,1),
(3,1730,	N'NotedAmount',			1,	N'Amount (VAT Excl.)',3,3,0),
(4,1730,	N'MonetaryValue',		1,	N'Amount Withheld',	1,2,0),
(9,1730,N'ExternalReference',	1,	N'Voucher #',		1,4,1),
(10,1730,N'PostingDate',			1,	N'Voucher Date',	1,4,1),
(11,1730,N'CenterId',			1,	N'Business Unit',	1,4,1);

EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryCustodyDefinitions = @LineDefinitionEntryCustodyDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;