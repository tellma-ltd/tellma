--DECLARE @AccountDefinitions dbo.AccountDefinitionList;
INSERT INTO dbo.AccountDefinitions([Id],[ShowOCE],[IsCenterMapped],[IsCurrencyMapped],
[IsContractMapped], [IsResourceMapped], [IsEntryTypeMapped],
				[Code],						[Name],								[EntryTypeParentId]) VALUES
(1,1,0,0,0,0,0,N'general-BS',				N'General B/S account',					NULL),
(2,0,0,0,0,0,0,N'general-PL',				N'General P/L account',					NULL),
(3,1,0,0,1,0,0,N'cash',					N'Cash account',						NULL),
(4,1,0,0,0,1,0,N'inventory',				N'Inventory account',					NULL),
(5,1,0,0,1,0,0,N'in-transit',				N'Inventory In Transit account',		NULL),
(6,1,0,0,0,1,0,N'in-progress',			N'Work In Progress account',			NULL),
(7,1,0,0,0,1,0,N'ppe',					N'Fixed asset account',					NULL),
(8,1,0,0,1,0,0,N'supplier',				N'Supplier account',					NULL),
(9,1,0,0,1,0,0,N'customer',				N'Customer account',					NULL),
(10,1,0,0,1,0,0,N'employee',				N'Employee account',					NULL),
(11,1,0,0,1,0,0,N'creditor',				N'Creditor account',					NULL),
(12,1,0,0,1,0,0,N'debtor',				N'Debtor account',						NULL),
(13,0,0,0,0,1,0,N'quant-emp-expense',		N'Quantifieable Emp. Expense',			NULL),
(14,0,0,0,0,1,0,N'revenue',				N'Revenue account',						NULL),
(15,0,0,0,0,1,0,N'depreciation-expense',	N'Depreciation Expense account',		NULL),
(16,0,0,0,0,1,0,N'COS',					N'Cost of Sales account',				NULL),
(17,1,0,0,1,0,0,N'partner',				N'Partner account',						NULL);


INSERT INTO dbo.AccountDefinitionCenterTypes
(AccountDefinitionId, CenterType) VALUES
(1,					N'Investment'),
(2,					N'Investment'),
(2,					N'Cost'),
(3,					N'Investment'),
(4,					N'Investment'),
(5,					N'Investment'),
(6,					N'Investment'),
(7,					N'Cost'),
(7,					N'Profit'),
(8,					N'Investment'),
(9,					N'Investment'),
(10,				N'Investment'),
(11,				N'Investment'),
(12,				N'Investment'),
(13,				N'Cost'),
(14,				N'Profit'),
(15,				N'Cost'),
(16,				N'Profit'),
(17,					N'Investment');
INSERT INTO dbo.AccountDefinitionCurrencies
(AccountDefinitionId, [CurrencyId]) VALUES
(1,						N'USD'),
(1,						N'SDG'),
(2,						N'USD'),
(2,						N'SDG'),
(3,						N'USD'),
(3,						N'SDG'),
(3,						N'SAR'),
(3,						N'AED'),
(4,						N'USD'),
(5,						N'USD'),
(6,						N'USD'),
(7,						N'USD'),
(8,						N'USD'),
(8,						N'SDG'),
(9,						N'USD'),
(9,						N'SDG'),
(10,					N'USD'),
(10,					N'SDG'),
(11,					N'USD'),
(11,					N'SDG'),
(12,					N'USD'),
(12,					N'SDG'),
(13,					N'USD'),
(13,					N'SDG'),
(14,					N'USD'),
(14,					N'SDG'),
(15,					N'USD'),
(16,					N'USD'),
(16,					N'SDG'),
(17,					N'USD');
INSERT INTO dbo.AccountDefinitionContractDefinitions
(AccountDefinitionId, [ContractDefinitionId]) VALUES
(3					, @cash_custodiansDef),
--(4					, @inventory_custodiansDef),
(8					, @suppliersDef),
(9					, @customersDef),
(10					, @employeesDef),
(11					, @creditorsDef),
(12					, @debtorsDef),
(17					, @partnersDef);
INSERT INTO dbo.AccountDefinitionResourceDefinitions
(AccountDefinitionId, [ResourceDefinitionId]) VALUES
(4					, @inventoriesDef),
(7					, @properties_plants_and_equipmentDef),
(7					, @computer_equipmentDef),
(4					, @inventoriesDef),
(4					, @inventoriesDef),
(13					, @employee_benefits_expensesDef),
(14					, @revenue_servicesDef),
(15					, @properties_plants_and_equipmentDef),
(15					, @computer_equipmentDef),
(16					, @revenue_servicesDef);

DECLARE @general_BSADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'general-BS')
DECLARE @general_PLADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'general-PL')
DECLARE @cashADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'cash')
DECLARE @inventoryADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'inventory')
DECLARE @in_transitADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'in-transit')
DECLARE @in_progressADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'in-progress')
DECLARE @ppeADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'ppe')
DECLARE @supplierADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'supplier')
DECLARE @customerADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'customer')
DECLARE @employeeADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'employee')
DECLARE @creditorADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'creditor')
DECLARE @debtorADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'debtor')
DECLARE @quant_emp_expenseADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'quant-emp-expense')
DECLARE @revenueADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'revenue')
DECLARE @depreciation_expenseADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'depreciation-expense')
DECLARE @COSADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'COS')
DECLARE @partnerADef INT = (SELECT [Id] FROM dbo.AccountDefinitions WHERE [Code] = N'partner')
