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
DECLARE @CBEUSD INT, @CBEETB INT, @CBELC INT, @ESL INT, @CapitalMA INT, @CapitalAA INT, 
	@RegusAccount INT, @VimeksAccount INT, @NocJimmaAccount INT, @ToyotaAccount INT, @PrepaidRental INT;
DECLARE @PPEVehicles INT, @PPEWarehouse INT;
DECLARE @fuelHR INT, @fuelSalesAdminAG INT, @fuelProduction INT, @fuelSalesDistAG INT;
DECLARE @VATInput INT, @VATOutput INT, @SalariesAdmin INT, @SalariesAccrualsTaxable INT, @OvertimeAdmin INT,
		@SalariesAccrualsNonTaxable INT, @EmployeesPayable INT, @EmployeesIncomeTaxPayable INT;

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
	Print 'Inserting GL Accounts'
	GOTO Err_Label;
END;

SELECT * FROM rpt.[AccountClassifications]();

INSERT INTO dbo.AccountDefinitions
([Id],							[TitlePlural],				[TitleSingular]) VALUES
(N'gl-accounts',				N'GL Accounts',				N'GL Account'); -- all hidden

--INSERT INTO @Accounts([Index], [GLAccountId], [Name], [Code]) VALUES
--(0,N'BankAndCash', N'CBE - USD', N'1101'),
--(1,N'BankAndCash', N'CBE - ETB', N'1102'), -- reserved money to pay for LC when needed
--(2,N'CurrentAssets', N'CBE - LC', N'1201'), -- reserved money to pay for LC when needed
--(3,N'CurrentAssets', N'TF1903950009', N'1209'), -- Merchandise in transit, for given LC
--(4,N'CurrentAssets', N'PPE Warehouse', N'1210'),
--(5,N'FixedAssets', N'PPE - Vehicles', N'1301'),
--(6,N'Payable', N'Vimeks', N'2101'),
--(7,N'Equity', N'Capital - MA', N'3101'),
--(8,N'Equity', N'Capital - AA', N'3102'),
--(9,N'Payable', N'Noc Jimma', N'2102'),
--(10,N'Payable',	N'Toyota', N'2103'),
--(11,N'Payable', N'Regus',	N'2104'),
--(12,N'Prepayments', N'Prepaid Rental',	N'1501'),
--(13,N'CurrentAssets', N'VAT Input', N'1401'),
--(14,N'CurrentLiabilities', N'VAT Output', N'2401'),
--(15,N'Payable', N'Salaries Accruals, taxable',		N'2501'),
--(16,N'Payable', N'Salaries Accruals, non taxable',	N'2502'),
--(17,N'Payable', N'Employees payable',				N'2503'),
--(18,N'Payable', N'Employees Income Tax payable',	N'2504'),
--(19,N'Expenses', N'fuel - HR', N'5101'),
--(20,N'Expenses', N'fuel - Sales - admin - AG', N'5102'),
--(21,N'DirectCosts', N'fuel - Production', N'5103'),
--(22,N'Expenses', N'fuel - Sales - distribution - AG', N'5201'),
--(23,N'Expenses', N'Salaries - Admin', N'5202'),
--(24,N'Expenses', N'Overtime - Admin', N'5203');
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