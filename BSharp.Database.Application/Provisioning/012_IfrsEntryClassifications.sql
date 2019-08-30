DECLARE @IfrsEntryClassifications AS TABLE (
	[Id]						NVARCHAR (255), -- Ifrs Concept
	[Node]						HIERARCHYID,
	[IfrsType]					NVARCHAR (255),
	[IsActive]					BIT					NOT NULL DEFAULT 1,
	[Label]						NVARCHAR (1024)		NOT NULL,
	[ForDebit]					BIT					NOT NULL DEFAULT 1,
	[ForCredit]					BIT					NOT NULL DEFAULT 1
	PRIMARY KEY NONCLUSTERED ([Id])
);

INSERT INTO @IfrsEntryClassifications([IfrsType], IsActive, [ForDebit], [ForCredit], [Node], [Id], [Label]) VALUES
('Regulatory', 1, 1, 1, '/1/', 'ChangesInPropertyPlantAndEquipment', 'Increase (decrease) in property, plant and equipment')
,('Regulatory', 1, 1, 0, '/1/1/', 'AdditionsOtherThanThroughBusinessCombinationsPropertyPlantAndEquipment', 'Additions other than through business combinations, property, plant and equipment')
,('Regulatory', 1, 1, 0, '/1/2/', 'AcquisitionsThroughBusinessCombinationsPropertyPlantAndEquipment', 'Acquisitions through business combinations, property, plant and equipment')
,('Regulatory', 1, 1, 1, '/1/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesPropertyPlantAndEquipment', 'Increase (decrease) through net exchange differences, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/4/', 'DepreciationPropertyPlantAndEquipment', 'Depreciation, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/5/', 'ImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment', 'Impairment loss recognised in profit or loss, property, plant and equipment')
,('Regulatory', 1, 1, 0, '/1/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossPropertyPlantAndEquipment', 'Reversal of impairment loss recognised in profit or loss, property, plant and equipment')
,('Regulatory', 1, 1, 1, '/1/7/', 'RevaluationIncreaseDecreasePropertyPlantAndEquipment', 'Revaluation increase (decrease), property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/8/', 'ImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment', 'Impairment loss recognised in other comprehensive income, property, plant and equipment')
,('Regulatory', 1, 1, 0, '/1/9/', 'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomePropertyPlantAndEquipment', 'Reversal of impairment loss recognised in other comprehensive income, property, plant and equipment')
,('Regulatory', 1, 1, 1, '/1/10/', 'IncreaseDecreaseThroughTransfersAndOtherChangesPropertyPlantAndEquipment', 'Increase (decrease) through transfers and other changes, property, plan')
,('Regulatory', 1, 1, 1, '/1/10/1/', 'IncreaseDecreaseThroughTransfersPropertyPlantAndEquipment', 'Increase (decrease) through transfers, property, plant and equipment')
,('Regulatory', 1, 1, 1, '/1/10/2/', 'IncreaseDecreaseThroughOtherChangesPropertyPlantAndEquipment', 'Increase (decrease) through other changes, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/11/', 'DisposalsAndRetirementsPropertyPlantAndEquipment', 'Disposals and retirements, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/11/1/', 'DisposalsPropertyPlantAndEquipment', 'Disposals, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/11/2/', 'RetirementsPropertyPlantAndEquipment', 'Retirements, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/12/', 'DecreaseThroughClassifiedAsHeldForSalePropertyPlantAndEquipment', 'Decrease through classified as held for sale, property, plant and equipment')
,('Regulatory', 1, 0, 1, '/1/13/', 'DecreaseThroughLossOfControlOfSubsidiaryPropertyPlantAndEquipment', 'Decrease through loss of control of subsidiary, property, plant and equipment')
,('Regulatory', 1, 1, 1, '/2/', 'ChangesInInvestmentProperty', 'Increase (decrease) in investment property')
,('Regulatory', 1, 1, 0, '/2/1/', 'AdditionsOtherThanThroughBusinessCombinationsInvestmentProperty', 'Additions other than through business combinations, investment property')
,('Regulatory', 1, 1, 0, '/2/1/1/', 'AdditionsFromSubsequentExpenditureRecognisedAsAssetInvestmentProperty', 'Additions from subsequent expenditure recognised as asset, investment property')
,('Regulatory', 1, 1, 0, '/2/1/2/', 'AdditionsFromAcquisitionsInvestmentProperty', 'Additions from acquisitions, investment property')
,('Regulatory', 1, 1, 0, '/2/2/', 'AcquisitionsThroughBusinessCombinationsInvestmentProperty', 'Acquisitions through business combinations, investment property')
,('Regulatory', 1, 1, 1, '/2/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesInvestmentProperty', 'Increase (decrease) through net exchange differences, investment property')
,('Regulatory', 1, 0, 1, '/2/4/', 'DepreciationInvestmentProperty', 'Depreciation, investment property')
,('Regulatory', 1, 0, 1, '/2/5/', 'ImpairmentLossRecognisedInProfitOrLossInvestmentProperty', 'Impairment loss recognised in profit or loss, investment property')
,('Regulatory', 1, 0, 1, '/2/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossInvestmentProperty', 'Reversal of impairment loss recognised in profit or loss, investment property')
,('Regulatory', 1, 1, 1, '/2/7/', 'GainsLossesOnFairValueAdjustmentInvestmentProperty', 'Gains (losses) on fair value adjustment, investment property')
,('Regulatory', 1, 1, 0, '/2/8/', 'TransferFromToInventoriesAndOwnerOccupiedPropertyInvestmentProperty', 'Transfer from (to) inventories and owner-occupied property, investment property')
,('Regulatory', 1, 1, 0, '/2/9/', 'TransferFromInvestmentPropertyUnderConstructionOrDevelopmentInvestmentProperty', 'Transfer from investment property under construction or development, investment property')
,('Regulatory', 1, 0, 1, '/2/10/', 'DisposalsInvestmentProperty', 'Disposals, investment property')
,('Regulatory', 1, 0, 1, '/2/11/', 'DecreaseThroughClassifiedAsHeldForSaleInvestmentProperty', 'Decrease through classified as held for sale, investment property')
,('Regulatory', 1, 1, 1, '/2/12/', 'IncreaseDecreaseThroughOtherChangesInvestmentProperty', 'Increase (decrease) through other changes, investment property')
,('Regulatory', 1, 1, 1, '/3/', 'ChangesInGoodwill', 'Increase (decrease) in goodwill')
,('Regulatory', 1, 1, 0, '/3/1/', 'AdditionalRecognitionGoodwill', 'Additional recognition, goodwill')
,('Regulatory', 0, 1, 0, '/3/2/', 'SubsequentRecognitionOfDeferredTaxAssetsGoodwill', 'Subsequent recognition of deferred tax assets, goodwill')
,('Regulatory', 0, 0, 1, '/3/3/', 'DecreaseThroughClassifiedAsHeldForSaleGoodwill', 'Decrease through classified as held for sale, goodwill')
,('Regulatory', 0, 1, 1, '/3/4/', 'GoodwillDerecognisedWithoutHavingPreviouslyBeenIncludedInDisposalGroupClassifiedAsHeldForSale', 'Goodwill derecognised without having previously been included in disposal group classified as held for sale')
,('Regulatory', 0, 1, 0, '/3/5/', 'ImpairmentLossRecognisedInProfitOrLossGoodwill', 'Impairment loss recognised in profit or loss, goodwill')
,('Regulatory', 0, 1, 1, '/3/6/', 'IncreaseDecreaseThroughNetExchangeDifferencesGoodwill', 'Increase (decrease) through net exchange differences, goodwill')
,('Regulatory', 0, 1, 1, '/3/7/', 'IncreaseDecreaseThroughTransfersAndOtherChangesGoodwill', 'Increase (decrease) through other changes, goodwill')
,('Regulatory', 1, 1, 1, '/4/', 'ChangesInIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) in intangible assets other than goodwill')
,('Regulatory', 1, 1, 0, '/4/1/', 'AdditionsOtherThanThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', 'Additions other than through business combinations, intangible assets other than goodwill')
,('Regulatory', 1, 1, 0, '/4/2/', 'AcquisitionsThroughBusinessCombinationsIntangibleAssetsOtherThanGoodwill', 'Acquisitions through business combinations, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through net exchange differences, intangible assets other than goodwill')
,('Regulatory', 1, 0, 1, '/4/4/', 'AmortisationIntangibleAssetsOtherThanGoodwill', 'Amortisation, intangible assets other than goodwill')
,('Regulatory', 1, 0, 1, '/4/5/', 'ImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', 'Impairment loss recognised in profit or loss, intangible assets other than goodwill')
,('Regulatory', 1, 1, 0, '/4/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossIntangibleAssetsOtherThanGoodwill', 'Reversal of impairment loss recognised in profit or loss, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/7/', 'RevaluationIncreaseDecreaseIntangibleAssetsOtherThanGoodwill', 'Revaluation increase (decrease), intangible assets other than goodwill')
,('Regulatory', 0, 0, 1, '/4/8/', 'ImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', 'Impairment loss recognised in other comprehensive income, intangible assets other than goodwill')
,('Regulatory', 0, 1, 0, '/4/9/', 'ReversalOfImpairmentLossRecognisedInOtherComprehensiveIncomeIntangibleAssetsOtherThanGoodwill', 'Reversal of impairment loss recognised in other comprehensive income, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/10/', 'DecreaseThroughClassifiedAsHeldForSaleIntangibleAssetsOtherThanGoodwill', 'Decrease through classified as held for sale, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/11/', 'DecreaseThroughLossOfControlOfSubsidiaryIntangibleAssetsOtherThanGoodwill', 'Decrease through loss of control of subsidiary, intangible assets other than goodwill')
,('Regulatory', 1, 0, 1, '/4/12/', 'DisposalsAndRetirementsIntangibleAssetsOtherThanGoodwill', 'Disposals and retirements, intangible assets other than goodwill')
,('Regulatory', 1, 0, 1, '/4/12/1/', 'DisposalsIntangibleAssetsOtherThanGoodwill', 'Disposals, intangible assets other than goodwill')
,('Regulatory', 1, 0, 1, '/4/12/2/', 'RetirementsIntangibleAssetsOtherThanGoodwill', 'Retirements, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/13/', 'IncreaseDecreaseThroughTransfersAndOtherChangesIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through transfers and other changes, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/13/1/', 'IncreaseDecreaseThroughTransfersIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through transfers, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/4/13/2/', 'IncreaseDecreaseThroughOtherChangesIntangibleAssetsOtherThanGoodwill', 'Increase (decrease) through other changes, intangible assets other than goodwill')
,('Regulatory', 1, 1, 1, '/5/', 'ChangesInBiologicalAssets', 'Increase (decrease) in biological assets')
,('Regulatory', 1, 1, 0, '/5/1/', 'AdditionsOtherThanThroughBusinessCombinationsBiologicalAssets', 'Additions other than through business combinations, biological assets')
,('Regulatory', 1, 1, 0, '/5/1/1/', 'AdditionsFromSubsequentExpenditureRecognisedAsAssetBiologicalAssets', 'Additions from subsequent expenditure recognised as asset, biological assets')
,('Regulatory', 1, 1, 0, '/5/1/2/', 'AdditionsFromPurchasesBiologicalAssets', 'Additions from purchases, biological assets')
,('Regulatory', 1, 1, 0, '/5/2/', 'AcquisitionsThroughBusinessCombinationsBiologicalAssets', 'Acquisitions through business combinations, biological assets')
,('Regulatory', 1, 1, 1, '/5/3/', 'IncreaseDecreaseThroughNetExchangeDifferencesBiologicalAssets', 'Increase (decrease) through net exchange differences, biological assets')
,('Regulatory', 1, 0, 1, '/5/4/', 'DepreciationBiologicalAssets', 'Depreciation, biological assets')
,('Regulatory', 1, 0, 1, '/5/5/', 'ImpairmentLossRecognisedInProfitOrLossBiologicalAssets', 'Impairment loss recognised in profit or loss, biological assets')
,('Regulatory', 1, 1, 0, '/5/6/', 'ReversalOfImpairmentLossRecognisedInProfitOrLossBiologicalAssets', 'Reversal of impairment loss recognised in profit or loss, biological assets')
,('Regulatory', 1, 1, 1, '/5/7/', 'GainsLossesOnFairValueAdjustmentBiologicalAssets', 'Gains (losses) on fair value adjustment, biological assets')
,('Regulatory', 1, 1, 1, '/5/7/1/', 'GainsLossesOnFairValueAdjustmentAttributableToPhysicalChangesBiologicalAssets', 'Gains (losses) on fair value adjustment attributable to physical changes, biological assets')
,('Regulatory', 1, 1, 1, '/5/7/2/', 'GainsLossesOnFairValueAdjustmentAttributableToPriceChangesBiologicalAssets', 'Gains (losses) on fair value adjustment attributable to price changes, biological assets')
,('Regulatory', 1, 1, 1, '/5/8/', 'IncreaseDecreaseThroughTransfersAndOtherChangesBiologicalAssets', 'Increase (decrease) through other changes, biological assets')
,('Regulatory', 1, 0, 1, '/5/9/', 'DisposalsBiologicalAssets', 'Disposals, biological assets')
,('Regulatory', 1, 0, 1, '/5/10/', 'DecreaseDueToHarvestBiologicalAssets', 'Decrease due to harvest, biological assets')
,('Regulatory', 1, 0, 1, '/5/11/', 'DecreaseThroughClassifiedAsHeldForSaleBiologicalAssets', 'Decrease through classified as held for sale, biological assets')
,('Regulatory', 1, 1, 1, '/6/', 'IncreaseDecreaseInCashAndCashEquivalents', 'Increase (decrease) in cash and cash equivalents before effect of exchange rate changes')
,('Regulatory', 1, 1, 1, '/6/1/', 'IncreaseDecreaseInCashAndCashEquivalentsBeforeEffectOfExchangeRateChanges', 'Increase (decrease) in cash and cash equivalents before effect of exchange rate changes')
,('Regulatory', 1, 1, 1, '/6/1/1/', 'CashFlowsFromUsedInOperatingActivities', 'Cash flows from (used in) operating activities')
,('Regulatory', 1, 1, 1, '/6/1/1/1/', 'CashFlowsFromUsedInOperations', 'Cash flows from (used in) operations')
,('Regulatory', 1, 1, 0, '/6/1/1/1/1/', 'ReceiptsFromSalesOfGoodsAndRenderingOfServices', 'Receipts from sales of goods and rendering of services')
,('Regulatory', 1, 1, 0, '/6/1/1/1/2/', 'ReceiptsFromRoyaltiesFeesCommissionsAndOtherRevenue', 'Receipts from royalties, fees, commissions and other revenue')
,('Regulatory', 1, 1, 0, '/6/1/1/1/3/', 'ReceiptsFromContractsHeldForDealingOrTradingPurpose', 'Receipts from contracts held for dealing or trading purposes')
,('Regulatory', 1, 1, 0, '/6/1/1/1/4/', 'ReceiptsFromPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', 'Receipts from premiums and claims, annuities and other policy benefits')
,('Regulatory', 1, 1, 0, '/6/1/1/1/5/', 'ReceiptsFromRentsAndSubsequentSalesOfSuchAssets', 'Receipts from rents and subsequent sales of assets held for rental to others and subsequently held for sale')
,('Regulatory', 1, 1, 0, '/6/1/1/1/6/', 'OtherCashReceiptsFromOperatingActivities', 'Other cash receipts from operating activities')
,('Regulatory', 1, 0, 1, '/6/1/1/1/7/', 'PaymentsToSuppliersForGoodsAndServices', 'Payments to suppliers for goods and services')
,('Regulatory', 1, 1, 0, '/6/1/1/1/8/', 'PaymentsFromContractsHeldForDealingOrTradingPurpose', 'Payments from contracts held for dealing or trading purpose')
,('Regulatory', 1, 0, 1, '/6/1/1/1/9/', 'PaymentsToAndOnBehalfOfEmployees', 'Payments to and on behalf of employees')
,('Regulatory', 1, 0, 1, '/6/1/1/1/10/', 'PaymentsForPremiumsAndClaimsAnnuitiesAndOtherPolicyBenefits', 'Payments for premiums and claims, annuities and other policy benefits')
,('Regulatory', 1, 0, 1, '/6/1/1/1/11/', 'PaymentsToManufactureOrAcquireAssetsHeldForRentalToOthersAndSubsequentlyHeldForSale', 'Payments to manufacture or acquire assets held for rental to others and subsequently held for sale')
,('Regulatory', 1, 0, 1, '/6/1/1/1/12/', 'OtherCashPaymentsFromOperatingActivities', 'Other cash payments from operating activities')
,('Regulatory', 1, 0, 1, '/6/1/1/2/', 'DividendsPaidClassifiedAsOperatingActivities', 'Dividends paid, classified as operating activities')
,('Regulatory', 1, 1, 0, '/6/1/1/3/', 'DividendsReceivedClassifiedAsOperatingActivities', 'Dividends received, classified as operating activities')
,('Regulatory', 1, 0, 1, '/6/1/1/4/', 'InterestPaidClassifiedAsOperatingActivities', 'Interest paid, classified as operating activities')
,('Regulatory', 1, 1, 0, '/6/1/1/5/', 'InterestReceivedClassifiedAsOperatingActivities', 'Interest received, classified as operating activities')
,('Regulatory', 1, 1, 1, '/6/1/1/6/', 'IncomeTaxesPaidRefundClassifiedAsOperatingActivities', 'Income taxes paid (refund), classified as operating activities')
,('Regulatory', 1, 1, 1, '/6/1/1/7/', 'OtherInflowsOutflowsOfCashClassifiedAsOperatingActivities', 'Other inflows (outflows) of cash, classified as operating activities')
,('Regulatory', 1, 1, 1, '/6/1/2/', 'CashFlowsFromUsedInInvestingActivities', 'Cash flows from (used in) investing activities')
,('Regulatory', 1, 1, 1, '/6/1/2/1/', 'CashFlowsFromLosingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', 'Cash flows from losing control of subsidiaries or other businesses, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/2/', 'CashFlowsUsedInObtainingControlOfSubsidiariesOrOtherBusinessesClassifiedAsInvestingActivities', 'Cash flows used in obtaining control of subsidiaries or other businesses, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/3/', 'OtherCashReceiptsFromSalesOfEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', 'Other cash receipts from sales of equity or debt instruments of other entities, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/4/', 'OtherCashPaymentsToAcquireEquityOrDebtInstrumentsOfOtherEntitiesClassifiedAsInvestingActivities', 'Other cash payments to acquire equity or debt instruments of other entities, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/5/', 'OtherCashReceiptsFromSalesOfInterestsInJointVenturesClassifiedAsInvestingActivities', 'Other cash receipts from sales of interests in joint ventures, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/6/', 'OtherCashPaymentsToAcquireInterestsInJointVenturesClassifiedAsInvestingActivities', 'Other cash payments to acquire interests in joint ventures, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/7/', 'ProceedsFromSalesOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', 'Proceeds from sales of property, plant and equipment, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/8/', 'PurchaseOfPropertyPlantAndEquipmentClassifiedAsInvestingActivities', 'Purchase of property, plant and equipment, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/9/', 'ProceedsFromSalesOfIntangibleAssetsClassifiedAsInvestingActivities', 'Proceeds from sales of intangible assets, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/10/', 'PurchaseOfIntangibleAssetsClassifiedAsInvestingActivities', 'Purchase of intangible assets, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/11/', 'ProceedsFromOtherLongtermAssetsClassifiedAsInvestingActivities', 'Proceeds from sales of other long-term assets, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/12/', 'PurchaseOfOtherLongtermAssetsClassifiedAsInvestingActivities', 'Purchase of other long-term assets, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/13/', 'ProceedsFromGovernmentGrantsClassifiedAsInvestingActivities', 'Proceeds from government grants, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/14/', 'CashAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', 'Cash advances and loans made to other parties, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/15/', 'CashReceiptsFromRepaymentOfAdvancesAndLoansMadeToOtherPartiesClassifiedAsInvestingActivities', 'Cash receipts from repayment of advances and loans made to other parties, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/16/', 'CashPaymentsForFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', 'Cash payments for futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/17/', 'CashReceiptsFromFutureContractsForwardContractsOptionContractsAndSwapContractsClassifiedAsInvestingActivities', 'Cash receipts from futures contracts, forward contracts, option contracts and swap contracts, classified as investing activities')
,('Regulatory', 1, 1, 1, '/6/1/2/18/', 'DividendsReceivedClassifiedAsInvestingActivities', 'Dividends received, classified as investing activities')
,('Regulatory', 1, 0, 1, '/6/1/2/19/', 'InterestPaidClassifiedAsInvestingActivities', 'Interest paid, classified as investing activities')
,('Regulatory', 1, 1, 0, '/6/1/2/20/', 'InterestReceivedClassifiedAsInvestingActivities', 'Interest received, classified as investing activities')
,('Regulatory', 1, 1, 1, '/6/1/2/21/', 'IncomeTaxesPaidRefundClassifiedAsInvestingActivities', 'Income taxes paid (refund), classified as investing activities')
,('Regulatory', 1, 1, 1, '/6/1/2/22/', 'OtherInflowsOutflowsOfCashClassifiedAsInvestingActivities', 'Other inflows (outflows) of cash, classified as investing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/', 'CashFlowsFromUsedInFinancingActivities', 'Cash flows from (used in) financing activities')
,('Regulatory', 1, 1, 0, '/6/1/3/1/', 'ProceedsFromChangesInOwnershipInterestsInSubsidiaries', 'Proceeds from changes in ownership interests in subsidiaries that do not result in loss of control')
,('Regulatory', 1, 0, 1, '/6/1/3/2/', 'PaymentsFromChangesInOwnershipInterestsInSubsidiaries', 'Payments from changes in ownership interests in subsidiaries that do not result in loss of control')
,('Regulatory', 1, 1, 0, '/6/1/3/3/', 'ProceedsFromIssuingShares', 'Proceeds from issuing shares')
,('Regulatory', 1, 1, 0, '/6/1/3/4/', 'ProceedsFromIssuingOtherEquityInstruments', 'Proceeds from issuing other equity instruments')
,('Regulatory', 1, 0, 1, '/6/1/3/5/', 'PaymentsToAcquireOrRedeemEntitysShares', 'Payments to acquire or redeem entity''s shares')
,('Regulatory', 1, 0, 1, '/6/1/3/6/', 'PaymentsOfOtherEquityInstruments', 'Payments of other equity instruments')
,('Regulatory', 1, 1, 0, '/6/1/3/7/', 'ProceedsFromBorrowingsClassifiedAsFinancingActivities', 'Proceeds from borrowings, classified as financing activities')
,('Regulatory', 1, 0, 1, '/6/1/3/8/', 'RepaymentsOfBorrowingsClassifiedAsFinancingActivities', 'Repayments of borrowings, classified as financing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/9/', 'PaymentsOfLeaseLiabilitiesClassifiedAsFinancingActivities', 'Payments of lease liabilities, classified as financing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/10/', 'ProceedsFromGovernmentGrantsClassifiedAsFinancingActivities', 'Proceeds from government grants, classified as financing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/11/', 'DividendsPaidClassifiedAsFinancingActivities', 'Dividends paid, classified as financing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/12/', 'InterestPaidClassifiedAsFinancingActivities', 'Interest paid, classified as financing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/13/', 'IncomeTaxesPaidRefundClassifiedAsFinancingActivities', 'Income taxes paid (refund), classified as financing activities')
,('Regulatory', 1, 1, 1, '/6/1/3/14/', 'OtherInflowsOutflowsOfCashClassifiedAsFinancingActivities', 'Other inflows (outflows) of cash, classified as financing activities')
,('Extension', 1, 1, 1, '/6/1/4/', 'InternalCashTransferExtension', 'Internal cash transfer')
,('Regulatory', 1, 1, 1, '/6/2/', 'EffectOfExchangeRateChangesOnCashAndCashEquivalents', 'Effect of exchange rate changes on cash and cash equivalents')
,('Regulatory', 1, 1, 1, '/7/', 'ChangesInEquity', 'Increase (decrease) in equity')
,('Regulatory', 1, 1, 1, '/7/1/', 'ComprehensiveIncome', 'Comprehensive income')
,('Regulatory', 1, 1, 1, '/7/1/1/', 'ProfitLoss', 'Profit (loss)')
,('Regulatory', 1, 1, 1, '/7/1/2/', 'OtherComprehensiveIncome', 'Other comprehensive income')
,('Regulatory', 1, 0, 1, '/7/2/', 'IssueOfEquity', 'Issue of equity')
,('Regulatory', 1, 0, 1, '/7/3/', 'DividendsPaid', 'Dividends recognised as distributions to owners')
,('Regulatory', 1, 1, 1, '/7/4/', 'IncreaseDecreaseThroughOtherContributionsByOwners', 'Increase through other contributions by owners, equity')
,('Regulatory', 1, 1, 1, '/7/5/', 'IncreaseDecreaseThroughOtherDistributionsToOwners', 'Decrease through other distributions to owners, equity')
,('Regulatory', 1, 1, 1, '/7/6/', 'IncreaseDecreaseThroughTransfersAndOtherChangesEquity', 'Increase (decrease) through other changes, equity')
,('Regulatory', 1, 1, 1, '/7/7/', 'IncreaseDecreaseThroughTreasuryShareTransactions', 'Increase (decrease) through treasury share transactions, equity')
,('Regulatory', 1, 1, 1, '/7/8/', 'IncreaseDecreaseThroughChangesInOwnershipInterestsInSubsidiariesThatDoNotResultInLossOfControl', 'Increase (decrease) through changes in ownership interests in subsidiaries that do not result in loss of control, equity')
,('Regulatory', 1, 1, 1, '/7/9/', 'IncreaseDecreaseThroughSharebasedPaymentTransactions', 'Increase (decrease) through share-based payment transactions, equity')
,('Regulatory', 1, 1, 1, '/7/10/', 'AmountRemovedFromReserveOfCashFlowHedgesAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of cash flow hedges and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,('Regulatory', 1, 1, 1, '/7/11/', 'AmountRemovedFromReserveOfChangeInValueOfTimeValueOfOptionsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of change in value of time value of options and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,('Regulatory', 1, 1, 1, '/7/12/', 'AmountRemovedFromReserveOfChangeInValueOfForwardElementsOfForwardContractsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of change in value of forward elements of forward contracts and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,('Regulatory', 1, 1, 1, '/7/13/', 'AmountRemovedFromReserveOfChangeInValueOfForeignCurrencyBasisSpreadsAndIncludedInInitialCostOrOtherCarryingAmountOfNonfinancialAssetLiabilityOrFirmCommitmentForWhichFairValueHedgeAccountingIsApplied', 'Amount removed from reserve of change in value of foreign currency basis spreads and included in initial cost or other carrying amount of non-financial asset (liability) or firm commitment for which fair value hedge accounting is applied')
,('Regulatory', 1, 1, 1, '/8/', 'ChangesInOtherProvisions', 'Increase (decrease) in other provisions')
,('Regulatory', 1, 1, 1, '/8/1/', 'AdditionalProvisionsOtherProvisions', 'Additional provisions, other provisions')
,('Regulatory', 1, 1, 1, '/8/1/1/', 'NewProvisionsOtherProvisions', 'New provisions, other provisions')
,('Regulatory', 1, 1, 1, '/8/1/2/', 'IncreaseDecreaseInExistingProvisionsOtherProvisions', 'Increase in existing provisions, other provisions')
,('Regulatory', 1, 1, 1, '/8/2/', 'AcquisitionsThroughBusinessCombinationsOtherProvisions', 'Acquisitions through business combinations, other provisions')
,('Regulatory', 1, 1, 1, '/8/3/', 'ProvisionUsedOtherProvisions', 'Provision used, other provisions')
,('Regulatory', 1, 1, 1, '/8/4/', 'UnusedProvisionReversedOtherProvisions', 'Unused provision reversed, other provisions')
,('Regulatory', 1, 1, 1, '/8/5/', 'IncreaseDecreaseThroughTimeValueOfMoneyAdjustmentOtherProvisions', 'Increase through adjustments arising from passage of time, other provisions')
,('Regulatory', 1, 1, 1, '/8/6/', 'IncreaseDecreaseThroughChangeInDiscountRateOtherProvisions', 'Increase (decrease) through change in discount rate, other provisions')
,('Regulatory', 1, 1, 1, '/8/7/', 'IncreaseDecreaseThroughNetExchangeDifferencesOtherProvisions', 'Increase (decrease) through net exchange differences, other provisions')
,('Regulatory', 1, 1, 1, '/8/8/', 'DecreaseThroughLossOfControlOfSubsidiaryOtherProvisions', 'Decrease through loss of control of subsidiary, other provisions')
,('Regulatory', 1, 1, 1, '/8/9/', 'IncreaseDecreaseThroughTransfersAndOtherChangesOtherProvisions', 'Increase (decrease) through transfers and other changes, other provisions')

--,('Extension', 1, 1, 1, '/9/', 'ExpenseByFunctionExtension', 'Expense, by function')
--,('Regulatory', 1, 1, 0, '/9/1/', 'CostOfSales', 'Cost of sales')
--,('Regulatory', 1, 1, 0, '/9/2/', 'DistributionCosts', 'Distribution costs')
--,('Regulatory', 1, 1, 0, '/9/3/', 'AdministrativeExpense', 'Administrative expenses')
--,('Regulatory', 1, 1, 0, '/9/4/', 'OtherExpenseByFunction', 'Other expense, by function')

,('Regulatory', 1, 1, 1, '/9/', N'ExpenseByNature', N'Expenses, by nature', N'The amount of expenses aggregated according to their nature (for example, depreciation, purchases of materials, transport costs, employee benefits and advertising costs), and not reallocated among functions within the entity.')
,('Regulatory', 1, 1, 1, '/9/1/', N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used', N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
,('Regulatory', 1, 1, 1, '/9/2/', N'CostOfMerchandiseSold', N'Cost of merchandise sold', N'The amount of merchandise that was sold during the period and recognised as an expense.')
,('Regulatory', 0, 1, 1, '/9/2/1/', N'CostOfPurchasedEnergySold', N'Cost of purchased energy sold', N'The amount of purchased energy that was sold during the period and recognised as an expense.')
,('Regulatory', 1, 1, 1, '/9/3/', N'ServicesExpense', N'Services expense', N'The amount of expense arising from services.')
,('Regulatory', 1, 1, 1, '/9/3/1/', N'InsuranceExpense', N'Insurance expense', N'The amount of expense arising from purchased insurance.')
,('Regulatory', 1, 1, 1, '/9/3/2/', N'ProfessionalFeesExpense', N'Professional fees expense', N'The amount of fees paid or payable for professional services.')
,('Regulatory', 1, 1, 1, '/9/3/3/', N'TransportationExpense', N'Transportation expense', N'The amount of expense arising from transportation services.')
,('Regulatory', 1, 1, 1, '/9/3/4/', N'BankAndSimilarCharges', N'Bank and similar charges', N'The amount of bank and similar charges recognised by the entity as an expense.')
,('Regulatory', 1, 1, 1, '/9/3/5/', N'EnergyTransmissionCharges', N'Energy transmission charges', N'The amount of charges related to transmission of energy.')
,('Regulatory', 1, 1, 1, '/9/3/6/', N'TravelExpense', N'Travel expense', N'The amount of expense arising from travel.')
,('Regulatory', 1, 1, 1, '/9/3/7/', N'CommunicationExpense', N'Communication expense', N'The amount of expense arising from communication.')
,('Regulatory', 1, 1, 1, '/9/3/8/', N'UtilitiesExpense', N'Utilities expense', N'The amount of expense arising from purchased utilities.')
,('Regulatory', 1, 1, 1, '/9/3/9/', N'AdvertisingExpense', N'Advertising expense', N'The amount of expense arising from advertising.')

,('Regulatory', 1, 1, 1, '/9/4/', N'EmployeeBenefitsExpense', N'Employee benefits expense', N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
,('Regulatory', 1, 1, 1, '/9/4/1/', N'ShorttermEmployeeBenefitsExpense', N'Short-term employee benefits expense', N'The amount of expense from employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services.')
,('Regulatory', 1, 1, 1, '/9/4/1/1/', N'WagesAndSalaries', N'Wages and salaries', N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
,('Regulatory', 1, 1, 1, '/9/4/1/2/', N'SocialSecurityContributions', N'Social security contributions', N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
,('Regulatory', 1, 1, 1, '/9/4/1/3/', N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits', N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
,('Regulatory', 1, 1, 1, '/9/4/2/', N'PostemploymentBenefitExpenseDefinedContributionPlans', N'Post-employment benefit expense, defined contribution plans', N'The amount of post-employment benefit expense relating to defined contribution plans. Defined contribution plans are post-employment benefit plans under which an entity pays fixed contributions into a separate entity (a fund) and will have no legal or constructive obligation to pay further contributions if the fund does not hold sufficient assets to pay all employee benefits relating to employee service in the current and prior periods.')
,('Regulatory', 1, 1, 1, '/9/4/3/', N'PostemploymentBenefitExpenseDefinedBenefitPlans', N'Post-employment benefit expense, defined benefit plans', N'The amount of post-employment benefit expense relating to defined benefit plans. [Refer: Defined benefit plans [member]]')
,('Regulatory', 1, 1, 1, '/9/4/4/', N'TerminationBenefitsExpense', N'Termination benefits expense', N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
,('Regulatory', 1, 1, 1, '/9/4/5/', N'OtherLongtermBenefits', N'Other long-term employee benefits', N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
,('Regulatory', 1, 1, 1, '/9/4/6/', N'OtherEmployeeExpense', N'Other employee expense', N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')

,('Regulatory', 1, 1, 1, '/9/5/', N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Depreciation, amortisation and impairment loss (reversal of impairment loss) recognised in profit or loss', N'The amount of depreciation expense, amortisation expense and impairment loss (reversal of impairment loss) recognised in profit or loss. [Refer: Depreciation and amortisation expense; Impairment loss (reversal of impairment loss) recognised in profit or loss]')
,('Regulatory', 1, 1, 1, '/9/5/1/', N'DepreciationAndAmortisationExpense', N'Depreciation and amortisation expense', N'The amount of depreciation and amortisation expense. Depreciation and amortisation are the systematic allocations of depreciable amounts of assets over their useful lives.')
,('Regulatory', 1, 1, 1, '/9/5/2/', N'DepreciationExpense', N'Depreciation expense', N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
,('Regulatory', 1, 1, 1, '/9/5/3/', N'AmortisationExpense', N'Amortisation expense', N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
,('Regulatory', 1, 1, 1, '/9/5/4/', N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Impairment loss (reversal of impairment loss) recognised in profit or loss', N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
,('Regulatory', 1, 1, 1, '/9/6/', N'TaxExpenseOtherThanIncomeTaxExpense', N'Tax expense other than income tax expense', N'The amount of tax expense exclusive of income tax expense.')
,('Regulatory', 1, 1, 1, '/9/6/1/', N'PropertyTaxExpense', N'Property tax expense', N'The amount of tax expense levied on property. Property is land or a building - or part of a building - or both.')
,('Regulatory', 1, 1, 1, '/9/7/', N'OtherExpenseByNature', N'Other expenses, by nature', N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')

,('Extension', 1, 1, 0, '/10/1/', 'InventoryPurchaseExtension', 'Inventory purchase')
,('Extension', 1, 0, 1, '/10/2/', 'InventoryProductionExtension', 'Inventory production')
,('Extension', 1, 0, 1, '/10/3/', 'InventorySalesExtension', 'Inventory sales')
,('Extension', 1, 0, 1, '/10/4/', 'InventoryConsumptionExtension', 'Inventory consumption')
,('Extension', 1, 0, 1, '/10/5/', 'InventoryLossExtension', 'Inventory loss')
,('Extension', 1, 1, 1, '/10/9/', 'InventoryTransferExtension', 'Inventory transfer')

-- The IfrsType is determined from the IfrsAccountClassifications and IfrsEntryClassifications tables
MERGE [dbo].[IfrsConcepts] As t
USING (SELECT  [Id], [IfrsType], [Label] FROM @IfrsEntryClassifications) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED AND
(
	t.[IfrsType]		<> s.[IfrsType]

) THEN
UPDATE SET
	t.[IfrsType]		= s.[IfrsType]
WHEN NOT MATCHED THEN
	INSERT ([Id], [IfrsType], [Label])
	VALUES (s.[Id], s.[IfrsType], s.[Label]);

MERGE [dbo].[IfrsEntryClassifications] AS t
USING (
	SELECT  EC.[Id], EC.[Node], EC.[IsActive], EC.[ForDebit], EC.[ForCredit], C.[Label], C.[Documentation]
	FROM @IfrsEntryClassifications EC
	JOIN dbo.IfrsConcepts C ON EC.[Id] = C.[Id]
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED AND
(
	t.[Node]			<>	s.[Node]		OR
	t.[IsActive]		<>	s.[IsActive]	OR
	t.[ForDebit]		<>	s.[ForDebit]	OR
	t.[ForCredit]		<>	s.[ForCredit]	OR
	t.[Label]			<>	s.[Label]
) THEN
UPDATE SET
	t.[Node]			=	s.[Node],
	t.[IsActive]		=	s.[IsActive],
	t.[ForDebit]		=	s.[ForDebit],
	t.[ForCredit]		=	s.[ForCredit],
	t.[Label]			=	s.[Label],
	t.[Documentation]	=	s.[Documentation]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Ifrs Entry Classifications extension concepts we added erroneously
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],	[Node],	[IsActive],		[ForDebit],	[ForCredit],	[Label], [Documentation])
    VALUES (s.[Id], s.[Node], s.[IsActive], s.[ForDebit], s.[ForCredit], s.[Label], s.[Documentation]);
--OUTPUT deleted.*, $action, inserted.*; -- Does not work with triggers
;
UPDATE dbo.[IfrsEntryClassifications] SET [IsLeaf] = 0 
WHERE [Isleaf] = 1 AND [Node] IN (SELECT [ParentNode] FROM dbo.[IfrsEntryClassifications]);

UPDATE dbo.[IfrsEntryClassifications] SET [IsLeaf] = 1
WHERE [Isleaf] = 0 AND [Node] NOT IN (SELECT [ParentNode] FROM dbo.[IfrsEntryClassifications]);
