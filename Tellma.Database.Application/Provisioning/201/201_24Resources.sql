	SET @DefinitionID = @ComputerEquipmentMemberRD; DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[Name],						[UnitId], [CurrencyId]) VALUES
	(0,	N'Dell 2320',				@wmo,		N'USD');

	EXEC [api].[Resources__Save]
		@DefinitionId = @DefinitionID,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Computer Equipment: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
