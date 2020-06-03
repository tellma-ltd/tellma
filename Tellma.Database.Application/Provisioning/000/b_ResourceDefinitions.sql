INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'property-plant-equipment', N'Property, Plant, Equipment', N'Property, plant, and equipment', N'industry', N'FixedAssets',60),
(1, N'office-equipment', N'Office Equipment', N'Office Equipment', N'fax', N'FixedAssets',60),
(2, N'computer-equipment', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets',60),
(3, N'machinery', N'Machinery', N'Machineries', N'cogs', N'FixedAssets',70),
(4, N'vehicles', N'Vehicles', N'Vehicles', N'car', N'FixedAssets',40),
(5, N'buildings', N'Building', N'Buildings', N'building', N'FixedAssets',50),
(6, N'investment-properties', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets',80),
(7, N'raw-grains', N'Raw Grain', N'Raw Grains', N'tractor', N'Purchasing',90),
(8, N'finished-grains', N'Cleaned Grain', N'Cleaned Grains', N'boxes', N'Production',100),
(9, N'byproducts-grains', N'Reject Grain', N'Reject Grains', N'recycle', N'Production',60),
(10, N'raw-vehicles', N'Vehicles Component', N'Vehicles Components', N'cogs', N'Purchasing',120),
(11, N'finished-vehicles', N'Assembled Vehicle', N'Assembled Vehicles', N'car-side', N'Production',120),
(12, N'raw-oils', N'Raw Material (Oil Milling)', N'Raw Materials (Oil Milling)', N'file-export', N'Purchasing',121),
(13, N'finished-oils', N'Processed Oil (Milling)', N'Processed Oil (Milling)', N'tint', N'Production',122),
(14, N'byproducts-oils', N'Oil Byproduct', N'Oil Byproducts', N'tint-slash', N'Production',123),
(15, N'work-in-progress', N'Work in Progress', N'Work In Progress', N'spinner', N'Production',124),
(16, N'medicines', N'Medicine', N'Medicines', N'pills', N'Purchasing',125),
(17, N'construction-materials', N'Construction Material', N'Construction Materials', N'building', N'Purchasing',126),
(18, N'employee-benefits', N'Employee Benefit', N'Employee Benefits', N'user-check', N'HumanCapital',127),
(19, N'finished-services', N'Revenue Service', N'Revenue Services', N'hands-helping', N'Sales',128);


	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility]				= N'Optional',
		[IdentifierLabel]					= N'Tag #',
		[CurrencyVisibility]				= N'Required',
		[DescriptionVisibility]				= N'Optional',
		[CenterVisibility]					= N'Required',
		[ResidualMonetaryValueVisibility]	= N'Required',
		[ResidualValueVisibility]			= N'Required'
	WHERE [Code] IN (N'computer-equipment', N'properties-plants-and-equipment');

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility]					= N'Optional',
		[Lookup1Label]						= N'Manufacturer',
		-- TODO: uncomment
		--[Lookup1DefinitionId]				= @it_equipment_manufacturersLKD,
		[Lookup2Visibility]					= N'Optional',
		[Lookup2Label]						= N'Operating System'
		--[Lookup2DefinitionId]				= @operating_systemsLKD
	WHERE [Code] = N'computer-equipment';

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'finished-services');

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

EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions Standard: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--Declarations
DECLARE @property_plant_equipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'property-plant-equipment');
DECLARE @office_equipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'office-equipment');
DECLARE @computer_equipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'computer-equipment');
DECLARE @machineryRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'machinery');
DECLARE @vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'vehicles');
DECLARE @buildingsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'buildings');
DECLARE @investment_propertiesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'investment-properties');
DECLARE @raw_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-grains');
DECLARE @finished_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-grains');
DECLARE @byproducts_grainsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'byproducts-grains');
DECLARE @raw_vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-vehicles');
DECLARE @finished_vehiclesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-vehicles');
DECLARE @raw_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'raw-oils');
DECLARE @finished_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-oils');
DECLARE @byproducts_oilsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'byproducts-oils');
DECLARE @work_in_progressRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'work-in-progress');
DECLARE @medicinesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'medicines');
DECLARE @construction_materialsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'construction-materials');
DECLARE @employee_benefitsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'employee-benefits');
DECLARE @finished_servicesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'finished-services');




















