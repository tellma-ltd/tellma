INSERT INTO dbo.Users([Id],[ExternalId],[Email],[LastAccess],[PermissionsVersion],[UserSettingsVersion],[CreatedAt],[CreatedById],[ModifiedAt],[ModifiedById]) VALUES
(1, N'ae472a50-a2c0-4a5d-8f27-96d8b1981933', N'admin@bsharp.online', N'2019-09-29 18:52:11.4376462 +01:00', N'F4C6CFF3-AC59-4FA4-BA33-6D50955D2423', N'21163B9A-C14C-4E4A-B640-48BD58B64898' , N'2019-08-31 14:39:27.7071184 +01:00' , 1 , N'2019-09-26 14:22:25.9091397 +01:00' , 1),
(4, N'd6a1f79f-c01a-4a02-8a00-494d91e7f412', N'ahmadakra1990@gmail.com', N'2019-09-04 13:15:00.5285569 +01:00', N'BC7F67DE-5ECD-48DC-A530-0587593AB06F', N'FEEF3A6D-E854-4283-992C-7775B81151B2' , N'2019-09-04 13:14:31.3021930 +01:00' , 1 , N'2019-09-05 17:06:01.2963275 +01:00' , 1),
(6, N'877b6f89-8ae6-4985-9738-1f5b57a7ea6e', N'ahmad.akra@cantab.net', N'2019-09-29 09:28:01.8997256 +01:00', N'7A7C5302-FCD3-4554-90BB-DE27749CF682', N'DA3E2F0F-938A-4D00-9AE6-88C2D14FC540' , N'2019-09-05 17:17:28.4581110 +01:00' , 1 , N'2019-09-05 17:17:28.4581110 +01:00' , 1);

INSERT INTO [dbo].[UserSettings]([UserId],[Key],[Value]) VALUES
(1, N'Agent/select', N'Code,AgentType,PreferredLanguage,IsRelated,IsActiv'),
(1, N'Currency/select', N'Description,Description2,Description3,E,IsActive,C'),
(1, N'MeasurementUnit/select', N'Description,UnitType,UnitAmount,BaseAmount'),
(1, N'Resource/finished-goods/select', N'ResourceLookup1,ResourceClassification, Code,Curre'),
(1, N'Resource/raw-materials/select', N'ResourceLookup1,ResourceClassification,MassUnit,Is'),
(1, N'ResourceClassification/finished-goods/select', N'Code,IsLeaf,IsActive'),
(1, N'ResourceLookup/thicknesses/select', N'IsActive'),
(1, N'Role/select', N'Code,IsPublic,IsActive,SavedBy'),
(1, N'User/select', N'Email,State,LastAccess,Agent'),
(6, N'MeasurementUnit/select', N'Name,Code,UnitType,UnitAmount,BaseAmount,IsActive');

INSERT INTO dbo.Settings( ShortCompanyName,ShortCompanyName2, ShortCompanyName3, PrimaryLanguageId, PrimaryLanguageSymbol, SecondaryLanguageId, SecondaryLanguageSymbol, TernaryLanguageId, TernaryLanguageSymbol, DefinitionsVersion,	SettingsVersion,	[FunctionalCurrencyId],	CreatedById, ModifiedById)
VALUES( N'ACME International', N'أكمي العالمية', N'ACME国际', N'en', N'English', N'ar', N'العربية', N'zh', N'中文', N'80207F1A-3D1E-4074-B280-0F6316FB94BD', N'6726AEBE-1FC2-4098-9F19-A7253BF39E23', N'ETB', 1,	1)

/*
SELECT 
		'(' + cast([Id] as nvarchar(50)) + ', N'''  + [ResourceDefinitionId] + ''', ' +  ISNULL(cast([ResourceClassificationId] as nvarchar(50)), N'NULL')
      + ', N''' + [Name] + ''', ' + COALESCE('N''' + [Name2] + '''', 'NULL') + ', '
      + COALESCE('N''' + [Name3] + '''', 'NULL') + ', '
      + COALESCE('N''' + [Code] + '''', 'NULL') + ', '
	   +  ISNULL(cast([CountUnitId] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([MassUnitId] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([VolumeUnitId] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([LengthUnitId] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([TimeUnitId] as nvarchar(50)), N'NULL') + ', '
      + COALESCE('N''' + [CurrencyId] + '''', 'NULL') + ', '	
      + COALESCE('N''' + [Memo] + '''', 'NULL') + ', '		   
      + COALESCE('N''' + [CustomsReference] + '''', 'NULL') + ', '	
	   +  ISNULL(cast([ResourceLookup1Id] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([ResourceLookup2Id] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([CreatedById] as nvarchar(50)), N'NULL') + ', '
	   	   +  ISNULL(cast([ModifiedById] as nvarchar(50)), N'NULL') + '),'
  FROM [BSharp.101].[dbo].[Resources]
*/
INSERT INTO [dbo].[Resources](
[Id], [ResourceDefinitionId], [ResourceClassificationId], [Name], [Name2], [Name3], [Code], [CountUnitId], [MassUnitId],[VolumeUnitId], [LengthUnitId], [TimeUnitId], [MonetaryValueCurrencyId], [Description], [CustomsReference], [Lookup1Id],[Lookup2Id], [CreatedById], [ModifiedById]) VALUES
(2, N'raw-materials', NULL, N'HR 1000mx0.8mm', NULL, NULL, N'HR 1000x0.8', NULL, 81, NULL, NULL, NULL, NULL, N'ETB', N'My default memo', NULL, 16, NULL, 1, 1),
(3, N'raw-materials', NULL, N'HR 1000mx0.9mm', NULL, NULL, N'HR 1000x0.9', NULL, 81, 83, 86, 72, 72, N'USD', N'This is a memo', N'398254', 17, 27, 1, 1),
(5, N'finished-goods', 4, N'Bucket - Blue', N'دلو أزرق', N'斗-蓝色', N'01', NULL, NULL, NULL, NULL, NULL, NULL, N'ETB', NULL, NULL, 3, NULL, 1, 1),
(6, N'finished-goods', 8, N'Bucket - Red', N'دلو أحمر', N'斗-红色', N'02', NULL, NULL, NULL, NULL, NULL, NULL, N'ETB', NULL, NULL, 1, NULL, 1, 1),
(7, N'finished-goods', 6, N'Bucket - Grey', N'دلو رمادي', N'斗-灰色', N'03', NULL, NULL, NULL, NULL, NULL, NULL, N'ETB', NULL, NULL, 10, NULL, 1, 1),
(8, N'finished-goods', 7, N'Bucket - Orange', N'دلو برتقالي', N'斗-橙色', N'04', NULL, NULL, NULL, NULL, NULL, NULL, N'ETB', NULL, NULL, 4, NULL, 1, 1),
(10, N'raw-materials', 3, N'HR 1000mx1.0mm', NULL, N'撤销删除', N'HR 1000x1.0', 68, 81, 83, 86, 72, 72, N'USD', N'My default memo', NULL, 18, 30, 1, 1);

/*
SELECT 
		'(' + cast([Id] as nvarchar(50)) + ', '
	+	ISNULL(cast([UnitType] as nvarchar(50)), N'NULL') + ', '
	  + COALESCE('N''' + [Name] + '''', 'NULL') + ', '
	  + COALESCE('N''' + [Name2] + '''', 'NULL') + ', '
      + COALESCE('N''' + [Description] + '''', 'NULL') + ', '
	   +  ISNULL(cast([UnitAmount] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([BaseAmount] as nvarchar(50)), N'NULL') + ', '
	   +  ISNULL(cast([CreatedById] as nvarchar(50)), N'NULL') + ', '
	   	   +  ISNULL(cast([ModifiedById] as nvarchar(50)), N'NULL') + '),'
		    FROM [BSharp.101].[dbo].[MeasurementUnits]
*/
INSERT INTO [dbo].[MeasurementUnits]([Id],[UnitType],[Name],[Name2],[Description],[UnitAmount],[BaseAmount],[CreatedById],[ModifiedById]) VALUES
(66, Count, N'ea', NULL, N'Each', 1, 1, 1, 1),
(67, Count, N'share', NULL, N'Shares', 1, 1, 1, 1),
(68, Count, N'pcs', NULL, N'Pieces', 1, 1, 1, 1),
(69, Time, N's', NULL, N'Second', 1, 1, 1, 1),
(70, Time, N'min', NULL, N'Minute', 1, 60, 1, 1),
(71, Time, N'hr', NULL, N'Hour', 1, 3600, 1, 1),
(72, Time, N'd', N'أيام', N'Day', 1, 86400, 1, 1),
(73, Time, N'mo', NULL, N'Month', 1, 2.592e+006, 1, 1),
(74, Time, N'yr', NULL, N'Year', 1, 3.1104e+007, 1, 1),
(75, Time, N'wd', NULL, N'Work Day', 1, 8, 1, 1),
(76, Time, N'wk', NULL, N'Week', 1, 604800, 1, 1),
(77, Time, N'wmo', NULL, N'Work Month', 1, 1248, 1, 1),
(78, Time, N'wwk', NULL, N'Work Week', 1, 48, 1, 1),
(79, Time, N'wyr', NULL, N'Work Year', 1, 14976, 1, 1),
(80, Mass, N'g', NULL, N'Gram', 1, 1, 1, 1),
(81, Mass, N'kg', NULL, N'Kilogram', 1, 1000, 1, 1),
(82, Mass, N'mt', NULL, N'Metric ton', 1, 1e+006, 1, 1),
(83, Volume, N'ltr', NULL, N'Liter', 1, 1, 1, 1),
(84, Volume, N'usg', NULL, N'US Gallon', 1, 3.78541, 1, 1),
(85, Distance, N'cm', NULL, N'Centimeter', 1, 1, 1, 1),
(86, Distance, N'm', NULL, N'Meter', 1, 100, 1, 1),
(87, Distance, N'in', N'إن', N'inch', 1, 2.541, 1, 1),
(88, Distance, N'ly', NULL, N'Lightyear', 1, 1, 1, 1);