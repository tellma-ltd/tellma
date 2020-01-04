	DECLARE @Customers dbo.[AgentList];
	DECLARE @Paint int, @Plastic int, @WaliaSteel int, @Lifan int;

IF @DB = N'100' -- ACME, USD, en/ar/zh		
	INSERT INTO @Customers
	([Index],	[Name]) VALUES
	(0,			N'Customer1'),
	(1,			N'Customer2'),
	(2,			N'Customer3'),
	(3,			N'Customer4');
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Customers
	([Index],	[Name],									[StartDate]) VALUES
	(0,			N'International African University',	NULL),
	(1,			N'Mico poultry',						NULL),
	(2,			N'Sabco',								NULL),
	(3,			N'al-Washm',							NULL),
	(4,			N'TAGI restaurants',					NULL);
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Customers
	([Index],	[Name],						[StartDate], [TaxIdentificationNumber]) VALUES
	(0,			N'Best Paint Industry',		'2017.09.15',	N'0000021411'),
	(1,			N'Best Plastic Industry',	'2017.10.25',	N'0000021411'),
	(2,			N'Walia Steel Industry, plc','2018.01.05',	N'0001656462'),
	(3,			N'Yangfan Motors, PLC',		'2017.10.25',	N'0005308731');

ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
	INSERT INTO @Customers
	([Index],	[Name],						[StartDate]) VALUES
	(0,			N'Wendy Semaneh',			'2017.09.15');

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Customers
	([Index],	[Name],								[StartDate], [TaxIdentificationNumber]) VALUES
	(0,			N'3F Finfine Furniture Factory',	'2017.09.15', N'0000007551'),
	(1,			N'4 Good Management Consultant PLC','2017.10.25', N'0045782603'),
	(2,			N'A.M.M METAL',						'2018.01.05', N'0045000771'),
	(3,			N'A.R.C',							'2017.10.25', N'0023353621');

	EXEC [api].[Agents__Save]
		@DefinitionId = N'customers',
		@Entities = @Customers,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Customers: Inserting'
		GOTO Err_Label;
	END;
	SELECT
		@WaliaSteel = (SELECT [Id] FROM [dbo].fi_Agents(N'customers', NULL) WHERE [Name] = N'Walia Steel Industry, plc'),
		@Paint = (SELECT [Id] FROM [dbo].fi_Agents(N'customers', NULL) WHERE [Name] = N'Best Paint Industry'),
		@Plastic = (SELECT [Id] FROM [dbo].fi_Agents(N'customers', NULL) WHERE [Name] = N'Best Plastic Industry'),
		@Lifan = (SELECT [Id] FROM [dbo].fi_Agents(N'customers', NULL) WHERE [Name] = N'Yangfan Motors, PLC');

	IF @DebugCustomers = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Customer Since', A.[IsActive]
		--A.[CustomerRating], A.[BillingAddress], A.[ShippingAddress], A.[CreditLine],
		--RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'customers', NULL) A
		--JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;