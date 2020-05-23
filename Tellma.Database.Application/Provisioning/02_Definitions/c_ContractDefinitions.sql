DECLARE @ContractDefinitions dbo.ContractDefinitionList;

IF NOT EXISTS(SELECT * FROM dbo.[ContractDefinitions])
BEGIN
	IF @DB = N'100' -- ACME, USD, en/ar/zh playground
	BEGIN
		INSERT INTO @ContractDefinitions([Index], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
		(0, N'creditors', N'Creditor', N'债权人', N'Creditor', N'Creditors', N'债权人', N'Creditors', N'hands', N'Financials',10),
		(1, N'debtors', N'Debtor', N'债务人', N'Debtor', N'Debtors', N'债务人', N'Debtors', N'hand-holding-usd', N'Financials',60),
		(2, N'owners', N'Owner', N'所有者', N'Owner', N'Owners', N'拥有者', N'Owners', N'power-off', N'Financials',70),
		(3, N'partners', N'Partner', N'伙伴', N'Partner', N'Partners', N'伙伴', N'Partners', N'user-tie', N'Financials',40),
		(4, N'suppliers', N'Supplier', N'供应商', N'Supplier', N'Suppliers', N'供应商', N'Suppliers', N'truck', N'Purchasing',50),
		(5, N'customers', N'Customer', N'顾客', N'Customer', N'Customers', N'顾客', N'Customers', N'balance-scale', N'Sales',80),
		(6, N'employees', N'Employee', N'雇员', N'Employee', N'Employees', N'雇员', N'Employees', N'user-friends', N'HumanCapital',80),
		(7, N'bank-accounts', N'Bank Account', N'银行账户', N'Bank Account', N'Bank Accounts', N'银行账户', N'Bank Accounts', N'book', N'Cash',90),
		(8, N'cash-accounts', N'Cash Account', N'现金账户', N'Cash Account', N'Cash Accounts', N'现金账户', N'Cash Accounts', N'money-check-alt', N'Cash',100),
		(9, N'warehouses', N'Warehouse', N'仓库', N'Warehouse', N'Warehouses', N'货仓', N'Warehouses', N'warehouse', N'Financials',60),
		(10, N'foreign-imports', N'Foreign Import', N'国外进口', N'Foreign Import', N'Foreign Imports', N'国外进口', N'Foreign Imports', N'file-import', N'Purchasing',120),
		(11, N'foreign-exports', N'Foreign Export', N'对外出口', N'Foreign Export', N'Foreign Exports', N'国外出口', N'Foreign Exports', N'file-export', N'Sales',120);
	END
	ELSE IF @DB = N'101' -- Banan SD, USD, en
	BEGIN
		PRINT N''
	END
	ELSE IF @DB = N'102' -- Banan ET, ETB, en
	BEGIN
		PRINT N''
	END
	ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
	BEGIN
		PRINT N''
	END
	ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
	BEGIN
		INSERT INTO @ContractDefinitions([Index], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
		(0, N'creditors', N'Creditor', N'አበዳሪ', N'Creditor', N'Creditors', N'አበዳሪዎች', N'Creditors', N'hands', N'Financials',10),
		(1, N'debtors', N'Debtor', N'አበዳሪ', N'Debtor', N'Debtors', N'አበዳሪዎች', N'Debtors', N'hand-holding-usd', N'Financials',60),
		(2, N'owners', N'Owner', N'ባለቤት', N'Owner', N'Owners', N'ባለቤቶች', N'Owners', N'power-off', N'Financials',70),
		(3, N'partners', N'Partner', N'አጋር', N'Partner', N'Partners', N'አጋሮች', N'Partners', N'user-tie', N'Financials',40),
		(4, N'suppliers', N'Supplier', N'አቅራቢ', N'Supplier', N'Suppliers', N'አቅራቢዎች', N'Suppliers', N'truck', N'Purchasing',50),
		(5, N'customers', N'Customer', N'ደንበኛው', N'Customer', N'Customers', N'ደንበኞች', N'Customers', N'balance-scale', N'Sales',80),
		(6, N'employees', N'Employee', N'ተቀጣሪ', N'Employee', N'Employees', N'ሠራተኞች', N'Employees', N'user-friends', N'HumanCapital',80),
		(7, N'bank-accounts', N'Bank Account', N'የባንክ ሒሳብ', N'Bank Account', N'Bank Accounts', N'የባንክ ሂሳቦች', N'Bank Accounts', N'book', N'Cash',90),
		(8, N'cash-accounts', N'Cash Account', N'የጥሬ ገንዘብ ሂሳብ', N'Cash Account', N'Cash Accounts', N'የጥሬ ገንዘብ መለያዎች', N'Cash Accounts', N'money-check-alt', N'Cash',100),
		(9, N'warehouses', N'Warehouse', N'መጋዘን', N'Warehouse', N'Warehouses', N'መጋዘኖች', N'Warehouses', N'warehouse', N'Financials',60),
		(10, N'foreign-imports', N'Foreign Import', N'የውጭ አስመጪ', N'Foreign Import', N'Foreign Imports', N'የውጭ ማስመጣት', N'Foreign Imports', N'file-import', N'Purchasing',120),
		(11, N'foreign-exports', N'Foreign Export', N'የውጭ መላኪያ', N'Foreign Export', N'Foreign Exports', N'የውጭ ንግድ', N'Foreign Exports', N'file-export', N'Sales',120);
	END
	ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
	BEGIN
		PRINT N''
	END
	ELSE IF @DB = N'106' -- Soreti, ETB, en/am
	BEGIN
		INSERT INTO @ContractDefinitions([Index], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
		(0, N'creditors', N'Creditor', N'አበዳሪ', N'Creditor', N'Creditors', N'አበዳሪዎች', N'Creditors', N'hands', N'Financials',10),
		(1, N'debtors', N'Debtor', N'አበዳሪ', N'Debtor', N'Debtors', N'አበዳሪዎች', N'Debtors', N'hand-holding-usd', N'Financials',60),
		(2, N'owners', N'Owner', N'ባለቤት', N'Owner', N'Owners', N'ባለቤቶች', N'Owners', N'power-off', N'Financials',70),
		(3, N'partners', N'Partner', N'አጋር', N'Partner', N'Partners', N'አጋሮች', N'Partners', N'user-tie', N'Financials',40),
		(4, N'suppliers', N'Supplier', N'አቅራቢ', N'Supplier', N'Suppliers', N'አቅራቢዎች', N'Suppliers', N'truck', N'Purchasing',50),
		(5, N'customers', N'Customer', N'ደንበኛው', N'Customer', N'Customers', N'ደንበኞች', N'Customers', N'balance-scale', N'Sales',80),
		(6, N'employees', N'Employee', N'ተቀጣሪ', N'Employee', N'Employees', N'ሠራተኞች', N'Employees', N'user-friends', N'HumanCapital',80),
		(7, N'bank-accounts', N'Bank Account', N'የባንክ ሒሳብ', N'Bank Account', N'Bank Accounts', N'የባንክ ሂሳቦች', N'Bank Accounts', N'book', N'Cash',90),
		(8, N'cash-accounts', N'Cash Account', N'የጥሬ ገንዘብ ሂሳብ', N'Cash Account', N'Cash Accounts', N'የጥሬ ገንዘብ መለያዎች', N'Cash Accounts', N'money-check-alt', N'Cash',100),
		(9, N'warehouses', N'Warehouse', N'መጋዘን', N'Warehouse', N'Warehouses', N'መጋዘኖች', N'Warehouses', N'warehouse', N'Financials',60),
		(10, N'foreign-imports', N'Foreign Import', N'የውጭ አስመጪ', N'Foreign Import', N'Foreign Imports', N'የውጭ ማስመጣት', N'Foreign Imports', N'file-import', N'Purchasing',120),
		(11, N'foreign-exports', N'Foreign Export', N'የውጭ መላኪያ', N'Foreign Export', N'Foreign Exports', N'የውጭ ንግድ', N'Foreign Exports', N'file-export', N'Sales',120);
	END
END
EXEC [api].[ContractDefinitions__Save]
	@Entities = @ContractDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;



IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'ContractDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

	DECLARE @creditorsCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'creditors');
	DECLARE @debtorsCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'debtors');
	DECLARE @ownersCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'owners');
	DECLARE @partnersCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'partners');
	DECLARE @suppliersCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'suppliers');
	DECLARE @customersCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'customers');
	DECLARE @employeesCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'employees');
	DECLARE @bank_accountsCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'bank-accounts');
	DECLARE @cash_accountsCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'cash-accounts');
	DECLARE @warehousesCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'warehouses');
	DECLARE @foreign_importsCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'foreign-imports');
	DECLARE @foreign_exportsCD INT = (SELECT [Id] FROM dbo.[ContractDefinitions] WHERE [Code] = N'foreign-exports');

