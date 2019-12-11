DECLARE @BasicAccounts dbo.AccountList;
DECLARE @RC_Cash INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [Code] = N'Cash');

INSERT INTO @BasicAccounts([Index],
	[AccountTypeId],				[AccountClassificationId],	[Name],								[Code], [CurrencyId]) VALUES
(0,N'Cash',							@BankAndCash_AC,			N'CBE - USD',						N'1101', N'ETB'),
(1,N'Cash',							@BankAndCash_AC,			N'CBE - ETB',						N'1102', N'ETB'),
(2,N'OtherCurrentLiabilities',		@BankAndCash_AC,			N'CBE - LC',						N'1201', N'ETB'), -- reserved DECIMAL (19,4) to pay for LC when needed
(3,N'Inventory',					@Inventories_AC,			N'TF1903950009',					N'1209', N'ETB'), -- Merchandise in transit, for given LC
(4,N'Inventory',					@Inventories_AC,			N'PPE Warehouse',					N'1210', N'ETB'),
(5,N'FixedAssets',					@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301', N'ETB'),
(6,N'OtherCurrentAssets',			@Debtors_AC,				N'Prepaid Rental',					N'1401', N'ETB'),
(7,N'AccountsReceivable',			@Debtors_AC,				N'VAT Input',						N'1501', N'ETB'),
(8,N'AccountsPayable',				@Liabilities_AC,			N'Vimeks',							N'2101', N'ETB'),
(9,N'AccountsPayable',				@Liabilities_AC,			N'Noc Jimma',						N'2102', N'ETB'),
(10,N'AccountsPayable',				@Liabilities_AC,			N'Toyota',							N'2103', N'ETB'),
(11,N'AccountsPayable',				@Liabilities_AC,			N'Regus',							N'2104', N'ETB'),
(12,N'AccountsPayable',				@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501', N'ETB'),
(13,N'AccountsPayable',				@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502', N'ETB'),
(14,N'AccountsPayable',				@Liabilities_AC,			N'Employees payable',				N'2503', N'ETB'),
(15,N'OtherCurrentLiabilities',		@Liabilities_AC,			N'VAT Output',						N'2601', N'ETB'),
(16,N'OtherCurrentLiabilities',		@Liabilities_AC,			N'Employees Income Tax payable',	N'2602', N'ETB'),
(17,N'EquityDoesntClose',			@Equity_AC,					N'Capital - MA',					N'3101', N'ETB'),
(18,N'EquityDoesntClose',			@Equity_AC,					N'Capital - AA',					N'3102', N'ETB'),
(19,N'Expenses',					@Expenses_AC,				N'fuel - HR',						N'5101', N'ETB'),
(20,N'Expenses',					@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102', N'ETB'),
(21,N'CostofSales',					@Expenses_AC,				N'fuel - Production',				N'5103', N'ETB'),
(22,N'Expenses',					@Expenses_AC,				N'fuel - Sales - distribution - AG',N'5201', N'ETB'),
(23,N'Expenses',					@Expenses_AC,				N'Salaries - Admin',				N'5202', N'ETB'),
(24,N'Expenses',					@Expenses_AC,				N'Overtime - Admin',				N'5203', N'ETB');

EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
	@Entities = @BasicAccounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting Basic Accounts'
	GOTO Err_Label;
END;

IF @DebugAccounts = 1
	SELECT * FROM map.Accounts() WHERE IsSmart = 0;

SELECT @BA_CBEUSD = [Id] FROM dbo.[Accounts] WHERE Code = N'1101';
SELECT @BA_CBEETB = [Id] FROM dbo.[Accounts] WHERE Code = N'1102';
SELECT @CBELC = [Id] FROM dbo.[Accounts] WHERE Code = N'1201';
SELECT @ESL = [Id] FROM dbo.[Accounts] WHERE Code = N'1209';
SELECT @PPEWarehouse = [Id] FROM dbo.[Accounts] WHERE Code = N'1210';
SELECT @PPEVehicles = [Id] FROM dbo.[Accounts] WHERE Code = N'1301'; 
SELECT @PrepaidRental = [Id] FROM dbo.[Accounts] WHERE Code = N'1401';
SELECT @VATInput = [Id] FROM dbo.[Accounts] WHERE Code = N'1501';

SELECT @VimeksAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2101';
SELECT @CapitalMA = [Id] FROM dbo.[Accounts] WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.[Accounts] WHERE Code = N'3102';

SELECT @NocJimmaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2102';
SELECT @ToyotaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2103';
SELECT @RegusAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2104';
SELECT @SalariesAccrualsTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2501';
SELECT @SalariesAccrualsNonTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2502';
SELECT @EmployeesPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2503';
SELECT @VATOutput = [Id] FROM dbo.[Accounts] WHERE Code = N'2601';
SELECT @EmployeesIncomeTaxPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2602';

SELECT @fuelHR = [Id] FROM dbo.[Accounts] WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.[Accounts] WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5201';

SELECT @SalariesAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5202';
SELECT @OvertimeAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5203';
