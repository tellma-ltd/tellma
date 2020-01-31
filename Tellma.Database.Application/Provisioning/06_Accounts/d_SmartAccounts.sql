DECLARE @SA_CBEUSD INT, @SA_CBEETB INT, @SA_CBELC INT, @SA_ESL INT, @SA_CapitalMA INT, @SA_CapitalAA INT, 
	@SA_RegusAccount INT, @SA_VimeksAccount INT, @SA_NocJimmaAccount INT, @SA_ToyotaAccount INT, @SA_PrepaidRental INT;
DECLARE @SA_PPEVehicles INT, @SA_PPEWarehouse INT;
DECLARE @SA_fuelHR INT, @SA_fuelSalesAdminAG INT, @SA_fuelProduction INT, @SA_fuelSalesDistAG INT;
DECLARE @SA_VATInput INT, @SA_VATOutput INT, @SA_SalariesAdmin INT, @SA_SalariesAccrualsTaxable INT, @SA_OvertimeAdmin INT,
		@SA_SalariesAccrualsNonTaxable INT, @SA_EmployeesPayable INT, @SA_EmployeesIncomeTaxPayable INT;
DECLARE @SmartAccounts dbo.AccountList;
IF @DB = N'101' -- Banan SD, USD, en
BEGIN
INSERT INTO @SmartAccounts([Index],
	[AccountTypeId],				[LegacyTypeId],[LegacyClassificationId],	[Name],			[Code],		[AgentDefinitionId], [IsCurrent], [AgentId],			[ResourceId], [EntryTypeId], [CurrencyId], [ResponsibilityCenterId]) VALUES
	(0,@CashAndCashEquivalents,		N'Cash',			@BankAndCash_AC,N'GM Fund/USD',		N'1011',	N'custodies',			1,		dbo.fn_AGCode__Id(N'GM'),	NULL,			NULL,				N'USD',		@RC_ExecutiveOffice),
	(1,@CashAndCashEquivalents,		N'Cash',			@BankAndCash_AC,N'GM Fund/SDG',		N'1012',	N'custodies',			1,		dbo.fn_AGCode__Id(N'GM'),	NULL,			NULL,				N'SDG',		@RC_ExecutiveOffice),
	--(2,@CashAndCashEquivalents	N'Cash',			@BankAndCash_AC,N'GM Fund',			N'1013',	N'custodies',			1,		dbo.fn_AGCode__Id(N'GM'),	NULL,			NULL,				NULL,		@RC_ExecutiveOffice),
	(3,@CashAndCashEquivalents,		N'Cash',			@BankAndCash_AC,N'BOK -	SDG',		N'1021',	N'banks',				1,		dbo.fn_AGCode__Id(N'BOK'),	NULL,			NULL,				N'SDG',		@RC_ExecutiveOffice),
	(4,@TradeAndOtherReceivables,	N'OtherCurrentAssets',@Debtors_AC,	N'VAT Input',		N'1301',	N'tax-agencies',		1,		@VAT,						NULL,			NULL,				N'SDG',		@RC_ExecutiveOffice),
	(5,@TradeAndOtherReceivables,	N'AccountsReceivable',@Debtors_AC,	N'Trade Debtors/USD',N'1101',	N'customers',			1,		NULL,						NULL,			NULL,				N'USD',		@RC_ExecutiveOffice),
	(6,@TradeAndOtherReceivables,	N'AccountsReceivable',@Debtors_AC,	N'Trade Debtors/SDG',N'1102',	N'customers',			1,		NULL,						NULL,			NULL,				N'SDG',		@RC_ExecutiveOffice),
	(7,@TradeAndOtherReceivables,	N'AccountsReceivable',@Debtors_AC,	N'Prepaid Salaries/USD',N'1201',N'employees',			1,		NULL,						NULL,			NULL,				N'USD',		@RC_ExecutiveOffice)
	
	;
	--UPDATE @SmartAccounts SET HasAgent = 1;
END
ELSE IF @DB IN (N'101', N'102', N'103', N'104') 
BEGIN
INSERT INTO @SmartAccounts([Index],
	[AccountTypeId],		[LegacyTypeId],	[LegacyClassificationId],	[Name],			[Code],	[AgentDefinitionId], [IsCurrent], [AgentId],	[ResourceId], [Identifier], [EntryTypeId], [CurrencyId]) VALUES
--(0,N'Cash',				@BankAndCash_AC,			N'CBE - USD',						N'1101'),
--(1,N'Cash',				@BankAndCash_AC,			N'CBE - ETB',						N'1102'),
(0,@CashAndCashEquivalents,	N'Cash',			@BankAndCash_AC,		N'CBE - USD 2',	N'1111',	N'banks',				1,		@Bank_CBE,		NULL,			NULL,			NULL,				N'USD'),
(1,@CashAndCashEquivalents,	N'Cash',			@BankAndCash_AC,		N'CBE - ETB 2',	N'1112',	N'banks',				1,		@Bank_CBE,		NULL,			NULL,			NULL,				N'ETB'),
--(3,@InventoriesTotal,		N'Inventory',			@Inventories_AC,			N'TF1903950009',					N'1209'), -- Merchandise in transit, for given LC
(4,@InventoriesTotal,			N'Inventory',		@Inventories_AC,		N'RM Warehouse',N'1220',	N'custodies',		1,		@Warehouse_RM,	NULL,			NULL,			NULL,				N'ETB'),
--(5,N'FixedAssets',		@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301'),
--(6,N'OtherCurrentAssets',	@Debtors_AC,				N'Prepaid Rental',					N'1401'),
--(7,N'AccountsReceivable',	@Debtors_AC,				N'VAT Input',						N'1501'),
--(8,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,			N'Vimeks',							N'2101'),
--(9,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,			N'Noc Jimma',						N'2102'),
(10,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,		N'Toyota',		N'2113',	N'suppliers',			1,		@Toyota,		NULL,			NULL,			NULL,				N'ETB'),
--(11,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,			N'Regus',							N'2104'),
--(12,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501'),
--(13,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502'),
--(14,@TradeAndOtherPayables,	N'AccountsPayable',	@Liabilities_AC,			N'Employees payable',				N'2503'),
--(17,N'EquityDoesntClose',	@Equity_AC,					N'Capital - MA',					N'3101'),
--(18,N'EquityDoesntClose',	@Equity_AC,					N'Capital - AA',					N'3102'),
--(19,N'Expenses',			@Expenses_AC,				N'fuel - HR',						N'5101'),
--(20,N'Expenses',			@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102'),
--(21,N'CostofSales',		@Expenses_AC,				N'fuel - Production',				N'5103'),
--(22,N'Expenses',			@Expenses_AC,				N'fuel - Sales - distribution - AG',1,N'5201'),
--(23,N'Expenses',			@Expenses_AC,				N'Salaries - Admin',				N'5212',	N'Expenses',		N'cost-centers',	dbo.fn_ATCode__Id(N'WagesAndSalaries'),	1,NULL,			NULL,			NULL,		dbo.fn_ECCode__Id('AdministrativeExpense')),
(24,@EmployeeBenefitsExpense,	N'Expenses',		@Expenses_AC,			N'Overtime - Admin',N'5213',N'cost-centers',	1,	NULL,			NULL,			NULL,		dbo.fn_ETCode__Id('AdministrativeExpense'),	N'ETB');
	--UPDATE @SmartAccounts SET HasAgent = 1;
	UPDATE @SmartAccounts SET HasResource = 1 WHERE [Index] IN (4, 5, 19,20, 21, 22, 23, 24);
END
EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
	@Entities = @SmartAccounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting Smart Accounts' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF @DebugAccounts = 1
	SELECT * FROM map.Accounts();

SELECT @SA_CBEUSD = [Id] FROM dbo.[Accounts] WHERE Code = N'1111';
SELECT @SA_CBEETB = [Id] FROM dbo.[Accounts] WHERE Code = N'1112';
--SELECT @SA_CBELC = [Id] FROM dbo.[Accounts] WHERE Code = N'1211';
--SELECT @SA_ESL = [Id] FROM dbo.[Accounts] WHERE Code = N'1219';
--SELECT @SA_PPEWarehouse = [Id] FROM dbo.[Accounts] WHERE Code = N'1211';
--SELECT @SA_PPEVehicles = [Id] FROM dbo.[Accounts] WHERE Code = N'1311'; 
--SELECT @SA_PrepaidRental = [Id] FROM dbo.[Accounts] WHERE Code = N'1411';
--SELECT @SA_VATInput = [Id] FROM dbo.[Accounts] WHERE Code = N'1511';

--SELECT @SA_VimeksAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2111';
--SELECT @SA_CapitalMA = [Id] FROM dbo.[Accounts] WHERE Code = N'3111';
--SELECT @SA_CapitalAA = [Id] FROM dbo.[Accounts] WHERE Code = N'3112';

--SELECT @SA_NocJimmaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2112';
SELECT @SA_ToyotaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2113';
--SELECT @SA_RegusAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2114';
--SELECT @SA_SalariesAccrualsTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2511';
--SELECT @SA_SalariesAccrualsNonTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2512';
--SELECT @SA_EmployeesPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2513';
--SELECT @SA_VATOutput = [Id] FROM dbo.[Accounts] WHERE Code = N'2611';
--SELECT @SA_EmployeesIncomeTaxPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2612';

--SELECT @SA_fuelHR = [Id] FROM dbo.[Accounts] WHERE Code = N'5111';
--SELECT @SA_fuelSalesAdminAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5112';
--SELECT @SA_fuelProduction = [Id] FROM dbo.[Accounts] WHERE Code = N'5113';
--SELECT @SA_fuelSalesDistAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5211';

--SELECT @SA_SalariesAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5212';
SELECT @SA_OvertimeAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5213';
