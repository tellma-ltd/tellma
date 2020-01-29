	-- We look at the specialized Excel files in the General Services department, and we add Resource definitions accordingly
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
	[Id],				[TitlePlural],		[TitleSingular],	[IdentifierLabel],[Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId]) VALUES
	(N'motor-vehicles',	N'Motor Vehicles',	N'Motor Vehicle',	N'Plate #',			N'Required',		N'Make',		N'vehicle-makes');
	
	EXEC [api].[ResourceDefinitions__Save]
		@Entities = @ResourceDefinitions,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DECLARE @ParentId INT = (SELECT Id FROM dbo.AccountTypes WHERE Code = N'MotorVehicles');
	 
	DECLARE @MotorVehicleDescendants AccountTypeList;
	INSERT INTO @MotorVehicleDescendants ([Index],
		[Code],					[Name],			[ParentId],	[IsAssignable]) VALUES
	(0, N'CarsExtension',		N'Cars',		@ParentId,	1),
	(1, N'MinivansExtension',	N'Minivans',	@ParentId,	1);

	EXEC [api].[AccountTypes__Save]
		@Entities = @MotorVehicleDescendants,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Classifications: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DECLARE @MotorVehicles dbo.ResourceList;
	INSERT INTO @MotorVehicles ([Index],
				[AccountTypeId],				[Name],	[AvailableSince],	[Lookup1Id],									[Identifier], [CountUnitId],				[Count]) VALUES
	(0, dbo.fn_RCCode__Id(N'CarsExtension'),	N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'),		N'AA 78172',	dbo.fn_UnitName__Id(N'ea'), 1),--1
	(1, dbo.fn_RCCode__Id(N'CarsExtension'),	N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'),		N'BX54662',		dbo.fn_UnitName__Id(N'ea'), 1),--1
	(2, dbo.fn_RCCode__Id(N'MinivansExtension'),N'Minivan 2019',N'2018.12.01' ,		dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'),	N'AA100000',	dbo.fn_UnitName__Id(N'ea'), 1),
	(3, dbo.fn_RCCode__Id(N'MinivansExtension'),N'Minivan 2019',N'2018.12.01' ,		dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'),	N'LM999812',	dbo.fn_UnitName__Id(N'ea'), 1);
	;

	EXEC [api].[Resources__Save]
		@DefinitionId = N'motor-vehicles',
		@Entities = @MotorVehicles,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT * FROM dbo.ResourceDefinitions WHERE [Id] = N'motor-vehicles';

		DECLARE @MotorVehiclesIds dbo.IdList;
		INSERT INTO @MotorVehiclesIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'motor-vehicles';

		SELECT [DefinitionId], [Id], [Classification], [Name] AS 'Vehcile', [Currency] AS 'Price In',--	[LengthUnit] AS 'Usage In',	
		[AvailableSince] AS 'Production Date', [Lookup1] AS N'Make', [Identifier] AS 'Plate #'
		FROM rpt.Resources(@MotorVehiclesIds);

		--Select * from resources;
	END
END