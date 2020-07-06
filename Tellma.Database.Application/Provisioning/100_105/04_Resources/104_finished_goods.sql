IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Id],				[TitlePlural],			[TitleSingular]) VALUES
	( N'steel-products',	N'Steel Products',		N'Steel products');

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		
	SET @PId= (SELECT [Id] FROM dbo.[AccountTypes] WHERE [Concept] = N'FinishedGoods');					

	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
	--N'FinishedGoods'
		[Name],				[Code]) VALUES
	(0,	N'CHS-76X2.0',		N'CHS76X2.0'),
	(1, N'CHS-200x3.8',		N'CHS200x3.8'),
	(2, N'RHS-120x80x2.8',	N'RHS120x80x2.8'),
	(3, N'RHS-30x20x2.8',	N'RHS30x20x2.8'),
	(4, N'L-38x1.1',		N'L38x1.1'),
	(5, N'L-38x1.2',		N'L38x1.2');

	--INSERT INTO @ResourceUnits([Index], [HeaderIndex],
	--		[UnitId],					[Multiplier]) VALUES
	--(0, 0, dbo.fn_UnitName__Id(N'pcs'),	1),
	--(1, 0, dbo.fn_UnitName__Id(N'kg'),	22.12),
	--(0, 1, dbo.fn_UnitName__Id(N'pcs'),	1),
	--(1, 1, dbo.fn_UnitName__Id(N'kg'),	110.68),
	--(0, 2, dbo.fn_UnitName__Id(N'pcs'),	1),
	--(1, 2, dbo.fn_UnitName__Id(N'kg'),	51.195),
	--(0, 3, dbo.fn_UnitName__Id(N'pcs'),	1),
	--(1, 3, dbo.fn_UnitName__Id(N'kg'),	12.01),
	--(0, 4, dbo.fn_UnitName__Id(N'pcs'),	1),
	--(1, 4, dbo.fn_UnitName__Id(N'kg'),	7.66),
	--(0, 5, dbo.fn_UnitName__Id(N'pcs'),	1),
	--(1, 5, dbo.fn_UnitName__Id(N'kg'),	8.82);

	EXEC [api].[Resources__Save]
		@DefinitionId = N'steel-products',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting steel products: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
END