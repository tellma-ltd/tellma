DECLARE @BA_CBEUSD INT, @BA_CBEETB INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT, 
	@RegusAccount INT, @VimeksAccount INT, @NocJimmaAccount INT, @BA_ToyotaAccount INT, @PrepaidRental INT;
DECLARE @PPEVehicles INT, @PPEWarehouse INT;
DECLARE @fuelHR INT, @fuelSalesAdminAG INT, @fuelProduction INT, @fuelSalesDistAG INT;
DECLARE @VATInput INT, @VATOutput INT, @SalariesAdmin INT, @SalariesAccrualsTaxable INT, @OvertimeAdmin INT,
		@SalariesAccrualsNonTaxable INT, @EmployeesPayable INT, @EmployeesIncomeTaxPayable INT;

DECLARE @BasicAccounts dbo.AccountList;

--IF @DB = N'100' -- ACME, USD, en/ar/zh
--INSERT INTO @BasicAccounts([Index],
--	[AccountTypeId],				[LegacyTypeId],				[AccountClassificationId],		[Name],								[Code], [CurrencyId]) VALUES
--(0,N'CashAndCashEquivalents',	N'Cash',						@BankAndCash_AC,			N'CBE - USD',						N'1101', N'USD'),
--(1,N'CashAndCashEquivalents',	N'Cash',						@BankAndCash_AC,			N'CBE - ETB',						N'1102', N'ETB'),
--(2,N'CashAndCashEquivalents',	N'Cash',						@BankAndCash_AC,			N'CBE - LC',						N'1201', N'ETB'), -- reserved DECIMAL (19,4) to pay for LC when needed
--(3,N'InventoriesTotal',			N'Inventory',					@Inventories_AC,			N'TF1903950009',					N'1209', N'ETB'), -- Merchandise in transit, for given LC
--(4,N'InventoriesTotal',			N'Inventory',					@Inventories_AC,			N'PPE Warehouse',					N'1210', N'ETB'),
--(5,N'PropertyPlantAndEquipment',N'FixedAssets',					@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301', N'ETB'),
--(6,N'TradeAndOtherReceivables',	N'OtherCurrentAssets',			@Debtors_AC,				N'Prepaid Rental',					N'1401', N'ETB'),
--(7,N'TradeAndOtherReceivables', N'AccountsReceivable',			@Debtors_AC,				N'VAT Input',						N'1501', N'ETB'),
--(8,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Vimeks',							N'2101', N'ETB'),
--(9,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Noc Jimma',						N'2102', N'ETB'),
--(10,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Toyota',							N'2103', N'ETB'),
--(11,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Regus',							N'2104', N'ETB'),
--(12,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501', N'ETB'),
--(13,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502', N'ETB'),
--(14,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Employees payable',				N'2503', N'ETB'),
--(15,N'TradeAndOtherPayables',	N'OtherCurrentLiabilities',		@Liabilities_AC,			N'VAT Output',						N'2601', N'ETB'),
--(16,N'TradeAndOtherPayables',	N'OtherCurrentLiabilities',		@Liabilities_AC,			N'Employees Income Tax payable',	N'2602', N'ETB'),
--(17,N'IssuedCapital',			N'EquityDoesntClose',			@Equity_AC,					N'Capital - MA',					N'3101', N'ETB'),
--(18,N'IssuedCapital',			N'EquityDoesntClose',			@Equity_AC,					N'Capital - AA',					N'3102', N'ETB'),
--(19,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'fuel - HR',						N'5101', N'ETB'),
--(20,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102', N'ETB'),
--(21,N'OtherExpenseByNature',	N'CostofSales',					@Expenses_AC,				N'fuel - Production',				N'5103', N'ETB'),
--(22,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'fuel - Sales - distribution - AG',N'5201', N'ETB'),
--(23,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'Salaries - Admin',				N'5202', N'ETB'),
--(24,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'Overtime - Admin',				N'5203', N'ETB');
--ELSE IF @DB = N'101' -- Banan SD, USD, en
--	Print N''
--ELSE IF @DB = N'102' -- Banan ET, ETB, en
--IF @DB IN (N'101', N'102', N'103', N'104') 
INSERT INTO @BasicAccounts([Index],
	[AccountTypeId],			[LegacyTypeId],			[LegacyClassificationId],	[Name],								[Code], [CurrencyId]) VALUES
(0,@CashAndCashEquivalents,		N'Cash',					@BankAndCash_AC,		N'CBE - USD',						N'1101', N'USD'),
(1,@CashAndCashEquivalents,		N'Cash',					@BankAndCash_AC,		N'CBE - ETB',						N'1102', N'ETB'),
(2,@CashAndCashEquivalents,		N'Cash',					@BankAndCash_AC,		N'CBE - LC',						N'1201', N'ETB'), -- reserved DECIMAL (19,4) to pay for LC when needed
(3,@InventoriesTotal,			N'Inventory',				@Inventories_AC,		N'TF1903950009',					N'1209', N'ETB'), -- Merchandise in transit, for given LC
(4,@InventoriesTotal,			N'Inventory',				@Inventories_AC,		N'PPE Warehouse',					N'1210', N'ETB'),
(5,@PropertyPlantAndEquipment,	N'FixedAssets',				@NonCurrentAssets_AC,	N'PPE - Vehicles',					N'1301', N'ETB'),
(6,@TradeAndOtherReceivables,	N'OtherCurrentAssets',		@Debtors_AC,			N'Prepaid Rental',					N'1401', N'ETB'),
(7,@TradeAndOtherReceivables,	N'AccountsReceivable',		@Debtors_AC,			N'VAT Input',						N'1501', N'ETB'),
(8,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Vimeks',							N'2101', N'ETB'),
(9,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Noc Jimma',						N'2102', N'ETB'),
(10,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Toyota',							N'2103', N'ETB'),
(11,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Regus',							N'2104', N'ETB'),
(12,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Salaries Accruals, taxable',		N'2501', N'ETB'),
(13,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Salaries Accruals, non taxable',	N'2502', N'ETB'),
(14,@TradeAndOtherPayables,		N'AccountsPayable',			@Liabilities_AC,		N'Employees payable',				N'2503', N'ETB'),
(15,@TradeAndOtherPayables,		N'OtherCurrentLiabilities',	@Liabilities_AC,		N'VAT Output',						N'2601', N'ETB'),
(16,@TradeAndOtherPayables,		N'OtherCurrentLiabilities',	@Liabilities_AC,		N'Employees Income Tax payable',	N'2602', N'ETB'),
(17,@IssuedCapital,				N'EquityDoesntClose',		@Equity_AC,				N'Capital - MA',					N'3101', N'ETB'),
(18,@IssuedCapital,				N'EquityDoesntClose',		@Equity_AC,				N'Capital - AA',					N'3102', N'ETB'),
(19,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - HR',						N'5101', N'ETB'),
(20,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - Sales - admin - AG',		N'5102', N'ETB'),
(21,@OtherExpenseByNature,		N'CostofSales',				@Expenses_AC,			N'fuel - Production',				N'5103', N'ETB'),
(22,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'fuel - Sales - distribution - AG',N'5201', N'ETB'),
(23,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'Salaries - Admin',				N'5202', N'ETB'),
(24,@OtherExpenseByNature,		N'Expenses',				@Expenses_AC,			N'Overtime - Admin',				N'5203', N'ETB');
UPDATE @BasicAccounts SET IsCurrent = (CASE WHEN [Index] IN (5,17,18) THEN 0 ELSE 1 END)
--ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
--	Print N''
--ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
--INSERT INTO @BasicAccounts([Index],
--	[AccountTypeId],			[LegacyTypeId],				[[LegacyClassificationId]],		[Name],								[Code], [CurrencyId]) VALUES
--(0,N'CashAndCashEquivalents',	N'Cash',						@BankAndCash_AC,			N'CBE - USD',						N'1101', N'USD'),
--(1,N'CashAndCashEquivalents',	N'Cash',						@BankAndCash_AC,			N'CBE - ETB',						N'1102', N'ETB'),
--(2,N'CashAndCashEquivalents',	N'Cash',						@BankAndCash_AC,			N'CBE - LC',						N'1201', N'ETB'), -- reserved DECIMAL (19,4) to pay for LC when needed
--(3,N'InventoriesTotal',			N'Inventory',					@Inventories_AC,			N'TF1903950009',					N'1209', N'ETB'), -- Merchandise in transit, for given LC
--(4,N'InventoriesTotal',			N'Inventory',					@Inventories_AC,			N'PPE Warehouse',					N'1210', N'ETB'),
--(5,N'PropertyPlantAndEquipment',N'FixedAssets',					@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301', N'ETB'),
--(6,N'TradeAndOtherReceivables',	N'OtherCurrentAssets',			@Debtors_AC,				N'Prepaid Rental',					N'1401', N'ETB'),
--(7,N'TradeAndOtherReceivables', N'AccountsReceivable',			@Debtors_AC,				N'VAT Input',						N'1501', N'ETB'),
--(8,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Vimeks',							N'2101', N'ETB'),
--(9,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Noc Jimma',						N'2102', N'ETB'),
--(10,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Toyota',							N'2103', N'ETB'),
--(11,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Regus',							N'2104', N'ETB'),
--(12,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501', N'ETB'),
--(13,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502', N'ETB'),
--(14,N'TradeAndOtherPayables',	N'AccountsPayable',				@Liabilities_AC,			N'Employees payable',				N'2503', N'ETB'),
--(15,N'TradeAndOtherPayables',	N'OtherCurrentLiabilities',		@Liabilities_AC,			N'VAT Output',						N'2601', N'ETB'),
--(16,N'TradeAndOtherPayables',	N'OtherCurrentLiabilities',		@Liabilities_AC,			N'Employees Income Tax payable',	N'2602', N'ETB'),
--(17,N'IssuedCapital',			N'EquityDoesntClose',			@Equity_AC,					N'Capital - MA',					N'3101', N'ETB'),
--(18,N'IssuedCapital',			N'EquityDoesntClose',			@Equity_AC,					N'Capital - AA',					N'3102', N'ETB'),
--(19,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'fuel - HR',						N'5101', N'ETB'),
--(20,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102', N'ETB'),
--(21,N'OtherExpenseByNature',	N'CostofSales',					@Expenses_AC,				N'fuel - Production',				N'5103', N'ETB'),
--(22,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'fuel - Sales - distribution - AG',N'5201', N'ETB'),
--(23,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'Salaries - Admin',				N'5202', N'ETB'),
--(24,N'OtherExpenseByNature',	N'Expenses',					@Expenses_AC,				N'Overtime - Admin',				N'5203', N'ETB');ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh

EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
	@Entities = @BasicAccounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting Basic Accounts: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF @DebugAccounts = 1
	SELECT * FROM map.Accounts();

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
SELECT @BA_ToyotaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2103';
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
