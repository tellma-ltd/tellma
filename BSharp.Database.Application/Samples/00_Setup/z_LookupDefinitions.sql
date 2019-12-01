INSERT INTO dbo.LookupDefinitions
([Id],							[TitleSingular],		[TitlePlural]) VALUES
(N'body-colors',				N'Body Color',			N'Body Colors'),
(N'vehicle-makes',				N'Vehicle Makes',		N'Vehicle Make'),
(N'steel-thicknesses',			N'Thicknesses',			N'Thickness'),
(N'it-equipment-manufacturers', N'IT Manufacturers',	N'IT Manufacturer'),
(N'operating-systems',			N'Operating Systems',	N'Operating System');

IF @DebugLookupDefinitions = 1
	SELECT * FROM dbo.LookupDefinitions;