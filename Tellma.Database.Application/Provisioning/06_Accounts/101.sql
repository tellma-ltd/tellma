﻿IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Accounts([Index],[IsCurrent],[HasResource],[SmartKey],--[Code],
					[AccountTypeId],			[Name],						[CurrencyId],	[ResponsibilityCenterId], [EntryTypeId], [AgentDefinitionId],	[AgentId]) VALUES
	-- Assets Accounts
	(0,1,0,N'PPE',	@PropertyPlantAndEquipment,	N'Property, plant and equipment',NULL,		@RC_Inv,					NULL,			N'employees',		NULL),

	(1,1,0,NULL,	@CashAndCashEquivalents,	N'GM Fund',					NULL,			@RC_Inv,					NULL,			N'custodies',		@GMSafe),
	(2,1,0,NULL,	@CashAndCashEquivalents,	N'KSA Fund',				NULL,			@RC_Inv,					NULL,			N'custodies',		@KSASafe),
	(3,1,0,NULL,	@CashAndCashEquivalents,	N'Bank Of Khartoum',		@SDG,			@RC_Inv,					NULL,			N'banks',			@Bank_BOK),

	(4,1,0,N'RCV',	@TradeAndOtherReceivables,	N'Tier-3 A/R - SDG',		@SDG,			@RC_Inv,					NULL,			N't3-customers',	NULL),
	(5,1,0,N'RCV',	@TradeAndOtherReceivables,	N'Tier-3 A/R - USD',		@USD,			@RC_Inv,					NULL,			N't3-customers',	NULL),
	(6,1,0,N'RCV',	@TradeAndOtherReceivables,	N'Tier-2 A/R - USD',		@USD,			@RC_Inv,					NULL,			N't2-customers',	NULL),
	(7,1,0,N'RCV',	@TradeAndOtherReceivables,	N'Tier-2 A/R - SAR',		N'SAR',			@RC_Inv,					NULL,			N't2-customers',	NULL),
	(8,1,0,NULL,	@TradeAndOtherReceivables,	N'Banan ET',				@USD,			@RC_Inv,					NULL,			NULL,				NULL),
	(9,1,0,NULL,	@TradeAndOtherReceivables,	N'PrimeLedgers A/R',		@USD,			@RC_Inv,					NULL,			NULL,				NULL),
	(10,1,0,NULL,	@TradeAndOtherReceivables,	N'Partners Withdrawals',	@USD,			@RC_Inv,					NULL,			N'partners',		NULL),
	(11,1,0,NULL,	@TradeAndOtherReceivables,	N'Abu Ammar Car Loan',		@USD,			@RC_Inv,					NULL,			N'employees',		@Abu_Ammar),
	(12,1,0,NULL,	@TradeAndOtherReceivables,	N'M. Ali Car Loan',			@USD,			@RC_Inv,					NULL,			N'employees',		@M_Ali),
	(13,1,0,NULL,	@TradeAndOtherReceivables,	N'El-Amin Car Loan',		@USD,			@RC_Inv,					NULL,			N'employees',		@el_Amin),
	(14,1,0,N'VAT',	@TradeAndOtherReceivables,	N'VAT Input',				@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(15,1,0,NULL,	@TradeAndOtherReceivables,	N'Commissions',				@USD,			@RC_Inv,					NULL,			NULL,				NULL),
	(16,1,0,NULL,	@TradeAndOtherReceivables,	N'Office Rent',				@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(17,1,0,NULL,	@TradeAndOtherReceivables,	N'Internet Expense',		@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(18,1,0,NULL,	@TradeAndOtherReceivables,	N'Car Rent Prepayment',		@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(19,1,0,NULL,	@TradeAndOtherReceivables,	N'House Rent Prepayment',	@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(20,1,0,NULL,	@TradeAndOtherReceivables,	N'Maintenance Prepayment',	@SDG,			@RC_Inv,					NULL,			NULL,				NULL),

	(21,0,0,NULL,	@TradeAndOtherReceivables,	N'Abu Ammar Car Loan - NC',	@USD,			@RC_Inv,					NULL,			N'employees',		@Abu_Ammar),
	(22,0,0,NULL,	@TradeAndOtherReceivables,	N'M. Ali Car Loan - NC',	@USD,			@RC_Inv,					NULL,			N'employees',		@M_Ali),
	(23,0,0,NULL,	@TradeAndOtherReceivables,	N'El-Amin Car Loan - NC',	@USD,			@RC_Inv,					NULL,			N'employees',		@el_Amin),
--	(24,0,0,NULL,	@TradeAndOtherReceivables,	N'Abdurrahman Loan',		@USD,			@RC_Inv,					NULL,			N'employees',		@el_Amin),

	-- Equity and Liabilities accounts
	(30,0,0,NULL,	@IssuedCapital,				N'Issued Capital',			@USD,			@RC_Inv,					NULL,			NULL,				NULL),
	(31,0,0,NULL,	@RetainedEarnings,			N'Retained Earnings',		@USD,			@RC_Inv,					NULL,			NULL,				NULL),

	(32,1,0,N'PBL',	@TradeAndOtherPayables,		N'Employees Payables',		@USD,			@RC_Inv,					NULL,			N'employees',		NULL),
	(33,1,0,NULL,	@TradeAndOtherPayables,		N'10% Retained Salaries',	@USD,			@RC_Inv,					NULL,			N'employees',		NULL),
	(34,1,0,NULL,	@TradeAndOtherPayables,		N'PrimeLedgers A/P',		@USD,			@RC_Inv,					NULL,			NULL,				NULL),
	(35,1,0,N'PBL',	@TradeAndOtherPayables,		N'Trade Payables',			NULL,			@RC_Inv,					NULL,			N'suppliers',		NULL),
	(36,1,0,N'ACR',	@TradeAndOtherPayables,		N'Accruals',				@USD,			@RC_Inv,					NULL,			N'suppliers',		NULL),
	(37,1,0,NULL,	@TradeAndOtherPayables,		N'Dividends Payables',		@USD,			@RC_Inv,					NULL,			N'partners',		NULL),
	(38,1,0,NULL,	@TradeAndOtherPayables,		N'Borrowings from M/A',		@USD,			@RC_Inv,					NULL,			N'partners',		@PartnerMA),

	(39,1,0,N'URV',	@TradeAndOtherPayables,		N'Unearned Revenues - T3',	NULL,			@RC_Inv,					NULL,			N't3-customers',	NULL),
	(40,1,0,N'URV',	@TradeAndOtherPayables,		N'Unearned Revenues - T2',	NULL,			@RC_Inv,					NULL,			N't2-customers',	NULL),
	(41,1,0,N'SSC',	@TradeAndOtherPayables,		N'Employee Pensions',		NULL,			@RC_Inv,					NULL,			NULL,				NULL),
	(42,1,0,N'ZKT',	@TradeAndOtherPayables,		N'Zakat',					@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(43,1,0,N'VAT',	@TradeAndOtherPayables,		N'VAT Output',				@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
--	(44,1,0,NULL,	@TradeAndOtherPayables,		N'Income Tax',				@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(45,1,0,N'EIT',	@TradeAndOtherPayables,		N'Employees Income Tax',	@SDG,			@RC_Inv,					NULL,			NULL,				NULL),
	(46,1,0,N'EST',	@TradeAndOtherPayables,		N'Employees Stamp Tax',		@SDG,			@RC_Inv,					NULL,			NULL,				NULL),

	-- Profit/Loss Accounts
	(50,1,0,N'RVN',	@Revenue,					N'Revenues',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(51,1,0,NULL,	@Revenue,					N'Commission',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(52,1,0,NULL,	@OtherIncome,				N'Rental Income',			NULL,			@RC_Inv,					NULL,			NULL,				NULL),

	(69,1,0,NULL,	@ServicesExpense,			N'Accounting Services',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(83,1,0,NULL,	@ServicesExpense,			N'Legal Services',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(62,1,0,NULL,	@ServicesExpense,			N'Transportation',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(82,1,0,NULL,	@ServicesExpense,			N'Banking Services',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(80,1,0,NULL,	@ServicesExpense,			N'Travel',					NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(81,1,0,NULL,	@ServicesExpense,			N'Visa',					NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(67,1,0,NULL,	@ServicesExpense,			N'Internet & Tel',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(60,1,0,NULL,	@ServicesExpense,			N'Cloud Hosting',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(75,1,0,NULL,	@ServicesExpense,			N'Utilities',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(72,1,0,NULL,	@ServicesExpense,			N'Office Rental',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(64,1,0,NULL,	@ServicesExpense,			N'Marketing Service',		NULL,			@RC_Inv,					@DistributionCosts,N'cost-objects',	NULL),
	(63,1,0,NULL,	@ServicesExpense,			N'Domain Registration',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(77,1,0,NULL,	@ServicesExpense,			N'Maintenance',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(78,1,0,NULL,	@ServicesExpense,			N'Medical',					NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(65,1,0,N'SLR',	@EmployeeBenefitsExpense,	N'Salaries',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(79,1,0,NULL,	@EmployeeBenefitsExpense,	N'Zakat & Eid',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(66,1,0,NULL,	@EmployeeBenefitsExpense,	N'Contractors Salaries',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(70,1,0,N'SSC',	@EmployeeBenefitsExpense,	N'Employee Pension Contribution',NULL,		@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(88,1,0,NULL,	@EmployeeBenefitsExpense,	N'Allowances & Bonuses',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(76,1,0,NULL,	@EmployeeBenefitsExpense,	N'Employee Meals',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(71,1,0,NULL,	@OtherExpenseByNature,		N'Stationery & Grocery',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(74,1,0,NULL,	@OtherExpenseByNature,		N'Gov fees',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(84,1,0,NULL,	@OtherExpenseByNature,		N'Presentation tools',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(85,1,0,NULL,	@OtherExpenseByNature,		N'Education & Certifications',NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(86,1,0,NULL,	@OtherExpenseByNature,		N'Consumables',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(87,1,0,NULL,	@OtherExpenseByNature,		N'Tender Fees',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(89,1,0,NULL,	@OtherExpenseByNature,		N'Office Furniture',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(90,1,0,NULL,	@OtherExpenseByNature,		N'Other Expenses',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(98,1,0,N'DPR',	@DepreciationExpense,		N'Depreciation',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(99,1,0,N'DSP',	@OtherExpenseByNature,		N'Gain (loss) on disposal',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL);

	-- Expenses
	/*
	(69,1,0,NULL,	@ProfessionalFeesExpense,	N'Accounting Services',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(83,1,0,NULL,	@ProfessionalFeesExpense,	N'Legal Services',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(62,1,0,NULL,	@TransportationExpense,		N'Transportation',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(82,1,0,NULL,	@BankAndSimilarCharges,		N'Banking Services',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(80,1,0,NULL,	@TravelExpense,				N'Travel',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(81,1,0,NULL,	@TravelExpense,				N'Visa',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(67,1,0,NULL,	@CommunicationExpense,		N'Internet & Tel',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(60,1,0,NULL,	@UtilitiesExpense,			N'Cloud Hosting',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(75,1,0,NULL,	@UtilitiesExpense,			N'Utilities',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(72,1,0,NULL,	@UtilitiesExpense,			N'Office Rental',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(64,1,0,NULL,	@AdvertisingExpense,		N'Marketing Service',	NULL,			@RC_Inv,					@DistributionCosts,N'cost-objects',	NULL),

	(65,1,0,1,@WagesAndSalaries,		N'Salaries',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(79,1,0,NULL,@WagesAndSalaries,		N'Zakat & Eid',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(66,1,0,NULL,@WagesAndSalaries,		N'Contractors Salaries',NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(70,1,0,1,@SocialSecurityContributions,N'Employee Pension Contribution',NULL,@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(88,1,0,NULL,@EmployeeBenefitsExpense,	N'Allowances & Bonuses',NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(76,1,0,NULL,@OtherShorttermEmployeeBenefits,N'Employee Meals',NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(63,1,0,NULL,@OtherExpenseByNature,	N'Domain Registration',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
		
	(71,1,0,NULL,@OtherExpenseByNature,	N'Stationery & Grocery',NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(74,1,0,NULL,@OtherExpenseByNature,	N'Gov fees',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(77,1,0,NULL,@OtherExpenseByNature,	N'Maintenance',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(78,1,0,NULL,@OtherExpenseByNature,	N'Medical',				NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(84,1,0,NULL,@OtherExpenseByNature,	N'Presentation tools',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(85,1,0,NULL,@OtherExpenseByNature,	N'Education & Certifications',NULL,		@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(86,1,0,NULL,@OtherExpenseByNature,	N'Consumables',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(87,1,0,NULL,@OtherExpenseByNature,	N'Tender Fees',			NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(89,1,0,NULL,@OtherExpenseByNature,	N'Office Furniture',	NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(90,1,0,NULL,@OtherExpenseByNature,	N'Other Expenses',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),

	(98,1,0,1,@OtherExpenseByNature,	N'Depreciation',		NULL,			@RC_Inv,					NULL,			N'cost-objects',	NULL),
	(99,1,0,1,@OtherExpenseByNature,	N'Gain (loss) on disposal',	NULL,		@RC_Inv,					NULL,			N'cost-objects',	NULL);
*/

	UPDATE @Accounts SET HasResource = 1 WHERE [Index] IN (0);

	EXEC [api].[Accounts__Save]
		@Entities = @Accounts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Accounts: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @1GMFund INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'GM Fund');
	DECLARE @1KSAFund INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'KSA Fund');
	DECLARE @1BOK INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Bank Of Khartoum');
	DECLARE @1Meals INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employee Meals');
	DECLARE @1Education INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Education & Certifications');
	DECLARE @1MAPayable INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Borrowings from M/A');
	DECLARE @1DomainRegistration INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Domain Registration');
	DECLARE @1Maintenance INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Maintenance');
	DECLARE @1Utilities INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Utilities');
	DECLARE @1Internet INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Internet & Tel');
	DECLARE @1EITax INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employees Income Tax');
	DECLARE @1EStax INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employees Stamp Tax');
	DECLARE @1T2ARUSD INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Tier-2 A/R - USD');
	DECLARE @1Revenues INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Revenues');
	--DECLARE @1GMFund INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employee Income Tax');
END