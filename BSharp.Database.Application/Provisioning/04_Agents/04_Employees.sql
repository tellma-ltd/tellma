	DECLARE @employees dbo.[AgentList];
	DECLARE @MohamadAkra int, @AhmadAkra int, @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakFikadu int,
			@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
			@MesfinWolde int;;
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate]) VALUES
	(0,			N'Mohamad Akra',	'2017.10.01',	N'E1',	7000,			1750,						0),
	(1,			N'Ahmad Akra',		'2017.10.01',	N'E2',	7000,			0,							0),
	(2,			N'Yisak Fikadu',	'2019.09.01',	N'E3',	4700,			0,							28.25),
	(3,			N'Badege Kebede',	'2019.09.01',	N'E4',	4700,			0,							28.25),
	(4,			N'Tizita Nigussie',	'2019.09.01',	N'E5',	4700,			0,							28.25),
	(5,			N'Ashenafi Fantahun','2019.09.01',	N'E6',	4700,			0,							28.25),
	(6,			N'Mesfin Wolde',	'2019.09.01',	N'E7',	4700,			0,							28.25)
	;
	UPDATE @employees SET UserId = @UserId WHERE [Index] = 0;
	EXEC [api].[Agents__Save]
		@DefinitionId = N'employees',
		@Entities = @employees,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Employees: Inserting'
		GOTO Err_Label;
	END;

	SELECT 
		@MohamadAkra = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Mohamad Akra'), 
		@AhmadAkra = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Ahmad Akra'), 
		@BadegeKebede = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Badege Kebede'), 
		@TizitaNigussie = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Tizita Nigussie'), 
		@Ashenafi = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Ashenafi Fantahun'), 
		@YisakFikadu = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Yisak Fikadu'), 
		@ZewdineshHora = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Zewdinesh Hora'), 
		@TigistNegash = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Tigist Negash'), 
		@RomanZenebe = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Roman Zenebe'), 
		@Mestawet = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Mestawet G/Egziyabhare'), 
		@AyelechHora = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Ayelech Hora'), 
		@YigezuLegesse = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Yigezu Legesse'), 
		@MesfinWolde = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Mesfin Wolde');

	UPDATE dbo.ResponsibilityCenters
	SET ManagerId = CASE 
		WHEN [Id] = @OS_IT			THEN @MohamadAkra
		WHEN [Id] = @OS_Steel				THEN @BadegeKebede
		WHEN [Id] = @RC_ExecutiveOffice	THEN @BadegeKebede
		WHEN [Id] = @RC_Finance			THEN @TizitaNigussie
		WHEN [Id] = @RC_Finance			THEN @TizitaNigussie
		WHEN [Id] = @RC_Production		THEN @MesfinWolde
		WHEN [Id] = @RC_Materials		THEN @AyelechHora
		WHEN [Id] = @RC_SalesAG			THEN @Ashenafi
		WHEN [Id] = @RC_SalesBole		THEN @Ashenafi
		ELSE  NULL
	END;

	IF @DebugEmployees = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Employee Since', A.[IsActive],
		A.[BasicSalary], A.[TransportationAllowance], A.[OvertimeRate] AS 'Day Overtime Rate'--, RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'employees', NULL) A
		--JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;