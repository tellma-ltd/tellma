/* Use Cases
Missing
	- Inserting
	- Updating
	- Deleting
	- Activating
	- Deactivating
*/

DECLARE @AccountClassifications dbo.AccountClassificationList;
Declare @Assets_AC INT, @CurrentAssets_AC INT, @BankAndCash_AC INT, @Debtors_AC INT, @Inventories_AC INT, @NonCurrentAssets_AC INT,
	@Liabilities_AC INT, @Equity_AC INT, @Revenue_AC INT, @Expenses_AC INT;

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

IF @DebugAccounts = 1
	SELECT * FROM [map].[AccountClassifications]();