BEGIN -- Cleanup & Declarations
	DECLARE @Agents [dbo].[AgentList];

	DECLARE @MohamadAkra int, @AhmadAkra int, @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakFikadu int,
			@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
			@MesfinWolde int;
	DECLARE @BananIT int, @WaliaSteel int, @Lifan int, @Sesay int, @ERCA int, @Paint int, @Plastic int, @CBE int, @AWB int,
			@NIB int, @Regus int, @NocJimma INT, @Toyota INT;
	DECLARE @ExecutiveOffice int, @Production int, @SalesAndMarketing int, @Finance int, @HR int,
			@MaterialsAndPurchasing int;
END
BEGIN -- Insert individuals and organizations
	--INSERT INTO @Agents([Index],
	--	[AgentType],		[Name],		[IsRelated],[TaxIdentificationNumber], [RegisteredAddress], [Title], [Gender], [BirthDate]) VALUES
	--(0,N'Individual',	N'Mohamad Akra',	0,		NULL,						NULL,				N'Dr.',		1,	'1966.02.19'),
	--(1,N'Individual',	N'Ahmad Akra',		0,		NULL,						NULL,				N'Mr.',		1,	'1992.09.21'),
	--(2,N'Individual',	N'Badege Kebede',	1,		NULL,						NULL,				N'ATO',		1,	NULL),
	--(3,N'Individual',	N'Tizita Nigussie',	0,		NULL,						NULL,				N'Ms.', 	2,	NULL),
	--(4,N'Individual',	N'Ashenafi Fantahun',0,		NULL,						NULL,				N'Mr.',		1,	NULL),
	--(5,N'Individual',	N'Yisak Tegene',	0,		NULL,						NULL,				N'Mr.',		1,	NULL),
	--(6,N'Individual',	N'Zewdinesh Hora',	0,		NULL,						NULL,				N'Ms.',		2,	NULL),
	--(7,N'Individual',	N'Tigist Negash',	0,		NULL,						NULL,				N'Ms.',		2,	NULL),
	--(8,N'Individual',	N'Roman Zenebe',	0,		NULL,						NULL,				N'Ms.',		2,	NULL),
	--(9,N'Individual',	N'Mestawet G/Egziyabhare',	0,NULL,						NULL,				N'Ms.',		2,	NULL),
	--(10,N'Individual',	N'Ayelech Hora',	0,		NULL,						NULL,				N'Ms.',		2,	NULL),
	--(11,N'Individual',	N'Yigezu Legesse',	0,		NULL,						NULL,				N'ATO',		2,	NULL),
	--(12,N'Individual',	N'Mesfin Wolde',	0,		N'0059603732',				NULL,				N'Eng.',	1,	NULL),

	--(13,N'Organization', N'Banan Information technologies, plc', 1,N'0054901530', N'AA, Bole, 316/3/203 A',NULL,9,	'2017.08.09'),
	--(14,N'Organization', N'Walia Steel Industry, plc', 1,N'0001656462',		NULL,				NULL,		9,	NULL),
	--(15,N'Organization', N'Yangfan Motors, PLC', 0,N'0005306731',				N'AA, Bole, 06, New',NULL,		9,	NULL),
	--(16,N'Organization', N'Sisay Tesfaye, PLC', 0,	N'',						NULL,				NULL,		9,	NULL),
	--(17,N'Organization', N'Ethiopian Revenues and Customs Authority', 0,NULL,	NULL,				NULL,		9,	NULL),
	--(18,N'Organization', N'Best Paint Industry', 0,NULL,						NULL,				NULL,		9,	NULL),
	--(19,N'Organization', N'Best Plastic Industry', 0,NULL,						NULL,				NULL,		9,	NULL),
	--(20,N'Organization', N'Commercial Bank of Ethiopia', 0,NULL,				NULL,				NULL,		9,	NULL),
	--(21,N'Organization', N'Awash Bank',	0,		NULL,						NULL,				NULL,		9,	NULL),
	--(22,N'Organization', N'NIB',			0,		NULL,						NULL,				NULL,		9,	NULL),
	--(23,N'Organization', N'Regus',			0,		N'0008895353',		N'AA, Girgi, 22, New',		NULL,		9,	NULL),
	
	--(24,N'Organization', N'Noc Jimma Ber Service Station',	0,NULL,				NULL,				NULL,		9,	NULL),

	--(25,N'Organization', N'Executive Office',1,	NULL,						NULL,				NULL,		9,	NULL),
	--(26,N'Organization', N'Production Department',0,NULL,						NULL,				NULL,		9,	NULL),
	--(27,N'Organization', N'Sales & Marketing Department',0,NULL,				NULL,				NULL,		9,	NULL),
	--(28,N'Organization', N'Finance Department',0,	NULL,						NULL,				NULL,		9,	NULL),
	--(29,N'Organization', N'Human Resources Department',0,NULL,					NULL,				NULL,		9,	NULL),
	--(30,N'Organization', N'Materials & Purchasing Department',0,NULL,			NULL,				NULL,		9,	NULL);


	INSERT INTO @Agents([Index],
	[AgentType],		[Name],				[IsRelated], [Code]) VALUES
	(0,N'Individual',	N'Mohamad Akra',	0,		''), -- shareholders
	(1,N'Individual',	N'Ahmad Akra',		0,		''),
	(2,N'Individual',	N'Badege Kebede',	1,		'E'), -- employees
	(3,N'Individual',	N'Tizita Nigussie',	0,		'E'),
	(4,N'Individual',	N'Ashenafi Fantahun',0,		'E'),
	(5,N'Individual',	N'Yisak Fikadu',	0,		'E'),
	(6,N'Individual',	N'Zewdinesh Hora',	0,		'E'),
	(7,N'Individual',	N'Tigist Negash',	0,		'E'),
	(8,N'Individual',	N'Roman Zenebe',	0,		'E'),
	(9,N'Individual',	N'Mestawet G/Egziyabhare',	0, 'E'),
	(10,N'Individual',	N'Ayelech Hora',	0,		'E'),
	(11,N'Individual',	N'Yigezu Legesse',	0,		'E'),
	(12,N'Individual',	N'Mesfin Wolde',	0,		'E'),

	(13,N'Organization', N'Banan Information technologies, plc', 1, 'S'), -- suppliers
	(14,N'Organization', N'Walia Steel Industry, plc',	1, ''),
	(15,N'Organization', N'Yangfan Motors, PLC', 0, 'S'), -- suppliers
	(16,N'Organization', N'Sisay Tesfaye, PLC', 0, 'O'),
	(17,N'Organization', N'Ethiopian Revenues and Customs Authority', 0, 'T'), -- taxing
	(18,N'Organization', N'Best Paint Industry', 1, 'CS'),
	(19,N'Organization', N'Best Plastic Industry', 1, 'CS'),
	(20,N'Organization', N'Commercial Bank of Ethiopia', 0, 'BC'), -- banking
	(21,N'Organization', N'Awash Bank', 0, 'B'), -- banking
	(22,N'Organization', N'NIB', 0, 'B');
	INSERT INTO @Agents([Index],
	[AgentType],		[Name],		[IsRelated], [Code], [TaxIdentificationNumber]) VALUES
	(23,N'Organization', N'Regus',0, 'S', N'4544287');
	INSERT INTO @Agents([Index],
	[AgentType],		[Name],		[IsRelated], [Code]) VALUES	
	(24,N'Organization', N'Noc Jimma Ber Service Station',	0, 'S'), -- suppliers
	(25,N'Organization', N'Toyota, Ethiopia',	0, 'S'),
	(26,N'Organization', N'Executive Office',1, 'R'),
	(27,N'Organization', N'Production Department',0, 'R'),
	(28,N'Organization', N'Sales & Marketing Department',0, 'R'),
	(29,N'Organization', N'Finance Department',0, 'R'),
	(30,N'Organization', N'Human Resources Department',0, 'R'),
	(31,N'Organization', N'Materials & Purchasing Department',0, 'R');
	UPDATE @Agents SET [Code] = [Code] + CAST([Index] AS NVARCHAR(50));
	EXEC [api].[Agents__Save]
		@Entities = @Agents,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Agents: Inserting'
		GOTO Err_Label;
	END;
END
	--IF @DebugAgents = 1
	--	SELECT * FROM [dbo].[Agents];

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
	@Toyota =  (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Toyota, Ethiopia'),

	@ExecutiveOffice = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Executive Office'),
	@Production = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Production Department'),
	@SalesAndMarketing = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Sales & Marketing Department'),
	@Finance = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Finance Department'),
	@HR = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Human Resources Department'),
	@MaterialsAndPurchasing = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Materials & Purchasing Department');
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