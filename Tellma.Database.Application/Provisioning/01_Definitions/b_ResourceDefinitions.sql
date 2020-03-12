DECLARE @ResourceDefinitions dbo.ResourceDefinitionList;


IF @DB = N'100'
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular],				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment',dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(1,N'investment-properties',			N'Investment Properties',				N'Investment Property',			dbo.fn_ATCode__Id(N'InvestmentProperty')),
	(2,N'intangible-assets',				N'Intangible Assets',					N'Intangible Asset',			dbo.fn_ATCode__Id(N'IntangibleAssetsOtherThanGoodwill')),
	(3,N'biological-assets',				N'Biological Assets',					N'Biological Asset',			dbo.fn_ATCode__Id(N'BiologicalAssets')),
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item',				dbo.fn_ATCode__Id(N'InventoriesTotal')),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense',				dbo.fn_ATCode__Id(N'ServicesExpense'));
END
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular],				[ParentAccountTypeId],										[MainMenuIcon],		[MainMenuSection], [MainMenuSortKey]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment',dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment'),			N'building',		N'FixedAssets',		100),
	(1,N'computer-equipment',				N'Computer Equipment',					N'Computer Equipment',			dbo.fn_ATCode__Id(N'ComputerEquipmentMemberExtension'),		N'laptop',			N'FixedAssets',		200),
	(2,N'intangible-assets',				N'Intangible Assets',					N'Intangible Asset',			dbo.fn_ATCode__Id(N'IntangibleAssetsOtherThanGoodwill'),	N'cube',			N'FixedAssets',		300),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense',				dbo.fn_ATCode__Id(N'ServicesExpense'),						N'hand-holding-usd', N'Purchasing',		400),
	(6,N'employee-benefits-expenses',		N'Employee Benefits Expenses',			N'Employee Benefits Expense',	dbo.fn_ATCode__Id(N'EmployeeBenefitsExpense'),				N'hand-holding-usd', N'HumanCapital',	500);

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility]		= N'Optional',
		[Lookup1Label]			= N'Manufacturer',
		[Lookup1DefinitionId]	= N'it-equipment-manufacturers',
		[Lookup2Visibility]		= N'Optional',
		[Lookup2Label]			= N'Operating System',
		[Lookup2DefinitionId]	= N'operating-systems'
	WHERE [Id] = N'computer-equipment';
END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular],				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment',dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(1,N'computer-equipment',				N'Computer Equipment',					N'Computer Equipment',			dbo.fn_ATCode__Id(N'ComputerEquipmentMemberExtension')),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense',				dbo.fn_ATCode__Id(N'ServicesExpense'));
END
ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular],				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment',dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item',				dbo.fn_ATCode__Id(N'InventoriesTotal')),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense',				dbo.fn_ATCode__Id(N'ServicesExpense'));
END
ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitleSingular],				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'Property, Plant and Equipment',dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(1,N'investment-properties',			N'Investment Properties',				N'Investment Property',			dbo.fn_ATCode__Id(N'InvestmentProperty')),
	(4,N'inventories',						N'Inventory Items',						N'Inventory Item',				dbo.fn_ATCode__Id(N'InventoriesTotal')),
	(5,N'services-expenses',				N'Services Expenses',					N'Service Expense',				dbo.fn_ATCode__Id(N'ServicesExpense'));
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @ResourceDefinitions([Index],
	[Id],									[TitlePlural],							[TitlePlural2],						[TitleSingular],					[TitleSingular2],				[ParentAccountTypeId]) VALUES
	(0,N'properties-plants-and-equipment',	N'Properties, Plants and Equipment',	N'ممتلكات ومنشآت ومعدات',		N'Property, Plant and Equipment',	N'ممتلكة -منشأة-معدة',	dbo.fn_ATCode__Id(N'PropertyPlantAndEquipment')),
	(4,N'paper-products',					N'Paper Products',						N'منتجات ورق',					N'Paper Product',					N'منتج ورق',					dbo.fn_ATCode__Id(N'Merchandise')),
	(5,N'services-expenses',				N'Services',							N'خدمات',						N'Service',							N'خدمة',						dbo.fn_ATCode__Id(N'ServicesExpense'));

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