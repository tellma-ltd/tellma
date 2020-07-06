IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],			[Identifier]) VALUES
	(0,	N'Office Chair',N'MA'),
	(1,	N'Office Chair',N'AA');

	EXEC [api].[Resources__Save]
		@DefinitionId = @OtherPropertyPlantAndEquipmentMemberRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END