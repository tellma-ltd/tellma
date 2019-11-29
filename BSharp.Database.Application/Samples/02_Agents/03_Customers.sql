	DECLARE @Customers dbo.[AgentList];
	DECLARE @Paint int, @Plastic int, @WaliaSteel int, @Lifan int;
		
	INSERT INTO @Customers
	([Index],	[Name],						[StartDate], [OperatingSegmentId]) VALUES
	(0,			N'Best Paint Industry',		'2017.09.15', @OS_BananIT),
	(1,			N'Best Plastic Industry',	'2017.10.25', @OS_BananIT),
	(2,			N'Walia Steel Industry, plc','2018.01.05', @OS_BananIT),
	(3,			N'Yangfan Motors, PLC',		'2017.10.25', @OS_BananIT);

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
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Customer Since', A.[IsActive],
		--A.[CustomerRating], A.[BillingAddress], A.[ShippingAddress], A.[CreditLine],
		RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'customers', NULL) A
		JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;