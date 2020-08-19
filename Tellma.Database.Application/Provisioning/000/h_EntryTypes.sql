﻿DECLARE @ET TABLE (
	[Index] INT, [Code] NVARCHAR (50),
	[Node] HIERARCHYID, [Concept] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
)
--Script
INSERT INTO @ET VALUES(0, N'10', N'/1/', N'ChangesInPropertyPlantAndEquipment', N'Increase (decrease) in property, plant and equipment', N'')
INSERT INTO @ET VALUES(1, N'101', N'/1/1/', N'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment', N'Additions other than through business combinations, property, plant and equipment', N'')
INSERT INTO @ET VALUES(2, N'102', N'/1/2/', N'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment', N'Acquisitions through business combinations, property, plant and equipment', N'')
INSERT INTO @ET VALUES(3, N'103', N'/1/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesPropertyPlantAndEquipment', N'Increase (decrease) through net exchange differences, property, plant and equipment', N'')
INSERT INTO @ET VALUES(4, N'104', N'/1/4/', N'DepreciationPropertyPlantAndEquipment', N'Depreciation, property, plant and equipment', N'')
INSERT INTO @ET VALUES(5, N'105', N'/1/5/', N'ImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment', N'Impairment loss recognised in profit or loss, property, plant and equipment', N'')
INSERT INTO @ET VALUES(6, N'106', N'/1/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment', N'Reversal of impairment loss recognised in profit or loss, property, plant and equipment', N'')
INSERT INTO @ET VALUES(7, N'107', N'/1/7/', N'RevaluationIncreaseDecreasePropertyPlantAndEquipment', N'Revaluation increase (decrease), property, plant and equipment', N'')
INSERT INTO @ET VALUES(8, N'108', N'/1/8/', N'ImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment', N'Impairment loss recognised in other comprehensive income, property, plant and equipment', N'')
INSERT INTO @ET VALUES(9, N'109', N'/1/9/', N'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment', N'Reversal of impairment loss recognised in other comprehensive income, property, plant and equipment', N'')
INSERT INTO @ET VALUES(10, N'110', N'/1/10/', N'IncreaseDecreaseThroughTransfersAndOtherChangesPropertyPlantAndEquipmentAbstract', N'Increase (decrease) through transfers and other changes, property, plant and equipment [abstract]', N'')
INSERT INTO @ET VALUES(11, N'1101', N'/1/10/1/', N'IncreaseDecreaseThroughTransfersPropertyPlantAndEquipment', N'Increase (decrease) through transfers, property, plant and equipment', N'')
INSERT INTO @ET VALUES(12, N'1102', N'/1/10/2/', N'IncreaseDecreaseThroughOtherChangesPropertyPlantAndEquipment', N'Increase (decrease) through other changes, property, plant and equipment', N'')
INSERT INTO @ET VALUES(13, N'111', N'/1/11/', N'DisposalsAndRetirementsPropertyPlantAndEquipment', N'Disposals and retirements, property, plant and equipment', N'')
INSERT INTO @ET VALUES(14, N'1111', N'/1/11/1/', N'DisposalsPropertyPlantAndEquipment', N'Disposals, property, plant and equipment', N'')
INSERT INTO @ET VALUES(15, N'1112', N'/1/11/2/', N'RetirementsPropertyPlantAndEquipment', N'Retirements, property, plant and equipment', N'')
INSERT INTO @ET VALUES(16, N'112', N'/1/12/', N'DecreaseThroughClassifiedAsHeldForSalePropertyPlantAndEquipment', N'Decrease through classified as held for sale, property, plant and equipment', N'')
INSERT INTO @ET VALUES(17, N'113', N'/1/13/', N'DecreaseThroughLossOfControlOfSubsidiaryPropertyPlantAndEquipment', N'Decrease through loss of control of subsidiary, property, plant and equipment', N'')
INSERT INTO @ET VALUES(18, N'2', N'/2/', N'ChangesInInvestmentProperty', N'Increase (decrease) in investment property', N'')
INSERT INTO @ET VALUES(19, N'201', N'/2/1/', N'AdditionsOtherThanThroughBusinessCombinationsInvestmentProperty', N'Additions other than through business combinations, investment property', N'')
INSERT INTO @ET VALUES(20, N'2011', N'/2/1/1/', N'AdditionsFromSubsequentExpenditureRecognisedAsAssetInvestmentProperty', N'Additions from subsequent expenditure recognised as asset, investment property', N'')
INSERT INTO @ET VALUES(21, N'2012', N'/2/1/2/', N'AdditionsFromAcquisitionsInvestmentProperty', N'Additions from acquisitions, investment property', N'')
INSERT INTO @ET VALUES(22, N'202', N'/2/2/', N'AcquisitionsThroughBusinessCombinationsInvestmentProperty', N'Acquisitions through business combinations, investment property', N'')
INSERT INTO @ET VALUES(23, N'203', N'/2/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesInvestmentProperty', N'Increase (decrease) through net exchange differences, investment property', N'')
INSERT INTO @ET VALUES(24, N'204', N'/2/4/', N'DepreciationInvestmentProperty', N'Depreciation, investment property', N'')
INSERT INTO @ET VALUES(25, N'205', N'/2/5/', N'ImpairmentLossRecognisedInProfitOrLossInvestmentProperty', N'Impairment loss recognised in profit or loss, investment property', N'')
INSERT INTO @ET VALUES(26, N'206', N'/2/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossInvestmentProperty', N'Reversal of impairment loss recognised in profit or loss, investment property', N'')
INSERT INTO @ET VALUES(27, N'207', N'/2/7/', N'GainsLossesOnFairValueAdjustmentInvestmentProperty', N'Gains (losses) on fair value adjustment, investment property', N'')
INSERT INTO @ET VALUES(28, N'208', N'/2/8/', N'TransferFromToInventoriesAndOwnerOccupiedPropertyInvestmentProperty', N'Transfer from (to) inventories and owner-occupied property, investment property', N'')
INSERT INTO @ET VALUES(29, N'209', N'/2/9/', N'TransferFromInvestmentPropertyUnderConstructionOrDevelopmentInvestmentProperty', N'Transfer from investment property under construction or development, investment property', N'')
INSERT INTO @ET VALUES(30, N'210', N'/2/10/', N'DisposalsInvestmentProperty', N'Disposals, investment property', N'')
INSERT INTO @ET VALUES(31, N'211', N'/2/11/', N'DecreaseThroughClassifiedAsHeldForSaleInvestmentProperty', N'Decrease through classified as held for sale, investment property', N'')
INSERT INTO @ET VALUES(32, N'212', N'/2/12/', N'IncreaseDecreaseThroughOtherChangesInvestmentProperty', N'Increase (decrease) through other changes, investment property', N'')
INSERT INTO @ET VALUES(33, N'3', N'/3/', N'ChangesInGoodwill', N'Increase (decrease) in goodwill', N'')
INSERT INTO @ET VALUES(34, N'31', N'/3/1/', N'SubsequentRecognitionOfDeferredTaxAssetsGoodwill', N'Subsequent recognition of deferred tax assets, goodwill', N'')
INSERT INTO @ET VALUES(35, N'32', N'/3/2/', N'IncreaseDecreaseThroughTransfersAndOtherChangesGoodwill', N'Increase (decrease) through other changes, goodwill', N'')
INSERT INTO @ET VALUES(36, N'33', N'/3/3/', N'AdditionalRecognitionGoodwill', N'Additional recognition, goodwill', N'')
INSERT INTO @ET VALUES(37, N'34', N'/3/4/', N'DecreaseThroughClassifiedAsHeldForSaleGoodwill', N'Decrease through classified as held for sale, goodwill', N'')
INSERT INTO @ET VALUES(38, N'35', N'/3/5/', N'GoodwillDerecognisedWithoutHavingPreviouslyBeenIncludedInDisposalGroupClassifiedAsHeldForSale', N'Goodwill derecognised without having previously been included in disposal group classified as held for sale', N'')
INSERT INTO @ET VALUES(39, N'36', N'/3/6/', N'ImpairmentLossRecognisedInProfitOrLossGoodwill', N'Impairment loss recognised in profit or loss, goodwill', N'')
INSERT INTO @ET VALUES(40, N'37', N'/3/7/', N'IncreaseDecreaseThroughNetExchangeDifferencesGoodwill', N'Increase (decrease) through net exchange differences, goodwill', N'')
INSERT INTO @ET VALUES(41, N'4', N'/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) in intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(42, N'401', N'/4/1/', N'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', N'Additions other than through business combinations, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(43, N'402', N'/4/2/', N'AcquisitionsThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', N'Acquisitions through business combinations, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(44, N'403', N'/4/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through net exchange differences, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(45, N'404', N'/4/4/', N'AmortisationIntangibleAssetsOtherThanGoodwill', N'Amortisation, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(46, N'405', N'/4/5/', N'ImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', N'Impairment loss recognised in profit or loss, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(47, N'406', N'/4/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', N'Reversal of impairment loss recognised in profit or loss, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(48, N'407', N'/4/7/', N'RevaluationIncreaseDecreaseIntangibleAssetsOtherThanGoodwill', N'Revaluation increase (decrease), intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(49, N'408', N'/4/8/', N'ImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', N'Impairment loss recognised in other comprehensive income, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(50, N'409', N'/4/9/', N'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', N'Reversal of impairment loss recognised in other comprehensive income, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(51, N'410', N'/4/10/', N'DecreaseThroughClassifiedAsHeldForSaleIntangibleAssetsOtherThanGoodwill', N'Decrease through classified as held for sale, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(52, N'411', N'/4/11/', N'DecreaseThroughLossOfControlOfSubsidiaryIntangibleAssetsOtherThanGoodwill', N'Decrease through loss of control of subsidiary, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(53, N'412', N'/4/12/', N'DisposalsAndRetirementsIntangibleAssetsOtherThanGoodwill', N'Disposals and retirements, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(54, N'4121', N'/4/12/1/', N'DisposalsIntangibleAssetsOtherThanGoodwill', N'Disposals, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(55, N'4122', N'/4/12/2/', N'RetirementsIntangibleAssetsOtherThanGoodwill', N'Retirements, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(56, N'413', N'/4/13/', N'IncreaseDecreaseThroughTransfersAndOtherChangesIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through transfers and other changes, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(57, N'4131', N'/4/13/1/', N'IncreaseDecreaseThroughTransfersIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through transfers, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(58, N'4132', N'/4/13/2/', N'IncreaseDecreaseThroughOtherChangesIntangibleAssetsOtherThanGoodwill', N'Increase (decrease) through other changes, intangible assets other than goodwill', N'')
INSERT INTO @ET VALUES(59, N'5', N'/5/', N'ChangesInBiologicalAssets', N'Increase (decrease) in biological assets', N'')
INSERT INTO @ET VALUES(60, N'501', N'/5/1/', N'AdditionsOtherThanThroughBusinessCombinationsBiologicalAssets', N'Additions other than through business combinations, biological assets', N'')
INSERT INTO @ET VALUES(61, N'5011', N'/5/1/1/', N'AdditionsFromSubsequentExpenditureRecognisedAsAssetBiologicalAssets', N'Additions from subsequent expenditure recognised as asset, biological assets', N'')
INSERT INTO @ET VALUES(62, N'5012', N'/5/1/2/', N'AdditionsFromPurchasesBiologicalAssets', N'Additions from purchases, biological assets', N'')
INSERT INTO @ET VALUES(63, N'502', N'/5/2/', N'AcquisitionsThroughBusinessCombinationsBiologicalAssets', N'Acquisitions through business combinations, biological assets', N'')
INSERT INTO @ET VALUES(64, N'503', N'/5/3/', N'IncreaseDecreaseThroughNetExchangeDifferencesBiologicalAssets', N'Increase (decrease) through net exchange differences, biological assets', N'')
INSERT INTO @ET VALUES(65, N'504', N'/5/4/', N'DepreciationBiologicalAssets', N'Depreciation, biological assets', N'')
INSERT INTO @ET VALUES(66, N'505', N'/5/5/', N'ImpairmentLossRecognisedInProfitOrLossBiologicalAssets', N'Impairment loss recognised in profit or loss, biological assets', N'')
INSERT INTO @ET VALUES(67, N'506', N'/5/6/', N'ReversalOfImpairmentLossRecognisedInProfitOrLossBiologicalAssets', N'Reversal of impairment loss recognised in profit or loss, biological assets', N'')
INSERT INTO @ET VALUES(68, N'507', N'/5/7/', N'GainsLossesOnFairValueAdjustmentBiologicalAssets', N'Gains (losses) on fair value adjustment, biological assets', N'')
INSERT INTO @ET VALUES(69, N'5071', N'/5/7/1/', N'GainsLossesOnFairValueAdjustmentAttributableToPhysicalChangesBiologicalAssets', N'Gains (losses) on fair value adjustment attributable to physical changes, biological assets', N'')
INSERT INTO @ET VALUES(70, N'5072', N'/5/7/2/', N'GainsLossesOnFairValueAdjustmentAttributableToPriceChangesBiologicalAssets', N'Gains (losses) on fair value adjustment attributable to price changes, biological assets', N'')
INSERT INTO @ET VALUES(71, N'508', N'/5/8/', N'IncreaseDecreaseThroughTransfersAndOtherChangesBiologicalAssets', N'Increase (decrease) through other changes, biological assets', N'')
INSERT INTO @ET VALUES(72, N'509', N'/5/9/', N'DisposalsBiologicalAssets', N'Disposals, biological assets', N'')
INSERT INTO @ET VALUES(73, N'510', N'/5/10/', N'DecreaseDueToHarvestBiologicalAssets', N'Decrease due to harvest, biological assets', N'')
INSERT INTO @ET VALUES(74, N'511', N'/5/11/', N'DecreaseThroughClassifiedAsHeldForSaleBiologicalAssets', N'Decrease through classified as held for sale, biological assets', N'')
INSERT INTO @ET VALUES(75, N'6', N'/6/', N'IncreaseDecreaseInCashAndCashEquivalents', N'Increase (decrease) in cash and cash equivalents', N'')
INSERT INTO @ET VALUES(76, N'61', N'/6/1/', N'IncreaseDecreaseInCashAndCashEquivalentsBeforeEffectOfExchangeRateChanges', N'Increase (decrease) in cash and cash equivalents before effect of exchange rate changes', N'')
INSERT INTO @ET VALUES(77, N'611', N'/6/1/1/', N'CashFlowsFromUsedInOperatingActivities', N'Cash flows from (used in) operating activities', N'')
INSERT INTO @ET VALUES(78, N'6111', N'/6/1/1/1/', N'CashFlowsFromUsedInOperations', N'Cash flows from (used in) operations', N'')
INSERT INTO @ET VALUES(79, N'611101', N'/6/1/1/1/1/', N'ReceiptsFromSalesOfGoodsAndRenderingOfServices', N'Receipts from sales of goods and rendering of services', N'')
INSERT INTO @ET VALUES(80, N'611102', N'/6/1/1/1/2/', N'ReceiptsFromRoyaltiesFeesCommissionsAndOtherRevenue', N'Receipts from royalties, fees, commissions and other revenue', N'')
INSERT INTO @ET VALUES(81, N'611103', N'/6/1/1/1/3/', N'ReceiptsFromContractsHeldForDealingOrTradingPurpose', N'Receipts from contracts held for dealing or trading purposes', N'')
INSERT INTO @ET VALUES(82, N'611104', N'/6/1/1/1/4/', N'ReceiptsFromPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', N'Receipts from premiums and claims, annuities and other policy benefits', N'')
INSERT INTO @ET VALUES(83, N'611105', N'/6/1/1/1/5/', N'ReceiptsFromRentsAndSubsequentSalesOfSuchAssets', N'Receipts from rents and subsequent sales of assets held for rental to others and subsequently held for sale', N'')
INSERT INTO @ET VALUES(84, N'611106', N'/6/1/1/1/6/', N'OtherCashReceiptsFromOperatingActivities', N'Other cash receipts from operating activities', N'')
INSERT INTO @ET VALUES(85, N'611107', N'/6/1/1/1/7/', N'PaymentsToSuppliersForGoodsAndServices', N'Payments to suppliers for goods and services', N'')
INSERT INTO @ET VALUES(86, N'611108', N'/6/1/1/1/8/', N'PaymentsFromContractsHeldForDealingOrTradingPurpose', N'Payments from contracts held for dealing or trading purpose', N'')
INSERT INTO @ET VALUES(87, N'611109', N'/6/1/1/1/9/', N'PaymentsToAndOnBehalfOfEmployees', N'Payments to and on behalf of employees', N'')
INSERT INTO @ET VALUES(88, N'611110', N'/6/1/1/1/10/', N'PaymentsForPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', N'Payments for premiums and claims, annuities and other policy benefits', N'')
INSERT INTO @ET VALUES(89, N'611111', N'/6/1/1/1/11/', N'PaymentsToManufactureOrAcquireAssetsHeldForRentalToOthersAndSubsequentlyHeldForSale', N'Payments to manufacture or acquire assets held for rental to others and subsequently held for sale', N'')
INSERT INTO @ET VALUES(90, N'611112', N'/6/1/1/1/12/', N'OtherCashPaymentsFromOperatingActivities', N'Other cash payments from operating activities', N'')
INSERT INTO @ET VALUES(91, N'6112', N'/6/1/1/2/', N'DividendsPaidClassifiedAsOperatingActivities', N'Dividends paid, classified as operating activities', N'')
INSERT INTO @ET VALUES(92, N'6113', N'/6/1/1/3/', N'DividendsReceivedClassifiedAsOperatingActivities', N'Dividends received, classified as operating activities', N'')
INSERT INTO @ET VALUES(93, N'6114', N'/6/1/1/4/', N'InterestPaidClassifiedAsOperatingActivities', N'Interest paid, classified as operating activities', N'')
INSERT INTO @ET VALUES(94, N'6115', N'/6/1/1/5/', N'InterestReceivedClassifiedAsOperatingActivities', N'Interest received, classified as operating activities', N'')
INSERT INTO @ET VALUES(95, N'6116', N'/6/1/1/6/', N'IncomeTaxesPaidRefundClassifiedAsOperatingActivities', N'Income taxes paid (refund), classified as operating activities', N'')
INSERT INTO @ET VALUES(96, N'6117', N'/6/1/1/7/', N'OtherInflowsOutflowsOfCashClassifiedAsOperatingActivities', N'Other inflows (outflows) of cash, classified as operating activities', N'')
INSERT INTO @ET VALUES(97, N'612', N'/6/1/2/', N'CashFlowsFromUsedInInvestingActivities', N'Cash flows from (used in) investing activities', N'')
INSERT INTO @ET VALUES(98, N'61201', N'/6/1/2/1/', N'CashFlowsFromLosingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', N'Cash flows from losing control of subsidiaries or other businesses, classified as investing activities', N'')
INSERT INTO @ET VALUES(99, N'61202', N'/6/1/2/2/', N'CashFlowsUsedInObtainingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', N'Cash flows used in obtaining control of subsidiaries or other businesses, classified as investing activities', N'')
INSERT INTO @ET VALUES(100, N'61203', N'/6/1/2/3/', N'OtherCashReceiptsFromSalesOfEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', N'Other cash receipts from sales of equity or debt instruments of other entities, classified as investing activities', N'')
INSERT INTO @ET VALUES(101, N'61204', N'/6/1/2/4/', N'OtherCashPaymentsToAcquireEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', N'Other cash payments to acquire equity or debt instruments of other entities, classified as investing activities', N'')
INSERT INTO @ET VALUES(102, N'61205', N'/6/1/2/5/', N'OtherCashReceiptsFromSalesOfInterestsInJointVenturesClassifiedAsInvestingActivities', N'Other cash receipts from sales of interests in joint ventures, classified as investing activities', N'')
INSERT INTO @ET VALUES(103, N'61206', N'/6/1/2/6/', N'OtherCashPaymentsToAcquireInterestsInJointVenturesClassifiedAsInvestingActivities', N'Other cash payments to acquire interests in joint ventures, classified as investing activities', N'')
INSERT INTO @ET VALUES(104, N'61207', N'/6/1/2/7/', N'ProceedsFromSalesOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', N'Proceeds from sales of property, plant and equipment, classified as investing activities', N'')
INSERT INTO @ET VALUES(105, N'61208', N'/6/1/2/8/', N'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', N'Purchase of property, plant and equipment, classified as investing activities', N'')
INSERT INTO @ET VALUES(106, N'61209', N'/6/1/2/9/', N'ProceedsFromSalesOfIntangibleAssetsClassifiedAsInvestingActivities', N'Proceeds from sales of intangible assets, classified as investing activities', N'')
INSERT INTO @ET VALUES(107, N'61210', N'/6/1/2/10/', N'PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities', N'Purchase of intangible assets, classified as investing activities', N'')
INSERT INTO @ET VALUES(108, N'61211', N'/6/1/2/11/', N'ProceedsFromOtherLongtermAssetsClassifiedAsInvestingActivities', N'Proceeds from sales of other long-term assets, classified as investing activities', N'')
INSERT INTO @ET VALUES(109, N'61212', N'/6/1/2/12/', N'PurchaseOfOtherLongtermAssetsClassifiedAsInvestingActivities', N'Purchase of other long-term assets, classified as investing activities', N'')
INSERT INTO @ET VALUES(110, N'61213', N'/6/1/2/13/', N'ProceedsFromGovernmentGrantsClassifiedAsInvestingActivities', N'Proceeds from government grants, classified as investing activities', N'')
INSERT INTO @ET VALUES(111, N'61214', N'/6/1/2/14/', N'CashAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', N'Cash advances and loans made to other parties, classified as investing activities', N'')
INSERT INTO @ET VALUES(112, N'61215', N'/6/1/2/15/', N'CashReceiptsFromRepaymentOfAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', N'Cash receipts from repayment of advances and loans made to other parties, classified as investing activities', N'')
INSERT INTO @ET VALUES(113, N'61216', N'/6/1/2/16/', N'CashPaymentsForFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', N'Cash payments for futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities', N'')
INSERT INTO @ET VALUES(114, N'61217', N'/6/1/2/17/', N'CashReceiptsFromFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', N'Cash receipts from futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities', N'')
INSERT INTO @ET VALUES(115, N'61218', N'/6/1/2/18/', N'DividendsReceivedClassifiedAsInvestingActivities', N'Dividends received, classified as investing activities', N'')
INSERT INTO @ET VALUES(116, N'61219', N'/6/1/2/19/', N'InterestPaidClassifiedAsInvestingActivities', N'Interest paid, classified as investing activities', N'')
INSERT INTO @ET VALUES(117, N'61220', N'/6/1/2/20/', N'InterestReceivedClassifiedAsInvestingActivities', N'Interest received, classified as investing activities', N'')
INSERT INTO @ET VALUES(118, N'61221', N'/6/1/2/21/', N'IncomeTaxesPaidRefundClassifiedAsInvestingActivities', N'Income taxes paid (refund), classified as investing activities', N'')
INSERT INTO @ET VALUES(119, N'61222', N'/6/1/2/22/', N'OtherInflowsOutflowsOfCashClassifiedAsInvestingActivities', N'Other inflows (outflows) of cash, classified as investing activities', N'')
INSERT INTO @ET VALUES(120, N'613', N'/6/1/3/', N'CashFlowsFromUsedInFinancingActivities', N'Cash flows from (used in) financing activities', N'')
INSERT INTO @ET VALUES(121, N'61301', N'/6/1/3/1/', N'ProceedsFromChangesInOwnershipInterestsInSubsidiaries', N'Proceeds from changes in ownership interests in subsidiaries that do not result in loss of control', N'')
INSERT INTO @ET VALUES(122, N'61302', N'/6/1/3/2/', N'PaymentsFromChangesInOwnershipInterestsInSubsidiaries', N'Payments from changes in ownership interests in subsidiaries that do not result in loss of control', N'')
INSERT INTO @ET VALUES(123, N'61303', N'/6/1/3/3/', N'ProceedsFromIssuingShares', N'Proceeds from issuing shares', N'')
INSERT INTO @ET VALUES(124, N'61304', N'/6/1/3/4/', N'ProceedsFromIssuingOtherEquityInstruments', N'Proceeds from issuing other equity instruments', N'')
INSERT INTO @ET VALUES(125, N'61305', N'/6/1/3/5/', N'PaymentsToAcquireOrRedeemEntitysShares', N'Payments to acquire or redeem entity''s shares', N'')
INSERT INTO @ET VALUES(126, N'61306', N'/6/1/3/6/', N'PaymentsOfOtherEquityInstruments', N'Payments of other equity instruments', N'')
INSERT INTO @ET VALUES(127, N'61307', N'/6/1/3/7/', N'ProceedsFromBorrowingsClassifiedAsFinancingActivities', N'Proceeds from borrowings, classified as financing activities', N'')
INSERT INTO @ET VALUES(128, N'61308', N'/6/1/3/8/', N'RepaymentsOfBorrowingsClassifiedAsFinancingActivities', N'Repayments of borrowings, classified as financing activities', N'')
INSERT INTO @ET VALUES(129, N'61309', N'/6/1/3/9/', N'PaymentsOfLeaseLiabilitiesClassifiedAsFinancingActivities', N'Payments of lease liabilities, classified as financing activities', N'')
INSERT INTO @ET VALUES(130, N'61310', N'/6/1/3/10/', N'ProceedsFromGovernmentGrantsClassifiedAsFinancingActivities', N'Proceeds from government grants, classified as financing activities', N'')
INSERT INTO @ET VALUES(131, N'61311', N'/6/1/3/11/', N'DividendsPaidClassifiedAsFinancingActivities', N'Dividends paid, classified as financing activities', N'')
INSERT INTO @ET VALUES(132, N'61312', N'/6/1/3/12/', N'InterestPaidClassifiedAsFinancingActivities', N'Interest paid, classified as financing activities', N'')
INSERT INTO @ET VALUES(133, N'61313', N'/6/1/3/13/', N'IncomeTaxesPaidRefundClassifiedAsFinancingActivities', N'Income taxes paid (refund), classified as financing activities', N'')
INSERT INTO @ET VALUES(134, N'61314', N'/6/1/3/14/', N'OtherInflowsOutflowsOfCashClassifiedAsFinancingActivities', N'Other inflows (outflows) of cash, classified as financing activities', N'')
INSERT INTO @ET VALUES(135, N'614', N'/6/1/4/', N'InternalCashTransferExtension', N'Internal cash transfer', N'')
INSERT INTO @ET VALUES(136, N'62', N'/6/2/', N'EffectOfExchangeRateChangesOnCashAndCashEquivalents', N'Effect of exchange rate changes on cash and cash equivalents', N'')
INSERT INTO @ET VALUES(137, N'7', N'/7/', N'ChangesInEquity', N'Increase (decrease) in equity', N'')
INSERT INTO @ET VALUES(141, N'702', N'/7/2/', N'IssueOfEquity', N'Issue of equity', N'')
INSERT INTO @ET VALUES(142, N'703', N'/7/3/', N'DividendsPaid', N'Dividends recognised as distributions to owners', N'')
INSERT INTO @ET VALUES(143, N'704', N'/7/4/', N'IncreaseDecreaseThroughOtherContributionsByOwners', N'Increase through other contributions by owners, equity', N'')
INSERT INTO @ET VALUES(144, N'705', N'/7/5/', N'IncreaseDecreaseThroughOtherDistributionsToOwners', N'Decrease through other distributions to owners, equity', N'')
INSERT INTO @ET VALUES(145, N'706', N'/7/6/', N'IncreaseDecreaseThroughTransfersAndOtherChangesEquity', N'Increase (decrease) through other changes, equity', N'')
INSERT INTO @ET VALUES(146, N'707', N'/7/7/', N'IncreaseDecreaseThroughTreasuryShareTransactions', N'Increase (decrease) through treasury share transactions, equity', N'')
INSERT INTO @ET VALUES(147, N'708', N'/7/8/', N'IncreaseDecreaseThroughChangesInOwnershipInterestsInSubsidiariesThatDoNotResultInLossOfControl', N'Increase (decrease) through changes in ownership interests in subsidiaries that do not result in loss of control, equity', N'')
INSERT INTO @ET VALUES(148, N'709', N'/7/9/', N'IncreaseDecreaseThroughSharebasedPaymentTransactions', N'Increase (decrease) through share-based payment transactions, equity', N'')
INSERT INTO @ET VALUES(149, N'710', N'/7/10/', N'AmountRemovedFromReserveOfCashFlowHedgesAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of cash flow hedges and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied', N'')
INSERT INTO @ET VALUES(150, N'711', N'/7/11/', N'AmountRemovedFromReserveOfChangeInValueOfTimeValueOfOptionsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of change in value of time value of options and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied', N'')
INSERT INTO @ET VALUES(151, N'712', N'/7/12/', N'AmountRemovedFromReserveOfChangeInValueOfForwardElementsOfForwardContractsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of change in value of forward elements of forward contracts and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied', N'')
INSERT INTO @ET VALUES(152, N'713', N'/7/13/', N'AmountRemovedFromReserveOfChangeInValueOfForeignCurrencyBasisSpreadsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', N'Amount removed from reserve of change in value of foreign currency basis spreads and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied', N'')
INSERT INTO @ET VALUES(153, N'8', N'/8/', N'ChangesInOtherProvisions', N'Increase (decrease) in other provisions', N'')
INSERT INTO @ET VALUES(154, N'81', N'/8/1/', N'AdditionalProvisionsOtherProvisions', N'Additional provisions, other provisions', N'')
INSERT INTO @ET VALUES(155, N'811', N'/8/1/1/', N'NewProvisionsOtherProvisions', N'New provisions, other provisions', N'')
INSERT INTO @ET VALUES(156, N'812', N'/8/1/2/', N'IncreaseDecreaseInExistingProvisionsOtherProvisions', N'Increase in existing provisions, other provisions', N'')
INSERT INTO @ET VALUES(157, N'82', N'/8/2/', N'AcquisitionsThroughBusinessCombinationsOtherProvisions', N'Acquisitions through business combinations, other provisions', N'')
INSERT INTO @ET VALUES(158, N'83', N'/8/3/', N'ProvisionUsedOtherProvisions', N'Provision used, other provisions', N'')
INSERT INTO @ET VALUES(159, N'84', N'/8/4/', N'UnusedProvisionReversedOtherProvisions', N'Unused provision reversed, other provisions', N'')
INSERT INTO @ET VALUES(160, N'85', N'/8/5/', N'IncreaseDecreaseThroughTimeValueOfMoneyAdjustmentOtherProvisions', N'Increase through adjustments arising from passage of time, other provisions', N'')
INSERT INTO @ET VALUES(161, N'86', N'/8/6/', N'IncreaseDecreaseThroughChangeInDiscountRateOtherProvisions', N'Increase (decrease) through change in discount rate, other provisions', N'')
INSERT INTO @ET VALUES(162, N'87', N'/8/7/', N'IncreaseDecreaseThroughNetExchangeDifferencesOtherProvisions', N'Increase (decrease) through net exchange differences, other provisions', N'')
INSERT INTO @ET VALUES(163, N'88', N'/8/8/', N'DecreaseThroughLossOfControlOfSubsidiaryOtherProvisions', N'Decrease through loss of control of subsidiary, other provisions', N'')
INSERT INTO @ET VALUES(164, N'89', N'/8/9/', N'IncreaseDecreaseThroughTransfersAndOtherChangesOtherProvisions', N'Increase (decrease) through transfers and other changes, other provisions', N'')
INSERT INTO @ET VALUES(172, N'9', N'/9/', N'ChangesInInventories', N'Increase (decrease) in inventories', N'')
INSERT INTO @ET VALUES(173, N'901', N'/9/1/', N'ReceiptsReturnsThroughPurchaseExtension', N'Receipts (returns) through purchase', N'')
INSERT INTO @ET VALUES(174, N'902', N'/9/2/', N'IncreaseDecreaseThroughProductionExtension', N'Increase (decrease) through production', N'')
INSERT INTO @ET VALUES(175, N'903', N'/9/3/', N'IncreaseDecreaseThroughMaintenanceExtension', N'Increase (decrease) through maintenance', N'')
INSERT INTO @ET VALUES(176, N'904', N'/9/4/', N'IncreaseDecreaseThrougConsumptionExtension', N'Increase (decrease) through consumption', N'')
INSERT INTO @ET VALUES(177, N'905', N'/9/5/', N'ReturnsIssuesThroughSaleExtension', N'Returns (Issues) through sales ', N'')
INSERT INTO @ET VALUES(178, N'906', N'/9/6/', N'PropertyPlantAndEquipmentClassifiedDeclassifiedAsInventoryExtension', N'Property, plant and equipment classified (reclassified) as inventory', N'')
INSERT INTO @ET VALUES(179, N'907', N'/9/7/', N'InventoryOverageShortageExtension', N'Inventory overage (shortage)', N'')
INSERT INTO @ET VALUES(180, N'908', N'/9/8/', N'InventoryWritedown2011', N'Inventory write-down', N'The amount of expense recognised related to the write-down of inventories to net realisable value. [Refer: Inventories]')
INSERT INTO @ET VALUES(181, N'909', N'/9/9/', N'ReversalOfInventoryWritedown', N'Reversal of inventory write-down', N'The amount recognised as a reduction in the amount of inventories recognised as an expense due to the reversal of any write-down of inventories resulting from an increase in net realisable value. [Refer: Inventories; Inventory write-down]')
INSERT INTO @ET VALUES(182, N'910', N'/9/10/', N'InternalInventoryTransferExtension', N'Inventory transfer', N'')

INSERT INTO @EntryTypes ([Index], [Code], [Concept], [Name], [ParentIndex], [Description])
SELECT ET.[Index], ET.[Code], ET.[Concept], ET.[Name], (SELECT [Index] FROM @ET WHERE [Node] = ET.[Node].GetAncestor(1)) AS ParentIndex, [Description]
FROM @ET ET;
UPDATE @EntryTypes SET IsAssignable = 1
WHERE [Index] NOT IN (SELECT [ParentIndex] FROM @EntryTypes WHERE [ParentIndex] IS NOT NULL)
UPDATE @EntryTypes SET IsAssignable = 0
WHERE [Index] IN (SELECT [ParentIndex] FROM @EntryTypes WHERE [ParentIndex] IS NOT NULL)

EXEC [api].[EntryTypes__Save]
	@Entities = @EntryTypes,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Entry Types: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;									

--UPDATE dbo.[EntryTypes] SET IsSystem = 1;
--UPDATE ET
--SET ET.IsActive = T.IsActive
--FROM dbo.[EntryTypes] ET JOIN @ET T ON ET.[Code] = T.[Code];

--UPDATE DB
--SET DB.[Node] = FE.[Node]
--FROM dbo.[EntryTypes] DB JOIN @ET FE ON DB.[Code] = FE.[Code]

--Declarations
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