IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],			[Identifier]) VALUES
	(0,	N'Office Chair',N'MA'),
	(1,	N'Office Chair',N'AA');

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
	[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1);

	EXEC [api].[Resources__Save]
		@DefinitionId = @property_plant_equipmentRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END