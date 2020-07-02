	IF NOT EXISTS(SELECT * FROM dbo.[AccountTypes])
BEGIN
DECLARE @AT TABLE (
	[Index] INT, [Code] NVARCHAR(50),
	[Node] HIERARCHYID, [EntryTypeParentCode] NVARCHAR (255), [Concept] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
)
--Script
INSERT INTO @AT VALUES(0,'1', '/1/', NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
INSERT INTO @AT VALUES(1,'11', '/1/1/', NULL,N'Assets', N'Assets',N'The amount of resources: (a) controlled by the entity as a result of past events; and (b) from which future economic benefits are expected to flow to the entity.')
INSERT INTO @AT VALUES(2,'111', '/1/1/1/', NULL,N'NoncurrentAssets', N'Non-current assets',N'The amount of assets that do not meet the definition of current assets. [Refer: Current assets]')
INSERT INTO @AT VALUES(3,'1111', '/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
INSERT INTO @AT VALUES(4,'111101', '/1/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'LandAndBuildings', N'Land and buildings',N'The amount of property, plant and equipment representing land and depreciable buildings and similar structures for use in operations. [Refer: Buildings; Land; Property, plant and equipment]')
INSERT INTO @AT VALUES(5,'1111011', '/1/1/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'Land', N'Land',N'The amount of property, plant and equipment representing land held by the entity for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(6,'1111012', '/1/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Buildings', N'Buildings',N'The amount of property, plant and equipment representing depreciable buildings and similar structures for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(7,'111102', '/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Machinery', N'Machinery',N'The amount of property, plant and equipment representing long-lived, depreciable machinery used in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(8,'111103', '/1/1/1/1/3/', N'ChangesInPropertyPlantAndEquipment',N'Vehicles', N'Vehicles',N'The amount of property, plant and equipment representing vehicles used in the entity''s operations, specifically to include aircraft, motor vehicles and ships. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(9,'111104', '/1/1/1/1/4/', N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(10,'111105', '/1/1/1/1/5/', N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(11,'111106', '/1/1/1/1/6/', N'ChangesInPropertyPlantAndEquipment',N'BearerPlants', N'Bearer plants',N'The amount of property, plant and equipment representing bearer plants. Bearer plant is a living plant that (a) is used in the production or supply of agricultural produce; (b) is expected to bear produce for more than one period; and (c) has a remote likelihood of being sold as agricultural produce, except for incidental scrap sales. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(12,'111107', '/1/1/1/1/7/', N'ChangesInPropertyPlantAndEquipment',N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',N'The amount of exploration and evaluation assets recognised as tangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(13,'111108', '/1/1/1/1/8/', N'ChangesInPropertyPlantAndEquipment',N'MiningAssets', N'Mining assets',N'The amount of assets related to mining activities of the entity.')
INSERT INTO @AT VALUES(14,'111109', '/1/1/1/1/9/', N'ChangesInPropertyPlantAndEquipment',N'OilAndGasAssets', N'Oil and gas assets',N'The amount of assets related to the exploration, evaluation, development or production of oil and gas.')
INSERT INTO @AT VALUES(15,'111110', '/1/1/1/1/10/', N'ChangesInPropertyPlantAndEquipment',N'ConstructionInProgress', N'Construction in progress',N'The amount of expenditure capitalised during the construction of non-current assets that are not yet available for use. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(16,'111111', '/1/1/1/1/11/', N'ChangesInPropertyPlantAndEquipment',N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', N'Owner-occupied property measured using investment property fair value model',N'The amount of property, plant and equipment representing owner-occupied property measured using the investment property fair value model applying paragraph 29A of IAS 16. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(17,'111199', '/1/1/1/1/99/', N'ChangesInPropertyPlantAndEquipment',N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment',N'The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(18,'1112', '/1/1/1/2/', NULL,N'InvestmentProperty', N'Investment property',N'The amount of property (land or a building - or part of a building - or both) held (by the owner or by the lessee as a right-of-use asset) to earn rentals or for capital appreciation or both, rather than for: (a) use in the production or supply of goods or services or for administrative purposes; or (b) sale in the ordinary course of business.')
INSERT INTO @AT VALUES(19,'11121', '/1/1/1/2/1/', N'ChangesInInvestmentProperty',N'InvestmentPropertyCompleted', N'Investment property completed',N'The amount of investment property whose construction or development is complete. [Refer: Investment property]')
INSERT INTO @AT VALUES(20,'11122', '/1/1/1/2/2/', NULL,N'InvestmentPropertyUnderConstructionOrDevelopment', N'Investment property under construction or development',N'The amount of property that is being constructed or developed for future use as investment property. [Refer: Investment property]')
INSERT INTO @AT VALUES(21,'1113', '/1/1/1/3/', N'ChangesInGoodwill',N'Goodwill', N'Goodwill',N'The amount of assets representing the future economic benefits arising from other assets acquired in a business combination that are not individually identified and separately recognised. [Refer: Business combinations [member]]')
INSERT INTO @AT VALUES(22,'1114', '/1/1/1/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(23,'1115', '/1/1/1/5/', NULL,N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',N'The amount of investments accounted for using the equity method. The equity method is a method of accounting whereby the investment is initially recognised at cost and adjusted thereafter for the post-acquisition change in the investor''s share of net assets of the investee. The investor''s profit or loss includes its share of the profit or loss of the investee. The investor''s other comprehensive income includes its share of the other comprehensive income of the investee. [Refer: At cost [member]]')
INSERT INTO @AT VALUES(24,'1116', '/1/1/1/6/', NULL,N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',N'The amount of investments in subsidiaries, joint ventures and associates in an entity''s separate financial statements. [Refer: Associates [member]; Joint ventures [member]; Subsidiaries [member]; Investments in subsidiaries]')
INSERT INTO @AT VALUES(25,'1117', '/1/1/1/7/', NULL,N'NoncurrentBiologicalAssets', N'Non-current biological assets',N'The amount of living animals or plants recognised as assets.')
INSERT INTO @AT VALUES(26,'1118', '/1/1/1/8/', NULL,N'NoncurrentReceivables', N'Trade and other non-current receivables',N'The amount of non-current trade receivables and non-current other receivables. [Refer: Non-current trade receivables; Other non-current receivables]')
INSERT INTO @AT VALUES(27,'11181', '/1/1/1/8/1/', NULL,N'NoncurrentTradeReceivables', N'Non-current trade receivables',N'The amount of non-current trade receivables and non-current other receivables. [Refer: Non-current trade receivables; Other non-current receivables]')
INSERT INTO @AT VALUES(28,'11182', '/1/1/1/8/2/', NULL,N'NoncurrentReceivablesDueFromRelatedParties', N'Non-current receivables due from related parties',N'The amount of non-current receivables due from related parties. [Refer: Related parties [member]]')
INSERT INTO @AT VALUES(29,'11183', '/1/1/1/8/3/', NULL,N'NoncurrentPrepaymentsAndNoncurrentAccruedIncome', N'Non-current prepayments and non-current accrued income',N'The amount of non-current prepayments and non-current accrued income. [Refer: Prepayments; Accrued income]')
INSERT INTO @AT VALUES(30,'111831', '/1/1/1/8/3/1/', NULL,N'NoncurrentPrepayments', N'Non-current prepayments',N'The amount of non-current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(31,'111832', '/1/1/1/8/3/2/', NULL,N'NoncurrentAccruedIncome', N'Non-current accrued income',N'The amount of non-current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(32,'11184', '/1/1/1/8/4/', NULL,N'NoncurrentReceivablesFromTaxesOtherThanIncomeTax', N'Non-current receivables from taxes other than income tax',N'The amount of non-current receivables from taxes other than income tax. [Refer: Receivables from taxes other than income tax]')
INSERT INTO @AT VALUES(33,'11185', '/1/1/1/8/5/', NULL,N'NoncurrentReceivablesFromSaleOfProperties', N'Non-current receivables from sale of properties',N'The amount of non-current receivables from sale of properties. [Refer: Receivables from sale of properties]')
INSERT INTO @AT VALUES(34,'11186', '/1/1/1/8/6/', NULL,N'NoncurrentReceivablesFromRentalOfProperties', N'Non-current receivables from rental of properties',N'The amount of non-current receivables from rental of properties. [Refer: Receivables from rental of properties]')
INSERT INTO @AT VALUES(35,'11187', '/1/1/1/8/7/', NULL,N'OtherNoncurrentReceivables', N'Other non-current receivables',N'The amount of non-current other receivables. [Refer: Other receivables]')
INSERT INTO @AT VALUES(36,'1119', '/1/1/1/9/', NULL,N'NoncurrentInventories', N'Non-current inventories',N'The amount of non-current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(37,'11110', '/1/1/1/10/', NULL,N'DeferredTaxAssets', N'Deferred tax assets',N'The amounts of income taxes recoverable in future periods in respect of: (a) deductible temporary differences; (b) the carryforward of unused tax losses; and (c) the carryforward of unused tax credits. [Refer: Temporary differences [member]; Unused tax credits [member]; Unused tax losses [member]]')
INSERT INTO @AT VALUES(38,'11111', '/1/1/1/11/', NULL,N'CurrentTaxAssetsNoncurrent', N'Current tax assets, non-current',N'The non-current amount of current tax assets. [Refer: Current tax assets]')
INSERT INTO @AT VALUES(39,'11112', '/1/1/1/12/', NULL,N'OtherNoncurrentFinancialAssets', N'Other non-current financial assets',N'The amount of non-current financial assets that the entity does not separately disclose in the same statement or note. [Refer: Other financial assets]')
INSERT INTO @AT VALUES(40,'11113', '/1/1/1/13/', NULL,N'OtherNoncurrentNonfinancialAssets', N'Other non-current non-financial assets',N'The amount of non-current non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(41,'11114', '/1/1/1/14/', NULL,N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Non-current non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of non-current non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(42,'112', '/1/1/2/', NULL,N'CurrentAssets', N'Current assets',N'The amount of assets that the entity (a) expects to realise or intends to sell or consume in its normal operating cycle; (b) holds primarily for the purpose of trading; (c) expects to realise within twelve months after the reporting period; or (d) classifies as cash or cash equivalents (as defined in IAS 7) unless the asset is restricted from being exchanged or used to settle a liability for at least twelve months after the reporting period. [Refer: Assets]')
INSERT INTO @AT VALUES(43,'1121', '/1/1/2/1/', N'ChangesInInventories',N'Inventories', N'Current inventories',N'The amount of current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(44,'11211', '/1/1/2/1/1/', N'ChangesInInventories',N'CurrentInventoriesHeldForSale', N'Current inventories held for sale',N'A classification of current inventory representing the amount of inventories held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(45,'112111', '/1/1/2/1/1/1/', N'ChangesInInventories',N'Merchandise', N'Current merchandise',N'A classification of current inventory representing the amount of goods acquired for resale. [Refer: Inventories]')
INSERT INTO @AT VALUES(46,'112112', '/1/1/2/1/1/2/', N'ChangesInInventories',N'CurrentFoodAndBeverage', N'Current food and beverage',N'A classification of current inventory representing the amount of food and beverage. [Refer: Inventories]')
INSERT INTO @AT VALUES(47,'112113', '/1/1/2/1/1/3/', N'ChangesInInventories',N'CurrentAgriculturalProduce', N'Current agricultural produce',N'A classification of current inventory representing the amount of harvested produce of the entity''s biological assets. [Refer: Biological assets; Inventories]')
INSERT INTO @AT VALUES(48,'112114', '/1/1/2/1/1/4/', N'ChangesInInventories',N'FinishedGoods', N'Current finished goods',N'A classification of current inventory representing the amount of goods that have completed the production process and are held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(49,'112115', '/1/1/2/1/1/5/', N'ChangesInInventories',N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property intended for sale in ordinary course of business',N'The amount of property intended for sale in the ordinary course of business of the entity. Property is land or a building - or part of a building - or both.')
INSERT INTO @AT VALUES(50,'11212', '/1/1/2/1/2/', NULL,N'WorkInProgress', N'Current work in progress',N'A classification of current inventory representing the amount of assets currently in production, which require further processes to be converted into finished goods or services. [Refer: Current finished goods; Inventories]')
INSERT INTO @AT VALUES(51,'11213', '/1/1/2/1/3/', N'ChangesInInventories',N'CurrentMaterialsAndSuppliesToBeConsumedInProductionProcessOrRenderingServices', N'Current materials and supplies to be consumed in production process or rendering services',N'A classification of current inventory representing the amount of materials and supplies to be consumed in a production process or while rendering services. [Refer: Inventories]')
INSERT INTO @AT VALUES(52,'112131', '/1/1/2/1/3/1/', N'ChangesInInventories',N'CurrentRawMaterialsAndCurrentProductionSupplies', N'Current raw materials and current production supplies',N'A classification of current inventory representing the amount of current raw materials and current production supplies. [Refer: Current production supplies; Current raw materials]')
INSERT INTO @AT VALUES(53,'1121311', '/1/1/2/1/3/1/1/', N'ChangesInInventories',N'RawMaterials', N'Current raw materials',N'A classification of current inventory representing the amount of assets to be consumed in the production process or in the rendering of services. [Refer: Inventories]')
INSERT INTO @AT VALUES(54,'1121312', '/1/1/2/1/3/1/2/', N'ChangesInInventories',N'ProductionSupplies', N'Current production supplies',N'A classification of current inventory representing the amount of supplies to be used for the production process. [Refer: Inventories]')
INSERT INTO @AT VALUES(55,'112132', '/1/1/2/1/3/2/', N'ChangesInInventories',N'CurrentPackagingAndStorageMaterials', N'Current packaging and storage materials',N'A classification of current inventory representing the amount of packaging and storage materials. [Refer: Inventories]')
INSERT INTO @AT VALUES(56,'112133', '/1/1/2/1/3/3/', N'ChangesInInventories',N'SpareParts', N'Current spare parts',N'A classification of current inventory representing the amount of interchangeable parts that are kept in an inventory and are used for the repair or replacement of failed parts. [Refer: Inventories]')
INSERT INTO @AT VALUES(57,'112134', '/1/1/2/1/3/4/', N'ChangesInInventories',N'CurrentFuel', N'Current fuel',N'A classification of current inventory representing the amount of fuel. [Refer: Inventories]')
INSERT INTO @AT VALUES(58,'11214', '/1/1/2/1/4/', NULL,N'CurrentInventoriesInTransit', N'Current inventories in transit',N'A classification of current inventory representing the amount of inventories in transit. [Refer: Inventories]')
INSERT INTO @AT VALUES(59,'11219', '/1/1/2/1/9/', N'ChangesInInventories',N'OtherInventories', N'Other current inventories',N'The amount of inventory that the entity does not separately disclose in the same statement or note. [Refer: Inventories]')
INSERT INTO @AT VALUES(60,'1122', '/1/1/2/2/', NULL,N'TradeAndOtherCurrentReceivables', N'Trade and other current receivables',N'The amount of current trade receivables and current other receivables. [Refer: Current trade receivables; Other current receivables]')
INSERT INTO @AT VALUES(61,'11221', '/1/1/2/2/1/', NULL,N'CurrentTradeReceivables', N'Current trade receivables',N'The amount of current trade receivables. [Refer: Trade receivables]')
INSERT INTO @AT VALUES(62,'11222', '/1/1/2/2/2/', NULL,N'TradeAndOtherCurrentReceivablesDueFromRelatedParties', N'Current receivables due from related parties',N'The amount of current receivables due from related parties. [Refer: Related parties [member]]')
INSERT INTO @AT VALUES(63,'11223', '/1/1/2/2/3/', NULL,N'CurrentPrepaymentsAndCurrentAccruedIncome', N'Current prepayments and current accrued income',N'The amount of current prepayments and current accrued income. [Refer: Prepayments; Accrued income]')
INSERT INTO @AT VALUES(64,'112231', '/1/1/2/2/3/1/', NULL,N'CurrentPrepayments', N'Current prepayments',N'The amount of current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(65,'112232', '/1/1/2/2/3/2/', NULL,N'CurrentAccruedIncome', N'Current accrued income',N'The amount of current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(66,'11224', '/1/1/2/2/4/', NULL,N'CurrentBilledButNotReceivedExtension', N'Current billed but not received',N'The amount invoiced but against which there was no good or service received')
INSERT INTO @AT VALUES(67,'11225', '/1/1/2/2/5/', NULL,N'CurrentReceivablesFromTaxesOtherThanIncomeTax', N'Current receivables from taxes other than income tax',N'The amount of current receivables from taxes other than income tax. [Refer: Receivables from taxes other than income tax]')
INSERT INTO @AT VALUES(68,'112251', '/1/1/2/2/5/1/', NULL,N'CurrentValueAddedTaxReceivables', N'Current value added tax receivables',N'The amount of current value added tax receivables. [Refer: Value added tax receivables]')
INSERT INTO @AT VALUES(69,'112252', '/1/1/2/2/5/2/', NULL,N'WithholdingTaxReceivablesExtension', N'Withholding tax receivables',N'The amount of receivables related to a withtholding tax.')
INSERT INTO @AT VALUES(70,'11227', '/1/1/2/2/7/', NULL,N'CurrentReceivablesFromRentalOfProperties', N'Current receivables from rental of properties',N'The amount of current receivables from rental of properties. [Refer: Receivables from rental of properties]')
INSERT INTO @AT VALUES(71,'11228', '/1/1/2/2/8/', NULL,N'OtherCurrentReceivables', N'Other current receivables',N'The amount of current other receivables. [Refer: Other receivables]')
INSERT INTO @AT VALUES(72,'11229', '/1/1/2/2/9/', NULL,N'AllowanceAccountForCreditLossesOfTradeAndOtherCurrentReceivablesExtension', N'Allowance account for credit losses of trade and other current receivables',N'The amount of an allowance account used to record impairments to trade and other current receivables due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(73,'1123', '/1/1/2/3/', NULL,N'CurrentTaxAssetsCurrent', N'Current tax assets, current',N'The current amount of current tax assets. [Refer: Current tax assets]')
INSERT INTO @AT VALUES(74,'1124', '/1/1/2/4/', NULL,N'CurrentBiologicalAssets', N'Current biological assets',N'The amount of current biological assets. [Refer: Biological assets]')
INSERT INTO @AT VALUES(75,'1125', '/1/1/2/5/', NULL,N'OtherCurrentFinancialAssets', N'Other current financial assets',N'The amount of current financial assets that the entity does not separately disclose in the same statement or note. [Refer: Other financial assets; Current financial assets]')
INSERT INTO @AT VALUES(76,'11251', '/1/1/2/5/1/', NULL,N'StaffDebtorsExtension', N'Staff debtors',N'')
INSERT INTO @AT VALUES(77,'11252', '/1/1/2/5/2/', NULL,N'SundryDebtorsExtension', N'Sundry debtors',N'')
INSERT INTO @AT VALUES(78,'11253', '/1/1/2/5/3/', NULL,N'CollectionGuaranteeExtension', N'Collection Guarantee',N'e.g., checks, LG, ..')
INSERT INTO @AT VALUES(79,'11254', '/1/1/2/5/4/', NULL,N'DishonouredGuaranteeExtension', N'Dishonoured Guarantee',N'e.g., dishonored check, invalid LG, etc')
INSERT INTO @AT VALUES(80,'1126', '/1/1/2/6/', NULL,N'OtherCurrentNonfinancialAssets', N'Other current non-financial assets',N'The amount of current non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(81,'1127', '/1/1/2/7/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(82,'11271', '/1/1/2/7/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'Cash', N'Cash',N'The amount of cash on hand and demand deposits. [Refer: Cash on hand]')
INSERT INTO @AT VALUES(83,'112711', '/1/1/2/7/1/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashOnHand', N'Cash on hand',N'The amount of cash held by the entity. This does not include demand deposits.')
INSERT INTO @AT VALUES(84,'112712', '/1/1/2/7/1/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'BalancesWithBanks', N'Balances with banks',N'The amount of cash balances held at banks.')
INSERT INTO @AT VALUES(85,'11272', '/1/1/2/7/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashEquivalents', N'Cash equivalents',N'The amount of short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value.')
INSERT INTO @AT VALUES(86,'112721', '/1/1/2/7/2/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ShorttermDepositsClassifiedAsCashEquivalents', N'Short-term deposits, classified as cash equivalents',N'A classification of cash equivalents representing short-term deposits. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(87,'112722', '/1/1/2/7/2/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ShorttermInvestmentsClassifiedAsCashEquivalents', N'Short-term investments, classified as cash equivalents',N'A classification of cash equivalents representing short-term investments. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(88,'112723', '/1/1/2/7/2/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'BankingArrangementsClassifiedAsCashEquivalents', N'Other banking arrangements, classified as cash equivalents',N'A classification of cash equivalents representing banking arrangements that the entity does not separately disclose in the same statement or note. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(89,'11273', '/1/1/2/7/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'OtherCashAndCashEquivalents', N'Other cash and cash equivalents',N'The amount of cash and cash equivalents that the entity does not separately disclose in the same statement or note. [Refer: Cash and cash equivalents]')
INSERT INTO @AT VALUES(90,'112731', '/1/1/2/7/3/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ChecksUnderCollectionExtension', N'Checks under collection',N'')
INSERT INTO @AT VALUES(91,'1128', '/1/1/2/8/', NULL,N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Current non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of current non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(92,'1129', '/1/1/2/9/', NULL,N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners',N'The amount of non-current assets or disposal groups classified as held for sale or as held for distribution to owners. [Refer: Non-current assets or disposal groups classified as held for distribution to owners; Non-current assets or disposal groups classified as held for sale]')
INSERT INTO @AT VALUES(93,'113', '/1/1/3/', NULL,N'AllowanceAccountForCreditLossesOfFinancialAssets', N'Allowance account for credit losses of financial assets',N'The amount of an allowance account used to record impairments to financial assets due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(94,'1131', '/1/1/3/1/', NULL,N'AllowanceAccountForCreditLossesOfTradeAndOtherReceivablesExtension', N'Allowance account for credit losses of trade and other receivables',N'The amount of an allowance account used to record impairments to trade and other receivables due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(95,'1132', '/1/1/3/2/', NULL,N'AllowanceAccountForCreditLossesOfOtherFinancialAssetsExtension', N'Allowance account for credit losses of other financial assets',N'The amount of an allowance account used to record impairments to other financial assets due to credit losses. [Refer: Financial assets]')
INSERT INTO @AT VALUES(96,'12', '/1/2/', NULL,N'EquityAndLiabilities', N'Equity and liabilities',N'The amount of the entity''s equity and liabilities. [Refer: Equity; Liabilities]')
INSERT INTO @AT VALUES(97,'121', '/1/2/1/', N'ChangesInEquity',N'Equity', N'Equity',N'The amount of residual interest in the assets of the entity after deducting all its liabilities.')
INSERT INTO @AT VALUES(98,'1211', '/1/2/1/1/', N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
INSERT INTO @AT VALUES(99,'1212', '/1/2/1/2/', N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(100,'1213', '/1/2/1/3/', N'ChangesInEquity',N'SharePremium', N'Share premium',N'The amount received or receivable from the issuance of the entity''s shares in excess of nominal value.')
INSERT INTO @AT VALUES(101,'1214', '/1/2/1/4/', N'ChangesInEquity',N'TreasuryShares', N'Treasury shares',N'An entity’s own equity instruments, held by the entity or other members of the consolidated group.')
INSERT INTO @AT VALUES(102,'1215', '/1/2/1/5/', N'ChangesInEquity',N'OtherEquityInterest', N'Other equity interest',N'The amount of equity interest of an entity without share capital that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(103,'1216', '/1/2/1/6/', N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(104,'121601', '/1/2/1/6/1/', N'ChangesInEquity',N'RevaluationSurplus', N'Revaluation surplus',N'A component of equity representing the accumulated revaluation surplus on the revaluation of assets recognised in other comprehensive income. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(105,'121602', '/1/2/1/6/2/', N'ChangesInEquity',N'ReserveOfExchangeDifferencesOnTranslation', N'Reserve of exchange differences on translation',N'A component of equity representing exchange differences on translation of financial statements of foreign operations recognised in other comprehensive income and accumulated in equity. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(106,'121603', '/1/2/1/6/3/', N'ChangesInEquity',N'ReserveOfCashFlowHedges', N'Reserve of cash flow hedges',N'A component of equity representing the accumulated portion of gain (loss) on a hedging instrument that is determined to be an effective hedge for cash flow hedges. [Refer: Cash flow hedges [member]]')
INSERT INTO @AT VALUES(107,'121604', '/1/2/1/6/4/', N'ChangesInEquity',N'ReserveOfGainsAndLossesOnHedgingInstrumentsThatHedgeInvestmentsInEquityInstruments', N'Reserve of gains and losses on hedging instruments that hedge investments in equity instruments',N'A component of equity representing the accumulated gains and losses on hedging instruments that hedge investments in equity instruments that the entity has designated at fair value through other comprehensive income.')
INSERT INTO @AT VALUES(108,'121605', '/1/2/1/6/5/', N'ChangesInEquity',N'ReserveOfChangeInValueOfTimeValueOfOptions', N'Reserve of change in value of time value of options',N'A component of equity representing the accumulated change in the value of the time value of options when separating the intrinsic value and time value of an option contract and designating as the hedging instrument only the changes in the intrinsic value.')
INSERT INTO @AT VALUES(109,'121606', '/1/2/1/6/6/', N'ChangesInEquity',N'ReserveOfChangeInValueOfForwardElementsOfForwardContracts', N'Reserve of change in value of forward elements of forward contracts',N'A component of equity representing the accumulated change in the value of the forward elements of forward contracts when separating the forward element and spot element of a forward contract and designating as the hedging instrument only the changes in the spot element.')
INSERT INTO @AT VALUES(110,'121607', '/1/2/1/6/7/', N'ChangesInEquity',N'ReserveOfChangeInValueOfForeignCurrencyBasisSpreads', N'Reserve of change in value of foreign currency basis spreads',N'A component of equity representing the accumulated change in the value of foreign currency basis spreads of financial instruments when excluding them from the designation of these financial instruments as hedging instruments.')
INSERT INTO @AT VALUES(111,'121608', '/1/2/1/6/8/', N'ChangesInEquity',N'ReserveOfGainsAndLossesOnFinancialAssetsMeasuredAtFairValueThroughOtherComprehensiveIncome', N'Reserve of gains and losses on financial assets measured at fair value through other comprehensive income',N'A component of equity representing the reserve of gains and losses on financial assets measured at fair value through other comprehensive income. [Refer: Financial assets measured at fair value through other comprehensive income; Other comprehensive income]')
INSERT INTO @AT VALUES(112,'121609', '/1/2/1/6/9/', N'ChangesInEquity',N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillBeReclassifiedToProfitOrLoss', N'Reserve of insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will be reclassified to profit or loss',N'A component of equity representing the accumulated insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will be reclassified subsequently to profit or loss. [Refer: Insurance finance income (expenses); Insurance contracts issued [member]]')
INSERT INTO @AT VALUES(113,'121610', '/1/2/1/6/10/', N'ChangesInEquity',N'ReserveOfInsuranceFinanceIncomeExpensesFromInsuranceContractsIssuedExcludedFromProfitOrLossThatWillNotBeReclassifiedToProfitOrLoss', N'Reserve of insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will not be reclassified to profit or loss',N'A component of equity representing the accumulated insurance finance income (expenses) from insurance contracts issued excluded from profit or loss that will not be reclassified subsequently to profit or loss. [Refer: Insurance finance income (expenses); Insurance contracts issued [member]]')
INSERT INTO @AT VALUES(114,'121611', '/1/2/1/6/11/', N'ChangesInEquity',N'ReserveOfFinanceIncomeExpensesFromReinsuranceContractsHeldExcludedFromProfitOrLoss', N'Reserve of finance income (expenses) from reinsurance contracts held excluded from profit or loss',N'A component of equity representing the accumulated finance income (expenses) from reinsurance contracts held excluded from profit or loss. [Refer: Insurance finance income (expenses); Reinsurance contracts held [member]]')
INSERT INTO @AT VALUES(115,'121612', '/1/2/1/6/12/', N'ChangesInEquity',N'ReserveOfGainsAndLossesOnRemeasuringAvailableforsaleFinancialAssets', N'Reserve of gains and losses on remeasuring available-for-sale financial assets',N'A component of equity representing accumulated gains and losses on remeasuring available-for-sale financial assets. [Refer: Financial assets available-for-sale]')
INSERT INTO @AT VALUES(116,'121613', '/1/2/1/6/13/', N'ChangesInEquity',N'ReserveOfSharebasedPayments', N'Reserve of share-based payments',N'A component of equity resulting from share-based payments.')
INSERT INTO @AT VALUES(117,'121614', '/1/2/1/6/14/', N'ChangesInEquity',N'ReserveOfRemeasurementsOfDefinedBenefitPlans', N'Reserve of remeasurements of defined benefit plans',N'A component of equity representing the accumulated remeasurements of defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(118,'121615', '/1/2/1/6/15/', N'ChangesInEquity',N'AmountRecognisedInOtherComprehensiveIncomeAndAccumulatedInEquityRelatingToNoncurrentAssetsOrDisposalGroupsHeldForSale', N'Amount recognised in other comprehensive income and accumulated in equity relating to non-current assets or disposal groups held for sale',N'The amount recognised in other comprehensive income and accumulated in equity, relating to non-current assets or disposal groups held for sale. [Refer: Non-current assets or disposal groups classified as held for sale; Other reserves; Other comprehensive income; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(119,'121616', '/1/2/1/6/16/', N'ChangesInEquity',N'ReserveOfGainsAndLossesFromInvestmentsInEquityInstruments', N'Reserve of gains and losses from investments in equity instruments',N'A component of equity representing accumulated gains and losses from investments in equity instruments that the entity has designated at fair value through other comprehensive income.')
INSERT INTO @AT VALUES(120,'121617', '/1/2/1/6/17/', N'ChangesInEquity',N'ReserveOfChangeInFairValueOfFinancialLiabilityAttributableToChangeInCreditRiskOfLiability', N'Reserve of change in fair value of financial liability attributable to change in credit risk of liability',N'A component of equity representing the accumulated change in fair value of financial liabilities attributable to change in the credit risk of the liabilities. [Refer: Credit risk [member]; Financial liabilities]')
INSERT INTO @AT VALUES(121,'121618', '/1/2/1/6/18/', N'ChangesInEquity',N'ReserveForCatastrophe', N'Reserve for catastrophe',N'A component of equity representing resources to provide for infrequent but severe catastrophic losses caused by events such as damage to nuclear installations or satellites, or earthquake damage.')
INSERT INTO @AT VALUES(122,'121619', '/1/2/1/6/19/', N'ChangesInEquity',N'ReserveForEqualisation', N'Reserve for equalisation',N'A component of equity representing resources to cover random fluctuations of claim expenses around the expected value of claims for some types of insurance contract.')
INSERT INTO @AT VALUES(123,'121620', '/1/2/1/6/20/', N'ChangesInEquity',N'ReserveOfDiscretionaryParticipationFeatures', N'Reserve of discretionary participation features',N'A component of equity resulting from discretionary participation features. Discretionary participation features are contractual rights to receive, as a supplement to guaranteed benefits, additional benefits: (a) that are likely to be a significant portion of the total contractual benefits; (b) whose amount or timing is contractually at the discretion of the issuer; and (c) that are contractually based on: (i) the performance of a specified pool of contracts or a specified type of contract; (ii) realised and/or unrealised investment returns on a specified pool of assets held by the issuer; or (iii) the profit or loss of the company, fund or other entity that issues the contract.')
INSERT INTO @AT VALUES(124,'121621', '/1/2/1/6/21/', N'ChangesInEquity',N'ReserveOfEquityComponentOfConvertibleInstruments', N'Reserve of equity component of convertible instruments',N'A component of equity representing components of convertible instruments classified as equity.')
INSERT INTO @AT VALUES(125,'121622', '/1/2/1/6/22/', N'ChangesInEquity',N'CapitalRedemptionReserve', N'Capital redemption reserve',N'A component of equity representing the reserve for the redemption of the entity''s own shares.')
INSERT INTO @AT VALUES(126,'121623', '/1/2/1/6/23/', N'ChangesInEquity',N'MergerReserve', N'Merger reserve',N'A component of equity that may result in relation to a business combination outside the scope of IFRS 3.')
INSERT INTO @AT VALUES(127,'121624', '/1/2/1/6/24/', N'ChangesInEquity',N'StatutoryReserve', N'Statutory reserve',N'A component of equity representing reserves created based on legal requirements.')
INSERT INTO @AT VALUES(128,'122', '/1/2/2/', NULL,N'Liabilities', N'Liabilities',N'The amount of a present obligation of the entity to transfer an economic resource as a result of past events. Economic resource is a right that has the potential to produce economic benefits.')
INSERT INTO @AT VALUES(129,'1221', '/1/2/2/1/', NULL,N'NoncurrentLiabilities', N'Non-current liabilities',N'The amount of liabilities that do not meet the definition of current liabilities. [Refer: Current liabilities]')
INSERT INTO @AT VALUES(130,'12211', '/1/2/2/1/1/', NULL,N'NoncurrentProvisions', N'Non-current provisions',N'The amount of non-current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(131,'122111', '/1/2/2/1/1/1/', NULL,N'NoncurrentProvisionsForEmployeeBenefits', N'Non-current provisions for employee benefits',N'The amount of non-current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(132,'122112', '/1/2/2/1/1/2/', NULL,N'OtherLongtermProvisions', N'Other non-current provisions',N'The amount of non-current provisions other than provisions for employee benefits. [Refer: Non-current provisions]')
INSERT INTO @AT VALUES(133,'12212', '/1/2/2/1/2/', NULL,N'NoncurrentPayables', N'Trade and other non-current payables',N'The amount of non-current trade payables and non-current other payables. [Refer: Other non-current payables; Non-current trade payables]')
INSERT INTO @AT VALUES(134,'12213', '/1/2/2/1/3/', NULL,N'DeferredTaxLiabilities', N'Deferred tax liabilities',N'The amounts of income taxes payable in future periods in respect of taxable temporary differences. [Refer: Temporary differences [member]]')
INSERT INTO @AT VALUES(135,'12214', '/1/2/2/1/4/', NULL,N'CurrentTaxLiabilitiesNoncurrent', N'Current tax liabilities, non-current',N'The non-current amount of current tax liabilities. [Refer: Current tax liabilities]')
INSERT INTO @AT VALUES(136,'12215', '/1/2/2/1/5/', NULL,N'OtherNoncurrentFinancialLiabilities', N'Other non-current financial liabilities',N'The amount of non-current financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(137,'12216', '/1/2/2/1/6/', NULL,N'OtherNoncurrentNonfinancialLiabilities', N'Other non-current non-financial liabilities',N'The amount of non-current non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(138,'1222', '/1/2/2/2/', NULL,N'CurrentLiabilities', N'Current liabilities',N'The amount of liabilities that: (a) the entity expects to settle in its normal operating cycle; (b) the entity holds primarily for the purpose of trading; (c) are due to be settled within twelve months after the reporting period; or (d) the entity does not have an unconditional right to defer settlement for at least twelve months after the reporting period.')
INSERT INTO @AT VALUES(139,'12221', '/1/2/2/2/1/', NULL,N'CurrentProvisions', N'Current provisions',N'The amount of current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(140,'122211', '/1/2/2/2/1/1/', NULL,N'CurrentProvisionsForEmployeeBenefits', N'Current provisions for employee benefits',N'The amount of current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(141,'122212', '/1/2/2/2/1/2/', N'ChangesInOtherProvisions',N'OtherShorttermProvisions', N'Other current provisions',N'The amount of current provisions other than provisions for employee benefits. [Refer: Provisions]')
INSERT INTO @AT VALUES(142,'12222', '/1/2/2/2/2/', NULL,N'TradeAndOtherCurrentPayables', N'Trade and other current payables',N'The amount of current trade payables and current other payables. [Refer: Current trade payables; Other current payables]')
INSERT INTO @AT VALUES(143,'122221', '/1/2/2/2/2/1/', NULL,N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'Current trade payables',N'The current amount of payment due to suppliers for goods and services used in entity''s business. [Refer: Current liabilities; Trade payables]')
INSERT INTO @AT VALUES(144,'122222', '/1/2/2/2/2/2/', NULL,N'TradeAndOtherCurrentPayablesToRelatedParties', N'Current payables to related parties',N'The amount of current payables due to related parties. [Refer: Related parties [member]; Payables to related parties]')
INSERT INTO @AT VALUES(145,'122223', '/1/2/2/2/2/3/', NULL,N'DeferredIncomeClassifiedAsCurrent', N'Deferred income classified as current',N'The amount of deferred income classified as current. [Refer: Deferred income]')
INSERT INTO @AT VALUES(146,'122224', '/1/2/2/2/2/4/', NULL,N'AccrualsClassifiedAsCurrent', N'Accruals classified as current',N'The amount of accruals classified as current. [Refer: Accruals]')
INSERT INTO @AT VALUES(147,'1222241', '/1/2/2/2/2/4/1/', NULL,N'ShorttermEmployeeBenefitsAccruals', N'Short-term employee benefits accruals',N'The amount of accruals for employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services. [Refer: Accruals classified as current]')
INSERT INTO @AT VALUES(148,'122226', '/1/2/2/2/2/6/', NULL,N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Current payables on social security and taxes other than income tax',N'The amount of current payables on social security and taxes other than incomes tax. [Refer: Payables on social security and taxes other than income tax]')
INSERT INTO @AT VALUES(149,'12222601', '/1/2/2/2/2/6/1/', NULL,N'CurrentValueAddedTaxPayables', N'Current value added tax payables',N'The amount of current value added tax payables. [Refer: Value added tax payables]')
INSERT INTO @AT VALUES(150,'12222602', '/1/2/2/2/2/6/2/', NULL,N'CurrentExciseTaxPayables', N'Current excise tax payables',N'The amount of current excise tax payables. [Refer: Excise tax payables]')
INSERT INTO @AT VALUES(151,'12222603', '/1/2/2/2/2/6/3/', NULL,N'CurrentSocialSecurityPayablesExtension', N'Current Social Security payables',N'The amount of current social security payables')
INSERT INTO @AT VALUES(152,'12222604', '/1/2/2/2/2/6/4/', NULL,N'CurrentZakatPayablesExtension', N'Current Zakat payables',N'The amount of current zakat payables')
INSERT INTO @AT VALUES(153,'12222605', '/1/2/2/2/2/6/5/', NULL,N'CurrentEmployeeIncomeTaxPayablesExtension', N'Current Employee Income tax payables',N'The amount of current employee income tax payables')
INSERT INTO @AT VALUES(154,'12222606', '/1/2/2/2/2/6/6/', NULL,N'CurrentEmployeeStampTaxPayablesExtension', N'Current Employee Stamp tax payables',N'The amount of current employee stamp tax payables')
INSERT INTO @AT VALUES(155,'12222607', '/1/2/2/2/2/6/7/', NULL,N'ProvidentFundPayableExtension', N'Provident fund payable',N'')
INSERT INTO @AT VALUES(156,'12222608', '/1/2/2/2/2/6/8/', NULL,N'WithholdingTaxPayableExtension', N'Withholding tax payable',N'')
INSERT INTO @AT VALUES(157,'12222609', '/1/2/2/2/2/6/9/', NULL,N'CostSharingPayableExtension', N'Cost sharing payable',N'')
INSERT INTO @AT VALUES(158,'12222610', '/1/2/2/2/2/6/10/', NULL,N'DividendTaxPayableExtension', N'Dividend tax payable',N'')
INSERT INTO @AT VALUES(159,'122227', '/1/2/2/2/2/7/', NULL,N'CurrentRetentionPayables', N'Current retention payables',N'The amount of current retention payables. [Refer: Retention payables]')
INSERT INTO @AT VALUES(160,'122228', '/1/2/2/2/2/8/', NULL,N'OtherCurrentPayables', N'Other current payables',N'The amount of current payables that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(161,'12223', '/1/2/2/2/3/', NULL,N'CurrentTaxLiabilitiesCurrent', N'Current tax liabilities, current',N'The current amount of current tax liabilities. [Refer: Current tax liabilities]')
INSERT INTO @AT VALUES(162,'12224', '/1/2/2/2/4/', NULL,N'OtherCurrentFinancialLiabilities', N'Other current financial liabilities',N'The amount of current financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities; Current financial liabilities]')
INSERT INTO @AT VALUES(163,'12225', '/1/2/2/2/5/', NULL,N'OtherCurrentNonfinancialLiabilities', N'Other current non-financial liabilities',N'The amount of current non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(164,'12226', '/1/2/2/2/6/', NULL,N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale', N'Liabilities included in disposal groups classified as held for sale',N'The amount of liabilities included in disposal groups classified as held for sale. [Refer: Liabilities; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(165,'2', '/2/', NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'The total of income less expenses from continuing and discontinued operations, excluding the components of other comprehensive income. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(166,'201', '/2/1/', NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(167,'2011', '/2/1/1/', NULL,N'RevenueFromSaleOfGoods', N'Revenue from sale of goods',N'The amount of revenue arising from the sale of goods. [Refer: Revenue]')
INSERT INTO @AT VALUES(168,'2012', '/2/1/2/', NULL,N'RevenueFromRenderingOfServices', N'Revenue from rendering of services',N'The amount of revenue arising from the rendering of services. [Refer: Revenue]')
INSERT INTO @AT VALUES(169,'2013', '/2/1/3/', NULL,N'RevenueFromInterest', N'Interest income',N'The amount of income arising from interest.')
INSERT INTO @AT VALUES(170,'2014', '/2/1/4/', NULL,N'RevenueFromDividends', N'Dividend income',N'The amount of dividends recognised as income. Dividends are distributions of profits to holders of equity investments in proportion to their holdings of a particular class of capital.')
INSERT INTO @AT VALUES(171,'2015', '/2/1/5/', NULL,N'OtherRevenue', N'Other revenue',N'The amount of revenue arising from sources that the entity does not separately disclose in the same statement or note. [Refer: Revenue]')
INSERT INTO @AT VALUES(172,'202', '/2/2/', NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(173,'203', '/2/3/', NULL,N'ExpenseByNature', N'Expenses by nature',N'The amount of expenses aggregated according to their nature (for example, depreciation, purchases of materials, transport costs, employee benefits and advertising costs), and not reallocated among functions within the entity.')
INSERT INTO @AT VALUES(174,'2031', '/2/3/1/', NULL,N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used',N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(175,'2032', '/2/3/2/', NULL,N'CostOfMerchandiseSold', N'Cost of merchandise sold',N'The amount of merchandise that was sold during the period and recognised as an expense.')
INSERT INTO @AT VALUES(176,'2033', '/2/3/3/', NULL,N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
INSERT INTO @AT VALUES(177,'20331', '/2/3/3/1/', NULL,N'InsuranceExpense', N'Insurance expense',N'The amount of expense arising from purchased insurance.')
INSERT INTO @AT VALUES(178,'20332', '/2/3/3/2/', NULL,N'ProfessionalFeesExpense', N'Professional fees expense',N'The amount of fees paid or payable for professional services.')
INSERT INTO @AT VALUES(179,'20333', '/2/3/3/3/', NULL,N'TransportationExpense', N'Transportation expense',N'The amount of expense arising from transportation services.')
INSERT INTO @AT VALUES(180,'20334', '/2/3/3/4/', NULL,N'BankAndSimilarCharges', N'Bank and similar charges',N'The amount of bank and similar charges recognised by the entity as an expense.')
INSERT INTO @AT VALUES(181,'20335', '/2/3/3/5/', NULL,N'TravelExpense', N'Travel expense',N'The amount of expense arising from travel.')
INSERT INTO @AT VALUES(182,'20336', '/2/3/3/6/', NULL,N'CommunicationExpense', N'Communication expense',N'The amount of expense arising from communication.')
INSERT INTO @AT VALUES(183,'20337', '/2/3/3/7/', NULL,N'UtilitiesExpense', N'Utilities expense',N'The amount of expense arising from purchased utilities.')
INSERT INTO @AT VALUES(184,'20338', '/2/3/3/8/', NULL,N'AdvertisingExpense', N'Advertising expense',N'The amount of expense arising from advertising.')
INSERT INTO @AT VALUES(185,'2034', '/2/3/4/', NULL,N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(186,'20341', '/2/3/4/1/', NULL,N'ShorttermEmployeeBenefitsExpense', N'Short-term employee benefits expense',N'The amount of expense from employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services.')
INSERT INTO @AT VALUES(187,'203411', '/2/3/4/1/1/', NULL,N'WagesAndSalaries', N'Wages and salaries',N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(188,'203412', '/2/3/4/1/2/', NULL,N'SocialSecurityContributions', N'Social security contributions',N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(189,'203413', '/2/3/4/1/3/', NULL,N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits',N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(190,'20342', '/2/3/4/2/', NULL,N'PostemploymentBenefitExpenseDefinedContributionPlans', N'Post-employment benefit expense, defined contribution plans',N'The amount of post-employment benefit expense relating to defined contribution plans. Defined contribution plans are post-employment benefit plans under which an entity pays fixed contributions into a separate entity (a fund) and will have no legal or constructive obligation to pay further contributions if the fund does not hold sufficient assets to pay all employee benefits relating to employee service in the current and prior periods.')
INSERT INTO @AT VALUES(191,'20343', '/2/3/4/3/', NULL,N'PostemploymentBenefitExpenseDefinedBenefitPlans', N'Post-employment benefit expense, defined benefit plans',N'The amount of post-employment benefit expense relating to defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(192,'20344', '/2/3/4/4/', NULL,N'TerminationBenefitsExpense', N'Termination benefits expense',N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(193,'20345', '/2/3/4/5/', NULL,N'OtherLongtermBenefits', N'Other long-term employee benefits',N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(194,'20346', '/2/3/4/6/', NULL,N'OtherEmployeeExpense', N'Other employee expense',N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(195,'2035', '/2/3/5/', NULL,N'DepreciationAndAmortisationExpense', N'Depreciation and amortisation expense',N'The amount of depreciation and amortisation expense. Depreciation and amortisation are the systematic allocations of depreciable amounts of assets over their useful lives.')
INSERT INTO @AT VALUES(196,'20351', '/2/3/5/1/', NULL,N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
INSERT INTO @AT VALUES(197,'20352', '/2/3/5/2/', NULL,N'AmortisationExpense', N'Amortisation expense',N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
INSERT INTO @AT VALUES(198,'2036', '/2/3/6/', NULL,N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Reversal of impairment loss (impairment loss) recognised in profit or loss',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(199,'2037', '/2/3/7/', NULL,N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(200,'204', '/2/4/', NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(201,'2041', '/2/4/1/', NULL,N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension', N'Gain (loss) on disposal of property, plant and equipment',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(202,'2042', '/2/4/2/', NULL,N'GainLossOnForeignExchangeExtension', N'Gain (loss) on foreign exchange',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(203,'205', '/2/5/', NULL,N'GainsLossesOnNetMonetaryPosition', N'Gains (losses) on net monetary position',N'The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')
INSERT INTO @AT VALUES(204,'206', '/2/6/', NULL,N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost', N'Gain (loss) arising from derecognition of financial assets measured at amortised cost',N'The gain (loss) arising from the derecognition of financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(205,'207', '/2/7/', NULL,N'FinanceIncome', N'Finance income',N'The amount of income associated with interest and other financing activities of the entity.')
INSERT INTO @AT VALUES(206,'208', '/2/8/', NULL,N'FinanceCosts', N'Finance costs',N'The amount of costs associated with financing activities of the entity.')
INSERT INTO @AT VALUES(207,'209', '/2/9/', NULL,N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9', N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9',N'The amount of impairment loss, impairment gain or reversal of impairment loss that is recognised in profit or loss in accordance with paragraph 5.5.8 of IFRS 9 and that arises from applying the impairment requirements in Section 5.5 of IFRS 9.')
INSERT INTO @AT VALUES(208,'210', '/2/10/', NULL,N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod', N'Share of profit (loss) of associates and joint ventures accounted for using equity method',N'The entity''s share of the profit (loss) of associates and joint ventures accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method; Joint ventures [member]; Profit (loss)]')
INSERT INTO @AT VALUES(209,'211', '/2/11/', NULL,N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates', N'Other income (expense) from subsidiaries, jointly controlled entities and associates',N'The amount of income or expense from subsidiaries, jointly controlled entities and associates that the entity does not separately disclose in the same statement or note. [Refer: Associates [member]; Subsidiaries [member]]')
INSERT INTO @AT VALUES(210,'212', '/2/12/', NULL,N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue', N'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category',N'The gains (losses) arising from the difference between the previous amortised cost and the fair value of financial assets reclassified out of the amortised cost into the fair value through profit or loss measurement category. [Refer: At fair value [member]; Financial assets at amortised cost]')
INSERT INTO @AT VALUES(211,'213', '/2/13/', NULL,N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory', N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category',N'The cumulative gain (loss) previously recognised in other comprehensive income arising from the reclassification of financial assets out of the fair value through other comprehensive income into the fair value through profit or loss measurement category. [Refer: Financial assets measured at fair value through other comprehensive income; Financial assets at fair value through profit or loss; Other comprehensive income]')
INSERT INTO @AT VALUES(212,'214', '/2/14/', NULL,N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions', N'Hedging gains (losses) for hedge of group of items with offsetting risk positions',N'The hedging gains (losses) for hedge of group of items with offsetting risk positions.')
INSERT INTO @AT VALUES(213,'215', '/2/15/', NULL,N'IncomeTaxExpenseContinuingOperations', N'Tax income (expense)',N'The aggregate amount included in the determination of profit (loss) for the period in respect of current tax and deferred tax. [Refer: Current tax expense (income); Deferred tax expense (income)]')
INSERT INTO @AT VALUES(214,'3', '/3/', NULL,N'ControlAccountsExtension', N'Control Accounts',N'')
INSERT INTO @AT VALUES(215,'31', '/3/1/', NULL,N'DocumentControlExtension', N'Document Control',N'')
INSERT INTO @AT VALUES(216,'311', '/3/1/1/', NULL,N'CashControlExtension', N'Cash control',N'')
INSERT INTO @AT VALUES(217,'312', '/3/1/2/', NULL,N'TradingControlExtension', N'Trading control',N'')
INSERT INTO @AT VALUES(218,'313', '/3/1/3/', NULL,N'PayrollControlExtension', N'Payroll control',N'')
INSERT INTO @AT VALUES(219,'319', '/3/1/9/', NULL,N'OtherControlExtension', N'Other document control',N'')
INSERT INTO @AT VALUES(220,'32', '/3/2/', NULL,N'FinalAccountsControlExtension', N'Final Account Control',N'')

	INSERT INTO @AccountTypes ([Index], [Code], [Concept], [Name], [ParentIndex], 
			[EntryTypeParentId], [Description])
	SELECT RC.[Index], RC.[Code], RC.[Concept], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex,
			(SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = RC.EntryTypeParentCode), [Description]
	FROM @AT RC;
	UPDATE @AccountTypes SET IsAssignable = 1
	WHERE [Index] NOT IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)
	UPDATE @AccountTypes SET IsAssignable = 0
	WHERE [Index] IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)

	EXEC [api].[AccountTypes__Save]
		@Entities = @AccountTypes,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	--UPDATE DB
	--SET DB.[Node] = FE.[Node]
	--FROM dbo.[AccountTypes] DB JOIN @AT FE ON DB.[Code] = FE.[Code]

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Account Types: Provisioning: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END
--DECLARE @ExpensesExtension INT = NULL -- just for convenience
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
DECLARE @InvestmentsInSubsidiariesJointVenturesAndAssociates INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'InvestmentsInSubsidiariesJointVenturesAndAssociates');
DECLARE @NoncurrentBiologicalAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentBiologicalAssets');
DECLARE @NoncurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivables');
DECLARE @NoncurrentTradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentTradeReceivables');
DECLARE @NoncurrentReceivablesDueFromRelatedParties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesDueFromRelatedParties');
DECLARE @NoncurrentPrepaymentsAndNoncurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPrepaymentsAndNoncurrentAccruedIncome');
DECLARE @NoncurrentPrepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPrepayments');
DECLARE @NoncurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAccruedIncome');
DECLARE @NoncurrentReceivablesFromTaxesOtherThanIncomeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesFromTaxesOtherThanIncomeTax');
DECLARE @NoncurrentReceivablesFromSaleOfProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesFromSaleOfProperties');
DECLARE @NoncurrentReceivablesFromRentalOfProperties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentReceivablesFromRentalOfProperties');
DECLARE @OtherNoncurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentReceivables');
DECLARE @NoncurrentInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentInventories');
DECLARE @DeferredTaxAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredTaxAssets');
DECLARE @CurrentTaxAssetsNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxAssetsNoncurrent');
DECLARE @OtherNoncurrentFinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentFinancialAssets');
DECLARE @OtherNoncurrentNonfinancialAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentNonfinancialAssets');
DECLARE @NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral');
DECLARE @CurrentAssets INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentAssets');
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
DECLARE @StaffDebtorsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'StaffDebtorsExtension');
DECLARE @SundryDebtorsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'SundryDebtorsExtension');
DECLARE @CollectionGuaranteeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CollectionGuaranteeExtension');
DECLARE @DishonouredGuaranteeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DishonouredGuaranteeExtension');
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
DECLARE @ChecksUnderCollectionExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ChecksUnderCollectionExtension');
DECLARE @CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral');
DECLARE @NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners');
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
DECLARE @NoncurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'NoncurrentPayables');
DECLARE @DeferredTaxLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredTaxLiabilities');
DECLARE @CurrentTaxLiabilitiesNoncurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxLiabilitiesNoncurrent');
DECLARE @OtherNoncurrentFinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentFinancialLiabilities');
DECLARE @OtherNoncurrentNonfinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherNoncurrentNonfinancialLiabilities');
DECLARE @CurrentLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentLiabilities');
DECLARE @CurrentProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentProvisions');
DECLARE @CurrentProvisionsForEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentProvisionsForEmployeeBenefits');
DECLARE @OtherShorttermProvisions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherShorttermProvisions');
DECLARE @TradeAndOtherCurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentPayables');
DECLARE @TradeAndOtherCurrentPayablesToTradeSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentPayablesToTradeSuppliers');
DECLARE @TradeAndOtherCurrentPayablesToRelatedParties INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradeAndOtherCurrentPayablesToRelatedParties');
DECLARE @DeferredIncomeClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DeferredIncomeClassifiedAsCurrent');
DECLARE @AccrualsClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AccrualsClassifiedAsCurrent');
DECLARE @ShorttermEmployeeBenefitsAccruals INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ShorttermEmployeeBenefitsAccruals');
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
DECLARE @CurrentRetentionPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentRetentionPayables');
DECLARE @OtherCurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentPayables');
DECLARE @CurrentTaxLiabilitiesCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CurrentTaxLiabilitiesCurrent');
DECLARE @OtherCurrentFinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentFinancialLiabilities');
DECLARE @OtherCurrentNonfinancialLiabilities INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherCurrentNonfinancialLiabilities');
DECLARE @LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale');
DECLARE @IncomeStatementAbstract INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'IncomeStatementAbstract');
DECLARE @Revenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'Revenue');
DECLARE @RevenueFromSaleOfGoods INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromSaleOfGoods');
DECLARE @RevenueFromRenderingOfServices INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromRenderingOfServices');
DECLARE @RevenueFromInterest INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromInterest');
DECLARE @RevenueFromDividends INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'RevenueFromDividends');
DECLARE @OtherRevenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherRevenue');
DECLARE @OtherIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherIncome');
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
DECLARE @DepreciationAndAmortisationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationAndAmortisationExpense');
DECLARE @DepreciationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DepreciationExpense');
DECLARE @AmortisationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'AmortisationExpense');
DECLARE @ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss');
DECLARE @OtherExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherExpenseByNature');
DECLARE @OtherGainsLosses INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherGainsLosses');
DECLARE @GainLossOnDisposalOfPropertyPlantAndEquipmentExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension');
DECLARE @GainLossOnForeignExchangeExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'GainLossOnForeignExchangeExtension');
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
DECLARE @ControlAccountsExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'ControlAccountsExtension');
DECLARE @DocumentControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'DocumentControlExtension');
DECLARE @CashControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'CashControlExtension');
DECLARE @TradingControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'TradingControlExtension');
DECLARE @PayrollControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'PayrollControlExtension');
DECLARE @OtherControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'OtherControlExtension');
DECLARE @FinalAccountsControlExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Concept] = N'FinalAccountsControlExtension');

END

UPDATE dbo.[AccountTypes] SET IsActive = 0 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsActive = 0);
DECLARE @ServicesExpenseNode HIERARCHYID = (SELECT [Node] FROM dbo.AccountTypes WHERE [Id] = @ServicesExpense);
UPDATE dbo.[AccountTypes] SET IsSystem = 1 WHERE [Node].IsDescendantOf(@ServicesExpenseNode) = 0;

INSERT INTO dbo.[AccountTypeResourceDefinitions]
([AccountTypeId],									[ResourceDefinitionId]) VALUES
(@ChecksUnderCollectionExtension,					@CheckReceivedRD),
(@CurrentInventoriesHeldForSale,					@TradeMedicineRD),
(@CurrentInventoriesHeldForSale,					@TradeConstructionMaterialRD),
(@CurrentInventoriesHeldForSale,					@TradeSparePartRD),
(@CurrentInventoriesHeldForSale,					@FinishedGrainRD),
(@CurrentInventoriesHeldForSale,					@FinishedVehicleRD),
(@CurrentInventoriesHeldForSale,					@FinishedOilRD),
(@CurrentInventoriesHeldForSale,					@ByproductGrainRD),
(@CurrentInventoriesHeldForSale,					@ByproductOilRD),
(@Merchandise,										@TradeMedicineRD),
(@Merchandise,										@TradeConstructionMaterialRD),
(@Merchandise,										@TradeSparePartRD),
(@FinishedGoods,									@FinishedGrainRD),
(@FinishedGoods,									@FinishedVehicleRD),
(@FinishedGoods,									@FinishedOilRD),
(@FinishedGoods,									@ByproductGrainRD),
(@FinishedGoods,									@ByproductOilRD),

(@WorkInProgress,									@WorkInProgressRD),

(@CurrentRawMaterialsAndCurrentProductionSupplies,	@RawGrainRD),
(@CurrentRawMaterialsAndCurrentProductionSupplies,	@RawVehicleRD),
(@RawMaterials,										@RawGrainRD),
(@RawMaterials,										@RawVehicleRD),
--(@PropertyIntendedForSaleInOrdinaryCourseOfBusiness, ),

(@Land,												@LandMemberRD),
(@Buildings,										@BuildingsMemberRD),
(@Buildings,										@LeaseholdImprovementsMemberRD),
(@Machinery,										@MachineryMemberRD), 
(@Machinery,										@PowerGeneratingAssetsMemberRD), 
(@Vehicles,											@MotorVehiclesMemberRD),
(@FixturesAndFittings,								@FixturesAndFittingsMemberRD),
(@FixturesAndFittings,								@NetworkInfrastructureMemberRD),
(@OfficeEquipment,									@OfficeEquipmentMemberRD),
(@OfficeEquipment,									@ComputerEquipmentMemberRD),
(@OfficeEquipment,									@CommunicationAndNetworkEquipmentMemberRD),
(@OfficeEquipment,									@ComputerEquipmentMemberRD),
(@BearerPlants,										@BearerPlantsMemberRD),
(@TangibleExplorationAndEvaluationAssets,			@TangibleExplorationAndEvaluationAssetsMemberRD),
(@MiningAssets,										@MiningAssetsMemberRD),
(@OilAndGasAssets,									@OilAndGasAssetsMemberRD),
(@ConstructionInProgress,							@ConstructionInProgressMemberRD),
(@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel,
													@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(@OtherPropertyPlantAndEquipment,					@OtherPropertyPlantAndEquipmentMemberRD),
(@InvestmentProperty,								@InvestmentPropertyCompletedMemberRD),
(@InvestmentProperty,								@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),

(@CollectionGuaranteeExtension,						@CheckReceivedRD),	
(@DishonouredGuaranteeExtension,					@CheckReceivedRD),	

(@RevaluationSurplus,								@LandMemberRD),
(@RevaluationSurplus,								@BuildingsMemberRD),
(@RevaluationSurplus,								@LeaseholdImprovementsMemberRD),
--(@RevaluationSurplus,								@MachineryMemberRD), 
--(@RevaluationSurplus,								@PowerGeneratingAssetsMemberRD), 
--(@RevaluationSurplus,								@MotorVehiclesMemberRD),
--(@RevaluationSurplus,								@FixturesAndFittingsMemberRD),
--(@RevaluationSurplus,								@NetworkInfrastructureMemberRD),
--(@RevaluationSurplus,								@OfficeEquipmentMemberRD),
--(@RevaluationSurplus,								@ComputerEquipmentMemberRD),
--(@RevaluationSurplus,								@CommunicationAndNetworkEquipmentMemberRD),
--(@RevaluationSurplus,								@ComputerEquipmentMemberRD),
--(@RevaluationSurplus,								@BearerPlantsMemberRD),
--(@RevaluationSurplus,								@TangibleExplorationAndEvaluationAssetsMemberRD),
--(@RevaluationSurplus,								@MiningAssetsMemberRD),
--(@RevaluationSurplus,								@OilAndGasAssetsMemberRD),
--(@RevaluationSurplus,								@ConstructionInProgressMemberRD),
--(@RevaluationSurplus,								@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
--(@RevaluationSurplus,								@OtherPropertyPlantAndEquipmentMemberRD),
(@RevaluationSurplus,								@InvestmentPropertyCompletedMemberRD),
(@RevaluationSurplus,								@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),

(@RevenueFromSaleOfGoods,							@TradeMedicineRD),
(@RevenueFromSaleOfGoods,							@TradeConstructionMaterialRD),
(@RevenueFromSaleOfGoods,							@TradeSparePartRD),
(@RevenueFromSaleOfGoods,							@FinishedGrainRD),
(@RevenueFromSaleOfGoods,							@FinishedVehicleRD),
(@RevenueFromSaleOfGoods,							@FinishedOilRD),
(@RevenueFromSaleOfGoods,							@ByproductGrainRD),
(@RevenueFromSaleOfGoods,							@ByproductOilRD),

(@RevenueFromRenderingOfServices,					@RevenueServiceRD),

--(@RevenueFromDividends,					InvestmentAccountedForUsingEquityMethod
--(@RevenueFromDividends,					InvestmentsInSubsidiariesJointVenturesAndAssociates

(@EmployeeBenefitsExpense,							@EmployeeBenefitRD),
(@ShorttermEmployeeBenefitsExpense,					@EmployeeBenefitRD),
(@WagesAndSalaries,									@EmployeeBenefitRD),
(@SocialSecurityContributions,						@EmployeeBenefitRD), 
(@OtherShorttermEmployeeBenefits,					@EmployeeBenefitRD), 
(@PostemploymentBenefitExpenseDefinedContributionPlans,	
													@EmployeeBenefitRD),
(@PostemploymentBenefitExpenseDefinedBenefitPlans,
													@EmployeeBenefitRD), 
(@TerminationBenefitsExpense,						@EmployeeBenefitRD),
(@OtherLongtermBenefits,							@EmployeeBenefitRD), 
(@OtherEmployeeExpense,								@EmployeeBenefitRD);

INSERT INTO dbo.[AccountTypeContractDefinitions]
([AccountTypeId],								[ContractDefinitionId]) VALUES
(@CashOnHand,									@CashOnHandAccountCD),
(@BalancesWithBanks	,							@BankAccountCD),
(@ChecksUnderCollectionExtension,				@BankAccountCD),

(@CurrentInventoriesHeldForSale,				@WarehouseCD),
(@CurrentRawMaterialsAndCurrentProductionSupplies,@WarehouseCD),
(@RawMaterials,									@WarehouseCD),
(@ProductionSupplies,							@WarehouseCD),
(@WorkInProgress,								@WarehouseCD),
(@FinishedGoods,								@WarehouseCD),
(@CurrentInventoriesInTransit,					@shipperCD),
(@CurrentPrepayments,							@SupplierCD),
(@StaffDebtorsExtension,						@DebtorCD), -- split it into 2 sundry debtor
(@SundryDebtorsExtension,						@EmployeeCD), -- staff debtor

(@CollectionGuaranteeExtension,					@CustomerCD),	
(@DishonouredGuaranteeExtension,				@CustomerCD),	
(@TradeAndOtherCurrentPayablesToTradeSuppliers,	@SupplierCD),
(@AccrualsClassifiedAsCurrent,					@SupplierCD), -- split into two
(@AccrualsClassifiedAsCurrent,					@EmployeeCD), -- last  5 days unpaid
(@DeferredIncomeClassifiedAsCurrent,			@CustomerCD),
(@CurrentTradeReceivables,						@CustomerCD),
(@CurrentAccruedIncome,							@CustomerCD),
(@OtherCurrentFinancialLiabilities,				@CreditorCD),
(@OtherCurrentFinancialLiabilities,				@PartnerCD),
(@CashControlExtension,							@SupplierCD),
(@CashControlExtension,							@CustomerCD);

INSERT INTO dbo.[AccountTypeNotedContractDefinitions]
([AccountTypeId],								[NotedContractDefinitionId]) VALUES
(@CurrentValueAddedTaxReceivables,				@SupplierCD),
(@CurrentValueAddedTaxPayables,					@CustomerCD),
(@CurrentEmployeeIncomeTaxPayablesExtension,	@EmployeeCD),
(@CurrentSocialSecurityPayablesExtension,		@EmployeeCD),
(@CurrentEmployeeStampTaxPayablesExtension,		@EmployeeCD);


INSERT INTO dbo.[AccountTypeCenterTypes]
([AccountTypeId],			[CenterType]) VALUES
(@CashControlExtension,		N'Parent'),
(@CashAndCashEquivalents,	N'Parent');