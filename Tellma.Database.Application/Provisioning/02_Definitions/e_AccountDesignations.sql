--DECLARE @AccountDesignations dbo.AccountDesignationList;
	-- 0 Set Value, 1 By Contract, 2 By Resource, 3 By Center
	-- 21: By Resource Lookup1 22: By Resource Lookup1 and Contract Id
INSERT INTO dbo.[AccountDesignations]([Id],[ShowOCE],[MapFunction],
	[Code],						[Name]) VALUES
(1,1,-1,N'general-BS',			N'General B/S account'),
(2,0,-1,N'general-PL',			N'General P/L account'),
(3,1,1,N'cash',					N'Cash account'),
(4,1,22,N'inventory',			N'Inventory account'),
(5,1,1,N'in-transit',			N'Inventory In Transit account'),
(6,1,0,N'in-progress',			N'Work In Progress account'),
(7,1,22,N'ppe',					N'Fixed asset account'),
(8,1,1,N'supplier',				N'Supplier account'),
(9,1,1,N'customer',				N'Customer account'),
(10,1,1,N'employee',			N'Employee account'),
(11,1,1,N'creditor',			N'Creditor account'),
(12,1,1,N'debtor',				N'Debtor account'),
(13,0,1,N'employee-bonus',		N'Employee Bonus'),
(14,0,0,N'revenue',				N'Revenue account'),
(15,0,0,N'depreciation-expense',N'Depreciation Expense account'),
(16,0,0,N'COS',					N'Cost of Sales account'),
(17,1,1,N'partner',				N'Partner account'),
(18,1,0,N'vat-payable',			N'VAT payable account'),
(19,1,0,N'vat-receivable',		N'VAT receivable account'),
(20,1,3,N'purchase-expense',	N'Purchase expenses account'),-- materials and services
(21,1,0,N'document-control',	N'Document control account'),
(22,1,0,N'employee-income-tax',	N'Employee Income Tax account'),
(23,1,0,N'employee-stamp-tax',	N'Employee Stamp Tax account'),
(24,1,0,N'exchange-gain-loss',	N'Exchange Loss (Gain) account'),
(25,1,0,N'exchange-variance',	N'Exchange Variance account')
;

INSERT INTO dbo.[AccountDesignationContractDefinitions]
([AccountDesignationId], [ContractDefinitionId]) VALUES
(3					, @cash_custodiansDef),
(4					, @inventory_custodiansDef),
(8					, @suppliersDef),
(9					, @customersDef),
(10					, @employeesDef),
(11					, @creditorsDef),
(12					, @debtorsDef),
(17					, @partnersDef);
INSERT INTO dbo.[AccountDesignationResourceDefinitions]
([AccountDesignationId], [ResourceDefinitionId]) VALUES
(4					, @inventoriesDef),
(7					, @properties_plants_and_equipmentDef),
(7					, @computer_equipmentDef),
(4					, @inventoriesDef),
(14					, @revenue_servicesDef),
(15					, @properties_plants_and_equipmentDef),
(15					, @computer_equipmentDef),
(16					, @revenue_servicesDef);

DECLARE @general_BSADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'general-BS')
DECLARE @general_PLADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'general-PL')
DECLARE @cashADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'cash')
DECLARE @inventoryADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'inventory')
DECLARE @in_transitADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'in-transit')
DECLARE @in_progressADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'in-progress')
DECLARE @ppeADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'ppe')
DECLARE @supplierADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'supplier')
DECLARE @customerADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'customer')
DECLARE @employeeADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'employee')
DECLARE @creditorADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'creditor')
DECLARE @debtorADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'debtor')
DECLARE @employee_bonusADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'employee-bonus')
DECLARE @revenueADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'revenue')
DECLARE @depreciation_expenseADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'depreciation-expense')
DECLARE @COSADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'COS')
DECLARE @partnerADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'partner')
DECLARE @vat_payableADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'vat-payable')
DECLARE @vat_receivableADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'vat-receivable')
DECLARE @purchase_expenseADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'purchase-expense')
DECLARE @document_controlADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'document-control')
DECLARE @eitaxADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'employee-income-tax')
DECLARE @estaxADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'employee-stamp-tax')
DECLARE @exchange_gain_lossADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'exchange-gain-loss')
DECLARE @exchange_varianceADef INT = (SELECT [Id] FROM dbo.AccountDesignations WHERE [Code] = N'exchange-variance')