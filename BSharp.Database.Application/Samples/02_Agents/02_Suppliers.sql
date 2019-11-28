	DECLARE @Suppliers dbo.[AgentRelationList];

	INSERT INTO @Suppliers
	([Index],	[AgentId],	[StartDate],	[OperatingSegmentId]) VALUES
	(0,			@BananIT,	'2017.09.15',	@OS_WSI),
	(1,			@Regus,		'2018.01.05',	@OS_BananIT),
	(2,			@NocJimma,	'2018.03.11',	@OS_WSI),
	(3,			@Toyota,	'2019.03.19',	@OS_WSI);

	EXEC [api].[AgentRelations__Save]
		@DefinitionId = N'suppliers',
		@Entities = @Suppliers,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Suppliers: Inserting'
		GOTO Err_Label;
	END;

	IF @DebugAgents = 1
		SELECT AR.[Code], A.[Name], AR.[StartDate] AS 'Supplier Since', AR.[IsActive],
		--AR.[SupplierRating], AR.[PaymentTerms], 
		RC.[Name] AS OperatingSegment
		FROM dbo.Agents A
		JOIN dbo.AgentRelations AR ON A.[Id] = AR.[AgentId] AND AR.[DefinitionId] = N'suppliers'
		JOIN dbo.ResponsibilityCenters RC ON AR.OperatingSegmentId = RC.Id;
