	DELETE FROM @Lookups; SET @DefinitionId = @ITEquipmentManufacturerLKD
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Dell'),
	(1,	N'HP'),
	(2,	N'Apple'),
	(3,	N'Microsoft'),
	(4, N'Lenovo');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lookups: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;						

	DELETE FROM @Lookups; SET @DefinitionId = @OperatingSystemLKD
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(1,	N'Windows 10'),
	(2,	N'Windows Server 2017'),
	(3,	N'iOS 13');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;