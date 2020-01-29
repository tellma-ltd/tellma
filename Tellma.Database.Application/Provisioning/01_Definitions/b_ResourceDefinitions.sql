DECLARE @ResourceDefinitions dbo.ResourceDefinitionList;

IF @DB = N'100' -- Banan SD, USD, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],								[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],								[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],								[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],								[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],								[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitlePlural2], [TitleSingular], [TitleSingular2]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, plants and equipment',	N'المنشآت والمعدات',	N'Property, plant and equipment', N'المنشأة-الماكينة'),
	--(1,N'investment-properties',			N'Investment properties',				N'',				N'Investment property',N''),
	--(2,N'intangible-assets',				N'Intangible assets',					N'',				N'Intangible asset', N''),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'products',							N'Products',							N'المنتجات',	N'Product',	N'المنتج'),
	(5,N'services-expenses',				N'Services',							N'الخدمات',		N'Service',		N'الخدمة');
END

EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions Standard: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;		