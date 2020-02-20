DECLARE @AgentDefinitions dbo.AgentDefinitionList;
IF NOT EXISTS(SELECT * FROM dbo.AgentDefinitions)
BEGIN
	IF @DB = N'100' -- ACME, USD, en/ar/zh playground
	BEGIN
		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitleSingular2],	[TitleSingular3], [TitlePlural],[TitlePlural2],		[TitlePlural3]) VALUES
		(0,N'cost-units',	N'Cost Unit',	N'وحدة التكلفة',	N'成本单位',		N'Cost Units',	N'وحدات التكلفة',	N'成本单位'),
		(1,N'cost-centers',	N'Cost Center',	N'مركز التكلفة',	N'成本中心',		N'Cost Centers',N'مراكز التكلفة',	N'成本中心'),
		(2,N'creditors',	N'Creditor',	N'الدائن',			N'债权人',		N'Creditors',	N'الدائنون',		N'债权人'),
		(3,N'customers',	N'Customer',	N'الزبون',			N'顾客',			N'Customers',	N'الزبائن',			N'顾客'),
		(4,N'debtors',		N'Debtor',		N'المدين',			N'债务人',		N'Debtors',		N'المدينون',		N'债务人'),
		(5,N'owners',		N'Owner',		N'المالك',			N'所有者',		N'Owners',		N'المالكون',		N'拥有者'),
		(6,N'suppliers',	N'Supplier',	N'المورد',			N'供应商',		N'Suppliers',	N'الموردون',		N'供应商'),
		(7,N'tax-agencies',	N'Tax Agency',	N'الإدارة الضريبية',N'税务局',		N'Tax Agencies',N'الإدارات الضريبية',N'税务机关'),
		(8,N'banks',		N'Bank',		N'البنك',			N'银行',			N'Banks',		N'البنوك',			N'银行业务'),
		(9,N'custodies',	N'Custody',		N'الخزنة',			N'保管',			N'Custodies',	N'الخزائن',			N'保管人'),
		(10,N'employees',	N'Employee',	N'الموظف',			N'雇员',			N'Employees',	N'الموظفون',		N'雇员');

	END
	ELSE IF @DB = N'101' -- Banan SD, USD, en
	BEGIN
		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitlePlural]) VALUES
		(0,N'cost-units',	N'Cost Unit',	N'Cost Units'),
		(1,N'cost-centers',	N'Cost Center',	N'Cost Centers'),
		(2,N'creditors',	N'Creditor',	N'Creditors'),
		(3,N'customers',	N'Customer',	N'Customers'),
		(4,N'debtors',		N'Debtor',		N'Debtors'),
		(5,N'partners',		N'Partner',		N'Partners'),
		(6,N'suppliers',	N'Supplier',	N'Suppliers'),
		--(7,N'tax-agencies',	N'Tax Agency',	N'Tax Agencies'),
		(8,N'banks',		N'Bank',		N'Banks'),
		(9,N'custodies',	N'Custody',		N'Custodies')	;

		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],	[TitlePlural],	[JobVisibility], [RatesVisibility], [RatesLabel]) VALUES
		(10,N'employees',	N'Employee',		N'Employees',	N'Optional',	N'Optional', N'Remuneration');
	END
	ELSE IF @DB = N'102' -- Banan ET, ETB, en
	BEGIN
		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitlePlural], [TaxIdentificationNumberVisibility]) VALUES
		(0,N'cost-units',	N'Cost Unit',	N'Cost Units',  NULL),
		(1,N'cost-centers',	N'Cost Center',	N'Cost Centers',  NULL),
		(2,N'creditors',	N'Creditor',	N'Creditors',  NULL),
		(3,N'customers',	N'Customer',	N'Customers',  N'Optional'),
		(4,N'debtors',		N'Debtor',		N'Debtors',  NULL),
--		(5,N'owners',		N'Owner',		N'Owners',  NULL),
		(6,N'suppliers',	N'Supplier',	N'Suppliers',  N'Optional'),
		(7,N'tax-agencies',	N'Tax Agency',	N'Tax Agencies',  NULL),
		(8,N'banks',		N'Bank',		N'Banks',  NULL),
		(9,N'custodies',	N'Custody',		N'Custodies',  NULL);

		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitlePlural],	[TaxIdentificationNumberVisibility],	[JobVisibility]) VALUES
		(10,N'employees',	N'Employee',	N'Employees',	N'Optional',							N'Required');
	END
	ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh car service
	BEGIN
	INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitleSingular2],[TitlePlural],		[TitlePlural2]) VALUES
		(0,N'cost-units',	N'Cost Unit',	N'成本单位',		N'Cost Units',		N'成本单位'),
		(1,N'cost-centers',	N'Cost Center',	N'成本中心',		N'Cost Centers',	N'成本中心'),
		(2,N'creditors',	N'Creditor',	N'债权人',		N'Creditors',		N'债权人'),
		(3,N'customers',	N'Customer',	N'顾客',			N'Customers',		N'顾客'),
		(4,N'debtors',		N'Debtor',		N'债务人',		N'Debtors',			N'债务人'),
--		(5,N'owners',		N'Owner',		N'所有者',		N'Owners',			N'拥有者'),
		(6,N'suppliers',	N'Supplier',	N'供应商',		N'Suppliers',		N'供应商'),
		(7,N'tax-agencies',	N'Tax Agency',	N'税务局',		N'Tax Agencies',	N'税务机关'),
		(8,N'banks',		N'Bank',		N'银行',			N'Banks',			N'银行业务'),
		(9,N'custodies',	N'Custody',		N'保管',			N'Custodies',		N'保管人');

		INSERT INTO @AgentDefinitions([Index],
		[Id],			[TitleSingular],[TitleSingular2], [TitlePlural], [TitlePlural2],[TaxIdentificationNumberVisibility],
		[JobVisibility]) VALUES
		(10,N'employees',	N'Employee',	N'雇员',			N'Employees',	N'雇员',		N'Optional',
		N'Optional');
	END
	ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am manyfacturing and sales
	BEGIN
		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
		(0,N'cost-units',	N'Cost Unit',	N'وحدة التكلفة',	N'Cost Units',	N'وحدات التكلفة'),
		(1,N'cost-centers',	N'Cost Center',	N'مركز التكلفة',	N'Cost Centers',N'مراكز التكلفة'),
		(2,N'creditors',	N'Creditor',	N'الدائن',			N'Creditors',	N'الدائنون'),
		(3,N'customers',	N'Customer',	N'الزبون',			N'Customers',	N'الزبائن'),
		(4,N'debtors',		N'Debtor',		N'المدين',			N'Debtors',		N'المدينون'),
	--	(5,N'owners',		N'Owner',		N'المالك',			N'Owners',		N'المالكون'),
		(6,N'suppliers',	N'Supplier',	N'المورد',			N'Suppliers',	N'الموردون'),
	--	(7,N'tax-agencies',	N'Tax Agency',	N'الإدارة الضريبية',N'Tax Agencies',N'الإدارات الضريبية'),
		(8,N'banks',		N'Bank',		N'البنك',			N'Banks',		N'البنوك'),
		(9,N'custodies',	N'Custody',		N'الخزنة',			N'Custodies',	N'الخزائن')	;

		INSERT INTO @AgentDefinitions([Index],
		[Id],			[TitleSingular],[TitleSingular2], [TitlePlural], [TitlePlural2], [TaxIdentificationNumberVisibility],
		[JobVisibility]) VALUES
		(10,N'employees',	N'Employee',	N'الموظف',		N'Employees',	N'الموظفون',N'Optional',		
		N'Required');
	END
	ELSE IF @DB = N'105' -- Simpex, SAR, en/ar trading
	BEGIN
		INSERT INTO @AgentDefinitions([Index],
		[Id],				[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2],[MainMenuSection]) VALUES
		(0,N'cost-units',	N'Cost Unit',	N'وحدة التكلفة',	N'Cost Units',	N'وحدات التكلفة', N'Financials'),
		(1,N'cost-centers',	N'Cost Center',	N'مركز التكلفة',	N'Cost Centers',N'مراكز التكلفة', N'Financials'),
		(2,N'creditors',	N'Creditor',	N'الدائن',			N'Creditors',	N'الدائنون', N'Financials'),
		(3,N'customers',	N'Customer',	N'الزبون',			N'Customers',	N'الزبائن', N'Financials'),
		(4,N'debtors',		N'Debtor',		N'المدين',			N'Debtors',		N'المدينون', N'Financials'),
	--	(5,N'owners',		N'Owner',		N'المالك',			N'Owners',		N'المالكون'),
		(6,N'suppliers',	N'Supplier',	N'المورد',			N'Suppliers',	N'الموردون', N'Financials'),
	--	(7,N'tax-agencies',	N'Tax Agency',	N'الإدارة الضريبية',N'Tax Agencies',N'الإدارات الضريبية', N'Financials'),
		(8,N'banks',		N'Bank',		N'البنك',			N'Banks',		N'البنوك', N'Financials'),
		(9,N'custodies',	N'Custody',		N'الخزنة',			N'Custodies',	N'الخزائن', N'Financials'),	
		(10,N'warehouses',	N'Warehouse',	N'المخزن',			N'Warehouses',	N'المخازن', N'Financials')
		;
		INSERT INTO @AgentDefinitions([Index],
		[Id],			[TitleSingular],[TitleSingular2], [TitlePlural], [TitlePlural2],	[JobVisibility]) VALUES
		(11,N'employees',	N'Employee',	N'الموظف',		N'Employees',	N'الموظفون',	N'Required');
	END
END

EXEC [api].[AgentDefinitions__Save]
	@Entities = @AgentDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'AgentDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

IF @DebugAgentDefinitions = 1
	SELECT * FROM dbo.AgentDefinitions;