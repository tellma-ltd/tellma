INSERT INTO @ContractDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'creditors', N'Creditor', N'Creditors', N'hands', N'Financials',10),
(1, N'debtors', N'Debtor', N'Debtors', N'hand-holding-usd', N'Financials',60),
(2, N'owners', N'Owner', N'Owners', N'power-off', N'Financials',70),
(3, N'partners', N'Partner', N'Partners', N'user-tie', N'Financials',40),
(4, N'suppliers', N'Supplier', N'Suppliers', N'truck', N'Purchasing',50),
(5, N'customers', N'Customer', N'Customers', N'balance-scale', N'Sales',80),
(6, N'employees', N'Employee', N'Employees', N'user-friends', N'HumanCapital',80),
(7, N'bank-accounts', N'Bank Account', N'Bank Accounts', N'book', N'Cash',90),
(8, N'cashonhand-accounts', N'Cash On Account', N'Cash On Accounts', N'funnel-dollar', N'Cash',95),
(9, N'warehouses', N'Warehouse', N'Warehouses', N'warehouse', N'Financials',60),
(10, N'foreign-imports', N'Foreign Import', N'Foreign Imports', N'file-import', N'Purchasing',120),
(11, N'foreign-exports', N'Foreign Export', N'Foreign Exports', N'file-export', N'Sales',120);



EXEC [api].[ContractDefinitions__Save]
	@Entities = @ContractDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'ContractDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
--Declarations
DECLARE @creditorsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'creditors');
DECLARE @debtorsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'debtors');
DECLARE @ownersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'owners');
DECLARE @partnersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'partners');
DECLARE @suppliersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'suppliers');
DECLARE @customersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'customers');
DECLARE @employeesCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'employees');
DECLARE @bank_accountsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'bank-accounts');
DECLARE @cashonhand_accountsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'cashonhand-accounts');
DECLARE @warehousesCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'warehouses');
DECLARE @foreign_importsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'foreign-imports');
DECLARE @foreign_exportsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'foreign-exports');
