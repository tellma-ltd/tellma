	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Id],					[TitlePlural],				[TitleSingular],	[IdentifierLabel]) VALUES
	(N'general-fixed-assets',	N'General fixed assets',	N'General fixed asset',	N'Used By');

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
		[AccountTypeId],							[Name],			[Identifier]) VALUES
	(0, dbo.fn_ATCode__Id(N'FixturesAndFittings'),	N'Office Chair',N'MA'),
	(1, dbo.fn_ATCode__Id(N'FixturesAndFittings'),	N'Office Chair',N'AA');

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
	[UnitId],					[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'yr'),	1),
	(0, 1, dbo.fn_UnitName__Id(N'yr'),	1);

	EXEC [api].[Resources__Save]
		@DefinitionId = N'general-fixed-assets',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;