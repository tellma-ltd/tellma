﻿DECLARE @EntryTypesTemp TABLE ([IsAssignable] BIT, [Index] INT, [ForDebit] BIT, [ForCredit] BIT, [Node] HIERARCHYID, [Code] NVARCHAR(255), [Name] NVARCHAR(255))

INSERT INTO @EntryTypesTemp([IsAssignable], [Index], [ForDebit], [ForCredit], [Node], [Code], [Name]) VALUES
 (0, 0, 1, 1, '/1/', 'ChangesInPropertyPlantAndEquipment', 'Increase (decrease) in property, plant and equipment')
,(1, 1, 1, 0, '/1/1/', 'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment', 'Additions other than through business combinations, property, plant and equipment')
,(1, 2, 1, 0, '/1/2/', 'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment', 'Acquisitions through business combinations, property, plant and equipment')
,(1, 3, 1, 1, '/1/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesPropertyPlantAndEquipment', 'Increase (decrease) through net exchange differences, property, plant and equipment')
,(1, 4, 0, 1, '/1/4/', 'DepreciationPropertyPlantAndEquipment', 'Depreciation, property, plant and equipment')
,(1, 5, 0, 1, '/1/5/', 'ImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment', 'Impairment loss recognised in profit or loss, property, plant and equipment')
,(1, 6, 1, 0, '/1/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment', 'Reversal of impairment loss recognised in profit or loss, property, plant and equipment')
,(1, 7, 1, 1, '/1/7/', 'RevaluationIncreaseDecreasePropertyPlantAndEquipment', 'Revaluation increase (decrease), property, plant and equipment')
,(1, 8, 0, 1, '/1/8/', 'ImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment', 'Impairment loss recognised in other comprehensive income, property, plant and equipment')
,(1, 9, 1, 0, '/1/9/', 'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment', 'Reversal of impairment loss recognised in other comprehensive income, property, plant and equipment')
,(0, 10, 1, 1, '/1/10/', 'IncreaseDecreaseThroughTransfersAndOtherChangesPropertyPlantAndEquipment', 'Increase (decrease) through transfers and other changes, property, plan')
,(1, 11, 1, 1, '/1/10/1/', 'IncreaseDecreaseThroughTransfersPropertyPlantAndEquipment', 'Increase (decrease) through transfers, property, plant and equipment')
,(1, 12, 1, 1, '/1/10/2/', 'IncreaseDecreaseThroughOtherChangesPropertyPlantAndEquipment', 'Increase (decrease) through other changes, property, plant and equipment')
,(0, 13, 0, 1, '/1/11/', 'DisposalsAndRetirementsPropertyPlantAndEquipment', 'Disposals and retirements, property, plant and equipment')
,(1, 14, 0, 1, '/1/11/1/', 'DisposalsPropertyPlantAndEquipment', 'Disposals, property, plant and equipment')
,(1, 15, 0, 1, '/1/11/2/', 'RetirementsPropertyPlantAndEquipment', 'Retirements, property, plant and equipment')
,(1, 16, 0, 1, '/1/12/', 'DecreaseThroughClassifiedAsHeldForSalePropertyPlantAndEquipment', 'Decrease through classified as held for sale, property, plant and equipment')
,(1, 17, 0, 1, '/1/13/', 'DecreaseThroughLossOfControlOfSubsidiaryPropertyPlantAndEquipment', 'Decrease through loss of control of subsidiary, property, plant and equipment')
,(0, 18, 1, 1, '/2/', 'ChangesInInvestmentProperty', 'Increase (decrease) in investment property')
,(0, 19, 1, 0, '/2/1/', 'AdditionsOtherThanThroughBusinessCombinationsInvestmentProperty', 'Additions other than through business combinations, investment property')
,(1, 20, 1, 0, '/2/1/1/', 'AdditionsFromSubsequentExpenditureRecognisedAsAssetInvestmentProperty', 'Additions from subsequent expenditure recognised as asset, investment property')
,(1, 21, 1, 0, '/2/1/2/', 'AdditionsFromAcquisitionsInvestmentProperty', 'Additions from acquisitions, investment property')
,(1, 22, 1, 0, '/2/2/', 'AcquisitionsThroughBusinessCombinationsInvestmentProperty', 'Acquisitions through business combinations, investment property')
,(1, 23, 1, 1, '/2/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesInvestmentProperty', 'Increase (decrease) through net exchange differences, investment property')
,(1, 24, 0, 1, '/2/4/', 'DepreciationInvestmentProperty', 'Depreciation, investment property')
,(1, 25, 0, 1, '/2/5/', 'ImpairmentLossRecognisedInProfitOrLossInvestmentProperty', 'Impairment loss recognised in profit or loss, investment property')
,(1, 26, 0, 1, '/2/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossInvestmentProperty', 'Reversal of impairment loss recognised in profit or loss, investment property')
,(1, 27, 1, 1, '/2/7/', 'GainsLossesOnFairValueAdjustmentInvestmentProperty', 'Gains (losses) on fair value adjustment, investment property')
,(1, 28, 1, 0, '/2/8/', 'TransferFromToInventoriesAndOwnerOccupiedPropertyInvestmentProperty', 'Transfer from (to) inventories and owner-occupied property, investment property')
,(1, 29, 1, 0, '/2/9/', 'TransferFromInvestmentPropertyUnderConstructionOrDevelopmentInvestmentProperty', 'Transfer from investment property under construction or development, investment property')
,(1, 30, 0, 1, '/2/10/', 'DisposalsInvestmentProperty', 'Disposals, investment property')
,(1, 31, 0, 1, '/2/11/', 'DecreaseThroughClassifiedAsHeldForSaleInvestmentProperty', 'Decrease through classified as held for sale, investment property')
,(1, 32, 1, 1, '/2/12/', 'IncreaseDecreaseThroughOtherChangesInvestmentProperty', 'Increase (decrease) through other changes, investment property')
,(0, 33, 1, 1, '/3/', 'ChangesInGoodwill', 'Increase (decrease) in goodwill')
,(1, 34, 1, 0, '/3/1/', 'AdditionalRecognitionGoodwill', 'Additional recognition, goodwill')
,(1, 35, 1, 0, '/3/2/', 'SubsequentRecognitionOfDeferredTaxAssetsGoodwill', 'Subsequent recognition of deferred tax assets, goodwill')
,(1, 36, 0, 1, '/3/3/', 'DecreaseThroughClassifiedAsHeldForSaleGoodwill', 'Decrease through classified as held for sale, goodwill')
,(1, 37, 1, 1, '/3/4/', 'GoodwillDerecognisedWithoutHavingPreviouslyBeenIncludedInDisposalGroupClassifiedAsHeldForSale', 'Goodwill derecognised without having previously been included in disposal group classified as held for sale')
,(1, 38, 1, 0, '/3/5/', 'ImpairmentLossRecognisedInProfitOrLossGoodwill', 'Impairment loss recognised in profit or loss, goodwill')
,(1, 39, 1, 1, '/3/6/', 'IncreaseDecreaseThroughNetExchangeDifferencesGoodwill', 'Increase (decrease) through net exchange differences, goodwill')
,(1, 40, 1, 1, '/3/7/', 'IncreaseDecreaseThroughTransfersAndOtherChangesGoodwill', 'Increase (decrease) through other changes, goodwill')
,(0, 41, 1, 1, '/4/', 'ChangesInIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) in intangible assets other than goodwill')
,(1, 42, 1, 0, '/4/1/', 'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', 'Additions other than through business combinations, intangible assets other than goodwill')
,(1, 43, 1, 0, '/4/2/', 'AcquisitionsThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', 'Acquisitions through business combinations, intangible assets other than goodwill')
,(1, 44, 1, 1, '/4/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through net exchange differences, intangible assets other than goodwill')
,(1, 45, 0, 1, '/4/4/', 'AmortisationIntangibleAssetsOtherThanGoodwill', 'Amortisation, intangible assets other than goodwill')
,(1, 46, 0, 1, '/4/5/', 'ImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', 'Impairment loss recognised in profit or loss, intangible assets other than goodwill')
,(1, 47, 1, 0, '/4/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', 'Reversal of impairment loss recognised in profit or loss, intangible assets other than goodwill')
,(1, 48, 1, 1, '/4/7/', 'RevaluationIncreaseDecreaseIntangibleAssetsOtherThanGoodwill', 'Revaluation increase (decrease), intangible assets other than goodwill')
,(1, 49, 0, 1, '/4/8/', 'ImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', 'Impairment loss recognised in other comprehensive income, intangible assets other than goodwill')
,(1, 50, 1, 0, '/4/9/', 'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', 'Reversal of impairment loss recognised in other comprehensive income, intangible assets other than goodwill')
,(1, 51, 1, 1, '/4/10/', 'DecreaseThroughClassifiedAsHeldForSaleIntangibleAssetsOtherThanGoodwill', 'Decrease through classified as held for sale, intangible assets other than goodwill')
,(1, 52, 1, 1, '/4/11/', 'DecreaseThroughLossOfControlOfSubsidiaryIntangibleAssetsOtherThanGoodwill', 'Decrease through loss of control of subsidiary, intangible assets other than goodwill')
,(0, 53, 0, 1, '/4/12/', 'DisposalsAndRetirementsIntangibleAssetsOtherThanGoodwill', 'Disposals and retirements, intangible assets other than goodwill')
,(1, 54, 0, 1, '/4/12/1/', 'DisposalsIntangibleAssetsOtherThanGoodwill', 'Disposals, intangible assets other than goodwill')
,(1, 55, 0, 1, '/4/12/2/', 'RetirementsIntangibleAssetsOtherThanGoodwill', 'Retirements, intangible assets other than goodwill')
,(0, 56, 1, 1, '/4/13/', 'IncreaseDecreaseThroughTransfersAndOtherChangesIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through transfers and other changes, intangible assets other than goodwill')
,(1, 57, 1, 1, '/4/13/1/', 'IncreaseDecreaseThroughTransfersIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through transfers, intangible assets other than goodwill')
,(1, 58, 1, 1, '/4/13/2/', 'IncreaseDecreaseThroughOtherChangesIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through other changes, intangible assets other than goodwill')
,(0, 59, 1, 1, '/5/', 'ChangesInBiologicalAssets', 'Increase (decrease) in biological assets')
,(0, 60, 1, 0, '/5/1/', 'AdditionsOtherThanThroughBusinessCombinationsBiologicalAssets', 'Additions other than through business combinations, biological assets')
,(1, 61, 1, 0, '/5/1/1/', 'AdditionsFromSubsequentExpenditureRecognisedAsAssetBiologicalAssets', 'Additions from subsequent expenditure recognised as asset, biological assets')
,(1, 62, 1, 0, '/5/1/2/', 'AdditionsFromPurchasesBiologicalAssets', 'Additions from purchases, biological assets')
,(1, 63, 1, 0, '/5/2/', 'AcquisitionsThroughBusinessCombinationsBiologicalAssets', 'Acquisitions through business combinations, biological assets')
,(1, 64, 1, 1, '/5/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesBiologicalAssets', 'Increase (decrease) through net exchange differences, biological assets')
,(1, 65, 0, 1, '/5/4/', 'DepreciationBiologicalAssets', 'Depreciation, biological assets')
,(1, 66, 0, 1, '/5/5/', 'ImpairmentLossRecognisedInProfitOrLossBiologicalAssets', 'Impairment loss recognised in profit or loss, biological assets')
,(1, 67, 1, 0, '/5/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossBiologicalAssets', 'Reversal of impairment loss recognised in profit or loss, biological assets')
,(0, 68, 1, 1, '/5/7/', 'GainsLossesOnFairValueAdjustmentBiologicalAssets', 'Gains (losses) on fair value adjustment, biological assets')
,(1, 69, 1, 1, '/5/7/1/', 'GainsLossesOnFairValueAdjustmentAttributableToPhysicalChangesBiologicalAssets', 'Gains (losses) on fair value adjustment attributable to physical changes, biological assets')
,(1, 70, 1, 1, '/5/7/2/', 'GainsLossesOnFairValueAdjustmentAttributableToPriceChangesBiologicalAssets', 'Gains (losses) on fair value adjustment attributable to price changes, biological assets')
,(1, 71, 1, 1, '/5/8/', 'IncreaseDecreaseThroughTransfersAndOtherChangesBiologicalAssets', 'Increase (decrease) through other changes, biological assets')
,(1, 72, 0, 1, '/5/9/', 'DisposalsBiologicalAssets', 'Disposals, biological assets')
,(1, 73, 0, 1, '/5/10/', 'DecreaseDueToHarvestBiologicalAssets', 'Decrease due to harvest, biological assets')
,(1, 74, 0, 1, '/5/11/', 'DecreaseThroughClassifiedAsHeldForSaleBiologicalAssets', 'Decrease through classified as held for sale, biological assets')
,(0, 75, 1, 1, '/6/', 'IncreaseDecreaseInCashAndCashEquivalents', 'Increase (decrease) in cash and cash equivalents')
,(0, 76, 1, 1, '/6/1/', 'IncreaseDecreaseInCashAndCashEquivalentsBeforeEffectOfExchangeRateChanges', 'Increase (decrease) in cash and cash equivalents before effect of exchange rate changes')
,(0, 77, 1, 1, '/6/1/1/', 'CashFlowsFromUsedInOperatingActivities', 'Cash flows from (used in) operating activities')
,(0, 78, 1, 1, '/6/1/1/1/', 'CashFlowsFromUsedInOperations', 'Cash flows from (used in) operations')
,(1, 79, 1, 0, '/6/1/1/1/1/', 'ReceiptsFromSalesOfGoodsAndRenderingOfServices', 'Receipts from sales of goods and rendering of services')
,(1, 80, 1, 0, '/6/1/1/1/2/', 'ReceiptsFromRoyaltiesFeesCommissionsAndOtherRevenue', 'Receipts from royalties, fees, commissions and other revenue')
,(1, 81, 1, 0, '/6/1/1/1/3/', 'ReceiptsFromContractsHeldForDealingOrTradingPurpose', 'Receipts from contracts held for dealing or trading purposes')
,(1, 82, 1, 0, '/6/1/1/1/4/', 'ReceiptsFromPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', 'Receipts from premiums and claims, annuities and other policy benefits')
,(1, 83, 1, 0, '/6/1/1/1/5/', 'ReceiptsFromRentsAndSubsequentSalesOfSuchAssets', 'Receipts from rents and subsequent sales of assets held for rental to others and subsequently held for sale')
,(1, 84, 1, 0, '/6/1/1/1/6/', 'OtherCashReceiptsFromOperatingActivities', 'Other cash receipts from operating activities')
,(1, 85, 0, 1, '/6/1/1/1/7/', 'PaymentsToSuppliersForGoodsAndServices', 'Payments to suppliers for goods and services')
,(1, 86, 1, 0, '/6/1/1/1/8/', 'PaymentsFromContractsHeldForDealingOrTradingPurpose', 'Payments from contracts held for dealing or trading purpose')
,(1, 87, 0, 1, '/6/1/1/1/9/', 'PaymentsToAndOnBehalfOfEmployees', 'Payments to and on behalf of employees')
,(1, 88, 0, 1, '/6/1/1/1/10/', 'PaymentsForPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', 'Payments for premiums and claims, annuities and other policy benefits')
,(1, 89, 0, 1, '/6/1/1/1/11/', 'PaymentsToManufactureOrAcquireAssetsHeldForRentalToOthersAndSubsequentlyHeldForSale', 'Payments to manufacture or acquire assets held for rental to others and subsequently held for sale')
,(1, 90, 0, 1, '/6/1/1/1/12/', 'OtherCashPaymentsFromOperatingActivities', 'Other cash payments from operating activities')
,(1, 91, 0, 1, '/6/1/1/2/', 'DividendsPaidClassifiedAsOperatingActivities', 'Dividends paid, classified as operating activities')
,(1, 92, 1, 0, '/6/1/1/3/', 'DividendsReceivedClassifiedAsOperatingActivities', 'Dividends received, classified as operating activities')
,(1, 93, 0, 1, '/6/1/1/4/', 'InterestPaidClassifiedAsOperatingActivities', 'Interest paid, classified as operating activities')
,(1, 94, 1, 0, '/6/1/1/5/', 'InterestReceivedClassifiedAsOperatingActivities', 'Interest received, classified as operating activities')
,(1, 95, 1, 1, '/6/1/1/6/', 'IncomeTaxesPaidRefundClassifiedAsOperatingActivities', 'Income taxes paid (refund), classified as operating activities')
,(1, 96, 1, 1, '/6/1/1/7/', 'OtherInflowsOutflowsOfCashClassifiedAsOperatingActivities', 'Other inflows (outflows) of cash, classified as operating activities')
,(0, 97, 1, 1, '/6/1/2/', 'CashFlowsFromUsedInInvestingActivities', 'Cash flows from (used in) investing activities')
,(1, 98, 1, 1, '/6/1/2/1/', 'CashFlowsFromLosingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', 'Cash flows from losing control of subsidiaries or other businesses, classified as investing activities')
,(1, 99, 0, 1, '/6/1/2/2/', 'CashFlowsUsedInObtainingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', 'Cash flows used in obtaining control of subsidiaries or other businesses, classified as investing activities')
,(1, 100, 1, 0, '/6/1/2/3/', 'OtherCashReceiptsFromSalesOfEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', 'Other cash receipts from sales of equity or debt instruments of other entities, classified as investing activities')
,(1, 101, 0, 1, '/6/1/2/4/', 'OtherCashPaymentsToAcquireEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', 'Other cash payments to acquire equity or debt instruments of other entities, classified as investing activities')
,(1, 102, 1, 0, '/6/1/2/5/', 'OtherCashReceiptsFromSalesOfInterestsInJointVenturesClassifiedAsInvestingActivities', 'Other cash receipts from sales of interests in joint ventures, classified as investing activities')
,(1, 103, 0, 1, '/6/1/2/6/', 'OtherCashPaymentsToAcquireInterestsInJointVenturesClassifiedAsInvestingActivities', 'Other cash payments to acquire interests in joint ventures, classified as investing activities')
,(1, 104, 1, 0, '/6/1/2/7/', 'ProceedsFromSalesOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', 'Proceeds from sales of property, plant and equipment, classified as investing activities')
,(1, 105, 0, 1, '/6/1/2/8/', 'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', 'Purchase of property, plant and equipment, classified as investing activities')
,(1, 106, 1, 0, '/6/1/2/9/', 'ProceedsFromSalesOfIntangibleAssetsClassifiedAsInvestingActivities', 'Proceeds from sales of intangible assets, classified as investing activities')
,(1, 107, 0, 1, '/6/1/2/10/', 'PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities', 'Purchase of intangible assets, classified as investing activities')
,(1, 108, 1, 0, '/6/1/2/11/', 'ProceedsFromOtherLongtermAssetsClassifiedAsInvestingActivities', 'Proceeds from sales of other long-term assets, classified as investing activities')
,(1, 109, 0, 1, '/6/1/2/12/', 'PurchaseOfOtherLongtermAssetsClassifiedAsInvestingActivities', 'Purchase of other long-term assets, classified as investing activities')
,(1, 110, 1, 0, '/6/1/2/13/', 'ProceedsFromGovernmentGrantsClassifiedAsInvestingActivities', 'Proceeds from government grants, classified as investing activities')
,(1, 111, 0, 1, '/6/1/2/14/', 'CashAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', 'Cash advances and loans made to other parties, classified as investing activities')
,(1, 112, 1, 0, '/6/1/2/15/', 'CashReceiptsFromRepaymentOfAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', 'Cash receipts from repayment of advances and loans made to other parties, classified as investing activities')
,(1, 113, 0, 1, '/6/1/2/16/', 'CashPaymentsForFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', 'Cash payments for futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities')
,(1, 114, 1, 0, '/6/1/2/17/', 'CashReceiptsFromFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', 'Cash receipts from futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities')
,(1, 115, 1, 1, '/6/1/2/18/', 'DividendsReceivedClassifiedAsInvestingActivities', 'Dividends received, classified as investing activities')
,(1, 116, 0, 1, '/6/1/2/19/', 'InterestPaidClassifiedAsInvestingActivities', 'Interest paid, classified as investing activities')
,(1, 117, 1, 0, '/6/1/2/20/', 'InterestReceivedClassifiedAsInvestingActivities', 'Interest received, classified as investing activities')
,(1, 118, 1, 1, '/6/1/2/21/', 'IncomeTaxesPaidRefundClassifiedAsInvestingActivities', 'Income taxes paid (refund), classified as investing activities')
,(1, 119, 1, 1, '/6/1/2/22/', 'OtherInflowsOutflowsOfCashClassifiedAsInvestingActivities', 'Other inflows (outflows) of cash, classified as investing activities')
,(0, 120, 1, 1, '/6/1/3/', 'CashFlowsFromUsedInFinancingActivities', 'Cash flows from (used in) financing activities')
,(1, 121, 1, 0, '/6/1/3/1/', 'ProceedsFromChangesInOwnershipInterestsInSubsidiaries', 'Proceeds from changes in ownership interests in subsidiaries that do not result in loss of control')
,(1, 122, 0, 1, '/6/1/3/2/', 'PaymentsFromChangesInOwnershipInterestsInSubsidiaries', 'Payments from changes in ownership interests in subsidiaries that do not result in loss of control')
,(1, 123, 1, 0, '/6/1/3/3/', 'ProceedsFromIssuingShares', 'Proceeds from issuing shares')
,(1, 124, 1, 0, '/6/1/3/4/', 'ProceedsFromIssuingOtherEquityInstruments', 'Proceeds from issuing other equity instruments')
,(1, 125, 0, 1, '/6/1/3/5/', 'PaymentsToAcquireOrRedeemEntitysShares', 'Payments to acquire or redeem entity''s shares')
,(1, 126, 0, 1, '/6/1/3/6/', 'PaymentsOfOtherEquityInstruments', 'Payments of other equity instruments')
,(1, 127, 1, 0, '/6/1/3/7/', 'ProceedsFromBorrowingsClassifiedAsFinancingActivities', 'Proceeds from borrowings, classified as financing activities')
,(1, 128, 0, 1, '/6/1/3/8/', 'RepaymentsOfBorrowingsClassifiedAsFinancingActivities', 'Repayments of borrowings, classified as financing activities')
,(1, 129, 1, 1, '/6/1/3/9/', 'PaymentsOfLeaseLiabilitiesClassifiedAsFinancingActivities', 'Payments of lease liabilities, classified as financing activities')
,(1, 130, 1, 1, '/6/1/3/10/', 'ProceedsFromGovernmentGrantsClassifiedAsFinancingActivities', 'Proceeds from government grants, classified as financing activities')
,(1, 131, 1, 1, '/6/1/3/11/', 'DividendsPaidClassifiedAsFinancingActivities', 'Dividends paid, classified as financing activities')
,(1, 132, 1, 1, '/6/1/3/12/', 'InterestPaidClassifiedAsFinancingActivities', 'Interest paid, classified as financing activities')
,(1, 133, 1, 1, '/6/1/3/13/', 'IncomeTaxesPaidRefundClassifiedAsFinancingActivities', 'Income taxes paid (refund), classified as financing activities')
,(1, 134, 1, 1, '/6/1/3/14/', 'OtherInflowsOutflowsOfCashClassifiedAsFinancingActivities', 'Other inflows (outflows) of cash, classified as financing activities')
,(1, 135, 1, 1, '/6/1/4/', 'InternalCashTransferExtension', 'Internal cash transfer')
,(1, 136, 1, 1, '/6/2/', 'EffectOfExchangeRateChangesOnCashAndCashEquivalents', 'Effect of exchange rate changes on cash and cash equivalents')
,(0, 137, 1, 1, '/7/', 'ChangesInEquity', 'Increase (decrease) in equity')
,(0, 138, 1, 1, '/7/1/', 'ComprehensiveIncome', 'Comprehensive income')
,(1, 139, 1, 1, '/7/1/1/', 'ProfitLoss', 'Profit (loss)')
,(1, 140, 1, 1, '/7/1/2/', 'OtherComprehensiveIncome', 'Other comprehensive income')
,(1, 141, 0, 1, '/7/2/', 'IssueOfEquity', 'Issue of equity')
,(1, 142, 0, 1, '/7/3/', 'DividendsPaid', 'Dividends recognised as distributions to owners')
,(1, 143, 1, 1, '/7/4/', 'IncreaseDecreaseThroughOtherContributionsByOwners', 'Increase through other contributions by owners, equity')
,(1, 144, 1, 1, '/7/5/', 'IncreaseDecreaseThroughOtherDistributionsToOwners', 'Decrease through other distributions to owners, equity')
,(1, 145, 1, 1, '/7/6/', 'IncreaseDecreaseThroughTransfersAndOtherChangesEquity', 'Increase (decrease) through other changes, equity')
,(1, 146, 1, 1, '/7/7/', 'IncreaseDecreaseThroughTreasuryShareTransactions', 'Increase (decrease) through treasury share transactions, equity')
,(1, 147, 1, 1, '/7/8/', 'IncreaseDecreaseThroughChangesInOwnershipInterestsInSubsidiariesThatDoNotResultInLossOfControl', 'Increase (decrease) through changes in ownership interests in subsidiaries that do not result in loss of control, equity')
,(1, 148, 1, 1, '/7/9/', 'IncreaseDecreaseThroughSharebasedPaymentTransactions', 'Increase (decrease) through share-based payment transactions, equity')
,(1, 149, 1, 1, '/7/10/', 'AmountRemovedFromReserveOfCashFlowHedgesAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of cash flow hedges and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,(1, 150, 1, 1, '/7/11/', 'AmountRemovedFromReserveOfChangeInValueOfTimeValueOfOptionsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of change in value of time value of options and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,(1, 151, 1, 1, '/7/12/', 'AmountRemovedFromReserveOfChangeInValueOfForwardElementsOfForwardContractsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of change in value of forward elements of forward contracts and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,(1, 152, 1, 1, '/7/13/', 'AmountRemovedFromReserveOfChangeInValueOfForeignCurrencyBasisSpreadsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of change in value of foreign currency basis spreads and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,(0, 153, 1, 1, '/8/', 'ChangesInOtherProvisions', 'Increase (decrease) in other provisions')
,(0, 154, 1, 1, '/8/1/', 'AdditionalProvisionsOtherProvisions', 'Additional provisions, other provisions')
,(1, 155, 1, 1, '/8/1/1/', 'NewProvisionsOtherProvisions', 'New provisions, other provisions')
,(1, 156, 1, 1, '/8/1/2/', 'IncreaseDecreaseInExistingProvisionsOtherProvisions', 'Increase in existing provisions, other provisions')
,(1, 157, 1, 1, '/8/2/', 'AcquisitionsThroughBusinessCombinationsOtherProvisions', 'Acquisitions through business combinations, other provisions')
,(1, 158, 1, 1, '/8/3/', 'ProvisionUsedOtherProvisions', 'Provision used, other provisions')
,(1, 159, 1, 1, '/8/4/', 'UnusedProvisionReversedOtherProvisions', 'Unused provision reversed, other provisions')
,(1, 160, 1, 1, '/8/5/', 'IncreaseDecreaseThroughTimeValueOfDECIMAL (19,4)AdjustmentOtherProvisions', 'Increase through adjustments arising from passage of time, other provisions')
,(1, 161, 1, 1, '/8/6/', 'IncreaseDecreaseThroughChangeInDiscountRateOtherProvisions', 'Increase (decrease) through change in discount rate, other provisions')
,(1, 162, 1, 1, '/8/7/', 'IncreaseDecreaseThroughNetExchangeDifferencesOtherProvisions', 'Increase (decrease) through net exchange differences, other provisions')
,(1, 163, 1, 1, '/8/8/', 'DecreaseThroughLossOfControlOfSubsidiaryOtherProvisions', 'Decrease through loss of control of subsidiary, other provisions')
,(1, 164, 1, 1, '/8/9/', 'IncreaseDecreaseThroughTransfersAndOtherChangesOtherProvisions', 'Increase (decrease) through transfers and other changes, other provisions')

,(0, 165, 1, 1, '/9/', 'ExpenseByFunctionExtension', 'Expense, by function')
,(1, 166, 0, 1, '/9/1/', 'CostOfSales', 'Cost of sales')
,(1, 167, 0, 1, '/9/2/', 'DistributionCosts', 'Distribution costs')
,(1, 168, 0, 1, '/9/3/', 'AdministrativeExpense', 'Administrative expenses')
,(1, 169, 0, 1, '/9/4/', 'OtherExpenseByFunction', 'Other expense, by function')

,(0, 170, 1, 1, '/10/',	N'ChangesInInventories', 'Increase (decrease) in inventories')
,(1, 171, 1, 0, '/10/1/', N'InventoryPurchaseExtension', 'Inventory purchase')
,(1, 172, 1, 1, '/10/2/', N'InventoryProductionExtension', 'Inventory production') -- for both input and output
,(1, 173, 0, 1, '/10/3/', N'InventorySalesExtension', 'Inventory sales')
,(1, 174, 0, 1, '/10/4/', N'InventoryConsumptionExtension', 'Inventory consumption') -- Selling, Distribution, and General Admin
,(1, 175, 0, 1, '/10/5/', N'InventoryLossExtension', 'Inventory loss') -- Oracle as: adjustment and physical inventory
,(1, 176, 0, 1, '/10/6/', N'InventoryReclassifiedAsPropertyPlantAndEquipment', 'Inventory reclassified as property, plant and equipment') -- reclassification
,(1, 177, 1, 0, '/10/7/', N'PropertyPlantAndEquipmentReclassifiedAsInventory', 'Fixed asset to inventory conversion') -- merge with previous
,(1, 178, 1, 1, '/10/9/', N'InternalInventoryTransferExtension', 'Inventory transfer')
-- TODO: we also need to add customer return and supplier return

DECLARE @EntryTypes dbo.EntryTypeList;

INSERT INTO @EntryTypes ([IsAssignable], [Index], [ForDebit], [ForCredit], [ParentIndex], [Code], [Name])
SELECT [IsAssignable], [Index], [ForDebit], [ForCredit], (SELECT [Index] FROM @EntryTypesTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [Code], [Name]
FROM @EntryTypesTemp RC	

EXEC [api].[EntryTypes__Save]
	@Entities = @EntryTypes,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Entry Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;									

UPDATE dbo.[EntryTypes] SET IsSystem = 1;

DECLARE @PaymentsToSuppliersForGoodsAndServices INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'PaymentsToSuppliersForGoodsAndServices' );
DECLARE @PaymentsToAndOnBehalfOfEmployees INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'PaymentsToAndOnBehalfOfEmployees' );


DECLARE @ReceiptsFromSalesOfGoodsAndRenderingOfServices	INT	 = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'ReceiptsFromSalesOfGoodsAndRenderingOfServices' );
DECLARE @ProceedsFromIssuingShares	INT	 = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'ProceedsFromIssuingShares' );
DECLARE @IssueOfEquity				INT	 = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'IssueOfEquity' );
DECLARE @InternalCashTransfer	INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'InternalCashTransferExtension' );
DECLARE @ProceedsFromBorrowingsClassifiedAsFinancingActivities	INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'ProceedsFromBorrowingsClassifiedAsFinancingActivities' );



DECLARE @InventoryPurchaseExtension	INT	 = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'InventoryPurchaseExtension' );
DECLARE @InternalInventoryTransferExtension INT = (SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = N'InternalInventoryTransferExtension');

DECLARE @PPEAdditions			INT		 = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment' );
DECLARE @InvReclassifiedAsPPE	INT		 = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'InventoryReclassifiedAsPropertyPlantAndEquipment' );
DECLARE @CostOfSales INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'CostOfSales' );
DECLARE @DistributionCosts	INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'DistributionCosts' );
DECLARE @AdministrativeExpense	INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'AdministrativeExpense' );
DECLARE @OEF	INT = (SELECT [Id] FROM dbo.[EntryTypes] WHERE [Code] = N'OtherExpenseByFunction' );