IF @DB = N'101' -- Banan SD, USD, en
BEGIN
/*
Entry Type - Account Type - Center - Currency - Contract Definition - Agent
*/
		
	

/*
-- 5: Direct, Cost of sales
-- 6: Indirect, Production, 7:service
-- 8:Distribution
-- 9:Admin. Nature: 2 digits, Center: 2 digits, Varieties: 1

	(101,N'90510',	@ProfessionalFeesExpense,		N'Acc. & Legal Services - USD',	@USD,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(102,N'90511',	@ProfessionalFeesExpense,		N'Acc. & Legal Services - SDG',	@SDG,			@C101_EXEC,		@AdministrativeExpense,	NULL),

	(110,N'X0611',	@TransportationExpense,			N'Transportation',			NULL,			NULL,			NULL,					NULL),
	(115,N'90710',	@BankAndSimilarCharges,			N'Banking Services',		NULL,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(120,N'X0811',	@TravelExpense,					N'Visa & Travel',			NULL,			NULL,			NULL,					NULL),
	(135,N'50511',	@ServicesExpense,				N'Cloud Hosting',			@USD,			NULL,			@CostOfSales,			NULL),
	
	(140,N'A1000',	@UtilitiesExpense,				N'Utilities',				@SDG,			@C101_UNALLOC,	@OtherExpenseByFunction,NULL),
	(145,N'A9900',	@ServicesExpense,				N'Office Rental',			@SDG,			@C101_UNALLOC,	@OtherExpenseByFunction,NULL),

	(150,N'81120',	@AdvertisingExpense,			N'Marketing Service - SDG',		NULL,			@C101_Sales,	@DistributionCosts,		NULL),
	(151,N'81120',	@AdvertisingExpense,			N'Marketing Service - SDG',		NULL,			@C101_Sales,	@DistributionCosts,		NULL),
	
	(165,N'X05113',	@ServicesExpense,				N'Medical',					NULL,			NULL,			NULL,					NULL),

	(170,N'92310',	@WagesAndSalaries,		N'Salaries - Exec Office Equip.',	@USD,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(171,N'82320',	@WagesAndSalaries,		N'Salaries - Sales Equip.',			@USD,			@C101_Sales,	@DistributionCosts,		NULL),
	(172,N'72330',	@WagesAndSalaries,		N'Salaries - Sys Admin Equip.',		@USD,			@C101_Sys,		@ServiceExtension,		NULL),
	(173,N'52340',	@WagesAndSalaries,		N'Salaries - B10/HCM Equip.',		@USD,			@C101_B10,		@CostOfSales,			NULL),
	(174,N'52350',	@WagesAndSalaries,		N'Salaries - BSmart Equip.',		@USD,			@C101_BSmart,	@CostOfSales,			NULL),
	(175,N'52360',	@WagesAndSalaries,		N'Salaries - Campus Equip.',		@USD,			@C101_Campus,	@CostOfSales,			NULL),
	(176,N'52370',	@WagesAndSalaries,		N'Salaries - Tellma Equip.',		@USD,			@C101_Tellma,	@CostOfSales,			NULL),
	(177,N'52380',	@WagesAndSalaries,		N'Salaries - Floor Rental Equip.',	@USD,			@C101_FFLR,		@CostOfSales,			NULL),

	(178,N'X1201',	@EmployeeBenefitsExpense,		N'Zakat & Eid',				@USD,			NULL,			NULL,					NULL),

	(180,N'50550',	@ProfessionalFeesExpense,		N'Salaries - B10 Contractors',@USD,			@C101_B10,		@CostOfSales,			NULL),
	(185,N'X1500',	@SocialSecurityContributions,	N'Employee Pension Contribution',@SDG,		NULL,			NULL,					NULL),

	(200,N'X99002',	@OtherExpenseByNature,			N'Stationery & Grocery',	NULL,			NULL,			NULL,					NULL),

	(205,N'99910',	@OtherExpenseByNature,			N'Gov fees',				@SDG,			@C101_EXEC,		@AdministrativeExpense,	NULL),

	(210,N'89920',	@OtherExpenseByNature,			N'Presentation tools',		NULL,			@C101_Sales,	@DistributionCosts,		NULL),
	(215,N'X99003',	@OtherExpenseByNature,			N'Education & Certifications',NULL,			NULL,			NULL,					NULL),
	(220,N'X99004',	@OtherExpenseByNature,			N'Consumables',				NULL,			NULL,			NULL,					NULL),
	(225,N'X99005',	@OtherExpenseByNature,			N'Tender Fees',				NULL,			NULL,			NULL,					NULL),
	(230,N'X99006',	@OtherExpenseByNature,			N'Office Furniture',		NULL,			NULL,			NULL,					NULL),
	(235,N'X99007',	@OtherExpenseByNature,			N'Other Expenses',			NULL,			NULL,			NULL,					NULL),

	(241,N'92311',	@DepreciationExpense,	N'Dep. Exp. - Exec Office Equip.',	@USD,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(242,N'82321',	@DepreciationExpense,	N'Dep. Exp. - Sales Equip.',		@USD,			@C101_Sales,	@DistributionCosts,		NULL),
	(243,N'72331',	@DepreciationExpense,	N'Dep. Exp. - Sys Admin Equip.',	@USD,			@C101_Sys,		@ServiceExtension,		NULL),
	(244,N'52341',	@DepreciationExpense,	N'Dep. Exp. - B10/HCM Equip.',		@USD,			@C101_B10,		@CostOfSales,			NULL),
	(245,N'52351',	@DepreciationExpense,	N'Dep. Exp. - BSmart Equip.',		@USD,			@C101_BSmart,	@CostOfSales,			NULL),
	(246,N'52361',	@DepreciationExpense,	N'Dep. Exp. - Campus Equip.',		@USD,			@C101_Campus,	@CostOfSales,			NULL),
	(247,N'52371',	@DepreciationExpense,	N'Dep. Exp. - Tellma Equip.',		@USD,			@C101_Tellma,	@CostOfSales,			NULL),
	(248,N'52381',	@DepreciationExpense,	N'Dep. Exp. - Floor Rental Equip.',	@USD,			@C101_FFLR,		@CostOfSales,			NULL),

	(250,N'B01',	@GainLossOnDisposalOfPropertyPlantAndEquipment,
														N'Gain (loss) on disposal',	@USD,			@C101_INV,			NULL,					NULL);

-- 5: Direct, Cost of sales
-- 6: Indirect, Production, 7:service
-- 8:Distribution
-- 9:Admin. Nature: 2 digits, Center: 2 digits, Varieties: 1

	-- Expenses
	(69,	@ProfessionalFeesExpense,	N'Accounting Services',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(83,	@ProfessionalFeesExpense,	N'Legal Services',		NULL,			@C101_INV,					NULL,			NULL,	NULL),

	(62,	@TransportationExpense,		N'Transportation',		NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(82,	@BankAndSimilarCharges,		N'Banking Services',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(80,	@TravelExpense,				N'Travel',				NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(81,	@TravelExpense,				N'Visa',				NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(60,	@UtilitiesExpense,			N'Cloud Hosting',		NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(72,	@UtilitiesExpense,			N'Office Rental',		NULL,			@C101_INV,					NULL,			NULL,	NULL),

	(64,	@AdvertisingExpense,		N'Marketing Service',	NULL,			@C101_INV,					@DistributionCosts,NULL,	NULL),

	(65,@WagesAndSalaries,		N'Salaries',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(79,@WagesAndSalaries,		N'Zakat & Eid',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(66,@WagesAndSalaries,		N'Contractors Salaries',			@C101_INV,					NULL,			NULL,	NULL),
	(70,@SocialSecurityContributions,N'Employee Pension Contribution',@C101_INV,					NULL,			NULL,	NULL),
	(88,@EmployeeBenefitsExpense,	N'Allowances & Bonuses',			@C101_INV,					NULL,			NULL,	NULL),
	
	
	(71,@OtherExpenseByNature,	N'Stationery & Grocery',			@C101_INV,					NULL,			NULL,	NULL),

	(74,@OtherExpenseByNature,	N'Gov fees',			NULL,			@C101_INV,					NULL,			NULL,	NULL),

	(77,@OtherExpenseByNature,	N'Maintenance',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(78,@OtherExpenseByNature,	N'Medical',				NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(84,@OtherExpenseByNature,	N'Presentation tools',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(85,@OtherExpenseByNature,	N'Education & Certifications',		@C101_INV,					NULL,			NULL,	NULL),
	(86,@OtherExpenseByNature,	N'Consumables',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(87,@OtherExpenseByNature,	N'Tender Fees',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(89,@OtherExpenseByNature,	N'Office Furniture',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(90,@OtherExpenseByNature,	N'Other Expenses',		NULL,			@C101_INV,					NULL,			NULL,	NULL),

	(98,@OtherExpenseByNature,	N'Depreciation',		NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(99,@OtherExpenseByNature,	N'Gain (loss) on disposal',	NULL,		@C101_INV,					NULL,			NULL,	NULL);
*/

	EXEC [api].[Accounts__Save]
		@Entities = @Accounts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Accounts: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @1GMFund INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'GM Fund');
	DECLARE @1AdminPC INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Admin Fund - SDG');
	DECLARE @1KSAFund INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'KSA Fund');
	DECLARE @1BOK INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Bank Of Khartoum - SDG');
	DECLARE @1Meals INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employee Meals');
	DECLARE @1Education INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Education & Certifications');
	DECLARE @1MAPayable INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Borrowings from M/A');
	DECLARE @1DomainRegistration INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Domain Registration');
	DECLARE @1Maintenance INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Maintenance');
	DECLARE @1Electricity INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Electricity');
	DECLARE @1Internet INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Internet & Tel');
	DECLARE @1EITax INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employees Income Tax');
	DECLARE @1EStax INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employees Stamp Tax');
	DECLARE @1AR INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Trade Receivables');
	DECLARE @1Revenues INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Revenues');
	DECLARE @1SubscriptionRevenues INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Subscription Revenues');
	DECLARE @1RentalIncome INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Rental Income - SAR');


	DECLARE @1DocumentControl INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Document Control');
	DECLARE @1VATInput INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'VAT Input');
	DECLARE @1VATOutput INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'VAT Output');

	DECLARE @1RetainedSalaries INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'10% Retained Salaries');
	DECLARE @1Bonuses INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Bonuses');
	DECLARE @1Termination INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Termination Benefits');
	DECLARE @1CashSuppliers INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Cash Suppliers');
	DECLARE @1CashCustomers INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Cash Customers');
	DECLARE @1ExchangeGainLoss INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Exchange Loss (Gain)');
	DECLARE @1ExchangeVariance INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Exchange Variance');
	DECLARE @1TradeReceivables INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Trade Receivables');
END