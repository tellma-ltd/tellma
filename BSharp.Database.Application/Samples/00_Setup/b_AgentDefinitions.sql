	INSERT INTO dbo.AgentDefinitions
	([Id],				[TitleSingular],	[TitlePlural]) VALUES
	(N'cost-units',		N'Cost Unit',		N'Cost Units'),
	(N'cost-centers',	N'Cost Center',		N'Cost Centers'),
	(N'creditors',		N'Creditor',		N'Creditors'),
	(N'customers',		N'Customer',		N'Customers'),
	(N'debtors',		N'Debtor',			N'Debtors'),
	(N'owners',			N'Owner',			N'Owners'),
	(N'suppliers',		N'Supplier',		N'Suppliers'),
	(N'tax-agencies',	N'Tax Agency',		N'Tax Agencies');

	INSERT INTO dbo.AgentDefinitions
	([Id],				[TitleSingular],	[TitlePlural],	[JobVisibility], [BasicSalaryVisibility], [TransportationAllowanceVisibility], [OvertimeRateVisibility]) VALUES
	(N'employees',		N'Employee',		N'Employees',	N'VisibleAndRequired',N'VisibleAndRequired',	N'VisibleAndRequired',				N'VisibleAndRequired');