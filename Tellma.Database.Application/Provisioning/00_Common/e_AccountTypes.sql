IF NOT EXISTS(SELECT * FROM dbo.[AccountTypes])
BEGIN
	DECLARE @AT TABLE ([Index] INT,[IsResourceClassification] BIT, [IsCurrent] BIT, [IsActive] BIT, [Node] HIERARCHYID, IsAssignable BIT,
						[EntryTypeParentCode] NVARCHAR (255), [Code] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX))

/*
	INSERT INTO @AccountTypesTemp
	([Code],										[Name],											[Node],		[IsAssignable], [Index]) VALUES
	(N'Assets',										N'Assets',										N'/1/',			0,0),
		(N'PropertyPlantAndEquipment',				N'Property, plant and equipment',				N'/1/1/',		0,1),
			(N'LandAndBuildings',					N'Land and buildings',							N'/1/1/1/',		0,2),
				(N'Land',							N'Land',										N'/1/1/1/1/',	1,3),
				(N'Buildings',						N'Buildings',									N'/1/1/1/2/',	1,4),
			(N'Machinery',							N'Machinery',									N'/1/1/2/',		1,5),
			(N'Vehicles',							N'Vehicles',									N'/1/1/3/',		0,6),
				(N'Ships',							N'Ships',										N'/1/1/3/1/',	1,7),
				(N'Aircraft',						N'Aircraft',									N'/1/1/3/2/',	1,8),
				(N'MotorVehicles',					N'Motor vehicles',								N'/1/1/3/3/',	1,9),
			(N'FixturesAndFittings',				N'Fixtures and fittings',						N'/1/1/4/',		1,10),
			(N'OfficeEquipment',					N'Office equipment',							N'/1/1/5/',		1,11),
			(N'ComputerEquipment',					N'Computer equipment',							N'/1/1/6/',		1,12),
			(N'CommunicationAndNetworkEquipment',	N'Communication and network equipment',			N'/1/1/7/',		1,13),
			(N'NetworkInfrastructure',				N'Network infrastructure',						N'/1/1/8/',		1,14),
			(N'BearerPlants',						N'Bearer plants',								N'/1/1/9/',		1,15),
			(N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',	N'/1/1/10/',1,16),
			(N'MiningAssets',						N'Mining assets',								N'/1/1/11/',	1,17),
				(N'MiningProperty',					N'Mining property',								N'/1/1/11/1/',	1,18),
			(N'OilAndGasAssets',					N'Oil and gas assets',							N'/1/1/12/',	1,19),
			(N'PowerGeneratingAssets',				N'Power generating assets',						N'/1/1/13/',	1,20),
			(N'LeaseholdImprovements',				N'Leasehold improvements',						N'/1/1/14/',	1,21),
			(N'ConstructionInProgress',				N'Construction in progress',					N'/1/1/15/',	1,22),
			(N'OtherPropertyPlantAndEquipment',		N'Other property, plant and equipment',			N'/1/1/16/',	1,23),
		(N'InvestmentProperty',						N'Investment property',							N'/1/2/',		1,24),
		(N'Goodwill',								N'Goodwill',									N'/1/3/',		1,25),
		(N'IntangibleAssetsOtherThanGoodwill',		N'Intangible assets other than goodwill',		N'/1/4/',		1,26),
		(N'BiologicalAssets',						N'Biological assets',							N'/1/10/',		1,27),
		(N'InventoriesTotal',							N'Inventories',									N'/1/11/',		0,28),
			(N'CurrentRawMaterialsAndCurrentProductionSupplies',
													N'Raw materials and Production supplies',		N'/1/11/1/',	0,29),
				(N'RawMaterials',					N'Raw materials',								N'/1/11/1/1/',	1,30),
				(N'ProductionSupplies',				N'Production supplies',							N'/1/11/1/2/',	1,31),
			(N'Merchandise',						N'Merchandise',									N'/1/11/2/',	1,32),
			(N'CurrentFoodAndBeverage',				N'Food and beverage',							N'/1/11/3/',	1,33),
			(N'CurrentAgriculturalProduce',			N'Agricultural produce',						N'/1/11/4/',	1,34),
			(N'FinishedGoods',						N'Finished goods',								N'/1/11/5/',	1,35),
			(N'SpareParts',							N'Spare parts',									N'/1/11/6/',	1,36),
			(N'CurrentFuel',						N'Fuel',										N'/1/11/7/',	1,37),
	-- Financial resources
	(N'FinancialAssets',							N'Financial assets',							N'/2/',			1,38),
	(N'',						N'Cash and cash equivalents',					N'/3/',			0,39),
		(N'Cash',									N'Cash',										N'/3/1/',		1,40),
		(N'CashEquivalents',						N'Cash equivalents',							N'/3/2/',		1,41),
	-- Equitiy and Liabilities and 
	(N'Equity',										N'Equity',										N'/4/',			0,42),
	--(N'OtherLongtermProvisions',					N'Other non-current provisions',				N'/5/',			0,43),
	-- Consumables and services
	(N'ExpenseByNature',							N'Expenses, by nature',							N'/6/',			0,44),
		(N'RawMaterialsAndConsumablesUsed',			N'Raw materials and consumables used',			N'/6/1/',		1,45),
		(N'CostOfMerchandiseSold',					N'Cost of merchandise sold',					N'/6/2/',		1,46),
			(N'CostOfPurchasedEnergySold',			N'Cost of purchased energy sold',				N'/6/2/1/',		1,47),

		(N'ServicesExpense',						N'Services',									N'/6/3/',		1,48),
			(N'InsuranceExpense',					N'Insurance',									N'/6/3/1/',		1,49),
			(N'ProfessionalFeesExpense',			N'Professional services',						N'/6/3/2/',		1,50),
			(N'TransportationExpense',				N'Transportation',								N'/6/3/3/',		1,51),
			(N'BankAndSimilarCharges',				N'Bank services',								N'/6/3/4/',		1,52),
			(N'EnergyTransmissionCharges',			N'Energy transmission',							N'/6/3/5/',		1,53),
			(N'TravelExpense',						N'Travel',										N'/6/3/6/',		1,54),
			(N'CommunicationExpense',				N'Communication',								N'/6/3/7/',		1,55),
			(N'UtilitiesExpense',					N'Utilities',									N'/6/3/8/',		1,56),
			(N'AdvertisingExpense',					N'Advertising',									N'/6/3/9/',		1,57),

		(N'EmployeeBenefitsExpense',				N'Employee benefits expense',					N'/6/4/',		0,58),
			(N'ShorttermEmployeeBenefitsExpense',	N'Short-term employee benefits expense',		N'/6/4/1/',		0,59),
				(N'WagesAndSalaries',				N'Wages and salaries',							N'/6/4/1/1/',	1,60),
				(N'SocialSecurityContributions',	N'Social security contributions',				N'/6/4/1/2/',	1,61),
				(N'OtherShorttermEmployeeBenefits',	N'Other short-term employee benefits',			N'/6/4/1/3/',	1,62),
			(N'PostemploymentBenefitExpenseDefinedContributionPlans',
													N'Post-employment benefit expense, defined contribution plans',
																									N'/6/4/2/',		1,63),																				

		(N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
													N'Depreciation, amortisation and impairment loss (reversal of impairment loss) recognised in profit or loss',
																									N'/6/5/',		0,64),
			(N'DepreciationAndAmortisationExpense',N'Depreciation and amortisation expense',		N'/6/5/1/',		0,65),
				(N'DepreciationExpense',			N'Depreciation expense',						N'/6/5/1/1/',	1,66),
				(N'AmortisationExpense',			N'Amortisation expense',						N'/6/5/1/2/',	1,67),
				(N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
													N'Impairment loss (reversal of impairment loss) recognised in profit or loss',
																									N'/6/6/',		1,68),
		(N'TaxExpenseOtherThanIncomeTaxExpense',	N'Tax expense other than income tax expense',	N'/6/7/',		1,69),
			(N'PropertyTaxExpense',					N'Property tax expense',						N'/6/7/1/',		1,70),
		(N'OtherExpenseByNature',					N'Other expenses, by nature',					N'/6/8/',		1,71);
	--==================================================================================================================================
	-- THESE MAY NEED TO BE DELETED
	--(N'FinancialAssets',						N'Financial assets',							N'/1/4/',		1),
	--(N'FinancialAssetsAtFairValueThroughProfitOrLoss',N'Financial assets at fair value through profit or loss',
	--																							N'/1/4/1/',		1),
	--(N'FinancialAssetsAvailableforsale',		N'Financial assets available-for-sale',			N'/1/4/2/',		1),
	--(N'HeldtomaturityInvestments',				N'Held-to-maturity investments',				N'/1/4/3/',		1),
	--(N'LoansAndReceivables',					N'Loans and receivables',						N'/1/4/4/',		1),
	--(N'FinancialAssetsAtAmortisedCost',			N'Financial assets at amortised cost',			N'/1/4/5/',		1),
	--(N'FinancialAssetsAtFairValueThroughOtherComprehensiveIncome',
	--											N'Financial assets at fair value through other comprehensive income',
	--																							N'/1/4/6/',		1),

	--(N'OtherNonfinancialAssets',				N'Other non-financial assets',					N'/1/5/',		1),
	---- TODO: The following may actually fit under financial assets
	--(N'InsuranceContractsIssuedThatAreAssets',	N'Insurance contracts issued that are assets',	N'/1/6/',		1),
	--(N'ReinsuranceContractsHeldThatAreAssets',	N'Reinsurance contracts held that are assets',	N'/1/7/',		1),
	--(N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',
	--																							N'/1/8/',		1),
	--(N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',
	--																							N'/1/9/',		1),
	---

	--(N'Liabilities',							N'Liabilities',									N'/2/',			0),
	--(N'FinancialLiabilities',					N'Financial liabilities',						N'/2/2/',		1),
	--(N'FinancialLiabilitiesAtFairValueThroughProfitOrLoss',
	--											N'Financial liabilities at fair value through profit or loss',
	--																							N'/2/2/1/',		1),
	--(N'FinancialLiabilitiesAtAmortisedCost',	N'Financial liabilities at amortised cost',		N'/2/2/2/',		1),

	--(N'OtherNonfinancialLiabilities',			N'Other non-financial liabilities',				N'/2/3/',		1),
	-- TODO: The following may actually fit under financial liabilities
	--(N'InsuranceContractsIssuedThatAreLiabilities',	N'Insurance contracts issued that are liabilities',	N'/2/4/',1),
	--(N'ReinsuranceContractsHeldThatAreLiabilities',	N'Reinsurance contracts held that are liabilities',	N'/2/5/',1),
*/
INSERT INTO @AT VALUES(0,0, NULL, 1, '/1/', 0,NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
INSERT INTO @AT VALUES(1,0, NULL, 1, '/1/1/', 0,NULL,N'AssetsAbstract', N'Assets [abstract]',N'')
INSERT INTO @AT VALUES(2,1, '0', 1, '/1/1/1/', 1,N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
INSERT INTO @AT VALUES(3,1, '0', 1, '/1/1/1/1/', 0,N'ChangesInPropertyPlantAndEquipment',N'LandAndBuildingsAbstract', N'Land and buildings [abstract]',N'')
INSERT INTO @AT VALUES(4,1, '0', 1, '/1/1/1/1/1/', 1,N'ChangesInPropertyPlantAndEquipment',N'Land', N'Land',N'The amount of property, plant and equipment representing land held by the entity for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(5,1, '0', 1, '/1/1/1/1/2/', 1,N'ChangesInPropertyPlantAndEquipment',N'Buildings', N'Buildings',N'The amount of property, plant and equipment representing depreciable buildings and similar structures for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(6,1, '0', 1, '/1/1/1/2/', 1,N'ChangesInPropertyPlantAndEquipment',N'Machinery', N'Machinery',N'The amount of property, plant and equipment representing long-lived, depreciable machinery used in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(7,1, '0', 0, '/1/1/1/3/', 0,N'ChangesInPropertyPlantAndEquipment',N'VehiclesAbstract', N'Vehicles [abstract]',N'')
INSERT INTO @AT VALUES(8,1, '0', 0, '/1/1/1/3/1/', 1,N'ChangesInPropertyPlantAndEquipment',N'Ships', N'Ships',N'The amount of property, plant and equipment representing seafaring or other maritime vessels used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(9,1, '0', 0, '/1/1/1/3/2/', 1,N'ChangesInPropertyPlantAndEquipment',N'Aircraft', N'Aircraft',N'The amount of property, plant and equipment representing aircraft used in the entity''s operations.')
INSERT INTO @AT VALUES(10,1, '0', 1, '/1/1/1/3/3/', 1,N'ChangesInPropertyPlantAndEquipment',N'MotorVehicles', N'Motor vehicles',N'The amount of property, plant and equipment representing self-propelled ground vehicles used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(11,1, '0', 1, '/1/1/1/4/', 1,N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(12,1, '0', 1, '/1/1/1/5/', 1,N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(13,1, '0', 0, '/1/1/1/6/', 1,N'ChangesInPropertyPlantAndEquipment',N'BearerPlants', N'Bearer plants',N'The amount of property, plant and equipment representing bearer plants. Bearer plant is a living plant that (a) is used in the production or supply of agricultural produce; (b) is expected to bear produce for more than one period; and (c) has a remote likelihood of being sold as agricultural produce, except for incidental scrap sales. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(14,1, '0', 0, '/1/1/1/7/', 1,N'ChangesInPropertyPlantAndEquipment',N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',N'The amount of exploration and evaluation assets recognised as tangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(15,1, '0', 0, '/1/1/1/8/', 1,N'ChangesInPropertyPlantAndEquipment',N'MiningAssets', N'Mining assets',N'The amount of assets related to mining activities of the entity.')
INSERT INTO @AT VALUES(16,1, '0', 0, '/1/1/1/9/', 1,N'ChangesInPropertyPlantAndEquipment',N'OilAndGasAssets', N'Oil and gas assets',N'The amount of assets related to the exploration, evaluation, development or production of oil and gas.')
INSERT INTO @AT VALUES(17,1, '0', 0, '/1/1/1/10/', 1,N'ChangesInPropertyPlantAndEquipment',N'ConstructionInProgress', N'Construction in progress',N'The amount of expenditure capitalised during the construction of non-current assets that are not yet available for use. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(18,1, '0', 1, '/1/1/1/11/', 1,N'ChangesInPropertyPlantAndEquipment',N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', N'Owner-occupied property measured using investment property fair value model',N'The amount of property, plant and equipment representing owner-occupied property measured using the investment property fair value model applying paragraph 29A of IAS 16. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(19,1, '0', 1, '/1/1/1/12/', 1,N'ChangesInPropertyPlantAndEquipment',N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment',N'The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(20,1, '0', 1, '/1/1/2/', 1,N'ChangesInInvestmentProperty',N'InvestmentProperty', N'Investment property',N'The amount of property (land or a building - or part of a building - or both) held (by the owner or by the lessee as a right-of-use asset) to earn rentals or for capital appreciation or both, rather than for: (a) use in the production or supply of goods or services or for administrative purposes; or (b) sale in the ordinary course of business.')
INSERT INTO @AT VALUES(23,0, '0', 0, '/1/1/3/', 1,N'ChangesInGoodwill',N'Goodwill', N'Goodwill',N'The amount of assets representing the future economic benefits arising from other assets acquired in a business combination that are not individually identified and separately recognised. [Refer: Business combinations [member]]')
INSERT INTO @AT VALUES(24,1, '0', 1, '/1/1/4/', 1,N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(34,0, NULL, 1, '/1/1/5/', 1,NULL,N'OtherFinancialAssets', N'Other financial assets',N'The amount of financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(35,0, NULL, 1, '/1/1/6/', 1,NULL,N'OtherNonfinancialAssets', N'Other non-financial assets',N'The amount of non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(36,0, '0', 1, '/1/1/7/', 1,NULL,N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',N'The amount of investments accounted for using the equity method. The equity method is a method of accounting whereby the investment is initially recognised at cost and adjusted thereafter for the post-acquisition change in the investor''s share of net assets of the investee. The investor''s profit or loss includes its share of the profit or loss of the investee. The investor''s other comprehensive income includes its share of the other comprehensive income of the investee. [Refer: At cost [member]]')
INSERT INTO @AT VALUES(39,0, '0', 1, '/1/1/8/', 1,NULL,N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',N'The amount of investments in subsidiaries, joint ventures and associates in an entity''s separate financial statements. [Refer: Associates [member]; Joint ventures [member]; Subsidiaries [member]; Investments in subsidiaries]')
INSERT INTO @AT VALUES(43,1, NULL, 0, '/1/1/9/', 1,N'ChangesInBiologicalAssets',N'BiologicalAssets', N'Biological assets',N'The amount of living animals or plants recognised as assets.')
INSERT INTO @AT VALUES(44,0, NULL, 0, '/1/1/10/', 1,NULL,N'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners', N'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners',N'The amount of non-current assets or disposal groups classified as held for sale or as held for distribution to owners. [Refer: Non-current assets or disposal groups classified as held for distribution to owners; Non-current assets or disposal groups classified as held for sale]')
INSERT INTO @AT VALUES(45,1, NULL, 1, '/1/1/11/', 1,N'ChangesInInventories',N'InventoriesTotal', N'Inventories',N'The amount of assets: (a) held for sale in the ordinary course of business; (b) in the process of production for such sale; or (c) in the form of materials or supplies to be consumed in the production process or in the rendering of services. Inventories encompass goods purchased and held for resale including, for example, merchandise purchased by a retailer and held for resale, or land and other property held for resale. Inventories also encompass finished goods produced, or work in progress being produced, by the entity and include materials and supplies awaiting use in the production process. [Refer: Current finished goods; Current merchandise; Current work in progress; Land]')
INSERT INTO @AT VALUES(46,1, '1', 1, '/1/1/11/1/', 0,N'ChangesInInventories',N'CurrentRawMaterialsAndCurrentProductionSuppliesAbstract', N'Current raw materials and current production supplies [abstract]',N'')
INSERT INTO @AT VALUES(47,1, '1', 1, '/1/1/11/1/1/', 1,N'ChangesInInventories',N'RawMaterials', N'Current raw materials',N'A classification of current inventory representing the amount of assets to be consumed in the production process or in the rendering of services. [Refer: Inventories]')
INSERT INTO @AT VALUES(48,1, '1', 1, '/1/1/11/1/2/', 1,N'ChangesInInventories',N'ProductionSupplies', N'Current production supplies',N'A classification of current inventory representing the amount of supplies to be used for the production process. [Refer: Inventories]')
INSERT INTO @AT VALUES(49,1, '0', 1, '/1/1/11/2/', 1,N'ChangesInInventories',N'Merchandise', N'Current merchandise',N'A classification of current inventory representing the amount of goods acquired for resale. [Refer: Inventories]')
INSERT INTO @AT VALUES(50,1, '0', 1, '/1/1/11/3/', 1,N'ChangesInInventories',N'CurrentFoodAndBeverage', N'Current food and beverage',N'A classification of current inventory representing the amount of food and beverage. [Refer: Inventories]')
INSERT INTO @AT VALUES(51,1, '0', 1, '/1/1/11/4/', 1,N'ChangesInInventories',N'CurrentAgriculturalProduce', N'Current agricultural produce',N'A classification of current inventory representing the amount of harvested produce of the entity''s biological assets. [Refer: Biological assets; Inventories]')
INSERT INTO @AT VALUES(52,1, '1', 1, '/1/1/11/5/', 1,N'ChangesInInventories',N'WorkInProgress', N'Current work in progress',N'A classification of current inventory representing the amount of assets currently in production, which require further processes to be converted into finished goods or services. [Refer: Current finished goods; Inventories]')
INSERT INTO @AT VALUES(53,1, '1', 1, '/1/1/11/6/', 1,N'ChangesInInventories',N'FinishedGoods', N'Current finished goods',N'A classification of current inventory representing the amount of goods that have completed the production process and are held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(54,1, '1', 1, '/1/1/11/7/', 1,N'ChangesInInventories',N'CurrentPackagingAndStorageMaterials', N'Current packaging and storage materials',N'A classification of current inventory representing the amount of packaging and storage materials. [Refer: Inventories]')
INSERT INTO @AT VALUES(55,1, '1', 1, '/1/1/11/8/', 1,N'ChangesInInventories',N'SpareParts', N'Current spare parts',N'A classification of current inventory representing the amount of interchangeable parts that are kept in an inventory and are used for the repair or replacement of failed parts. [Refer: Inventories]')
INSERT INTO @AT VALUES(56,1, '1', 1, '/1/1/11/9/', 1,N'ChangesInInventories',N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property intended for sale in ordinary course of business',N'The amount of property intended for sale in the ordinary course of business of the entity. Property is land or a building - or part of a building - or both.')
INSERT INTO @AT VALUES(57,0, '1', 1, '/1/1/11/10/', 1,N'ChangesInInventories',N'CurrentInventoriesInTransit', N'Current inventories in transit',N'A classification of current inventory representing the amount of inventories in transit. [Refer: Inventories]')
INSERT INTO @AT VALUES(58,1, '1', 1, '/1/1/11/11/', 1,N'ChangesInInventories',N'OtherInventories', N'Other current inventories',N'The amount of inventory that the entity does not separately disclose in the same statement or note. [Refer: Inventories]')
INSERT INTO @AT VALUES(59,0, NULL, 1, '/1/1/12/', 1,NULL,N'CurrentTaxAssets', N'Current tax assets',N'The excess of amount paid for current tax in respect of current and prior periods over the amount due for those periods. Current tax is the amount of income taxes payable (recoverable) in respect of the taxable profit (tax loss) for a period.')
INSERT INTO @AT VALUES(60,0, NULL, 0, '/1/1/13/', 1,NULL,N'DeferredTaxAssets', N'Deferred tax assets',N'The amounts of income taxes recoverable in future periods in respect of: (a) deductible temporary differences; (b) the carryforward of unused tax losses; and (c) the carryforward of unused tax credits. [Refer: Temporary differences [member]; Unused tax credits [member]; Unused tax losses [member]]')
INSERT INTO @AT VALUES(61,0, NULL, 1, '/1/1/14/', 1,NULL,N'TradeAndOtherReceivables', N'Trade and other receivables',N'The amount of trade receivables and other receivables. [Refer: Trade receivables; Other receivables]')
INSERT INTO @AT VALUES(68,0, '1', 1, '/1/1/15/', 1,N'IncreaseDecreaseInCashAndCashEquivalents',N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(77,0, '1', 0, '/1/1/16/', 1,NULL,N'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(78,0, NULL, 1, '/1/2/', 0,NULL,N'EquityAndLiabilitiesAbstract', N'Equity and liabilities [abstract]',N'')
INSERT INTO @AT VALUES(79,0, '0', 1, '/1/2/1/', 0,N'ChangesInEquity',N'EquityAbstract', N'Equity [abstract]',N'')
INSERT INTO @AT VALUES(80,0, '0', 1, '/1/2/1/1/', 1,N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
INSERT INTO @AT VALUES(81,0, '0', 1, '/1/2/1/2/', 1,N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(85,0, '0', 1, '/1/2/1/3/', 1,N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(102,0, NULL, 1, '/1/2/2/', 0,NULL,N'LiabilitiesAbstract', N'Liabilities [abstract]',N'')
INSERT INTO @AT VALUES(103,0, NULL, 1, '/1/2/2/1/', 1,NULL,N'TradeAndOtherPayables', N'Trade and other payables',N'The amount of trade payables and other payables. [Refer: Trade payables; Other payables]')
INSERT INTO @AT VALUES(104,0, NULL, 1, '/1/2/2/1/1/', 1,NULL,N'TradeAndOtherPayablesToTradeSuppliers', N'Trade payables',N'The amount of payment due to suppliers for goods and services used in the entity''s business.')
INSERT INTO @AT VALUES(105,0, NULL, 1, '/1/2/2/1/2/', 1,NULL,N'DeferredIncome', N'Deferred income',N'The amount of liability representing income that has been received but is not yet earned. [Refer: Revenue]')
INSERT INTO @AT VALUES(106,0, NULL, 1, '/1/2/2/1/3/', 1,NULL,N'Accruals', N'Accruals',N'The amount of liabilities to pay for goods or services that have been received or supplied but have not been paid, invoiced or formally agreed with the supplier, including amounts due to employees.')
INSERT INTO @AT VALUES(107,0, NULL, 1, '/1/2/2/1/4/', 1,NULL,N'PayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Payables on social security and taxes other than income tax',N'The amount of payment due on social security and taxes other than income tax. Income taxes include all domestic and foreign taxes that are based on taxable profits. Income taxes also include taxes, such as withholding taxes, that are payable by a subsidiary, associate or joint arrangement on distributions to the reporting entity.')
INSERT INTO @AT VALUES(108,0, NULL, 0, '/1/2/2/1/4/1/', 1,NULL,N'ValueAddedTaxPayables', N'Value added tax payables',N'The amount of payables related to a value added tax.')
INSERT INTO @AT VALUES(109,0, NULL, 0, '/1/2/2/1/4/2/', 1,NULL,N'ExciseTaxPayables', N'Excise tax payables',N'The amount of payables related to excise tax.')
INSERT INTO @AT VALUES(110,0, NULL, 1, '/1/2/2/1/5/', 1,NULL,N'RetentionPayables', N'Retention payables',N'The amount of payment that is withheld by the entity, pending the fulfilment of a condition.')
INSERT INTO @AT VALUES(111,0, NULL, 1, '/1/2/2/1/6/', 1,NULL,N'OtherPayables', N'Other payables',N'Amounts payable that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(112,0, NULL, 1, '/1/2/2/2/', 0,NULL,N'ProvisionsAbstract', N'Provisions [abstract]',N'')
INSERT INTO @AT VALUES(113,0, NULL, 1, '/1/2/2/2/1/', 1,NULL,N'ProvisionsForEmployeeBenefits', N'Provisions for employee benefits',N'The amount of provisions for employee benefits. [Refer: Employee benefits expense; Provisions]')
INSERT INTO @AT VALUES(114,0, NULL, 1, '/1/2/2/2/2/', 1,N'ChangesInOtherProvisions',N'OtherProvisions', N'Other provisions',N'The amount of provisions other than provisions for employee benefits. [Refer: Provisions]')
INSERT INTO @AT VALUES(121,0, NULL, 1, '/1/2/2/3/', 1,NULL,N'OtherFinancialLiabilities', N'Other financial liabilities',N'The amount of financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Financial liabilities]')
INSERT INTO @AT VALUES(122,0, NULL, 1, '/1/2/2/4/', 1,NULL,N'OtherNonfinancialLiabilities', N'Other non-financial liabilities',N'The amount of non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(125,0, NULL, 1, '/1/2/2/5/', 1,NULL,N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale', N'Liabilities included in disposal groups classified as held for sale',N'The amount of liabilities included in disposal groups classified as held for sale. [Refer: Liabilities; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(126,0, NULL, 1, '/2/', 0,NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'')
INSERT INTO @AT VALUES(127,0, '1', 1, '/2/1/', 1,NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(133,0, '1', 1, '/2/2/', 1,NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(134,0, '1', 1, '/2/3/', 0,N'ExpenseByFunctionExtension',N'ExpenseByNatureAbstract', N'Expenses by nature [abstract]',N'The amount of acquisition and administration expense relating to insurance contracts. [Refer: Types of insurance contracts [member]]')
INSERT INTO @AT VALUES(135,0, '1', 1, '/2/3/1/', 1,N'ExpenseByFunctionExtension',N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used',N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(137,1, '1', 1, '/2/3/2/', 1,N'ExpenseByFunctionExtension',N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
--INSERT INTO @AT VALUES(138,1, '1', 1, '/2/3/3/', 1,N'ExpenseByFunctionExtension',N'InsuranceExpense', N'Insurance expense',N'The amount of expense arising from purchased insurance.')
--INSERT INTO @AT VALUES(139,1, '1', 1, '/2/3/4/', 1,N'ExpenseByFunctionExtension',N'ProfessionalFeesExpense', N'Professional fees expense',N'The amount of fees paid or payable for professional services.')
--INSERT INTO @AT VALUES(140,1, '1', 1, '/2/3/5/', 1,N'ExpenseByFunctionExtension',N'TransportationExpense', N'Transportation expense',N'The amount of expense arising from transportation services.')
INSERT INTO @AT VALUES(146,1, '1', 1, '/2/3/3/', 1,N'ExpenseByFunctionExtension',N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(147,1, '1', 1, '/2/3/3/1/', 0,N'ExpenseByFunctionExtension',N'ShorttermEmployeeBenefitsExpenseAbstract', N'Short-term employee benefits expense [abstract]',N'')
INSERT INTO @AT VALUES(148,1, '1', 1, '/2/3/3/1/1/', 1,N'ExpenseByFunctionExtension',N'WagesAndSalaries', N'Wages and salaries',N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(149,0, '1', 1, '/2/3/3/1/2/', 1,N'ExpenseByFunctionExtension',N'SocialSecurityContributions', N'Social security contributions',N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(150,0, '1', 1, '/2/3/3/1/3/', 1,N'ExpenseByFunctionExtension',N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits',N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(151,0, '1', 1, '/2/3/3/2/', 1,N'ExpenseByFunctionExtension',N'PostemploymentBenefitExpenseDefinedContributionPlans', N'Post-employment benefit expense, defined contribution plans',N'The amount of post-employment benefit expense relating to defined contribution plans. Defined contribution plans are post-employment benefit plans under which an entity pays fixed contributions into a separate entity (a fund) and will have no legal or constructive obligation to pay further contributions if the fund does not hold sufficient assets to pay all employee benefits relating to employee service in the current and prior periods.')
INSERT INTO @AT VALUES(152,0, '1', 0, '/2/3/3/3/', 1,N'ExpenseByFunctionExtension',N'PostemploymentBenefitExpenseDefinedBenefitPlans', N'Post-employment benefit expense, defined benefit plans',N'The amount of post-employment benefit expense relating to defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(153,0, '1', 1, '/2/3/3/4/', 1,N'ExpenseByFunctionExtension',N'TerminationBenefitsExpense', N'Termination benefits expense',N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(154,0, '1', 1, '/2/3/3/5/', 1,N'ExpenseByFunctionExtension',N'OtherLongtermBenefits', N'Other long-term employee benefits',N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(155,0, '1', 1, '/2/3/3/6/', 1,N'ExpenseByFunctionExtension',N'OtherEmployeeExpense', N'Other employee expense',N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(156,0, '1', 1, '/2/3/4/', 0,N'ExpenseByFunctionExtension',N'DepreciationAndAmortisationExpenseAbstract', N'Depreciation and amortisation expense [abstract]',N'')
INSERT INTO @AT VALUES(157,0, '1', 1, '/2/3/4/1/', 1,N'ExpenseByFunctionExtension',N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
INSERT INTO @AT VALUES(158,0, '1', 1, '/2/3/4/2/', 1,N'ExpenseByFunctionExtension',N'AmortisationExpense', N'Amortisation expense',N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
INSERT INTO @AT VALUES(159,0, '1', 1, '/2/3/5/', 1,N'ExpenseByFunctionExtension',N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Reversal of impairment loss (impairment loss) recognised in profit or loss',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(160,0, '1', 1, '/2/3/6/', 1,N'ExpenseByFunctionExtension',N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(161,0, '1', 1, '/2/4/', 1,NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(162,0, '1', 0, '/2/5/', 1,NULL,N'GainsLossesOnNetMonetaryPosition', N'Gains (losses) on net monetary position',N'The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')
INSERT INTO @AT VALUES(163,0, '1', 0, '/2/6/', 1,NULL,N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost', N'Gain (loss) arising from derecognition of financial assets measured at amortised cost',N'The gain (loss) arising from the derecognition of financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(164,0, '1', 0, '/2/7/', 1,NULL,N'FinanceIncome', N'Finance income',N'The amount of income associated with interest and other financing activities of the entity.')
INSERT INTO @AT VALUES(165,0, '1', 1, '/2/8/', 1,NULL,N'FinanceCosts', N'Finance costs',N'The amount of costs associated with financing activities of the entity.')
INSERT INTO @AT VALUES(166,0, '1', 1, '/2/9/', 1,NULL,N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9', N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9',N'The amount of impairment loss, impairment gain or reversal of impairment loss that is recognised in profit or loss in accordance with paragraph 5.5.8 of IFRS 9 and that arises from applying the impairment requirements in Section 5.5 of IFRS 9.')
INSERT INTO @AT VALUES(167,0, '1', 0, '/2/10/', 1,NULL,N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod', N'Share of profit (loss) of associates and joint ventures accounted for using equity method',N'The entity''s share of the profit (loss) of associates and joint ventures accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method; Joint ventures [member]; Profit (loss)]')
INSERT INTO @AT VALUES(168,0, '1', 0, '/2/11/', 1,NULL,N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates', N'Other income (expense) from subsidiaries, jointly controlled entities and associates',N'The amount of income or expense from subsidiaries, jointly controlled entities and associates that the entity does not separately disclose in the same statement or note. [Refer: Associates [member]; Subsidiaries [member]]')
INSERT INTO @AT VALUES(169,0, '1', 0, '/2/12/', 1,NULL,N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue', N'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category',N'The gains (losses) arising from the difference between the previous amortised cost and the fair value of financial assets reclassified out of the amortised cost into the fair value through profit or loss measurement category. [Refer: At fair value [member]; Financial assets at amortised cost]')
INSERT INTO @AT VALUES(170,0, '1', 0, '/2/13/', 1,NULL,N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory', N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category',N'The cumulative gain (loss) previously recognised in other comprehensive income arising from the reclassification of financial assets out of the fair value through other comprehensive income into the fair value through profit or loss measurement category. [Refer: Financial assets measured at fair value through other comprehensive income; Financial assets at fair value through profit or loss; Other comprehensive income]')
INSERT INTO @AT VALUES(171,0, '1', 0, '/2/14/', 1,NULL,N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions', N'Hedging gains (losses) for hedge of group of items with offsetting risk positions',N'The hedging gains (losses) for hedge of group of items with offsetting risk positions.')
INSERT INTO @AT VALUES(172,0, '1', 0, '/2/15/', 1,NULL,N'IncomeTaxExpenseContinuingOperations', N'Tax income (expense)',N'The aggregate amount included in the determination of profit (loss) for the period in respect of current tax and deferred tax. [Refer: Current tax expense (income); Deferred tax expense (income)]')


	DECLARE @AccountTypes dbo.AccountTypeList

	--INSERT INTO @AccountTypes ([Code], [Name], [ParentIndex], [IsAssignable], [Index])
	--SELECT [Code], [Name], (SELECT [Index] FROM @AccountTypesTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [IsAssignable], [Index]
	--FROM @AccountTypesTemp RC

	INSERT INTO @AccountTypes ([Index], [Code], [Name], [ParentIndex], [IsAssignable], [IsCurrent],
								[IsResourceClassification], [EntryTypeParentId], [Description])
	SELECT RC.[Index], RC.[Code], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex,
			[IsAssignable],  [IsCurrent], [IsResourceClassification],
			(SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = RC.EntryTypeParentCode), [Description]
	FROM @AT RC

	--select * from 	@AccountTypes;			
	EXEC [api].[AccountTypes__Save]
		@Entities = @AccountTypes,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	UPDATE dbo.[AccountTypes] SET IsSystem = 1;
	UPDATE dbo.[AccountTypes] SET IsActive = 0 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsActive = 0);

	UPDATE dbo.[AccountTypes] SET IsReal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'PropertyPlantAndEquipment')) = 1
	UPDATE dbo.[AccountTypes] SET IsReal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'InvestmentProperty')) = 1
	UPDATE dbo.[AccountTypes] SET IsReal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'BiologicalAssets')) = 1
	UPDATE dbo.[AccountTypes] SET IsReal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'InventoriesTotal')) = 1
	UPDATE dbo.[AccountTypes] SET IsReal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'ExpenseByNatureAbstract')) = 1

	UPDATE dbo.[AccountTypes] SET IsPersonal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherReceivables')) = 1
	UPDATE dbo.[AccountTypes] SET IsPersonal = 1
	WHERE [Node].IsDescendantOf((SELECT [Node] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherPayables')) = 1

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Account Types: Provisioning: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;																					
END
DECLARE @CashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashAndCashEquivalents');
DECLARE @InventoriesTotal INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'InventoriesTotal');
DECLARE @PropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'PropertyPlantAndEquipment');
DECLARE @TradeAndOtherReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherReceivables');
DECLARE @TradeAndOtherPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherPayables');
DECLARE @IssuedCapital INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IssuedCapital');
DECLARE @EmployeeBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeBenefitsExpense');
DECLARE @OtherExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherExpenseByNature');