DECLARE @LookupDefinitions dbo.LookupDefinitionList;

IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],									[TitleSingular],			[TitlePlural],				[MainMenuIcon],		[MainMenuSection], [MainMenuSortKey]) VALUES
	(0,N'it-equipment-manufacturers',		N'IT Manufacturer',			N'IT Manufacturers',		N'microchip',		N'Administration',		100),
	(1,N'operating-systems',				N'Operating System',		N'Operating Systems',		N'laptop-code',		N'Administration',		200);
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],									[TitleSingular],			[TitlePlural]) VALUES
	(0,N'it-equipment-manufacturers',		N'IT Manufacturer',			N'IT Manufacturers'),
	(1,N'operating-systems',				N'Operating System',		N'Operating Systems');
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],					[TitleSingular],	[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
	(0,N'body-colors',		N'Body Color',		N'机身颜色',			N'Body Colors',	N'车身颜色'),
	(1,N'vehicle-makes',	N'Vehicle Make',	N'车辆型号',			N'Vehicle Makes',N'车辆型号');
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],					[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES
	(0,N'steel-thicknesses',N'Thickness',		N'ውፍረት',			N'Thicknesses',		N'ወፍራም'),
	(1,N'vehicle-makes',	N'Vehicle Make',	N'የተሽከርካሪ ሞዴል',	N'Vehicle Makes',	N'የተሽከርካሪ ሞዴሎች');
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],					[TitleSingular],	[TitleSingular2],	[TitlePlural],		[TitlePlural2]) VALUES
	(0,N'paper-origins',	N'Paper Origin',	N'مصدر الورق',		N'Paper Origins',	N'مصادر الورق'),
	(1,N'paper-groups',		N'Paper Group',		N'مجموعة الورق',	N'Paper Groups',	N'مجموعات الورق'),
	(2,N'paper-types',		N'Paper Type',		N'نوع الورق',		N'Paper Types',		N'أنواع الورق');
END

EXEC dal.LookupDefinitions__Save
	@Entities = @LookupDefinitions;