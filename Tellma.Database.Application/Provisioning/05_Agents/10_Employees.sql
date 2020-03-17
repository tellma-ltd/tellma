	DECLARE @employees dbo.[AgentList], @AgentRates dbo.AgentRateList;


IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code]) VALUES
	(0,			N'Mohamad Akra',	'2017.10.01',	N'E1'),
	(1,			N'Ahmad Akra',		'2017.10.01',	N'E2');
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @employees
	([Index],	[Name],				[Name2],			[Code]) VALUES
	(0,			N'Ahmad Habashi',	N'أحمد حبشي',		N'E1'),
	(1,			N'Ahmad Abdussalam',N'أحمد عبدالسلام',	N'E2'),
	(101,		N'Abu Ammar',		N'أبو عمار',		N'E101'),
	(102,		N'Mohamad Ali',		N'محمد علي',		N'E102'),
	(103,		N'elAmin elTayeb',	N'الأمين الطيب',		N'E103')
	;

	INSERT INTO @AgentRates([Index], [HeaderIndex],
	[ResourceId],					[UnitId],	[CurrencyId],	[Rate]) VALUES
	(0,0,@BasicSalary,				@WorkMonth, N'SDG',			3500),
	(0,1,@HourlyWage,				@Hour,		N'USD',			5.5);
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code]) VALUES
	(0,			N'Mohamad Akra',	'2017.10.01',	N'E01'),
	(1,			N'Ahmad Akra',		'2017.10.01',	N'E02'),
	(2,			N'Yisak Fikadu',	'2019.09.01',	N'E04'),
	(3,			N'Abrham Tenker',	'2020.09.01',	N'E05');

	INSERT INTO @AgentRates([Index], [HeaderIndex],
	[ResourceId],					[UnitId],	[CurrencyId],	[Rate]) VALUES
	(0,0,@BasicSalary,				@WorkMonth, N'ETB',			7000),
	(1,0,@TransportationAllowance,	@WorkMonth, N'ETB',			1750),
	(0,1,@BasicSalary,				@WorkMonth, N'ETB',			7000),
	(0,2,@BasicSalary,				@WorkMonth, N'ETB',			4700),
	(1,2,@DataPackage,				@WorkMonth, N'ETB',			300),
	(2,2,@DayOvertime,				@Hour,		N'ETB',			28.25),
	(3,2,@NightOvertime,			@Hour,		N'ETB',			33.5),
	(4,2,@RestOvertime,				@Hour,		N'ETB',			45.19),
	(5,2,@HolidayOvertime,			@Hour,		N'ETB',			56.5),
	(0,3,@BasicSalary,				@WorkMonth, N'ETB',			10200),
	(1,3,@TransportationAllowance,	@WorkMonth, N'ETB',			1000),
	(2,3,@MealAllowance,			@WorkMonth, N'ETB',			1000);
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code]) VALUES
	(0,			N'Salman',			'2017.10.01',	N'E1');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @employees
	([Index],	[Name],				[StartDate],	[Code]) VALUES
	(1,			N'Badege Kebede',	'2019.09.01',	N'E1'),
	(2,			N'Tizita Nigussie',	'2019.09.01',	N'E2'),
	(3,			N'Ashenafi Fantahun','2019.09.01',	N'E3'),
	(4,			N'Mesfin Wolde',	'2019.09.01',	N'E4'),
	(5,			N'Zewdinesh Hora',	'2019.09.01',	N'E5'),
	(6,			N'Tigist Negash',	'2019.09.01',	N'E6'),
	(7,			N'Mestawet G/Egziyabhare','2019.09.01',N'E7'),
	(8,			N'Ayelech Hora',	'2019.09.01',	N'E8');

	INSERT INTO @AgentRates
	([Index], [HeaderIndex], [ResourceId], [UnitId], [CurrencyId], [Rate]) VALUES
	(0,			1,			@BasicSalary,	@WorkMonth, N'ETB',		30000),
	(0,			2,			@BasicSalary,	@WorkMonth, N'ETB',		8000),
	(0,			3,			@BasicSalary,	@WorkMonth, N'ETB',		15000),
	(0,			4,			@BasicSalary,	@WorkMonth, N'ETB',		4700),
	(0,			5,			@BasicSalary,	@WorkMonth, N'ETB',		4700),
	(1,			5,			@DayOvertime,	@Hour,		N'ETB',		28.25),
	(0,			6,			@BasicSalary,	@WorkMonth, N'ETB',		4700),
	(1,			6,			@DayOvertime,	@Hour,		N'ETB',		28.25),
	(0,			7,			@BasicSalary,	@WorkMonth, N'ETB',		4700),
	(1,			7,			@DayOvertime,	@Hour,		N'ETB',		28.25),	
	(0,			8,			@BasicSalary,	@WorkMonth, N'ETB',		4700)
	;
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @employees
	([Index],	[Name],		[Name2],	[StartDate],	[Code]) VALUES
	(0,			N'Salman',	N'سلمان',	'2017.10.01',	N'E1'),
	(1,			N'Tareq',	N'طارق',	'2017.10.01',	N'E2'),
	(2,			N'Hisham',	N'هشام',	'2019.09.01',	N'E3');

	END
	UPDATE @employees SET UserId = @AdminUserId WHERE [Index] = 0;
	EXEC [api].[Agents__Save]
		@DefinitionId = N'employees',
		@Entities = @employees,
		@AgentRates = @AgentRates,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Employees: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @Abu_Ammar INT, @M_Ali INT, @el_Amin INT;
	DECLARE @MohamadAkra int, @AhmadAkra int;
	
	DECLARE @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakFikadu int,
		@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
		@MesfinWolde int;

	SELECT 
		@Abu_Ammar = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Abu Ammar'), 
		@M_Ali = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'Mohamad Ali'), 
		@el_Amin = (SELECT [Id] FROM [dbo].fi_Agents(N'employees', NULL) WHERE [Name] = N'elAmin elTayeb'), 
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