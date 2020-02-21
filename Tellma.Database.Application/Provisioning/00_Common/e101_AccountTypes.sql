DECLARE @AccountTypes dbo.AccountTypeList;
DECLARE @AT TABLE (
	[Index] INT,[IsResourceClassification] BIT, [IsCurrent] BIT, [IsActive] BIT, [IsAssignable] BIT, [IsReal] BIT, [IsPersonal] BIT, [IsSystem] BIT,
	[Node] HIERARCHYID, [EntryTypeParentCode] NVARCHAR (255), [Code] NVARCHAR (255), [Name] NVARCHAR (512), [Description] NVARCHAR (MAX)
)
IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @AT VALUES(0,0,NULL, 1,0,0,0,0,'/1/', NULL,N'StatementOfFinancialPositionAbstract', N'Statement of financial position [abstract]',N'')
	INSERT INTO @AT VALUES(1,0,NULL, 1,0,0,0,0,'/1/1/', NULL,N'AssetsAbstract', N'Assets [abstract]',N'')
	INSERT INTO @AT VALUES(2,1,0, 1,1,1,0,1,'/1/1/1/', N'ChangesInPropertyPlantAndEquipment',N'PropertyPlantAndEquipment', N'Property, plant and equipment',N'The amount of tangible assets that: (a) are held for use in the production or supply of goods or services, for rental to others, or for administrative purposes; and (b) are expected to be used during more than one period.')
	INSERT INTO @AT VALUES(11,1,0, 1,1,1,0,0,'/1/1/1/4/', N'ChangesInPropertyPlantAndEquipment',N'FixturesAndFittings', N'Fixtures and fittings',N'The amount of fixtures and fittings, not permanently attached to real property, used in the entity''s operations.')
	INSERT INTO @AT VALUES(12,1,0, 1,1,1,0,0,'/1/1/1/5/', N'ChangesInPropertyPlantAndEquipment',N'OfficeEquipment', N'Office equipment',N'The amount of property, plant and equipment representing equipment used to support office functions, not specifically used in the production process. [Refer: Property, plant and equipment]')
	INSERT INTO @AT VALUES(13,1,0, 1,1,1,0,0,'/1/1/1/5/1/', N'ChangesInPropertyPlantAndEquipment',N'ComputerEquipmentMemberExtension', N'Computer equipment',N'The amount of property, plant and equipment representing computer accessories. [Refer: Property, plant and equipment]')
	INSERT INTO @AT VALUES(14,1,0, 1,1,1,0,0,'/1/1/1/5/2/', N'ChangesInPropertyPlantAndEquipment',N'ComputerAccessoriesExtension', N'Computer accessories',N'The amount of property, plant and equipment representing computer equipment. [Refer: Property, plant and equipment]')
	INSERT INTO @AT VALUES(21,1,0, 1,1,1,0,0,'/1/1/1/12/', N'ChangesInPropertyPlantAndEquipment',N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment',N'The amount of property, plant and equipment that the entity does not separately disclose in the same statement or note. [Refer: Property, plant and equipment]')
	INSERT INTO @AT VALUES(26,1,0, 1,1,1,0,1,'/1/1/4/', N'ChangesInIntangibleAssetsOtherThanGoodwill',N'IntangibleAssetsOtherThanGoodwill', N'Intangible assets other than goodwill',N'The amount of identifiable non-monetary assets without physical substance. This amount does not include goodwill. [Refer: Goodwill]')
	INSERT INTO @AT VALUES(36,0,NULL, 1,1,1,1,0,'/1/1/5/', NULL,N'OtherFinancialAssets', N'Other financial assets',N'The amount of financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
	INSERT INTO @AT VALUES(37,0,NULL, 1,1,1,1,0,'/1/1/6/', NULL,N'OtherNonfinancialAssets', N'Other non-financial assets',N'The amount of non-financial assets that the entity does not separately disclose in the same statement or note. [Refer: Financial assets]')
	INSERT INTO @AT VALUES(63,0,NULL, 1,0,0,1,0,'/1/1/14/', NULL,N'TradeAndOtherReceivables', N'Trade and other receivables',N'The amount of trade receivables and other receivables. [Refer: Trade receivables; Other receivables]')
	INSERT INTO @AT VALUES(71,0,1, 1,0,0,1,0,'/1/1/15/', N'IncreaseDecreaseInCashAndCashEquivalents',N'CashAndCashEquivalents', N'Cash and cash equivalents',N'The amount of cash on hand and demand deposits, along with short-term, highly liquid investments that are readily convertible to known amounts of cash and that are subject to an insignificant risk of changes in value. [Refer: Cash; Cash equivalents]')
	INSERT INTO @AT VALUES(84,0,NULL, 1,0,0,0,0,'/1/2/', NULL,N'EquityAndLiabilitiesAbstract', N'Equity and liabilities [abstract]',N'')
	INSERT INTO @AT VALUES(85,0,0, 1,0,0,0,0,'/1/2/1/', N'ChangesInEquity',N'EquityAbstract', N'Equity [abstract]',N'')
	INSERT INTO @AT VALUES(86,0,0, 1,1,0,0,0,'/1/2/1/1/', N'ChangesInEquity',N'IssuedCapital', N'Issued capital',N'The nominal value of capital issued.')
	INSERT INTO @AT VALUES(87,0,0, 1,1,0,0,1,'/1/2/1/2/', N'ChangesInEquity',N'RetainedEarnings', N'Retained earnings',N'A component of equity representing the entity''s cumulative undistributed earnings or deficit.')
	INSERT INTO @AT VALUES(91,0,0, 1,1,0,0,0,'/1/2/1/3/', N'ChangesInEquity',N'OtherReserves', N'Other reserves',N'A component of equity representing reserves within equity, not including retained earnings. [Refer: Retained earnings]')
	INSERT INTO @AT VALUES(108,0,NULL, 1,0,0,0,0,'/1/2/2/', NULL,N'LiabilitiesAbstract', N'Liabilities [abstract]',N'')
	INSERT INTO @AT VALUES(109,0,NULL, 1,1,0,1,0,'/1/2/2/1/', NULL,N'TradeAndOtherPayables', N'Trade and other payables',N'The amount of trade payables and other payables. [Refer: Trade payables; Other payables]')
	INSERT INTO @AT VALUES(122,0,NULL, 1,0,0,0,0,'/1/2/2/2/', NULL,N'ProvisionsAbstract', N'Provisions [abstract]',N'')
	INSERT INTO @AT VALUES(123,0,NULL, 1,1,0,1,0,'/1/2/2/2/1/', NULL,N'ProvisionsForEmployeeBenefits', N'Provisions for employee benefits',N'The amount of provisions for employee benefits. [Refer: Employee benefits expense; Provisions]')
	INSERT INTO @AT VALUES(124,0,NULL, 1,1,0,0,0,'/1/2/2/2/2/', N'ChangesInOtherProvisions',N'OtherProvisions', N'Other provisions',N'The amount of provisions other than provisions for employee benefits. [Refer: Provisions]')
	INSERT INTO @AT VALUES(131,0,NULL, 1,1,0,1,0,'/1/2/2/5/', NULL,N'OtherFinancialLiabilities', N'Other financial liabilities',N'The amount of financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Financial liabilities]')
	INSERT INTO @AT VALUES(132,0,NULL, 1,1,0,1,0,'/1/2/2/6/', NULL,N'OtherNonfinancialLiabilities', N'Other non-financial liabilities',N'The amount of non-financial liabilities that the entity does not separately disclose in the same statement or note. [Refer: Other financial liabilities]')
	INSERT INTO @AT VALUES(136,0,NULL, 1,0,0,0,0,'/2/', NULL,N'IncomeStatementAbstract', N'Profit or loss [abstract]',N'')
	INSERT INTO @AT VALUES(137,0,1, 1,1,1,1,0,'/2/1/', NULL,N'Revenue', N'Revenue',N'The income arising in the course of an entity''s ordinary activities. Income is increases in economic benefits during the accounting period in the form of inflows or enhancements of assets or decreases of liabilities that result in an increase in equity, other than those relating to contributions from equity participants.')
	INSERT INTO @AT VALUES(143,0,1, 0,1,0,0,0,'/2/2/', NULL,N'OtherIncome', N'Other income',N'The amount of operating income that the entity does not separately disclose in the same statement or note.')
	INSERT INTO @AT VALUES(144,0,1, 1,0,0,1,0,'/2/3/', N'ExpenseByFunctionExtension',N'ExpenseByNatureAbstract', N'Expenses by nature [abstract]',N'The amount of acquisition and administration expense relating to insurance contracts. [Refer: Types of insurance contracts [member]]')
	INSERT INTO @AT VALUES(146,0,1, 0,1,1,1,1,'/2/3/2/', N'ExpenseByFunctionExtension',N'CostOfMerchandiseSold', N'Cost of merchandise sold',N'The amount of merchandise that was sold during the period and recognised as an expense.')
	INSERT INTO @AT VALUES(147,1,1, 1,1,1,1,0,'/2/3/3/', N'ExpenseByFunctionExtension',N'ServicesExpense', N'Services expense',N'The amount of expense arising from services.')
	INSERT INTO @AT VALUES(156,1,1, 1,1,0,1,0,'/2/3/4/', N'ExpenseByFunctionExtension',N'EmployeeBenefitsExpense', N'Employee benefits expense',N'The expense of all forms of consideration given by an entity in exchange for a service rendered by employees or for the termination of employment.')
	INSERT INTO @AT VALUES(167,0,1, 1,1,1,1,1,'/2/3/5/', N'ExpenseByFunctionExtension',N'DepreciationExpense', N'Depreciation expense',N'The amount of depreciation expense. Depreciation is the systematic allocation of depreciable amounts of tangible assets over their useful lives.')
	INSERT INTO @AT VALUES(170,0,1, 1,1,0,1,0,'/2/3/7/', N'ExpenseByFunctionExtension',N'OtherExpenseByNature', N'Other expenses',N'The amount of expenses that the entity does not separately disclose in the same statement or note when the entity uses the ''nature of expense'' form for its analysis of expenses. [Refer: Expenses, by nature]')
	INSERT INTO @AT VALUES(171,0,1, 1,1,0,0,0,'/2/4/', NULL,N'OtherGainsLosses', N'Other gains (losses)',N'The gains (losses) that the entity does not separately disclose in the same statement or note.')

	INSERT INTO @AccountTypes ([Index], [Code], [Name], [ParentIndex], 
			[IsAssignable], [IsCurrent],[IsResourceClassification], [IsReal],[IsPersonal],
			[EntryTypeParentId], [Description])
	SELECT RC.[Index], RC.[Code], RC.[Name], (SELECT [Index] FROM @AT WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex,
			[IsAssignable],  [IsCurrent], [IsResourceClassification], [IsReal],[IsPersonal],
			(SELECT [Id] FROM dbo.EntryTypes WHERE [Code] = RC.EntryTypeParentCode), [Description]
	FROM @AT RC;
		
	EXEC [api].[AccountTypes__Save]
		@Entities = @AccountTypes,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	UPDATE dbo.[AccountTypes] SET IsSystem = 1 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsSystem = 1);
	UPDATE dbo.[AccountTypes] SET IsActive = 0 WHERE [Code] IN (SELECT [Code] FROM @AT WHERE IsActive = 0);

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Account Types: Provisioning: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;																					
END

DECLARE @PropertyPlantAndEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'PropertyPlantAndEquipment');
DECLARE @FixturesAndFittings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'FixturesAndFittings');
DECLARE @OfficeEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OfficeEquipment');
DECLARE @ComputerEquipment INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ComputerEquipmentMemberExtension');
DECLARE @ComputerAccessories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ComputerAccessoriesExtension');

DECLARE @TradeAndOtherReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherReceivables');
--DECLARE @ValueAddedTaxReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ValueAddedTaxReceivables'); 
--DECLARE @TradeReceivables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeReceivables');
--DECLARE @Prepayments INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Prepayments');

DECLARE @InventoriesTotal INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'InventoriesTotal');
DECLARE @Merchandise INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Merchandise');
DECLARE @CurrentInventoriesInTransit INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CurrentInventoriesInTransit');
DECLARE @OtherInventories INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherInventories');
DECLARE @CashAndCashEquivalents INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CashAndCashEquivalents');

DECLARE @IssuedCapital INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'IssuedCapital'); 
DECLARE @RetainedEarnings INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RetainedEarnings');

DECLARE @TradeAndOtherPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TradeAndOtherPayables'); 
--DECLARE @SocialSecurityPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'SocialSecurityPayablesExtension'); 
--DECLARE @ValueAddedTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ValueAddedTaxPayables'); 
--DECLARE @ZakatPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ZakatPayablesExtension'); 
--DECLARE @EmployeeIncomeTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeIncomeTaxPayablesExtension'); 
--DECLARE @EmployeeStampTaxPayables INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeStampTaxPayablesExtension'); 

DECLARE @Revenue INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Revenue');
DECLARE @OtherIncome INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherIncome');

DECLARE @RawMaterialsAndConsumablesUsed INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'RawMaterialsAndConsumablesUsed');

DECLARE @ServicesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ServicesExpense');
--DECLARE @ProfessionalFeesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'ProfessionalFeesExpense');
--DECLARE @TransportationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TransportationExpense');
--DECLARE @BankAndSimilarCharges INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'BankAndSimilarCharges');
--DECLARE @TravelExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TravelExpense');
--DECLARE @CommunicationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'CommunicationExpense');
--DECLARE @UtilitiesExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'UtilitiesExpense');
--DECLARE @AdvertisingExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'AdvertisingExpense');
DECLARE @EmployeeBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'EmployeeBenefitsExpense');
--DECLARE @WagesAndSalaries INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'WagesAndSalaries');
--DECLARE @SocialSecurityContributions INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'SocialSecurityContributions');
--DECLARE @OtherShorttermEmployeeBenefits INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherShorttermEmployeeBenefits');

--DECLARE @TerminationBenefitsExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'TerminationBenefitsExpense');
DECLARE @DepreciationExpense INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'DepreciationExpense');

DECLARE @OtherExpenseByNature INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'OtherExpenseByNature');
/*
PostemploymentBenefitExpenseDefinedContributionPlans
PostemploymentBenefitExpenseDefinedBenefitPlans

OtherLongtermBenefits
OtherEmployeeExpense
*/