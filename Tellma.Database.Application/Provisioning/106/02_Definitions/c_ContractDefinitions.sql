INSERT INTO @ContractDefinitions([Index], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'creditors', N'Creditor', N'አበዳሪ', N'Creditor', N'Creditors', N'አበዳሪዎች', N'Creditors', N'hands', N'Financials',10),
(1, N'debtors', N'Debtor', N'አበዳሪ', N'Debtor', N'Debtors', N'አበዳሪዎች', N'Debtors', N'hand-holding-usd', N'Financials',60),
(2, N'owners', N'Owner', N'ባለቤት', N'Owner', N'Owners', N'ባለቤቶች', N'Owners', N'power-off', N'Financials',70),
(3, N'partners', N'Partner', N'አጋር', N'Partner', N'Partners', N'አጋሮች', N'Partners', N'user-tie', N'Financials',40),
(4, N'suppliers', N'Supplier', N'አቅራቢ', N'Supplier', N'Suppliers', N'አቅራቢዎች', N'Suppliers', N'truck', N'Purchasing',50),
(5, N'customers', N'Customer', N'ደንበኛው', N'Customer', N'Customers', N'ደንበኞች', N'Customers', N'balance-scale', N'Sales',80),
(6, N'employees', N'Employee', N'ተቀጣሪ', N'Employee', N'Employees', N'ሠራተኞች', N'Employees', N'user-friends', N'HumanCapital',80),
(7, N'bank-accounts', N'Bank Account', N'የባንክ ሒሳብ', N'Bank Account', N'Bank Accounts', N'የባንክ ሂሳቦች', N'Bank Accounts', N'book', N'Cash',90),
(8, N'petty-cash-funds', N'Petty Cash Fund', N'የቤት እንስሳት ገንዘብ ፈንድ', N'Petty Cash Fund', N'Petty Cash Funds', N'የቤት እንስሳት ጥሬ ገንዘብ', N'Petty Cash Funds', N'money-check-alt', N'Cash',100),
(9, N'cashiers', N'Cashier', N'ገንዘብ ተቀባይ', N'Cashier', N'Cashiers', N'ገንዘብ ተቀባይ', N'Cashiers', N'money-check-alt', N'Cash',101),
(10, N'warehouses', N'Warehouse', N'መጋዘን', N'Warehouse', N'Warehouses', N'መጋዘኖች', N'Warehouses', N'warehouse', N'Financials',60),
(11, N'foreign-imports', N'Foreign Import', N'የውጭ አስመጪ', N'Foreign Import', N'Foreign Imports', N'የውጭ ማስመጣት', N'Foreign Imports', N'file-import', N'Purchasing',120),
(12, N'foreign-exports', N'Foreign Export', N'የውጭ መላኪያ', N'Foreign Export', N'Foreign Exports', N'የውጭ ንግድ', N'Foreign Exports', N'file-export', N'Sales',120);

EXEC [api].[ContractDefinitions__Save]
	@Entities = @ContractDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'ContractDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
	DECLARE @106creditorsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'creditors');
	DECLARE @106debtorsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'debtors');
	DECLARE @106ownersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'owners');
	DECLARE @106partnersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'partners');
	DECLARE @106suppliersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'suppliers');
	DECLARE @106customersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'customers');
	DECLARE @106employeesCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'employees');
	DECLARE @106bank_accountsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'bank-accounts');
	DECLARE @106petty_cash_fundsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'petty-cash-funds');
	DECLARE @106cashiersCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'cashiers');
	DECLARE @106warehousesCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'warehouses');
	DECLARE @106foreign_importsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'foreign-imports');
	DECLARE @106foreign_exportsCD INT = (SELECT [Id] FROM dbo.ContractDefinitions WHERE [Code] = N'foreign-exports');