	INSERT INTO dbo.AgentRelationDefinitions([Id], [SingularLabel], [PluralLabel], [Prefix]) VALUES
	(N'suppliers', N'Supplier', N'Suppliers', N'P');

	DECLARE @Suppliers dbo.[AgentRelationList];

	INSERT INTO @Suppliers
	([Index],	[AgentId],	[StartDate]) VALUES
	(0,			@BananIT,	'2017.09.15'),
	(1,			@Lifan,		'2017.10.25'),
	(2,			@Regus,		'2018.01.05'),
	(3,			@NocJimma,	'2018.03.11'),
	(4,			@Toyota,	'2019.03.19');

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
		AR.[SupplierRating], AR.[PaymentTerms]
		FROM dbo.Agents A
		JOIN dbo.AgentRelations AR
		ON A.[Id] = AR.[AgentId]
		AND AR.AgentRelationDefinitionId = N'suppliers';