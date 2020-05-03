IF @DB = N'101' -- Banan SD, USD, en
BEGIN
/*
Entry Type - Account Type - Center - Currency - Contract Definition - Agent
	*/
		
	INSERT INTO @Accounts([Index],
		[Code], [DefinitionId],	[ClassificationId],	[IfrsTypeId],						[Name],							[CurrencyId],	[CenterId],		[EntryTypeId],		[ContractId]) VALUES
	-- Assets Accounts
	(10,N'12001',@ppeADef,@NonCurrentAssets_AC,		@FixturesAndFittings,				N'Fixtures and fittings',		@USD,			NULL,			@PPEAdditions,			NULL),
	(11,N'12002',@ppeADef,@NonCurrentAssets_AC,		@OfficeEquipment,					N'Office equipment',			@USD,			NULL,			@PPEAdditions,			NULL),
	(12,N'12003',@ppeADef,@NonCurrentAssets_AC,		@OfficeEquipment,					N'Comp. equip. & acc.',			@USD,			NULL,			@PPEAdditions,			NULL),

	(310,N'12011',@ppeADef,@NonCurrentAssets_AC,	@FixturesAndFittings,				N'Acc. Dep.- Fixtures and fittings',@USD,		NULL,			@PPEDepreciations,		NULL),
	(311,N'12012',@ppeADef,@NonCurrentAssets_AC,	@OfficeEquipment,					N'Acc. Dep.- Office equipment',	@USD,			NULL,			@PPEDepreciations,		NULL),
	(312,N'12013',@ppeADef,@NonCurrentAssets_AC,	@OfficeEquipment,					N'Acc. Dep.- Comp. equip. & acc.',@USD,			NULL,			@PPEDepreciations,		NULL),

	(20,N'11201',@customerADef,	@Debtors_AC,		@CurrentTradeReceivables,			N'Trade Receivables',			NULL,			@C101_INV,		NULL,					NULL),

	(21,N'11211',@general_BSADef,@Debtors_AC,		@CurrentTradeReceivables,			N'Banan ET',					@USD,			@C101_INV,		NULL,					NULL),
	(22,N'11212',@general_BSADef,@Debtors_AC,		@CurrentTradeReceivables,			N'PrimeLedgers A/R',			@USD,			@C101_INV,		NULL,					NULL),

	(23,N'11221',@partnerADef,	@Debtors_AC,		@TradeAndOtherCurrentReceivables,	N'Partners Withdrawals',		@USD,			@C101_INV,		NULL,					NULL),
													-- TODO: remove this extension
	(24,N'12021',@employeeADef,	@Debtors_AC,		@OtherNoncurrentReceivables,		N'Abu Ammar Car Loan',			@USD,			@C101_INV,		NULL,					@Abu_Ammar),
	(25,N'12022',@employeeADef,	@Debtors_AC,		@OtherNoncurrentReceivables,		N'M. Ali Car Loan',				@USD,			@C101_INV,		NULL,					@M_Ali),
	(26,N'12023',@employeeADef,	@Debtors_AC,		@OtherNoncurrentReceivables,		N'El-Amin Car Loan',			@USD,			@C101_INV,		NULL,					@el_Amin),
	(27,N'12031',@general_BSADef,@Debtors_AC,		@CurrentValueAddedTaxReceivables,	N'VAT Input',					NULL,			@C101_INV,		NULL,					NULL),
--	(28,N'11206',@general_BSADef,@Debtors_AC,		@TradeAndOtherCurrentReceivables,	N'Commissions',					@USD,			@C101_INV,		NULL,					NULL),
	(29,N'11231',@supplierADef,	@Debtors_AC,		@CurrentTradeReceivables,			N'Office Rent',					@SDG,			@C101_INV,		NULL,					NULL),
	(30,N'11232',@supplierADef,	@Debtors_AC,		@CurrentPrepayments,				N'Internet Prepayment',			@SDG,			@C101_INV,		NULL,					NULL),
	(31,N'11233',@supplierADef,	@Debtors_AC,		@CurrentPrepayments,				N'Car Rent Prepayment',			@SDG,			@C101_INV,		NULL,					NULL),
	(32,N'11234',@supplierADef,	@Debtors_AC,		@CurrentPrepayments,				N'House Rent Prepayment',		@SDG,			@C101_INV,		NULL,					NULL),
	(33,N'11235',@supplierADef,	@Debtors_AC,		@CurrentPrepayments,				N'Maintenance Prepayment',		@SDG,			@C101_INV,		NULL,					NULL),

	(38,N'11241',@customerADef,	@Debtors_AC,		@CurrentAccruedIncome,				N'Accrued Income',				NULL,			@C101_INV,		NULL,					NULL),

	(41,N'11111',@cashADef,		@BankAndCash_AC,	@CashOnHand,						N'GM Fund - SDG',				@SDG,			@C101_INV,		NULL,					@GMSafe),
	(42,N'11112',@cashADef,		@BankAndCash_AC,	@CashOnHand,						N'GM Fund - USD',				@USD,			@C101_INV,		NULL,					@GMSafe),
	(43,N'11113',@cashADef,		@BankAndCash_AC,	@CashOnHand,						N'Admin Fund - SDG',			@SDG,			@C101_INV,		NULL,					@AdminSafe),
	(44,N'11114',@cashADef,		@BankAndCash_AC,	@CashOnHand,						N'KSA Fund',					@SAR,			@C101_INV,		NULL,					@KSASafe),
	(45,N'11121',@cashADef,		@BankAndCash_AC,	@BalancesWithBanks,					N'Bank Of Khartoum - SDG',		@SDG,			@C101_INV,		NULL,					@KRTBank),

	-- Equity and Liabilities accounts
	(50,N'30001',@general_BSADef,@Equity_AC,		@IssuedCapital,						N'Issued Capital',				@USD,			@C101_INV,		NULL,					NULL),
	(51,N'30002',@general_BSADef,@Equity_AC,		@RetainedEarnings,					N'Retained Earnings',			@USD,			@C101_INV,		NULL,					NULL),

	(61,N'21001',@employeeADef,@CurrentLiabilities_AC,@CurrentPayablesToEmployeesExtension,N'Employees Payables - USD',	@USD,			@C101_INV,		NULL,					NULL),
	(62,N'21002',@employeeADef,@CurrentLiabilities_AC,@CurrentPayablesToEmployeesExtension,N'Employees Payables - SDG',	@SDG,			@C101_INV,		NULL,					NULL),
	(63,N'21003',@employeeADef,@CurrentLiabilities_AC,@TradeAndOtherCurrentPayables,		N'10% Retained Salaries',	@USD,			@C101_INV,		NULL,					NULL),
	(64,N'21004',@general_BSADef,@CurrentLiabilities_AC,@TradeAndOtherCurrentPayables,	N'PrimeLedgers A/P',			@USD,			@C101_INV,		NULL,					NULL);
/*
	(65,N'3030',	@TradeAndOtherCurrentPayablesToTradeSuppliers,N'Trade Payables',	NULL,			@C101_INV,		NULL,				N'suppliers',		NULL),
	(66,N'3035',	@AccrualsClassifiedAsCurrent,		N'Accrued Expenses',			NULL,			@C101_INV,		NULL,				N'suppliers',		NULL),
	(67,N'3040',	@TradeAndOtherCurrentPayables,		N'Dividends Payables',			@USD,			@C101_INV,		NULL,				N'partners',		NULL),
	(68,N'3045',	@TradeAndOtherCurrentPayables,		N'Borrowings from M/A',			@USD,			@C101_INV,		NULL,				N'partners',		@PartnerMA),

	(69,N'3049',	@DeferredIncomeClassifiedAsCurrent,	N'Deferred Income - USD',		@USD,			@C101_INV,		NULL,				NULL),
	(70,N'3050',	@DeferredIncomeClassifiedAsCurrent,	N'Deferred Income - SDG',		@SDG,			@C101_INV,		NULL,				NULL),
	(71,N'3110',	@CurrentSocialSecurityPayablesExtension,N'Employee Pensions',		@SDG,			@C101_INV,		NULL,				NULL),
	(72,N'3120',	@CurrentZakatPayablesExtension,		N'Zakat',						@SDG,			@C101_INV,		NULL,				NULL),
	(73,N'3130',	@CurrentValueAddedTaxPayables,		N'VAT Output',					NULL,			@C101_INV,		NULL,				NULL),
--	(74,N'3140',	@CurrentTradeAndOtherPayables,		N'Income Tax',					@SDG,			@C101_INV,		NULL,				NULL),
	(75,N'3150',	@CurrentEmployeeIncomeTaxPayablesExtension,	N'Employees Income Tax',@SDG,			@C101_INV,		NULL,				NULL),
	(76,N'3160',	@CurrentEmployeeStampTaxPayablesExtension,	N'Employees Stamp Tax',	@SDG,			@C101_INV,		NULL,				NULL),
		-- Profit/Loss Accounts
	(91,N'4110',	@RevenueFromRenderingOfServices,	N'Revenues - B10 - USD',		@USD,			@C101_B10,		NULL,				NULL),
	(92,N'4120',	@RevenueFromRenderingOfServices,	N'Revenues - BSmart - USD',		@USD,			@C101_BSmart,	NULL,				NULL),
	(93,N'4121',	@RevenueFromRenderingOfServices,	N'Revenues - BSmart - SDG',		@SDG,			@C101_BSmart,	NULL,				NULL),
	(94,N'4130',	@RevenueFromRenderingOfServices,	N'Revenues - Campus - USD',		@USD,			@C101_Campus,	NULL,				NULL),
	(95,N'4140',	@RevenueFromRenderingOfServices,	N'Revenues - Tellma - USD',		@USD,			@C101_Tellma,	NULL,				NULL),
/*	-- Add Account Type: Commissions
	(95,N'4210',	@Revenue,							N'Commission - B10',			NULL,			@C101_B10,		NULL,				NULL),
	(96,N'4220',	@Revenue,							N'Commission - BSmart',			NULL,			@C101_BSmart,	NULL,				NULL),
	(97,N'4230',	@Revenue,							N'Commission - Campus',			NULL,			@C101_Campus,	NULL,				NULL),
	(98,N'4240',	@Revenue,							N'Commission - Tellma',			NULL,			@C101_Tellma,	NULL,				NULL),
*/
	(99,N'4910',	@OtherRevenue,						N'Rental Income - SAR',			@SAR,			@C101_FFLR,		NULL,				NULL),
-- 5: Direct, Cost of sales
-- 6: Indirect, Production, 7:service
-- 8:Distribution
-- 9:Admin. Nature: 2 digits, Center: 2 digits, Varieties: 1

	(101,N'90510',	@ProfessionalFeesExpense,		N'Acc. & Legal Services - USD',	@USD,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(102,N'90511',	@ProfessionalFeesExpense,		N'Acc. & Legal Services - SDG',	@SDG,			@C101_EXEC,		@AdministrativeExpense,	NULL),
/*
	(110,N'X0611',	@TransportationExpense,			N'Transportation',			NULL,			NULL,			NULL,					NULL),
	(115,N'90710',	@BankAndSimilarCharges,			N'Banking Services',		NULL,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(120,N'X0811',	@TravelExpense,					N'Visa & Travel',			NULL,			NULL,			NULL,					NULL),
	(130,N'A0900',	@CommunicationExpense,			N'Internet & Tel',			NULL,			@C101_UNALLOC,	@OtherExpenseByFunction,NULL),
	(135,N'50511',	@ServicesExpense,				N'Cloud Hosting',			@USD,			NULL,			@CostOfSales,			NULL),
*/	
	(140,N'A1000',	@UtilitiesExpense,				N'Utilities',				@SDG,			@C101_UNALLOC,	@OtherExpenseByFunction,NULL),
	(145,N'A9900',	@ServicesExpense,				N'Office Rental',			@SDG,			@C101_UNALLOC,	@OtherExpenseByFunction,NULL),
/*
	(150,N'81120',	@AdvertisingExpense,			N'Marketing Service - SDG',		NULL,			@C101_Sales,	@DistributionCosts,		NULL),
	(151,N'81120',	@AdvertisingExpense,			N'Marketing Service - SDG',		NULL,			@C101_Sales,	@DistributionCosts,		NULL),
	(155,N'X05111',	@ServicesExpense,				N'Domain Registration',		@USD,			NULL,			NULL,					NULL),
	(160,N'X05112',	@ServicesExpense,				N'Maintenance',				NULL,			NULL,			NULL,					NULL),
	(165,N'X05113',	@ServicesExpense,				N'Medical',					NULL,			NULL,			NULL,					NULL),
*/
	(170,N'92310',	@WagesAndSalaries,		N'Salaries - Exec Office Equip.',	@USD,			@C101_EXEC,		@AdministrativeExpense,	NULL),
	(171,N'82320',	@WagesAndSalaries,		N'Salaries - Sales Equip.',			@USD,			@C101_Sales,	@DistributionCosts,		NULL),
	(172,N'72330',	@WagesAndSalaries,		N'Salaries - Sys Admin Equip.',		@USD,			@C101_Sys,		@ServiceExtension,		NULL),
	(173,N'52340',	@WagesAndSalaries,		N'Salaries - B10/HCM Equip.',		@USD,			@C101_B10,		@CostOfSales,			NULL),
	(174,N'52350',	@WagesAndSalaries,		N'Salaries - BSmart Equip.',		@USD,			@C101_BSmart,	@CostOfSales,			NULL),
	(175,N'52360',	@WagesAndSalaries,		N'Salaries - Campus Equip.',		@USD,			@C101_Campus,	@CostOfSales,			NULL),
	(176,N'52370',	@WagesAndSalaries,		N'Salaries - Tellma Equip.',		@USD,			@C101_Tellma,	@CostOfSales,			NULL),
	(177,N'52380',	@WagesAndSalaries,		N'Salaries - Floor Rental Equip.',	@USD,			@C101_FFLR,		@CostOfSales,			NULL),
/*
	(178,N'X1201',	@EmployeeBenefitsExpense,		N'Zakat & Eid',				@USD,			NULL,			NULL,					NULL),

	(180,N'50550',	@ProfessionalFeesExpense,		N'Salaries - B10 Contractors',@USD,			@C101_B10,		@CostOfSales,			NULL),
	(185,N'X1500',	@SocialSecurityContributions,	N'Employee Pension Contribution',@SDG,		NULL,			NULL,					NULL),
	(190,N'X1202',	@EmployeeBenefitsExpense,		N'Allowances & Bonuses',	NULL,			NULL,			NULL,					NULL),
	(195,N'X99001',	@OtherExpenseByNature,			N'Employee Meals',			NULL,			NULL,			NULL,					NULL),

	(200,N'X99002',	@OtherExpenseByNature,			N'Stationery & Grocery',	NULL,			NULL,			NULL,					NULL),
	*/
	(205,N'99910',	@OtherExpenseByNature,			N'Gov fees',				@SDG,			@C101_EXEC,		@AdministrativeExpense,	NULL),
/*
	(210,N'89920',	@OtherExpenseByNature,			N'Presentation tools',		NULL,			@C101_Sales,	@DistributionCosts,		NULL),
	(215,N'X99003',	@OtherExpenseByNature,			N'Education & Certifications',NULL,			NULL,			NULL,					NULL),
	(220,N'X99004',	@OtherExpenseByNature,			N'Consumables',				NULL,			NULL,			NULL,					NULL),
	(225,N'X99005',	@OtherExpenseByNature,			N'Tender Fees',				NULL,			NULL,			NULL,					NULL),
	(230,N'X99006',	@OtherExpenseByNature,			N'Office Furniture',		NULL,			NULL,			NULL,					NULL),
	(235,N'X99007',	@OtherExpenseByNature,			N'Other Expenses',			NULL,			NULL,			NULL,					NULL),
*/
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
*/
-- 5: Direct, Cost of sales
-- 6: Indirect, Production, 7:service
-- 8:Distribution
-- 9:Admin. Nature: 2 digits, Center: 2 digits, Varieties: 1

	-- Expenses
	/*
	(69,	@ProfessionalFeesExpense,	N'Accounting Services',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(83,	@ProfessionalFeesExpense,	N'Legal Services',		NULL,			@C101_INV,					NULL,			NULL,	NULL),

	(62,	@TransportationExpense,		N'Transportation',		NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(82,	@BankAndSimilarCharges,		N'Banking Services',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(80,	@TravelExpense,				N'Travel',				NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(81,	@TravelExpense,				N'Visa',				NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(67,	@CommunicationExpense,		N'Internet & Tel',		NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(60,	@UtilitiesExpense,			N'Cloud Hosting',		NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(75,	@UtilitiesExpense,			N'Utilities',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(72,	@UtilitiesExpense,			N'Office Rental',		NULL,			@C101_INV,					NULL,			NULL,	NULL),

	(64,	@AdvertisingExpense,		N'Marketing Service',	NULL,			@C101_INV,					@DistributionCosts,NULL,	NULL),

	(65,@WagesAndSalaries,		N'Salaries',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(79,@WagesAndSalaries,		N'Zakat & Eid',			NULL,			@C101_INV,					NULL,			NULL,	NULL),
	(66,@WagesAndSalaries,		N'Contractors Salaries',			@C101_INV,					NULL,			NULL,	NULL),
	(70,@SocialSecurityContributions,N'Employee Pension Contribution',@C101_INV,					NULL,			NULL,	NULL),
	(88,@EmployeeBenefitsExpense,	N'Allowances & Bonuses',			@C101_INV,					NULL,			NULL,	NULL),
	(76,@OtherShorttermEmployeeBenefits,N'Employee Meals',			@C101_INV,					NULL,			NULL,	NULL),

	(63,@OtherExpenseByNature,	N'Domain Registration',	NULL,			@C101_INV,					NULL,			NULL,	NULL),
		
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
	DECLARE @1AR INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Trade Receivables');
	DECLARE @1Revenues INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Revenues');
	--DECLARE @1GMFund INT = (SELECT [Id] FROM dbo.Accounts WHERE [Name] = N'Employee Income Tax');
END