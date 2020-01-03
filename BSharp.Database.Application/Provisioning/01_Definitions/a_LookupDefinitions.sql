DECLARE @LookupDefinitions dbo.LookupDefinitionList;

IF @DB = N'100' -- ACME SAR, en/ar/zh
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],				[TitleSingular],	[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
	(0,N'body-colors',	N'Body Color',		N'اللون',			N'Body Colors',	N'الألوان'),
	(1,N'vehicle-makes',N'Vehicle Make',	N'الموديل',			N'Vehicle Makes',N'الموديلات');
END

ELSE IF @DB = N'101' -- Banan SD, USD, en
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
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],				[TitleSingular],	[TitleSingular2],	[TitlePlural],	[TitlePlural2]) VALUES
	(0,N'body-colors',	N'Body Color',		N'اللون',			N'Body Colors',	N'الألوان'),
	(1,N'vehicle-makes',N'Vehicle Make',	N'الموديل',			N'Vehicle Makes',N'الموديلات');
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @LookupDefinitions([Index],
	[Id],						[TitleSingular],		[TitlePlural]) VALUES
	(1,N'steel-thicknesses',	N'Thickness',			N'Thicknesses');
END

EXEC dal.LookupDefinitions__Save
	@Entities = @LookupDefinitions

IF @DebugLookupDefinitions = 1
	SELECT * FROM dbo.LookupDefinitions;