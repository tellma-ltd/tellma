IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	DECLARE @PaperProducts dbo.ResourceList;
	DECLARE @PM INT = (SELECT [Id] FROM dbo.AccountTypes WHERE [Code] = N'Merchandise');
	INSERT INTO @PaperProducts ([Index],
	--N'Merchandise'
		[AccountTypeId],[Name],				[Code],				[MassUnitId],				[CountUnitId]) VALUES
	(0,	@PM,			N'Bond/A4/White/80',N'B4W80',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(1, @PM,			N'Bond/A3/White/80',N'B3W80',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(2, @PM,			N'Bond/A4/Beige/80',N'B4B80',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(3, @PM,			N'Bond/A3/Beige/80',N'B3B80',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(4, @PM,			N'Bond/A4/White/100',N'B4W100',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs')),
	(5, @PM,			N'Bond/A3/White/100',N'B3W100',			dbo.fn_UnitName__Id(N'Kg'),	dbo.fn_UnitName__Id(N'pcs'));

	EXEC [api].[Resources__Save]
		@DefinitionId = N'paper-products',
		@Entities = @paperProducts,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting paper products: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT N'paper-products' AS [Resource Definition]
		DECLARE @paperProductsIds dbo.IdList;
		INSERT INTO @paperProductsIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'paper-products';

		SELECT [Name] AS 'paper Prooduct', [MassUnit] AS 'Weight In', [CountUnit] AS 'Count In'
		FROM rpt.Resources(@paperProductsIds);
	END
END