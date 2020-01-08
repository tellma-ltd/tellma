	DECLARE @TaxAgencies dbo.[AgentList];
	DECLARE @VAT int, @BPT INT, @EIT INT, @WT int, @Pension INT;

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @TaxAgencies
	([Index], [Name]) VALUES
	(0,		N'VAT Dept'),
	(1,		N'Income Tax Dept'),
	(2,		N'Employee Income Tax Dept'),
	(3,		N'Employee Pension Dept');
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @TaxAgencies
	([Index], [Name]) VALUES
	(0,		N'VAT Dept'),
	(1,		N'Income Tax Dept'),
	(2,		N'Employee Income Tax Dept'),
	(3,		N'Employee Pension Dept');
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @TaxAgencies
	([Index], [Name]) VALUES
	(0,		N'VAT Dept'),
	(1,		N'Income Tax Dept'),
	(2,		N'Employee Income Tax Dept'),
	(3,		N'Employee Pension Dept'),
	(4,		N'WT Dept');
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
	INSERT INTO @TaxAgencies
	([Index], [Name]) VALUES
	(0,		N'VAT Dept'),
	(1,		N'Income Tax Dept'),
	(2,		N'Employee Income Tax Dept'),
	(3,		N'Employee Pension Dept'),
	(4,		N'WT Dept');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @TaxAgencies
	([Index], [Name]) VALUES
	(0,		N'VAT Dept'),
	(1,		N'Income Tax Dept'),
	(2,		N'Employee Income Tax Dept'),
	(3,		N'Employee Pension Dept'),
	(4,		N'WT Dept');

	EXEC [api].[Agents__Save]
		@DefinitionId = N'tax-agencies',
		@Entities = @TaxAgencies,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Tax Agencies: Inserting'
		GOTO Err_Label;
	END;
	SELECT
		@VAT = (SELECT [Id] FROM [dbo].fi_Agents(N'tax-agencies', NULL) WHERE [Name] = N'VAT Dept'),
		@BPT = (SELECT [Id] FROM [dbo].fi_Agents(N'tax-agencies', NULL) WHERE [Name] = N'Income Tax Dept'),
		@EIT = (SELECT [Id] FROM [dbo].fi_Agents(N'tax-agencies', NULL) WHERE [Name] = N'Employee Income Tax Dept'),
		@Pension =  (SELECT [Id] FROM [dbo].fi_Agents(N'tax-agencies', NULL) WHERE [Name] = N'Employee Pension Dept'),
		@WT =  (SELECT [Id] FROM [dbo].fi_Agents(N'tax-agencies', NULL) WHERE [Name] = N'WT Dept');

	IF @DebugTaxAgencies = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Supplier Since', A.[IsActive]
		--AR.[SupplierRating], AR.[PaymentTerms], 
		--RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'TaxAgencies', NULL) A
		--JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;