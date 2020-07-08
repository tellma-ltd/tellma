	-- We look at the specialized Excel files in the General Services department, and we add Resource definitions accordingly
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],			[FromDate],			[Lookup1Id],									[Identifier]) VALUES
	(0, N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(@VehicleMakeLKD, N'Toyota'),		N'AA 78172'),--1
	(1, N'Prius 2018',	N'2017.10.01',		dbo.fn_Lookup(@VehicleMakeLKD, N'Toyota'),		N'BX54662'),--1
	(2, N'Minivan 2019',N'2018.12.01' ,		dbo.fn_Lookup(@VehicleMakeLKD, N'Mercedes'),	N'AA100000'),
	(3, N'Minivan 2019',N'2018.12.01' ,		dbo.fn_Lookup(@VehicleMakeLKD, N'Mercedes'),	N'LM999812');

	EXEC [api].[Resources__Save]
		@DefinitionId = @MotorVehiclesMemberRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END