DECLARE @ET TABLE ([Index] INT, [IsAbstract] BIT, [IsActive] BIT, [Node] HIERARCHYID, [Code] NVARCHAR(255), [Name] NVARCHAR(255))

INSERT INTO @ET VALUES(0, 1, 1, N'/1/', N'ChangesInPropertyPlantAndEquipment', N'Increase (decrease) in property, plant and equipment')
INSERT INTO @ET VALUES(1, 0, 1, N'/1/1/', N'Additions other than through business combinations, property, plant and equipment', N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(2, 0, 0, N'/1/2/', N'Acquisitions through business combinations, property, plant and equipment', N'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(3, 1, 0, N'/1/3/', N'Increase (decrease) through net exchange differences, property, plant and equipment', N'IncreaseDecreaseThroughNetExchangeDifferencesPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(4, 0, 1, N'/1/4/', N'Depreciation, property, plant and equipment', N'DepreciationPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(5, 0, 0, N'/1/5/', N'Impairment loss recognised in profit or loss, property, plant and equipment', N'ImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(6, 0, 0, N'/1/6/', N'Reversal of impairment loss recognised in profit or loss, property, plant and equipment', N'ReversalOfImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(7, 0, 1, N'/1/7/', N'Revaluation increase (decrease), property, plant and equipment', N'RevaluationIncreaseDecreasePropertyPlantAndEquipment')
INSERT INTO @ET VALUES(8, 0, 0, N'/1/8/', N'Impairment loss recognised in other comprehensive income, property, plant and equipment', N'ImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment')
INSERT INTO @ET VALUES(9, 0, 0, N'/1/9/', N'Reversal of impairment loss recognised in other comprehensive income, property, plant and equipment', N'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment')
INSERT INTO @ET VALUES(10, 0, 0, N'/1/10/', N'Increase (decrease) through transfers and other changes, property, plant and equipment [abstract]', N'IncreaseDecreaseThroughTransfersAndOtherChangesPropertyPlantAndEquipmentAbstract')
INSERT INTO @ET VALUES(11, 0, 0, N'/1/10/1/', N'Increase (decrease) through transfers, property, plant and equipment', N'IncreaseDecreaseThroughTransfersPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(12, 0, 0, N'/1/10/2/', N'Increase (decrease) through other changes, property, plant and equipment', N'IncreaseDecreaseThroughOtherChangesPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(13, 0, 1, N'/1/11/', N'Disposals and retirements, property, plant and equipment', N'DisposalsAndRetirementsPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(14, 0, 1, N'/1/11/1/', N'Disposals, property, plant and equipment', N'DisposalsPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(15, 1, 1, N'/1/11/2/', N'Retirements, property, plant and equipment', N'RetirementsPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(16, 0, 1, N'/1/12/', N'Decrease through classified as held for sale, property, plant and equipment', N'DecreaseThroughClassifiedAsHeldForSalePropertyPlantAndEquipment')
INSERT INTO @ET VALUES(17, 0, 0, N'/1/13/', N'Decrease through loss of control of subsidiary, property, plant and equipment', N'DecreaseThroughLossOfControlOfSubsidiaryPropertyPlantAndEquipment')
INSERT INTO @ET VALUES(18, 1, 1, N'/2/', N'ChangesInInvestmentProperty', N'Increase (decrease) in investment property')
INSERT INTO @ET VALUES(19, 1, 1, N'/2/1/', N'AdditionsOtherThanThroughBusinessCombinationsInvestmentProperty', N'Additions other than through business combinations, investment property')
INSERT INTO @ET VALUES(20, 0, 0, N'/2/1/1/', N'AdditionsFromSubsequentExpenditureRecognisedAsAssetInvestmentProperty', N'Additions from subsequent expenditure recognised as asset, investment property')
INSERT INTO @ET VALUES(21, 0, 0, N'/2/1/2/', N'AdditionsFromAcquisitionsInvestmentProperty', N'Additions from acquisitions, investment property')
INSERT INTO @ET VALUES(22, 0, 0, N'/2/2/', N'AcquisitionsThroughBusinessCombinationsInvestmentProperty', N'Acquisitions through business combinations, investment property')
INSERT INTO @ET VALUES(23, 0, 0, N'/2/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesInvestmentProperty', N'Increase (decrease) through net exchange differences, investment property')
INSERT INTO @ET VALUES(24, 0, 1, N'/2/4/', N'DepreciationInvestmentProperty', N'Depreciation, investment property')
INSERT INTO @ET VALUES(25, 0, 0, N'/2/5/', N'ImpairmentLossRecognisedInProfitOrLossInvestmentProperty', N'Impairment loss recognised in profit or loss, investment property')
INSERT INTO @ET VALUES(26, 0, 0, N'/2/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossInvestmentProperty', N'Reversal of impairment loss recognised in profit or loss, investment property')
INSERT INTO @ET VALUES(27, 0, 1, N'/2/7/', N'GainsLossesOnFairValueAdjustmentInvestmentProperty', N'Gains (losses) on fair value adjustment, investment property')
INSERT INTO @ET VALUES(28, 0, 0, N'/2/8/', N'TransferFromToInventoriesAndOwnerOccupiedPropertyInvestmentProperty', N'Transfer from (to) inventories and owner-occupied property, investment property')
INSERT INTO @ET VALUES(29, 0, 0, N'/2/9/', N'TransferFromInvestmentPropertyUnderConstructionOrDevelopmentInvestmentProperty', N'Transfer from investment property under construction or development, investment property')
INSERT INTO @ET VALUES(30, 0, 0, N'/2/10/', N'DisposalsInvestmentProperty', N'Disposals, investment property')
INSERT INTO @ET VALUES(31, 0, 0, N'/2/11/', N'DecreaseThroughClassifiedAsHeldForSaleInvestmentProperty', N'Decrease through classified as held for sale, investment property')
INSERT INTO @ET VALUES(32, 0, 1, N'/2/12/', N'IncreaseDecreaseThroughOtherChangesInvestmentProperty', N'Increase (decrease) through other changes, investment property')
INSERT INTO @ET VALUES(33, 1, 1, N'/3/', N'ChangesInGoodwill', N'Increase (decrease) in goodwill')
INSERT INTO @ET VALUES(34, 0, 1, N'/3/1/', N'SubsequentRecognitionOfDeferredTaxAssetsGoodwill', N'Subsequent recognition of deferred tax assets, goodwill')
INSERT INTO @ET VALUES(35, 0, 1, N'/3/2/', N'IncreaseDecreaseThroughTransfersAndOtherChangesGoodwill', N'Increase (decrease) through other changes, goodwill')
INSERT INTO @ET VALUES(36, 0, 1, N'/3/3/', N'AdditionalRecognitionGoodwill', N'Additional recognition, goodwill')
INSERT INTO @ET VALUES(37, 0, 1, N'/3/4/', N'DecreaseThroughClassifiedAsHeldForSaleGoodwill', N'Decrease through classified as held for sale, goodwill')
INSERT INTO @ET VALUES(38, 0, 1, N'/3/5/', N'GoodwillDerecognisedWithoutHavingPreviouslyBeenIncludedInDisposalGroupClassifiedAsHeldForSale', N'Goodwill derecognised without having previously been included in disposal group classified as held for sale')
INSERT INTO @ET VALUES(39, 1, 1, N'/3/6/', N'ImpairmentLossRecognisedInProfitOrLossGoodwill', N'Impairment loss recognised in profit or loss, goodwill')
INSERT INTO @ET VALUES(40, 1, 1, N'/3/7/', N'IncreaseDecreaseThroughNetExchangeDifferencesGoodwill', N'Increase (decrease) through net exchange differences, goodwill')
INSERT INTO @ET VALUES(41, 1, 1, N'/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) in intangible assets other than goodwill')
INSERT INTO @ET VALUES(42, 0, 1, N'/4/1/', N'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', N'Additions other than through business combinations, intangible assets other than goodwill')
INSERT INTO @ET VALUES(43, 0, 1, N'/4/2/', N'AcquisitionsThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', N'Acquisitions through business combinations, intangible assets other than goodwill')
INSERT INTO @ET VALUES(44, 1, 1, N'/4/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through net exchange differences, intangible assets other than goodwill')
INSERT INTO @ET VALUES(45, 0, 1, N'/4/4/', N'AmortisationIntangibleAssetsOtherThanGoodwill', N'Amortisation, intangible assets other than goodwill')
INSERT INTO @ET VALUES(46, 0, 1, N'/4/5/', N'ImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', N'Impairment loss recognised in profit or loss, intangible assets other than goodwill')
INSERT INTO @ET VALUES(47, 0, 1, N'/4/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', N'Reversal of impairment loss recognised in profit or loss, intangible assets other than goodwill')
INSERT INTO @ET VALUES(48, 0, 1, N'/4/7/', N'RevaluationIncreaseDecreaseIntangibleAssetsOtherThanGoodwill', N'Revaluation increase (decrease), intangible assets other than goodwill')
INSERT INTO @ET VALUES(49, 0, 1, N'/4/8/', N'ImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', N'Impairment loss recognised in other comprehensive income, intangible assets other than goodwill')
INSERT INTO @ET VALUES(50, 0, 1, N'/4/9/', N'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', N'Reversal of impairment loss recognised in other comprehensive income, intangible assets other than goodwill')
INSERT INTO @ET VALUES(51, 1, 1, N'/4/10/', N'DecreaseThroughClassifiedAsHeldForSaleIntangibleAssetsOtherThanGoodwill', N'Decrease through classified as held for sale, intangible assets other than goodwill')
INSERT INTO @ET VALUES(52, 0, 1, N'/4/11/', N'DecreaseThroughLossOfControlOfSubsidiaryIntangibleAssetsOtherThanGoodwill', N'Decrease through loss of control of subsidiary, intangible assets other than goodwill')
INSERT INTO @ET VALUES(53, 0, 1, N'/4/12/', N'DisposalsAndRetirementsIntangibleAssetsOtherThanGoodwill', N'Disposals and retirements, intangible assets other than goodwill')
INSERT INTO @ET VALUES(54, 0, 1, N'/4/12/1/', N'DisposalsIntangibleAssetsOtherThanGoodwill', N'Disposals, intangible assets other than goodwill')
INSERT INTO @ET VALUES(55, 0, 1, N'/4/12/2/', N'RetirementsIntangibleAssetsOtherThanGoodwill', N'Retirements, intangible assets other than goodwill')
INSERT INTO @ET VALUES(56, 0, 1, N'/4/13/', N'IncreaseDecreaseThroughTransfersAndOtherChangesIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through transfers and other changes, intangible assets other than goodwill')
INSERT INTO @ET VALUES(57, 0, 1, N'/4/13/1/', N'IncreaseDecreaseThroughTransfersIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through transfers, intangible assets other than goodwill')
INSERT INTO @ET VALUES(58, 0, 1, N'/4/13/2/', N'IncreaseDecreaseThroughOtherChangesIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through other changes, intangible assets other than goodwill')
INSERT INTO @ET VALUES(59, 1, 1, N'/5/', N'ChangesInBiologicalAssets', N'Increase (decrease) in biological assets')
INSERT INTO @ET VALUES(60, 0, 1, N'/5/1/', N'AdditionsOtherThanThroughBusinessCombinationsBiologicalAssets', N'Additions other than through business combinations, biological assets')
INSERT INTO @ET VALUES(61, 0, 0, N'/5/1/1/', N'AdditionsFromSubsequentExpenditureRecognisedAsAssetBiologicalAssets', N'Additions from subsequent expenditure recognised as asset, biological assets')
INSERT INTO @ET VALUES(62, 0, 0, N'/5/1/2/', N'AdditionsFromPurchasesBiologicalAssets', N'Additions from purchases, biological assets')
INSERT INTO @ET VALUES(63, 0, 0, N'/5/2/', N'AcquisitionsThroughBusinessCombinationsBiologicalAssets', N'Acquisitions through business combinations, biological assets')
INSERT INTO @ET VALUES(64, 0, 0, N'/5/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesBiologicalAssets', N'Increase (decrease) through net exchange differences, biological assets')
INSERT INTO @ET VALUES(65, 0, 1, N'/5/4/', N'DepreciationBiologicalAssets', N'Depreciation, biological assets')
INSERT INTO @ET VALUES(66, 0, 0, N'/5/5/', N'ImpairmentLossRecognisedInProfitOrLossBiologicalAssets', N'Impairment loss recognised in profit or loss, biological assets')
INSERT INTO @ET VALUES(67, 0, 0, N'/5/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossBiologicalAssets', N'Reversal of impairment loss recognised in profit or loss, biological assets')
INSERT INTO @ET VALUES(68, 0, 1, N'/5/7/', N'GainsLossesOnFairValueAdjustmentBiologicalAssets', N'Gains (losses) on fair value adjustment, biological assets')
INSERT INTO @ET VALUES(69, 0, 0, N'/5/7/1/', N'GainsLossesOnFairValueAdjustmentAttributableToPhysicalChangesBiologicalAssets', N'Gains (losses) on fair value adjustment attributable to physical changes, biological assets')
INSERT INTO @ET VALUES(70, 0, 0, N'/5/7/2/', N'GainsLossesOnFairValueAdjustmentAttributableToPriceChangesBiologicalAssets', N'Gains (losses) on fair value adjustment attributable to price changes, biological assets')
INSERT INTO @ET VALUES(71, 0, 0, N'/5/8/', N'IncreaseDecreaseThroughTransfersAndOtherChangesBiologicalAssets', N'Increase (decrease) through other changes, biological assets')
INSERT INTO @ET VALUES(72, 0, 1, N'/5/9/', N'DisposalsBiologicalAssets', N'Disposals, biological assets')
INSERT INTO @ET VALUES(73, 0, 0, N'/5/10/', N'DecreaseDueToHarvestBiologicalAssets', N'Decrease due to harvest, biological assets')
INSERT INTO @ET VALUES(74, 0, 0, N'/5/11/', N'DecreaseThroughClassifiedAsHeldForSaleBiologicalAssets', N'Decrease through classified as held for sale, biological assets')
INSERT INTO @ET VALUES(75, 1, 1, N'/6/', N'IncreaseDecreaseInCashAndCashEquivalents', N'Increase (decrease) in cash and cash equivalents')
INSERT INTO @ET VALUES(76, 1, 1, N'/6/1/', N'IncreaseDecreaseInCashAndCashEquivalentsBeforeEffectOfExchangeRateChanges', N'Increase (decrease) in cash and cash equivalents before effect of exchange rate changes')
INSERT INTO @ET VALUES(77, 0, 1, N'/6/1/1/', N'CashFlowsFromUsedInOperatingActivities', N'Cash flows from (used in) operating activities')
INSERT INTO @ET VALUES(78, 1, 1, N'/6/1/1/1/', N'CashFlowsFromUsedInOperations', N'Cash flows from (used in) operations')
INSERT INTO @ET VALUES(79, 0, 0, N'/6/1/1/1/1/', N'ReceiptsFromSalesOfGoodsAndRenderingOfServices', N'Receipts from sales of goods and rendering of services')
INSERT INTO @ET VALUES(80, 0, 0, N'/6/1/1/1/2/', N'ReceiptsFromRoyaltiesFeesCommissionsAndOtherRevenue', N'Receipts from royalties, fees, commissions and other revenue')
INSERT INTO @ET VALUES(81, 0, 1, N'/6/1/1/1/3/', N'ReceiptsFromContractsHeldForDealingOrTradingPurpose', N'Receipts from contracts held for dealing or trading purposes')
INSERT INTO @ET VALUES(82, 0, 0, N'/6/1/1/1/4/', N'ReceiptsFromPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', N'Receipts from premiums and claims, annuities and other policy benefits')
INSERT INTO @ET VALUES(83, 0, 0, N'/6/1/1/1/5/', N'ReceiptsFromRentsAndSubsequentSalesOfSuchAssets', N'Receipts from rents and subsequent sales of assets held for rental to others and subsequently held for sale')
INSERT INTO @ET VALUES(84, 0, 1, N'/6/1/1/1/6/', N'OtherCashReceiptsFromOperatingActivities', N'Other cash receipts from operating activities')
INSERT INTO @ET VALUES(85, 0, 0, N'/6/1/1/1/7/', N'PaymentsToSuppliersForGoodsAndServices', N'Payments to suppliers for goods and services')
INSERT INTO @ET VALUES(86, 0, 0, N'/6/1/1/1/8/', N'PaymentsFromContractsHeldForDealingOrTradingPurpose', N'Payments from contracts held for dealing or trading purpose')
INSERT INTO @ET VALUES(87, 0, 0, N'/6/1/1/1/9/', N'PaymentsToAndOnBehalfOfEmployees', N'Payments to and on behalf of employees')
INSERT INTO @ET VALUES(88, 0, 0, N'/6/1/1/1/10/', N'PaymentsForPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', N'Payments for premiums and claims, annuities and other policy benefits')
INSERT INTO @ET VALUES(89, 0, 0, N'/6/1/1/1/11/', N'PaymentsToManufactureOrAcquireAssetsHeldForRentalToOthersAndSubsequentlyHeldForSale', N'Payments to manufacture or acquire assets held for rental to others and subsequently held for sale')
INSERT INTO @ET VALUES(90, 0, 0, N'/6/1/1/1/12/', N'OtherCashPaymentsFromOperatingActivities', N'Other cash payments from operating activities')
INSERT INTO @ET VALUES(91, 0, 1, N'/6/1/1/2/', N'DividendsPaidClassifiedAsOperatingActivities', N'Dividends paid, classified as operating activities')
INSERT INTO @ET VALUES(92, 0, 1, N'/6/1/1/3/', N'DividendsReceivedClassifiedAsOperatingActivities', N'Dividends received, classified as operating activities')
INSERT INTO @ET VALUES(93, 1, 1, N'/6/1/1/4/', N'InterestPaidClassifiedAsOperatingActivities', N'Interest paid, classified as operating activities')
INSERT INTO @ET VALUES(94, 0, 0, N'/6/1/1/5/', N'InterestReceivedClassifiedAsOperatingActivities', N'Interest received, classified as operating activities')
INSERT INTO @ET VALUES(95, 0, 0, N'/6/1/1/6/', N'IncomeTaxesPaidRefundClassifiedAsOperatingActivities', N'Income taxes paid (refund), classified as operating activities')
INSERT INTO @ET VALUES(96, 0, 0, N'/6/1/1/7/', N'OtherInflowsOutflowsOfCashClassifiedAsOperatingActivities', N'Other inflows (outflows) of cash, classified as operating activities')
INSERT INTO @ET VALUES(97, 1, 1, N'/6/1/2/', N'CashFlowsFromUsedInInvestingActivities', N'Cash flows from (used in) investing activities')
INSERT INTO @ET VALUES(98, 0, 0, N'/6/1/2/1/', N'CashFlowsFromLosingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', N'Cash flows from losing control of subsidiaries or other businesses, classified as investing activities')
INSERT INTO @ET VALUES(99, 0, 0, N'/6/1/2/2/', N'CashFlowsUsedInObtainingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', N'Cash flows used in obtaining control of subsidiaries or other businesses, classified as investing activities')
INSERT INTO @ET VALUES(100, 0, 0, N'/6/1/2/3/', N'OtherCashReceiptsFromSalesOfEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', N'Other cash receipts from sales of equity or debt instruments of other entities, classified as investing activities')
INSERT INTO @ET VALUES(101, 0, 0, N'/6/1/2/4/', N'OtherCashPaymentsToAcquireEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', N'Other cash payments to acquire equity or debt instruments of other entities, classified as investing activities')
INSERT INTO @ET VALUES(102, 0, 0, N'/6/1/2/5/', N'OtherCashReceiptsFromSalesOfInterestsInJointVenturesClassifiedAsInvestingActivities', N'Other cash receipts from sales of interests in joint ventures, classified as investing activities')
INSERT INTO @ET VALUES(103, 0, 0, N'/6/1/2/6/', N'OtherCashPaymentsToAcquireInterestsInJointVenturesClassifiedAsInvestingActivities', N'Other cash payments to acquire interests in joint ventures, classified as investing activities')
INSERT INTO @ET VALUES(104, 0, 1, N'/6/1/2/7/', N'ProceedsFromSalesOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', N'Proceeds from sales of property, plant and equipment, classified as investing activities')
INSERT INTO @ET VALUES(105, 0, 1, N'/6/1/2/8/', N'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', N'Purchase of property, plant and equipment, classified as investing activities')
INSERT INTO @ET VALUES(106, 0, 1, N'/6/1/2/9/', N'ProceedsFromSalesOfIntangibleAssetsClassifiedAsInvestingActivities', N'Proceeds from sales of intangible assets, classified as investing activities')
INSERT INTO @ET VALUES(107, 0, 1, N'/6/1/2/10/', N'PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities', N'Purchase of intangible assets, classified as investing activities')
INSERT INTO @ET VALUES(108, 0, 0, N'/6/1/2/11/', N'ProceedsFromOtherLongtermAssetsClassifiedAsInvestingActivities', N'Proceeds from sales of other long-term assets, classified as investing activities')
INSERT INTO @ET VALUES(109, 0, 0, N'/6/1/2/12/', N'PurchaseOfOtherLongtermAssetsClassifiedAsInvestingActivities', N'Purchase of other long-term assets, classified as investing activities')
INSERT INTO @ET VALUES(110, 0, 1, N'/6/1/2/13/', N'ProceedsFromGovernmentGrantsClassifiedAsInvestingActivities', N'Proceeds from government grants, classified as investing activities')
INSERT INTO @ET VALUES(111, 0, 0, N'/6/1/2/14/', N'CashAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', N'Cash advances and loans made to other parties, classified as investing activities')
INSERT INTO @ET VALUES(112, 0, 0, N'/6/1/2/15/', N'CashReceiptsFromRepaymentOfAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', N'Cash receipts from repayment of advances and loans made to other parties, classified as investing activities')
INSERT INTO @ET VALUES(113, 0, 0, N'/6/1/2/16/', N'CashPaymentsForFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', N'Cash payments for futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities')
INSERT INTO @ET VALUES(114, 0, 0, N'/6/1/2/17/', N'CashReceiptsFromFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', N'Cash receipts from futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities')
INSERT INTO @ET VALUES(115, 0, 0, N'/6/1/2/18/', N'DividendsReceivedClassifiedAsInvestingActivities', N'Dividends received, classified as investing activities')
INSERT INTO @ET VALUES(116, 0, 0, N'/6/1/2/19/', N'InterestPaidClassifiedAsInvestingActivities', N'Interest paid, classified as investing activities')
INSERT INTO @ET VALUES(117, 0, 1, N'/6/1/2/20/', N'InterestReceivedClassifiedAsInvestingActivities', N'Interest received, classified as investing activities')
INSERT INTO @ET VALUES(118, 0, 0, N'/6/1/2/21/', N'IncomeTaxesPaidRefundClassifiedAsInvestingActivities', N'Income taxes paid (refund), classified as investing activities')
INSERT INTO @ET VALUES(119, 0, 1, N'/6/1/2/22/', N'OtherInflowsOutflowsOfCashClassifiedAsInvestingActivities', N'Other inflows (outflows) of cash, classified as investing activities')
INSERT INTO @ET VALUES(120, 1, 1, N'/6/1/3/', N'CashFlowsFromUsedInFinancingActivities', N'Cash flows from (used in) financing activities')
INSERT INTO @ET VALUES(121, 0, 0, N'/6/1/3/1/', N'ProceedsFromChangesInOwnershipInterestsInSubsidiaries', N'Proceeds from changes in ownership interests in subsidiaries that do not result in loss of control')
INSERT INTO @ET VALUES(122, 0, 0, N'/6/1/3/2/', N'PaymentsFromChangesInOwnershipInterestsInSubsidiaries', N'Payments from changes in ownership interests in subsidiaries that do not result in loss of control')
INSERT INTO @ET VALUES(123, 0, 1, N'/6/1/3/3/', N'ProceedsFromIssuingShares', N'Proceeds from issuing shares')
INSERT INTO @ET VALUES(124, 0, 0, N'/6/1/3/4/', N'ProceedsFromIssuingOtherEquityInstruments', N'Proceeds from issuing other equity instruments')
INSERT INTO @ET VALUES(125, 0, 0, N'/6/1/3/5/', N'PaymentsToAcquireOrRedeemEntitysShares', N'Payments to acquire or redeem entity''s shares')
INSERT INTO @ET VALUES(126, 0, 0, N'/6/1/3/6/', N'PaymentsOfOtherEquityInstruments', N'Payments of other equity instruments')
INSERT INTO @ET VALUES(127, 0, 1, N'/6/1/3/7/', N'ProceedsFromBorrowingsClassifiedAsFinancingActivities', N'Proceeds from borrowings, classified as financing activities')
INSERT INTO @ET VALUES(128, 0, 1, N'/6/1/3/8/', N'RepaymentsOfBorrowingsClassifiedAsFinancingActivities', N'Repayments of borrowings, classified as financing activities')
INSERT INTO @ET VALUES(129, 0, 1, N'/6/1/3/9/', N'PaymentsOfLeaseLiabilitiesClassifiedAsFinancingActivities', N'Payments of lease liabilities, classified as financing activities')
INSERT INTO @ET VALUES(130, 0, 0, N'/6/1/3/10/', N'ProceedsFromGovernmentGrantsClassifiedAsFinancingActivities', N'Proceeds from government grants, classified as financing activities')
INSERT INTO @ET VALUES(131, 0, 1, N'/6/1/3/11/', N'DividendsPaidClassifiedAsFinancingActivities', N'Dividends paid, classified as financing activities')
INSERT INTO @ET VALUES(132, 0, 1, N'/6/1/3/12/', N'InterestPaidClassifiedAsFinancingActivities', N'Interest paid, classified as financing activities')
INSERT INTO @ET VALUES(133, 0, 0, N'/6/1/3/13/', N'IncomeTaxesPaidRefundClassifiedAsFinancingActivities', N'Income taxes paid (refund), classified as financing activities')
INSERT INTO @ET VALUES(134, 0, 1, N'/6/1/3/14/', N'OtherInflowsOutflowsOfCashClassifiedAsFinancingActivities', N'Other inflows (outflows) of cash, classified as financing activities')
INSERT INTO @ET VALUES(135, 0, 1, N'/6/1/4/', N'InternalCashTransferExtension', N'Internal cash transfer')
INSERT INTO @ET VALUES(136, 0, 1, N'/6/2/', N'EffectOfExchangeRateChangesOnCashAndCashEquivalents', N'Effect of exchange rate changes on cash and cash equivalents')
INSERT INTO @ET VALUES(137, 1, 1, N'/7/', N'ChangesInEquity', N'Increase (decrease) in equity')
INSERT INTO @ET VALUES(138, 1, 0, N'/7/1/', N'ComprehensiveIncome', N'Comprehensive income')
INSERT INTO @ET VALUES(139, 0, 0, N'/7/1/1/', N'ProfitLoss', N'Profit (loss)')
INSERT INTO @ET VALUES(140, 0, 0, N'/7/1/2/', N'OtherComprehensiveIncome', N'Other comprehensive income')
INSERT INTO @ET VALUES(141, 0, 1, N'/7/2/', N'IssueOfEquity', N'Issue of equity')
INSERT INTO @ET VALUES(142, 0, 1, N'/7/3/', N'DividendsPaid', N'Dividends recognised as distributions to owners')
INSERT INTO @ET VALUES(143, 0, 1, N'/7/4/', N'IncreaseDecreaseThroughOtherContributionsByOwners', N'Increase through other contributions by owners, equity')
INSERT INTO @ET VALUES(144, 0, 0, N'/7/5/', N'IncreaseDecreaseThroughOtherDistributionsToOwners', N'Decrease through other distributions to owners, equity')
INSERT INTO @ET VALUES(145, 0, 0, N'/7/6/', N'IncreaseDecreaseThroughTransfersAndOtherChangesEquity', N'Increase (decrease) through other changes, equity')
INSERT INTO @ET VALUES(146, 0, 0, N'/7/7/', N'IncreaseDecreaseThroughTreasuryShareTransactions', N'Increase (decrease) through treasury share transactions, equity')
INSERT INTO @ET VALUES(147, 0, 0, N'/7/8/', N'IncreaseDecreaseThroughChangesInOwnershipInterestsInSubsidiariesThatDoNotResultInLossOfControl', N'Increase (decrease) through changes in ownership interests in subsidiaries that do not result in loss of control, equity')
INSERT INTO @ET VALUES(148, 0, 0, N'/7/9/', N'IncreaseDecreaseThroughSharebasedPaymentTransactions', N'Increase (decrease) through share-based payment transactions, equity')
INSERT INTO @ET VALUES(149, 0, 0, N'/7/10/', N'AmountRemovedFromReserveOfCashFlowHedgesAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of cash flow hedges and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
INSERT INTO @ET VALUES(150, 0, 0, N'/7/11/', N'AmountRemovedFromReserveOfChangeInValueOfTimeValueOfOptionsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of change in value of time value of options and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
INSERT INTO @ET VALUES(151, 0, 0, N'/7/12/', N'AmountRemovedFromReserveOfChangeInValueOfForwardElementsOfForwardContractsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of change in value of forward elements of forward contracts and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
INSERT INTO @ET VALUES(152, 0, 0, N'/7/13/', N'AmountRemovedFromReserveOfChangeInValueOfForeignCurrencyBasisSpreadsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of change in value of foreign currency basis spreads and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
INSERT INTO @ET VALUES(153, 1, 1, N'/8/', N'ChangesInOtherProvisions', N'Increase (decrease) in other provisions')
INSERT INTO @ET VALUES(154, 0, 1, N'/8/1/', N'AdditionalProvisionsOtherProvisions', N'Additional provisions, other provisions')
INSERT INTO @ET VALUES(155, 0, 0, N'/8/1/1/', N'NewProvisionsOtherProvisions', N'New provisions, other provisions')
INSERT INTO @ET VALUES(156, 0, 0, N'/8/1/2/', N'IncreaseDecreaseInExistingProvisionsOtherProvisions', N'Increase in existing provisions, other provisions')
INSERT INTO @ET VALUES(157, 0, 0, N'/8/2/', N'AcquisitionsThroughBusinessCombinationsOtherProvisions', N'Acquisitions through business combinations, other provisions')
INSERT INTO @ET VALUES(158, 0, 1, N'/8/3/', N'ProvisionUsedOtherProvisions', N'Provision used, other provisions')
INSERT INTO @ET VALUES(159, 0, 0, N'/8/4/', N'UnusedProvisionReversedOtherProvisions', N'Unused provision reversed, other provisions')
INSERT INTO @ET VALUES(160, 0, 0, N'/8/5/', N'IncreaseDecreaseThroughTimeValueOfMoneyAdjustmentOtherProvisions', N'Increase through adjustments arising from passage of time, other provisions')
INSERT INTO @ET VALUES(161, 0, 0, N'/8/6/', N'IncreaseDecreaseThroughChangeInDiscountRateOtherProvisions', N'Increase (decrease) through change in discount rate, other provisions')
INSERT INTO @ET VALUES(162, 0, 0, N'/8/7/', N'IncreaseDecreaseThroughNetExchangeDifferencesOtherProvisions', N'Increase (decrease) through net exchange differences, other provisions')
INSERT INTO @ET VALUES(163, 0, 0, N'/8/8/', N'DecreaseThroughLossOfControlOfSubsidiaryOtherProvisions', N'Decrease through loss of control of subsidiary, other provisions')
INSERT INTO @ET VALUES(164, 0, 0, N'/8/9/', N'IncreaseDecreaseThroughTransfersAndOtherChangesOtherProvisions', N'Increase (decrease) through transfers and other changes, other provisions')
INSERT INTO @ET VALUES(165, 1, 1, N'/9/', N'ExpenseByFunctionExtension', N'Expense, by function')
INSERT INTO @ET VALUES(166, 0, 1, N'/9/1/', N'CostOfSales', N'Cost of sales')
INSERT INTO @ET VALUES(167, 0, 1, N'/9/2/', N'DistributionCosts', N'Distribution costs')
INSERT INTO @ET VALUES(168, 0, 1, N'/9/3/', N'AdministrativeExpense', N'Administrative expenses')
INSERT INTO @ET VALUES(169, 0, 1, N'/9/4/', N'OtherExpenseByFunction', N'Other expense, by function')
INSERT INTO @ET VALUES(170, 1, 1, N'/10/', N'ChangesInInventories', N'Increase (decrease) in inventories')
INSERT INTO @ET VALUES(171, 0, 1, N'/10/1/', N'InventoryPurchaseExtension', N'Inventory purchase')
INSERT INTO @ET VALUES(172, 0, 1, N'/10/2/', N'InventoryProductionExtension', N'Inventory production')
INSERT INTO @ET VALUES(173, 0, 1, N'/10/3/', N'InventorySalesExtension', N'Inventory sales')
INSERT INTO @ET VALUES(174, 0, 1, N'/10/4/', N'InventoryConsumptionExtension', N'Inventory consumption')
INSERT INTO @ET VALUES(175, 0, 1, N'/10/5/', N'InventoryGainLossExtension', N'Inventory Gain (loss)')
INSERT INTO @ET VALUES(176, 0, 1, N'/10/6/', N'InventoryReclassifiedAsPropertyPlantAndEquipmentExtension', N'Inventory reclassified as property, plant and equipment')
INSERT INTO @ET VALUES(177, 0, 1, N'/10/7/', N'PropertyPlantAndEquipmentReclassifiedAsInventoryExtension', N'Fixed asset to inventory conversion')
INSERT INTO @ET VALUES(178, 0, 1, N'/10/8/', N'InternalInventoryTransferExtension', N'Inventory transfer')


-- TODO: we also need to add customer return and supplier return

DECLARE @EntryTypes dbo.EntryTypeList;

INSERT INTO @EntryTypes ([Index], [IsAssignable], [ParentIndex], [Code], [Name])
SELECT [Index], 1-[IsAbstract], (SELECT [Index] FROM @ET WHERE [Node] = ET.[Node].GetAncestor(1)) AS ParentIndex, [Code], [Name]
FROM @ET ET

EXEC [api].[EntryTypes__Save]
	@Entities = @EntryTypes,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Entry Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;									

UPDATE dbo.[EntryTypes] SET IsSystem = 1;
UPDATE ET
SET ET.IsActive = T.IsActive
FROM dbo.[EntryTypes] ET JOIN @ET T ON ET.[Code] = T.[Code];

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