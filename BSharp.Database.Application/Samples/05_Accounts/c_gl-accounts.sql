DECLARE @GLAccounts dbo.AccountList;
INSERT INTO @GLAccounts([Index],
	[AccountGroupId],				[AccountClassificationId],	[Name],								[Code]) VALUES
(0,N'BalancesWithBanks',			@BankAndCash_AC,			N'CBE - USD',						N'1101'),
(1,N'BalancesWithBanks',			@BankAndCash_AC,			N'CBE - ETB',						N'1102'),
(2,N'OtherCurrentLiability',		@BankAndCash_AC,			N'CBE - LC',						N'1201'), -- reserved money to pay for LC when needed
--(3,N'Inventories',					@Inventories_AC,			N'TF1903950009',					N'1209'), -- Merchandise in transit, for given LC
--(4,N'Inventories',					@Inventories_AC,			N'PPE Warehouse',					N'1210'),
(5,N'PropertyPlantAndEquipment',	@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301'),
(6,N'TradeAndOtherCurrentPayables',	@Liabilities_AC,			N'Vimeks',							N'2101'),
(7,N'Equity',						@Equity_AC,					N'Capital - MA',					N'3101'),
(8,N'Equity',						@Equity_AC,					N'Capital - AA',					N'3102'),
(9,N'TradeAndOtherCurrentPayables', @Liabilities_AC,			N'Noc Jimma',						N'2102'),
(10,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Toyota',							N'2103'),
(11,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Regus',							N'2104'),
(12,N'TradeAndOtherCurrentPayables',@Debtors_AC,				N'Prepaid Rental',					N'1501'),
--(13,N'CurrentAssets',				@Debtors_AC,				N'VAT Input',						N'1401'),
--(14,N'CurrentLiabilities',			@Liabilities_AC,			N'VAT Output',						N'2401'),
(15,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501'),
(16,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502'),
(17,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Employees payable',				N'2503'),
(18,N'CurrentLiabilities',			@Liabilities_AC,			N'Employees Income Tax payable',	N'2504'),
(19,N'AdministrativeExpense',		@Expenses_AC,				N'fuel - HR',						N'5101'),
(20,N'AdministrativeExpense',		@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102'),
(21,N'CostOfSales',					@Expenses_AC,				N'fuel - Production',				N'5103'),
(22,N'DistributionCosts',			@Expenses_AC,				N'fuel - Sales - distribution - AG',N'5201'),
(23,N'AdministrativeExpense',		@Expenses_AC,				N'Salaries - Admin',				N'5202'),
(24,N'AdministrativeExpense',		@Expenses_AC,				N'Overtime - Admin',				N'5203');


EXEC [api].[Accounts__Save] --  N'cash-and-cash-equivalents',
	@Entities = @GLAccounts,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting G/L Accounts'
	GOTO Err_Label;
END;

SELECT @CBEUSD = [Id] FROM dbo.[Accounts] WHERE Code = N'1101';
SELECT @CBEETB = [Id] FROM dbo.[Accounts] WHERE Code = N'1102';
SELECT @CBELC = [Id] FROM dbo.[Accounts] WHERE Code = N'1201';
SELECT @ESL = [Id] FROM dbo.[Accounts] WHERE Code = N'1209';
SELECT @PPEWarehouse = [Id] FROM dbo.[Accounts] WHERE Code = N'1210';
SELECT @PPEVehicles = [Id] FROM dbo.[Accounts] WHERE Code = N'1301'; 
SELECT @VimeksAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2101';
SELECT @CapitalMA = [Id] FROM dbo.[Accounts] WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.[Accounts] WHERE Code = N'3102';

SELECT @NocJimmaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2102';
SELECT @ToyotaAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2103';
SELECT @RegusAccount = [Id] FROM dbo.[Accounts] WHERE Code = N'2104';
SELECT @PrepaidRental = [Id] FROM dbo.[Accounts] WHERE Code = N'1501';


SELECT @SalariesAccrualsTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2501';
SELECT @SalariesAccrualsNonTaxable = [Id] FROM dbo.[Accounts] WHERE Code = N'2502';
SELECT @EmployeesPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2503';
SELECT @EmployeesIncomeTaxPayable = [Id] FROM dbo.[Accounts] WHERE Code = N'2504'

SELECT @fuelHR = [Id] FROM dbo.[Accounts] WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.[Accounts] WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.[Accounts] WHERE Code = N'5201';

SELECT @SalariesAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5202';
SELECT @OvertimeAdmin = [Id] FROM dbo.[Accounts] WHERE Code = N'5203';
