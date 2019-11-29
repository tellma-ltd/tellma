	DECLARE @employees dbo.[AgentList];

	INSERT INTO @employees
	([Index],	[AgentId],		[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate], [OperatingSegmentId]) VALUES
	(0,			@MohamadAkra,	'2017.10.01',	N'E1',	7000,			1750,						0,				@OS_BananIT),
	(1,			@AhmadAkra,		'2017.10.01',	N'E2',	7000,			0,							0,				@OS_BananIT),
	(2,			@YisakFikadu,	'2019.09.01',	N'E3',	4700,			0,							28.25,			@OS_BananIT),
	(3,			@BadegeKebede,	'2019.09.01',	N'E3',	4700,			0,							28.25,			@OS_BananIT),
	(4,			@TizitaNigussie,'2019.09.01',	N'E3',	4700,			0,							28.25,			@OS_BananIT),
	(5,			@Ashenafi,		'2019.09.01',	N'E3',	4700,			0,							28.25,			@OS_BananIT),
	(6,			@MesfinWolde,	'2019.09.01',	N'E3',	4700,			0,							28.25,			@OS_BananIT)
	;

	EXEC [api].[Agents__Save]
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
		AR.[BasicSalary], AR.[TransportationAllowance], AR.[OvertimeRate] AS 'Day Overtime Rate', RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'employees', NULL) A
		JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;