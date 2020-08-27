IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions ([Index],
		[Code],			[TitlePlural],		[TitleSingular], [DefaultUnitId], [DefaultUnitMassUnitId]) VALUES
	(0, N'FinishedSteel',	N'Steel Products',	N'Steel products', @pcs,			@Kg);

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		
	DECLARE @FinishedSteelRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedSteel');

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
	--N'FinishedGoods'
		[Name],				[Code],				[UnitMass], [CurrencyId]) VALUES
	(0,	N'CHS-76X2.0',		N'CHS76X2.0',		22.12,		@ETB),
	(1, N'CHS-200x3.8',		N'CHS200x3.8',		110.68,		@ETB),
	(2, N'RHS-120x80x2.8',	N'RHS120x80x2.8',	51.95,		@ETB),
	(3, N'RHS-30x20x2.8',	N'RHS30x20x2.8',	12.01,		@ETB),
	(4, N'L-38x1.1',		N'L38x1.1',			7.66,		@ETB),
	(5, N'L-38x1.2',		N'L38x1.2',			8.82,		@ETB);

	EXEC [api].[Resources__Save]
		@DefinitionId = @FinishedSteelRD,
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting steel products: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END