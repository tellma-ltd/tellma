CREATE PROCEDURE [dbo].[LineDefinitions__Insert]
AS
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
DECLARE @GoodsAndServicesReceivedFromSuppliersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GoodsAndServicesReceivedFromSuppliersControlExtensions');
DECLARE @CustomersControlAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CustomersControlAccountsExtension');
DECLARE @CashReceiptsFromCustomersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashReceiptsFromCustomersControlExtension');
DECLARE @GoodsAndServicesIssuedToCustomersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GoodsAndServicesIssuedToCustomersControlExtension');
DECLARE @PayrollControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PayrollControlExtension');
DECLARE @CashPaymentsToEmployeesControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashPaymentsToEmployeesControlExtension');
DECLARE @OthersAccountsControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OthersAccountsControlExtension');
DECLARE @CashPaymentsToOthersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashPaymentsToOthersControlExtension');
DECLARE @CashReceiptsFromOthersControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashReceiptsFromOthersControlExtension');
DECLARE @GuaranteesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GuaranteesExtension');
DECLARE @CollectionGuaranteeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CollectionGuaranteeExtension');
DECLARE @DishonouredGuaranteeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DishonouredGuaranteeExtension');
DECLARE @MigrationAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'MigrationAccountsExtension');	

DECLARE @CreditorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Creditor');
DECLARE @DebtorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Debtor');
DECLARE @OwnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Owner');
DECLARE @PartnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Partner');
DECLARE @SupplierRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Supplier');
DECLARE @CustomerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Customer');
DECLARE @EmployeeRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Employee');
DECLARE @BankBranchRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BankBranch');
DECLARE @BankAccountCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'BankAccount');
DECLARE @SafeCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Safe');
DECLARE @WarehouseCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Warehouse');
DECLARE @PPECustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'PPECustody');
DECLARE @RentalCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Rental');
DECLARE @TransitCustodyCD INT = (SELECT [Id] FROM dbo.[CustodyDefinitions] WHERE [Code] = N'Shipper');

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
DECLARE @LineDefinitionEntryNotedRelationDefinitions LineDefinitionEntryNotedRelationDefinitionList;
DECLARE @LineDefinitionStateReasons dbo.[LineDefinitionStateReasonList];
DECLARE @Workflows dbo.[WorkflowList];
DECLARE @WorkflowSignatures dbo.WorkflowSignatureList;
DECLARE @ValidationErrorsJson nvarchar(max);

INSERT INTO @LineDefinitions([Index], [Code], [Description], [TitleSingular], [TitlePlural], [AllowSelectiveSigning], [ViewDefaultsToForm]) VALUES
(402, N'CashReceiptFromTradeReceivableWithWT', N'Receiving cash payment from customer/lessee, with WT', N'Cash', N'Cash', 0, 1),
(403, N'StockIssueToTradeReceivable', N'Issuing stock to customer', N'Stock', N'Stock', 0, 0);
--402:CashReceiptFromTradeReceivableWithWT: -- assume all in same currency
UPDATE @LineDefinitions
SET [Script] = N'
	UPDATE @ProcessedWideLines
	SET
		[CurrencyId2]		= [CurrencyId3],
		[CurrencyId1]		= [CurrencyId3],
		[CurrencyId0]		= [CurrencyId3],
		[CenterId2]			= [CenterId3],
		[CenterId1]			= [CenterId3],
		[CenterId0]			= COALESCE([CenterId0], [CenterId3]),
		[MonetaryValue3]	= ISNULL([MonetaryValue3], 0),
		[MonetaryValue2]	= ISNULL([MonetaryValue2], 0),
		[MonetaryValue1]	= IIF(ISNUMERIC([ExternalReference1]) = 1, 0.02 * [MonetaryValue3], 0),
		[MonetaryValue0]	= ISNULL([MonetaryValue3], 0) + ISNULL([MonetaryValue2], 0) - 
								IIF(ISNUMERIC([ExternalReference1]) = 1, 0.02 * [MonetaryValue3], 0),
		[ExternalReference1]= ISNULL([ExternalReference1], N''--''),
		[NotedAmount2]		= ISNULL([MonetaryValue3], 0),
		[NotedAmount1]		= ISNULL([MonetaryValue3], 0),
		[NotedRelationId2]	= [NotedRelationId3],
		[NotedRelationId1]	= [NotedRelationId3],
		-- Entry Type may change depending on nature of items
		[EntryTypeId0]		= (SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = N''ReceiptsFromSalesOfGoodsAndRenderingOfServices''),
		[NotedAgentName0]	= (SELECT [Name] FROM dbo.[Relations] WHERE [Id] = [NotedRelationId3])
'
WHERE [Index] = 402;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],	[AccountTypeId]) VALUES
(0,402,+1,		@CashAndCashEquivalents),
(1,402,+1,		@WithholdingTaxReceivablesExtension),
(2,402,-1,		@CurrentValueAddedTaxPayables),
(3,402,-1,		@GoodsAndServicesIssuedToCustomersControlExtension); 
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,402,	N'Memo',				1,	N'Memo',			1,4,1),
(1,402,	N'NotedRelationId',		3,	N'Customer',		1,4,1),
(2,402,	N'CurrencyId',			3,	N'Currency',		1,2,1),
(3,402,	N'MonetaryValue',		3,	N'Amount (VAT Excl)',1,2,0), -- 
(4,402,	N'MonetaryValue',		2,	N'VAT',				1,4,0),
(5,402,	N'ExternalReference',	2,	N'Invoice #',		1,4,0),
(6,402,	N'MonetaryValue',		1,	N'Amount Withheld',	4,4,0),
(7,402,	N'ExternalReference',	1,	N'WT Voucher #',	4,4,0),
(8,402,	N'ExternalReference',	0,	N'Check #',			4,4,0),
(9,402,	N'CustodyId',			0,	N'Cash/Bank Acct',	4,4,0),
(10,402,N'PostingDate',			3,	N'Payment Date',	1,2,1),
(11,402, N'CenterId',			3,	N'Business Unit',	1,4,1);
--403:StockIssueToTradeReceivable: (This is the Cash sale version, we still need credit sale versions)
UPDATE @LineDefinitions
SET [Script] = N'
	WITH InventoryAccounts AS (
		SELECT A.[Id]
		FROM dbo.Accounts A
		JOIN dbo.AccountTypes ATC ON A.[AccountTypeId] = ATC.[Id]
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
WHERE [Index] = 403;
INSERT INTO @LineDefinitionEntries([Index], [HeaderIndex],
[Direction],[AccountTypeId],									[EntryTypeId]) VALUES
(0,403,+1,	@CostOfMerchandiseSold,								NULL),
(1,403,-1,	@Inventories,										@ReceiptsReturnsThroughPurchaseExtension),
(2,403,+1,	@GoodsAndServicesIssuedToCustomersControlExtension,	NULL),
(3,403,-1,	@Revenue,											NULL)
INSERT INTO @LineDefinitionEntryResourceDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[ResourceDefinitionId]) VALUES
(0,1,403,@MerchandiseRD),
(1,1,403,@CurrentFoodAndBeverageRD),
(2,1,403,@CurrentAgriculturalProduceRD),
(3,1,403,@FinishedGoodsRD),
(4,1,403,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(5,1,403,@TradeMedicineRD),
(6,1,403,@TradeConstructionMaterialRD),
(7,1,403,@TradeSparePartRD),
(8,1,403,@FinishedGrainRD),
(9,1,403,@ByproductGrainRD),
(10,1,403,@FinishedVehicleRD),
(11,1,403,@FinishedOilRD),
(12,1,403,@ByproductOilRD);
INSERT INTO @LineDefinitionEntryCustodyDefinitions([Index], [LineDefinitionEntryIndex], [LineDefinitionIndex],
[CustodyDefinitionId]) VALUES
(0,1,403,@WarehouseCD);
INSERT INTO @LineDefinitionColumns([Index], [HeaderIndex],
		[ColumnName],[EntryIndex],	[Label],			[RequiredState],
														[ReadOnlyState],
														[InheritsFromHeader]) VALUES
(0,403,	N'Memo',				1,	N'Memo',			1,4,1),
(1,403,	N'NotedRelationId',		3,	N'Customer',		3,4,1),
(2,403,	N'CustodyId',			1,	N'Warehouse',		3,4,1),
(3,403,	N'ResourceId',			1,	N'Item',			2,4,0),
(4,403,	N'Quantity',			1,	N'Qty',				2,4,0),
(5,403,	N'UnitId',				1,	N'Unit',			2,4,0),
(6,403,	N'CurrencyId',			2,	N'Currency',		1,2,1),
(7,403,	N'MonetaryValue',		3,	N'Cost (VAT Excl.)',1,2,0),
(10,403,N'PostingDate',			1,	N'Issued On',		1,4,1),
(11,403,N'CenterId',			2,	N'Business Unit',	1,4,1);

EXEC sys.sp_set_session_context 'UserId', 1;
EXEC [api].[LineDefinitions__Save]
	@Entities = @LineDefinitions,
	@LineDefinitionEntries = @LineDefinitionEntries,
	@LineDefinitionEntryCustodyDefinitions = @LineDefinitionEntryCustodyDefinitions,
	@LineDefinitionEntryResourceDefinitions = @LineDefinitionEntryResourceDefinitions,
	@LineDefinitionEntryNotedRelationDefinitions = @LineDefinitionEntryNotedRelationDefinitions,
	@LineDefinitionColumns = @LineDefinitionColumns,
	@LineDefinitionGenerateParameters = @LineDefinitionGenerateParameters,
	@LineDefinitionStateReasons = @LineDefinitionStateReasons,
	@Workflows = @Workflows,
	@WorkflowSignatures = @WorkflowSignatures,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
/*
	select * from LineDefinitions 
	select * from documentdefinitions
	select * from documentdefinitionLinedefinitions

	insert into documentdefinitionLinedefinitions(DocumentdefinitionId, LineDefinitionId, [Index], IsVisibleByDefault, SavedById)
	Values
	(3, 34, 0, 1, 1),
	(3, 35, 0, 1, 1),
	(3, 1, 0, 1, 1);
	update documentdefinitions set state = N'Visible' where Id in (1, 3)
	Update Settings set DefinitionsVersion = newId()
*/