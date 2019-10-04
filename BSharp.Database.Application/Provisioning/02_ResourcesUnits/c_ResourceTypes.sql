DECLARE @ResourceTypes AS TABLE (
	[Id]					NVARCHAR (255)		PRIMARY KEY NONCLUSTERED,
	[Name]					NVARCHAR (255)		NOT NULL,
	[Name2]					NVARCHAR (255),
	[Name3]					NVARCHAR (255),
	[IsAssignable]			BIT					NOT NULL DEFAULT 1,
	[IsActive]				BIT					NOT NULL DEFAULT 1,
	[Node]					HIERARCHYID			NOT NULL
);
INSERT INTO @ResourceTypes
([Id],										[Name],											[Node],			[IsAssignable], [IsActive]) VALUES
(N'Assets',									N'Assets',										N'/1/',			0,				1),
(N'PropertyPlantAndEquipment',				N'Property, plant and equipment',				N'/1/1/',		1,				1),
(N'LandAndBuildings',						N'Land and buildings',							N'/1/1/1/',		1,				1),
(N'Land',									N'Land',										N'/1/1/1/1/',	1,				1),
(N'Buildings',								N'Buildings',									N'/1/1/1/2/',	1,				1),
(N'Machinery',								N'Machinery',									N'/1/1/2/',		1,				1),
(N'Vehicles',								N'Vehicles',									N'/1/1/3/',		1,				1),
(N'Ships',									N'Ships',										N'/1/1/3/1/',	1,				0),
(N'Aircraft',								N'Aircraft',									N'/1/1/3/2/',	1,				0),
(N'MotorVehicles',							N'Motor vehicles',								N'/1/1/3/3/',	1,				1),
(N'FixturesAndFittings',					N'Fixtures and fittings',						N'/1/1/4/',		1,				1),
(N'OfficeEquipment',						N'Office equipment',							N'/1/1/5/',		1,				1),
(N'ComputerEquipment',						N'Computer equipment',							N'/1/1/6/',		1,				1),
(N'CommunicationAndNetworkEquipment',		N'Communication and network equipment',			N'/1/1/7/',		1,				1),
(N'NetworkInfrastructure',					N'Network infrastructure',						N'/1/1/8/',		1,				1),
(N'BearerPlants',							N'Bearer plants',								N'/1/1/9/',		1,				0),
(N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets',	N'/1/1/10/',	1,				0),
(N'MiningAssets',							N'Mining assets',								N'/1/1/11/',	1,				0),
(N'MiningProperty',							N'Mining property',								N'/1/1/11/1/',	1,				0),
(N'OilAndGasAssets',						N'Oil and gas assets',							N'/1/1/12/',	1,				0),
(N'PowerGeneratingAssets',					N'Power generating assets',						N'/1/1/13/',	1,				1),
(N'LeaseholdImprovements',					N'Leasehold improvements',						N'/1/1/14/',	1,				1),
(N'ConstructionInProgress',					N'Construction in progress',					N'/1/1/15/',	1,				1),
(N'OtherPropertyPlantAndEquipment',			N'Other property, plant and equipment',			N'/1/1/16/',	1,				1),

(N'InvestmentProperty',						N'Investment property',							N'/1/2/',		1,				0),
(N'IntangibleAssetsOtherThanGoodwill',		N'Intangible assets other than goodwill',		N'/1/3/',		1,				1),
(N'FinancialAssets',						N'Financial assets',							N'/1/4/',		1,				0),
(N'FinancialAssetsAtFairValueThroughProfitOrLoss',N'Financial assets at fair value through profit or loss',
																							N'/1/4/1/',		1,				0),
(N'FinancialAssetsAvailableforsale',		N'Financial assets available-for-sale',			N'/1/4/2/',		1,				0),
(N'HeldtomaturityInvestments',				N'Held-to-maturity investments',				N'/1/4/3/',		1,				1),
(N'LoansAndReceivables',					N'Loans and receivables',						N'/1/4/4/',		1,				1),
(N'FinancialAssetsAtAmortisedCost',			N'Financial assets at amortised cost',			N'/1/4/5/',		1,				0),
(N'FinancialAssetsAtFairValueThroughOtherComprehensiveIncome',
											N'Financial assets at fair value through other comprehensive income',
																							N'/1/4/6/',		1,				0),

(N'OtherNonfinancialAssets',				N'Other non-financial assets',					N'/1/5/',		1,				0),
-- TODO: The following may actually fit under financial assets
(N'InsuranceContractsIssuedThatAreAssets',	N'Insurance contracts issued that are assets',	N'/1/6/',		1,				0),
(N'ReinsuranceContractsHeldThatAreAssets',	N'Reinsurance contracts held that are assets',	N'/1/7/',		1,				0),
(N'InvestmentAccountedForUsingEquityMethod', N'Investments accounted for using equity method',
																							N'/1/8/',		1,				0),
(N'InvestmentsInSubsidiariesJointVenturesAndAssociates', N'Investments in subsidiaries, joint ventures and associates',
																							N'/1/9/',		1,				0),
---
(N'BiologicalAssets',						N'Biological assets',							N'/1/10/',		1,				0),
(N'Inventories',							N'Inventories',									N'/1/11/',		1,				1),
(N'CurrentRawMaterialsAndCurrentProductionSupplies',
											N'Raw materials and Production supplies',		N'/1/11/1/',	1,				1),
(N'RawMaterials',							N'Raw materials',								N'/1/11/1/1/',	1,				1),
(N'ProductionSupplies',						N'Production supplies',							N'/1/11/1/2/',	1,				1),
(N'Merchandise',							N'Merchandise',									N'/1/11/2/',	1,				1),
(N'CurrentFoodAndBeverage',					N'Food and beverage',							N'/1/11/3/',	1,				0),
(N'CurrentAgriculturalProduce',				N'Agricultural produce',						N'/1/11/4/',	1,				0),
(N'FinishedGoods',							N'Finished goods',								N'/1/11/5/',	1,				1),
(N'SpareParts',								N'Spare parts',									N'/1/11/6/',	1,				0),
(N'CurrentFuel',							N'Fuel',										N'/1/11/7/',	1,				0),

(N'TradeAndOtherReceivables',				N'Trade and other receivables',					N'/1/12/',		1,				1),
(N'Cash',									N'Cash',										N'/1/13/',		1,				1),
(N'Liabilities',							N'Liabilities',									N'/2/',			0,				1),
(N'TradeAndOtherPayables',					N'Trade and other payables',					N'/2/1/',		1,				1),
(N'FinancialLiabilities',					N'Financial liabilities',						N'/2/2/',		1,				0),
(N'FinancialLiabilitiesAtFairValueThroughProfitOrLoss',
											N'Financial liabilities at fair value through profit or loss',
																							N'/2/2/1/',		1,				0),
(N'FinancialLiabilitiesAtAmortisedCost',	N'Financial liabilities at amortised cost',		N'/2/2/2/',		1,				0),

(N'OtherNonfinancialLiabilities',			N'Other non-financial liabilities',				N'/2/3/',		1,				0),
-- TODO: The following may actually fit under financial liabilities
(N'InsuranceContractsIssuedThatAreLiabilities',	N'Insurance contracts issued that are liabilities',	N'/2/4/',1,				0),
(N'ReinsuranceContractsHeldThatAreLiabilities',	N'Reinsurance contracts held that are liabilities',	N'/2/5/',1,				0),

(N'ExpenseByNature',						N'Expenses, by nature',							N'/3/',			0,				1),
(N'ServicesExpense',						N'Services',									N'/3/1/',		1,				1),
(N'InsuranceExpense',						N'Insurance',									N'/3/1/1/',		1,				1),
(N'ProfessionalFeesExpense',				N'Professional services',						N'/3/1/2/',		1,				1),
(N'TransportationExpense',					N'Transportation',								N'/3/1/3/',		1,				1),
(N'BankAndSimilarCharges',					N'Bank services',								N'/3/1/4/',		1,				1),
(N'EnergyTransmissionCharges',				N'Energy transmission',							N'/3/1/5/',		1,				1),
(N'TravelExpense',							N'Travel',										N'/3/1/6/',		1,				1),
(N'CommunicationExpense',					N'Communication',								N'/3/1/7/',		1,				1),
(N'UtilitiesExpense',						N'Utilities',									N'/3/1/8/',		1,				1),
(N'AdvertisingExpense',						N'Advertising',									N'/3/1/9/',		1,				1),

(N'EmployeeBenefitsExpense',				N'Employee benefits expense',					N'/3/2/',		1,				1),
(N'ShorttermEmployeeBenefitsExpense',		N'Short-term employee benefits expense',		N'/3/2/1/',		1,				1),
(N'WagesAndSalaries',						N'Wages and salaries',							N'/3/2/1/1/',	1,				1),
(N'SocialSecurityContributions',			N'Social security contributions',				N'/3/2/1/2/',	1,				1),
(N'OtherShorttermEmployeeBenefits',			N'Other short-term employee benefits',			N'/3/2/1/3/',	1,				1),
(N'PostemploymentBenefitExpenseDefinedContributionPlans',
											N'Post-employment benefit expense, defined contribution plans',
																							N'/3/2/2/',		1,				1),																							
(N'OtherExpenseByNature',					N'Other',										N'/3/3/',		1,				1)					
																							;
MERGE [dbo].[ResourceTypes] AS t
USING (
		SELECT [Id], [IsAssignable], [Name], [Name2], [Name3], [IsActive], [Node]
		FROM @ResourceTypes
) AS s
ON s.[Id] = t.[Id]
WHEN MATCHED
THEN
	UPDATE SET
		t.[IsAssignable]	=	s.[IsAssignable],
		t.[Name]			=	s.[Name],
		t.[Name2]			=	s.[Name2],
		t.[Name3]			=	s.[Name3],
		t.[Node]			=	s.[Node]
WHEN NOT MATCHED BY SOURCE THEN
    DELETE -- to delete Ifrs Resource Classifications extension concepts we added erroneously
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Id],	[IsAssignable],		[Name],	[Name2],	[Name3],	[IsActive],	[Node])
    VALUES (s.[Id], s.[IsAssignable], s.[Name], s.[Name2], s.[Name3], s.[IsActive], s.[Node]);