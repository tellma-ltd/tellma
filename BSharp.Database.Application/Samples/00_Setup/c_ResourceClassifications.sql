IF NOT EXISTS(SELECT * FROM dbo.ResourceDefinitions WHERE [Id] = N'Basic')
INSERT INTO dbo.ResourceDefinitions (
	[Id],	[TitlePlural],	[TitleSingular]) VALUES
(N'Basic',	N'Items',		N'Item');
	
DECLARE @ResourceClassificationsTemp TABLE ([Code] NVARCHAR(255), [Name] NVARCHAR(255), [Node] HIERARCHYID, [IsAssignable] BIT, [Index] INT)
INSERT INTO @ResourceClassificationsTemp
([Code],									[Name],											[Node],		[IsAssignable], [Index]) VALUES
-- Non financial resources
(N'NonFinancialAssets',						N'Non financial assets',						N'/1/',			0,0),
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
(N'IntangibleAssetsOtherThanGoodwill',		N'Intangible assets other than goodwill',		N'/1/3/',		1,25),
(N'BiologicalAssets',						N'Biological assets',							N'/1/10/',		1,26),
(N'Inventories',							N'Inventories',									N'/1/11/',		0,27),
	(N'CurrentRawMaterialsAndCurrentProductionSupplies',
											N'Raw materials and Production supplies',		N'/1/11/1/',	0,28),
		(N'RawMaterials',					N'Raw materials',								N'/1/11/1/1/',	1,29),
		(N'ProductionSupplies',				N'Production supplies',							N'/1/11/1/2/',	1,30),
	(N'Merchandise',						N'Merchandise',									N'/1/11/2/',	1,31),
	(N'CurrentFoodAndBeverage',				N'Food and beverage',							N'/1/11/3/',	1,32),
	(N'CurrentAgriculturalProduce',			N'Agricultural produce',						N'/1/11/4/',	1,33),
	(N'FinishedGoods',						N'Finished goods',								N'/1/11/5/',	1,34),
	(N'SpareParts',							N'Spare parts',									N'/1/11/6/',	1,35),
	(N'CurrentFuel',						N'Fuel',										N'/1/11/7/',	1,36),
-- Financial resources
(N'FinancialAssets',						N'Financial assets',							N'/2/',			0,37),
(N'CashAndCashEquivalents',					N'Cash and cash equivalents',					N'/2/1/',		0,38),
	(N'Cash',								N'Cash',										N'/2/1/1/',		1,39),
	(N'CashEquivalents',					N'Cash equivalents',							N'/2/1/2/',		1,40),
-- Consumables and services
(N'ExpenseByNature',						N'Expenses, by nature',							N'/3/',			0,41),
( N'RawMaterialsAndConsumablesUsed',		N'Raw materials and consumables used',			N'/3/1/',		1,42),
(N'CostOfMerchandiseSold',					N'Cost of merchandise sold',					N'/3/2/',		1,43),
(N'CostOfPurchasedEnergySold',				N'Cost of purchased energy sold',				N'/3/2/1/',		1,44),

(N'ServicesExpense',						N'Services',									N'/3/3/',		1,45),
(N'InsuranceExpense',						N'Insurance',									N'/3/3/1/',		1,46),
(N'ProfessionalFeesExpense',				N'Professional services',						N'/3/3/2/',		1,47),
(N'TransportationExpense',					N'Transportation',								N'/3/3/3/',		1,48),
(N'BankAndSimilarCharges',					N'Bank services',								N'/3/3/4/',		1,49),
(N'EnergyTransmissionCharges',				N'Energy transmission',							N'/3/3/5/',		1,50),
(N'TravelExpense',							N'Travel',										N'/3/3/6/',		1,51),
(N'CommunicationExpense',					N'Communication',								N'/3/3/7/',		1,52),
(N'UtilitiesExpense',						N'Utilities',									N'/3/3/8/',		1,53),
(N'AdvertisingExpense',						N'Advertising',									N'/3/3/9/',		1,54),

(N'EmployeeBenefitsExpense',				N'Employee benefits expense',					N'/3/4/',		0,55),
(N'ShorttermEmployeeBenefitsExpense',		N'Short-term employee benefits expense',		N'/3/4/1/',		0,56),
(N'WagesAndSalaries',						N'Wages and salaries',							N'/3/4/1/1/',	1,57),
(N'SocialSecurityContributions',			N'Social security contributions',				N'/3/4/1/2/',	1,58),
(N'OtherShorttermEmployeeBenefits',			N'Other short-term employee benefits',			N'/3/4/1/3/',	1,59),
(N'PostemploymentBenefitExpenseDefinedContributionPlans',
											N'Post-employment benefit expense, defined contribution plans',
																							N'/3/4/2/',		1,60),																				

(N'DepreciationAmortisationAndImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
											N'Depreciation, amortisation and impairment loss (reversal of impairment loss) recognised in profit or loss',
																							N'/3/5/',		0,61),
(N'DepreciationAndAmortisationExpense',		N'Depreciation and amortisation expense',		N'/3/5/1/',		0,62),
(N'DepreciationExpense',					N'Depreciation expense',						N'/3/5/1/1/',	1,63),
(N'AmortisationExpense',					N'Amortisation expense',						N'/3/5/1/2/',	1,64),
(N'ImpairmentLossReversalOfImpairmentLossRecognisedInProfitOrLoss',
											N'Impairment loss (reversal of impairment loss) recognised in profit or loss',
																							N'/3/6/',		1,65),
(N'TaxExpenseOtherThanIncomeTaxExpense',	N'Tax expense other than income tax expense',	N'/3/7/',		1,66),
(N'PropertyTaxExpense',						N'Property tax expense',						N'/3/7/1/',		1,67),
(N'OtherExpenseByNature',					N'Other expenses, by nature',					N'/3/8/',		1,68);
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


DECLARE @ResourceClassifications dbo.ResourceClassificationList

INSERT INTO @ResourceClassifications ([Code], [Name], [ParentIndex], [IsAssignable], [Index])
SELECT [Code], [Name], (SELECT [Index] FROM @ResourceClassificationsTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [IsAssignable], [Index]
FROM @ResourceClassificationsTemp RC
				
EXEC [api].[ResourceClassifications__Save]
	@Entities = @ResourceClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Classifications: Inserting'
	GOTO Err_Label;
END;																					

IF @DebugResourceClassifications = 1
	SELECT
		RC.Id,
		SPACE(5 * (RC.[Node].GetLevel() - 1)) +  RC.[Name] As [Name],
		RC.[Node].ToString() As [Node],
		RC.[ResourceDefinitionId],
		RC.[IsAssignable],
		RC.[IsActive],
		(SELECT COUNT(*) FROM ResourceClassifications WHERE [ParentNode] = RC.[Node]) AS [ChildCount]
	FROM dbo.ResourceClassifications RC