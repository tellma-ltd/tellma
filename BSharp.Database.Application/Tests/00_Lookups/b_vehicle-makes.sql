INSERT INTO dbo.LookupDefinitions([Id]) VALUES(N'vehicle-makes');
INSERT INTO dbo.Lookups
([LookupDefinitionId], [Name]) VALUES
(N'vehicle-makes',		N'Toyota'),
(N'vehicle-makes',		N'Mercedes'),
(N'vehicle-makes',		N'Honda'),
(N'vehicle-makes',		N'BMW');
