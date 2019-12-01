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
	
		INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'computer-equipment'
						[Name],				[IsLeaf],	[Node]) VALUES
		(N'it-equipment',	N'Computers',		0,			N'/1/'),
		(N'it-equipment',	N'Servers',			1,			N'/1/1/'),
		(N'it-equipment',	N'Desktops',		1,			N'/1/2/'),
		(N'it-equipment',	N'Laptops',			1,			N'/1/3/'),
		(N'it-equipment',	N'Mobiles',			1,			N'/2/'),
		(N'it-equipment',	N'Printers',		0,			N'/3/'),
		(N'it-equipment',	N'Routers',			1,			N'/4/');

	DECLARE @ITEquipment dbo.ResourceList;
	INSERT INTO @ITEquipment ([Index],
		[ResourceClassificationId],		[Name],			[TimeUnitId],				[Lookup1Id],											[Lookup2Id]) VALUES
-- N'ComputerEquipment',	
	(0,dbo.fn_RCCode__Id(N'Servers'),	N'VH-GMT-01',	dbo.fn_UnitName__Id(N'Yr'),	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Dell'),	dbo.fn_Lookup(N'operating-systems', N'Windows Server 2017')),
-- N'CommunicationAndNetworkEquipment',	
	(1,dbo.fn_RCCode__Id(N'Printers'),	N'HP-Deskject',	dbo.fn_UnitName__Id(N'Yr'),	dbo.fn_Lookup(N'it-equipment-manufacturers', N'HP'),	NULL),
-- N'NetworkInfrastructure',
	(2,dbo.fn_RCCode__Id(N'Routers'),	N'ASUS Router',	dbo.fn_UnitName__Id(N'Yr'), dbo.fn_Lookup(N'it-equipment-manufacturers', N'Apple'),	dbo.fn_Lookup(N'operating-systems', N'iOS 13'));
	
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
			[Lookup1] AS 'Manufacturer', [Lookup2] AS 'Operating System'
		FROM rpt.Resources(@ITEquipmentIds);
	END