	DECLARE @Customers dbo.[AgentRelationList];

	INSERT INTO @Customers
	([Index],	[AgentId],	[StartDate], [OperatingSegmentId]) VALUES
	(0,			@Paint,		'2017.09.15', @OS_BananIT),
	(1,			@Plastic,	'2017.10.25', @OS_BananIT),
	(2,			@WaliaSteel,'2018.01.05', @OS_BananIT),
	(3,			@Lifan,		'2017.10.25', @OS_BananIT);

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
		--AR.[CustomerRating], AR.[BillingAddress], AR.[ShippingAddress], AR.[CreditLine],
		RC.[Name] AS OperatingSegment
		FROM dbo.Agents A
		JOIN dbo.AgentRelations AR ON A.[Id] = AR.[AgentId] AND AR.[DefinitionId] = N'customers'
		JOIN dbo.ResponsibilityCenters RC ON AR.OperatingSegmentId = RC.Id;		