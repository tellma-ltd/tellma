IF NOT EXISTS(SELECT * FROM dbo.[AccountTypes])
BEGIN
DECLARE @AT TABLE (
	[Index] INT,
	[Node] HIERARCHYID, [EntryTypeParentCode] NVARCHAR (255), [Code] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
)
DECLARE @AccountTypes dbo.AccountTypeList;

INSERT INTO @AT VALUES(0,'/1/', NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
INSERT INTO @AT VALUES(1,'/1/1/', NULL,N'Assets', N'Assets',N'The amount of resources: (a) controlled by the entity as a result of past events; and (b) from which future economic benefits are expected to flow to the entity.')
INSERT INTO @AT VALUES(2,'/1/1/1/', NULL,N'NoncurrentAssets', N'Non-current assets',N'The amount of assets that do not meet the definition of current assets. [Refer: Current assets]')
INSERT INTO @AT VALUES(3,'/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
INSERT INTO @AT VALUES(4,'/1/1/1/1/1/', NULL,N'LandAndBuildings', N'Land and buildings',N'The amount of property, plant and equipment representing land and depreciable buildings and similar structures for use in operations. [Refer: Buildings; Land; Property, plant and equipment]')
INSERT INTO @AT VALUES(5,'/1/1/1/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'Land', N'Land',N'The amount of property, plant and equipment representing land held by the entity for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(6,'/1/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Buildings', N'Buildings',N'The amount of property, plant and equipment representing depreciable buildings and similar structures for use in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(7,'/1/1/1/1/2/', N'ChangesInPropertyPlantAndEquipment',N'Machinery', N'Machinery',N'The amount of property, plant and equipment representing long-lived, depreciable machinery used in operations. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(8,'/1/1/1/1/3/', N'ChangesInPropertyPlantAndEquipment',N'Vehicles', N'Vehicles',N'The amount of property, plant and equipment representing vehicles used in the entity''s operations, specifically to include aircraft, motor vehicles and ships. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(9,'/1/1/1/1/4/', N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
INSERT INTO @AT VALUES(10,'/1/1/1/1/5/', N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(11,'/1/1/1/1/6/', NULL,N'BearerPlants', N'Bearer plants',N'The amount of property, plant and equipment representing bearer plants. Bearer plant is a living plant that (a) is used in the production or supply of agricultural produce; (b) is expected to bear produce for more than one period; and (c) has a remote likelihood of being sold as agricultural produce, except for incidental scrap sales. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(12,'/1/1/1/1/7/', N'ChangesInPropertyPlantAndEquipment',N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',N'The amount of exploration and evaluation assets recognised as tangible assets in accordance with the entity''s accounting policy. [Refer: Exploration and evaluation assets [member]]')
INSERT INTO @AT VALUES(13,'/1/1/1/1/8/', NULL,N'MiningAssets', N'Mining assets',N'The amount of assets related to mining activities of the entity.')
INSERT INTO @AT VALUES(14,'/1/1/1/1/9/', NULL,N'OilAndGasAssets', N'Oil and gas assets',N'The amount of assets related to the exploration, evaluation, development or production of oil and gas.')
INSERT INTO @AT VALUES(15,'/1/1/1/1/10/', N'ChangesInPropertyPlantAndEquipment',N'ConstructionInProgress', N'Construction in progress',N'The amount of expenditure capitalised during the construction of non-current assets that are not yet available for use. [Refer: Non-current assets]')
INSERT INTO @AT VALUES(16,'/1/1/1/1/11/', N'ChangesInPropertyPlantAndEquipment',N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', N'Owner-occupied property measured using investment property fair value model',N'The amount of property, plant and equipment representing owner-occupied property measured using the investment property fair value model applying paragraph 29A of IAS 16. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(17,'/1/1/1/1/99/', N'ChangesInPropertyPlantAndEquipment',N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment',N'The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
INSERT INTO @AT VALUES(18,'/1/1/1/2/', N'ChangesInInvestmentProperty',N'InvestmentProperty', N'Investment property',N'The amount of property (land or a building - or part of a building - or both) held (by the owner or by the lessee as a right-of-use asset) to earn rentals or for capital appreciation or both, rather than for: (a) use in the production or supply of goods or services or for administrative purposes; or (b) sale in the ordinary course of business.')
INSERT INTO @AT VALUES(19,'/1/1/1/3/', N'ChangesInGoodwill',N'Goodwill', N'Goodwill',N'The amount of assets representing the future economic benefits arising from other assets acquired in a business combination that are not individually identified and separately recognised. [Refer: Business combinations [member]]')
INSERT INTO @AT VALUES(20,'/1/1/1/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
INSERT INTO @AT VALUES(21,'/1/1/1/5/', NULL,N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',N'The amount of investments accounted for using the equity method. The equity method is a method of accounting whereby the investment is initially recognised at cost and adjusted thereafter for the post-acquisition change in the investor''s share of net assets of the investee. The investor''s profit or loss includes its share of the profit or loss of the investee. The investor''s other comprehensive income includes its share of the other comprehensive income of the investee. [Refer: At cost [member]]')
INSERT INTO @AT VALUES(22,'/1/1/1/6/', NULL,N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',N'The amount of investments in subsidiaries, joint ventures and associates in an entity''s separate financial statements. [Refer: Associates [member]; Joint ventures [member]; Subsidiaries [member]; Investments in subsidiaries]')
INSERT INTO @AT VALUES(23,'/1/1/1/7/', NULL,N'NoncurrentBiologicalAssets', N'Non-current biological assets',N'The amount of living animals or plants recognised as assets.')
INSERT INTO @AT VALUES(24,'/1/1/1/8/', NULL,N'NoncurrentReceivables', N'Trade and other non-current receivables',N'The amount of non-current trade receivables and non-current other receivables. [Refer: Non-current trade receivables; Other non-current receivables]')
INSERT INTO @AT VALUES(25,'/1/1/1/8/1/', NULL,N'NoncurrentTradeReceivables', N'Non-current trade receivables',N'The amount of non-current trade receivables and non-current other receivables. [Refer: Non-current trade receivables; Other non-current receivables]')
INSERT INTO @AT VALUES(26,'/1/1/1/8/2/', NULL,N'NoncurrentReceivablesDueFromRelatedParties', N'Non-current receivables due from related parties',N'The amount of non-current receivables due from related parties. [Refer: Related parties [member]]')
INSERT INTO @AT VALUES(27,'/1/1/1/8/3/', NULL,N'NoncurrentPrepaymentsAndNoncurrentAccruedIncome', N'Non-current prepayments and non-current accrued income',N'The amount of non-current prepayments and non-current accrued income. [Refer: Prepayments; Accrued income]')
INSERT INTO @AT VALUES(28,'/1/1/1/8/3/1/', NULL,N'NoncurrentPrepayments', N'Non-current prepayments',N'The amount of non-current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(29,'/1/1/1/8/3/2/', NULL,N'NoncurrentAccruedIncome', N'Non-current accrued income',N'The amount of non-current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(30,'/1/1/1/8/4/', NULL,N'NoncurrentReceivablesFromTaxesOtherThanIncomeTax', N'Non-current receivables from taxes other than income tax',N'The amount of non-current receivables from taxes other than income tax. [Refer: Receivables from taxes other than income tax]')
INSERT INTO @AT VALUES(31,'/1/1/1/8/5/', NULL,N'NoncurrentReceivablesFromSaleOfProperties', N'Non-current receivables from sale of properties',N'The amount of non-current receivables from sale of properties. [Refer: Receivables from sale of properties]')
INSERT INTO @AT VALUES(32,'/1/1/1/8/6/', NULL,N'NoncurrentReceivablesFromRentalOfProperties', N'Non-current receivables from rental of properties',N'The amount of non-current receivables from rental of properties. [Refer: Receivables from rental of properties]')
INSERT INTO @AT VALUES(33,'/1/1/1/8/7/', NULL,N'OtherNoncurrentReceivables', N'Other non-current receivables',N'The amount of non-current other receivables. [Refer: Other receivables]')
INSERT INTO @AT VALUES(34,'/1/1/1/9/', NULL,N'NoncurrentInventories', N'Non-current inventories',N'The amount of non-current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(35,'/1/1/1/10/', NULL,N'DeferredTaxAssets', N'Deferred tax assets',N'The amounts of income taxes recoverable in future periods in respect of: (a) deductible temporary differences; (b) the carryforward of unused tax losses; and (c) the carryforward of unused tax credits. [Refer: Temporary differences [member]; Unused tax credits [member]; Unused tax losses [member]]')
INSERT INTO @AT VALUES(36,'/1/1/1/11/', NULL,N'CurrentTaxAssetsNoncurrent', N'Current tax assets, non-current',N'The non-current amount of current tax assets. [Refer: Current tax assets]')
INSERT INTO @AT VALUES(37,'/1/1/1/12/', NULL,N'OtherNoncurrentFinancialAssets', N'Other non-current financial assets',N'The amount of non-current financial assets that the entity does not separately disclose in the same statement or note. [Refer: Other financial assets]')
INSERT INTO @AT VALUES(38,'/1/1/1/13/', NULL,N'OtherNoncurrentNonfinancialAssets', N'Other non-current non-financial assets',N'The amount of non-current non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(39,'/1/1/1/14/', NULL,N'NoncurrentNoncashAssetsPledgedAsCollateralForWhichTransfereeHasRightByContractOrCustomToSellOrRepledgeCollateral', N'Non-current non-cash assets pledged as collateral for which transferee has right by contract or custom to sell or repledge collateral',N'The amount of non-current non-cash collateral assets (such as debt or equity instruments) provided to a transferee, for which the transferee has the right by contract or custom to sell or repledge the collateral.')
INSERT INTO @AT VALUES(40,'/1/1/2/', NULL,N'CurrentAssets', N'Current assets',N'')
INSERT INTO @AT VALUES(41,'/1/1/2/1/', N'ChangesInInventories',N'Inventories', N'Current inventories',N'The amount of current inventories. [Refer: Inventories]')
INSERT INTO @AT VALUES(42,'/1/1/2/1/1/', N'ChangesInInventories',N'CurrentRawMaterialsAndCurrentProductionSupplies', N'Current raw materials and current production supplies',N'A classification of current inventory representing the amount of current raw materials and current production supplies. [Refer: Current production supplies; Current raw materials]')
INSERT INTO @AT VALUES(43,'/1/1/2/1/2/', N'ChangesInInventories',N'Merchandise', N'Current merchandise',N'A classification of current inventory representing the amount of goods acquired for resale. [Refer: Inventories]')
INSERT INTO @AT VALUES(44,'/1/1/2/1/3/', N'ChangesInInventories',N'CurrentFoodAndBeverage', N'Current food and beverage',N'A classification of current inventory representing the amount of food and beverage. [Refer: Inventories]')
INSERT INTO @AT VALUES(45,'/1/1/2/1/4/', N'ChangesInInventories',N'CurrentAgriculturalProduce', N'Current agricultural produce',N'A classification of current inventory representing the amount of harvested produce of the entity''s biological assets. [Refer: Biological assets; Inventories]')
INSERT INTO @AT VALUES(46,'/1/1/2/1/5/', N'ChangesInInventories',N'WorkInProgress', N'Current work in progress',N'A classification of current inventory representing the amount of assets currently in production, which require further processes to be converted into finished goods or services. [Refer: Current finished goods; Inventories]')
INSERT INTO @AT VALUES(47,'/1/1/2/1/6/', N'ChangesInInventories',N'FinishedGoods', N'Current finished goods',N'A classification of current inventory representing the amount of goods that have completed the production process and are held for sale in the ordinary course of business. [Refer: Inventories]')
INSERT INTO @AT VALUES(48,'/1/1/2/1/7/', N'ChangesInInventories',N'CurrentPackagingAndStorageMaterials', N'Current packaging and storage materials',N'A classification of current inventory representing the amount of packaging and storage materials. [Refer: Inventories]')
INSERT INTO @AT VALUES(49,'/1/1/2/1/8/', N'ChangesInInventories',N'SpareParts', N'Current spare parts',N'A classification of current inventory representing the amount of interchangeable parts that are kept in an inventory and are used for the repair or replacement of failed parts. [Refer: Inventories]')
INSERT INTO @AT VALUES(50,'/1/1/2/1/9/', N'ChangesInInventories',N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property intended for sale in ordinary course of business',N'The amount of property intended for sale in the ordinary course of business of the entity. Property is land or a building - or part of a building - or both.')
INSERT INTO @AT VALUES(51,'/1/1/2/1/10/', N'ChangesInInventories',N'CurrentInventoriesInTransit', N'Current inventories in transit',N'A classification of current inventory representing the amount of inventories in transit. [Refer: Inventories]')
INSERT INTO @AT VALUES(52,'/1/1/2/1/11/', N'ChangesInInventories',N'OtherInventories', N'Other current inventories',N'The amount of inventory that the entity does not separately disclose in the same statement or note. [Refer: Inventories]')
INSERT INTO @AT VALUES(53,'/1/1/2/2/', NULL,N'TradeAndOtherCurrentReceivables', N'Trade and other current receivables',N'The amount of current trade receivables and current other receivables. [Refer: Current trade receivables; Other current receivables]')
INSERT INTO @AT VALUES(54,'/1/1/2/2/1/', NULL,N'CurrentTradeReceivables', N'Current trade receivables',N'The amount of current trade receivables. [Refer: Trade receivables]')
INSERT INTO @AT VALUES(55,'/1/1/2/2/3/', NULL,N'CurrentPrepayments', N'Current prepayments',N'The amount of current prepayments. [Refer: Prepayments]')
INSERT INTO @AT VALUES(56,'/1/1/2/2/4/', NULL,N'CurrentAccruedIncome', N'Current accrued income',N'The amount of current accrued income. [Refer: Accrued income]')
INSERT INTO @AT VALUES(57,'/1/1/2/2/5/', NULL,N'CurrentValueAddedTaxReceivables', N'Current value added tax receivables',N'The amount of current value added tax receivables. [Refer: Value added tax receivables]')
INSERT INTO @AT VALUES(58,'/1/1/2/2/6/', NULL,N'WithholdingTaxReceivablesExtension', N'Withholding tax receivables',N'The amount of receivables related to a withtholding tax.')
INSERT INTO @AT VALUES(59,'/1/1/2/2/9/', NULL,N'OtherCurrentReceivables', N'Other current receivables',N'The amount of current other receivables. [Refer: Other receivables]')
INSERT INTO @AT VALUES(60,'/1/1/2/3/', NULL,N'CurrentTaxAssetsCurrent', N'Current tax assets, current',N'The current amount of current tax assets. [Refer: Current tax assets]')
INSERT INTO @AT VALUES(61,'/1/1/2/4/', NULL,N'CurrentBiologicalAssets', N'Current biological assets',N'The amount of current biological assets. [Refer: Biological assets]')
INSERT INTO @AT VALUES(62,'/1/1/2/5/', NULL,N'OtherCurrentFinancialAssets', N'Other current financial assets',N'The amount of current financial assets that the entity does not separately disclose in the same statement or note. [Refer: Other financial assets; Current financial assets]')
INSERT INTO @AT VALUES(63,'/1/1/2/6/', NULL,N'OtherCurrentNonfinancialAssets', N'Other current non-financial assets',N'The amount of current non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
INSERT INTO @AT VALUES(64,'/1/1/2/7/', NULL,N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
INSERT INTO @AT VALUES(65,'/1/1/2/7/1/', NULL,N'Cash', N'Cash',N'The amount of cash on hand and demand deposits. [Refer: Cash on hand]')
INSERT INTO @AT VALUES(66,'/1/1/2/7/1/1/', NULL,N'CashOnHand', N'Cash on hand',N'The amount of cash held by the entity. This does not include demand deposits.')
INSERT INTO @AT VALUES(67,'/1/1/2/7/1/2/', NULL,N'BalancesWithBanks', N'Balances with banks',N'The amount of cash balances held at banks.')
INSERT INTO @AT VALUES(68,'/1/1/2/7/2/', NULL,N'CashEquivalents', N'Cash equivalents',N'The amount of short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value.')
INSERT INTO @AT VALUES(69,'/1/1/2/7/2/1/', NULL,N'ShorttermDepositsClassifiedAsCashEquivalents', N'Short-term deposits, classified as cash equivalents',N'A classification of cash equivalents representing short-term deposits. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(70,'/1/1/2/7/2/2/', NULL,N'ShorttermInvestmentsClassifiedAsCashEquivalents', N'Short-term investments, classified as cash equivalents',N'A classification of cash equivalents representing short-term investments. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(71,'/1/1/2/7/2/3/', NULL,N'BankingArrangementsClassifiedAsCashEquivalents', N'Other banking arrangements, classified as cash equivalents',N'A classification of cash equivalents representing banking arrangements that the entity does not separately disclose in the same statement or note. [Refer: Cash equivalents]')
INSERT INTO @AT VALUES(72,'/1/1/2/7/3/', NULL,N'OtherCashAndCashEquivalents', N'Other cash and cash equivalents',N'The amount of cash and cash equivalents that the entity does not separately disclose in the same statement or note. [Refer: Cash and cash equivalents]')
INSERT INTO @AT VALUES(73,'/1/2/', NULL,N'EquityAndLiabilities', N'Equity and liabilities',N'The amount of the entity''s equity and liabilities. [Refer: Equity; Liabilities]')
INSERT INTO @AT VALUES(74,'/1/2/1/', N'ChangesInEquity',N'Equity', N'Equity',N'The amount of residual interest in the assets of the entity after deducting all its liabilities.')
INSERT INTO @AT VALUES(75,'/1/2/1/1/', N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
INSERT INTO @AT VALUES(76,'/1/2/1/2/', N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
INSERT INTO @AT VALUES(77,'/1/2/1/3/', N'ChangesInEquity',N'SharePremium', N'Share premium',N'The amount received or receivable from the issuance of the entity''s shares in excess of nominal value.')
INSERT INTO @AT VALUES(78,'/1/2/1/4/', N'ChangesInEquity',N'TreasuryShares', N'Treasury shares',N'An entity’s own equity instruments, held by the entity or other members of the consolidated group.')
INSERT INTO @AT VALUES(79,'/1/2/1/5/', N'ChangesInEquity',N'OtherEquityInterest', N'Other equity interest',N'The amount of equity interest of an entity without share capital that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(80,'/1/2/1/6/', N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
INSERT INTO @AT VALUES(81,'/1/2/2/', NULL,N'Liabilities', N'Liabilities',N'The amount of a present obligation of the entity to transfer an economic resource as a result of past events. Economic resource is a right that has the potential to produce economic benefits.')
INSERT INTO @AT VALUES(82,'/1/2/2/1/', NULL,N'NoncurrentLiabilities', N'Non-current liabilities',N'The amount of liabilities that do not meet the definition of current liabilities. [Refer: Current liabilities]')
INSERT INTO @AT VALUES(83,'/1/2/2/1/1/', NULL,N'NoncurrentProvisions', N'Non-current provisions',N'The amount of non-current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(84,'/1/2/2/1/1/1/', NULL,N'NoncurrentProvisionsForEmployeeBenefits', N'Non-current provisions for employee benefits',N'The amount of non-current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(85,'/1/2/2/1/1/2/', NULL,N'OtherLongtermProvisions', N'Other non-current provisions',N'The amount of non-current provisions other than provisions for employee benefits. [Refer: Non-current provisions]')
INSERT INTO @AT VALUES(86,'/1/2/2/1/2/', NULL,N'NoncurrentPayables', N'Trade and other non-current payables',N'The amount of non-current trade payables and non-current other payables. [Refer: Other non-current payables; Non-current trade payables]')
INSERT INTO @AT VALUES(87,'/1/2/2/1/3/', NULL,N'DeferredTaxLiabilities', N'Deferred tax liabilities',N'The amounts of income taxes payable in future periods in respect of taxable temporary differences. [Refer: Temporary differences [member]]')
INSERT INTO @AT VALUES(88,'/1/2/2/1/4/', NULL,N'CurrentTaxLiabilitiesNoncurrent', N'Current tax liabilities, non-current',N'The non-current amount of current tax liabilities. [Refer: Current tax liabilities]')
INSERT INTO @AT VALUES(89,'/1/2/2/1/5/', NULL,N'OtherNoncurrentFinancialLiabilities', N'Other non-current financial liabilities',N'The amount of non-current financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(90,'/1/2/2/1/6/', NULL,N'OtherNoncurrentNonfinancialLiabilities', N'Other non-current non-financial liabilities',N'The amount of non-current non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(91,'/1/2/2/2/', NULL,N'CurrentLiabilities', N'Current liabilities',N'The amount of liabilities that: (a) the entity expects to settle in its normal operating cycle; (b) the entity holds primarily for the purpose of trading; (c) are due to be settled within twelve months after the reporting period; or (d) the entity does not have an unconditional right to defer settlement for at least twelve months after the reporting period.')
INSERT INTO @AT VALUES(92,'/1/2/2/2/1/', NULL,N'CurrentProvisions', N'Current provisions',N'The amount of current provisions. [Refer: Provisions]')
INSERT INTO @AT VALUES(93,'/1/2/2/2/1/1/', NULL,N'CurrentProvisionsForEmployeeBenefits', N'Current provisions for employee benefits',N'The amount of current provisions for employee benefits. [Refer: Provisions for employee benefits]')
INSERT INTO @AT VALUES(94,'/1/2/2/2/1/2/', N'ChangesInOtherProvisions',N'OtherShorttermProvisions', N'Other current provisions',N'The amount of current provisions other than provisions for employee benefits. [Refer: Provisions]')
INSERT INTO @AT VALUES(95,'/1/2/2/2/2/', NULL,N'TradeAndOtherCurrentPayables', N'Trade and other current payables',N'The amount of current trade payables and current other payables. [Refer: Current trade payables; Other current payables]')
INSERT INTO @AT VALUES(96,'/1/2/2/2/2/1/', NULL,N'TradeAndOtherCurrentPayablesToTradeSuppliers', N'Current trade payables',N'The current amount of payment due to suppliers for goods and services used in entity''s business. [Refer: Current liabilities; Trade payables]')
INSERT INTO @AT VALUES(97,'/1/2/2/2/2/2/', NULL,N'DeferredIncomeClassifiedAsCurrent', N'Deferred income classified as current',N'The amount of deferred income classified as current. [Refer: Deferred income]')
INSERT INTO @AT VALUES(98,'/1/2/2/2/2/3/', NULL,N'AccrualsClassifiedAsCurrent', N'Accruals classified as current',N'The amount of accruals classified as current. [Refer: Accruals]')
INSERT INTO @AT VALUES(99,'/1/2/2/2/2/4/', NULL,N'CurrentPayablesToEmployeesExtension', N'Current Employees payables',N'Amounts payable that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(100,'/1/2/2/2/2/5/', NULL,N'ShorttermEmployeeBenefitsAccruals', N'Short-term employee benefits accruals',N'The amount of accruals for employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services. [Refer: Accruals classified as current]')
INSERT INTO @AT VALUES(101,'/1/2/2/2/2/6/', NULL,N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax', N'Current payables on social security and taxes other than income tax',N'The amount of current payables on social security and taxes other than incomes tax. [Refer: Payables on social security and taxes other than income tax]')
INSERT INTO @AT VALUES(102,'/1/2/2/2/2/6/1/', NULL,N'CurrentValueAddedTaxPayables', N'Current value added tax payables',N'The amount of current value added tax payables. [Refer: Value added tax payables]')
INSERT INTO @AT VALUES(103,'/1/2/2/2/2/6/2/', NULL,N'CurrentExciseTaxPayables', N'Current excise tax payables',N'The amount of current excise tax payables. [Refer: Excise tax payables]')
INSERT INTO @AT VALUES(104,'/1/2/2/2/2/6/3/', NULL,N'CurrentSocialSecurityPayablesExtension', N'Current Social Security payables',N'The amount of current social security payables')
INSERT INTO @AT VALUES(105,'/1/2/2/2/2/6/4/', NULL,N'CurrentZakatPayablesExtension', N'Current Zakat payables',N'The amount of current zakat payables')
INSERT INTO @AT VALUES(106,'/1/2/2/2/2/6/5/', NULL,N'CurrentEmployeeIncomeTaxPayablesExtension', N'Current Employee Income tax payables',N'The amount of current employee income tax payables')
INSERT INTO @AT VALUES(107,'/1/2/2/2/2/6/6/', NULL,N'CurrentEmployeeStampTaxPayablesExtension', N'Current Employee Stamp tax payables',N'The amount of current employee stamp tax payables')
INSERT INTO @AT VALUES(108,'/1/2/2/2/2/7/', NULL,N'CurrentRetentionPayables', N'Current retention payables',N'The amount of current retention payables. [Refer: Retention payables]')
INSERT INTO @AT VALUES(109,'/1/2/2/2/2/8/', NULL,N'OtherCurrentPayables', N'Other current payables',N'The amount of current payables that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(110,'/1/2/2/2/3/', NULL,N'CurrentTaxLiabilitiesCurrent', N'Current tax liabilities, current',N'The current amount of current tax liabilities. [Refer: Current tax liabilities]')
INSERT INTO @AT VALUES(111,'/1/2/2/2/4/', NULL,N'OtherCurrentFinancialLiabilities', N'Other current financial liabilities',N'The amount of current financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities; Current financial liabilities]')
INSERT INTO @AT VALUES(112,'/1/2/2/2/5/', NULL,N'OtherCurrentNonfinancialLiabilities', N'Other current non-financial liabilities',N'The amount of current non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
INSERT INTO @AT VALUES(113,'/1/2/2/2/6/', NULL,N'LiabilitiesIncludedInDisposalGroupsClassifiedAsHeldForSale', N'Liabilities included in disposal groups classified as held for sale',N'The amount of liabilities included in disposal groups classified as held for sale. [Refer: Liabilities; Disposal groups classified as held for sale [member]]')
INSERT INTO @AT VALUES(114,'/2/', NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'The total of income less expenses from continuing and discontinued operations, excluding the components of other comprehensive income. [Refer: Other comprehensive income]')
INSERT INTO @AT VALUES(115,'/2/1/', NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
INSERT INTO @AT VALUES(116,'/2/1/1/', NULL,N'RevenueFromSaleOfGoods', N'Revenue from sale of goods',N'The amount of revenue arising from the sale of goods. [Refer: Revenue]')
INSERT INTO @AT VALUES(117,'/2/1/2/', NULL,N'RevenueFromRenderingOfServices', N'Revenue from rendering of services',N'The amount of revenue arising from the rendering of services. [Refer: Revenue]')
INSERT INTO @AT VALUES(118,'/2/1/3/', NULL,N'RevenueFromInterest', N'Interest income',N'The amount of income arising from interest.')
INSERT INTO @AT VALUES(119,'/2/1/4/', NULL,N'RevenueFromDividends', N'Dividend income',N'The amount of dividends recognised as income. Dividends are distributions of profits to holders of equity investments in proportion to their holdings of a particular class of capital.')
INSERT INTO @AT VALUES(120,'/2/1/5/', NULL,N'OtherRevenue', N'Other revenue',N'The amount of revenue arising from sources that the entity does not separately disclose in the same statement or note. [Refer: Revenue]')
INSERT INTO @AT VALUES(121,'/2/2/', NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(122,'/2/3/', N'ExpenseByFunctionExtension',N'ExpenseByNature', N'Expenses by nature',N'The amount of expenses aggregated according to their nature (for example, depreciation, purchases of materials, transport costs, employee benefits and advertising costs), and not reallocated among functions within the entity.')
INSERT INTO @AT VALUES(123,'/2/3/1/', N'ExpenseByFunctionExtension',N'RawMaterialsAndConsumablesUsed', N'Raw materials and consumables used',N'The amount of raw materials and consumables used in the production process or in the rendering of services. [Refer: Current raw materials]')
INSERT INTO @AT VALUES(124,'/2/3/2/', N'ExpenseByFunctionExtension',N'CostOfMerchandiseSold', N'Cost of merchandise sold',N'The amount of merchandise that was sold during the period and recognised as an expense.')
INSERT INTO @AT VALUES(125,'/2/3/3/', N'ExpenseByFunctionExtension',N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
INSERT INTO @AT VALUES(126,'/2/3/3/1/', N'ExpenseByFunctionExtension',N'InsuranceExpense', N'Insurance expense',N'The amount of expense arising from purchased insurance.')
INSERT INTO @AT VALUES(127,'/2/3/3/2/', N'ExpenseByFunctionExtension',N'ProfessionalFeesExpense', N'Professional fees expense',N'The amount of fees paid or payable for professional services.')
INSERT INTO @AT VALUES(128,'/2/3/3/3/', N'ExpenseByFunctionExtension',N'TransportationExpense', N'Transportation expense',N'The amount of expense arising from transportation services.')
INSERT INTO @AT VALUES(129,'/2/3/3/4/', N'ExpenseByFunctionExtension',N'BankAndSimilarCharges', N'Bank and similar charges',N'The amount of bank and similar charges recognised by the entity as an expense.')
INSERT INTO @AT VALUES(130,'/2/3/3/5/', N'ExpenseByFunctionExtension',N'TravelExpense', N'Travel expense',N'The amount of expense arising from travel.')
INSERT INTO @AT VALUES(131,'/2/3/3/6/', N'ExpenseByFunctionExtension',N'CommunicationExpense', N'Communication expense',N'The amount of expense arising from communication.')
INSERT INTO @AT VALUES(132,'/2/3/3/7/', N'ExpenseByFunctionExtension',N'UtilitiesExpense', N'Utilities expense',N'The amount of expense arising from purchased utilities.')
INSERT INTO @AT VALUES(133,'/2/3/3/8/', N'ExpenseByFunctionExtension',N'AdvertisingExpense', N'Advertising expense',N'The amount of expense arising from advertising.')
INSERT INTO @AT VALUES(134,'/2/3/4/', N'ExpenseByFunctionExtension',N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
INSERT INTO @AT VALUES(135,'/2/3/4/1/', N'ExpenseByFunctionExtension',N'ShorttermEmployeeBenefitsExpense', N'Short-term employee benefits expense',N'The amount of expense from employee benefits (other than termination benefits) that are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services.')
INSERT INTO @AT VALUES(136,'/2/3/4/1/1/', N'ExpenseByFunctionExtension',N'WagesAndSalaries', N'Wages and salaries',N'A class of employee benefits expense that represents wages and salaries. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(137,'/2/3/4/1/2/', N'ExpenseByFunctionExtension',N'SocialSecurityContributions', N'Social security contributions',N'A class of employee benefits expense that represents social security contributions. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(138,'/2/3/4/1/3/', N'ExpenseByFunctionExtension',N'OtherShorttermEmployeeBenefits', N'Other short-term employee benefits',N'The amount of expense from employee benefits (other than termination benefits), which are expected to be settled wholly within twelve months after the end of the annual reporting period in which the employees render the related services, that the entity does not separately disclose in the same statement or note. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(139,'/2/3/4/2/', N'ExpenseByFunctionExtension',N'PostemploymentBenefitExpenseDefinedContributionPlans', N'Post-employment benefit expense, defined contribution plans',N'The amount of post-employment benefit expense relating to defined contribution plans. Defined contribution plans are post-employment benefit plans under which an entity pays fixed contributions into a separate entity (a fund) and will have no legal or constructive obligation to pay further contributions if the fund does not hold sufficient assets to pay all employee benefits relating to employee service in the current and prior periods.')
INSERT INTO @AT VALUES(140,'/2/3/4/3/', N'ExpenseByFunctionExtension',N'PostemploymentBenefitExpenseDefinedBenefitPlans', N'Post-employment benefit expense, defined benefit plans',N'The amount of post-employment benefit expense relating to defined benefit plans. [Refer: Defined benefit plans [member]]')
INSERT INTO @AT VALUES(141,'/2/3/4/4/', N'ExpenseByFunctionExtension',N'TerminationBenefitsExpense', N'Termination benefits expense',N'The amount of expense in relation to termination benefits. Termination benefits are employee benefits provided in exchange for the termination of an employee''s employment as a result of either: (a) an entity''s decision to terminate an employee''s employment before the normal retirement date; or (b) an employee''s decision to accept an offer of benefits in exchange for the termination of employment. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(142,'/2/3/4/5/', N'ExpenseByFunctionExtension',N'OtherLongtermBenefits', N'Other long-term employee benefits',N'The amount of long-term employee benefits other than post-employment benefits and termination benefits. Such benefits may include long-term paid absences, jubilee or other long-service benefits, long-term disability benefits, long-term profit-sharing and bonuses and long-term deferred remuneration. [Refer: Employee benefits expense]')
INSERT INTO @AT VALUES(143,'/2/3/4/6/', N'ExpenseByFunctionExtension',N'OtherEmployeeExpense', N'Other employee expense',N'The amount of employee expenses that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(144,'/2/3/5/', N'ExpenseByFunctionExtension',N'DepreciationAndAmortisationExpense', N'Depreciation and amortisation expense',N'The amount of depreciation and amortisation expense. Depreciation and amortisation are the systematic allocations of depreciable amounts of assets over their useful lives.')
INSERT INTO @AT VALUES(145,'/2/3/5/1/', N'ExpenseByFunctionExtension',N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
INSERT INTO @AT VALUES(146,'/2/3/5/2/', N'ExpenseByFunctionExtension',N'AmortisationExpense', N'Amortisation expense',N'The amount of amortisation expense. Amortisation is the systematic allocation of depreciable amounts of intangible assets over their useful lives.')
INSERT INTO @AT VALUES(147,'/2/3/6/', N'ExpenseByFunctionExtension',N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss', N'Reversal of impairment loss (impairment loss) recognised in profit or loss',N'The amount of impairment loss or reversal of impairment loss recognised in profit or loss. [Refer: Impairment loss recognised in profit or loss; Reversal of impairment loss recognised in profit or loss]')
INSERT INTO @AT VALUES(148,'/2/3/7/', N'ExpenseByFunctionExtension',N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
INSERT INTO @AT VALUES(149,'/2/4/', NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(150,'/2/4/1/', NULL,N'GainLossOnDisposalOfPropertyPlantAndEquipmentExtension', N'Gain (loss) on disposal of property, plant and equipment',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(151,'/2/4/2/', NULL,N'GainLossOnForeignExchangeExtension', N'Gain (loss) on foreign exchange',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')
INSERT INTO @AT VALUES(152,'/2/5/', NULL,N'GainsLossesOnNetMonetaryPosition', N'Gains (losses) on net monetary position',N'The gains (losses) representing the difference resulting from the restatement of non-monetary assets, owners'' equity and items in the statement of comprehensive income and the adjustment of index linked assets and liabilities in hyperinflationary reporting.')
INSERT INTO @AT VALUES(153,'/2/6/', NULL,N'GainLossArisingFromDerecognitionOfFinancialAssetsMeasuredAtAmortisedCost', N'Gain (loss) arising from derecognition of financial assets measured at amortised cost',N'The gain (loss) arising from the derecognition of financial assets measured at amortised cost. [Refer: Financial assets at amortised cost]')
INSERT INTO @AT VALUES(154,'/2/7/', NULL,N'FinanceIncome', N'Finance income',N'The amount of income associated with interest and other financing activities of the entity.')
INSERT INTO @AT VALUES(155,'/2/8/', NULL,N'FinanceCosts', N'Finance costs',N'The amount of costs associated with financing activities of the entity.')
INSERT INTO @AT VALUES(156,'/2/9/', NULL,N'ImpairmentLossImpairmentGainAndReversalOfImpairmentLossDeterminedInAccordanceWithIFRS9', N'Impairment gain and reversal of impairment loss (impairment loss) determined in accordance with IFRS 9',N'The amount of impairment loss, impairment gain or reversal of impairment loss that is recognised in profit or loss in accordance with paragraph 5.5.8 of IFRS 9 and that arises from applying the impairment requirements in Section 5.5 of IFRS 9.')
INSERT INTO @AT VALUES(157,'/2/10/', NULL,N'ShareOfProfitLossOfAssociatesAndJointVenturesAccountedForUsingEquityMethod', N'Share of profit (loss) of associates and joint ventures accounted for using equity method',N'The entity''s share of the profit (loss) of associates and joint ventures accounted for using the equity method. [Refer: Associates [member]; Investments accounted for using equity method; Joint ventures [member]; Profit (loss)]')
INSERT INTO @AT VALUES(158,'/2/11/', NULL,N'OtherIncomeExpenseFromSubsidiariesJointlyControlledEntitiesAndAssociates', N'Other income (expense) from subsidiaries, jointly controlled entities and associates',N'The amount of income or expense from subsidiaries, jointly controlled entities and associates that the entity does not separately disclose in the same statement or note. [Refer: Associates [member]; Subsidiaries [member]]')
INSERT INTO @AT VALUES(159,'/2/12/', NULL,N'GainsLossesArisingFromDifferenceBetweenPreviousCarryingAmountAndFairValueOfFinancialAssetsReclassifiedAsMeasuredAtFairValue', N'Gains (losses) arising from difference between previous amortised cost and fair value of financial assets reclassified out of amortised cost into fair value through profit or loss measurement category',N'The gains (losses) arising from the difference between the previous amortised cost and the fair value of financial assets reclassified out of the amortised cost into the fair value through profit or loss measurement category. [Refer: At fair value [member]; Financial assets at amortised cost]')
INSERT INTO @AT VALUES(160,'/2/13/', NULL,N'CumulativeGainLossPreviouslyRecognisedInOtherComprehensiveIncomeArisingFromReclassificationOfFinancialAssetsOutOfFairValueThroughOtherComprehensiveIncomeIntoFairValueThroughProfitOrLossMeasurementCategory', N'Cumulative gain (loss) previously recognised in other comprehensive income arising from reclassification of financial assets out of fair value through other comprehensive income into fair value through profit or loss measurement category',N'The cumulative gain (loss) previously recognised in other comprehensive income arising from the reclassification of financial assets out of the fair value through other comprehensive income into the fair value through profit or loss measurement category. [Refer: Financial assets measured at fair value through other comprehensive income; Financial assets at fair value through profit or loss; Other comprehensive income]')
INSERT INTO @AT VALUES(161,'/2/14/', NULL,N'HedgingGainsLossesForHedgeOfGroupOfItemsWithOffsettingRiskPositions', N'Hedging gains (losses) for hedge of group of items with offsetting risk positions',N'The hedging gains (losses) for hedge of group of items with offsetting risk positions.')
INSERT INTO @AT VALUES(162,'/2/15/', NULL,N'IncomeTaxExpenseContinuingOperations', N'Tax income (expense)',N'The aggregate amount included in the determination of profit (loss) for the period in respect of current tax and deferred tax. [Refer: Current tax expense (income); Deferred tax expense (income)]')

INSERT INTO @AccountTypes ([Index], [Code], [Name], [ParentIndex], 
		[EntryTypeParentId], [Description])
SELECT RC.[Index], RC.[Code], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex,
		(SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = RC.EntryTypeParentCode), [Description]
FROM @AT RC;
UPDATE @AccountTypes SET IsAssignable = 1
WHERE [Index] NOT IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)
UPDATE @AccountTypes SET IsAssignable = 0
WHERE [Index] IN (SELECT [ParentIndex] FROM @AccountTypes WHERE [ParentIndex] IS NOT NULL)

DECLARE @CurrentCarLoanReceivablesExtension INT, @PartnersWithdrawalExtension INT;


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

DECLARE @IntangibleAssetsOtherThanGoodwill INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IntangibleAssetsOtherThanGoodwill');

DECLARE @TradeAndOtherNonCurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherNonCurrentReceivables');
DECLARE @OtherNoncurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherNoncurrentReceivables');

DECLARE @TradeAndOtherCurrentReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherCurrentReceivables');
DECLARE @CurrentValueAddedTaxReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentValueAddedTaxReceivables'); 
DECLARE @CurrentTradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentTradeReceivables');
DECLARE @CurrentPrepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentPrepayments');
DECLARE @CurrentAccruedIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentAccruedIncome');

DECLARE @Inventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Inventories');
DECLARE @Merchandise INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Merchandise');
DECLARE @CurrentInventoriesInTransit INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentInventoriesInTransit');
DECLARE @OtherInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherInventories');

DECLARE @CashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashAndCashEquivalents');
DECLARE @CashOnHand INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashOnHand');
DECLARE @BalancesWithBanks INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'BalancesWithBanks');


DECLARE @IssuedCapital INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IssuedCapital'); 
DECLARE @RetainedEarnings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RetainedEarnings');
DECLARE @OtherEquityInterest INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherEquityInterest');

DECLARE @TradeAndOtherCurrentPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherCurrentPayables'); 
DECLARE @TradeAndOtherCurrentPayablesToTradeSuppliers INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherCurrentPayablesToTradeSuppliers'); 
DECLARE @AccrualsClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AccrualsClassifiedAsCurrent'); 
DECLARE @DeferredIncomeClassifiedAsCurrent INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'DeferredIncomeClassifiedAsCurrent'); 

DECLARE @CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentPayablesOnSocialSecurityAndTaxesOtherThanIncomeTax'); 
DECLARE @CurrentSocialSecurityPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentSocialSecurityPayablesExtension'); 
DECLARE @CurrentValueAddedTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentValueAddedTaxPayables'); 
DECLARE @CurrentZakatPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentZakatPayablesExtension'); 
DECLARE @CurrentEmployeeIncomeTaxPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentEmployeeIncomeTaxPayablesExtension'); 
DECLARE @CurrentEmployeeStampTaxPayablesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentEmployeeStampTaxPayablesExtension'); 
DECLARE @CurrentPayablesToEmployeesExtension INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentPayablesToEmployeesExtension'); 

DECLARE @Revenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Revenue');
DECLARE @RevenueFromRenderingOfServices INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RevenueFromRenderingOfServices');
DECLARE @OtherRevenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherRevenue');
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