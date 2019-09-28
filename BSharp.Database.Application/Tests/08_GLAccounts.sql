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
INSERT INTO dbo.AccountDefinitions
([Id],							[TitlePlural],				[TitleSingular],		[AccountType]) VALUES
(N'simple-accounts',			N'Simple Account',			N'Simple Accounts',		NULL),
(N'trade-payables-accounts',	N'Suppliers Payables',		N'Supplier Payable',	N'Payable'),
(N'trade-prepayments-accounts',	N'Suppliers Prepayments',	N'Supplier Prepayment',	N'Prepayments'),
(N'employee-payables-accounts',	N'Employee Payables',		N'Employee Payable',	N'Payable'),
(N'trade-receivables-accounts',	N'Customer Receivables',	N'Customer Receivable',	N'Receivable'),
(N'employee-debtors-accounts',	N'Employee Loans',			N'Employee Loan',		N'Receivable'),
(N'banks-accounts',				N'Bank Accounts',			N'Bank Account',		N'BankAndCash'),
(N'inventories-accounts',		N'Inventory Accounts',		N'Inventory Account',	N'CurrentAssets'),
(N'fixed-assets-accounts',		N'Fixed Asset Accounts',	N'Fixed Asset Account',	N'FixedAssets');
DECLARE @CBEUSD INT, @CBEETB INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT, 
	@RegusAccount INT, @VimeksAccount INT, @NocJimmaAccount INT, @ToyotaAccount INT, @PrepaidRental INT;
DECLARE @PPEVehicles INT, @PPEWarehouse INT;
DECLARE @fuelHR INT, @fuelSalesAdminAG INT, @fuelProduction INT, @fuelSalesDistAG INT;
DECLARE @VATInput INT, @VATOutput INT, @SalariesAdmin INT, @SalariesAccrualsTaxable INT, @OvertimeAdmin INT,
		@SalariesAccrualsNonTaxable INT, @EmployeesPayable INT, @EmployeesIncomeTaxPayable INT;

INSERT INTO dbo.[GLAccounts]([AccountType], [Name], [Code]) VALUES
(N'BankAndCash', N'CBE - USD', N'1101'),
(N'BankAndCash', N'CBE - ETB', N'1102'), -- reserved money to pay for LC when needed
(N'CurrentAssets', N'CBE - LC', N'1201'), -- reserved money to pay for LC when needed

(N'CurrentAssets', N'TF1903950009', N'1209'), -- Merchandise in transit, for given LC
(N'CurrentAssets', N'PPE Warehouse', N'1210'),
(N'FixedAssets', N'PPE - Vehicles', N'1301'),

(N'Payable', N'Vimeks', N'2101'),

(N'Equity', N'Capital - MA', N'3101'),
(N'Equity', N'Capital - AA', N'3102');

SELECT @CBEUSD = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1101';
SELECT @CBEETB = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1102';
SELECT @CBELC = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1201';
SELECT @ESL = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1209';
SELECT @PPEWarehouse = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1210';
SELECT @PPEVehicles = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1301'; 
SELECT @VimeksAccount = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2101';
SELECT @CapitalMA = [Id] FROM dbo.[GLAccounts] WHERE Code = N'3101';
SELECT @CapitalAA = [Id] FROM dbo.[GLAccounts] WHERE Code = N'3102';

INSERT INTO dbo.[GLAccounts]
([AccountType],	[Name],	[Code]) VALUES
(N'Payable', N'Noc Jimma', N'2102'),
(N'Payable',	N'Toyota', N'2103'),
(N'Payable', N'Regus',	N'2104'),
(N'Prepayments', N'Prepaid Rental',	N'1501'),
(N'CurrentAssets', N'VAT Input', N'1401'),
(N'CurrentLiabilities', N'VAT Output', N'2401')
;

SELECT @NocJimmaAccount = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2102';
SELECT @ToyotaAccount = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2103';
SELECT @RegusAccount = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2104';
SELECT @PrepaidRental = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1501';
SELECT @VATInput = [Id] FROM dbo.[GLAccounts] WHERE Code = N'1401';
SELECT @VATOutput = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2401';


INSERT INTO dbo.[GLAccounts]
([AccountType], [Name],							[Code]) VALUES
(N'Payable', N'Salaries Accruals, taxable',		N'2501'),
(N'Payable', N'Salaries Accruals, non taxable',	N'2502'),
(N'Payable', N'Employees payable',				N'2503'),
(N'Payable', N'Employees Income Tax payable',	N'2504')
;
SELECT @SalariesAccrualsTaxable = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2501';
SELECT @SalariesAccrualsNonTaxable = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2502';
SELECT @EmployeesPayable = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2503';
SELECT @EmployeesIncomeTaxPayable = [Id] FROM dbo.[GLAccounts] WHERE Code = N'2504'

INSERT INTO dbo.[GLAccounts]
([AccountType], [Name], [Code]) VALUES
(N'Expenses', N'fuel - HR', N'5101'),
(N'Expenses', N'fuel - Sales - admin - AG', N'5102'),
(N'DirectCosts', N'fuel - Production', N'5103'),
(N'Expenses', N'fuel - Sales - distribution - AG', N'5201');
SELECT @fuelHR = [Id] FROM dbo.[GLAccounts] WHERE Code = N'5101';
SELECT @fuelSalesAdminAG = [Id] FROM dbo.[GLAccounts] WHERE Code = N'5102';
SELECT @fuelProduction = [Id] FROM dbo.[GLAccounts] WHERE Code = N'5103';
SELECT @fuelSalesDistAG = [Id] FROM dbo.[GLAccounts] WHERE Code = N'5201';

INSERT INTO dbo.[GLAccounts](
[AccountType], [Name], [Code]) VALUES
(N'Expenses', N'Salaries - Admin', N'5202'),
(N'Expenses', N'Overtime - Admin', N'5203')
;
SELECT @SalariesAdmin = [Id] FROM dbo.[GLAccounts] WHERE Code = N'5202';
SELECT @OvertimeAdmin = [Id] FROM dbo.[GLAccounts] WHERE Code = N'5203';