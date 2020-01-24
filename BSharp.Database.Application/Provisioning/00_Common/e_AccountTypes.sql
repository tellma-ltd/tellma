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
INSERT INTO @AT VALUES(0,0, NULL, 1, '/1/', 0,NULL,'StatementOfFinancialPositionAbstract', 'Statement of financial position [abstract]','')
INSERT INTO @AT VALUES(1,0, NULL, 1, '/1/1/', 0,NULL,'AssetsAbstract', 'Assets [abstract]','')
INSERT INTO @AT VALUES(2,1, '0', 1, '/1/1/1/', 1,'ChangesInPropertyPlantAndEquipment','PropertyPlantAndEquipment', 'Property, plant and equipment','The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')

INSERT INTO @AT VALUES(3,1, '0', 1, '/1/1/1/1/', 0,'ChangesInPropertyPlantAndEquipment','LandAndBuildingsAbstract', 'Land and buildings [abstract]','')
INSERT INTO @AT VALUES(4,1, '0', 1, '/1/1/1/1/1/', 1,'ChangesInPropertyPlantAndEquipment','Land', 'Land','The amount of property, plant and equipment representing land held by the entity for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(5,1, '0', 1, '/1/1/1/1/2/', 1,'ChangesInPropertyPlantAndEquipment','Buildings', 'Buildings','The amount of property, plant and equipment representing depreciable buildings and similar structures for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(6,1, '0', 1, '/1/1/1/2/', 1,'ChangesInPropertyPlantAndEquipment','Machinery', 'Machinery','The amount of property, plant and equipment representing long-lived, depreciable machinery used in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(7,1, '0', 0, '/1/1/1/3/', 0,'ChangesInPropertyPlantAndEquipment','VehiclesAbstract', 'Vehicles [abstract]','')
INSERT INTO @AT VALUES(8,1, '0', 0, '/1/1/1/3/1/', 1,'ChangesInPropertyPlantAndEquipment','Ships', 'Ships','The amount of property, plant and equipment representing seafaring or other maritime vessels used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(9,1, '0', 0, '/1/1/1/3/2/', 1,'ChangesInPropertyPlantAndEquipment','Aircraft', 'Aircraft','The amount of property, plant and equipment representing aircraft used in the entity''s operations.')
INSERT INTO @AT VALUES(10,1, '0', 1, '/1/1/1/3/3/', 1,'ChangesInPropertyPlantAndEquipment','MotorVehicles', 'Motor vehicles','The amount of property, plant and equipment representing self-propelled ground vehicles used in the entity''s operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(11,1, '0', 1, '/1/1/1/4/', 1,'ChangesInPropertyPlantAndEquipment','FixturesAndFittings', 'Fixtures and fittings','The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(12,1, '0', 1, '/1/1/1/5/', 1,'ChangesInPropertyPlantAndEquipment','OfficeEquipment', 'Office equipment','The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(13,1, '0', 0, '/1/1/1/6/', 1,'ChangesInPropertyPlantAndEquipment','BearerPlants', 'Bearer plants','The amount of property, plant and equipment representing bearer plants. Bearer plant is a living plant that (a) is used in the production or supply of agricultural produce; (b) is expected to bear produce for more than one period; and (c) has a remote likelihood of being sold as agricultural produce, except for incidental scrap sales. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(14,1, '0', 0, '/1/1/1/7/', 1,'ChangesInPropertyPlantAndEquipment','TangibleExplorationAndEvaluationAssets', 'Tangible exploration and evaluation assets','The amount of exploration and evaluation assets recognised as tangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(15,1, '0', 0, '/1/1/1/8/', 1,'ChangesInPropertyPlantAndEquipment','MiningAssets', 'Mining assets','The amount of assets related to mining activities of the entity.')
INSERT INTO @AT VALUES(16,1, '0', 0, '/1/1/1/9/', 1,'ChangesInPropertyPlantAndEquipment','OilAndGasAssets', 'Oil and gas assets','The amount of assets related to the exploration, evaluation, development or production of oil and gas.')
INSERT INTO @AT VALUES(17,1, '0', 0, '/1/1/1/10/', 1,'ChangesInPropertyPlantAndEquipment','ConstructionInProgress', 'Construction in progress','The amount of expenditure capitalised during the construction of non-current assets that are not yet available for use. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(18,1, '0', 1, '/1/1/1/11/', 1,'ChangesInPropertyPlantAndEquipment','OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', 'Owner-occupied property measured using investment property fair value model','The amount of property, plant and equipment representing owner-occupied property measured using the investment property fair value model applying paragraph 29A of IAS 16. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(19,1, '0', 1, '/1/1/1/12/', 1,'ChangesInPropertyPlantAndEquipment','OtherPropertyPlantAndEquipment', 'Other property, plant and equipment','The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(20,1, '0', 1, '/1/1/2/', 1,'ChangesInInvestmentProperty','InvestmentProperty', 'Investment property','The amount of property (land or a building - or part of a building - or both) held (by the owner or by the lessee as a right-of-use asset) to earn rentals or for capital appreciation or both, rather than for: (a) use in the production or supply of goods or services or for administrative purposes; or (b) sale in the ordinary course of business.')
INSERT INTO @AT VALUES(23,0, '0', 0, '/1/1/3/', 1,'ChangesInGoodwill','Goodwill', 'Goodwill','The amount of assets representing the future economic benefits arising from other assets acquired in a business combination that are not individually identified and separately recognised. [Refer: Business combinations [member]]')
INSERT INTO @AT VALUES(24,1, '0', 1, '/1/1/4/', 1,'ChangesInIntangibleAssetsOtherThanGoodwill','IntangibleAssetsOtherThanGoodwill', 'Intangible assets other than goodwill','The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(34,0, NULL, 1, '/1/1/5/', 1,NULL,'OtherFinancialAssets', 'Other financial assets','The amount of financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(35,0, NULL, 1, '/1/1/6/', 1,NULL,'OtherNonfinancialAssets', 'Other non-financial assets','The amount of non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(36,0, '0', 1, '/1/1/7/', 1,NULL,'InvestmentAccountedForUsingEquityMethod', 'Investments accounted for using equity method','The amount of investments accounted for using the equity method. The equity method is a method of accounting whereby the investment is initially recognised at cost and adjusted thereafter for the post-acquisition change in the investor''s share of net assets of the investee. The investor''s profit or loss includes its share of the profit or loss of the investee. The investor''s other comprehensive income includes its share of the other comprehensive income of the investee. [Refer: At cost [member]]')
INSERT INTO @AT VALUES(39,0, '0', 1, '/1/1/8/', 1,NULL,'InvestmentsInSubsidiariesJointVenturesAndAssociates', 'Investments in subsidiaries, joint ventures and associates','The amount of investments in subsidiaries, joint ventures and associates in an entity''s separate financial statements. [Refer: Associates [member]; Joint ventures [member]; Subsidiaries [member]; Investments in subsidiaries]')
INSERT INTO @AT VALUES(43,1, NULL, 0, '/1/1/9/', 1,'ChangesInBiologicalAssets','BiologicalAssets', 'Biological assets','The amount of living animals or plants recognised as assets.')
INSERT INTO @AT VALUES(44,0, NULL, 0, '/1/1/10/', 1,NULL,'NoncurrentAssetsOrDisposalGroupsClassifiedAsHeldForSaleOrAsHeldForDistributionToOwners', 'Non-current assets or disposal groups classified as held for sale or as held for distribution to owners','The amount of non-current assets or disposal groups classified as held for sale or as held for distribution to owners. [Refer: Non-current assets or disposal groups classified as held for distribution to owners; Non-current assets or disposal groups classified as held for sale]')
INSERT INTO @AT VALUES(45,1, NULL, 1, '/1/1/11/', 1,'ChangesInInventories','InventoriesTotal', 'Inventories','The amount of assets: (a) held for sale in the ordinary course of business; (b) in the process of production for such sale; or (c) in the form of materials or supplies to be consumed in the production process or in the rendering of services. Inventories encompass goods purchased and held for resale including, for example, merchandise purchased by a retailer and held for resale, or land and other property held for resale. Inventories also encompass finished goods produced, or work in progress being produced, by the entity and include materials and supplies awaiting use in the production process. [Refer: Current finished goods; Current merchandise; Current work in progress; Land]')
INSERT INTO @AT VALUES(46,1, '1', 1, '/1/1/11/1/', 0,'ChangesInInventories','CurrentRawMaterialsAndCurrentProductionSuppliesAbstract', 'Current raw materials and current production supplies [abstract]','')
INSERT INTO @AT VALUES(47,1, '1', 1, '/1/1/11/1/1/', 1,'ChangesInInventories','RawMaterials', 'Current raw materials','A classification of current inventory representing the amount of assets to be consumed in the production process or in the rendering of services. [Refer: Inventories]')
INSERT INTO @AT VALUES(48,1, '1', 1, '/1/1/11/1/2/', 1,'ChangesInInventories','ProductionSupplies', 'Current production supplies','A classification of current inventory representing the amount of supplies to be used for the production process. [Refer: Inventories]')
INSERT INTO @AT VALUES(49,1, '0', 1, '/1/1/11/2/', 1,'ChangesInInventories','Merchandise', 'Current merchandise','A classification of current inventory representing the amount of goods acquired for resale. [Refer: Inventories]')
INSERT INTO @AT VALUES(50,1, '0', 1, '/1/1/11/3/', 1,'ChangesInInventories','CurrentFoodAndBeverage', 'Current food and beverage','A classification of current inventory representing the amount of food and beverage. [Refer: Inventories]')
INSERT INTO @AT VALUES(51,1, '0', 1, '/1/1/11/4/', 1,'ChangesInInventories','CurrentAgriculturalProduce', 'Current agricultural produce','A classification of current inventory representing the amount of harvested produce of the entity''s biological assets. [Refer: Biological assets; Inventories]')
INSERT INTO @AT VALUES(52,1, '1', 1, '/1/1/11/5/', 1,'ChangesInInventories','WorkInProgress', 'Current work in progress','A classification of current inventory representing the amount of assets currently in production, which require further processes to be converted into finished goods or services. [Refer: Current finished goods; Inventories]')
INSERT INTO @AT VALUES(53,1, '1', 1, '/1/1/11/6/', 1,'ChangesInInventories','FinishedGoods', 'Current finished goods','A classification of current inventory representing the amount of goods that have completed the production process and are held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(54,1, '1', 1, '/1/1/11/7/', 1,'ChangesInInventories','CurrentPackagingAndStorageMaterials', 'Current packaging and storage materials','A classification of current inventory representing the amount of packaging and storage materials. [Refer: Inventories]')
INSERT INTO @AT VALUES(55,1, '1', 1, '/1/1/11/8/', 1,'ChangesInInventories','SpareParts', 'Current spare parts','A classification of current inventory representing the amount of interchangeable parts that are kept in an inventory and are used for the repair or replacement of failed parts. [Refer: Inventories]')
INSERT INTO @AT VALUES(56,1, '1', 1, '/1/1/11/9/', 1,'ChangesInInventories','PropertyIntendedForSaleInOrdinaryCourseOfBusiness', 'Property intended for sale in ordinary course of business','The amount of property intended for sale in the ordinary course of business of the entity. Property is land or a building - or part of a building - or both.')
INSERT INTO @AT VALUES(57,0, '1', 1, '/1/1/11/10/', 1,'ChangesInInventories','CurrentInventoriesInTransit', 'Current inventories in transit','A classification of current inventory representing the amount of inventories in transit. [Refer: Inventories]')
INSERT INTO @AT VALUES(58,1, '1', 1, '/1/1/11/11/', 1,'ChangesInInventories','OtherInventories', 'Other current inventories','The amount of inventory that the entity does not separately disclose in the same statement or note. [Refer: Inventories]')

INSERT INTO @AT VALUES(59,0, NULL, 1, '/1/1/12/', 1,NULL,'CurrentTaxAssets', 'Current tax assets','The excess of amount paid for current tax in respect of current and prior periods over the amount due for those periods. Current tax is the amount of income taxes payable (recoverable) in respect of the taxable profit (tax loss) for a period.')
INSERT INTO @AT VALUES(60,0, NULL, 0, '/1/1/13/', 1,NULL,'DeferredTaxAssets', 'Deferred tax assets','The amounts of income taxes recoverable in future periods in respect of: (a) deductible temporary differences; (b) the carryforward of unused tax losses; and (c) the carryforward of unused tax credits. [Refer: Temporary differences [member]; Unused tax credits [member]; Unused tax losses [member]]')
INSERT INTO @AT VALUES(61,0, NULL, 1, '/1/1/14/', 1,NULL,'TradeAndOtherReceivables', 'Trade and other receivables','The amount of trade receivables and other receivables. [Refer: Trade receivables; Other receivables]')
INSERT INTO @AT VALUES(68,0, '1', 1, '/1/1/15/', 1,'IncreaseDecreaseInCashAndCashEquivalents','CashAndCashEquivalents', 'Cash and cash equivalents','The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(77,0, '1', 0, '/1/1/16/', 1,NULL,'NoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', 'Non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral','The amount of non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(78,0, NULL, 1, '/1/2/', 0,NULL,'EquityAndLiabilitiesAbstract', 'Equity and liabilities [abstract]','')
INSERT INTO @AT VALUES(79,0, '0', 1, '/1/2/1/', 0,'ChangesInEquity','EquityAbstract', 'Equity [abstract]','')
INSERT INTO @AT VALUES(80,0, '0', 1, '/1/2/1/1/', 1,'ChangesInEquity','IssuedCapital', 'Issued capital','The nominal value of capital issued.')
INSERT INTO @AT VALUES(81,0, '0', 1, '/1/2/1/2/', 1,'ChangesInEquity','RetainedEarnings', 'Retained earnings','A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(85,0, '0', 1, '/1/2/1/3/', 1,'ChangesInEquity','OtherReserves', 'Other reserves','A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(102,0, NULL, 1, '/1/2/2/', 0,NULL,'LiabilitiesAbstract', 'Liabilities [abstract]','')
INSERT INTO @AT VALUES(103,0, NULL, 1, '/1/2/2/1/', 1,NULL,'TradeAndOtherPayables', 'Trade and other payables','The amount of trade payables and other payables. [Refer: Trade payables; Other payables]')
INSERT INTO @AT VALUES(112,0, NULL, 1, '/1/2/2/2/', 0,NULL,'ProvisionsAbstract', 'Provisions [abstract]','')
INSERT INTO @AT VALUES(113,0, NULL, 1, '/1/2/2/2/1/', 1,NULL,'ProvisionsForEmployeeBenefits', 'Provisions for employee benefits','The amount of provisions for employee benefits. [Refer: Employee benefits expense; Provisions]')
INSERT INTO @AT VALUES(114,0, NULL, 1, '/1/2/2/2/2/', 1,'ChangesInOtherProvisions','OtherProvisions', 'Other provisions','The amount of provisions other than provisions for employee benefits. [Refer: Provisions]')
INSERT INTO @AT VALUES(121,0, NULL, 1, '/1/2/2/3/', 1,NULL,'OtherFinancialLiabilities', 'Other financial liabilities','The amount of financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Financial liabilities]')
INSERT INTO @AT VALUES(122,0, NULL, 1, '/1/2/2/6/', 1,NULL,'OtherNonfinancialLiabilities', 'Other non-financial liabilities','The amount of non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(125,0, NULL, 1, '/1/2/2/7/', 1,NULL,'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale', 'Liabilities included in disposal groups classified as held for sale','The amount of liabilities included in disposal groups classified as held for sale. [Refer: Liabilities; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(126,0, NULL, 1, '/2/', 0,NULL,'IncomeStatementAbstract', 'Profit or loss [abstract]','')
INSERT INTO @AT VALUES(127,0, '1', 1, '/2/1/', 1,NULL,'Revenue', 'Revenue','The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(133,0, '1', 1, '/2/2/', 1,NULL,'OtherIncome', 'Other income','The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(134,0, '1', 1, '/2/3/', 0,'ExpenseByFunctionExtension','ExpenseByNatureAbstract', 'Expenses by nature [abstract]','The amount of acquisition and administration expense relating to insurance contracts. [Refer: Types of insurance contracts [member]]')
INSERT INTO @AT VALUES(135,0, '1', 1, '/2/3/1/', 1,'ExpenseByFunctionExtension','RawMaterialsAndConsumablesUsed', 'Raw materials and consumables used','The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(137,1, '1', 1, '/2/3/2/', 1,'ExpenseByFunctionExtension','ServicesExpense', 'Services expense','The amount of expense arising from services.')
INSERT INTO @AT VALUES(146,1, '1', 1, '/2/3/3/', 1,'ExpenseByFunctionExtension','EmployeeBenefitsExpense', 'Employee benefits expense','The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(156,0, '1', 1, '/2/3/4/', 0,'ExpenseByFunctionExtension','DepreciationAndAmortisationExpenseAbstract', 'Depreciation and amortisation expense [abstract]','')
INSERT INTO @AT VALUES(159,0, '1', 1, '/2/3/5/', 1,'ExpenseByFunctionExtension','ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', 'Reversal of impairment loss (impairment loss) recognised in profit or loss','The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(160,0, '1', 1, '/2/3/6/', 1,'ExpenseByFunctionExtension','OtherExpenseByNature', 'Other expenses','The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(161,0, '1', 1, '/2/4/', 1,NULL,'OtherGainsLosses', 'Other gains (losses)','The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(162,0, '1', 0, '/2/5/', 1,NULL,'GainsLossesOnNetMonetaryPosition', 'Gains (losses) on net monetary position','The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')
INSERT INTO @AT VALUES(163,0, '1', 0, '/2/6/', 1,NULL,'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost', 'Gain (loss) arising from derecognition of financial assets measured at amortised cost','The gain (loss) arising from the derecognition of financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(164,0, '1', 0, '/2/7/', 1,NULL,'FinanceIncome', 'Finance income','The amount of income associated with interest and other financing activities of the entity.')
INSERT INTO @AT VALUES(165,0, '1', 1, '/2/8/', 1,NULL,'FinanceCosts', 'Finance costs','The amount of costs associated with financing activities of the entity.')
INSERT INTO @AT VALUES(166,0, '1', 1, '/2/9/', 1,NULL,'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9', 'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9','The amount of impairment loss, impairment gain or reversal of impairment loss that is recognised in profit or loss in accordance with paragraph 5.5.8 of IFRS 9 and that arises from applying the impairment requirements in Section 5.5 of IFRS 9.')
INSERT INTO @AT VALUES(167,0, '1', 0, '/2/10/', 1,NULL,'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod', 'Share of profit (loss) of associates and joint ventures accounted for using equity method','The entity''s share of the profit (loss) of associates and joint ventures accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method; Joint ventures [member]; Profit (loss)]')
INSERT INTO @AT VALUES(168,0, '1', 0, '/2/11/', 1,NULL,'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates', 'Other income (expense) from subsidiaries, jointly controlled entities and associates','The amount of income or expense from subsidiaries, jointly controlled entities and associates that the entity does not separately disclose in the same statement or note. [Refer: Associates [member]; Subsidiaries [member]]')
INSERT INTO @AT VALUES(169,0, '1', 0, '/2/12/', 1,NULL,'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue', 'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category','The gains (losses) arising from the difference between the previous amortised cost and the fair value of financial assets reclassified out of the amortised cost into the fair value through profit or loss measurement category. [Refer: At fair value [member]; Financial assets at amortised cost]')
INSERT INTO @AT VALUES(170,0, '1', 0, '/2/13/', 1,NULL,'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory', 'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category','The cumulative gain (loss) previously recognised in other comprehensive income arising from the reclassification of financial assets out of the fair value through other comprehensive income into the fair value through profit or loss measurement category. [Refer: Financial assets measured at fair value through other comprehensive income; Financial assets at fair value through profit or loss; Other comprehensive income]')
INSERT INTO @AT VALUES(171,0, '1', 0, '/2/14/', 1,NULL,'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions', 'Hedging gains (losses) for hedge of group of items with offsetting risk positions','The hedging gains (losses) for hedge of group of items with offsetting risk positions.')
INSERT INTO @AT VALUES(172,0, '1', 0, '/2/15/', 1,NULL,'IncomeTaxExpenseContinuingOperations', 'Tax income (expense)','The aggregate amount included in the determination of profit (loss) for the period in respect of current tax and deferred tax. [Refer: Current tax expense (income); Deferred tax expense (income)]')


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
	UPDATE dbo.[AccountTypes] SET IsReal = 1 WHERE [Node].IsDescendantOf('/1/1/1/') = 1
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