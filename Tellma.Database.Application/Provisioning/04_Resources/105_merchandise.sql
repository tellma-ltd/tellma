IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	DELETE FROM @Resources; DELETE FROM @ResourceUnits;
	INSERT INTO @Resources ([Index],
	--N'Merchandise'
		[AccountTypeId],[Name2],							[Name],								[Code],
		[Lookup1Id],										[Lookup2Id],												
		[Lookup3Id],										[Decimal1],			[Decimal2],
		[Text1],											[Text2],			[Int1],			[Int2]) VALUES
	(0, @Merchandise,	N'رول جريدة أبيض فنلندي ستورا',	N'Newspaper Roll White Finnish Estora',	N'2504-66011',
		[dbo].[fn_Lookup](N'paper-origins', N'Finnish'),	[dbo].[fn_Lookup](N'paper-groups', N'Newspaper Roll paper'),
		[dbo].[fn_Lookup](N'paper-types', N'Newspaper'),	66,					0,
		N'White - أبيض',									N'50-66',			49,				8),

	(1,	@Merchandise,	N'مكربن أولى أبيض - فونكس',		N'Carbonless Coated Paper White Phoenix',N'0200-01231',
		[dbo].[fn_Lookup](N'paper-origins', N'Thai'),		[dbo].[fn_Lookup](N'paper-groups', N'Carbonless Coated paper'),
		[dbo].[fn_Lookup](N'paper-types', N'Commercial'),	70,					100,	
		N'White - أبيض',									N'70-100',			56,				1),
	(2, @Merchandise,	N'كربون لامع 70-100\130غ',			N'2/S Coated Paper Glazed - 70x100/130 g',	N'3300-01201',
		[dbo].[fn_Lookup](N'paper-origins', N'Finnish'),	[dbo].[fn_Lookup](N'paper-groups', N'Coated Paper Glazed'),
		[dbo].[fn_Lookup](N'paper-types', N'Commercial'),	70,					100,
		N'White - أبيض',									N'70-100',			130,			6),
	(3,	@Merchandise,	N'كربون لامع 60-90\170غ',			N'2/S Coated Paper Glazed - 60x90/170 g',	N'3300-01202',
		[dbo].[fn_Lookup](N'paper-origins', N'German'),		[dbo].[fn_Lookup](N'paper-groups', N'Coated Paper Glazed'),
		[dbo].[fn_Lookup](N'paper-types', N'Commercial'),	60,					90,
		N'White - أبيض',									N'60-90',			170,			4),

	(4, @Merchandise,	N'كربون لامع 62-92\200غ',			N'2/S Coated Paper Glazed - 62x92/200 g',	N'3300-01203',
		[dbo].[fn_Lookup](N'paper-origins', N'German'),	[dbo].[fn_Lookup](N'paper-groups', N'Coated Paper Glazed'),
		[dbo].[fn_Lookup](N'paper-types', N'Commercial'),	62,					92,
		N'White - أبيض',									N'62-92',			200,			7),

	(5,	@Merchandise,	N'كربون لامع 62-92\300غ',			N'2/S Coated Paper Glazed - 62x92/300 g',	N'3300-01204',
		[dbo].[fn_Lookup](N'paper-origins', N'German'),		[dbo].[fn_Lookup](N'paper-groups', N'Coated Paper Glazed'),
		[dbo].[fn_Lookup](N'paper-types', N'Commercial'),	62,					92,
		N'White - أبيض',									N'62-92',			300,			7),
	(6, @Merchandise,	N'كربون لامع 70-100\250غ',			N'2/S Coated Paper Glazed - 70x100/250 g',	N'3300-01205',
		[dbo].[fn_Lookup](N'paper-origins', N'German'),	[dbo].[fn_Lookup](N'paper-groups', N'Coated Paper Glazed'),
		[dbo].[fn_Lookup](N'paper-types', N'Commercial'),	70,					100,
		N'Color - ملون',									N'70-100',			250,			2);

	INSERT INTO @ResourceUnits([Index], [HeaderIndex],
			[UnitId],						[Multiplier]) VALUES
	(0, 0, dbo.fn_UnitName__Id(N'mt'),		1),
	(0, 1, dbo.fn_UnitName__Id(N'mt'),		1),
	(1, 1, dbo.fn_UnitName__Id(N'500pkt'),	19600),
	(0, 2, dbo.fn_UnitName__Id(N'mt'),		1),
	(1, 2, dbo.fn_UnitName__Id(N'500pkt'),	20000),
	(0, 3, dbo.fn_UnitName__Id(N'mt'),		1),
	(1, 3, dbo.fn_UnitName__Id(N'500pkt'),	22000),
	(0, 4, dbo.fn_UnitName__Id(N'mt'),		1),
	(1, 4, dbo.fn_UnitName__Id(N'500pkt'),	24000),
	(0, 5, dbo.fn_UnitName__Id(N'mt'),		1),
	(1, 5, dbo.fn_UnitName__Id(N'500pkt'),	26000),
	(0, 6, dbo.fn_UnitName__Id(N'mt'),		1),
	(1, 6, dbo.fn_UnitName__Id(N'500pkt'),	28000);


	EXEC [api].[Resources__Save]
		@DefinitionId = N'paper-products',
		@Entities = @Resources,
		@ResourceUnits = @ResourceUnits,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting paper products: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	DECLARE @R0 INT, @R1 INT, @R2 INT, @R3 INT, @R4 INT, @R5 INT, @R6 INT, @R7 INT;
	SELECT @R0 = [Id] FROM dbo.Resources WHERE [Code] = N'2504-66011';
	SELECT @R1 = [Id] FROM dbo.Resources WHERE [Code] = N'0200-01231';
	SELECT @R2 = [Id] FROM dbo.Resources WHERE [Code] = N'3300-01201';
	SELECT @R3 = [Id] FROM dbo.Resources WHERE [Code] = N'3300-01202';
	SELECT @R4 = [Id] FROM dbo.Resources WHERE [Code] = N'3300-01203';
	SELECT @R5 = [Id] FROM dbo.Resources WHERE [Code] = N'3300-01204';
	SELECT @R6 = [Id] FROM dbo.Resources WHERE [Code] = N'3300-01205';

END