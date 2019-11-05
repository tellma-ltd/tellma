	INSERT INTO dbo.ResourceDefinitions (
		[Id],				[TitlePlural],			[TitleSingular],	[ResourceTypeParentList]) VALUES
	( N'steel-products',	N'Steel Products',		N'Steel products',	N'FinishedGoods');

	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'steel-products'
						[Name],	[IsLeaf],	[Node]) VALUES
	(N'steel-products',	N'D',		1,			N'/1/'),
	(N'steel-products',	N'HSP',		0,			N'/2/'),
	(N'steel-products',	N'CHS',		1,			N'/2/1/'),
	(N'steel-products',	N'RHS',		1,			N'/2/2/'),
	(N'steel-products',	N'SHS',		1,			N'/2/3/'),
	(N'steel-products',	N'LTZ',		0,			N'/3/'),
	(N'steel-products',	N'L',		1,			N'/3/1/'),
	(N'steel-products',	N'T',		1,			N'/3/2/'),
	(N'steel-products',	N'Z',		1,			N'/3/3/'),
	(N'steel-products',	N'SM',		1,			N'/4/'),
	(N'steel-products',	N'CP',		1,			N'/5/'),
	(N'steel-products',	N'Other',	1,			N'/6/');

	DECLARE @SteelProducts dbo.ResourceList;
	INSERT INTO @SteelProducts ([Index],
	[ResourceTypeId],	[ResourceClassificationId], [Name],				[Code],				[MassUnitId],				[CountUnitId]) VALUES
	(0, N'FinishedGoods',dbo.fn_RCName__Id(N'CHS'), N'CHS-76X2.0',		N'CHS76X2.0',		dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(1, N'FinishedGoods',dbo.fn_RCName__Id(N'CHS'),	N'CHS-200x3.8',		N'CHS200x3.8',		dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(2, N'FinishedGoods',dbo.fn_RCName__Id(N'RHS'), N'RHS-120x80x2.8',	N'RHS120x80x2.8',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(3, N'FinishedGoods',dbo.fn_RCName__Id(N'RHS'),	N'RHS-30x20x2.8',	N'RHS30x20x2.8',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(4, N'FinishedGoods',dbo.fn_RCName__Id(N'L'),	N'L-38x1.1',		N'L38x1.1',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(5, N'FinishedGoods',dbo.fn_RCName__Id(N'L'),	N'L-38x1.2',		N'L38x1.2',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs'));

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
	INSERT INTO @SteelProductsIds SELECT [Id] FROM dbo.Resources WHERE [ResourceDefinitionId] = N'steel-products';

	SELECT ResourceTypeId, [Name] AS 'Steel Prooduct', [MassUnit] AS 'Weight In', [CountUnit] AS 'Count In'
	FROM rpt.Resources(@SteelProductsIds);
END