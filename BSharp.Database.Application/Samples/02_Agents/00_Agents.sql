BEGIN -- Cleanup & Declarations
	DECLARE @Individuals [dbo].[AgentList], @Organizations  [dbo].[AgentList], @CostObjects  [dbo].[AgentList];

	DECLARE @MohamadAkra int, @AhmadAkra int, @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakFikadu int,
			@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
			@MesfinWolde int;
	DECLARE @BananIT int, @WaliaSteel int, @Lifan int, @Sesay int, @ERCA int, @Paint int, @Plastic int, @CBE int, @AWB int,
			@NIB int, @Regus int, @NocJimma INT, @Toyota INT;
END
BEGIN 
	INSERT INTO @Individuals([Index],
		[Name],					[IsRelated], [Code]) VALUES
	(0,	N'Mohamad Akra',		0,			''), -- shareholders
	(1,	N'Ahmad Akra',			0,			''),
	(2,	N'Badege Kebede',		1,			'E'), -- employees
	(3,	N'Tizita Nigussie',		0,			'E'),
	(4,	N'Ashenafi Fantahun',	0,			'E'),
	(5,	N'Yisak Fikadu',		0,			'E'),
	(6,	N'Zewdinesh Hora',		0,			'E'),
	(7,	N'Tigist Negash',		0,			'E'),
	(8,	N'Roman Zenebe',		0,			'E'),
	(9,	N'Mestawet G/Egziyabhare',	0,		'E'),
	(10,N'Ayelech Hora',		0,			'E'),
	(11,N'Yigezu Legesse',		0,			'E'),
	(12,N'Mesfin Wolde',		0,			'E');
	UPDATE @Individuals SET [Code] = [Code] + CAST([Index] AS NVARCHAR(50));

	EXEC [api].[Agents__Save]
		@DefinitionId = N'individuals',
		@Entities = @Individuals,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Individuals: Inserting'
		GOTO Err_Label;
	END;

	INSERT INTO @Organizations([Index],
		[Name],									[IsRelated], [Code]) VALUES
	(13, N'Banan Information technologies, plc',	1,		'S'), -- suppliers
	(14, N'Walia Steel Industry, plc',				1,		''),
	(15, N'Yangfan Motors, PLC',					0,		'S'), -- suppliers
	(16, N'Sisay Tesfaye, PLC',						0,		'O'),
	(17, N'Ethiopian Revenues and Customs Authority',0,		'T'), -- taxing
	(18, N'Best Paint Industry',					1,		'CS'),
	(19, N'Best Plastic Industry',					1,		'CS'),
	(20, N'Commercial Bank of Ethiopia',			0,		'BC'), -- banking
	(21, N'Awash Bank',								0,		'B'), -- banking
	(22, N'NIB',									0,		'B');

	INSERT INTO @Organizations([Index],
		[Name],									[IsRelated], [Code], [TaxIdentificationNumber]) VALUES
	(23, N'Regus',									0,		'S',	N'4544287');

	INSERT INTO @Organizations([Index],
		[Name],									[IsRelated], [Code]) VALUES	
	(24, N'Noc Jimma Ber Service Station',			0,		'S'), -- suppliers
	(25, N'Toyota, Ethiopia',						0,		'S'),
	(26, N'Executive Office',						1,		'R'),
	(27, N'Production Department',					0,		'R'),
	(28, N'Sales & Marketing Department',			0,		'R'),
	(29, N'Finance Department',						0,		'R'),
	(30, N'Human Resources Department',				0,		'R'),
	(31, N'Materials & Purchasing Department',		0,		'R');
	UPDATE @Organizations SET [Code] = [Code] + CAST([Index] AS NVARCHAR(50));

	EXEC [api].[Agents__Save]
		@DefinitionId = N'organizations',
		@Entities = @Organizations,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Organizations: Inserting'
		GOTO Err_Label;
	END;
END
	IF @DebugAgents = 1
		SELECT * FROM [dbo].[Agents];

SELECT 
	@MohamadAkra = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Mohamad Akra'), 
	@AhmadAkra = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ahmad Akra'), 
	@BadegeKebede = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Badege Kebede'), 
	@TizitaNigussie = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Tizita Nigussie'), 
	@Ashenafi = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ashenafi Fantahun'), 
	@YisakFikadu = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Yisak Fikadu'), 
	@ZewdineshHora = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Zewdinesh Hora'), 
	@TigistNegash = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Tigist Negash'), 
	@RomanZenebe = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Roman Zenebe'), 
	@Mestawet = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Mestawet G/Egziyabhare'), 
	@AyelechHora = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ayelech Hora'), 
	@YigezuLegesse = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Yigezu Legesse'), 
	@MesfinWolde = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Mesfin Wolde'),
	@BananIT = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Banan Information technologies, plc'),
	@WaliaSteel = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Walia Steel Industry, plc'),
	@Lifan = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Yangfan Motors, PLC'),
	@Sesay = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Sisay Tesfaye, PLC'),
	@ERCA = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ethiopian Revenues and Customs Authority'),
	@Paint = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Best Paint Industry'),
	@Plastic = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Best Plastic Industry'),
	@CBE = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Commercial Bank of Ethiopia'),
	@AWB = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Awash Bank'),
	@NIB = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'NIB'),
	@Regus = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Regus'),
	@NocJimma = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Noc Jimma Ber Service Station'),
	@Toyota =  (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Toyota, Ethiopia');


/*
INSERT INTO dbo.AgentsResources(
	[AgentId], [RelationType],	[ResourceId], [UnitCost], CreatedAt, CreatedById, ModifiedAt, ModifiedById) VALUES
	(@MohamadAkra, N'Employee',		@HOvertime,		450,		@Now,		@UserId, @Now,		@UserId),
	(@AhmadAkra, N'Employee',		@ROvertime,		400,		@Now,		@UserId, @Now,		@UserId),
	(@TizitaNigussie, N'Employee',	@ROvertime,		200,		@Now,		@UserId, @Now,		@UserId),
	(@TizitaNigussie, N'Employee',	@LaborDaily,	250,		@Now,		@UserId, @Now,		@UserId);

BEGIN -- Users
	IF NOT EXISTS(SELECT * FROM [dbo].[Users])
	INSERT INTO [dbo].[Users]([Id], [Name], [AgentId]) VALUES
	(N'system@banan-it.com', N'B#', NULL),
	(N'mohamad.akra@banan-it.com', N'Mohamad Akra', @MohamadAkra),
	(N'ahmad.akra@banan-it.com', N'Ahmad Akra', @AhmadAkra),
	(N'badegek@gmail.com', N'Badege', @BadegeKebede),
	(N'mintewelde00@gmail.com', N'Tizita', @TizitaNigussie),
	(N'ashenafi935@gmail.com', N'Ashenafi', @Ashenafi),
	(N'yisak.tegene@gmail.com', N'Yisak', @YisakTegene),
	(N'zewdnesh.hora@gmail.com', N'Zewdinesh Hora', @ZewdineshHora),
	(N'tigistnegash74@gmail.com', N'Tigist', @TigistNegash),
	(N'roman.zen12@gmail.com', N'Roman', @RomanZenebe),
	(N'mestawetezige@gmail.com', N'Mestawet', @Mestawet),
	(N'ayelech.hora@gmail.com', N'Ayelech', @AyelechHora),
	(N'info@banan-it.com', N'Banan IT', NULL)
END

*/