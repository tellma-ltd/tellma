	INSERT INTO dbo.AgentRelationDefinitions([Id], [SingularLabel], [PluralLabel], [Prefix]) VALUES
	(N'customers', N'Customer', N'Customers', N'C');

	DECLARE @Customers dbo.[AgentRelationList];

	INSERT INTO @Customers
	([Index],	[AgentId],	[StartDate], [CreditLine], [OperatingSegmentId]) VALUES
	(0,			@Paint,		'2017.09.15', 150000,		@OS_BananIT),
	(1,			@Plastic,	'2017.10.25', 6000,			@OS_BananIT),
	(2,			@WaliaSteel,'2018.01.05', 60000,		@OS_BananIT),
	(3,			@Lifan,		'2017.10.25',	0,			@OS_BananIT);

	EXEC [api].[AgentRelations__Save]
		@DefinitionId = N'customers',
		@Entities = @Customers,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Customers: Inserting'
		GOTO Err_Label;
	END;

	IF @DebugAgents = 1
		SELECT AR.[Code], A.[Name], AR.[StartDate] AS 'Customer Since', AR.[IsActive],
		AR.[CustomerRating], AR.[BillingAddress], AR.[ShippingAddress], AR.[CreditLine], RC.[Name] AS OperatingSegment
		FROM dbo.Agents A
		JOIN dbo.AgentRelations AR ON A.[Id] = AR.[AgentId] AND AR.AgentRelationDefinitionId = N'customers'
		JOIN dbo.ResponsibilityCenters RC ON AR.OperatingSegmentId = RC.Id;		