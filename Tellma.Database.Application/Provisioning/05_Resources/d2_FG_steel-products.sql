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
	SET @PId= (SELECT [Id] FROM dbo.[AccountTypes] WHERE [Code] = N'FinishedGoods');
	DELETE FROM @AccountTypes;
	INSERT INTO @AccountTypes([ParentId],[Index], [ParentIndex],
					[Code],		[Name],	[IsAssignable]--, [Node]
																	) VALUES
	(@PId,0,NULL,	N'D',		N'D',	1),--			N'/1/11/5/1/'),
	(@PId,1,NULL,	N'HSP',		N'HSP',	0),--			N'/1/11/5/2/'),
	(NULL,2,1,		N'CHS',		N'CHS',	1),--			N'/1/11/5/2/1/'),
	(NULL,3,1,		N'RHS',		N'RHS',	1),--			N'/1/11/5/2/2/'),
	(NULL,4,1,		N'SHS',		N'SHS',	1),--			N'/1/11/5/2/3/'),
	(@PId,5,NULL,	N'LTZ',		N'LTZ',	0),--			N'/1/11/5/3/'),
	(NULL,6,5,		N'L',		N'L',	1),--			N'/1/11/5/3/1/'),
	(NULL,7,5,		N'T',		N'T',	1),--			N'/1/11/5/3/2/'),
	(NULL,8,5,		N'Z',		N'Z',	1),--			N'/1/11/5/3/3/'),
	(@PId,9,NULL,	N'SM',		N'SM',	1),--			N'/1/11/5/4/'),
	(@PId,10,NULL,	N'CP',		N'CP',	1),--			N'/1/11/5/5/'),
	(@PId,11,NULL,	N'Other',	N'Other',1);--			N'/1/11/5/6/');
	
	EXEC [api].[AccountTypes__Save]
		@Entities = @AccountTypes,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Classifications: Provisioning: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;												

	DECLARE @SteelProducts dbo.ResourceList;
	INSERT INTO @SteelProducts ([Index],
	--N'FinishedGoods'
		[AccountTypeId],			[Name],				[Code],				[MassUnitId],				[CountUnitId]) VALUES
	(0,	dbo.fn_ATCode__Id(N'CHS'),	N'CHS-76X2.0',		N'CHS76X2.0',		dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(1, dbo.fn_ATCode__Id(N'CHS'),	N'CHS-200x3.8',		N'CHS200x3.8',		dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(2, dbo.fn_ATCode__Id(N'RHS'),	 N'RHS-120x80x2.8',	N'RHS120x80x2.8',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(3, dbo.fn_ATCode__Id(N'RHS'),	N'RHS-30x20x2.8',	N'RHS30x20x2.8',	dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(4, dbo.fn_ATCode__Id(N'L'),	N'L-38x1.1',		N'L38x1.1',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(5, dbo.fn_ATCode__Id(N'L'),	N'L-38x1.2',		N'L38x1.2',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs'));

	EXEC [api].[Resources__Save]
		@DefinitionId = N'steel-products',
		@Entities = @SteelProducts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting steel products: ' + @ValidationErrorsJson
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