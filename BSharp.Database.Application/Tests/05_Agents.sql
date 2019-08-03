BEGIN -- Cleanup & Declarations
	DECLARE @AgentsDTO [dbo].[AgentList];

	DECLARE @MohamadAkra int, @AhmadAkra int, @BadegeKebede int, @TizitaNigussie int, @Ashenafi int, @YisakTegene int,
			@ZewdineshHora int, @TigistNegash int, @RomanZenebe int, @Mestawet int, @AyelechHora int, @YigezuLegesse int,
			@MesfinWolde int;
	DECLARE @BananIT int, @WaliaSteel int, @Lifan int, @Sesay int, @ERCA int, @Paint int, @Plastic int, @CBE int, @AWB int,
			@NIB int, @Regus int;
	DECLARE @ExecutiveOffice int, @Production int, @SalesAndMarketing int, @Finance int, @HR int,
			@MaterialsAndPurchasing int;
END
BEGIN -- Insert individuals and organizations
	INSERT INTO @AgentsDTO
	([AgentType],		[Name],		[IsRelated],[TaxIdentificationNumber], [Address], [Title], [Gender], [BirthDateTime]) VALUES
	(N'Individual',	N'Mohamad Akra',	0,		NULL,						NULL,				N'Dr.',		'M',	'1966.02.19'),
	(N'Individual',	N'Ahmad Akra',		0,		NULL,						NULL,				N'Mr.',		'M',	'1992.09.21'),
	(N'Individual',	N'Badege Kebede',	1,		NULL,						NULL,				N'ATO',		'M',	NULL),
	(N'Individual',	N'Tizita Nigussie',	0,		NULL,						NULL,				N'Ms.', 	'F',	NULL),
	(N'Individual',	N'Ashenafi Fantahun',0,		NULL,						NULL,				N'Mr.',		'M',	NULL),
	(N'Individual',	N'Yisak Tegene',	0,		NULL,						NULL,				N'Mr.',		'M',	NULL),
	(N'Individual',	N'Zewdinesh Hora',	0,		NULL,						NULL,				N'Ms.',		'F',	NULL),
	(N'Individual',	N'Tigist Negash',	0,		NULL,						NULL,				N'Ms.',		'F',	NULL),
	(N'Individual',	N'Roman Zenebe',	0,		NULL,						NULL,				N'Ms.',		'F',	NULL),
	(N'Individual',	N'Mestawet G/Egziyabhare',	0,NULL,						NULL,				N'Ms.',		'F',	NULL),
	(N'Individual',	N'Ayelech Hora',	0,		NULL,						NULL,				N'Ms.',		'F',	NULL),
	(N'Individual',	N'Yigezu Legesse',	0,		NULL,						NULL,				N'ATO',		'F',	NULL),
	(N'Individual',	N'Mesfin Wolde',	0,		N'0059603732',				NULL,				N'Eng.',	'M',	NULL),

	(N'Organization', N'Banan Information technologies, plc', 1,N'0054901530', N'AA, Bole, 316/3/203 A',NULL,NULL,	'2017.08.09'),
	(N'Organization', N'Walia Steel Industry, plc', 1,N'0001656462',			NULL,			NULL,		NULL,	NULL),
	(N'Organization', N'Yangfan Motors, PLC', 0,N'0005306731',				N'AA, Bole, 06, New',NULL,		NULL,	NULL),
	(N'Organization', N'Sisay Tesfaye, PLC', 0,	N'',						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Ethiopian Revenues and Customs Authority', 0,NULL,	NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Best Paint Industry', 0,NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Best Plastic Industry', 0,NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Commercial Bank of Ethiopia', 0,NULL,				NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Awash Bank',	0,		NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'NIB',			0,		NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Regus',			0,		N'0008895353',		N'AA, Girgi, 22, New',		NULL,		NULL,	NULL),
	
	(N'Organization', N'Executive Office',1,	NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Production Department',0,NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Sales & Marketing Department',0,NULL,				NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Finance Department',0,	NULL,						NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Human Resources Department',0,NULL,					NULL,				NULL,		NULL,	NULL),
	(N'Organization', N'Materials & Purchasing Department',0,NULL,			NULL,				NULL,		NULL,	NULL);

	EXEC [dbo].[api_Agents__Save]
		@Entities = @AgentsDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @ResultsJson OUTPUT

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Agents: Place 1'
		GOTO Err_Label;
	END;

	IF @DebugAgents = 1
		SELECT * FROM [dbo].[fr_Agents__Json](@ResultsJson);
END

-- Inserting
DELETE FROM @AgentsDTO;
INSERT INTO @AgentsDTO (
	[Id], [AgentType], [Name], [Code], [IsRelated], [TaxIdentificationNumber], [Address], [Title], [Gender], [BirthDateTime], [EntityState])
SELECT
	[Id], [AgentType], [Name], [Code], [IsRelated], [TaxIdentificationNumber], [Address], [Title], [Gender], [BirthDateTime], N'Unchanged'
FROM [dbo].[Agents]
WHERE [Name] Like N'%Akra' OR [Name] Like N'Y%';

-- Updating MA TIN
	UPDATE @AgentsDTO
	SET 
		[TaxIdentificationNumber] = N'0059603732',
		[EntityState] = N'Updated'
	WHERE [Name] = N'Ahmad Akra';

	--UPDATE @AgentsDTO 
	--SET 
	--	[Code] = N'MA',
	--	[EntityState] = N'Updated'
	--WHERE [Name] Like N'%Akra';

-- Deleting Legesse record
	UPDATE @AgentsDTO
	SET [EntityState] = N'Deleted' 
	WHERE [Name] = N'Yigezu Legesse';

	EXEC [dbo].[api_Agents__Save]
		@Entities = @AgentsDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @ResultsJson OUTPUT

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Agents: Place 2'
		GOTO Err_Label;
	END;
	
	IF @DebugAgents = 1
		SELECT * FROM [dbo].[fr_Agents__Json](@ResultsJson);

	IF @DebugAgents = 1
		SELECT * FROM [dbo].[Agents];

SELECT 
	@MohamadAkra = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Mohamad Akra'), 
	@AhmadAkra = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ahmad Akra'), 
	@BadegeKebede = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Badege Kebede'), 
	@TizitaNigussie = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Tizita Nigussie'), 
	@Ashenafi = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Ashenafi Fantahun'), 
	@YisakTegene = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Yisak Tegene'), 
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

	@ExecutiveOffice = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Executive Office'),
	@Production = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Production Department'),
	@SalesAndMarketing = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Sales & Marketing Department'),
	@Finance = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Finance Department'),
	@HR = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Human Resources Department'),
	@MaterialsAndPurchasing = (SELECT [Id] FROM [dbo].[Agents] WHERE [Name] = N'Materials & Purchasing Department');

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

--		SELECT @MohamadAkra AS MA, @AhmadAkra AS AA, @TigistNegash AS TN, @TizitaNigussie As Tiz;
DECLARE @AgentSettingSave [dbo].SettingList, @AgentSettingResultJson nvarchar(max)

INSERT INTO @AgentSettingSave
([Field],[Value]) Values(N'TaxAuthority', @ERCA);

EXEC [dbo].[api_Settings__Save]
		@Settings = @AgentSettingSave,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT,
		@ResultsJson = @AgentSettingResultJson OUTPUT;