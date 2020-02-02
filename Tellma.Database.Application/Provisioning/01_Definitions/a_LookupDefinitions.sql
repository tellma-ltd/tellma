DECLARE @LookupDefinitions dbo.LookupDefinitionList;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],								[TitleSingular],		[TitlePlural]) VALUES
	(0,N'it-equipment-manufacturers',	N'IT Manufacturer',		N'IT Manufacturers'),
	(1,N'operating-systems',			N'Operating System',	N'Operating Systems');
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],								[TitleSingular],		[TitlePlural]) VALUES
	(0,N'it-equipment-manufacturers',	N'IT Manufacturer',		N'IT Manufacturers'),
	(1,N'operating-systems',			N'Operating System',	N'Operating Systems');
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],				[TitleSingular],	[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
	(0,N'body-colors',	N'Body Color',		N'机身颜色',			N'Body Colors',	N'车身颜色'),
	(1,N'vehicle-makes',N'Vehicle Make',	N'车辆型号',			N'Vehicle Makes',N'车辆型号');
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],					[TitleSingular],[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES
	(0,N'steel-thicknesses',N'Thickness',	N'ውፍረት',			N'Thicknesses',		N'ወፍራም'),
	(1,N'vehicle-makes',	N'Vehicle Make',N'የተሽከርካሪ ሞዴል',	N'Vehicle Makes',	N'የተሽከርካሪ ሞዴሎች');
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],						[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES
	(0,N'paper-types',			N'Paper Type',		N'نوع الورقة',		N'Paper Types',		N'أنواع الورق'),
	(1,N'paper-sizes',			N'Paper Size',		N'مقاس الورق',		N'Paper Sizes',		N'مقاسات الورق'),
	(2,N'paper-weights',		N'Paper Weight'	,	N'وزن الورق',		N'Paper Weights',	N'أوزان الورق');
END

EXEC dal.LookupDefinitions__Save
	@Entities = @LookupDefinitions

IF @DebugLookupDefinitions = 1
	SELECT * FROM dbo.LookupDefinitions;