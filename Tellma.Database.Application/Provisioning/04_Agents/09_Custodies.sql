	DECLARE @Custodies dbo.[AgentList];
	DECLARE @Warehouse_RM int, @Warehouse_FG int;

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Custodies
	([Index], [Name]) VALUES
	(0,		N'RM Warehouse'),
	(1,		N'FG Warehouse'),
	(2,		N'Cashier');
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @Custodies
	([Index],	[Name],				[Code]) VALUES
	(0,			N'elAmin alTayyib', N'GM');
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @Custodies
	([Index], [Name]) VALUES
	(0,		N'GM Petty Cash');
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
	INSERT INTO @Custodies
	([Index], [Name]) VALUES
	(0,		N'RM Warehouse'),
	(1,		N'FG Warehouse'),
	(2,		N'Cashier');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @Custodies
	([Index], [Name]) VALUES
	(0,		N'RM Warehouse'),
	(1,		N'FG Warehouse'),
	(2,		N'Cashier');

	EXEC [api].[Agents__Save]
		@DefinitionId = N'custodies',
		@Entities = @Custodies,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Custodies: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	SELECT
		@Warehouse_RM = (SELECT [Id] FROM [dbo].fi_Agents(N'custodies', NULL) WHERE [Name] = N'RM Warehouse'),
		@Warehouse_FG = (SELECT [Id] FROM [dbo].fi_Agents(N'custodies', NULL) WHERE [Name] = N'FG Warehouse');

	IF @DebugCustodies = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Custody Since', A.[IsActive]
		--AR.[SupplierRating], AR.[PaymentTerms], 
		--RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'custodies', NULL) A
		--JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;