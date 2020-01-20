	DECLARE @employees dbo.[AgentList];
	DECLARE @MohamadAkra int, @AhmadAkra int, @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakFikadu int,
			@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
			@MesfinWolde int;

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate]) VALUES
	(0,			N'Mohamad Akra',	'2017.10.01',	N'E1',	7000,			1750,						0),
	(1,			N'Ahmad Akra',		'2017.10.01',	N'E2',	7000,			0,							0);
ELSE IF @DB = N'101' -- Banan SD, USD, en
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code],	[BasicSalary]) VALUES
	(0,			N'Ahmad Abdussalam','2017.10.01',	N'E1',	7000),
	(1,			N'Omar al-Sammani',	'2017.10.01',	N'E2',	7000);
ELSE IF @DB = N'102' -- Banan ET, ETB, en
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate]) VALUES
	(0,			N'Mohamad Akra',	'2017.10.01',	N'E1',	7000,			1750,						0),
	(1,			N'Ahmad Akra',		'2017.10.01',	N'E2',	7000,			0,							0),
	(2,			N'Yisak Fikadu',	'2019.09.01',	N'E3',	4700,			0,							28.25);
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate]) VALUES
	(0,			N'Salman',			'2017.10.01',	N'E1',	7000,			1750,						0);
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code],	[BasicSalary], [TransportationAllowance], [OvertimeRate]) VALUES
	(1,			N'Badege Kebede',	'2019.09.01',	N'E1',	30000,			0,							0),
	(2,			N'Tizita Nigussie',	'2019.09.01',	N'E2',	8000,			0,							0),
	(3,			N'Ashenafi Fantahun','2019.09.01',	N'E3',	15000,			0,							0),
	(4,			N'Mesfin Wolde',	'2019.09.01',	N'E4',	4700,			0,							0),
	(5,			N'Zewdinesh Hora',	'2019.09.01',	N'E5',	4700,			0,							28.25),
	(6,			N'Tigist Negash',	'2019.09.01',	N'E6',	4700,			0,							28.25),
	(7,			N'Mestawet G/Egziyabhare','2019.09.01',N'E7',4700,			0,							28.25),
	(8,			N'Ayelech Hora',	'2019.09.01',	N'E8',	4700,			0,							0);

	UPDATE @employees SET UserId = @AdminUserId WHERE [Index] = 0;
	EXEC [api].[Agents__Save]
		@DefinitionId = N'employees',
		@Entities = @employees,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Employees: Inserting: ' + @ValidationErrorsJson
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

	--UPDATE dbo.ResponsibilityCenters
	--SET ManagerId = CASE 
	--	WHEN [Id] = @OS_Steel			THEN @BadegeKebede
	--	WHEN [Id] = @RC_ExecutiveOffice	THEN @BadegeKebede
	--	WHEN [Id] = @RC_Finance			THEN @TizitaNigussie
	--	WHEN [Id] = @RC_Finance			THEN @TizitaNigussie
	--	WHEN [Id] = @RC_Production		THEN @MesfinWolde
	--	WHEN [Id] = @RC_Materials		THEN @AyelechHora
	--	WHEN [Id] = @RC_SalesAG			THEN @Ashenafi
	--	WHEN [Id] = @RC_SalesBole		THEN @Ashenafi
	--	ELSE  NULL
	--END;

	IF @DebugEmployees = 1
		SELECT A.[Code], A.[Name], A.[StartDate] AS 'Employee Since', A.[IsActive],
		A.[BasicSalary], A.[TransportationAllowance], A.[OvertimeRate] AS 'Day Overtime Rate'--, RC.[Name] AS OperatingSegment
		FROM dbo.fi_Agents(N'employees', NULL) A
		--JOIN dbo.ResponsibilityCenters RC ON A.OperatingSegmentId = RC.Id;