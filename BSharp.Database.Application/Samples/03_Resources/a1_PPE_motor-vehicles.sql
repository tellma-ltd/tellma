	-- We look at the specialized Excel files in the General Services department, and we add Resource definitions accordingly
	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[DescriptorIdLabel],[Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId]) VALUES
	(N'motor-vehicles',	N'Motor Vehicles',	N'Motor Vehicle',	N'Plate #',			N'Required',		N'Make',		N'vehicle-makes');
	
	DECLARE @MotorVehicleDescendants ResourceClassificationList;
	INSERT INTO @MotorVehicleDescendants ([Index],
		[Code],					[Name],			[Path],			[IsAssignable], [ResourceDefinitionId]) VALUES
	(0, N'CarsExtension',		N'Cars',		N'/1/1/3/3/1/',	1,				N'motor-vehicles'),
	(1, N'MinivansExtension',	N'Minivans',	N'/1/1/3/3/2/',	1,				N'motor-vehicles');

	EXEC [api].[ResourceClassifications__Save]
		@Entities = @MotorVehicleDescendants,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Classifications: Inserting'
		GOTO Err_Label;
	END;		

	DECLARE @MotorVehicles dbo.ResourceList;
	INSERT INTO @MotorVehicles ([Index], [OperatingSegmentId],
				[ResourceClassificationId],				[Name],			[AvailableSince],	[Lookup1Id],									[DescriptorId], [CountUnitId],				[Count]) VALUES
	(0, @OS_WSI, dbo.fn_RCCode__Id(N'CarsExtension'),	N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'),		N'AA 78172',	dbo.fn_UnitName__Id(N'ea'), 1),--1
	(1, @OS_WSI, dbo.fn_RCCode__Id(N'CarsExtension'),	N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'),		N'BX54662',		dbo.fn_UnitName__Id(N'ea'), 1),--1
	(2, @OS_WSI, dbo.fn_RCCode__Id(N'MinivansExtension'),N'Minivan 2019',N'2018.12.01' ,	dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'),	N'AA100000',	dbo.fn_UnitName__Id(N'ea'), 1),
	(3, @OS_WSI, dbo.fn_RCCode__Id(N'MinivansExtension'),N'Minivan 2019',N'2018.12.01' ,	dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'),	N'LM999812',	dbo.fn_UnitName__Id(N'ea'), 1);
	;

	EXEC [api].[Resources__Save]
		@DefinitionId = N'motor-vehicles',
		@Entities = @MotorVehicles,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles)'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT * FROM dbo.ResourceDefinitions WHERE [Id] = N'motor-vehicles';

		DECLARE @MotorVehiclesIds dbo.IdList;
		INSERT INTO @MotorVehiclesIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'motor-vehicles';

		SELECT [DefinitionId], [Id], [Classification], [Name] AS 'Vehcile',[OperatingSegment], [Currency] AS 'Price In',--	[LengthUnit] AS 'Usage In',	
		[AvailableSince] AS 'Production Date', [Lookup1] AS N'Make', [DescriptorId] AS 'Plate #'
		FROM rpt.Resources(@MotorVehiclesIds);

		--Select * from resources;
	END