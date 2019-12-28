IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	PRINT N'BSharp.' + @DB;
	INSERT INTO dbo.LookupDefinitions
	([Id],							[TitleSingular],		[TitlePlural]) VALUES
	(N'it-equipment-manufacturers', N'IT Manufacturers',	N'IT Manufacturer'),
	(N'operating-systems',			N'Operating Systems',	N'Operating System');
END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	PRINT N'BSharp.' + @DB;
	INSERT INTO dbo.LookupDefinitions
	([Id],							[TitleSingular],		[TitlePlural]) VALUES
	(N'it-equipment-manufacturers', N'IT Manufacturers',	N'IT Manufacturer'),
	(N'operating-systems',			N'Operating Systems',	N'Operating System');
	END
IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	PRINT N'BSharp.' + @DB;
	INSERT INTO dbo.LookupDefinitions
	([Id],				[TitleSingular],	[TitleSingular2],[TitlePlural],	[TitlePlural2]) VALUES
	(N'body-colors',	N'Body Color',		N'اللون',		N'Body Colors',	N'الألوان'),
	(N'vehicle-makes',	N'Vehicle Makes',	N'الموديل',		N'Vehicle Make',N'الموديلات');
END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	PRINT N'BSharp.' + @DB;
	INSERT INTO dbo.LookupDefinitions
	([Id],							[TitleSingular],		[TitlePlural]) VALUES
	(N'vehicle-makes',				N'Vehicle Makes',		N'Vehicle Make'),
	(N'steel-thicknesses',			N'Thicknesses',			N'Thickness');
END

IF @DebugLookupDefinitions = 1
	SELECT * FROM dbo.LookupDefinitions;