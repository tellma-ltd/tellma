IF NOT EXISTS(SELECT * FROM dbo.ResourceDefinitions)
BEGIN
	INSERT INTO dbo.ResourceDefinitions
	([Id],								[TitlePlural],							[TitleSingular]) VALUES
	(N'properties-plants-and-equipment',N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(N'investment-properties',			N'Investment properties',				N'Investment property'),
	(N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
	(N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(N'inventories',					N'Inventory items',						N'Inventory Item'),
	(N'services-expenses',				N'Services expenses',					N'Service expense');

END