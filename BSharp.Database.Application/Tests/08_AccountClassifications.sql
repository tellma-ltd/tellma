/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/
-- TODO
-- Try Accounts that have built in Resource, Agent, Responsibility center, or a combination thereof.


DECLARE @AccountClassifications dbo.AccountClassificationList, @Accounts dbo.AccountList;
Declare @Assets_AC INT, @CurrentAssets_AC INT, @BankAndCash_AC INT, @Debtors_AC INT, @Inventories_AC INT, @NonCurrentAssets_AC INT,
	@Liabilities_AC INT, @Equity_AC INT, @Revenue_AC INT, @Expenses_AC INT;

DECLARE @CBEUSD INT, @CBEETB INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT, 
	@RegusAccount INT, @VimeksAccount INT, @NocJimmaAccount INT, @ToyotaAccount INT, @PrepaidRental INT;
DECLARE @PPEVehicles INT, @PPEWarehouse INT;
DECLARE @fuelHR INT, @fuelSalesAdminAG INT, @fuelProduction INT, @fuelSalesDistAG INT;
DECLARE @VATInput INT, @VATOutput INT, @SalariesAdmin INT, @SalariesAccrualsTaxable INT, @OvertimeAdmin INT,
		@SalariesAccrualsNonTaxable INT, @EmployeesPayable INT, @EmployeesIncomeTaxPayable INT;

		INSERT INTO dbo.AccountDefinitions
([Id],							[TitlePlural],				[TitleSingular]) VALUES
(N'gl-accounts',				N'GL Accounts',				N'GL Account'),
(N'customers-accounts',			N'Customers Accounts',		N'Customer Account'),
(N'employees-accounts',			N'Employees Accounts',		N'Employee Account'),
(N'suppliers-accounts',			N'Suppliers Accounts',		N'Supplier Account'),
(N'inventories-accounts',		N'Inventories Accounts',	N'Inventory Account'),
(N'fixed-assets-accounts',		N'Fixed Assets Accounts',	N'Fixed Asset Account'),
(N'banks-accounts',				N'Banks Accounts',			N'Bank Account'),
(N'cash-on-accounts',			N'Cash On Hand Accounts',	N'Cash On Hand Account');

INSERT INTO @AccountClassifications([Index], [Name], [Code]) VALUES
(0, N'Assets', N'1'),
(1, N'Current Assets', N'11'),
(2, N'Bank and Cash', N'111'),
(3, N'Debtors', N'112'),
(4, N'Inventory', N'113'),
(5, N'Non-current Assets', N'12'),
(6, N'Liabilities', N'2'),
(7, N'Equity', N'3'),
(8, N'Revenue', N'4'),
(9, N'Expenses', N'5')
;
EXEC [api].[AccountClassifications__Save] --  N'cash-and-cash-equivalents',
	@Entities = @AccountClassifications,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Inserting AccountClassifications'
	GOTO Err_Label;
END;

SELECT @Assets_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1';
SELECT @CurrentAssets_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'11';
SELECT @BankAndCash_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'111';
SELECT @Debtors_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'112';
SELECT @Inventories_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'113';
SELECT @NonCurrentAssets_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'12';
SELECT @Liabilities_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2';
SELECT @Equity_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'3';
SELECT @Revenue_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'4';
SELECT @Expenses_AC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5';

SELECT * FROM [map].[AccountClassifications]();

INSERT INTO @Accounts([Index], [AccountTypeId], [AccountClassificationId], [Name], [Code]) VALUES
(0,N'BalancesWithBanks', @BankAndCash_AC, N'CBE - USD', N'1101'),
(1,N'BalancesWithBanks',@BankAndCash_AC, N'CBE - ETB', N'1102'); -- reserved money to pay for LC when needed
INSERT INTO @Accounts([Index],
	[AccountTypeId],				[AccountClassificationId],	[Name],								[Code]) VALUES
(2,N'Liabilities',					@CurrentAssets_AC,			N'CBE - LC',						N'1201'), -- reserved money to pay for LC when needed
(3,N'Inventories',					@CurrentAssets_AC,			N'TF1903950009',					N'1209'), -- Merchandise in transit, for given LC
(4,N'Inventories',					@CurrentAssets_AC,			N'PPE Warehouse',					N'1210'),
(5,N'PropertyPlantAndEquipment',	@NonCurrentAssets_AC,		N'PPE - Vehicles',					N'1301'),
(6,N'TradeAndOtherCurrentPayables',	@CurrentAssets_AC,			N'Vimeks',							N'2101'),
(7,N'Equity',						@Equity_AC,					N'Capital - MA',					N'3101'),
(8,N'Equity',						@Equity_AC,					N'Capital - AA',					N'3102'),
(9,N'TradeAndOtherCurrentPayables', @Liabilities_AC,			N'Noc Jimma',						N'2102'),
(10,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Toyota',							N'2103'),
(11,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Regus',							N'2104'),
(12,N'TradeAndOtherCurrentPayables',@Debtors_AC,				N'Prepaid Rental',					N'1501'),
(13,N'CurrentAssets',				@CurrentAssets_AC,			N'VAT Input',						N'1401'),
(14,N'CurrentLiabilities',			@Liabilities_AC,			N'VAT Output',						N'2401'),
(15,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Salaries Accruals, taxable',		N'2501'),
(16,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Salaries Accruals, non taxable',	N'2502'),
(17,N'TradeAndOtherCurrentPayables',@Liabilities_AC,			N'Employees payable',				N'2503'),
(18,N'CurrentLiabilities',			NULL,						N'Employees Income Tax payable',	N'2504'),
(19,N'AdministrativeExpense',		@Expenses_AC,				N'fuel - HR',						N'5101'),
(20,N'AdministrativeExpense',		@Expenses_AC,				N'fuel - Sales - admin - AG',		N'5102'),
(21,N'CostOfSales',					@Expenses_AC,				N'fuel - Production',				N'5103'),
(22,N'DistributionCosts',			@Expenses_AC,				N'fuel - Sales - distribution - AG',N'5201'),
(23,N'AdministrativeExpense',		@Expenses_AC,				N'Salaries - Admin',				N'5202'),
(24,N'AdministrativeExpense',		@Expenses_AC,				N'Overtime - Admin',				N'5203');

SELECT * FROM dbo.Accounts;

SELECT @CBEUSD = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1101';
SELECT @CBEETB = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1102';
SELECT @CBELC = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1201';
SELECT @ESL = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1209';
SELECT @PPEWarehouse = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1210';
SELECT @PPEVehicles = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1301'; 
SELECT @VimeksAccount = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2101';
SELECT @CapitalMA = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'3102';

SELECT @NocJimmaAccount = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2102';
SELECT @ToyotaAccount = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2103';
SELECT @RegusAccount = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2104';
SELECT @PrepaidRental = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1501';
SELECT @VATInput = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'1401';
SELECT @VATOutput = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2401';

SELECT @SalariesAccrualsTaxable = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2501';
SELECT @SalariesAccrualsNonTaxable = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2502';
SELECT @EmployeesPayable = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2503';
SELECT @EmployeesIncomeTaxPayable = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'2504'

SELECT @fuelHR = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5201';

SELECT @SalariesAdmin = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5202';
SELECT @OvertimeAdmin = [Id] FROM dbo.[AccountClassifications] WHERE Code = N'5203';


--INSERT INTO dbo.AccountDefinitions
--([Id],							[TitlePlural],				[TitleSingular], [CustodianLabel] VALUES
--(N'gl-accounts',				N'GL Accounts',				N'GL Account'), -- all hidden
--(N'suppliers-accounts',			N'Suppliers Accounts',		N'Supplier Account'), -- Resource [Currency], Agent [Supplier]
--(N'customers-accounts',			N'Customers Accounts',		N'Customer Account'), -- Resource [Currency], Agent [Customer]
--(N'employees-accounts',			N'Employees Accounts',		N'Employee Account'), -- Resource [Currency], Agent [Employee]
--(N'cash-on-hand-accounts',		N'Cash On Hand Accounts',	N'Cash On Hand Account'),
--(N'banks-accounts',				N'Bank Accounts',			N'Bank Account'),
--(N'inventories-accounts',		N'Inventory Accounts',		N'Inventory Account'),
--(N'fixed-assets-accounts',		N'Fixed Asset Accounts',	N'Fixed Asset Account');