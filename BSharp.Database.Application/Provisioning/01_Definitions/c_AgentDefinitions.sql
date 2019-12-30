DECLARE @AgentDefinitions dbo.AgentDefinitionList;
IF NOT EXISTS(SELECT * FROM dbo.AgentDefinitions)
BEGIN
	INSERT INTO @AgentDefinitions([Index],
	[Id],				[TitleSingular],[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
	(0,N'cost-units',	N'Cost Unit',	N'وحدة التكلفة',	N'Cost Units',	N'وحدات التكلفة'),
	(1,N'cost-centers',	N'Cost Center',	N'مركز التكلفة',	N'Cost Centers',N'مراكز التكلفة'),
	(2,N'creditors',	N'Creditor',	N'الدائن',			N'Creditors',	N'الدائنون'),
	(3,N'customers',	N'Customer',	N'الزبون',			N'Customers',	N'الزبائن'),
	(4,N'debtors',		N'Debtor',		N'المدين',			N'Debtors',		N'المدينون'),
	(5,N'owners',		N'Owner',		N'المالك',			N'Owners',		N'المالكون'),
	(6,N'suppliers',	N'Supplier',	N'المورد',			N'Suppliers',	N'الموردون'),
	(7,N'tax-agencies',	N'Tax Agency',	N'الإدارة الضريبية',N'Tax Agencies',N'الإدارات الضريبية'),
	(8,N'banks',		N'Bank',		N'البنك',			N'Banks',		N'البنوك'),
	(9,N'custodies',	N'Custody',		N'الخزنة',			N'Custodies',	N'الخزائن')
	;

	-- TODO: depends on country
	INSERT INTO @AgentDefinitions([Index],
	[Id],			[TitleSingular],[TitleSingular2], [TitlePlural], [TitlePlural2],	[JobVisibility], [BasicSalaryVisibility], [TransportationAllowanceVisibility], [OvertimeRateVisibility]) VALUES
	(10,N'employees',	N'Employee',	N'الموظف',		N'Employees',	N'الموظفون',	N'Required',	 N'Required',			  N'Required',							N'Required');
END

EXEC dal.AgentDefinitions__Save
	@Entities = @AgentDefinitions

IF @DebugAgentDefinitions = 1
	SELECT * FROM dbo.LookupDefinitions;