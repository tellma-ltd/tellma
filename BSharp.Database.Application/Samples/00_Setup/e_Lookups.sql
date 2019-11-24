INSERT INTO dbo.LookupDefinitions([Id]) VALUES(N'body-colors');
INSERT INTO dbo.Lookups
([LookupDefinitionId], [Name]) VALUES
(N'body-colors',		N'Black'),
(N'body-colors',		N'White'),
(N'body-colors',		N'Silver'),
(N'body-colors',		N'Navy Blue');

INSERT INTO dbo.LookupDefinitions([Id]) VALUES(N'vehicle-makes');
INSERT INTO dbo.Lookups
([LookupDefinitionId], [Name]) VALUES
(N'vehicle-makes',		N'Toyota'),
(N'vehicle-makes',		N'Mercedes'),
(N'vehicle-makes',		N'Honda'),
(N'vehicle-makes',		N'BMW');

INSERT INTO dbo.LookupDefinitions([Id]) VALUES(N'steel-thicknesses');
INSERT INTO dbo.Lookups
([LookupDefinitionId], [Name]) VALUES
(N'steel-thicknesses',	N'0.3'),
(N'steel-thicknesses',	N'0.4'),
(N'steel-thicknesses',	N'0.7'),
(N'steel-thicknesses',	N'1.2');

INSERT INTO dbo.LookupDefinitions([Id]) VALUES(N'it-equipment-manufacturers');
INSERT INTO dbo.Lookups
([LookupDefinitionId],				[Name]) VALUES
(N'it-equipment-manufacturers',		N'Dell'),
(N'it-equipment-manufacturers',		N'HP'),
(N'it-equipment-manufacturers',		N'Apple'),
(N'it-equipment-manufacturers',		N'Microsoft');

INSERT INTO dbo.LookupDefinitions([Id]) VALUES(N'operating-systems');
INSERT INTO dbo.Lookups
([LookupDefinitionId],				[Name]) VALUES
(N'operating-systems',		N'Windows 10'),
(N'operating-systems',		N'Windows Server 2017'),
(N'operating-systems',		N'iOS 13'),
(N'operating-systems',		N'Windows XP');

IF @DebugLookups = 1
	SELECT * FROM map.Lookups();