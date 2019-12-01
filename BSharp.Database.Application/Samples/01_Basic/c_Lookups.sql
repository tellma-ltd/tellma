INSERT INTO dbo.Lookups
([DefinitionId],		[Name],			[SortKey]) VALUES
(N'body-colors',		N'Black',		1),
(N'body-colors',		N'White',		2),
(N'body-colors',		N'Silver',		3),
(N'body-colors',		N'Navy Blue',	4);

INSERT INTO dbo.Lookups
([DefinitionId],		[Name],		[SortKey]) VALUES
(N'vehicle-makes',		N'Toyota',	1),
(N'vehicle-makes',		N'Mercedes',2),
(N'vehicle-makes',		N'Honda',	3),
(N'vehicle-makes',		N'BMW',		4);

INSERT INTO dbo.Lookups
([DefinitionId],		[Name],	[SortKey]) VALUES
(N'steel-thicknesses',	N'0.3',	1),
(N'steel-thicknesses',	N'0.4',	2),
(N'steel-thicknesses',	N'0.7',	3),
(N'steel-thicknesses',	N'1.2',	4);

INSERT INTO dbo.Lookups
([DefinitionId],				[Name],		[SortKey]) VALUES
(N'it-equipment-manufacturers',	N'Dell',	1),
(N'it-equipment-manufacturers',	N'HP',		2),
(N'it-equipment-manufacturers',	N'Apple',	3),
(N'it-equipment-manufacturers',	N'Microsoft',4);

INSERT INTO dbo.Lookups
([DefinitionId],		[Name],					[SortKey]) VALUES
(N'operating-systems',	N'Windows XP',			1),
(N'operating-systems',	N'Windows 10',			2),
(N'operating-systems',	N'Windows Server 2017',	3),
(N'operating-systems',	N'iOS 13',				4);
;

IF @DebugLookups = 1
	SELECT * FROM map.Lookups();