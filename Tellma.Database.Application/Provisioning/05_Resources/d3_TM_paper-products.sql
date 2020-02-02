IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	DECLARE @PaperProducts dbo.ResourceList;
	INSERT INTO @PaperProducts ([Index],
	--N'Merchandise'
		[AccountTypeId],[Name],								[Name2],								[Code],
		[Lookup1Id],										[Lookup2Id],												
		[MassUnitId],										[CountUnitId]) VALUES
	(0, @Merchandise,	N'رول جريدة أبيض فنلندي ستورا',	N'Newspaper Roll White Finnish Estora',	N'2504-66011',
		[dbo].[fn_Lookup](N'paper-origins', N'Finnish'),	[dbo].[fn_Lookup](N'paper-groups', N'Newspaper Roll paper'),
		dbo.fn_UnitName__Id(N'mt'),							NULL),

	(1,	@Merchandise,	N'مكربن أولى أبيض - فونكس',		N'Carbonless Coated Paper White Phoenix',N'0200-01231',
		[dbo].[fn_Lookup](N'paper-origins', N'Thai'),		[dbo].[fn_Lookup](N'paper-groups', N'Carbonless Coated paper'),
		dbo.fn_UnitName__Id(N'mt'),							dbo.fn_UnitName__Id(N'pkt'))
		
	;

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