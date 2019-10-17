	INSERT INTO dbo.AgentRelationDefinitions([Id], [SingularLabel], [PluralLabel], [Prefix]) VALUES
	(N'customers', N'Customer', N'Customers', N'C');

	DECLARE @Customers dbo.[AgentRelationList];

	INSERT INTO @Customers
	([Index],	[AgentId],	[StartDate], [CreditLine]) VALUES
	(0,			@Paint,		'2017.09.15', 100000),
	(1,			@Plastic,	'2017.10.25', 50000),
	(2,			@CBE,		'2018.01.05', 0);;

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
		AR.[CustomerRating], AR.[BillingAddress], AR.[ShippingAddress], AR.[CreditLine]
		FROM dbo.Agents A
		JOIN dbo.AgentRelations AR
		ON A.[Id] = AR.[AgentId]
		AND AR.AgentRelationDefinitionId = N'customers';