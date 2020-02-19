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

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
				[AccountTypeId],				[Name],			[AvailableSince],	[Lookup1Id],									[Identifier]) VALUES
	(0, dbo.fn_ATCode__Id(N'CarsExtension'),	N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'),		N'AA 78172'),--1
	(1, dbo.fn_ATCode__Id(N'CarsExtension'),	N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'),		N'BX54662'),--1
	(2, dbo.fn_ATCode__Id(N'MinivansExtension'),N'Minivan 2019',N'2018.12.01' ,		dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'),	N'AA100000'),
	(3, dbo.fn_ATCode__Id(N'MinivansExtension'),N'Minivan 2019',N'2018.12.01' ,		dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'),	N'LM999812');
	;
	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 2, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 3, dbo.fn_UnitName__Id(N'yr'),	1);

	EXEC [api].[Resources__Save]
		@DefinitionId = N'motor-vehicles',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END