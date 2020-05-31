
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
		[CenterVisibility]					= N'Required',
		[ResidualMonetaryValueVisibility]	= N'Required',
		[ResidualValueVisibility]			= N'Required'
	WHERE [Code] IN (N'computer-equipment', N'properties-plants-and-equipment');

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'revenue-services');

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility]					= N'Optional',
		[Lookup1Label]						= N'Manufacturer',
		[Lookup1DefinitionId]				= @it_equipment_manufacturersLKD,
		[Lookup2Visibility]					= N'Optional',
		[Lookup2Label]						= N'Operating System',
		[Lookup2DefinitionId]				= @operating_systemsLKD
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

ELSE IF @DB = N'106' -- Soreti, ETB, en/am
BEGIN
	INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitleSingular2], [TitleSingular3], [TitlePlural], [TitlePlural2], [TitlePlural3], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
	(0, N'office-equipment', N'Office equipment', N'የቢሮ መሣሪያዎች', N'Office equipment', N'Office equipment', N'የቢሮ መሣሪያዎች', N'Office equipment', N'chair-office', N'Assets',60),
	(1, N'machinery', N'Machinery', N'ማሽኖች', N'Machinery', N'Machineries', N'ማሽኖች', N'Machineries', N'cogs', N'Assets',70),
	(2, N'vehicles', N'Vehicles', N'ተሽከርካሪዎች', N'Vehicles', N'Vehicles', N'ተሽከርካሪዎች', N'Vehicles', N'cars', N'Assets',40),
	(3, N'buildings', N'Building', N'ህንፃዎች', N'Building', N'Buildings', N'ህንፃዎች', N'Buildings', N'building', N'Assets',50),
	(4, N'investment-properties', N'Investment property', N'የኢንmentስትሜንት ንብረቶች', N'Investment property', N'Investment properties', N'የኢንmentስትሜንት ንብረቶች', N'Investment properties', N'city', N'Assets',80),
	(5, N'raw-grains', N'Raw grain', N'ጥሬ እህሎች', N'Raw grain', N'Raw grains', N'ጥሬ እህሎች', N'Raw grains', N'wheat', N'Purchasing',90),
	(6, N'finished-grains', N'Cleaned grain', N'የተጣራ እህል', N'Cleaned grain', N'Cleaned grains', N'የተጣራ እህል', N'Cleaned grains', N'wheat', N'Production',100),
	(7, N'byproducts-grains', N'Reject grain', N'እህልን ይከልክሉ', N'Reject grain', N'Reject grains', N'እህልን ይከልክሉ', N'Reject grains', N'wheat', N'Production',60),
	(8, N'raw-vehicles', N'Vehicles component', N'የተሽከርካሪዎች ክፍሎች', N'Vehicles component', N'Vehicles components', N'የተሽከርካሪዎች ክፍሎች', N'Vehicles components', N'tire', N'Purchasing',120),
	(9, N'finished-vehicles', N'Assembled vehicle', N'የተሰበሰቡ ተሽከርካሪዎች', N'Assembled vehicle', N'Assembled vehicles', N'የተሰበሰቡ ተሽከርካሪዎች', N'Assembled vehicles', N'car-side', N'Production',120),
	(10, N'raw-oils', N'Raw materials (Oil Milling)', N'ጥሬ እቃዎች (ዘይት ቁፋሮ)', N'Raw materials (Oil Milling)', N'Raw materials (Oil Milling)', N'ጥሬ እቃዎች (ዘይት ቁፋሮ)', N'Raw materials (Oil Milling)', N'file-export', N'Purchasing',121),
	(11, N'finished-oils', N'Processed Oil (Milling)', N'የተቀቀለ ዘይት (ወፍጮ)', N'Processed Oil (Milling)', N'Processed Oil (Milling)', N'የተቀቀለ ዘይት (ወፍጮ)', N'Processed Oil (Milling)', N'tint', N'Production',122),
	(12, N'byproducts-oils', N'Oil byproduct', N'የዘይት ፍሬ', N'Oil byproduct', N'Oil byproducts', N'ዘይት ያመርታል', N'Oil byproducts', N'tint-slash', N'Production',123),
	(13, N'work-in-progress', N'Work in progress', N'ገና በሂደት ላይ ያለ ስራ', N'Work in progress', N'Work in progress', N'ገና በሂደት ላይ ያለ ስራ', N'Work in progress', N'spinner', N'Production',124),
	(14, N'medicines', N'Medicine', N'መድሃኒት', N'Medicine', N'Medicines', N'መድሃኒቶች', N'Medicines', N'pills', N'Purchasing',125),
	(15, N'construction-materials', N'Construction material', N'የግንባታ ቁሳቁሶች', N'Construction material', N'Construction materials', N'የግንባታ ቁሳቁሶች', N'Construction materials', N'building', N'Purchasing',126);
END
EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

DECLARE @properties_plants_and_equipmentDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'properties-plants-and-equipment');
DECLARE @computer_equipmentDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'computer-equipment');
DECLARE @intangible_assetsDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'intangible-assets');
DECLARE @revenue_servicesDef INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'revenue-services');
DECLARE @employee_benefits_expensesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'employee-benefits-expenses');
DECLARE @raw_materialsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'raw-materials');
DECLARE @finished_goodsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'finished-goods');
DECLARE @merchandiseRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'merchandise');
DECLARE @work_in_progressRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'work-in-progress');

DECLARE @106office_equipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'office-equipment');
DECLARE @106machineryRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'machinery');
DECLARE @106vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'vehicles');
DECLARE @106buildingsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'buildings');
DECLARE @106investment_propertiesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'investment-properties');
DECLARE @106raw_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-grains');
DECLARE @106finished_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-grains');
DECLARE @106byproducts_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'byproducts-grains');
DECLARE @106raw_vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-vehicles');
DECLARE @106finished_vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-vehicles');
DECLARE @106raw_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-oils');
DECLARE @106finished_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-oils');
DECLARE @106byproducts_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'byproducts-oils');
DECLARE @106work_in_progressRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'work-in-progress');
DECLARE @106medicinesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'medicines');
DECLARE @106construction_materialsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'construction-materials');


IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions Standard: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;		