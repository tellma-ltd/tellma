	DECLARE @Suppliers dbo.[AgentList];
	DECLARE @BananIT int, @Regus int, @NocJimma INT, @Toyota INT;
	INSERT INTO @Suppliers
	([Index], [Name],								[StartDate],	[OperatingSegmentId], [TaxIdentificationNumber]) VALUES
	(0,		N'Banan Information technologies, plc',	'2017.09.15',	@OS_WSI,				NULL),
	(1,		N'Regus',								'2018.01.05',	@OS_BananIT,			N'4544287'),
	(2,		N'Noc Jimma Ber Service Station',		'2018.03.11',	@OS_WSI,				NULL),
	(3,		N'Toyota, Ethiopia',					'2019.03.19',	@OS_WSI,				NULL);

	EXEC [api].[Agents__Save]
		@DefinitionId = N'suppliers',
		@Entities = @Suppliers,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Suppliers: Inserting'
		GOTO Err_Label;
	END;
	SELECT
		@BananIT = (SELECT [Id] FROM [dbo].fi_Agents(N'suppliers', NULL) WHERE [Name] = N'Banan Information technologies, plc'),
		@Regus = (SELECT [Id] FROM [dbo].fi_Agents(N'suppliers', NULL) WHERE [Name] = N'Regus'),
		@NocJimma = (SELECT [Id] FROM [dbo].fi_Agents(N'suppliers', NULL) WHERE [Name] = N'Noc Jimma Ber Service Station'),
		@Toyota =  (SELECT [Id] FROM [dbo].fi_Agents(N'suppliers', NULL) WHERE [Name] = N'Toyota, Ethiopia');

	IF @DebugSuppliers = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Supplier Since', A.[IsActive],
		--AR.[SupplierRating], AR.[PaymentTerms], 
		RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'suppliers', NULL) A
		JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;