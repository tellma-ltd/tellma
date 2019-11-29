	INSERT INTO dbo.AgentDefinitions
	([Id],				[TitleSingular],	[TitlePlural],	[Prefix]) VALUES
	(N'cost-units',		N'Cost Unit',		N'Cost Units',	N'CU'),
	(N'cost-centers',	N'Cost Center',		N'Cost Centers',N'CC'),
	(N'creditors',		N'Creditor',		N'Creditors',	N'CR'),
	(N'customers',		N'Customer',		N'Customers',	N'C'),
	(N'debtors',		N'Debtor',			N'Debtors',		N'DR'),
	(N'owners',			N'Owner',			N'Owners',		N'O'),
	(N'suppliers',		N'Supplier',		N'Suppliers',	N'P'),
	(N'tax-agencies',	N'Tax Agency',		N'Tax Agencies',N'TX');

	INSERT INTO dbo.AgentDefinitions
	([Id],				[TitleSingular],	[TitlePlural],	[Prefix], [JobTitleVisibility], [BasicSalaryVisibility], [TransportationAllowanceVisibility], [OvertimeRateVisibility]) VALUES
	(N'employees',		N'Employee',		N'Employees',	N'E',		N'VisibleAndRequired',N'VisibleAndRequired',	N'VisibleAndRequired',				N'VisibleAndRequired');