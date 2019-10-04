	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[ResourceTypeId], [Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId]) VALUES
	(N'motor-vehicles',	N'Motor Vehicles',	N'Motor Vehicle',	N'MotorVehicles', N'Required',			N'Make',		N'vehicle-makes');
	
	DECLARE @MotorVehicles dbo.ResourceList, @MotorVehiclesPickList dbo.ResourcePickList;
	INSERT INTO @MotorVehicles ([Index],
	[ResourceTypeId],		[Name],						[CurrencyId],	[LengthUnitId],				[ResourceLookup1Id]) VALUES
	(0, N'MotorVehicles',	N'Toyota Prius 2018',		N'USD',			dbo.fn_UnitName__Id(N'Km'),	dbo.fn_Lookup(N'vehicle-makes', N'Toyota')),--1
	(1, N'MotorVehicles',	N'Mercedes Minivan 2019',	N'USD',			dbo.fn_UnitName__Id(N'Km'),	dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'));

	EXEC [api].[Resources__Save]
		@DefinitionId = N'motor-vehicles',
		@Entities = @MotorVehicles,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles)'
		GOTO Err_Label;
	END;
	-- Resources and Picks are entered from the same resource definition screen?
	INSERT INTO @MotorVehiclesPickList([Index],
		[ResourceId],									[ProductionDate],	[Code], [Name],									[MonetaryValue], [Length], [ResourcePickString1]) VALUES
	(0,dbo.fn_ResourceName__Id(N'Toyota Prius 2018'),		N'2017.10.01',	N'101', N'Toyota Camry 2018, Plate AA12345',	20000,			120000,		N'AA12345'),
	(1,dbo.fn_ResourceName__Id(N'Toyota Prius 2018'),		N'2017.10.15',	N'102', N'Toyota Prius 2016, Plate BX54662',	8000,			120000,		N'BX54662'),
	(2,dbo.fn_ResourceName__Id(N'Mercedes Minivan 2019'),	N'2017.10.01',	N'103', N'Mercedes Minivan, Plate AA100000',	14000,			120000,		N'AA100000'),
	(3,dbo.fn_ResourceName__Id(N'Mercedes Minivan 2019'),	N'2017.10.15',	N'104', N'Toyota Minivan 2016, Plate LM999812', 16000,			120000,		N'LM999812');
/*
	EXEC [api].[ResourcePicks__Save]
		@ResourceId = @ResourceId,
		@Entities = @MotorVehiclesPickList,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles)'
		GOTO Err_Label;
	END;

	INSERT INTO dbo.ResourceClassifications (
	[ResourceDefinitionId],				[Name],								[IsLeaf],	[Node]) VALUES
	(N'motor-vehicles',					N'Cars',								0,		N'/1/'),
	(N'motor-vehicles',					N'Diesel Cars',							1,		N'/1/1/'),
	(N'motor-vehicles',					N'Petrol Cars',							1,		N'/1/2/'),
	(N'motor-vehicles',					N'Electric Cars',						1,		N'/1/3/'),
	(N'motor-vehicles',					N'Hybrid Cars',							1,		N'/1/4/'),
	(N'motor-vehicles',					N'Buses',								1,		N'/2/'),
	(N'motor-vehicles',					N'Motorcycles',							1,		N'/3/'),
	(N'motor-vehicles',					N'Trucks',								1,		N'/4/')
	*/