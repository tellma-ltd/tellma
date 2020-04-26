IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AccountTypeId],						[Name],			[Identifier]) VALUES
	(0, dbo.fn_ATCode__Id(N'OfficeEquipment'),	N'Office Chair',N'MA'),
	(1, dbo.fn_ATCode__Id(N'OfficeEquipment'),	N'Office Chair',N'AA');

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
	[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1);

	EXEC [api].[Resources__Save]
		@DefinitionId = @properties_plants_and_equipmentDef,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END