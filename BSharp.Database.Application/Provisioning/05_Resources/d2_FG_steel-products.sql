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
		Print 'Resource Definitions: Inserting'
		GOTO Err_Label;
	END;		

	DELETE FROM @ResourceClassificationsTemp;
	INSERT INTO @ResourceClassificationsTemp
	([Code],	[Name],	[IsAssignable], [Node],	[Index]) VALUES
	(N'D',		N'D',	1,				N'/1/',	0),
	(N'HSP',	N'HSP',	0,				N'/2/', 1),
	(N'CHS',	N'CHS',	1,				N'/2/1/', 2),
	(N'RHS',	N'RHS',	1,				N'/2/2/', 3),
	(N'SHS',	N'SHS',	1,				N'/2/3/', 4),
	(N'LTZ',	N'LTZ',	0,				N'/3/', 5),
	(N'L',		N'L',	1,				N'/3/1/', 6),
	(N'T',		N'T',	1,				N'/3/2/', 7),
	(N'Z',		N'Z',	1,				N'/3/3/', 8),
	(N'SM',		N'SM',	1,				N'/4/', 9),
	(N'CP',		N'CP',	1,				N'/5/', 10),
	(N'Other',	N'Other',1,				N'/6/', 11);
	
	DELETE FROM @ResourceClassifications
	INSERT INTO @ResourceClassifications (
	[ResourceDefinitionId],		[Code], [Name], [ParentIndex], [IsAssignable], [Index])
	SELECT N'steel-products', [Code], [Name], (SELECT [Index] FROM @ResourceClassificationsTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [IsAssignable], [Index]
	FROM @ResourceClassificationsTemp RC

	EXEC [api].[ResourceClassifications__Save]
		@Entities = @ResourceClassifications,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Classifications: Provisioning'
		GOTO Err_Label;
	END;												

	DECLARE @SteelProducts dbo.ResourceList;
	INSERT INTO @SteelProducts ([Index],
	--N'FinishedGoods'
		[ResourceClassificationId], [Name],				[Code],				[MassUnitId],				[CountUnitId]) VALUES
	(0,	dbo.fn_RCCode__Id(N'CHS'),	N'CHS-76X2.0',		N'CHS76X2.0',		dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(1, dbo.fn_RCCode__Id(N'CHS'),	N'CHS-200x3.8',		N'CHS200x3.8',		dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(2, dbo.fn_RCCode__Id(N'RHS'),	 N'RHS-120x80x2.8',	N'RHS120x80x2.8',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(3, dbo.fn_RCCode__Id(N'RHS'),	N'RHS-30x20x2.8',	N'RHS30x20x2.8',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(4, dbo.fn_RCCode__Id(N'L'),	N'L-38x1.1',		N'L38x1.1',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(5, dbo.fn_RCCode__Id(N'L'),	N'L-38x1.2',		N'L38x1.2',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs'));

	EXEC [api].[Resources__Save]
		@DefinitionId = N'steel-products',
		@Entities = @SteelProducts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting steel products'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT N'steel-products' AS [Resource Definition]
		DECLARE @SteelProductsIds dbo.IdList;
		INSERT INTO @SteelProductsIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'steel-products';

		SELECT [Name] AS 'Steel Prooduct', [MassUnit] AS 'Weight In', [CountUnit] AS 'Count In'
		FROM rpt.Resources(@SteelProductsIds);
	END
END