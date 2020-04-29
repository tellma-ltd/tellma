IF NOT EXISTS(SELECT * FROM dbo.[AccountTypes])
BEGIN
	DECLARE @AT TABLE (
		[Index] INT,[IsResourceClassification] BIT, [IsCurrent] BIT, [IsActive] BIT, [IsAssignable] BIT, [IsReal] BIT, [IsPersonal] BIT, [IsSystem] BIT,
		[Node] HIERARCHYID, [EntryTypeParentCode] NVARCHAR (255), [Code] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
	)
INSERT INTO @AT VALUES(0,0,NULL, 1,0,0,0,1,'/1/', NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
INSERT INTO @AT VALUES(1,0,NULL, 1,0,0,0,1,'/1/1/', NULL,N'AssetsAbstract', N'Assets [abstract]',N'')
INSERT INTO @AT VALUES(2,1,0, 1,1,1,0,1,'/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
INSERT INTO @AT VALUES(3,1,0, 1,0,1,0,0,'/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'LandAndBuildingsAbstract', N'Land and buildings [abstract]',N'')
INSERT INTO @AT VALUES(4,1,0, 1,1,1,0,0,'/1/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'Land', N'Land',N'The amount of property, plant and equipment representing land held by the entity for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(5,1,0, 1,1,1,0,0,'/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Buildings', N'Buildings',N'The amount of property, plant and equipment representing depreciable buildings and similar structures for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(6,1,0, 1,1,1,0,0,'/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Machinery', N'Machinery',N'The amount of property, plant and equipment representing long-lived, depreciable machinery used in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(7,1,0, 0,0,1,0,0,'/1/1/1/3/', N'ChangesInPropertyPlantAndEquipment',N'VehiclesAbstract', N'Vehicles [abstract]',N'')
INSERT INTO @AT VALUES(8,1,0, 0,1,1,0,0,'/1/1/1/3/1/', N'ChangesInPropertyPlantAndEquipment',N'Ships', N'Ships',N'The amount of property, plant and equipment representing seafaring or other maritime vessels used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(9,1,0, 0,1,1,0,0,'/1/1/1/3/2/', N'ChangesInPropertyPlantAndEquipment',N'Aircraft', N'Aircraft',N'The amount of property, plant and equipment representing aircraft used in the entity''s operations.')
INSERT INTO @AT VALUES(10,1,0, 1,1,1,1,0,'/1/1/1/3/3/', N'ChangesInPropertyPlantAndEquipment',N'MotorVehicles', N'Motor vehicles',N'The amount of property, plant and equipment representing self-propelled ground vehicles used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(11,1,0, 1,1,1,0,0,'/1/1/1/4/', N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(12,1,0, 1,1,1,0,0,'/1/1/1/5/', N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(13,1,0, 1,1,1,0,0,'/1/1/1/5/1/', N'ChangesInPropertyPlantAndEquipment',N'ComputerEquipmentMemberExtension', N'Computer equipment',N'The amount of property, plant and equipment representing computer accessories. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(14,1,0, 1,1,1,0,0,'/1/1/1/5/2/', N'ChangesInPropertyPlantAndEquipment',N'ComputerAccessoriesExtension', N'Computer accessories',N'The amount of property, plant and equipment representing computer equipment. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(15,1,0, 0,1,1,0,0,'/1/1/1/6/', N'ChangesInPropertyPlantAndEquipment',N'BearerPlants', N'Bearer plants',N'The amount of property, plant and equipment representing bearer plants. Bearer plant is a living plant that (a) is used in the production or supply of agricultural produce; (b) is expected to bear produce for more than one period; and (c) has a remote likelihood of being sold as agricultural produce, except for incidental scrap sales. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(16,1,0, 0,1,1,0,0,'/1/1/1/7/', N'ChangesInPropertyPlantAndEquipment',N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',N'The amount of exploration and evaluation assets recognised as tangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(17,1,0, 0,1,1,0,0,'/1/1/1/8/', N'ChangesInPropertyPlantAndEquipment',N'MiningAssets', N'Mining assets',N'The amount of assets related to mining activities of the entity.')
INSERT INTO @AT VALUES(18,1,0, 0,1,1,0,0,'/1/1/1/9/', N'ChangesInPropertyPlantAndEquipment',N'OilAndGasAssets', N'Oil and gas assets',N'The amount of assets related to the exploration, evaluation, development or production of oil and gas.')
INSERT INTO @AT VALUES(19,1,0, 0,1,1,0,0,'/1/1/1/10/', N'ChangesInPropertyPlantAndEquipment',N'ConstructionInProgress', N'Construction in progress',N'The amount of expenditure capitalised during the construction of non-current assets that are not yet available for use. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(20,1,0, 0,1,1,0,0,'/1/1/1/11/', N'ChangesInPropertyPlantAndEquipment',N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', N'Owner-occupied property measured using investment property fair value model',N'The amount of property, plant and equipment representing owner-occupied property measured using the investment property fair value model applying paragraph 29A of IAS 16. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(21,1,0, 1,1,1,0,0,'/1/1/1/12/', N'ChangesInPropertyPlantAndEquipment',N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment',N'The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(22,1,0, 0,1,1,0,1,'/1/1/2/', N'ChangesInInvestmentProperty',N'InvestmentProperty', N'Investment property',N'The amount of property (land or a building - or part of a building - or both) held (by the owner or by the lessee as a right-of-use asset) to earn rentals or for capital appreciation or both, rather than for: (a) use in the production or supply of goods or services or for administrative purposes; or (b) sale in the ordinary course of business.')
INSERT INTO @AT VALUES(23,0,0, 0,1,1,0,0,'/1/1/2/1/', N'ChangesInInvestmentProperty',N'InvestmentPropertyCompleted', N'Investment property completed',N'The amount of investment property whose construction or development is complete. [Refer: Investment property]')
INSERT INTO @AT VALUES(24,0,0, 0,1,1,0,0,'/1/1/2/2/', N'ChangesInInvestmentProperty',N'InvestmentPropertyUnderConstructionOrDevelopment', N'Investment property under construction or development',N'The amount of property that is being constructed or developed for future use as investment property. [Refer: Investment property]')
INSERT INTO @AT VALUES(25,0,0, 0,1,0,0,1,'/1/1/3/', N'ChangesInGoodwill',N'Goodwill', N'Goodwill',N'The amount of assets representing the future economic benefits arising from other assets acquired in a business combination that are not individually identified and separately recognised. [Refer: Business combinations [member]]')
INSERT INTO @AT VALUES(26,1,0, 0,1,1,0,1,'/1/1/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(27,1,0, 0,1,1,0,0,'/1/1/4/1/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'BrandNames', N'Brand names',N'The amount of intangible assets representing rights to a group of complementary assets such as a trademark (or service mark) and its related trade name, formulas, recipes and technological expertise. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(28,1,0, 0,1,1,1,0,'/1/1/4/2/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleExplorationAndEvaluationAssets', N'Intangible exploration and evaluation assets',N'The amount of exploration and evaluation assets recognised as intangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(29,1,0, 0,1,1,1,0,'/1/1/4/3/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'MastheadsAndPublishingTitles', N'Mastheads and publishing titles',N'The amount of intangible assets representing rights acquired through registration to use mastheads and publishing titles. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(30,1,0, 0,1,1,1,0,'/1/1/4/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'ComputerSoftware', N'Computer software',N'The amount of intangible assets representing computer software. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(31,1,0, 0,1,1,1,0,'/1/1/4/5/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'LicencesAndFranchises', N'Licences and franchises',N'The amount of intangible assets representing the right to use certain intangible assets owned by another entity and the right to operate a business using the name, merchandise, services, methodologies, promotional support, marketing and supplies granted by another entity. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(32,1,0, 0,1,1,1,0,'/1/1/4/6/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRights', N'Copyrights, patents and other industrial property rights, service and operating rights',N'The amount of intangible assets representing copyrights, patents and other industrial property rights, service and operating rights. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(33,1,0, 0,1,1,1,0,'/1/1/4/7/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'RecipesFormulaeModelsDesignsAndPrototypes', N'Recipes, formulae, models, designs and prototypes',N'The amount of intangible assets representing recipes, formulae, models, designs and prototypes. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(34,1,0, 0,1,1,1,0,'/1/1/4/8/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsUnderDevelopment', N'Intangible assets under development',N'The amount of intangible assets representing such assets under development. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(35,1,0, 0,1,1,0,0,'/1/1/4/9/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'OtherIntangibleAssets', N'Other intangible assets',N'The amount of intangible assets that the entity does not separately disclose in the same statement or note. [Refer: Intangible assets other than goodwill]')
INSERT INTO @AT VALUES(36,0,NULL, 1,1,1,1,1,'/1/1/5/', NULL,N'OtherFinancialAssets', N'Other financial assets',N'The amount of financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(37,0,NULL, 1,1,1,1,1,'/1/1/6/', NULL,N'OtherNonfinancialAssets', N'Other non-financial assets',N'The amount of non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(38,0,0, 0,1,1,0,1,'/1/1/7/', NULL,N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',N'The amount of investments accounted for using the equity method. The equity method is a method of accounting whereby the investment is initially recognised at cost and adjusted thereafter for the post-acquisition change in the investor''s share of net assets of the investee. The investor''s profit or loss includes its share of the profit or loss of the investee. The investor''s other comprehensive income includes its share of the other comprehensive income of the investee. [Refer: At cost [member]]')
INSERT INTO @AT VALUES(39,0,0, 0,1,1,0,0,'/1/1/7/1/', NULL,N'InvestmentsInAssociatesAccountedForUsingEquityMethod', N'Investments in associates accounted for using equity method',N'The amount of investments in associates accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method]')
INSERT INTO @AT VALUES(40,0,0, 0,1,1,0,0,'/1/1/7/2/', NULL,N'InvestmentsInJointVenturesAccountedForUsingEquityMethod', N'Investments in joint ventures accounted for using equity method',N'The amount of investments in joint ventures accounted for using the equity method. [Refer: Joint ventures [member]; Investments accounted for using equity method]')
INSERT INTO @AT VALUES(41,0,0, 0,1,1,0,1,'/1/1/8/', NULL,N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',N'The amount of investments in subsidiaries, joint ventures and associates in an entity''s separate financial statements. [Refer: Associates [member]; Joint ventures [member]; Subsidiaries [member]; Investments in subsidiaries]')
INSERT INTO @AT VALUES(42,0,0, 0,1,1,0,0,'/1/1/8/1/', NULL,N'InvestmentsInSubsidiaries', N'Investments in subsidiaries',N'The amount of investments in subsidiaries in an entity''s separate financial statements. [Refer: Subsidiaries [member]]')
INSERT INTO @AT VALUES(43,0,0, 0,1,1,0,0,'/1/1/8/2/', NULL,N'InvestmentsInJointVentures', N'Investments in joint ventures',N'The amount of investments in joint ventures in an entity''s separate financial statements. [Refer: Joint ventures [member]]')
INSERT INTO @AT VALUES(44,0,0, 0,1,1,0,0,'/1/1/8/3/', NULL,N'InvestmentsInAssociates', N'Investments in associates',N'The amount of investments in associates in an entity''s separate financial statements. [Refer: Associates [member]]')
INSERT INTO @AT VALUES(45,1,NULL, 0,1,1,0,1,'/1/1/9/', N'ChangesInBiologicalAssets',N'BiologicalAssets', N'Biological assets',N'The amount of living animals or plants recognised as assets.')
INSERT INTO @AT VALUES(46,0,NULL, 0,1,1,0,1,'/1/1/10/', NULL,N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners',N'The amount of non-current assets or disposal groups classified as held for sale or as held for distribution to owners. [Refer: Non-current assets or disposal groups classified as held for distribution to owners; Non-current assets or disposal groups classified as held for sale]')
INSERT INTO @AT VALUES(47,1,NULL, 0,1,1,1,1,'/1/1/11/', N'ChangesInInventories',N'InventoriesTotal', N'Inventories',N'The amount of assets: (a) held for sale in the ordinary course of business; (b) in the process of production for such sale; or (c) in the form of materials or supplies to be consumed in the production process or in the rendering of services. Inventories encompass goods purchased and held for resale including, for example, merchandise purchased by a retailer and held for resale, or land and other property held for resale. Inventories also encompass finished goods produced, or work in progress being produced, by the entity and include materials and supplies awaiting use in the production process. [Refer: Current finished goods; Current merchandise; Current work in progress; Land]')
INSERT INTO @AT VALUES(48,1,1, 0,0,1,1,0,'/1/1/11/1/', N'ChangesInInventories',N'CurrentRawMaterialsAndCurrentProductionSuppliesAbstract', N'Current raw materials and current production supplies [abstract]',N'')
INSERT INTO @AT VALUES(49,1,1, 0,1,1,1,1,'/1/1/11/1/1/', N'ChangesInInventories',N'RawMaterials', N'Current raw materials',N'A classification of current inventory representing the amount of assets to be consumed in the production process or in the rendering of services. [Refer: Inventories]')
INSERT INTO @AT VALUES(50,1,1, 0,1,1,1,0,'/1/1/11/1/2/', N'ChangesInInventories',N'ProductionSupplies', N'Current production supplies',N'A classification of current inventory representing the amount of supplies to be used for the production process. [Refer: Inventories]')
INSERT INTO @AT VALUES(51,1,0, 0,1,1,1,1,'/1/1/11/2/', N'ChangesInInventories',N'Merchandise', N'Current merchandise',N'A classification of current inventory representing the amount of goods acquired for resale. [Refer: Inventories]')
INSERT INTO @AT VALUES(52,1,0, 0,1,1,1,0,'/1/1/11/3/', N'ChangesInInventories',N'CurrentFoodAndBeverage', N'Current food and beverage',N'A classification of current inventory representing the amount of food and beverage. [Refer: Inventories]')
INSERT INTO @AT VALUES(53,1,0, 0,1,1,1,0,'/1/1/11/4/', N'ChangesInInventories',N'CurrentAgriculturalProduce', N'Current agricultural produce',N'A classification of current inventory representing the amount of harvested produce of the entity''s biological assets. [Refer: Biological assets; Inventories]')
INSERT INTO @AT VALUES(54,1,1, 0,1,1,1,1,'/1/1/11/5/', N'ChangesInInventories',N'WorkInProgress', N'Current work in progress',N'A classification of current inventory representing the amount of assets currently in production, which require further processes to be converted into finished goods or services. [Refer: Current finished goods; Inventories]')
INSERT INTO @AT VALUES(55,1,1, 0,1,1,1,1,'/1/1/11/6/', N'ChangesInInventories',N'FinishedGoods', N'Current finished goods',N'A classification of current inventory representing the amount of goods that have completed the production process and are held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(56,1,1, 0,1,1,1,0,'/1/1/11/7/', N'ChangesInInventories',N'CurrentPackagingAndStorageMaterials', N'Current packaging and storage materials',N'A classification of current inventory representing the amount of packaging and storage materials. [Refer: Inventories]')
INSERT INTO @AT VALUES(57,1,1, 0,1,1,1,0,'/1/1/11/8/', N'ChangesInInventories',N'SpareParts', N'Current spare parts',N'A classification of current inventory representing the amount of interchangeable parts that are kept in an inventory and are used for the repair or replacement of failed parts. [Refer: Inventories]')
INSERT INTO @AT VALUES(58,1,1, 0,1,1,0,0,'/1/1/11/9/', N'ChangesInInventories',N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property intended for sale in ordinary course of business',N'The amount of property intended for sale in the ordinary course of business of the entity. Property is land or a building - or part of a building - or both.')
INSERT INTO @AT VALUES(59,0,1, 0,1,1,1,1,'/1/1/11/10/', N'ChangesInInventories',N'CurrentInventoriesInTransit', N'Current inventories in transit',N'A classification of current inventory representing the amount of inventories in transit. [Refer: Inventories]')
INSERT INTO @AT VALUES(60,1,1, 0,1,1,1,0,'/1/1/11/11/', N'ChangesInInventories',N'OtherInventories', N'Other current inventories',N'The amount of inventory that the entity does not separately disclose in the same statement or note. [Refer: Inventories]')
INSERT INTO @AT VALUES(61,0,NULL, 0,1,0,0,0,'/1/1/12/', NULL,N'CurrentTaxAssets', N'Current tax assets',N'The excess of amount paid for current tax in respect of current and prior periods over the amount due for those periods. Current tax is the amount of income taxes payable (recoverable) in respect of the taxable profit (tax loss) for a period.')
INSERT INTO @AT VALUES(62,0,NULL, 0,1,0,0,0,'/1/1/13/', NULL,N'DeferredTaxAssets', N'Deferred tax assets',N'The amounts of income taxes recoverable in future periods in respect of: (a) deductible temporary differences; (b) the carryforward of unused tax losses; and (c) the carryforward of unused tax credits. [Refer: Temporary differences [member]; Unused tax credits [member]; Unused tax losses [member]]')
INSERT INTO @AT VALUES(63,0,NULL, 1,0,0,1,0,'/1/1/14/', NULL,N'TradeAndOtherReceivables', N'Trade and other receivables',N'The amount of trade receivables and other receivables. [Refer: Trade receivables; Other receivables]')
INSERT INTO @AT VALUES(64,0,NULL, 1,1,0,1,1,'/1/1/14/1/', NULL,N'TradeReceivables', N'Trade receivables',N'The amount due from customers for goods and services sold.')
INSERT INTO @AT VALUES(65,0,NULL, 1,1,0,1,1,'/1/1/14/2/', NULL,N'Prepayments', N'Prepayments',N'Receivables that represent amounts paid for goods and services before they have been delivered.')
INSERT INTO @AT VALUES(66,0,NULL, 1,1,0,1,1,'/1/1/14/3/', NULL,N'AccruedIncome', N'Accrued income',N'The amount of asset representing income that has been earned but is not yet received.')
INSERT INTO @AT VALUES(67,0,NULL, 1,1,0,0,0,'/1/1/14/4/', NULL,N'ReceivablesFromTaxesOtherThanIncomeTax', N'Receivables from taxes other than income tax',N'The amount of receivables from taxes other than income tax. Income taxes include all domestic and foreign taxes that are based on taxable profits. Income taxes also include taxes, such as withholding taxes, that are payable by a subsidiary, associate or joint arrangement on distributions to the reporting entity.')
INSERT INTO @AT VALUES(68,0,NULL, 1,1,0,0,1,'/1/1/14/4/1/', NULL,N'ValueAddedTaxReceivables', N'Value added tax receivables',N'The amount of receivables related to a value added tax.')
INSERT INTO @AT VALUES(69,0,1, 1,1,0,0,1,'/1/1/14/4/2/', NULL,N'WithholdingTaxReceivablesExtension', N'Withholding tax receivables',N'The amount of receivables related to a withtholding tax.')
INSERT INTO @AT VALUES(70,0,NULL, 1,1,0,1,0,'/1/1/14/5/', NULL,N'OtherReceivables', N'Other receivables',N'The amount receivable by the entity that it does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(71,0,1, 1,0,0,1,0,'/1/1/15/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(72,0,1, 1,0,0,1,0,'/1/1/15/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashAbstract', N'Cash [abstract]',N'')
INSERT INTO @AT VALUES(73,0,1, 1,1,0,1,1,'/1/1/15/1/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashOnHand', N'Cash on hand',N'The amount of cash held by the entity. This does not include demand deposits.')
INSERT INTO @AT VALUES(74,0,1, 1,1,0,1,1,'/1/1/15/1/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'BalancesWithBanks', N'Balances with banks',N'The amount of cash balances held at banks.')
INSERT INTO @AT VALUES(75,0,1, 0,0,0,1,0,'/1/1/15/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashEquivalentsAbstract', N'Cash equivalents [abstract]',N'')
INSERT INTO @AT VALUES(76,0,1, 0,1,0,1,0,'/1/1/15/2/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ShorttermDepositsClassifiedAsCashEquivalents', N'Short-term deposits, classified as cash equivalents',N'A classification of cash equivalents representing short-term deposits. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(77,0,1, 0,1,1,1,0,'/1/1/15/2/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ShorttermInvestmentsClassifiedAsCashEquivalents', N'Short-term investments, classified as cash equivalents',N'A classification of cash equivalents representing short-term investments. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(78,0,1, 0,1,0,1,0,'/1/1/15/2/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'BankingArrangementsClassifiedAsCashEquivalents', N'Other banking arrangements, classified as cash equivalents',N'A classification of cash equivalents representing banking arrangements that the entity does not separately disclose in the same statement or note. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(79,0,1, 0,1,0,1,0,'/1/1/15/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'OtherCashAndCashEquivalents', N'Other cash and cash equivalents',N'The amount of cash and cash equivalents that the entity does not separately disclose in the same statement or note. [Refer: Cash and cash equivalents]')
INSERT INTO @AT VALUES(80,0,1, 0,1,1,1,1,'/1/1/15/3/1/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ReceivedChecksExtension', N'Received Checks',N'The amount of checks received bit not deposited yet')
INSERT INTO @AT VALUES(81,0,1, 0,1,1,1,1,'/1/1/15/3/2/', N'IncreaseDecreaseInCashAndCashEquivalents',N'ChecksUnderCollectionExtension', N'Checks under collection',N'The amount of checks deposited but not credit to account yet')
INSERT INTO @AT VALUES(82,0,1, 0,1,1,1,0,'/1/1/15/3/3/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CreditCardSlipsExtension', N'Credit card slips',N'The amount of credit card slips collected but not submitted yet')
INSERT INTO @AT VALUES(83,0,1, 0,1,1,0,0,'/1/1/16/', NULL,N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(84,0,NULL, 1,0,0,0,0,'/1/2/', NULL,N'EquityAndLiabilitiesAbstract', N'Equity and liabilities [abstract]',N'')
INSERT INTO @AT VALUES(85,0,0, 1,0,0,0,0,'/1/2/1/', N'ChangesInEquity',N'EquityAbstract', N'Equity [abstract]',N'')
INSERT INTO @AT VALUES(86,0,0, 1,1,0,0,0,'/1/2/1/1/', N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
INSERT INTO @AT VALUES(87,0,0, 1,1,0,0,1,'/1/2/1/2/', N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(88,0,0, 0,1,0,0,0,'/1/2/1/3/', N'ChangesInEquity',N'SharePremium', N'Share premium',N'The amount received or receivable from the issuance of the entity''s shares in excess of nominal value.')
INSERT INTO @AT VALUES(89,0,0, 0,1,0,0,0,'/1/2/1/4/', N'ChangesInEquity',N'TreasuryShares', N'Treasury shares',N'An entity’s own equity instruments, held by the entity or other members of the consolidated group.')
INSERT INTO @AT VALUES(90,0,0, 0,1,0,0,0,'/1/2/1/5/', N'ChangesInEquity',N'OtherEquityInterest', N'Other equity interest',N'The amount of equity interest of an entity without share capital that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(91,0,0, 1,1,0,0,0,'/1/2/1/6/', N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(107,0,0, 1,1,0,0,0,'/1/2/1/6/16/', N'ChangesInEquity',N'StatutoryReserve', N'Statutory reserve',N'A component of equity representing reserves created based on legal requirements.')
INSERT INTO @AT VALUES(108,0,NULL, 1,0,0,0,0,'/1/2/2/', NULL,N'LiabilitiesAbstract', N'Liabilities [abstract]',N'')
INSERT INTO @AT VALUES(109,0,NULL, 1,1,0,1,0,'/1/2/2/1/', NULL,N'TradeAndOtherPayables', N'Trade and other payables',N'The amount of trade payables and other payables. [Refer: Trade payables; Other payables]')
INSERT INTO @AT VALUES(110,0,NULL, 1,1,0,1,1,'/1/2/2/1/1/', NULL,N'TradeAndOtherPayablesToTradeSuppliers', N'Trade payables',N'The amount of payment due to suppliers for goods and services used in the entity''s business.')
INSERT INTO @AT VALUES(111,0,NULL, 1,1,0,1,1,'/1/2/2/1/2/', NULL,N'DeferredIncome', N'Deferred income',N'The amount of liability representing income that has been received but is not yet earned. [Refer: Revenue]')
INSERT INTO @AT VALUES(112,0,NULL, 1,1,0,1,1,'/1/2/2/1/3/', NULL,N'Accruals', N'Accruals',N'The amount of liabilities to pay for goods or services that have been received or supplied but have not been paid, invoiced or formally agreed with the supplier, including amounts due to employees.')
INSERT INTO @AT VALUES(113,0,NULL, 1,1,0,0,1,'/1/2/2/1/4/', NULL,N'PayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Payables on social security and taxes other than income tax',N'The amount of payment due on social security and taxes other than income tax. Income taxes include all domestic and foreign taxes that are based on taxable profits. Income taxes also include taxes, such as withholding taxes, that are payable by a subsidiary, associate or joint arrangement on distributions to the reporting entity.')
INSERT INTO @AT VALUES(114,0,1, 0,1,0,0,1,'/1/2/2/1/4/1/', NULL,N'ValueAddedTaxPayables', N'Value added tax payables',N'The amount of payables related to a value added tax.')
INSERT INTO @AT VALUES(115,0,NULL, 0,1,0,0,0,'/1/2/2/1/4/2/', NULL,N'ExciseTaxPayables', N'Excise tax payables',N'The amount of payables related to excise tax.')
INSERT INTO @AT VALUES(116,0,1, 0,1,0,0,1,'/1/2/2/1/4/3/', NULL,N'SocialSecurityPayablesExtension', N'Social Security payables',N'The amount of payables related to social security')
INSERT INTO @AT VALUES(117,0,1, 1,1,0,0,1,'/1/2/2/1/4/4/', NULL,N'ZakatPayablesExtension', N'Zakat payables',N'The amount of payables related to a withtholding tax.')
INSERT INTO @AT VALUES(118,0,1, 1,1,0,0,1,'/1/2/2/1/4/5/', NULL,N'EmployeeIncomeTaxPayablesExtension', N'Employee Income tax payables',N'')
INSERT INTO @AT VALUES(119,0,1, 1,1,0,0,1,'/1/2/2/1/4/6/', NULL,N'EmployeeStampTaxPayablesExtension', N'Employee Stamp tax payables',N'')
INSERT INTO @AT VALUES(120,0,NULL, 1,1,0,1,0,'/1/2/2/1/5/', NULL,N'RetentionPayables', N'Retention payables',N'The amount of payment that is withheld by the entity, pending the fulfilment of a condition.')
INSERT INTO @AT VALUES(121,0,NULL, 1,1,0,1,0,'/1/2/2/1/6/', NULL,N'OtherPayables', N'Other payables',N'Amounts payable that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(122,0,NULL, 1,1,0,1,0,'/1/2/2/1/6/1/', NULL,N'PayablesToEmployeesExtension', N'Employees payables',N'Amounts payable that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(123,0,NULL, 1,0,0,0,0,'/1/2/2/2/', NULL,N'ProvisionsAbstract', N'Provisions [abstract]',N'')
INSERT INTO @AT VALUES(124,0,NULL, 1,1,0,1,0,'/1/2/2/2/1/', NULL,N'ProvisionsForEmployeeBenefits', N'Provisions for employee benefits',N'The amount of provisions for employee benefits. [Refer: Employee benefits expense; Provisions]')
INSERT INTO @AT VALUES(125,0,NULL, 1,1,0,0,0,'/1/2/2/2/2/', N'ChangesInOtherProvisions',N'OtherProvisions', N'Other provisions',N'The amount of provisions other than provisions for employee benefits. [Refer: Provisions]')
INSERT INTO @AT VALUES(126,0,NULL, 0,1,0,0,0,'/1/2/2/2/3/', N'ChangesInOtherProvisions',N'WarrantyProvision', N'Warranty provision',N'The amount of provision for estimated costs of making good under warranties for products sold. [Refer: Provisions]')
INSERT INTO @AT VALUES(127,0,NULL, 0,1,0,0,0,'/1/2/2/2/4/', N'ChangesInOtherProvisions',N'RestructuringProvision', N'Restructuring provision',N'The amount of provision for restructuring, such as the sale or termination of a line of business; closure of business locations in a country or region or relocation of activities from one country or region to another; changes in management structure; and fundamental reorganisations that have a material effect on the nature and focus of the entity''s operations. [Refer: Other provisions]')
INSERT INTO @AT VALUES(128,0,NULL, 0,1,0,0,0,'/1/2/2/2/5/', N'ChangesInOtherProvisions',N'LegalProceedingsProvision', N'Legal proceedings provision',N'The amount of provision for legal proceedings. [Refer: Other provisions]')
INSERT INTO @AT VALUES(129,0,NULL, 0,1,0,0,0,'/1/2/2/2/6/', N'ChangesInOtherProvisions',N'RefundsProvision', N'Refunds provision',N'The amount of provision for refunds to be made by the entity to its customers. [Refer: Other provisions]')
INSERT INTO @AT VALUES(130,0,NULL, 0,1,0,0,0,'/1/2/2/2/7/', N'ChangesInOtherProvisions',N'OnerousContractsProvision', N'Onerous contracts provision',N'The amount of provision for onerous contracts. An onerous contract is a contract in which the unavoidable costs of meeting the obligation under the contract exceed the economic benefits expected to be received under it. [Refer: Other provisions]')
INSERT INTO @AT VALUES(131,0,NULL, 0,1,0,0,0,'/1/2/2/2/8/', N'ChangesInOtherProvisions',N'ProvisionForDecommissioningRestorationAndRehabilitationCosts', N'Provision for decommissioning, restoration and rehabilitation costs',N'The amount of provision for costs related to decommissioning, restoration and rehabilitation. [Refer: Other provisions]')
INSERT INTO @AT VALUES(132,0,NULL, 1,1,0,1,0,'/1/2/2/5/', NULL,N'OtherFinancialLiabilities', N'Other financial liabilities',N'The amount of financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Financial liabilities]')
INSERT INTO @AT VALUES(133,0,NULL, 1,1,0,1,0,'/1/2/2/6/', NULL,N'OtherNonfinancialLiabilities', N'Other non-financial liabilities',N'The amount of non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(136,0,NULL, 1,1,0,0,0,'/1/2/2/7/', NULL,N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale', N'Liabilities included in disposal groups classified as held for sale',N'The amount of liabilities included in disposal groups classified as held for sale. [Refer: Liabilities; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(137,0,NULL, 1,0,0,0,0,'/2/', NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'')
INSERT INTO @AT VALUES(138,0,1, 1,1,1,1,0,'/2/1/', NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(139,0,1, 0,1,1,1,0,'/2/1/1/', NULL,N'RevenueFromSaleOfGoods', N'Revenue from sale of goods',N'The amount of revenue arising from the sale of goods. [Refer: Revenue]')
INSERT INTO @AT VALUES(140,0,1, 0,1,1,1,0,'/2/1/2/', NULL,N'RevenueFromRenderingOfServices', N'Revenue from rendering of services',N'The amount of revenue arising from the rendering of services. [Refer: Revenue]')
INSERT INTO @AT VALUES(141,0,1, 0,1,0,0,0,'/2/1/3/', NULL,N'RevenueFromInterest', N'Interest income',N'The amount of income arising from interest.')
INSERT INTO @AT VALUES(142,0,1, 0,1,0,1,0,'/2/1/4/', NULL,N'RevenueFromDividends', N'Dividend income',N'The amount of dividends recognised as income. Dividends are distributions of profits to holders of equity investments in proportion to their holdings of a particular class of capital.')
INSERT INTO @AT VALUES(143,0,1, 0,1,0,1,0,'/2/1/5/', NULL,N'OtherRevenue', N'Other revenue',N'The amount of revenue arising from sources that the entity does not separately disclose in the same statement or note. [Refer: Revenue]')
INSERT INTO @AT VALUES(144,0,1, 0,1,0,0,0,'/2/2/', NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(145,0,1, 1,0,0,1,0,'/2/3/', N'ExpenseByFunctionExtension',N'ExpenseByNatureAbstract', N'Expenses by nature [abstract]',N'The amount of acquisition and administration expense relating to insurance contracts. [Refer: Types of insurance contracts [member]]')
INSERT INTO @AT VALUES(146,0,1, 0,1,1,1,1,'/2/3/1/', N'ExpenseByFunctionExtension',N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used',N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(147,0,1, 0,1,1,1,1,'/2/3/2/', N'ExpenseByFunctionExtension',N'CostOfMerchandiseSold', N'Cost of merchandise sold',N'The amount of merchandise that was sold during the period and recognised as an expense.')
INSERT INTO @AT VALUES(148,1,1, 1,1,1,1,0,'/2/3/3/', N'ExpenseByFunctionExtension',N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
INSERT INTO @AT VALUES(149,1,1, 1,1,0,1,0,'/2/3/3/1/', N'ExpenseByFunctionExtension',N'InsuranceExpense', N'Insurance expense',N'The amount of expense arising from purchased insurance.')
INSERT INTO @AT VALUES(150,1,1, 1,1,0,1,0,'/2/3/3/2/', N'ExpenseByFunctionExtension',N'ProfessionalFeesExpense', N'Professional fees expense',N'The amount of fees paid or payable for professional services.')
INSERT INTO @AT VALUES(151,1,1, 1,1,0,1,0,'/2/3/3/3/', N'ExpenseByFunctionExtension',N'TransportationExpense', N'Transportation expense',N'The amount of expense arising from transportation services.')
INSERT INTO @AT VALUES(152,1,1, 1,1,0,1,0,'/2/3/3/4/', N'ExpenseByFunctionExtension',N'BankAndSimilarCharges', N'Bank and similar charges',N'The amount of bank and similar charges recognised by the entity as an expense.')
INSERT INTO @AT VALUES(153,1,1, 1,1,0,1,0,'/2/3/3/5/', N'ExpenseByFunctionExtension',N'TravelExpense', N'Travel expense',N'The amount of expense arising from travel.')
INSERT INTO @AT VALUES(154,1,1, 1,1,0,1,0,'/2/3/3/6/', N'ExpenseByFunctionExtension',N'CommunicationExpense', N'Communication expense',N'The amount of expense arising from communication.')
INSERT INTO @AT VALUES(155,1,1, 1,1,0,1,0,'/2/3/3/7/', N'ExpenseByFunctionExtension',N'UtilitiesExpense', N'Utilities expense',N'The amount of expense arising from purchased utilities.')
INSERT INTO @AT VALUES(156,1,1, 1,1,0,1,0,'/2/3/3/8/', N'ExpenseByFunctionExtension',N'AdvertisingExpense', N'Advertising expense',N'The amount of expense arising from advertising.')
INSERT INTO @AT VALUES(157,1,1, 1,1,0,1,0,'/2/3/4/', N'ExpenseByFunctionExtension',N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(158,1,1, 1,0,0,1,0,'/2/3/4/1/', N'ExpenseByFunctionExtension',N'ShorttermEmployeeBenefitsExpenseAbstract', N'Short-term employee benefits expense [abstract]',N'')
INSERT INTO @AT VALUES(159,1,1, 1,1,0,1,1,'/2/3/4/1/1/', N'ExpenseByFunctionExtension',N'WagesAndSalaries', N'Wages and salaries',N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(160,0,1, 1,1,0,1,1,'/2/3/4/1/2/', N'ExpenseByFunctionExtension',N'SocialSecurityContributions', N'Social security contributions',N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(161,0,1, 1,1,0,1,0,'/2/3/4/1/3/', N'ExpenseByFunctionExtension',N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits',N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(162,0,1, 1,1,0,1,0,'/2/3/4/2/', N'ExpenseByFunctionExtension',N'PostemploymentBenefitExpenseDefinedContributionPlans', N'Post-employment benefit expense, defined contribution plans',N'The amount of post-employment benefit expense relating to defined contribution plans. Defined contribution plans are post-employment benefit plans under which an entity pays fixed contributions into a separate entity (a fund) and will have no legal or constructive obligation to pay further contributions if the fund does not hold sufficient assets to pay all employee benefits relating to employee service in the current and prior periods.')
INSERT INTO @AT VALUES(163,0,1, 0,1,0,1,0,'/2/3/4/3/', N'ExpenseByFunctionExtension',N'PostemploymentBenefitExpenseDefinedBenefitPlans', N'Post-employment benefit expense, defined benefit plans',N'The amount of post-employment benefit expense relating to defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(164,0,1, 1,1,0,1,0,'/2/3/4/4/', N'ExpenseByFunctionExtension',N'TerminationBenefitsExpense', N'Termination benefits expense',N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(165,0,1, 1,1,0,1,0,'/2/3/4/5/', N'ExpenseByFunctionExtension',N'OtherLongtermBenefits', N'Other long-term employee benefits',N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(166,0,1, 1,1,0,1,0,'/2/3/4/6/', N'ExpenseByFunctionExtension',N'OtherEmployeeExpense', N'Other employee expense',N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(167,0,1, 1,0,0,1,1,'/2/3/5/', N'ExpenseByFunctionExtension',N'DepreciationAndAmortisationExpenseAbstract', N'Depreciation and amortisation expense [abstract]',N'')
INSERT INTO @AT VALUES(168,0,1, 1,1,1,1,1,'/2/3/5/1/', N'ExpenseByFunctionExtension',N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
INSERT INTO @AT VALUES(169,0,1, 1,1,1,1,1,'/2/3/5/2/', N'ExpenseByFunctionExtension',N'AmortisationExpense', N'Amortisation expense',N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
INSERT INTO @AT VALUES(170,0,1, 0,1,0,1,0,'/2/3/6/', N'ExpenseByFunctionExtension',N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Reversal of impairment loss (impairment loss) recognised in profit or loss',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(171,0,1, 1,1,0,1,0,'/2/3/7/', N'ExpenseByFunctionExtension',N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(172,0,1, 1,1,0,0,0,'/2/4/', NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(173,0,1, 1,1,0,0,0,'/2/4/1/', NULL,N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension', N'Gain (loss) on disposal of property, plant and equipment',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(174,0,1, 1,1,0,0,0,'/2/4/2/', NULL,N'GainLossOnForeignExchangeExtension', N'Gain (loss) on foreign exchange',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(175,0,1, 0,1,0,0,0,'/2/5/', NULL,N'GainsLossesOnNetMonetaryPosition', N'Gains (losses) on net monetary position',N'The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')
INSERT INTO @AT VALUES(176,0,1, 0,1,0,0,0,'/2/6/', NULL,N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost', N'Gain (loss) arising from derecognition of financial assets measured at amortised cost',N'The gain (loss) arising from the derecognition of financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(177,0,1, 0,1,0,0,0,'/2/7/', NULL,N'FinanceIncome', N'Finance income',N'The amount of income associated with interest and other financing activities of the entity.')
INSERT INTO @AT VALUES(178,0,1, 0,1,0,0,0,'/2/8/', NULL,N'FinanceCosts', N'Finance costs',N'The amount of costs associated with financing activities of the entity.')
INSERT INTO @AT VALUES(179,0,1, 0,1,0,0,0,'/2/9/', NULL,N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9', N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9',N'The amount of impairment loss, impairment gain or reversal of impairment loss that is recognised in profit or loss in accordance with paragraph 5.5.8 of IFRS 9 and that arises from applying the impairment requirements in Section 5.5 of IFRS 9.')
INSERT INTO @AT VALUES(180,0,1, 0,1,0,0,0,'/2/10/', NULL,N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod', N'Share of profit (loss) of associates and joint ventures accounted for using equity method',N'The entity''s share of the profit (loss) of associates and joint ventures accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method; Joint ventures [member]; Profit (loss)]')
INSERT INTO @AT VALUES(181,0,1, 0,1,0,0,0,'/2/11/', NULL,N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates', N'Other income (expense) from subsidiaries, jointly controlled entities and associates',N'The amount of income or expense from subsidiaries, jointly controlled entities and associates that the entity does not separately disclose in the same statement or note. [Refer: Associates [member]; Subsidiaries [member]]')
INSERT INTO @AT VALUES(182,0,1, 0,1,0,0,0,'/2/12/', NULL,N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue', N'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category',N'The gains (losses) arising from the difference between the previous amortised cost and the fair value of financial assets reclassified out of the amortised cost into the fair value through profit or loss measurement category. [Refer: At fair value [member]; Financial assets at amortised cost]')
INSERT INTO @AT VALUES(183,0,1, 0,1,0,0,0,'/2/13/', NULL,N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory', N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category',N'The cumulative gain (loss) previously recognised in other comprehensive income arising from the reclassification of financial assets out of the fair value through other comprehensive income into the fair value through profit or loss measurement category. [Refer: Financial assets measured at fair value through other comprehensive income; Financial assets at fair value through profit or loss; Other comprehensive income]')
INSERT INTO @AT VALUES(184,0,1, 0,1,0,0,0,'/2/14/', NULL,N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions', N'Hedging gains (losses) for hedge of group of items with offsetting risk positions',N'The hedging gains (losses) for hedge of group of items with offsetting risk positions.')
INSERT INTO @AT VALUES(185,0,1, 0,1,0,0,0,'/2/15/', NULL,N'IncomeTaxExpenseContinuingOperations', N'Tax income (expense)',N'The aggregate amount included in the determination of profit (loss) for the period in respect of current tax and deferred tax. [Refer: Current tax expense (income); Deferred tax expense (income)]')

	IF @DB = N'100' -- ACME, USD, en/ar/zh
	BEGIN
		UPDATE @AT Set IsActive = 1
	END
	ELSE IF @DB = N'101' -- Banan SD, USD, en
	BEGIN
		UPDATE @AT Set IsActive = 1 WHERE [Code] IN (
			N'ReceivedChecksExtension',
			N'ChecksUnderCollectionExtension',
			N'OtherInventories',
			N'RevenueFromRenderingOfServices',
			N'OtherRevenue',
			N'OtherIncome'
		);
	END
	ELSE IF @DB = N'102' -- Banan ET, ETB, en
	BEGIN 
		UPDATE @AT Set IsActive = 1 WHERE [Code] IN (
			N'ReceivedChecksExtension',
			N'ChecksUnderCollectionExtension',
			N'OtherInventories',
			N'RevenueFromRenderingOfServices'
		);
	END
	ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
	BEGIN
		UPDATE @AT Set IsActive = 1 WHERE [Code] IN (
			N'InventoriesTotal',
			N'Merchandise',
			N'WorkInProgress',
			N'CurrentInventoriesInTransit',
			N'OtherInventories',
			N'RevenueFromSaleOfGoods',
			N'RevenueFromRenderingOfServices',
			N'CostOfMerchandiseSold'
		);
	END
	ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	BEGIN
		UPDATE @AT Set IsActive = 1 WHERE [Code] IN (
			N'InventoriesTotal',
			N'Merchandise',
			N'WorkInProgress',
			N'CurrentInventoriesInTransit',
			N'OtherInventories',
			N'RevenueFromSaleOfGoods',
			N'RawMaterialsAndConsumablesUsed'
		);
	END
	ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
	BEGIN
		UPDATE @AT Set IsActive = 1 WHERE [Code] IN (
			N'InventoriesTotal',
			N'Merchandise',
			N'CurrentInventoriesInTransit',
			N'OtherInventories',
			N'RevenueFromSaleOfGoods',
			N'CostOfMerchandiseSold'
		);
	END

	DECLARE @AccountTypes dbo.AccountTypeList;
	INSERT INTO @AccountTypes ([Index], [Code], [Name], [ParentIndex], 
			[IsAssignable], [IsResourceClassification], [IsReal],[IsPersonal],
			[EntryTypeParentId], [Description])
	SELECT RC.[Index], RC.[Code], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex,
			[IsAssignable],  [IsResourceClassification], [IsReal],[IsPersonal],
			(SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = RC.EntryTypeParentCode), [Description]
	FROM @AT RC;
		
	EXEC [api].[AccountTypes__Save]
		@Entities = @AccountTypes,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	UPDATE dbo.[AccountTypes] SET IsActive = 0 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsActive = 0);

	UPDATE DB
	SET DB.[Node] = FE.[Node]
	FROM dbo.[AccountTypes] DB JOIN @AT FE ON DB.[Code] = FE.[Code]

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Account Types: Provisioning: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;																					
END
DECLARE @StatementOfFinancialPositionAbstract INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'StatementOfFinancialPositionAbstract');
DECLARE @PropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'PropertyPlantAndEquipment');
DECLARE @FixturesAndFittings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'FixturesAndFittings');
DECLARE @OfficeEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OfficeEquipment');
DECLARE @ComputerEquipmentMemberExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ComputerEquipmentMemberExtension');
DECLARE @ComputerAccessoriesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ComputerAccessoriesExtension');

DECLARE @IntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IntangibleAssetsOtherThanGoodwill');


DECLARE @TradeAndOtherReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherReceivables');
DECLARE @ValueAddedTaxReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ValueAddedTaxReceivables'); 
DECLARE @TradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeReceivables');
DECLARE @Prepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Prepayments');
DECLARE @AccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AccruedIncome');

DECLARE @InventoriesTotal INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'InventoriesTotal');
DECLARE @Merchandise INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Merchandise');
DECLARE @CurrentInventoriesInTransit INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentInventoriesInTransit');
DECLARE @OtherInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherInventories');

DECLARE @CashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashAndCashEquivalents');
DECLARE @CashOnHand INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashOnHand');
DECLARE @BalancesWithBanks INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'BalancesWithBanks');


DECLARE @IssuedCapital INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IssuedCapital'); 
DECLARE @RetainedEarnings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RetainedEarnings');

DECLARE @TradeAndOtherPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherPayables'); 
DECLARE @TradeAndOtherPayablesToTradeSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherPayablesToTradeSuppliers'); 
DECLARE @Accruals INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Accruals'); 
DECLARE @DeferredIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'DeferredIncome'); 

DECLARE @SocialSecurityPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'SocialSecurityPayablesExtension'); 
DECLARE @ValueAddedTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ValueAddedTaxPayables'); 
DECLARE @ZakatPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ZakatPayablesExtension'); 
DECLARE @EmployeeIncomeTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeIncomeTaxPayablesExtension'); 
DECLARE @EmployeeStampTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeStampTaxPayablesExtension'); 
DECLARE @PayablesToEmployeesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'PayablesToEmployeesExtension'); 

DECLARE @Revenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Revenue');
DECLARE @OtherIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherIncome');

DECLARE @RawMaterialsAndConsumablesUsed INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RawMaterialsAndConsumablesUsed');

DECLARE @ServicesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ServicesExpense');
DECLARE @ProfessionalFeesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ProfessionalFeesExpense');
DECLARE @TransportationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TransportationExpense');
DECLARE @BankAndSimilarCharges INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'BankAndSimilarCharges');
DECLARE @TravelExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TravelExpense');
DECLARE @CommunicationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CommunicationExpense');
DECLARE @UtilitiesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'UtilitiesExpense');
DECLARE @AdvertisingExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AdvertisingExpense');
DECLARE @EmployeeBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeBenefitsExpense');
DECLARE @WagesAndSalaries INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'WagesAndSalaries');
DECLARE @SocialSecurityContributions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'SocialSecurityContributions');
DECLARE @OtherShorttermEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherShorttermEmployeeBenefits');

DECLARE @TerminationBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TerminationBenefitsExpense');
DECLARE @DepreciationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'DepreciationExpense');
DECLARE @GainLossOnDisposalOfPropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension');

DECLARE @OtherExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherExpenseByNature');
/*
PostemploymentBenefitExpenseDefinedContributionPlans
PostemploymentBenefitExpenseDefinedBenefitPlans

OtherLongtermBenefits
OtherEmployeeExpense
*/