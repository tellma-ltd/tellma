DECLARE @SmartAccounts dbo.AccountList;
INSERT INTO @SmartAccounts([Index], [IsSmart],
	[AccountTypeId],		[AccountClassificationId],	[Name],								[Code],		[ContractType], [AgentDefinitionId], [ResourceClassificationId], [IsCurrent], [AgentId],	[ResourceId], [Identifier], [EntryClassificationId]) VALUES
--(0,N'Cash',				@BankAndCash_AC,			N'CBE - USD',						N'1101'),
--(1,N'Cash',				@BankAndCash_AC,			N'CBE - ETB',						N'1102'),
(0,1,N'Cash',				@BankAndCash_AC,			N'CBE - USD',						N'1111',	N'OnHand',		N'banks',			dbo.fn_RCCode__Id(N'Cash'),		1,		@Bank_CBE,		@R_USD,			NULL,			NULL),
(1,1,N'Cash',				@BankAndCash_AC,			N'CBE - ETB',						N'1112',	N'OnHand',		N'banks',			dbo.fn_RCCode__Id(N'Cash'),		1,		@Bank_CBE,		@R_ETB,			NULL,			NULL),
--(3,1,N'Inventory',			@Inventories_AC,			N'TF1903950009',					N'1209'), -- Merchandise in transit, for given LC
(4,1,N'Inventory',			@Inventories_AC,			N'RM Warehouse',					N'1220',	N'OnHand',		N'custodies',dbo.fn_RCCode__Id(N'RawMaterials'),	1,		@Warehouse_RM,	NULL,			NULL,			NULL),
--(5,1,N'FixedAssets',		@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301'),
--(6,1,N'OtherCurrentAssets',	@Debtors_AC,				N'Prepaid Rental',					N'1401'),
--(7,1,N'AccountsReceivable',	@Debtors_AC,				N'VAT Input',						N'1501'),
--(8,1,N'AccountsPayable',	@Liabilities_AC,			N'Vimeks',							N'2101'),
--(9,1,N'AccountsPayable',	@Liabilities_AC,			N'Noc Jimma',						N'2102'),
--(10,1,N'AccountsPayable',	@Liabilities_AC,			N'Toyota',							N'2103'),
--(11,1,N'AccountsPayable',	@Liabilities_AC,			N'Regus',							N'2104'),
--(12,1,N'AccountsPayable',	@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501'),
--(13,1,N'AccountsPayable',	@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502'),
--(14,1,N'AccountsPayable',	@Liabilities_AC,			N'Employees payable',				N'2503'),
--(17,1,N'EquityDoesntClose',	@Equity_AC,					N'Capital - MA',					N'3101'),
--(18,1,N'EquityDoesntClose',	@Equity_AC,					N'Capital - AA',					N'3102'),
--(19,1,N'Expenses',			@Expenses_AC,				N'fuel - HR',						N'5101'),
--(20,1,N'Expenses',			@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102'),
--(21,1,N'CostofSales',		@Expenses_AC,				N'fuel - Production',				N'5103'),
--(22,1,N'Expenses',			@Expenses_AC,				N'fuel - Sales - distribution - AG',1,N'5201'),
--(23,1,N'Expenses',			@Expenses_AC,				N'Salaries - Admin',				N'5212',	N'Expense',		N'cost-centers',	dbo.fn_RCCode__Id(N'WagesAndSalaries'),	1,NULL,			NULL,			NULL,		dbo.fn_ECCode__Id('AdministrativeExpense')),
(24,1,N'Expenses',			@Expenses_AC,				N'Overtime - Admin',				N'5213',	N'Expense',		N'cost-centers',	dbo.fn_RCCode__Id(N'WagesAndSalaries'),	1,	NULL,			NULL,			NULL,		dbo.fn_ECCode__Id('AdministrativeExpense'));

EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
	@Entities = @SmartAccounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting Smart Accounts'
	GOTO Err_Label;
END;

IF @DebugAccounts = 1
	SELECT * FROM map.Accounts() WHERE IsSmart = 1;

DECLARE @SA_CBEUSD INT, @SA_CBEETB INT, @SA_OvertimeAdmin INT;
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
--SELECT @SA_ToyotaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2113';
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
