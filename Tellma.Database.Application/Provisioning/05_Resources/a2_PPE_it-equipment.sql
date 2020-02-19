-- We look at the specialized Excel files in the IT department, and we define add Resource definitions accordingly
IF @DB = N'101' -- Banan SD, USD, en
OR @DB = N'102' -- Banan ET, ETB, en
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],
		[TimeUnitVisibility], [CurrencyVisibility],
		[Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId],
		[Lookup2Visibility], [Lookup2Label], [Lookup2DefinitionId]
	) VALUES (
		N'it-equipment',	N'IT Equipment',	N'IT Equipment',
--		N'ComputerEquipment, CommunicationAndNetworkEquipment, NetworkInfrastructure',
		N'Required', N'Optional',
		N'Optional', N'Manufacturer', N'it-equipment-manufacturers',
		N'Optional', N'Operating System', N'operating-systems'
	);
		
	EXEC [api].[ResourceDefinitions__Save]
		@Entities = @ResourceDefinitions,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	--DECLARE @ComputerEquipmentId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'ComputerEquipment');
	--DECLARE @CommunicationAndNetworkEquipmentId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'CommunicationAndNetworkEquipment');
	--DECLARE @NetworkInfrastructureId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'NetworkInfrastructure');

	DECLARE @ComputerEquipmentId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'OfficeEquipment');
	DECLARE @CommunicationAndNetworkEquipmentId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'OfficeEquipment');
	DECLARE @NetworkInfrastructureId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'OfficeEquipment');
	   
	DECLARE @ITEquipmentDescendants dbo.AccountTypeList;
	INSERT INTO @ITEquipmentDescendants ([Index],
		[Code],					[Name],			[ParentId],			[IsAssignable]) VALUES
	--N'ComputerEquipment',						N'/1/1/6/'
	(0, N'ComputersExtension',	N'Computers',	@ComputerEquipmentId,	1),
	(1, N'ServersExtension',	N'Servers',		@ComputerEquipmentId,	1),
	(2, N'DesktopsExtension',	N'Desktops',	@ComputerEquipmentId,	1),
	(3, N'LaptopsExtension',	N'Laptops',		@ComputerEquipmentId,	1),
	--(N'CommunicationAndNetworkEquipment',		N'/1/1/7/'
	(4, N'MobilesExtension',	N'Mobiles',		@CommunicationAndNetworkEquipmentId,	1),
	(5, N'PrintersExtension',	N'Printers',	@CommunicationAndNetworkEquipmentId,	1),
	--N'NetworkInfrastructure',					N'/1/1/8/'
	(6, N'RoutersExtension',	N'Routers',		@NetworkInfrastructureId,	1);
	
	EXEC [api].[AccountTypes__Save]
		@Entities = @ITEquipmentDescendants,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Classifications: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AccountTypeId],							[Name],			[Identifier],	[Lookup1Id],											[Lookup2Id]) VALUES
	(0,dbo.fn_ATCode__Id(N'ServersExtension'),	N'Dell ML 200',		N'FZ889123',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Dell'),	dbo.fn_Lookup(N'operating-systems', N'Windows Server 2017')),
	(1,dbo.fn_ATCode__Id(N'PrintersExtension'),	N'HP Deskject',		N'SS9898224',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'HP'),	NULL),
	(2,dbo.fn_ATCode__Id(N'RoutersExtension'),	N'ASUS Router',		N'100022311',	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Apple'),	dbo.fn_Lookup(N'operating-systems', N'iOS 13'));
	
	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 2, dbo.fn_UnitName__Id(N'yr'),	1);
	
	EXEC [api].[Resources__Save]
		@DefinitionId = N'it-equipment',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (it-equipment): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END