	IF NOT EXISTS(SELECT * FROM dbo.[AccountTypes])
BEGIN
DECLARE @AT TABLE (
	[Index] INT, [AllowsPureUnit] BIT, [IsMonetary] BIT, [Code] NVARCHAR(50),
	[Node] HIERARCHYID, [EntryTypeParentConcept] NVARCHAR (255), [Concept] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
)
--Script
INSERT INTO @AT VALUES(0,0,0,'1', '/1/', NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
INSERT INTO @AT VALUES(1,0,0,'11', '/1/1/', NULL,N'Assets', N'Assets',N'The amount of resources: (a) controlled by the entity as a result of past events; and (b) from which future economic benefits are expected to flow to the entity.')
INSERT INTO @AT VALUES(2,0,0,'111', '/1/1/1/', NULL,N'NoncurrentAssets', N'Non-current assets',N'The amount of assets that do not meet the definition of current assets. [Refer: Current assets]')
INSERT INTO @AT VALUES(3,1,0,'1111', '/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
INSERT INTO @AT VALUES(4,1,0,'111101', '/1/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'LandAndBuildings', N'Land and buildings',N'The amount of property, plant and equipment representing land and depreciable buildings and similar structures for use in operations. [Refer: Buildings; Land; Property, plant and equipment]')
INSERT INTO @AT VALUES(5,1,0,'1111011', '/1/1/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'Land', N'Land',N'The amount of property, plant and equipment representing land held by the entity for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(6,1,0,'1111012', '/1/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Buildings', N'Buildings',N'The amount of property, plant and equipment representing depreciable buildings and similar structures for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(7,1,0,'111102', '/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Machinery', N'Machinery',N'The amount of property, plant and equipment representing long-lived, depreciable machinery used in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(8,1,0,'111103', '/1/1/1/1/3/', N'ChangesInPropertyPlantAndEquipment',N'Vehicles', N'Vehicles',N'The amount of property, plant and equipment representing vehicles used in the entity''s operations, specifically to include aircraft, motor vehicles and ships. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(9,1,0,'1111031', '/1/1/1/1/3/1/', N'ChangesInPropertyPlantAndEquipment',N'Ships', N'Ships',N'The amount of property, plant and equipment representing seafaring or other maritime vessels used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(10,1,0,'1111032', '/1/1/1/1/3/2/', N'ChangesInPropertyPlantAndEquipment',N'Aircraft', N'Aircraft',N'The amount of property, plant and equipment representing aircraft used in the entity''s operations.')
INSERT INTO @AT VALUES(11,1,0,'1111033', '/1/1/1/1/3/3/', N'ChangesInPropertyPlantAndEquipment',N'MotorVehicles', N'Motor Vehicles',N'The amount of property, plant and equipment representing self-propelled ground vehicles used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(12,1,0,'111106', '/1/1/1/1/6/', N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(13,1,0,'111107', '/1/1/1/1/7/', N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(14,1,0,'111108', '/1/1/1/1/8/', N'ChangesInPropertyPlantAndEquipment',N'BearerPlants', N'Bearer plants',N'The amount of property, plant and equipment representing bearer plants. Bearer plant is a living plant that (a) is used in the production or supply of agricultural produce; (b) is expected to bear produce for more than one period; and (c) has a remote likelihood of being sold as agricultural produce, except for incidental scrap sales. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(15,1,0,'111109', '/1/1/1/1/9/', N'ChangesInPropertyPlantAndEquipment',N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',N'The amount of exploration and evaluation assets recognised as tangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(16,1,0,'111110', '/1/1/1/1/10/', N'ChangesInPropertyPlantAndEquipment',N'MiningAssets', N'Mining assets',N'The amount of assets related to mining activities of the entity.')
INSERT INTO @AT VALUES(17,1,0,'111111', '/1/1/1/1/11/', N'ChangesInPropertyPlantAndEquipment',N'OilAndGasAssets', N'Oil and gas assets',N'The amount of assets related to the exploration, evaluation, development or production of oil and gas.')
INSERT INTO @AT VALUES(18,0,0,'111112', '/1/1/1/1/12/', NULL,N'ConstructionInProgress', N'Construction in progress',N'The amount of expenditure capitalised during the construction of non-current assets that are not yet available for use. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(19,1,0,'111113', '/1/1/1/1/13/', N'ChangesInPropertyPlantAndEquipment',N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', N'Owner-occupied property measured using investment property fair value model',N'The amount of property, plant and equipment representing owner-occupied property measured using the investment property fair value model applying paragraph 29A of IAS 16. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(20,1,0,'111199', '/1/1/1/1/99/', N'ChangesInPropertyPlantAndEquipment',N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment',N'The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(21,0,0,'1112', '/1/1/1/2/', NULL,N'InvestmentProperty', N'Investment property',N'The amount of property (land or a building - or part of a building - or both) held (by the owner or by the lessee as a right-of-use asset) to earn rentals or for capital appreciation or both, rather than for: (a) use in the production or supply of goods or services or for administrative purposes; or (b) sale in the ordinary course of business.')
INSERT INTO @AT VALUES(22,1,0,'11121', '/1/1/1/2/1/', N'ChangesInInvestmentProperty',N'InvestmentPropertyCompleted', N'Investment property completed',N'The amount of investment property whose construction or development is complete. [Refer: Investment property]')
INSERT INTO @AT VALUES(23,0,0,'11122', '/1/1/1/2/2/', NULL,N'InvestmentPropertyUnderConstructionOrDevelopment', N'Investment property under construction or development',N'The amount of property that is being constructed or developed for future use as investment property. [Refer: Investment property]')
INSERT INTO @AT VALUES(24,0,0,'1113', '/1/1/1/3/', N'ChangesInGoodwill',N'Goodwill', N'Goodwill',N'The amount of assets representing the future economic benefits arising from other assets acquired in a business combination that are not individually identified and separately recognised. [Refer: Business combinations [member]]')
INSERT INTO @AT VALUES(25,1,0,'1114', '/1/1/1/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(26,0,0,'1115', '/1/1/1/5/', NULL,N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',N'The amount of investments accounted for using the equity method. The equity method is a method of accounting whereby the investment is initially recognised at cost and adjusted thereafter for the post-acquisition change in the investor''s share of net assets of the investee. The investor''s profit or loss includes its share of the profit or loss of the investee. The investor''s other comprehensive income includes its share of the other comprehensive income of the investee. [Refer: At cost [member]]')
INSERT INTO @AT VALUES(27,0,0,'11151', '/1/1/1/5/1/', NULL,N'InvestmentsInAssociatesAccountedForUsingEquityMethod', N'Investments in associates accounted for using equity method',N'The amount of investments in associates accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method]')
INSERT INTO @AT VALUES(28,0,0,'11152', '/1/1/1/5/2/', NULL,N'InvestmentsInJointVenturesAccountedForUsingEquityMethod', N'Investments in joint ventures accounted for using equity method',N'The amount of investments in joint ventures accounted for using the equity method. [Refer: Joint ventures [member]; Investments accounted for using equity method]')
INSERT INTO @AT VALUES(29,0,0,'1116', '/1/1/1/6/', NULL,N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',N'The amount of investments in subsidiaries, joint ventures and associates in an entity''s separate financial statements. [Refer: Associates [member]; Joint ventures [member]; Subsidiaries [member]; Investments in subsidiaries]')
INSERT INTO @AT VALUES(30,0,0,'11161', '/1/1/1/6/1/', NULL,N'InvestmentsInSubsidiaries', N'Investments in subsidiaries',N'The amount of investments in subsidiaries in an entity''s separate financial statements. [Refer: Subsidiaries [member]]')
INSERT INTO @AT VALUES(31,0,0,'11162', '/1/1/1/6/2/', NULL,N'InvestmentsInJointVentures', N'Investments in joint ventures',N'The amount of investments in joint ventures in an entity''s separate financial statements. [Refer: Joint ventures [member]]')
INSERT INTO @AT VALUES(32,0,0,'11163', '/1/1/1/6/3/', NULL,N'InvestmentsInAssociates', N'Investments in associates',N'The amount of investments in associates in an entity''s separate financial statements. [Refer: Associates [member]]')
INSERT INTO @AT VALUES(33,1,0,'1117', '/1/1/1/7/', NULL,N'NoncurrentBiologicalAssets', N'Non-current biological assets',N'The amount of living animals or plants recognised as assets.')
INSERT INTO @AT VALUES(34,0,0,'1118', '/1/1/1/8/', NULL,N'NoncurrentReceivables', N'Trade and other non-current receivables',N'The amount of non-current trade receivables and non-current other receivables. [Refer: Non-current trade receivables; Other non-current receivables]')
INSERT INTO @AT VALUES(35,0,1,'11181', '/1/1/1/8/1/', NULL,N'NoncurrentTradeReceivables', N'Non-current trade receivables',N'The amount of non-current trade receivables. [Refer: Trade receivables]')
INSERT INTO @AT VALUES(36,0,1,'11182', '/1/1/1/8/2/', NULL,N'NoncurrentReceivablesDueFromRelatedParties', N'Non-current receivables due from related parties',N'The amount of non-current receivables due from related parties. [Refer: Related parties [member]]')
INSERT INTO @AT VALUES(37,0,0,'11183', '/1/1/1/8/3/', NULL,N'NoncurrentPrepaymentsAndNoncurrentAccruedIncome', N'Non-current prepayments and non-current accrued income',N'The amount of non-current prepayments and non-current accrued income. [Refer: Prepayments; Accrued income]')
INSERT INTO @AT VALUES(38,0,0,'111831', '/1/1/1/8/3/1/', NULL,N'NoncurrentPrepayments', N'Non-current prepayments',N'The amount of non-current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(39,0,0,'111832', '/1/1/1/8/3/2/', NULL,N'NoncurrentAccruedIncome', N'Non-current accrued income',N'The amount of non-current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(40,0,1,'11184', '/1/1/1/8/4/', NULL,N'NoncurrentReceivablesFromTaxesOtherThanIncomeTax', N'Non-current receivables from taxes other than income tax',N'The amount of non-current receivables from taxes other than income tax. [Refer: Receivables from taxes other than income tax]')
INSERT INTO @AT VALUES(41,0,1,'111841', '/1/1/1/8/4/1/', NULL,N'NoncurrentValueAddedTaxReceivables', N'Non-current value added tax receivables',N'The amount of non-current value added tax receivables. [Refer: Value added tax receivables]')
INSERT INTO @AT VALUES(42,0,1,'11185', '/1/1/1/8/5/', NULL,N'NoncurrentReceivablesFromSaleOfProperties', N'Non-current receivables from sale of properties',N'The amount of non-current receivables from sale of properties. [Refer: Receivables from sale of properties]')
INSERT INTO @AT VALUES(43,0,1,'11186', '/1/1/1/8/6/', NULL,N'NoncurrentReceivablesFromRentalOfProperties', N'Non-current receivables from rental of properties',N'The amount of non-current receivables from rental of properties. [Refer: Receivables from rental of properties]')
INSERT INTO @AT VALUES(44,0,1,'11187', '/1/1/1/8/7/', NULL,N'OtherNoncurrentReceivables', N'Other non-current receivables',N'The amount of non-current other receivables. [Refer: Other receivables]')
INSERT INTO @AT VALUES(45,0,0,'1119', '/1/1/1/9/', NULL,N'NoncurrentInventories', N'Non-current inventories',N'The amount of non-current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(46,0,1,'11110', '/1/1/1/10/', NULL,N'DeferredTaxAssets', N'Deferred tax assets',N'The amounts of income taxes recoverable in future periods in respect of: (a) deductible temporary differences; (b) the carryforward of unused tax losses; and (c) the carryforward of unused tax credits. [Refer: Temporary differences [member]; Unused tax credits [member]; Unused tax losses [member]]')
INSERT INTO @AT VALUES(47,0,1,'11111', '/1/1/1/11/', NULL,N'CurrentTaxAssetsNoncurrent', N'Current tax assets, non-current',N'The non-current amount of current tax assets. [Refer: Current tax assets]')
INSERT INTO @AT VALUES(48,0,1,'11112', '/1/1/1/12/', NULL,N'OtherNoncurrentFinancialAssets', N'Other non-current financial assets',N'The amount of non-current financial assets that the entity does not separately disclose in the same statement or note. [Refer: Other financial assets]')
INSERT INTO @AT VALUES(49,0,1,'111121', '/1/1/1/12/1/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLoss', N'Non-current financial assets at fair value through profit or loss',N'The amount of non-current financial assets measured at fair value through profit or loss. [Refer: Financial assets at fair value through profit or loss]')
INSERT INTO @AT VALUES(50,0,1,'1111211', '/1/1/1/12/1/1/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition', N'Non-current financial assets at fair value through profit or loss, designated upon initial recognition or subsequently',N'The amount of non-current financial assets measured at fair value through profit or loss that were designated as such upon initial recognition or subsequently. [Refer: Financial assets at fair value through profit or loss, designated upon initial recognition or subsequently]')
INSERT INTO @AT VALUES(51,0,1,'1111212', '/1/1/1/12/1/2/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMeasuredAsSuchInAccordanceWithExemptionForRepurchaseOfOwnFinancialLiabilities', N'Non-current financial assets at fair value through profit or loss, measured as such in accordance with exemption for repurchase of own financial liabilities',N'The amount of non-current financial assets at fair value through profit or loss measured as such in accordance with the exemption for repurchase of own financial liabilities. [Refer: Financial assets at fair value through profit or loss, measured as such in accordance with exemption for repurchase of own financial liabilities]')
INSERT INTO @AT VALUES(52,0,1,'1111213', '/1/1/1/12/1/3/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMeasuredAsSuchInAccordanceWithExemptionForReacquisitionOfOwnEquityInstruments', N'Non-current financial assets at fair value through profit or loss, measured as such in accordance with exemption for reacquisition of own equity instruments',N'The amount of non-current financial assets at fair value through profit or loss measured as such in accordance with the exemption for reacquisition of own equity instruments. [Refer: Financial assets at fair value through profit or loss, measured as such in accordance with exemption for reacquisition of own equity instruments]')
INSERT INTO @AT VALUES(53,0,1,'1111214', '/1/1/1/12/1/4/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading', N'Non-current financial assets at fair value through profit or loss, classified as held for trading',N'The amount of non-current financial assets that are measured at fair value through profit or loss and that are classified as held for trading. [Refer: Financial assets at fair value through profit or loss, classified as held for trading]')
INSERT INTO @AT VALUES(54,0,1,'1111215', '/1/1/1/12/1/5/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughProfitOrLossMandatorilyMeasuredAtFairValue', N'Non-current financial assets at fair value through profit or loss, mandatorily measured at fair value',N'The amount of non-current financial assets mandatorily measured at fair value through profit or loss in accordance with IFRS 9. [Refer: Financial assets at fair value through profit or loss, mandatorily measured at fair value]')
INSERT INTO @AT VALUES(55,0,1,'111122', '/1/1/1/12/2/', NULL,N'NoncurrentFinancialAssetsAvailableforsale', N'Non-current financial assets available-for-sale',N'The amount of non-current financial assets available-for-sale. [Refer: Financial assets available-for-sale; Non-current financial assets]')
INSERT INTO @AT VALUES(56,0,1,'111123', '/1/1/1/12/3/', NULL,N'NoncurrentHeldtomaturityInvestments', N'Non-current held-to-maturity investments',N'The amount of non-current held-to-maturity investments. [Refer: Held-to-maturity investments]')
INSERT INTO @AT VALUES(57,0,1,'111124', '/1/1/1/12/4/', NULL,N'NoncurrentLoansAndReceivables', N'Non-current loans and receivables',N'The amount of non-current loans and receivables. [Refer: Loans and receivables]')
INSERT INTO @AT VALUES(58,0,1,'111125', '/1/1/1/12/5/', NULL,N'NoncurrentFinancialAssetsAtFairValueThroughOtherComprehensiveIncome', N'Non-current financial assets at fair value through other comprehensive income',N'The amount of non-current financial assets at fair value through other comprehensive income. [Refer: Financial assets at fair value through other comprehensive income]')
INSERT INTO @AT VALUES(59,0,1,'1111251', '/1/1/1/12/5/1/', NULL,N'NoncurrentFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome', N'Non-current financial assets measured at fair value through other comprehensive income',N'The amount of non-current financial assets measured at fair value through other comprehensive income. [Refer: Financial assets measured at fair value through other comprehensive income]')
INSERT INTO @AT VALUES(60,0,1,'1111252', '/1/1/1/12/5/2/', NULL,N'NoncurrentInvestmentsInEquityInstrumentsDesignatedAtFairValueThroughOtherComprehensiveIncome', N'Non-current investments in equity instruments designated at fair value through other comprehensive income',N'The amount of non-current investments in equity instruments that the entity has designated at fair value through other comprehensive income. [Refer: Investments in equity instruments designated at fair value through other comprehensive income]')
INSERT INTO @AT VALUES(61,0,1,'111126', '/1/1/1/12/6/', NULL,N'NoncurrentFinancialAssetsAtAmortisedCost', N'Non-current financial assets at amortised cost',N'The amount of non-current financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(62,0,0,'11113', '/1/1/1/13/', NULL,N'OtherNoncurrentNonfinancialAssets', N'Other non-current non-financial assets',N'The amount of non-current non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(63,0,0,'11114', '/1/1/1/14/', NULL,N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Non-current non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of non-current non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(64,0,0,'112', '/1/1/2/', NULL,N'CurrentAssets', N'Current assets',N'The amount of assets that the entity (a) expects to realise or intends to sell or consume in its normal operating cycle; (b) holds primarily for the purpose of trading; (c) expects to realise within twelve months after the reporting period; or (d) classifies as cash or cash equivalents (as defined in IAS 7) unless the asset is restricted from being exchanged or used to settle a liability for at least twelve months after the reporting period. [Refer: Assets]')
INSERT INTO @AT VALUES(65,0,0,'1121', '/1/1/2/1/', NULL,N'CurrentAssetsOtherThanAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners', N'Current assets other than non-current assets or disposal groups classified as held for sale or as held for distribution to owners',N'The amount of current assets other than non-current assets or disposal groups classified as held for sale or as held for distribution to owners. [Refer: Current assets; Disposal groups classified as held for sale [member]; Non-current assets or disposal groups classified as held for sale; Non-current assets or disposal groups classified as held for distribution to owners]')
INSERT INTO @AT VALUES(66,0,0,'11211', '/1/1/2/1/1/', N'ChangesInInventories',N'Inventories', N'Current inventories',N'The amount of current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(67,0,0,'112111', '/1/1/2/1/1/1/', N'ChangesInInventories',N'CurrentInventoriesHeldForSale', N'Current inventories held for sale',N'A classification of current inventory representing the amount of inventories held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(68,0,0,'1121111', '/1/1/2/1/1/1/1/', N'ChangesInInventories',N'Merchandise', N'Current merchandise',N'A classification of current inventory representing the amount of goods acquired for resale. [Refer: Inventories]')
INSERT INTO @AT VALUES(69,0,0,'1121112', '/1/1/2/1/1/1/2/', N'ChangesInInventories',N'CurrentFoodAndBeverage', N'Current food and beverage',N'A classification of current inventory representing the amount of food and beverage. [Refer: Inventories]')
INSERT INTO @AT VALUES(70,0,0,'1121113', '/1/1/2/1/1/1/3/', N'ChangesInInventories',N'CurrentAgriculturalProduce', N'Current agricultural produce',N'A classification of current inventory representing the amount of harvested produce of the entity''s biological assets. [Refer: Biological assets; Inventories]')
INSERT INTO @AT VALUES(71,0,0,'1121114', '/1/1/2/1/1/1/4/', N'ChangesInInventories',N'FinishedGoods', N'Current finished goods',N'A classification of current inventory representing the amount of goods that have completed the production process and are held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(72,0,0,'1121115', '/1/1/2/1/1/1/5/', N'ChangesInInventories',N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property intended for sale in ordinary course of business',N'The amount of property intended for sale in the ordinary course of business of the entity. Property is land or a building - or part of a building - or both.')
INSERT INTO @AT VALUES(73,0,0,'112112', '/1/1/2/1/1/2/', NULL,N'WorkInProgress', N'Current work in progress',N'A classification of current inventory representing the amount of assets currently in production, which require further processes to be converted into finished goods or services. [Refer: Current finished goods; Inventories]')
INSERT INTO @AT VALUES(74,0,0,'112113', '/1/1/2/1/1/3/', N'ChangesInInventories',N'CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices', N'Current materials and supplies to be consumed in production process or rendering services',N'A classification of current inventory representing the amount of materials and supplies to be consumed in a production process or while rendering services. [Refer: Inventories]')
INSERT INTO @AT VALUES(75,0,0,'1121131', '/1/1/2/1/1/3/1/', N'ChangesInInventories',N'CurrentRawMaterialsAndCurrentProductionSupplies', N'Current raw materials and current production supplies',N'A classification of current inventory representing the amount of current raw materials and current production supplies. [Refer: Current production supplies; Current raw materials]')
INSERT INTO @AT VALUES(76,0,0,'11211311', '/1/1/2/1/1/3/1/1/', N'ChangesInInventories',N'RawMaterials', N'Current raw materials',N'A classification of current inventory representing the amount of assets to be consumed in the production process or in the rendering of services. [Refer: Inventories]')
INSERT INTO @AT VALUES(77,0,0,'11211312', '/1/1/2/1/1/3/1/2/', N'ChangesInInventories',N'ProductionSupplies', N'Current production supplies',N'A classification of current inventory representing the amount of supplies to be used for the production process. [Refer: Inventories]')
INSERT INTO @AT VALUES(78,0,0,'1121132', '/1/1/2/1/1/3/2/', N'ChangesInInventories',N'CurrentPackagingAndStorageMaterials', N'Current packaging and storage materials',N'A classification of current inventory representing the amount of packaging and storage materials. [Refer: Inventories]')
INSERT INTO @AT VALUES(79,0,0,'1121133', '/1/1/2/1/1/3/3/', N'ChangesInInventories',N'SpareParts', N'Current spare parts',N'A classification of current inventory representing the amount of interchangeable parts that are kept in an inventory and are used for the repair or replacement of failed parts. [Refer: Inventories]')
INSERT INTO @AT VALUES(80,0,0,'1121134', '/1/1/2/1/1/3/4/', N'ChangesInInventories',N'CurrentFuel', N'Current fuel',N'A classification of current inventory representing the amount of fuel. [Refer: Inventories]')
INSERT INTO @AT VALUES(81,0,0,'112114', '/1/1/2/1/1/4/', NULL,N'CurrentInventoriesInTransit', N'Current inventories in transit',N'A classification of current inventory representing the amount of inventories in transit. [Refer: Inventories]')
INSERT INTO @AT VALUES(82,0,0,'112119', '/1/1/2/1/1/9/', N'ChangesInInventories',N'OtherInventories', N'Other current inventories',N'The amount of inventory that the entity does not separately disclose in the same statement or note. [Refer: Inventories]')
INSERT INTO @AT VALUES(83,0,0,'11212', '/1/1/2/1/2/', NULL,N'TradeAndOtherCurrentReceivables', N'Trade and other current receivables',N'The amount of current trade receivables and current other receivables. [Refer: Current trade receivables; Other current receivables]')
INSERT INTO @AT VALUES(84,0,1,'112121', '/1/1/2/1/2/1/', NULL,N'CurrentTradeReceivables', N'Current trade receivables',N'The amount of current trade receivables. [Refer: Trade receivables]')
INSERT INTO @AT VALUES(85,0,1,'112122', '/1/1/2/1/2/2/', NULL,N'TradeAndOtherCurrentReceivablesDueFromRelatedParties', N'Current receivables due from related parties',N'The amount of current receivables due from related parties. [Refer: Related parties [member]]')
INSERT INTO @AT VALUES(86,0,0,'112123', '/1/1/2/1/2/3/', NULL,N'CurrentPrepaymentsAndCurrentAccruedIncome', N'Current prepayments and current accrued income',N'The amount of current prepayments and current accrued income. [Refer: Prepayments; Accrued income]')
INSERT INTO @AT VALUES(87,0,0,'1121231', '/1/1/2/1/2/3/1/', NULL,N'CurrentPrepayments', N'Current prepayments',N'The amount of current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(88,0,0,'11212311', '/1/1/2/1/2/3/1/1/', NULL,N'CurrentAdvancesToSuppliers', N'Current advances to suppliers',N'The amount of current advances made to suppliers before goods or services are received.')
INSERT INTO @AT VALUES(89,0,0,'11212312', '/1/1/2/1/2/3/1/2/', NULL,N'CurrentPrepaidExpenses', N'Current prepaid expenses',N'The amount recognised as a current asset for expenditures made prior to the period when the economic benefit will be realised.')
INSERT INTO @AT VALUES(90,0,1,'1121232', '/1/1/2/1/2/3/2/', NULL,N'CurrentAccruedIncome', N'Current accrued income',N'The amount of current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(91,0,1,'112124', '/1/1/2/1/2/4/', NULL,N'CurrentBilledButNotReceivedExtension', N'Current billed but not received',N'The amount invoiced but against which there was no good or service received')
INSERT INTO @AT VALUES(92,0,1,'112125', '/1/1/2/1/2/5/', NULL,N'CurrentReceivablesFromTaxesOtherThanIncomeTax', N'Current receivables from taxes other than income tax',N'The amount of current receivables from taxes other than income tax. [Refer: Receivables from taxes other than income tax]')
INSERT INTO @AT VALUES(93,0,1,'1121251', '/1/1/2/1/2/5/1/', NULL,N'CurrentValueAddedTaxReceivables', N'Current value added tax receivables',N'The amount of current value added tax receivables. [Refer: Value added tax receivables]')
INSERT INTO @AT VALUES(94,0,1,'1121252', '/1/1/2/1/2/5/2/', NULL,N'WithholdingTaxReceivablesExtension', N'Withholding tax receivables',N'The amount of receivables related to a withtholding tax.')
INSERT INTO @AT VALUES(95,0,1,'112127', '/1/1/2/1/2/7/', NULL,N'CurrentReceivablesFromRentalOfProperties', N'Current receivables from rental of properties',N'The amount of current receivables from rental of properties. [Refer: Receivables from rental of properties]')
INSERT INTO @AT VALUES(96,0,1,'112128', '/1/1/2/1/2/8/', NULL,N'OtherCurrentReceivables', N'Other current receivables',N'The amount of current other receivables. [Refer: Other receivables]')
INSERT INTO @AT VALUES(97,0,1,'112129', '/1/1/2/1/2/9/', NULL,N'AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension', N'Allowance account for credit losses of trade and other current receivables',N'The amount of an allowance account used to record impairments to trade and other current receivables due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(98,0,1,'11213', '/1/1/2/1/3/', NULL,N'CurrentTaxAssetsCurrent', N'Current tax assets, current',N'The current amount of current tax assets. [Refer: Current tax assets]')
INSERT INTO @AT VALUES(99,0,0,'11214', '/1/1/2/1/4/', NULL,N'CurrentBiologicalAssets', N'Current biological assets',N'The amount of current biological assets. [Refer: Biological assets]')
INSERT INTO @AT VALUES(100,0,1,'11215', '/1/1/2/1/5/', NULL,N'OtherCurrentFinancialAssets', N'Other current financial assets',N'The amount of current financial assets that the entity does not separately disclose in the same statement or note. [Refer: Other financial assets; Current financial assets]')
INSERT INTO @AT VALUES(101,0,0,'11216', '/1/1/2/1/6/', NULL,N'OtherCurrentNonfinancialAssets', N'Other current non-financial assets',N'The amount of current non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(102,0,1,'11217', '/1/1/2/1/7/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(103,0,1,'112171', '/1/1/2/1/7/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'Cash', N'Cash',N'The amount of cash on hand and demand deposits. [Refer: Cash on hand]')
INSERT INTO @AT VALUES(104,0,1,'1121711', '/1/1/2/1/7/1/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashOnHand', N'Cash on hand',N'The amount of cash held by the entity. This does not include demand deposits.')
INSERT INTO @AT VALUES(105,0,1,'1121712', '/1/1/2/1/7/1/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'BalancesWithBanks', N'Balances with banks',N'The amount of cash balances held at banks.')
INSERT INTO @AT VALUES(106,0,1,'112172', '/1/1/2/1/7/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashEquivalents', N'Cash equivalents',N'The amount of short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value.')
INSERT INTO @AT VALUES(107,0,1,'1121721', '/1/1/2/1/7/2/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ShorttermDepositsClassifiedAsCashEquivalents', N'Short-term deposits, classified as cash equivalents',N'A classification of cash equivalents representing short-term deposits. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(108,0,1,'1121722', '/1/1/2/1/7/2/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ShorttermInvestmentsClassifiedAsCashEquivalents', N'Short-term investments, classified as cash equivalents',N'A classification of cash equivalents representing short-term investments. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(109,0,1,'1121723', '/1/1/2/1/7/2/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'BankingArrangementsClassifiedAsCashEquivalents', N'Other banking arrangements, classified as cash equivalents',N'A classification of cash equivalents representing banking arrangements that the entity does not separately disclose in the same statement or note. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(110,0,1,'112173', '/1/1/2/1/7/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'OtherCashAndCashEquivalents', N'Other cash and cash equivalents',N'The amount of cash and cash equivalents that the entity does not separately disclose in the same statement or note. [Refer: Cash and cash equivalents]')
INSERT INTO @AT VALUES(111,0,0,'11218', '/1/1/2/1/8/', NULL,N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Current non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of current non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(112,0,0,'1122', '/1/1/2/2/', NULL,N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners',N'The amount of non-current assets or disposal groups classified as held for sale or as held for distribution to owners. [Refer: Non-current assets or disposal groups classified as held for distribution to owners; Non-current assets or disposal groups classified as held for sale]')
INSERT INTO @AT VALUES(113,0,0,'11221', '/1/1/2/2/1/', NULL,N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSale', N'Non-current assets or disposal groups classified as held for sale',N'The amount of non-current assets or disposal groups classified as held for sale. [Refer: Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(114,0,0,'11222', '/1/1/2/2/2/', NULL,N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForDistributionToOwners', N'Non-current assets or disposal groups classified as held for distribution to owners',N'The amount of non-current assets or disposal groups classified as held for distribution to owners. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(115,0,1,'113', '/1/1/3/', NULL,N'AllowanceAccountForCreditLossesOfFinancialAssets', N'Allowance account for credit losses of financial assets',N'The amount of an allowance account used to record impairments to financial assets due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(116,0,1,'1131', '/1/1/3/1/', NULL,N'AllowanceAccountForCreditLossesOfTradeAndOtherReceivablesExtension', N'Allowance account for credit losses of trade and other receivables',N'The amount of an allowance account used to record impairments to trade and other receivables due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(117,0,1,'1132', '/1/1/3/2/', NULL,N'AllowanceAccountForCreditLossesOfOtherFinancialAssetsExtension', N'Allowance account for credit losses of other financial assets',N'The amount of an allowance account used to record impairments to other financial assets due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(118,0,0,'12', '/1/2/', NULL,N'EquityAndLiabilities', N'Equity and liabilities',N'The amount of the entity''s equity and liabilities. [Refer: Equity; Liabilities]')
INSERT INTO @AT VALUES(119,0,0,'121', '/1/2/1/', N'ChangesInEquity',N'Equity', N'Equity',N'The amount of residual interest in the assets of the entity after deducting all its liabilities.')
INSERT INTO @AT VALUES(120,0,0,'1211', '/1/2/1/1/', N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
INSERT INTO @AT VALUES(121,0,0,'1212', '/1/2/1/2/', N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(122,0,0,'1213', '/1/2/1/3/', N'ChangesInEquity',N'SharePremium', N'Share premium',N'The amount received or receivable from the issuance of the entity''s shares in excess of nominal value.')
INSERT INTO @AT VALUES(123,0,0,'1214', '/1/2/1/4/', N'ChangesInEquity',N'TreasuryShares', N'Treasury shares',N'An entity’s own equity instruments, held by the entity or other members of the consolidated group.')
INSERT INTO @AT VALUES(124,0,0,'1215', '/1/2/1/5/', N'ChangesInEquity',N'OtherEquityInterest', N'Other equity interest',N'The amount of equity interest of an entity without share capital that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(125,0,0,'1216', '/1/2/1/6/', N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(126,0,0,'121601', '/1/2/1/6/1/', N'ChangesInEquity',N'RevaluationSurplus', N'Revaluation surplus',N'A component of equity representing the accumulated revaluation surplus on the revaluation of assets recognised in other comprehensive income. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(127,0,0,'121602', '/1/2/1/6/2/', N'ChangesInEquity',N'ReserveOfExchangeDifferencesOnTranslation', N'Reserve of exchange differences on translation',N'A component of equity representing exchange differences on translation of financial statements of foreign operations recognised in other comprehensive income and accumulated in equity. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(128,0,0,'121603', '/1/2/1/6/3/', N'ChangesInEquity',N'ReserveOfCashFlowHedges', N'Reserve of cash flow hedges',N'A component of equity representing the accumulated portion of gain (loss) on a hedging instrument that is determined to be an effective hedge for cash flow hedges. [Refer: Cash flow hedges [member]]')
INSERT INTO @AT VALUES(129,0,0,'121604', '/1/2/1/6/4/', N'ChangesInEquity',N'ReserveOfGainsAndLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments', N'Reserve of gains and losses on hedging instruments that hedge investments in equity instruments',N'A component of equity representing the accumulated gains and losses on hedging instruments that hedge investments in equity instruments that the entity has designated at fair value through other comprehensive income.')
INSERT INTO @AT VALUES(130,0,0,'121605', '/1/2/1/6/5/', N'ChangesInEquity',N'ReserveOfChangeInValueOfTimeValueOfOptions', N'Reserve of change in value of time value of options',N'A component of equity representing the accumulated change in the value of the time value of options when separating the intrinsic value and time value of an option contract and designating as the hedging instrument only the changes in the intrinsic value.')
INSERT INTO @AT VALUES(131,0,0,'121606', '/1/2/1/6/6/', N'ChangesInEquity',N'ReserveOfChangeInValueOfForwardElementsOfForwardContracts', N'Reserve of change in value of forward elements of forward contracts',N'A component of equity representing the accumulated change in the value of the forward elements of forward contracts when separating the forward element and spot element of a forward contract and designating as the hedging instrument only the changes in the spot element.')
INSERT INTO @AT VALUES(132,0,0,'121607', '/1/2/1/6/7/', N'ChangesInEquity',N'ReserveOfChangeInValueOfForeignCurrencyBasisSpreads', N'Reserve of change in value of foreign currency basis spreads',N'A component of equity representing the accumulated change in the value of foreign currency basis spreads of financial instruments when excluding them from the designation of these financial instruments as hedging instruments.')
INSERT INTO @AT VALUES(133,0,0,'121608', '/1/2/1/6/8/', N'ChangesInEquity',N'ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome', N'Reserve of gains and losses on financial assets measured at fair value through other comprehensive income',N'A component of equity representing the reserve of gains and losses on financial assets measured at fair value through other comprehensive income. [Refer: Financial assets measured at fair value through other comprehensive income; Other comprehensive income]')
INSERT INTO @AT VALUES(134,0,0,'121609', '/1/2/1/6/9/', N'ChangesInEquity',N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillBeReclassifiedToProfitOrLoss', N'Reserve of insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will be reclassified to profit or loss',N'A component of equity representing the accumulated insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will be reclassified subsequently to profit or loss. [Refer: Insurance finance income (expenses); Insurance contracts issued [member]]')
INSERT INTO @AT VALUES(135,0,0,'121610', '/1/2/1/6/10/', N'ChangesInEquity',N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss', N'Reserve of insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will not be reclassified to profit or loss',N'A component of equity representing the accumulated insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will not be reclassified subsequently to profit or loss. [Refer: Insurance finance income (expenses); Insurance contracts issued [member]]')
INSERT INTO @AT VALUES(136,0,0,'121611', '/1/2/1/6/11/', N'ChangesInEquity',N'ReserveOfFinanceIncomeExpensesFromReinsuranceContractsHeldExcludedFromProfitOrLoss', N'Reserve of finance income (expenses) from reinsurance contracts held excluded from profit or loss',N'A component of equity representing the accumulated finance income (expenses) from reinsurance contracts held excluded from profit or loss. [Refer: Insurance finance income (expenses); Reinsurance contracts held [member]]')
INSERT INTO @AT VALUES(137,0,0,'121612', '/1/2/1/6/12/', N'ChangesInEquity',N'ReserveOfGainsAndLossesOnRemeasuringAvailableforsaleFinancialAssets', N'Reserve of gains and losses on remeasuring available-for-sale financial assets',N'A component of equity representing accumulated gains and losses on remeasuring available-for-sale financial assets. [Refer: Financial assets available-for-sale]')
INSERT INTO @AT VALUES(138,0,0,'121613', '/1/2/1/6/13/', N'ChangesInEquity',N'ReserveOfSharebasedPayments', N'Reserve of share-based payments',N'A component of equity resulting from share-based payments.')
INSERT INTO @AT VALUES(139,0,0,'121614', '/1/2/1/6/14/', N'ChangesInEquity',N'ReserveOfRemeasurementsOfDefinedBenefitPlans', N'Reserve of remeasurements of defined benefit plans',N'A component of equity representing the accumulated remeasurements of defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(140,0,0,'121615', '/1/2/1/6/15/', N'ChangesInEquity',N'AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale', N'Amount recognised in other comprehensive income and accumulated in equity relating to non-current assets or disposal groups held for sale',N'The amount recognised in other comprehensive income and accumulated in equity, relating to non-current assets or disposal groups held for sale. [Refer: Non-current assets or disposal groups classified as held for sale; Other reserves; Other comprehensive income; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(141,0,0,'121616', '/1/2/1/6/16/', N'ChangesInEquity',N'ReserveOfGainsAndLossesFromInvestmentsInEquityInstruments', N'Reserve of gains and losses from investments in equity instruments',N'A component of equity representing accumulated gains and losses from investments in equity instruments that the entity has designated at fair value through other comprehensive income.')
INSERT INTO @AT VALUES(142,0,0,'121617', '/1/2/1/6/17/', N'ChangesInEquity',N'ReserveOfChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability', N'Reserve of change in fair value of financial liability attributable to change in credit risk of liability',N'A component of equity representing the accumulated change in fair value of financial liabilities attributable to change in the credit risk of the liabilities. [Refer: Credit risk [member]; Financial liabilities]')
INSERT INTO @AT VALUES(143,0,0,'121618', '/1/2/1/6/18/', N'ChangesInEquity',N'ReserveForCatastrophe', N'Reserve for catastrophe',N'A component of equity representing resources to provide for infrequent but severe catastrophic losses caused by events such as damage to nuclear installations or satellites, or earthquake damage.')
INSERT INTO @AT VALUES(144,0,0,'121619', '/1/2/1/6/19/', N'ChangesInEquity',N'ReserveForEqualisation', N'Reserve for equalisation',N'A component of equity representing resources to cover random fluctuations of claim expenses around the expected value of claims for some types of insurance contract.')
INSERT INTO @AT VALUES(145,0,0,'121620', '/1/2/1/6/20/', N'ChangesInEquity',N'ReserveOfDiscretionaryParticipationFeatures', N'Reserve of discretionary participation features',N'A component of equity resulting from discretionary participation features. Discretionary participation features are contractual rights to receive, as a supplement to guaranteed benefits, additional benefits: (a) that are likely to be a significant portion of the total contractual benefits; (b) whose amount or timing is contractually at the discretion of the issuer; and (c) that are contractually based on: (i) the performance of a specified pool of contracts or a specified type of contract; (ii) realised and/or unrealised investment returns on a specified pool of assets held by the issuer; or (iii) the profit or loss of the company, fund or other entity that issues the contract.')
INSERT INTO @AT VALUES(146,0,0,'121621', '/1/2/1/6/21/', N'ChangesInEquity',N'ReserveOfEquityComponentOfConvertibleInstruments', N'Reserve of equity component of convertible instruments',N'A component of equity representing components of convertible instruments classified as equity.')
INSERT INTO @AT VALUES(147,0,0,'121622', '/1/2/1/6/22/', N'ChangesInEquity',N'CapitalRedemptionReserve', N'Capital redemption reserve',N'A component of equity representing the reserve for the redemption of the entity''s own shares.')
INSERT INTO @AT VALUES(148,0,0,'121623', '/1/2/1/6/23/', N'ChangesInEquity',N'MergerReserve', N'Merger reserve',N'A component of equity that may result in relation to a business combination outside the scope of IFRS 3.')
INSERT INTO @AT VALUES(149,0,0,'121624', '/1/2/1/6/24/', N'ChangesInEquity',N'StatutoryReserve', N'Statutory reserve',N'A component of equity representing reserves created based on legal requirements.')
INSERT INTO @AT VALUES(150,0,0,'122', '/1/2/2/', NULL,N'Liabilities', N'Liabilities',N'The amount of a present obligation of the entity to transfer an economic resource as a result of past events. Economic resource is a right that has the potential to produce economic benefits.')
INSERT INTO @AT VALUES(151,0,0,'1221', '/1/2/2/1/', NULL,N'NoncurrentLiabilities', N'Non-current liabilities',N'The amount of liabilities that do not meet the definition of current liabilities. [Refer: Current liabilities]')
INSERT INTO @AT VALUES(152,0,0,'12211', '/1/2/2/1/1/', NULL,N'NoncurrentProvisions', N'Non-current provisions',N'The amount of non-current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(153,0,1,'122111', '/1/2/2/1/1/1/', NULL,N'NoncurrentProvisionsForEmployeeBenefits', N'Non-current provisions for employee benefits',N'The amount of non-current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(154,0,0,'122112', '/1/2/2/1/1/2/', NULL,N'OtherLongtermProvisions', N'Other non-current provisions',N'The amount of non-current provisions other than provisions for employee benefits. [Refer: Non-current provisions]')
INSERT INTO @AT VALUES(155,0,0,'1221121', '/1/2/2/1/1/2/1/', NULL,N'LongtermWarrantyProvision', N'Non-current warranty provision',N'The amount of non-current provision for warranties. [Refer: Warranty provision]')
INSERT INTO @AT VALUES(156,0,1,'1221122', '/1/2/2/1/1/2/2/', NULL,N'LongtermRestructuringProvision', N'Non-current restructuring provision',N'The amount of non-current provision for restructuring. [Refer: Restructuring provision]')
INSERT INTO @AT VALUES(157,0,1,'1221123', '/1/2/2/1/1/2/3/', NULL,N'LongtermLegalProceedingsProvision', N'Non-current legal proceedings provision',N'The amount of non-current provision for legal proceedings. [Refer: Legal proceedings provision]')
INSERT INTO @AT VALUES(158,0,1,'1221124', '/1/2/2/1/1/2/4/', NULL,N'NoncurrentRefundsProvision', N'Non-current refunds provision',N'The amount of non-current provision for refunds. [Refer: Refunds provision]')
INSERT INTO @AT VALUES(159,0,1,'1221125', '/1/2/2/1/1/2/5/', NULL,N'LongtermOnerousContractsProvision', N'Non-current onerous contracts provision',N'The amount of non-current provision for onerous contracts. [Refer: Onerous contracts provision]')
INSERT INTO @AT VALUES(160,0,1,'1221126', '/1/2/2/1/1/2/6/', NULL,N'LongtermProvisionForDecommissioningRestorationAndRehabilitationCosts', N'Non-current provision for decommissioning, restoration and rehabilitation costs',N'The amount of non-current provision for decommissioning, restoration and rehabilitation costs. [Refer: Provision for decommissioning, restoration and rehabilitation costs]')
INSERT INTO @AT VALUES(161,0,1,'1221127', '/1/2/2/1/1/2/7/', NULL,N'LongtermMiscellaneousOtherProvisions', N'Non-current miscellaneous other provisions',N'The amount of miscellaneous non-current other provisions. [Refer: Miscellaneous other provisions]')
INSERT INTO @AT VALUES(162,0,0,'12212', '/1/2/2/1/2/', NULL,N'NoncurrentPayables', N'Trade and other non-current payables',N'The amount of non-current trade payables and non-current other payables. [Refer: Other non-current payables; Non-current trade payables]')
INSERT INTO @AT VALUES(163,0,1,'122121', '/1/2/2/1/2/1/', NULL,N'NoncurrentPayablesToTradeSuppliers', N'Non-current trade payables',N'The non-current amount of payment due to suppliers for goods and services used in the entity''s business. [Refer: Trade payables]')
INSERT INTO @AT VALUES(164,0,1,'122122', '/1/2/2/1/2/2/', NULL,N'NoncurrentPayablesToRelatedParties', N'Non-current payables to related parties',N'The amount of non-current payables due to related parties. [Refer: Related parties [member]; Payables to related parties]')
INSERT INTO @AT VALUES(165,0,1,'122123', '/1/2/2/1/2/3/', NULL,N'AccrualsAndDeferredIncomeClassifiedAsNoncurrent', N'Accruals and deferred income classified as non-current',N'The amount of accruals and deferred income classified as non-current. [Refer: Accruals and deferred income]')
INSERT INTO @AT VALUES(166,0,0,'1221231', '/1/2/2/1/2/3/1/', NULL,N'DeferredIncomeClassifiedAsNoncurrent', N'Deferred income classified as non-current',N'The amount of deferred income classified as non-current. [Refer: Deferred income]')
INSERT INTO @AT VALUES(167,0,0,'12212311', '/1/2/2/1/2/3/1/1/', NULL,N'RentDeferredIncomeClassifiedAsNoncurrent', N'Rent deferred income classified as non-current',N'The amount of rent deferred income classified as non-current. [Refer: Rent deferred income]')
INSERT INTO @AT VALUES(168,0,1,'1221232', '/1/2/2/1/2/3/1/1/', NULL,N'AccrualsClassifiedAsNoncurrent', N'Accruals classified as non-current',N'The amount of accruals classified as non-current. [Refer: Accruals]')
INSERT INTO @AT VALUES(169,0,1,'122124', '/1/2/2/1/2/4/', NULL,N'NoncurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Non-current payables on social security and taxes other than income tax',N'The amount of non-current payables on social security and taxes other than incomes tax. [Refer: Payables on social security and taxes other than income tax]')
INSERT INTO @AT VALUES(170,0,1,'1221241', '/1/2/2/1/2/4/1/', NULL,N'NoncurrentValueAddedTaxPayables', N'Non-current value added tax payables',N'The amount of non-current value added tax payables. [Refer: Value added tax payables]')
INSERT INTO @AT VALUES(171,0,1,'1221242', '/1/2/2/1/2/4/2/', NULL,N'NoncurrentExciseTaxPayables', N'Non-current excise tax payables',N'The amount of non-current excise tax payables. [Refer: Excise tax payables]')
INSERT INTO @AT VALUES(172,0,1,'1221243', '/1/2/2/1/2/5/', NULL,N'NoncurrentRetentionPayables', N'Non-current retention payables',N'The amount of non-current retention payables. [Refer: Retention payables]')
INSERT INTO @AT VALUES(173,0,1,'1221244', '/1/2/2/1/2/6/', NULL,N'OtherNoncurrentPayables', N'Other non-current payables',N'The amount of non-current payables that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(174,0,1,'12213', '/1/2/2/1/3/', NULL,N'DeferredTaxLiabilities', N'Deferred tax liabilities',N'The amounts of income taxes payable in future periods in respect of taxable temporary differences. [Refer: Temporary differences [member]]')
INSERT INTO @AT VALUES(175,0,1,'12214', '/1/2/2/1/4/', NULL,N'CurrentTaxLiabilitiesNoncurrent', N'Current tax liabilities, non-current',N'The non-current amount of current tax liabilities. [Refer: Current tax liabilities]')
INSERT INTO @AT VALUES(176,0,1,'12215', '/1/2/2/1/5/', NULL,N'OtherNoncurrentFinancialLiabilities', N'Other non-current financial liabilities',N'The amount of non-current financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(177,0,1,'122151', '/1/2/2/1/5/1/', NULL,N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLoss', N'Non-current financial liabilities at fair value through profit or loss',N'The amount of non-current financial liabilities measured at fair value through profit or loss. [Refer: Financial liabilities at fair value through profit or loss]')
INSERT INTO @AT VALUES(178,0,1,'1221511', '/1/2/2/1/5/1/1/', NULL,N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading', N'Non-current financial liabilities at fair value through profit or loss, classified as held for trading',N'The amount of non-current financial liabilities at fair value through profit or loss that meet the definition of held for trading. [Refer: Non-current financial liabilities at fair value through profit or loss]')
INSERT INTO @AT VALUES(179,0,1,'1221512', '/1/2/2/1/5/1/2/', NULL,N'NoncurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition', N'Non-current financial liabilities at fair value through profit or loss, designated upon initial recognition or subsequently',N'The amount of non-current financial liabilities measured at fair value through profit or loss that were designated as such upon initial recognition or subsequently. [Refer: Financial liabilities at fair value through profit or loss, designated upon initial recognition or subsequently]')
INSERT INTO @AT VALUES(180,0,1,'122152', '/1/2/2/1/5/2/', NULL,N'NoncurrentFinancialLiabilitiesAtAmortisedCost', N'Non-current financial liabilities at amortised cost',N'The amount of non-current financial liabilities measured at amortised cost. [Refer: Financial liabilities at amortised cost]')
INSERT INTO @AT VALUES(181,0,0,'12216', '/1/2/2/1/6/', NULL,N'OtherNoncurrentNonfinancialLiabilities', N'Other non-current non-financial liabilities',N'The amount of non-current non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(182,0,0,'1222', '/1/2/2/2/', NULL,N'CurrentLiabilities', N'Current liabilities',N'The amount of liabilities that: (a) the entity expects to settle in its normal operating cycle; (b) the entity holds primarily for the purpose of trading; (c) are due to be settled within twelve months after the reporting period; or (d) the entity does not have an unconditional right to defer settlement for at least twelve months after the reporting period.')
INSERT INTO @AT VALUES(183,0,0,'12221', '/1/2/2/2/1/', NULL,N'CurrentProvisions', N'Current provisions',N'The amount of current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(184,0,1,'122211', '/1/2/2/2/1/1/', NULL,N'CurrentProvisionsForEmployeeBenefits', N'Current provisions for employee benefits',N'The amount of current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(185,0,0,'122212', '/1/2/2/2/1/2/', N'ChangesInOtherProvisions',N'OtherShorttermProvisions', N'Other current provisions',N'The amount of current provisions other than provisions for employee benefits. [Refer: Provisions]')
INSERT INTO @AT VALUES(186,0,0,'1222121', '/1/2/2/2/1/2/1/', N'ChangesInOtherProvisions',N'ShorttermWarrantyProvision', N'Current warranty provision',N'The amount of current provision for warranties. [Refer: Warranty provision]')
INSERT INTO @AT VALUES(187,0,1,'1222122', '/1/2/2/2/1/2/2/', N'ChangesInOtherProvisions',N'ShorttermRestructuringProvision', N'Current restructuring provision',N'The amount of current provision for restructuring. [Refer: Restructuring provision]')
INSERT INTO @AT VALUES(188,0,1,'1222123', '/1/2/2/2/1/2/3/', N'ChangesInOtherProvisions',N'ShorttermLegalProceedingsProvision', N'Current legal proceedings provision',N'The amount of current provision for legal proceedings. [Refer: Legal proceedings provision]')
INSERT INTO @AT VALUES(189,0,1,'1222124', '/1/2/2/2/1/2/4/', N'ChangesInOtherProvisions',N'CurrentRefundsProvision', N'Current refunds provision',N'The amount of current provision for refunds. [Refer: Refunds provision]')
INSERT INTO @AT VALUES(190,0,1,'1222125', '/1/2/2/2/1/2/5/', N'ChangesInOtherProvisions',N'ShorttermOnerousContractsProvision', N'Current onerous contracts provision',N'The amount of current provision for onerous contracts. [Refer: Onerous contracts provision]')
INSERT INTO @AT VALUES(191,0,1,'1222126', '/1/2/2/2/1/2/6/', N'ChangesInOtherProvisions',N'ShorttermProvisionForDecommissioningRestorationAndRehabilitationCosts', N'Current provision for decommissioning, restoration and rehabilitation costs',N'The amount of current provision for decommissioning, restoration and rehabilitation costs. [Refer: Provision for decommissioning, restoration and rehabilitation costs]')
INSERT INTO @AT VALUES(192,0,1,'1222127', '/1/2/2/2/1/2/7/', N'ChangesInOtherProvisions',N'ShorttermMiscellaneousOtherProvisions', N'Current miscellaneous other provisions',N'The amount of miscellaneous current other provisions. [Refer: Miscellaneous other provisions]')
INSERT INTO @AT VALUES(193,0,0,'12222', '/1/2/2/2/2/', NULL,N'TradeAndOtherCurrentPayables', N'Trade and other current payables',N'The amount of current trade payables and current other payables. [Refer: Current trade payables; Other current payables]')
INSERT INTO @AT VALUES(194,0,1,'122221', '/1/2/2/2/2/1/', NULL,N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'Current trade payables',N'The current amount of payment due to suppliers for goods and services used in entity''s business. [Refer: Current liabilities; Trade payables]')
INSERT INTO @AT VALUES(195,0,1,'122222', '/1/2/2/2/2/2/', NULL,N'TradeAndOtherCurrentPayablesToRelatedParties', N'Current payables to related parties',N'The amount of current payables due to related parties. [Refer: Related parties [member]; Payables to related parties]')
INSERT INTO @AT VALUES(196,0,0,'122224', '/1/2/2/2/2/4/', NULL,N'AccrualsAndDeferredIncomeClassifiedAsCurrent', N'Accruals and deferred income classified as current',N'The amount of accruals and deferred income classified as current. [Refer: Accruals and deferred income]')
INSERT INTO @AT VALUES(197,0,0,'1222241', '/1/2/2/2/2/4/1/', NULL,N'DeferredIncomeClassifiedAsCurrent', N'Deferred income classified as current',N'The amount of deferred income classified as current. [Refer: Deferred income]')
INSERT INTO @AT VALUES(198,0,0,'12222411', '/1/2/2/2/2/4/1/1/', NULL,N'RentDeferredIncomeClassifiedAsCurrent', N'Rent deferred income classified as current',N'The amount of deferred income arising on rental activity. [Refer: Deferred income]')
INSERT INTO @AT VALUES(199,0,1,'1222242', '/1/2/2/2/2/4/2/', NULL,N'AccrualsClassifiedAsCurrent', N'Accruals classified as current',N'The amount of accruals classified as current. [Refer: Accruals]')
INSERT INTO @AT VALUES(200,0,1,'12222421', '/1/2/2/2/2/4/2/1/', NULL,N'ShorttermEmployeeBenefitsAccruals', N'Short-term employee benefits accruals',N'The amount of accruals for employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services. [Refer: Accruals classified as current]')
INSERT INTO @AT VALUES(201,0,0,'122225', '/1/2/2/2/2/5/', NULL,N'CurrentBilledButNotIssuedExtension', N'Current billed but not delivered to trade customers',N'The amount invoiced but against which there was no good or service delivered to the customer')
INSERT INTO @AT VALUES(202,0,1,'122226', '/1/2/2/2/2/6/', NULL,N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Current payables on social security and taxes other than income tax',N'The amount of current payables on social security and taxes other than incomes tax. [Refer: Payables on social security and taxes other than income tax]')
INSERT INTO @AT VALUES(203,0,1,'12222601', '/1/2/2/2/2/6/1/', NULL,N'CurrentValueAddedTaxPayables', N'Current value added tax payables',N'The amount of current value added tax payables. [Refer: Value added tax payables]')
INSERT INTO @AT VALUES(204,0,1,'12222602', '/1/2/2/2/2/6/2/', NULL,N'CurrentExciseTaxPayables', N'Current excise tax payables',N'The amount of current excise tax payables. [Refer: Excise tax payables]')
INSERT INTO @AT VALUES(205,0,1,'12222603', '/1/2/2/2/2/6/3/', NULL,N'CurrentSocialSecurityPayablesExtension', N'Current Social Security payables',N'The amount of current social security payables')
INSERT INTO @AT VALUES(206,0,1,'12222604', '/1/2/2/2/2/6/4/', NULL,N'CurrentZakatPayablesExtension', N'Current Zakat payables',N'The amount of current zakat payables')
INSERT INTO @AT VALUES(207,0,1,'12222605', '/1/2/2/2/2/6/5/', NULL,N'CurrentEmployeeIncomeTaxPayablesExtension', N'Current Employee Income tax payables',N'The amount of current employee income tax payables')
INSERT INTO @AT VALUES(208,0,1,'12222606', '/1/2/2/2/2/6/6/', NULL,N'CurrentEmployeeStampTaxPayablesExtension', N'Current Employee Stamp tax payables',N'The amount of current employee stamp tax payables')
INSERT INTO @AT VALUES(209,0,1,'12222607', '/1/2/2/2/2/6/7/', NULL,N'ProvidentFundPayableExtension', N'Provident fund payable',N'')
INSERT INTO @AT VALUES(210,0,1,'12222608', '/1/2/2/2/2/6/8/', NULL,N'WithholdingTaxPayableExtension', N'Withholding tax payable',N'')
INSERT INTO @AT VALUES(211,0,1,'12222609', '/1/2/2/2/2/6/9/', NULL,N'CostSharingPayableExtension', N'Cost sharing payable',N'')
INSERT INTO @AT VALUES(212,0,1,'12222610', '/1/2/2/2/2/6/10/', NULL,N'DividendTaxPayableExtension', N'Dividend tax payable',N'')
INSERT INTO @AT VALUES(213,0,1,'12222611', '/1/2/2/2/2/6/11/', NULL,N'ProfitTaxPayableExtension', N'Business Profit tax payable',N'')
INSERT INTO @AT VALUES(214,0,1,'122227', '/1/2/2/2/2/7/', NULL,N'CurrentRetentionPayables', N'Current retention payables',N'The amount of current retention payables. [Refer: Retention payables]')
INSERT INTO @AT VALUES(215,0,1,'122228', '/1/2/2/2/2/8/', NULL,N'OtherCurrentPayables', N'Other current payables',N'The amount of current payables that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(216,0,1,'12223', '/1/2/2/2/3/', NULL,N'CurrentTaxLiabilitiesCurrent', N'Current tax liabilities, current',N'The current amount of current tax liabilities. [Refer: Current tax liabilities]')
INSERT INTO @AT VALUES(217,0,1,'12224', '/1/2/2/2/4/', NULL,N'OtherCurrentFinancialLiabilities', N'Other current financial liabilities',N'The amount of current financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities; Current financial liabilities]')
INSERT INTO @AT VALUES(218,0,1,'122241', '/1/2/2/2/4/1/', NULL,N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossAbstract', N'Current financial liabilities at fair value through profit or loss',N'The amount of current financial liabilities measured at fair value through profit or loss. [Refer: Financial liabilities at fair value through profit or loss]')
INSERT INTO @AT VALUES(219,0,1,'1222411', '/1/2/2/2/4/1/1/', NULL,N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossClassifiedAsHeldForTrading', N'Current financial liabilities at fair value through profit or loss, classified as held for trading',N'The amount of current financial liabilities at fair value through profit or loss that meet the definition of held for trading. [Refer: Current financial liabilities at fair value through profit or loss]')
INSERT INTO @AT VALUES(220,0,1,'1222412', '/1/2/2/2/4/1/2/', NULL,N'CurrentFinancialLiabilitiesAtFairValueThroughProfitOrLossDesignatedUponInitialRecognition', N'Current financial liabilities at fair value through profit or loss, designated upon initial recognition or subsequently',N'The amount of current financial liabilities measured at fair value through profit or loss that were designated as such upon initial recognition or subsequently. [Refer: Financial liabilities at fair value through profit or loss, designated upon initial recognition or subsequently]')
INSERT INTO @AT VALUES(221,0,1,'122242', '/1/2/2/2/4/2/', NULL,N'CurrentFinancialLiabilitiesAtAmortisedCost', N'Current financial liabilities at amortised cost',N'The amount of current financial liabilities measured at amortised cost. [Refer: Financial liabilities at amortised cost]')
INSERT INTO @AT VALUES(222,0,1,'12225', '/1/2/2/2/5/', NULL,N'OtherCurrentNonfinancialLiabilities', N'Other current non-financial liabilities',N'The amount of current non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(223,0,1,'12226', '/1/2/2/2/6/', NULL,N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale', N'Liabilities included in disposal groups classified as held for sale',N'The amount of liabilities included in disposal groups classified as held for sale. [Refer: Liabilities; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(224,0,0,'2', '/2/', NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'')
INSERT INTO @AT VALUES(225,0,0,'21', '/2/1/', NULL,N'ProfitLoss', N'Profit (loss)',N'The total of income less expenses from continuing and discontinued operations, excluding the components of other comprehensive income. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(226,0,0,'211', '/2/1/1/', NULL,N'ProfitLossFromContinuingOperations', N'Profit (loss) from continuing operations',N'The profit (loss) from continuing operations. [Refer: Continuing operations [member]; Profit (loss)]')
INSERT INTO @AT VALUES(227,0,0,'2111', '/2/1/1/1/', NULL,N'ProfitLossBeforeTax', N'Profit (loss) before tax',N'The profit (loss) before tax expense or income. [Refer: Profit (loss)]')
INSERT INTO @AT VALUES(228,0,0,'211101', '/2/1/1/1/1/', NULL,N'ProfitLossFromOperatingActivities', N'Profit (loss) from operating activities',N'The profit (loss) from operating activities of the entity. [Refer: Profit (loss)]')
INSERT INTO @AT VALUES(229,0,0,'2111011', '/2/1/1/1/1/1/', NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(230,0,0,'21110111', '/2/1/1/1/1/1/1/', NULL,N'RevenueFromSaleOfGoods', N'Revenue from sales of merchandise',N'The amount of revenue arising from the sale of goods. [Refer: Revenue]')
INSERT INTO @AT VALUES(231,0,0,'211101111', '/2/1/1/1/1/1/1/1/', NULL,N'RevenueFromSaleOfFoodAndBeverage', N'Revenue from sale of food and beverage',N'The amount of revenue arising from the sale of food and beverage. [Refer: Revenue]')
INSERT INTO @AT VALUES(232,0,0,'211101112', '/2/1/1/1/1/1/1/2/', NULL,N'RevenueFromSaleOfAgriculturalProduce', N'Revenue from sale of agricultural produce',N'The amount of revenue arising from the sale of agricultural produce. [Refer: Revenue]')
INSERT INTO @AT VALUES(233,0,0,'21110112', '/2/1/1/1/1/1/2/', NULL,N'RevenueFromRenderingOfServices', N'Revenue from rendering of services',N'The amount of revenue arising from the rendering of services. [Refer: Revenue]')
INSERT INTO @AT VALUES(234,0,0,'21110113', '/2/1/1/1/1/1/3/', NULL,N'RevenueFromConstructionContracts', N'Revenue from construction contracts',N'The amount of revenue arising from construction contracts. Construction contracts are contracts specifically negotiated for the construction of an asset or a combination of assets that are closely interrelated or interdependent in terms of their design, technology and function or their ultimate purpose or use. [Refer: Revenue]')
INSERT INTO @AT VALUES(235,0,0,'21110114', '/2/1/1/1/1/1/4/', NULL,N'RevenueFromRoyalties', N'Royalty income',N'The amount of income arising from royalties.')
INSERT INTO @AT VALUES(236,0,0,'21110115', '/2/1/1/1/1/1/5/', NULL,N'LicenceFeeIncome', N'Licence fee income',N'The amount of income arising from licence fees.')
INSERT INTO @AT VALUES(237,0,0,'21110116', '/2/1/1/1/1/1/6/', NULL,N'FranchiseFeeIncome', N'Franchise fee income',N'The amount of income arising from franchise fees.')
INSERT INTO @AT VALUES(238,0,0,'21110117', '/2/1/1/1/1/1/7/', NULL,N'RevenueFromInterest', N'Interest income',N'The amount of income arising from interest.')
INSERT INTO @AT VALUES(239,0,0,'21110118', '/2/1/1/1/1/1/8/', NULL,N'RevenueFromDividends', N'Dividend income',N'The amount of dividends recognised as income. Dividends are distributions of profits to holders of equity investments in proportion to their holdings of a particular class of capital.')
INSERT INTO @AT VALUES(240,0,0,'21110119', '/2/1/1/1/1/1/9/', NULL,N'OtherRevenue', N'Other revenue',N'The amount of revenue arising from sources that the entity does not separately disclose in the same statement or note. [Refer: Revenue]')
INSERT INTO @AT VALUES(241,0,0,'2111012', '/2/1/1/1/1/2/', NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(242,0,0,'2111013', '/2/1/1/1/1/3/', NULL,N'ChangesInInventoriesOfFinishedGoodsAndWorkInProgress', N'Decrease (increase) in inventories of finished goods and work in progress',N'The decrease (increase) in inventories of finished goods and work in progress. [Refer: Inventories; Current finished goods; Current work in progress]')
INSERT INTO @AT VALUES(243,0,0,'2111014', '/2/1/1/1/1/4/', NULL,N'OtherWorkPerformedByEntityAndCapitalised', N'Other work performed by entity and capitalised',N'The amount of the entity''s own work capitalised from items originally classified as costs that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(244,0,0,'2111015', '/2/1/1/1/1/5/', NULL,N'ExpenseByNature', N'Expenses by nature',N'The amount of expenses aggregated according to their nature (for example, depreciation, purchases of materials, transport costs, employee benefits and advertising costs), and not reallocated among functions within the entity.')
INSERT INTO @AT VALUES(245,0,0,'21110151', '/2/1/1/1/1/5/1/', NULL,N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used',N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(246,0,0,'21110152', '/2/1/1/1/1/5/2/', NULL,N'CostOfMerchandiseSold', N'Cost of goods sold',N'The amount of merchandise that was sold during the period and recognised as an expense.')
INSERT INTO @AT VALUES(247,0,0,'21110153', '/2/1/1/1/1/5/3/', NULL,N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
INSERT INTO @AT VALUES(248,0,0,'211101531', '/2/1/1/1/1/5/3/1/', NULL,N'InsuranceExpense', N'Insurance expense',N'The amount of expense arising from purchased insurance.')
INSERT INTO @AT VALUES(249,0,0,'211101532', '/2/1/1/1/1/5/3/2/', NULL,N'ProfessionalFeesExpense', N'Professional fees expense',N'The amount of fees paid or payable for professional services.')
INSERT INTO @AT VALUES(250,0,0,'211101533', '/2/1/1/1/1/5/3/3/', NULL,N'TransportationExpense', N'Transportation expense',N'The amount of expense arising from transportation services.')
INSERT INTO @AT VALUES(251,0,0,'211101534', '/2/1/1/1/1/5/3/4/', NULL,N'BankAndSimilarCharges', N'Bank and similar charges',N'The amount of bank and similar charges recognised by the entity as an expense.')
INSERT INTO @AT VALUES(252,0,0,'211101535', '/2/1/1/1/1/5/3/5/', NULL,N'TravelExpense', N'Travel expense',N'The amount of expense arising from travel.')
INSERT INTO @AT VALUES(253,0,0,'211101536', '/2/1/1/1/1/5/3/6/', NULL,N'CommunicationExpense', N'Communication expense',N'The amount of expense arising from communication.')
INSERT INTO @AT VALUES(254,0,0,'211101537', '/2/1/1/1/1/5/3/7/', NULL,N'UtilitiesExpense', N'Utilities expense',N'The amount of expense arising from purchased utilities.')
INSERT INTO @AT VALUES(255,0,0,'211101538', '/2/1/1/1/1/5/3/8/', NULL,N'AdvertisingExpense', N'Advertising expense',N'The amount of expense arising from advertising.')
INSERT INTO @AT VALUES(256,0,0,'21110154', '/2/1/1/1/1/5/4/', NULL,N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(257,0,0,'211101541', '/2/1/1/1/1/5/4/1/', NULL,N'ShorttermEmployeeBenefitsExpense', N'Short-term employee benefits expense',N'The amount of expense from employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services.')
INSERT INTO @AT VALUES(258,0,0,'2111015411', '/2/1/1/1/1/5/4/1/1/', NULL,N'WagesAndSalaries', N'Wages and salaries',N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(259,0,0,'2111015412', '/2/1/1/1/1/5/4/1/2/', NULL,N'SocialSecurityContributions', N'Social security contributions',N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(260,0,0,'2111015413', '/2/1/1/1/1/5/4/1/3/', NULL,N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits',N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(261,0,0,'211101542', '/2/1/1/1/1/5/4/2/', NULL,N'PostemploymentBenefitExpenseDefinedContributionPlans', N'Post-employment benefit expense, defined contribution plans',N'The amount of post-employment benefit expense relating to defined contribution plans. Defined contribution plans are post-employment benefit plans under which an entity pays fixed contributions into a separate entity (a fund) and will have no legal or constructive obligation to pay further contributions if the fund does not hold sufficient assets to pay all employee benefits relating to employee service in the current and prior periods.')
INSERT INTO @AT VALUES(262,0,0,'211101543', '/2/1/1/1/1/5/4/3/', NULL,N'PostemploymentBenefitExpenseDefinedBenefitPlans', N'Post-employment benefit expense, defined benefit plans',N'The amount of post-employment benefit expense relating to defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(263,0,0,'211101544', '/2/1/1/1/1/5/4/4/', NULL,N'TerminationBenefitsExpense', N'Termination benefits expense',N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(264,0,0,'211101545', '/2/1/1/1/1/5/4/5/', NULL,N'OtherLongtermBenefits', N'Other long-term employee benefits',N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(265,0,0,'211101546', '/2/1/1/1/1/5/4/6/', NULL,N'OtherEmployeeExpense', N'Other employee expense',N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(266,0,0,'21110155', '/2/1/1/1/1/5/5/', NULL,N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Depreciation, amortisation and impairment loss (reversal of impairment loss) recognised in profit or loss',N'The amount of depreciation expense, amortisation expense and impairment loss (reversal of impairment loss) recognised in profit or loss. [Refer: Depreciation and amortisation expense; Impairment loss (reversal of impairment loss) recognised in profit or loss]')
INSERT INTO @AT VALUES(267,0,0,'211101551', '/2/1/1/1/1/5/5/1/', NULL,N'DepreciationAndAmortisationExpense', N'Depreciation and amortisation expense',N'The amount of depreciation and amortisation expense. Depreciation and amortisation are the systematic allocations of depreciable amounts of assets over their useful lives.')
INSERT INTO @AT VALUES(268,0,0,'2111015511', '/2/1/1/1/1/5/5/1/1/', NULL,N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
INSERT INTO @AT VALUES(269,0,0,'2111015512', '/2/1/1/1/1/5/5/1/2/', NULL,N'AmortisationExpense', N'Amortisation expense',N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
INSERT INTO @AT VALUES(270,0,0,'211101552', '/2/1/1/1/1/5/5/2/', NULL,N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Reversal of impairment loss (impairment loss) recognised in profit or loss',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(271,0,0,'21110155211', '/2/1/1/1/1/5/5/2/1/', NULL,N'WritedownsReversalsOfInventories', N'Write-downs (reversals of write-downs) of inventories',N'The amount recognised resulting from the write-down of inventories to net realisable value or reversals of those write-downs. [Refer: Inventories]')
INSERT INTO @AT VALUES(272,0,0,'2111015522', '/2/1/1/1/1/5/5/2/2/', NULL,N'WritedownsReversalsOfPropertyPlantAndEquipment', N'Write-downs (reversals of write-downs) of property, plant and equipment',N'The amount recognised resulting from the write-down of property, plant and equipment to its recoverable amount or reversals of those write-downs. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(273,0,0,'2111015523', '/2/1/1/1/1/5/5/2/3/', NULL,N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossTradeReceivables', N'Impairment loss (reversal of impairment loss) recognised in profit or loss, trade receivables',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss for trade receivables. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss; Trade receivables]')
INSERT INTO @AT VALUES(274,0,0,'2111015524', '/2/1/1/1/1/5/5/2/4/', NULL,N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossLoansAndAdvances', N'Impairment loss (reversal of impairment loss) recognised in profit or loss, loans and advances',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss for loans and advances. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(275,0,0,'21110157', '/2/1/1/1/1/5/7/', NULL,N'TaxExpenseOtherThanIncomeTaxExpense', N'Tax expense other than income tax expense',N'The amount of tax expense exclusive of income tax expense.')
INSERT INTO @AT VALUES(276,0,0,'21110159', '/2/1/1/1/1/5/9/', NULL,N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(277,0,0,'2111016', '/2/1/1/1/1/6/', NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(278,1,0,'21110162', '/2/1/1/1/1/6/2/', NULL,N'GainsLossesOnDisposalsOfPropertyPlantAndEquipment', N'Gain (loss) on disposal of property, plant and equipment',N'The gains (losses) on disposals of property, plant and equipment. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(279,1,0,'211101621', '/2/1/1/1/1/6/2/1/', NULL,N'GainsOnDisposalsOfPropertyPlantAndEquipment', N'Gains on disposals of property, plant and equipment',N'The gain on the disposal of property, plant and equipment. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(280,1,0,'211101622', '/2/1/1/1/1/6/2/2/', NULL,N'LossesOnDisposalsOfPropertyPlantAndEquipment', N'Losses on disposals of property, plant and equipment',N'The losses on the disposal of property, plant and equipment. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(281,1,0,'21110163', '/2/1/1/1/1/6/3/', NULL,N'GainsLossesOnDisposalsOfInvestmentProperties', N'Gains (losses) on disposals of investment properties',N'The gains (losses) on disposals of investment properties. [Refer: Investment property]')
INSERT INTO @AT VALUES(282,1,0,'211101631', '/2/1/1/1/1/6/3/1/', NULL,N'GainsOnDisposalsOfInvestmentProperties', N'Gains on disposals of investment properties',N'The gain on disposals of investment properties. [Refer: Investment property]')
INSERT INTO @AT VALUES(283,1,0,'211101632', '/2/1/1/1/1/6/3/2/', NULL,N'LossesOnDisposalsOfInvestmentProperties', N'Losses on disposals of investment properties',N'The losses on disposals of investment properties. [Refer: Investment property]')
INSERT INTO @AT VALUES(284,0,0,'21110164', '/2/1/1/1/1/6/4/', NULL,N'GainsLossesOnDisposalsOfInvestments', N'The gains (losses) on disposals of investments.',N'The gains (losses) on disposals of investments.')
INSERT INTO @AT VALUES(285,0,0,'211101641', '/2/1/1/1/1/6/4/1/', NULL,N'GainsOnDisposalsOfInvestments', N'Gains on disposals of investments',N'The gain on the disposal of investments.')
INSERT INTO @AT VALUES(286,0,0,'211101642', '/2/1/1/1/1/6/4/2/', NULL,N'LossesOnDisposalsOfInvestments', N'Losses on disposals of investments',N'The losses on the disposal of investments.')
INSERT INTO @AT VALUES(287,0,0,'21110165', '/2/1/1/1/1/6/5/', NULL,N'GainsLossesOnExchangeDifferencesOnTranslationRecognisedInProfitOrLoss', N'Foreign exchange gain (loss)',N'The amount of exchange differences recognised in profit or loss that arise from foreign currency transactions, excluding those arising on financial instruments measured at fair value through profit or loss in accordance with IFRS 9. [Refer: At fair value [member]; Financial instruments, class [member]]')
INSERT INTO @AT VALUES(288,0,0,'211101651', '/2/1/1/1/1/6/5/1/', NULL,N'NetForeignExchangeGain', N'Net foreign exchange gain',N'The net gain arising from foreign exchange differences. [Refer: Foreign exchange gain (loss)]')
INSERT INTO @AT VALUES(289,0,0,'211101652', '/2/1/1/1/1/6/5/2/', NULL,N'NetForeignExchangeLoss', N'Net foreign exchange loss',N'The net loss arising from foreign exchange differences. [Refer: Foreign exchange gain (loss)]')
INSERT INTO @AT VALUES(290,0,0,'211103', '/2/1/1/1/3/', NULL,N'GainsLossesOnNetMonetaryPosition', N'Gains (losses) on net monetary position',N'The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')
INSERT INTO @AT VALUES(291,0,0,'211104', '/2/1/1/1/4/', NULL,N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost', N'Gain (loss) arising from derecognition of financial assets measured at amortised cost',N'The gain (loss) arising from the derecognition of financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(292,0,0,'211105', '/2/1/1/1/5/', NULL,N'FinanceIncome', N'Finance income',N'The amount of income associated with interest and other financing activities of the entity.')
INSERT INTO @AT VALUES(293,0,0,'211106', '/2/1/1/1/6/', NULL,N'FinanceCosts', N'Finance costs',N'The amount of costs associated with financing activities of the entity.')
INSERT INTO @AT VALUES(294,0,0,'211107', '/2/1/1/1/7/', NULL,N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9', N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9',N'The amount of impairment loss, impairment gain or reversal of impairment loss that is recognised in profit or loss in accordance with paragraph 5.5.8 of IFRS 9 and that arises from applying the impairment requirements in Section 5.5 of IFRS 9.')
INSERT INTO @AT VALUES(295,0,0,'211108', '/2/1/1/1/8/', NULL,N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod', N'Share of profit (loss) of associates and joint ventures accounted for using equity method',N'The entity''s share of the profit (loss) of associates and joint ventures accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method; Joint ventures [member]; Profit (loss)]')
INSERT INTO @AT VALUES(296,0,0,'211109', '/2/1/1/1/9/', NULL,N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates', N'Other income (expense) from subsidiaries, jointly controlled entities and associates',N'The amount of income or expense from subsidiaries, jointly controlled entities and associates that the entity does not separately disclose in the same statement or note. [Refer: Associates [member]; Subsidiaries [member]]')
INSERT INTO @AT VALUES(297,0,0,'211110', '/2/1/1/1/10/', NULL,N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue', N'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category',N'The gains (losses) arising from the difference between the previous amortised cost and the fair value of financial assets reclassified out of the amortised cost into the fair value through profit or loss measurement category. [Refer: At fair value [member]; Financial assets at amortised cost]')
INSERT INTO @AT VALUES(298,0,0,'211111', '/2/1/1/1/11/', NULL,N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory', N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category',N'The cumulative gain (loss) previously recognised in other comprehensive income arising from the reclassification of financial assets out of the fair value through other comprehensive income into the fair value through profit or loss measurement category. [Refer: Financial assets measured at fair value through other comprehensive income; Financial assets at fair value through profit or loss; Other comprehensive income]')
INSERT INTO @AT VALUES(299,0,0,'211112', '/2/1/1/1/12/', NULL,N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions', N'Hedging gains (losses) for hedge of group of items with offsetting risk positions',N'The hedging gains (losses) for hedge of group of items with offsetting risk positions.')
INSERT INTO @AT VALUES(300,0,0,'2112', '/2/1/1/2/', NULL,N'IncomeTaxExpenseContinuingOperations', N'Tax income (expense)',N'The aggregate amount included in the determination of profit (loss) for the period in respect of current tax and deferred tax. [Refer: Current tax expense (income); Deferred tax expense (income)]')
INSERT INTO @AT VALUES(301,0,0,'212', '/2/1/2/', NULL,N'ProfitLossFromDiscontinuedOperations', N'Profit (loss) from discontinued operations',N'The profit (loss) from discontinued operations. [Refer: Discontinued operations [member]; Profit (loss)]')
INSERT INTO @AT VALUES(302,0,0,'3', '/3/', NULL,N'OtherComprehensiveIncome', N'Other comprehensive income',N'')
INSERT INTO @AT VALUES(303,0,0,'31', '/3/1/', NULL,N'ComponentsOfOtherComprehensiveIncomeThatWillNotBeReclassifiedToProfitOrLossBeforeTax', N'Components of other comprehensive income that will not be reclassified to profit or loss, before tax [abstract]',N'The amount of other comprehensive income that will not be reclassified to profit or loss, before tax. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(304,0,0,'311', '/3/1/1/', NULL,N'OtherComprehensiveIncomeBeforeTaxGainsLossesFromInvestmentsInEquityInstruments', N'Other comprehensive income, before tax, gains (losses) from investments in equity instruments',N'The amount of other comprehensive income, before tax, related to gains (losses) from changes in the fair value of investments in equity instruments that the entity has designated at fair value through other comprehensive income. [Refer: Other comprehensive income, before tax]')
INSERT INTO @AT VALUES(305,0,0,'312', '/3/1/2/', NULL,N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnRevaluation', N'Other comprehensive income, before tax, gains (losses) on revaluation',N'The amount of other comprehensive income, before tax, related to gains (losses) in relation to changes in the revaluation surplus. [Refer: Other comprehensive income, before tax; Revaluation surplus]')
INSERT INTO @AT VALUES(306,0,0,'313', '/3/1/3/', NULL,N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnRemeasurementsOfDefinedBenefitPlans', N'Other comprehensive income, before tax, gains (losses) on remeasurements of defined benefit plans',N'The amount of other comprehensive income, before tax, related to gains (losses) on remeasurements of defined benefit plans, which comprise actuarial gains and losses; the return on plan assets, excluding amounts included in net interest on the net defined benefit liability (asset); and any change in the effect of the asset ceiling, excluding amounts included in net interest on the net defined benefit liability (asset). [Refer: Other comprehensive income, before tax; Defined benefit plans [member]; Plan assets [member]; Net defined benefit liability (asset)]')
INSERT INTO @AT VALUES(307,0,0,'314', '/3/1/4/', NULL,N'OtherComprehensiveIncomeBeforeTaxChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability', N'Other comprehensive income, before tax, change in fair value of financial liability attributable to change in credit risk of liability',N'The amount of other comprehensive income, before tax, related to change in the fair value of financial liability attributable to change in the credit risk of the liability. [Refer: Other comprehensive income, before tax; Credit risk [member]]')
INSERT INTO @AT VALUES(308,0,0,'315', '/3/1/5/', NULL,N'OtherComprehensiveIncomeBeforeTaxGainsLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments', N'Other comprehensive income, before tax, gains (losses) on hedging instruments that hedge investments in equity instruments',N'The amount of other comprehensive income, before tax, related to gains (losses) on hedging instruments that hedge investments in equity instruments that the entity has designated at fair value through other comprehensive income. [Refer: Other comprehensive income, before tax]')
INSERT INTO @AT VALUES(309,0,0,'316', '/3/1/6/', NULL,N'OtherComprehensiveIncomeBeforeTaxInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss', N'Other comprehensive income, before tax, insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will not be reclassified to profit or loss',N'The amount of other comprehensive income, before tax, related to insurance finance income (expenses) from insurance contracts issued that will not be reclassified subsequently to profit or loss. [Refer: Insurance finance income (expenses); Insurance contracts issued [member]]')
INSERT INTO @AT VALUES(310,0,0,'317', '/3/1/7/', NULL,N'ShareOfOtherComprehensiveIncomeOfAssociatesAndJointVenturesAccountedForUsingEquityMethodThatWillNotBeReclassifiedToProfitOrLossBeforeTax', N'Share of other comprehensive income of associates and joint ventures accounted for using equity method that will not be reclassified to profit or loss, before tax',N'Share of the other comprehensive income of associates and joint ventures accounted for using the equity method that will not be reclassified to profit or loss, before tax.')
INSERT INTO @AT VALUES(311,0,0,'32', '/3/2/', NULL,N'ComponentsOfOtherComprehensiveIncomeThatWillBeReclassifiedToProfitOrLossBeforeTax', N'Components of other comprehensive income that will be reclassified to profit or loss, before tax [abstract]',N'The amount of other comprehensive income that will be reclassified to profit or loss, before tax. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(312,0,0,'321', '/3/2/1/', NULL,N'OtherComprehensiveIncomeBeforeTaxExchangeDifferencesOnTranslation', N'Exchange differences on translation',N'The amount of other comprehensive income, before tax, related to exchange differences on translation of financial statements of foreign operations. [Refer: Other comprehensive income, before tax]')
INSERT INTO @AT VALUES(313,0,0,'3211', '/3/2/1/1/', NULL,N'GainsLossesOnExchangeDifferencesOnTranslationBeforeTax', N'Gains (losses) on exchange differences on translation, before tax',N'The gains (losses) recognised in other comprehensive income on exchange differences on the translation of financial statements of foreign operations, before tax. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(314,0,0,'3212', '/3/2/1/2/', NULL,N'ReclassificationAdjustmentsOnExchangeDifferencesOnTranslationBeforeTax', N'Reclassification adjustments on exchange differences on translation, before tax',N'The amount of reclassification adjustments related to exchange differences when the financial statements of foreign operations are translated, before tax. Reclassification adjustments are amounts reclassified to profit (loss) in the current period that were recognised in other comprehensive income in the current or previous periods. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(315,0,0,'322', '/3/2/2/', NULL,N'OtherComprehensiveIncomeBeforeTaxAvailableforsaleFinancialAssets', N'Available-for-sale financial assets',N'The amount of other comprehensive income, before tax, related to available-for-sale financial assets. [Refer: Financial assets available-for-sale; Other comprehensive income, before tax]')
INSERT INTO @AT VALUES(316,0,0,'3221', '/3/2/2/1/', NULL,N'GainsLossesOnRemeasuringAvailableforsaleFinancialAssetsBeforeTax', N'Gains (losses) on remeasuring available-for-sale financial assets, before tax',N'The gains (losses) recognised in other comprehensive income on remeasuring available-for-sale financial assets, before tax. [Refer: Financial assets available-for-sale]')
INSERT INTO @AT VALUES(317,0,0,'3222', '/3/2/2/2/', NULL,N'ReclassificationAdjustmentsOnAvailableforsaleFinancialAssetsBeforeTax', N'Reclassification adjustments on available-for-sale financial assets, before tax',N'The amount of reclassification adjustments related to available-for-sale financial assets, before tax. Reclassification adjustments are amounts reclassified to profit (loss) in the current period that were recognised in other comprehensive income in the current or previous periods. [Refer: Financial assets available-for-sale; Other comprehensive income]')
INSERT INTO @AT VALUES(318,0,0,'4', '/4/', NULL,N'ControlAccountsExtension', N'Control accounts',N'')
INSERT INTO @AT VALUES(319,0,0,'41', '/4/1/', NULL,N'TradersControlAccountsExtension', N'Traders control accounts',N'')
INSERT INTO @AT VALUES(320,0,0,'411', '/4/1/1/', NULL,N'SuppliersControlAccountsExtension', N'Suppliers control accounts',N'')
INSERT INTO @AT VALUES(321,0,1,'4111', '/4/1/1/1/', NULL,N'CashPaymentsToSuppliersControlExtension', N'Cash payments to suppliers control',N'')
INSERT INTO @AT VALUES(322,0,0,'4112', '/4/1/1/2/', NULL,N'GoodsAndServicesReceivedFromSuppliersControlExtensions', N'Goods/services received from suppliers control',N'')
INSERT INTO @AT VALUES(323,0,0,'412', '/4/1/2/', NULL,N'CustomersControlAccountsExtension', N'Customer control accounts',N'')
INSERT INTO @AT VALUES(324,0,1,'4121', '/4/1/2/1/', NULL,N'CashReceiptsFromCustomersControlExtension', N'Cash receipts from customers control',N'')
INSERT INTO @AT VALUES(325,0,0,'4122', '/4/1/2/2/', NULL,N'GoodsAndServicesIssuedToCustomersControlExtension', N'Goods/Services delivered to customers control',N'')
INSERT INTO @AT VALUES(326,0,0,'413', '/4/1/3/', NULL,N'PayrollControlExtension', N'Payroll control',N'')
INSERT INTO @AT VALUES(327,0,1,'4131', '/4/1/3/1/', NULL,N'CashPaymentsToEmployeesControlExtension', N'Cash payments to employees control',N'')
INSERT INTO @AT VALUES(328,0,0,'419', '/4/1/9/', NULL,N'OthersAccountsControlExtension', N'Others control accounts',N'')
INSERT INTO @AT VALUES(329,0,1,'4191', '/4/1/9/1/', NULL,N'CashPaymentsToOthersControlExtension', N'Cash payments to others control',N'')
INSERT INTO @AT VALUES(330,0,1,'4192', '/4/1/9/2/', NULL,N'CashReceiptsFromOthersControlExtension', N'Cash receipts from others control',N'')
INSERT INTO @AT VALUES(331,0,1,'42', '/4/2/', NULL,N'GuaranteesExtension', N'Guarantees',N'')
INSERT INTO @AT VALUES(332,0,1,'421', '/4/2/1/', NULL,N'CollectionGuaranteeExtension', N'Collection Guarantee',N'e.g., checks, LG, ..')
INSERT INTO @AT VALUES(333,0,1,'422', '/4/2/2/', NULL,N'DishonouredGuaranteeExtension', N'Dishonoured Guarantee',N'e.g., dishonored check, invalid LG, etc')
INSERT INTO @AT VALUES(334,0,1,'5', '/5/', NULL,N'MigrationAccountsExtension', N'Migration accounts',N'')

INSERT INTO @AccountTypes ([Index], [Code], [Concept], [Name], [ParentIndex], [AllowsPureUnit], [IsMonetary],
		[EntryTypeParentId], [Description])
SELECT RC.[Index], RC.[Code], RC.[Concept], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [AllowsPureUnit], [IsMonetary],
		(SELECT [Id] FROM dbo.EntryTypes WHERE [Concept] = RC.EntryTypeParentConcept), [Description]
FROM @AT RC;
UPDATE @AccountTypes SET IsAssignable = 1
WHERE [Index] NOT IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)
UPDATE @AccountTypes SET IsAssignable = 0
WHERE [Index] IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)

	--[Time1Label], [Time1Label2], [Time1Label3],
	--[Time2Label], [Time2Label2], [Time2Label3],
	--[ExternalReferenceLabel], [ExternalReferenceLabel2], [ExternalReferenceLabel3], 
	--[AdditionalReferenceLabel], [AdditionalReferenceLabel2], [AdditionalReferenceLabel3],
	--[NotedAgentNameLabel], [NotedAgentNameLabel2], [NotedAgentNameLabel3],
	--[NotedAmountLabel], [NotedAmountLabel2], [NotedAmountLabel3],
	--[NotedDateLabel], [NotedDateLabel2], [NotedDateLabel3]

EXEC [api].[AccountTypes__Save]
	@Entities = @AccountTypes,
	@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
	@AccountTypeCustodyDefinitions = @AccountTypeCustodyDefinitions,
	@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Account Types: Provisioning: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF (1=1) -- Declarations
BEGIN
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
DECLARE @GoodsAndServicesReceivedFromSuppliersControlExtensions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GoodsAndServicesReceivedFromSuppliersControlExtensions');
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
END

DELETE FROM @AccountTypes
INSERT INTO @AccountTypes(
	[Index], [ParentIndex], [Id], [ParentId],
	[Code],
	[Concept],
	[Name], [Name2], [Name3],
	[Description], [Description2], [Description3], 
	[IsMonetary],
	[IsAssignable],
	[AllowsPureUnit],
	[EntryTypeParentId],
	[Time1Label], [Time1Label2], [Time1Label3],
	[Time2Label], [Time2Label2], [Time2Label3],
	[ExternalReferenceLabel], [ExternalReferenceLabel2], [ExternalReferenceLabel3], 
	[AdditionalReferenceLabel], [AdditionalReferenceLabel2], [AdditionalReferenceLabel3],
	[NotedAgentNameLabel], [NotedAgentNameLabel2], [NotedAgentNameLabel3],
	[NotedAmountLabel], [NotedAmountLabel2], [NotedAmountLabel3],
	[NotedDateLabel], [NotedDateLabel2], [NotedDateLabel3]
)
SELECT
	[Id], [ParentId], [Id], [ParentId],
	[Code],
	[Concept],
	[Name], [Name2], [Name3],
	[Description], [Description2], [Description3], 
	[IsMonetary],
	[IsAssignable],
	[AllowsPureUnit],
	[EntryTypeParentId],
	[Time1Label], [Time1Label2], [Time1Label3],
	[Time2Label], [Time2Label2], [Time2Label3],
	[ExternalReferenceLabel], [ExternalReferenceLabel2], [ExternalReferenceLabel3], 
	[AdditionalReferenceLabel], [AdditionalReferenceLabel2], [AdditionalReferenceLabel3],
	[NotedAgentNameLabel], [NotedAgentNameLabel2], [NotedAgentNameLabel3],
	[NotedAmountLabel], [NotedAmountLabel2], [NotedAmountLabel3],
	[NotedDateLabel], [NotedDateLabel2], [NotedDateLabel3]
FROM dbo.AccountTypes

INSERT INTO @AccountTypeResourceDefinitions([Index],
[HeaderIndex],											[ResourceDefinitionId]) VALUES
(0,@Land,												@LandMemberRD),
(1,@Buildings,											@BuildingsMemberRD),
(2,@Machinery,											@MachineryMemberRD), 
(3,@Machinery,											@PowerGeneratingAssetsMemberRD), 
(4,@MotorVehicles,										@MotorVehiclesMemberRD),
(5,@FixturesAndFittings,								@FixturesAndFittingsMemberRD),
(6,@FixturesAndFittings,								@NetworkInfrastructureMemberRD),
(7,@FixturesAndFittings,								@LeaseholdImprovementsMemberRD),
(8,@OfficeEquipment,									@OfficeEquipmentMemberRD),
(9,@OfficeEquipment,									@ComputerEquipmentMemberRD),
(10,@OfficeEquipment,									@CommunicationAndNetworkEquipmentMemberRD),
(11,@BearerPlants,										@BearerPlantsMemberRD),
(12,@TangibleExplorationAndEvaluationAssets,			@TangibleExplorationAndEvaluationAssetsMemberRD),
(13,@MiningAssets,										@MiningAssetsMemberRD),
(14,@OilAndGasAssets,									@OilAndGasAssetsMemberRD),
(15,@ConstructionInProgress,							@ConstructionInProgressMemberRD),
(16,@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel,
														@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(17,@OtherPropertyPlantAndEquipment,					@OtherPropertyPlantAndEquipmentMemberRD),

(117,@NoncurrentLoansAndReceivables,@EmployeeLoanRD),
(18,@InvestmentPropertyCompleted,						@InvestmentPropertyCompletedMemberRD),
(19,@InvestmentPropertyUnderConstructionOrDevelopment,	@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),

(119,@NoncurrentTradeReceivables,@TradeReceivableRD),
(120,@NoncurrentReceivablesDueFromRelatedParties,@TradeReceivableRD),
(121,@NoncurrentPrepayments,@PrepaymentsRD),
(122,@NoncurrentAccruedIncome,@AccruedIncomeRD),
(123,@NoncurrentReceivablesFromSaleOfProperties,@ReceivablesFromSaleOfPropertiesRD),
(124,@NoncurrentReceivablesFromRentalOfProperties,@ReceivablesFromRentalOfPropertiesRD),

(20,@Merchandise,										@MerchandiseRD),
(21,@Merchandise,										@TradeMedicineRD),
(22,@Merchandise,										@TradeConstructionMaterialRD),
(23,@Merchandise,										@TradeSparePartRD),
(24,@CurrentFoodAndBeverage,							@CurrentFoodAndBeverageRD),
(25,@CurrentAgriculturalProduce,						@CurrentAgriculturalProduceRD),
(26,@FinishedGoods,										@FinishedGoodsRD),
(27,@FinishedGoods,										@FinishedGrainRD),
(28,@FinishedGoods,										@FinishedVehicleRD),
(29,@FinishedGoods,										@FinishedOilRD),
(30,@FinishedGoods,										@ByproductGrainRD),
(31,@FinishedGoods,										@ByproductOilRD),
(32,@PropertyIntendedForSaleInOrdinaryCourseOfBusiness,@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(33,@WorkInProgress,									@WorkInProgressRD),
(34,@RawMaterials,										@RawMaterialsRD),
(35,@RawMaterials,										@RawGrainRD),
(36,@RawMaterials,										@RawVehicleRD),
(37,@ProductionSupplies,								@ProductionSuppliesRD),
(38,@CurrentPackagingAndStorageMaterials,				@CurrentPackagingAndStorageMaterialsRD),
(39,@SpareParts,										@SparePartsRD),
(40,@CurrentFuel,										@CurrentFuelRD),
(41,@OtherInventories,									@OtherInventoriesRD),

(146,@CurrentTradeReceivables,@TradeReceivableRD),
(147,@TradeAndOtherCurrentReceivablesDueFromRelatedParties,@TradeReceivableRD),
(148,@CurrentPrepayments,@PrepaymentsRD),
(149,@CurrentAccruedIncome,@AccruedIncomeRD),
(150,@CurrentReceivablesFromRentalOfProperties,@ReceivablesFromRentalOfPropertiesRD),
(151,@OtherCurrentFinancialAssets,@EmployeeLoanRD),

(42,@CashOnHand,										@CheckReceivedRD), -- for checks to be deposited

(153,@LongtermWarrantyProvision,@WarrantyProvisionRD),
(154,@LongtermRestructuringProvision,@RestructuringProvisionRD),
(155,@NoncurrentRefundsProvision,@RefundsProvisionRD),
(156,@NoncurrentPayablesToTradeSuppliers,@TradePayableRD),
(157,@NoncurrentPayablesToRelatedParties,@TradePayableRD),
(158,@DeferredIncomeClassifiedAsNoncurrent,@DeferredIncomeRD),
(159,@RentDeferredIncomeClassifiedAsNoncurrent,@RentDeferredIncomeRD),
(160,@AccrualsClassifiedAsNoncurrent,@AccrualsRD),
(161,@ShorttermWarrantyProvision,@WarrantyProvisionRD),
(162,@ShorttermRestructuringProvision,@RestructuringProvisionRD),
(163,@CurrentRefundsProvision,@RefundsProvisionRD),
(164,@TradeAndOtherCurrentPayablesToTradeSuppliers,@TradePayableRD),
(165,@TradeAndOtherCurrentPayablesToRelatedParties,@TradePayableRD),
(166,@DeferredIncomeClassifiedAsCurrent,@DeferredIncomeRD),
(167,@RentDeferredIncomeClassifiedAsCurrent,@RentDeferredIncomeRD),
(168,@AccrualsClassifiedAsCurrent,@AccrualsRD),
(169,@ShorttermEmployeeBenefitsAccruals,@AccrualsRD),
(170,@CurrentBilledButNotIssuedExtension,@CurrentBilledButNotIssuedRD),
(171,@CurrentRetentionPayables,@RetentionPayableRD),

(46,@RevaluationSurplus,								@LandMemberRD),
(47,@RevaluationSurplus,								@BuildingsMemberRD),
(48,@RevaluationSurplus,								@MachineryMemberRD), 
(49,@RevaluationSurplus,								@PowerGeneratingAssetsMemberRD), 
(50,@RevaluationSurplus,								@MotorVehiclesMemberRD),
(51,@RevaluationSurplus,								@FixturesAndFittingsMemberRD),
(52,@RevaluationSurplus,								@NetworkInfrastructureMemberRD),
(53,@RevaluationSurplus,								@LeaseholdImprovementsMemberRD),
(54,@RevaluationSurplus,								@OfficeEquipmentMemberRD),
(55,@RevaluationSurplus,								@ComputerEquipmentMemberRD),
(56,@RevaluationSurplus,								@CommunicationAndNetworkEquipmentMemberRD),
(57,@RevaluationSurplus,								@ComputerEquipmentMemberRD),
(58,@RevaluationSurplus,								@BearerPlantsMemberRD),
(59,@RevaluationSurplus,								@TangibleExplorationAndEvaluationAssetsMemberRD),
(60,@RevaluationSurplus,								@MiningAssetsMemberRD),
(61,@RevaluationSurplus,								@OilAndGasAssetsMemberRD),
(62,@RevaluationSurplus,								@ConstructionInProgressMemberRD),
(63,@RevaluationSurplus,								@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(64,@RevaluationSurplus,								@OtherPropertyPlantAndEquipmentMemberRD),
(65,@RevaluationSurplus,								@InvestmentPropertyCompletedMemberRD),
(66,@RevaluationSurplus,								@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),

(67,@RevenueFromSaleOfGoods,							@MerchandiseRD),
(68,@RevenueFromSaleOfGoods,							@TradeMedicineRD),
(69,@RevenueFromSaleOfGoods,							@TradeConstructionMaterialRD),
(70,@RevenueFromSaleOfGoods,							@TradeSparePartRD),
(73,@RevenueFromSaleOfGoods,							@FinishedGoodsRD),
(74,@RevenueFromSaleOfGoods,							@FinishedGrainRD),
(75,@RevenueFromSaleOfGoods,							@FinishedVehicleRD),
(76,@RevenueFromSaleOfGoods,							@FinishedOilRD),
(77,@RevenueFromSaleOfGoods,							@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),

(181,@RevenueFromSaleOfFoodAndBeverage,@CurrentFoodAndBeverageRD),
(182,@RevenueFromSaleOfAgriculturalProduce,@CurrentAgriculturalProduceRD),

(78,@RevenueFromRenderingOfServices,					@RevenueServiceRD),

(79,@OtherRevenue,										@ByproductGrainRD),
(80,@OtherRevenue,										@ByproductOilRD),

--(81,@RevenueFromDividends,							@InvestmentAccountedForUsingEquityMethodRD),
--(82,@RevenueFromDividends,							@InvestmentsInSubsidiariesJointVenturesAndAssociatesRD),
(83,@RawMaterialsAndConsumablesUsed,					@RawMaterialsRD),

(84,@CostOfMerchandiseSold,								@CurrentAgriculturalProduceRD),
(85,@CostOfMerchandiseSold,								@CurrentFoodAndBeverageRD),
(86,@CostOfMerchandiseSold,								@FinishedGoodsRD),
(87,@CostOfMerchandiseSold,								@FinishedGrainRD),
(88,@CostOfMerchandiseSold,								@FinishedOilRD),
(89,@CostOfMerchandiseSold,								@FinishedVehicleRD),
(90,@CostOfMerchandiseSold,								@MerchandiseRD),
(91,@CostOfMerchandiseSold,								@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(92,@CostOfMerchandiseSold,								@TradeConstructionMaterialRD),
(93,@CostOfMerchandiseSold,								@TradeMedicineRD),
(94,@CostOfMerchandiseSold,								@TradeSparePartRD),

(105,@EmployeeBenefitsExpense,							@EmployeeBenefitRD),
(106,@ShorttermEmployeeBenefitsExpense,					@EmployeeBenefitRD),
(107,@WagesAndSalaries,									@EmployeeBenefitRD),
(108,@SocialSecurityContributions,						@EmployeeBenefitRD), 
(109,@OtherShorttermEmployeeBenefits,					@EmployeeBenefitRD), 
(100,@PostemploymentBenefitExpenseDefinedContributionPlans,	
														@EmployeeBenefitRD),
(101,@PostemploymentBenefitExpenseDefinedBenefitPlans,	@EmployeeBenefitRD), 
(102,@TerminationBenefitsExpense,						@EmployeeBenefitRD),
(103,@OtherLongtermBenefits,							@EmployeeBenefitRD), 
(104,@OtherEmployeeExpense,								@EmployeeBenefitRD),

(113,@CollectionGuaranteeExtension,						@CheckReceivedRD),
(114,@DishonouredGuaranteeExtension,					@CheckReceivedRD);

INSERT INTO @AccountTypeCustodyDefinitions([Index],
[HeaderIndex],										[CustodyDefinitionId]) VALUES
(0,@MotorVehicles,									@RentalCD),
(1,@MotorVehicles,									@PPECustodyCD),
(2,@OfficeEquipment,								@PPECustodyCD),
(3,@InvestmentPropertyCompleted,					@RentalCD),
(7,@Merchandise,									@WarehouseCD),
(8,@CurrentFoodAndBeverage,							@WarehouseCD),
(9,@CurrentAgriculturalProduce,						@WarehouseCD),
(10,@FinishedGoods,									@WarehouseCD),
(11,@WorkInProgress,								@WarehouseCD),
(12,@RawMaterials,									@WarehouseCD),
(13,@ProductionSupplies,							@WarehouseCD),
(14,@CurrentPackagingAndStorageMaterials,			@WarehouseCD),
(15,@SpareParts,									@WarehouseCD),
(16,@CurrentFuel,									@WarehouseCD),
(116,@CurrentInventoriesInTransit,					@ShipperCD),
(17,@OtherInventories,								@WarehouseCD),
(25,@CurrentReceivablesFromRentalOfProperties,		@RentalCD),
(27,@CashOnHand,									@SafeCD),
(28,@BalancesWithBanks,								@BankAccountCD),
(29,@RevenueFromSaleOfGoods,						@WarehouseCD),
(30,@RevenueFromSaleOfFoodAndBeverage,				@WarehouseCD),
(31,@RevenueFromSaleOfAgriculturalProduce,			@WarehouseCD),
(32,@CostOfMerchandiseSold,							@WarehouseCD),
(44,@CollectionGuaranteeExtension,					@SafeCD),
(45,@DishonouredGuaranteeExtension,					@SafeCD);

INSERT INTO @AccountTypeNotedRelationDefinitions([Index],
[HeaderIndex],										[NotedRelationDefinitionId]) VALUES
(0,@NoncurrentValueAddedTaxReceivables,				@SupplierCD),
(1,@CurrentValueAddedTaxReceivables,				@SupplierCD),
(2,@WithholdingTaxReceivablesExtension,				@CustomerCD),
(3,@CurrentReceivablesFromRentalOfProperties,		@CustomerCD),
(4,@CurrentValueAddedTaxPayables,					@CustomerCD),
(5,@CurrentSocialSecurityPayablesExtension,			@EmployeeCD),
(6,@CurrentEmployeeIncomeTaxPayablesExtension,		@EmployeeCD),
(7,@CurrentEmployeeStampTaxPayablesExtension,		@EmployeeCD),
(8,@ProvidentFundPayableExtension,					@EmployeeCD),
(9,@WithholdingTaxPayableExtension,					@SupplierCD),
(10,@CostSharingPayableExtension,					@EmployeeCD),
(11,@DividendTaxPayableExtension,					@PartnerCD),

(103,@NoncurrentValueAddedTaxPayables,@CustomerCD),
(106,@CurrentZakatPayablesExtension,@EmployeeCD),

(12,@RevenueFromSaleOfGoods,						@CustomerCD),
(13,@RevenueFromSaleOfFoodAndBeverage,				@CustomerCD),
(14,@RevenueFromSaleOfAgriculturalProduce,			@CustomerCD),
(15,@RevenueFromRenderingOfServices,				@CustomerCD),
(16,@OtherRevenue,									@CustomerCD),
(17,@CostOfMerchandiseSold,							@CustomerCD),
(18,@InsuranceExpense,								@SupplierCD),
(19,@ProfessionalFeesExpense,						@SupplierCD),
(20,@TransportationExpense,							@SupplierCD),
(21,@BankAndSimilarCharges,							@SupplierCD),
(22,@TravelExpense,									@SupplierCD),
(23,@CommunicationExpense,							@SupplierCD),
(24,@UtilitiesExpense,								@SupplierCD),
(25,@AdvertisingExpense,							@SupplierCD),
(26,@WagesAndSalaries,								@EmployeeCD),
(27,@SocialSecurityContributions,					@EmployeeCD),
(28,@OtherShorttermEmployeeBenefits,				@EmployeeCD),
(29,@PostemploymentBenefitExpenseDefinedContributionPlans,
													@EmployeeCD),
(30,@PostemploymentBenefitExpenseDefinedBenefitPlans,@EmployeeCD),
(31,@TerminationBenefitsExpense,					@EmployeeCD),
(32,@OtherLongtermBenefits,							@EmployeeCD),
(33,@OtherEmployeeExpense,							@EmployeeCD),


(35,@CashPaymentsToSuppliersControlExtension,@SupplierCD),
(36,@GoodsAndServicesReceivedFromSuppliersControlExtensions,@SupplierCD),
(37,@CashReceiptsFromCustomersControlExtension,@CustomerCD),
(38,@GoodsAndServicesIssuedToCustomersControlExtension,@CustomerCD),
(39,@CashPaymentsToEmployeesControlExtension,@EmployeeCD);

EXEC [api].[AccountTypes__Save]
	@Entities = @AccountTypes,
	@AccountTypeResourceDefinitions = @AccountTypeResourceDefinitions,
	@AccountTypeCustodyDefinitions = @AccountTypeCustodyDefinitions,
	@AccountTypeNotedRelationDefinitions = @AccountTypeNotedRelationDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Account Types: Provisioning with weak entities: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--DELETE FROM @IndexedIds;
--INSERT INTO @IndexedIds([Index], [Id]) SELECT ROW_NUMBER() OVER(ORDER BY [Id]), [Id]  FROM dbo.AccountTypes WHERE [Concept] NOT IN (
--N'StatementOfFinancialPositionAbstract',
--N'Assets',
--N'NoncurrentAssets',
--N'PropertyPlantAndEquipment',
--N'OfficeEquipment',
--N'NoncurrentReceivables',
--N'NoncurrentTradeReceivables',
--N'NoncurrentReceivablesDueFromRelatedParties',
--N'NoncurrentPrepaymentsAndNoncurrentAccruedIncome',
--N'NoncurrentPrepayments',
--N'NoncurrentAccruedIncome',
--N'NoncurrentReceivablesFromTaxesOtherThanIncomeTax',
--N'OtherNoncurrentReceivables',
--N'CurrentAssets',
--N'Inventories',
--N'WorkInProgress',
--N'CurrentInventoriesInTransit',
--N'OtherInventories',
--N'TradeAndOtherCurrentReceivables',
--N'CurrentTradeReceivables',
--N'TradeAndOtherCurrentReceivablesDueFromRelatedParties',
--N'CurrentPrepaymentsAndCurrentAccruedIncome',
--N'CurrentPrepayments',
--N'CurrentAccruedIncome',
--N'CurrentBilledButNotReceivedExtension',
--N'CurrentReceivablesFromTaxesOtherThanIncomeTax',
--N'CurrentValueAddedTaxReceivables',
--N'OtherCurrentReceivables',
--N'OtherCurrentFinancialAssets',
--N'CashAndCashEquivalents',
--N'Cash',
--N'CashOnHand',
--N'BalancesWithBanks',
--N'OtherCashAndCashEquivalents',
--N'EquityAndLiabilities',
--N'Equity',
--N'IssuedCapital',
--N'RetainedEarnings',
--N'OtherReserves',
--N'RevaluationSurplus',
--N'StatutoryReserve',
--N'Liabilities',
--N'NoncurrentLiabilities',
--N'NoncurrentProvisions',
--N'NoncurrentProvisionsForEmployeeBenefits',
--N'OtherLongtermProvisions',
--N'NoncurrentPayables',
--N'OtherNoncurrentFinancialLiabilities',
--N'CurrentLiabilities',
--N'CurrentProvisions',
--N'CurrentProvisionsForEmployeeBenefits',
--N'OtherShorttermProvisions',
--N'TradeAndOtherCurrentPayables',
--N'TradeAndOtherCurrentPayablesToTradeSuppliers',
--N'TradeAndOtherCurrentPayablesToRelatedParties',
--N'DeferredIncomeClassifiedAsCurrent',
--N'AccrualsClassifiedAsCurrent',
--N'ShorttermEmployeeBenefitsAccruals',
--N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax',
--N'CurrentValueAddedTaxPayables',
--N'CurrentSocialSecurityPayablesExtension',
--N'CurrentEmployeeIncomeTaxPayablesExtension',
--N'DividendTaxPayableExtension',
--N'OtherCurrentPayables',
--N'OtherCurrentFinancialLiabilities',
--N'IncomeStatementAbstract',
--N'ProfitLoss',
--N'ProfitLossFromContinuingOperations',
--N'ProfitLossBeforeTax',
--N'ProfitLossFromOperatingActivities',
--N'Revenue',
--N'RevenueFromSaleOfGoods',
--N'RevenueFromRenderingOfServices',
--N'RevenueFromDividends',
--N'OtherRevenue',
--N'OtherIncome',
--N'ExpenseByNature',
--N'RawMaterialsAndConsumablesUsed',
--N'CostOfMerchandiseSold',
--N'ServicesExpense',
--N'InsuranceExpense',
--N'ProfessionalFeesExpense',
--N'TransportationExpense',
--N'BankAndSimilarCharges',
--N'TravelExpense',
--N'CommunicationExpense',
--N'UtilitiesExpense',
--N'AdvertisingExpense',
--N'EmployeeBenefitsExpense',
--N'ShorttermEmployeeBenefitsExpense',
--N'WagesAndSalaries',
--N'SocialSecurityContributions',
--N'OtherShorttermEmployeeBenefits',
--N'PostemploymentBenefitExpenseDefinedContributionPlans',
--N'PostemploymentBenefitExpenseDefinedBenefitPlans',
--N'TerminationBenefitsExpense',
--N'OtherLongtermBenefits',
--N'OtherEmployeeExpense',
--N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
--N'DepreciationAndAmortisationExpense',
--N'DepreciationExpense',
--N'AmortisationExpense',
--N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
--N'WritedownsReversalsOfPropertyPlantAndEquipment',
--N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossTradeReceivables',
--N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLossLoansAndAdvances',
--N'TaxExpenseOtherThanIncomeTaxExpense',
--N'OtherExpenseByNature',
--N'OtherGainsLosses',
--N'GainsLossesOnDisposalsOfPropertyPlantAndEquipment',
--N'GainsLossesOnNetMonetaryPosition',
--N'FinanceIncome',
--N'FinanceCosts',
--N'ControlAccountsExtension',
--N'TradersControlAccountsExtension',
--N'SuppliersControlAccountsExtension',
--N'CashPaymentsToSuppliersControlExtension',
--N'GoodsAndServicesReceivedFromSuppliersControlExtensions',
--N'CustomersControlAccountsExtension',
--N'CashReceiptsFromCustomersControlExtension',
--N'GoodsAndServicesIssuedToCustomersControlExtension',
--N'PayrollControlExtension',
--N'OthersAccountsControlExtension',
--N'CashPaymentsToOthersControlExtension',
--N'CashReceiptsFromOthersControlExtension',
--N'GuaranteesExtension',
--N'CollectionGuaranteeExtension',
--N'DishonouredGuaranteeExtension')
--EXEC [api].[AccountTypes__Activate]
--	@IndexedIds = @IndexedIds,
--	@IsActive =0,
--	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--IF @ValidationErrorsJson IS NOT NULL 
--BEGIN
--	Print 'Account Types: Deactivating: ' + @ValidationErrorsJson
--	GOTO Err_Label;
--END;

DECLARE @ServicesExpenseNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Id] = @ServicesExpense);
UPDATE dbo.[AccountTypes] SET IsSystem = 1 WHERE [Node].IsDescendantOf(@ServicesExpenseNode) = 0;


END