IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Identifier],	[Name],											[Lookup2Id]) VALUES
		-- N'Vehicles'
	(0, N'101',		N'Toyota Camry 2018 Navy Blue/White/Leather',	dbo.fn_Lookup(@BodyColorLKD, N'Navy Blue')),
	(1, N'102',		N'Toyota Camry 2018 Black/Silver/Wool',			dbo.fn_Lookup(@BodyColorLKD, N'Black')),
	(2, N'199',		N'Fake',										NULL),--1
	(3, N'201',		N'Toyota Yaris 2018 White/White/Leather',		dbo.fn_Lookup(@BodyColorLKD, N'White'));--1

	EXEC [api].[Resources__Save]
		@DefinitionId = @FinishedVehicleRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Finished Vehicles: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END