	INSERT INTO dbo.AgentRelationDefinitions([Id], [SingularLabel], [PluralLabel], [Prefix]) VALUES
	(N'employees', N'Employee', N'Employees', N'E');

	DECLARE @employees dbo.[AgentRelationList];

	INSERT INTO @employees
	([Index],	[AgentId],		[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate]) VALUES
	(0,			@MohamadAkra,	'2017.10.01',	N'E1',	7000,			1750,						0),
	(1,			@AhmadAkra,		'2017.10.01',	N'E2',	7000,			0,							0),
	(2,			@YisakFikadu,	'2019.09.01',	N'E3',	4700,			0,							28.25);

	EXEC [api].[AgentRelations__Save]
		@DefinitionId = N'employees',
		@Entities = @employees,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Employees: Inserting'
		GOTO Err_Label;
	END;

	IF @DebugAgents = 1
		SELECT AR.[Code], A.[Name], AR.[StartDate] AS 'Employee Since', AR.[IsActive],
		AR.[BasicSalary], AR.[TransportationAllowance], AR.[OvertimeRate] AS 'Day Overtime Rate'
		FROM dbo.Agents A
		JOIN dbo.AgentRelations AR
		ON A.[Id] = AR.[AgentId]
		AND AR.AgentRelationDefinitionId = N'employees';