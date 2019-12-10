DECLARE @ResourceClassificationsTemp TABLE ([Code] NVARCHAR(255), [Name] NVARCHAR(255), [Node] HIERARCHYID, [IsAssignable] BIT, [Index] INT)
INSERT INTO @ResourceClassificationsTemp
([Code],									[Name],											[Node],		[IsAssignable], [Index]) VALUES
-- Non financial resources
(N'NonFinancialAssets',							N'Non financial assets',						N'/1/',			0,0),
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
	(N'Inventories',							N'Inventories',									N'/1/11/',		0,28),
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
(N'CashAndCashEquivalents',						N'Cash and cash equivalents',					N'/3/',			0,39),
	(N'Cash',									N'Cash',										N'/3/1/',		1,40),
	(N'CashEquivalents',						N'Cash equivalents',							N'/3/2/',		1,41),
-- Equitiy and Liabilities and 
(N'Equity',										N'Expenses, by nature',							N'/4/',			0,42),
(N'OtherLongtermProvisions',					N'Other non-current provisions',				N'/5/',			0,43),
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

DECLARE @ResourceClassifications dbo.ResourceClassificationList

INSERT INTO @ResourceClassifications ([Code], [Name], [ParentIndex], [IsAssignable], [Index])
SELECT [Code], [Name], (SELECT [Index] FROM @ResourceClassificationsTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [IsAssignable], [Index]
FROM @ResourceClassificationsTemp RC
UPDATE @ResourceClassifications SET ResourceDefinitionId = N'currencies' WHERE [Code] = N'Cash';

--select * from 	@ResourceClassifications;			
EXEC [api].[ResourceClassifications__Save]
	@Entities = @ResourceClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

UPDATE dbo.ResourceClassifications SET IsSystem = 1;

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