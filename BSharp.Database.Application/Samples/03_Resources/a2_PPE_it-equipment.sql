	-- We look at the specialized Excel files in the IT department, and we define add Resource definitions accordingly
		INSERT INTO dbo.ResourceDefinitions (
			[Id],			[TitlePlural],		[TitleSingular],
			[ResourceClassificationVisibility], [TimeUnitVisibility], [CurrencyVisibility],
			[Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId],
			[Lookup2Visibility], [Lookup2Label], [Lookup2DefinitionId]
		) VALUES (
			N'it-equipment',	N'IT Equipment',	N'IT Equipment',
	--		N'ComputerEquipment, CommunicationAndNetworkEquipment, NetworkInfrastructure',
			N'Required', N'Required', N'Optional',
			N'Optional', N'Manufacturer', N'it-equipment-manufacturers',
			N'Optional', N'Operating System', N'operating-systems'
		);
	
	DECLARE @ITEquipmentDescendants ResourceClassificationList;
	INSERT INTO @ITEquipmentDescendants ([Index],
		[Code],					[Name],			[Path],			[IsAssignable], [ResourceDefinitionId]) VALUES
	--N'ComputerEquipment',						N'/1/1/6/'
	(0, N'ComputersExtension',	N'Computers',	N'/1/1/6/1/',	1,				N'it-equipment'),
	(1, N'ServersExtension',	N'Servers',		N'/1/1/6/2/',	1,				N'it-equipment'),
	(2, N'DesktopsExtension',	N'Desktops',	N'/1/1/6/3/',	1,				N'it-equipment'),
	(3, N'LaptopsExtension',	N'Laptops',		N'/1/1/6/4/',	1,				N'it-equipment'),
	--(N'CommunicationAndNetworkEquipment',		N'/1/1/7/'
	(4, N'MobilesExtension',	N'Mobiles',		N'/1/1/7/1/',	1,				N'it-equipment'),
	(5, N'PrintersExtension',	N'Printers',	N'/1/1/7/2/',	1,				N'it-equipment'),
	--N'NetworkInfrastructure',					N'/1/1/8/'
	(6, N'RoutersExtension',	N'Routers',		N'/1/1/8/1/',	1,				N'it-equipment');
	
	EXEC [api].[ResourceClassifications__Save]
		@Entities = @ITEquipmentDescendants,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Classifications: Inserting'
		GOTO Err_Label;
	END;		

	DECLARE @ITEquipment dbo.ResourceList;
	INSERT INTO @ITEquipment ([Index], [OperatingSegmentId],
		[ResourceClassificationId],							[Name],			[TimeUnitId],				[DescriptorId], [Lookup1Id],											[Lookup2Id]) VALUES
	(0,@OS_IT,dbo.fn_RCCode__Id(N'ServersExtension'),	N'Dell ML 200',	dbo.fn_UnitName__Id(N'Yr'),	N'FZ889123',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Dell'),	dbo.fn_Lookup(N'operating-systems', N'Windows Server 2017')),
	(1,@OS_IT,dbo.fn_RCCode__Id(N'PrintersExtension'),	N'HP Deskject',	dbo.fn_UnitName__Id(N'Yr'),	N'SS9898224',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'HP'),	NULL),
	(2,@OS_IT,dbo.fn_RCCode__Id(N'RoutersExtension'),	N'ASUS Router',	dbo.fn_UnitName__Id(N'Yr'), N'100022311',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Apple'),	dbo.fn_Lookup(N'operating-systems', N'iOS 13'));
	
	EXEC [api].[Resources__Save]
		@DefinitionId = N'it-equipment',
		@Entities = @ITEquipment,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (it-equipment)'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT  N'it-equipment' AS [Resource Definition]

		DECLARE @ITEquipmentIds dbo.IdList;
		INSERT INTO @ITEquipmentIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'it-equipment';

		SELECT [Classification], [Name] AS 'IT Equipment', [TimeUnit] AS 'Usage In',
			[Lookup1] AS 'Manufacturer', [Lookup2] AS 'Operating System', [OperatingSegment]
		FROM rpt.Resources(@ITEquipmentIds);
	END