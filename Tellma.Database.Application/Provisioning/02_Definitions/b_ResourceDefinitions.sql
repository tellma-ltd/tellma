DECLARE @ResourceDefinitions dbo.ResourceDefinitionList;

IF @DB = N'100'
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Code],									[TitlePlural],							[TitleSingular]) VALUES--,				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment'),--,@PropertyPlantAndEquipment),
	(1,N'investment-properties',			N'Investment Properties',				N'Investment Property'),--,			dbo.fn_ATCode__Id(N'InvestmentProperty')),
	(2,N'intangible-assets',				N'Intangible Assets',					N'Intangible Asset'),--,			@IntangibleAssetsOtherThanGoodwill),
	(3,N'biological-assets',				N'Biological Assets',					N'Biological Asset'),--,			dbo.fn_ATCode__Id(N'BiologicalAssets')),
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item'),--,				@Inventories),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense');--;,				@ServicesExpense);
END
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Code],									[TitlePlural],							[TitleSingular],				[TitlePlural2],	[TitleSingular2],	[MainMenuIcon],		[MainMenuSection], [MainMenuSortKey]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant or Equipment',NULL,NULL,							N'building',		N'FixedAssets',		100),--@PropertyPlantAndEquipment
	(1,N'computer-equipment',				N'Computer Equipment',					N'Computer Equipment',			N'أجهزة كمبيوتر',N'جهاز كمبيوتر',	N'laptop',			N'FixedAssets',		200),--@ComputerEquipmentMemberExtension
	(2,N'intangible-assets',				N'Intangible Assets',					N'Intangible Asset',			NULL,NULL,							N'cube',			N'FixedAssets',		300),--@IntangibleAssetsOtherThanGoodwill
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item',				NULL,NULL,							N'home',			N'Purchasing',		300),--@Inventories
	(5,N'revenue-services',					N'Revenue Services',					N'Revenue Service',				NULL,NULL,							N'hand-holding-usd', N'Purchasing',		400),--@ServicesExpense
	(6,N'employee-benefits-expenses',		N'Employee Benefits Expenses',			N'Employee Benefits Expense',	NULL,NULL,							N'hand-holding-usd', N'HumanCapital',	500);--@EmployeeBenefitsExpense
	
	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility]				= N'Optional',
		[IdentifierLabel]					= N'Tag #',
		[IdentifierLabel2]					= N'رقم التعريف',
		[CurrencyVisibility]				= N'Required',
		[DescriptionVisibility]				= N'Optional',
		[AssetTypeVisibility]				= N'Required',
		[ExpenseTypeVisibility]				= N'Required',
		[ExpenseEntryTypeVisibility]		= N'Required',
		[CenterVisibility]					= N'Required',
		[ResidualMonetaryValueVisibility]	= N'Required',
		[ResidualValueVisibility]			= N'Required'
	WHERE [Code] IN (N'computer-equipment', N'properties-plants-and-equipment');

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional',
		[RevenueTypeVisibility]				= N'Required'
	WHERE [Code] IN (N'revenue-services');

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility]					= N'Optional',
		[Lookup1Label]						= N'Manufacturer',
		[Lookup1DefinitionId]				= @it_equipment_manufacturersDef,
		[Lookup2Visibility]					= N'Optional',
		[Lookup2Label]						= N'Operating System',
		[Lookup2DefinitionId]				= @operating_systemsDef
	WHERE [Code] = N'computer-equipment';
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Code],									[TitlePlural],							[TitleSingular]) VALUES--,				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment'),--,dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(1,N'computer-equipment',				N'Computer Equipment',					N'Computer Equipment'),--,			dbo.fn_ATCode__Id(N'ComputerEquipmentMemberExtension')),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense');--,				@ServicesExpense);
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Code],									[TitlePlural],							[TitleSingular]) VALUES--,				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment'),--,dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item'),--,				@Inventories),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense');--,				@ServicesExpense);
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Code],									[TitlePlural],							[TitleSingular]) VALUES--,				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment'),--,dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(1,N'investment-properties',			N'Investment Properties',				N'Investment Property'),--,			dbo.fn_ATCode__Id(N'InvestmentProperty')),
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item'),--,				@Inventories),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense');--,				@ServicesExpense);
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Code],									[TitlePlural],							[TitlePlural2],						[TitleSingular],					[TitleSingular2]) VALUES--,		[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'ممتلكات ومنشآت ومعدات',		N'Property, Plant and Equipment',	N'ممتلكة -منشأة-معدة'),--,		@PropertyPlantAndEquipment),
	(4,N'paper-products',					N'Paper Products',						N'منتجات ورق',					N'Paper Product',					N'منتج ورق'),--,				dbo.fn_ATCode__Id(N'Merchandise')),
	(5,N'services-expenses',				N'Services',							N'خدمات',						N'Service',							N'خدمة');--,							@ServicesExpense);
		UPDATE @ResourceDefinitions
	SET
		[DescriptionVisibility] = N'Optional',
		[ReorderLevelVisibility] = N'Optional',
		[EconomicOrderQuantityVisibility] = N'Optional',
		
		[Decimal1Label] = N'Property 1',			
		[Decimal1Label2] = N'صفة 1',
		[Decimal1Visibility]= N'Optional',

		[Decimal2Label]	= N'Property 2',
		[Decimal2Label2] = N'صفة 2',
		[Decimal2Visibility] = N'Optional',

		[Int1Label] = N'Grammage',
		[Int1Label2] = N'غراماج',
		[Int1Visibility] = N'Optional',

		[Int2Label]	= N'Delivery Period',	
		[Int2Label2] = N'مدة التسليم',
		[Int2Visibility] = N'Optional',

		[Lookup1Label]	= N'Origin',
		[Lookup1Label2]	= N'التصنيع'	,
		[Lookup1Visibility] = N'Required',
		[Lookup1DefinitionId] = N'paper-origins',

		[Lookup2Label]	= N'Group',
		[Lookup2Label2]	= N'المجموعة'	,
		[Lookup2Visibility] = N'Required',
		[Lookup2DefinitionId] = N'paper-groups',

		[Lookup3Label]	= N'Type',
		[Lookup3Label2]	= N'النوع'	,
		[Lookup3Visibility] = N'Required',
		[Lookup3DefinitionId] = N'paper-types',

		[Text1Label] = N'Color',				
		[Text1Label2] = N'اللون',
		[Text1Visibility] = N'Optional',

		[Text2Label]	= N'Size',
		[Text2Label2] = N'المقاس',
		[Text2Visibility] = N'Optional'

	WHERE [Code] = N'paper-products'
END

EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DECLARE @properties_plants_and_equipmentDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'properties-plants-and-equipment');
DECLARE @computer_equipmentDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'computer-equipment');
DECLARE @intangible_assetsDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'intangible-assets');
DECLARE @inventoriesDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'inventories');
DECLARE @revenue_servicesDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'revenue-services');
DECLARE @employee_benefits_expensesDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'employee-benefits-expenses');

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions Standard: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;		