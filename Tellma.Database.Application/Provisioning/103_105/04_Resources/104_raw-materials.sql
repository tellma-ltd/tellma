IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Code],		[TitlePlural],	[TitleSingular],	[IdentifierLabel], [IdentifierVisibility], [DefaultUnitId]) VALUES
	( N'SteelRoll',	N'Steel Rolls',	N'Steel Roll',		N'Roll #',			N'Optional',			@mt);

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		
	DECLARE @SteelRollRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'SteelRoll');
	DELETE FROM @Resources;
	INSERT INTO @Resources ([Index],
		[Name],				[Code],			[Identifier], [UnitMass], [UnitMassUnitId],	[Lookup1Id]) VALUES
	(0, N'HR 1000MMx1.9MM',	N'HR1000x1.9',	N'1001',		23.332,		@Kg,			dbo.fn_Lookup(N'SteelThickness', N'1.9')),
	(1, N'CR 1000MMx1.4MM',	N'CR1000x1.4',	N'1002',		11.214,		@Kg,			dbo.fn_Lookup(N'SteelThickness', N'1.4'));
	EXEC [api].[Resources__Save]
		@DefinitionId = @SteelRollRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting raw materials: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END