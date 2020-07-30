DECLARE @Employees dbo.[RelationList];

IF @DB = N'100' -- ACME, USD, en/ar/zh
	INSERT INTO @Employees
	([Index],	[Name],				[FromDate],	[Code]) VALUES
	(0,			N'Mohamad Akra',	'2017.10.01',	N'E1'),
	(1,			N'Ahmad Akra',		'2017.10.01',	N'E2');

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
INSERT INTO @Employees
	([Index],	[Name],				[FromDate],	[Code]) VALUES
	(0,			N'Salman',			'2017.10.01',	N'E1');
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Employees
	([Index],	[Name],				[FromDate],	[Code]) VALUES
	(1,			N'Badege Kebede',	'2019.09.01',	N'E1'),
	(2,			N'Tizita Nigussie',	'2019.09.01',	N'E2'),
	(3,			N'Ashenafi Fantahun','2019.09.01',	N'E3'),
	(4,			N'Mesfin Wolde',	'2019.09.01',	N'E4'),
	(5,			N'Zewdinesh Hora',	'2019.09.01',	N'E5'),
	(6,			N'Tigist Negash',	'2019.09.01',	N'E6'),
	(7,			N'Mestawet G/Egziyabhare','2019.09.01',N'E7'),
	(8,			N'Ayelech Hora',	'2019.09.01',	N'E8');
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Employees
	([Index],	[Name],		[Name2],	[FromDate],	[Code]) VALUES
	(0,			N'Salman',	N'سلمان',	'2017.10.01',	N'E1'),
	(1,			N'Tareq',	N'طارق',	'2017.10.01',	N'E2'),
	(2,			N'Hisham',	N'هشام',	'2019.09.01',	N'E3');

	END
	INSERT INTO @RelationUsers([Index],[HeaderIndex], [UserId])
	VALUES(0,0,@AdminUserId)

	EXEC [api].[Relations__Save]
		@DefinitionId = @EmployeeCD,
		@Entities = @Employees,
		@RelationUsers = @RelationUsers,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Employees: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	
	DECLARE @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakFikadu int,
		@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
		@MesfinWolde int;

	SELECT 
		@BadegeKebede = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Badege Kebede'), 
		@TizitaNigussie = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Tizita Nigussie'), 
		@Ashenafi = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Ashenafi Fantahun'), 
		@YisakFikadu = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Yisak Fikadu'), 
		@ZewdineshHora = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Zewdinesh Hora'), 
		@TigistNegash = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Tigist Negash'), 
		@RomanZenebe = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Roman Zenebe'), 
		@Mestawet = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Mestawet G/Egziyabhare'), 
		@AyelechHora = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Ayelech Hora'), 
		@YigezuLegesse = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Yigezu Legesse'), 
		@MesfinWolde = (SELECT [Id] FROM [dbo].[fi_Relations](N'employees', NULL) WHERE [Name] = N'Mesfin Wolde');