	-- We look at the specialized Excel files in the IT department, and we define add Resource definitions accordingly
		INSERT INTO dbo.ResourceDefinitions (
			[Id],			[TitlePlural],		[TitleSingular],	[ResourceTypeParentList],
			[ResourceClassificationVisibility], [TimeUnitVisibility], [CurrencyVisibility],
			[Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId],
			[Lookup2Visibility], [Lookup2Label], [Lookup2DefinitionId]
		) VALUES (
			N'it-equipment',	N'IT Equipment',	N'IT Equipment',
			N'ComputerEquipment, CommunicationAndNetworkEquipment, NetworkInfrastructure',
			N'Required', N'Required', N'Optional',
			N'Optional', N'Manufacturer', N'it-equipment-manufacturers',
			N'Optional', N'Operating System', N'operating-systems'
		);
	
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'computer-equipment'
						[Name],				[IsLeaf],	[Node]) VALUES
	(N'it-equipment',	N'Servers',			1,			N'/1/'),
	(N'it-equipment',	N'Desktops',		1,			N'/2/'),
	(N'it-equipment',	N'Laptops',			1,			N'/3/'),
	(N'it-equipment',	N'Mobiles',			1,			N'/4/'),
	(N'it-equipment',	N'Printers',		0,			N'/5/'),
	(N'it-equipment',	N'Color printers',	1,			N'/5/1/'),
	(N'it-equipment',	N'B/W printers',	1,			N'/5/2/');


	DECLARE @ITEquipment dbo.ResourceList, @ITEquipmentPickList dbo.ResourcePickList;
	INSERT INTO @ITEquipment ([Index],
	[ResourceTypeId],				[Name],				[ResourceClassificationId],			[CurrencyId],	[TimeUnitId],				[ResourceLookup1Id], [ResourceLookup2Id]) VALUES
	(0, N'ComputerEquipment',		N'Blade Servers',	dbo.fn_RCName__Id(N'Blade Servers'),'USD',			dbo.fn_UnitName__Id(N'Yr'),	dbo.fn_Lookup(N'it-equipment-manufacturers', N'Toyota'),
																																		dbo.fn_Lookup(N'it-equipment-manufacturers', N'Toyota'))
	(0, N'CommunicationAndNetworkEquipment',N'Printers',									N'USD',			dbo.fn_UnitName__Id(N'Km'),	dbo.fn_Lookup(N'vehicle-makes', N'Toyota')),--1
	(0, N'NetworkInfrastructure',			N'Mobiles',		N'USD',			dbo.fn_UnitName__Id(N'Km'),	dbo.fn_Lookup(N'vehicle-makes', N'Toyota')),--1
	
	
