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
	[Id],									[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, plants and equipment',	N'Property, plant and equipment'),
	(1,N'investment-properties',			N'Investment properties',				N'Investment property'),
	(2,N'intangible-assets',				N'Intangible assets',					N'Intangible asset'),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'inventories',						N'Inventory items',						N'Inventory Item'),
	(5,N'services-expenses',				N'Services expenses',					N'Service expense');
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitlePlural2],						[TitleSingular],					[TitleSingular2]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, plants and equipment',	N'الممتلكات والمنشآت والمعدات',	N'Property, plant and equipment',	N'الممتلكة -المنشأة-المعدة'),
	--(1,N'investment-properties',			N'Investment properties',				N'',								N'Investment property',				N''),
	--(2,N'intangible-assets',				N'Intangible assets',					N'',								N'Intangible asset',				N''),
--	(3,N'biological-assets',				N'Biological assets',					N'Biological asset'),
	(4,N'paper-products',					N'Paper Products',						N'منتجات الورق',					N'Paper Product',					N'منتج ورق'),
	(5,N'services-expenses',				N'Services',							N'الخدمات',							N'Service',							N'الخدمة');

UPDATE @ResourceDefinitions
SET
	[CountUnitVisibility] = N'Required',
	[MassUnitVisibility] = N'Required',
	[DescriptionVisibility] = N'Optional',
	[Lookup1Label]	= N'Type',
	[Lookup1Label2]	= N'النوع'	,
	[Lookup1Visibility] = N'Required',
	[Lookup1DefinitionId] = N'paper-types',
	[Lookup2Label]	= N'Size',
	[Lookup2Label2]	= N'المقاس'	,
	[Lookup2Visibility] = N'Required',
	[Lookup2DefinitionId] = N'paper-sizes',
	--[Lookup3Label]	= N'Weights',
	--[Lookup3Label2]	= N'الوزن'	,
	--[Lookup3Visibility] = N'Required',
	--[Lookup3DefinitionId] = N'paper-weights',
	[DueDateLabel] = N'Expiry Date',
	[DueDateLabel2]	= N'انتهاء الصلاحية',
	[DueDateVisibility] = N'Optional'
WHERE [Id] = N'paper-products'
END

EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions Standard: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;		