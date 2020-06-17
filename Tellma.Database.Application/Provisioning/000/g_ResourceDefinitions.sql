INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'PropertyPlantAndEquipment', N'Property, Plant, Equipment', N'Property, plant, and equipment', N'industry', N'FixedAssets',10),
(1, N'OfficeEquipment', N'Office Equipment', N'Office Equipment', N'fax', N'FixedAssets',14),
(2, N'ComputerEquipment', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets',18),
(3, N'Machinery', N'Machinery', N'Machineries', N'cogs', N'FixedAssets',22),
(4, N'Vehicle', N'Vehicle', N'Vehicles', N'car', N'FixedAssets',26),
(5, N'Building', N'Building', N'Buildings', N'building', N'FixedAssets',30),
(6, N'InvestmentProperty', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets',34),
(7, N'RawGrain', N'Raw Grain', N'Raw Grains', N'tractor', N'Purchasing',38),
(8, N'FinishedGrain', N'Cleaned Grain', N'Cleaned Grains', N'boxes', N'Production',42),
(9, N'ByproductGrain', N'Reject Grain', N'Reject Grains', N'recycle', N'Production',46),
(10, N'RawVehicle', N'Vehicle Component', N'Vehicles Components', N'cogs', N'Purchasing',50),
(11, N'FinishedVehicle', N'Assembled Vehicle', N'Assembled Vehicles', N'car-side', N'Production',54),
(12, N'RawOil', N'Raw Material (Oil Milling)', N'Raw Materials (Oil Milling)', N'file-export', N'Purchasing',58),
(13, N'FinishedOil', N'Processed Oil (Milling)', N'Processed Oils (Milling)', N'tint', N'Production',62),
(14, N'ByproductOil', N'Oil Byproduct', N'Oils Byproducts', N'tint-slash', N'Production',66),
(15, N'WorkInProgress', N'Work in Progress', N'Works In Progress', N'spinner', N'Production',70),
(16, N'Medicine', N'Medicine', N'Medicines', N'pills', N'Purchasing',74),
(17, N'ConstructionMaterial', N'Construction Material', N'Construction Materials', N'building', N'Purchasing',78),
(18, N'EmployeeBenefit', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital',82),
(19, N'FinishedService', N'Revenue Service', N'Revenue Services', N'hands-helping', N'Sales',86),
(20, N'CheckReceived', N'Check Received', N'Checks Received', N'money-check', N'Financials',90);


	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility]				= N'Optional',
		[IdentifierLabel]					= N'Tag #',
		[CurrencyVisibility]				= N'Required',
		[DescriptionVisibility]				= N'Optional',
		[CenterVisibility]					= N'Required',
		[ResidualMonetaryValueVisibility]	= N'Required',
		[ResidualValueVisibility]			= N'Required'
	WHERE [Code] IN (N'ComputerEquipment', N'PropertyPlantAndEquipment');

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility]					= N'Optional',
		[Lookup1Label]						= N'Manufacturer',
		[Lookup1DefinitionId]				= @ITEquipmentManufacturerLKD,
		[Lookup2Visibility]					= N'Optional',
		[Lookup2Label]						= N'Operating System',
		[Lookup2DefinitionId]				= @OperatingSystemLKD
	WHERE [Code] = N'ComputerEquipment';

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'FinishedService');

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

	WHERE [Code] = N'PaperProducts'

	UPDATE @ResourceDefinitions
	SET [LocationVisibility] = N'Optional'
	WHERE [Code] = N'InvestmentProperty'

EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--Declarations
DECLARE @PropertyPlantAndEquipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'PropertyPlantAndEquipment');
DECLARE @OfficeEquipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'OfficeEquipment');
DECLARE @ComputerEquipmentRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ComputerEquipment');
DECLARE @MachineryRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'Machinery');
DECLARE @VehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'Vehicle');
DECLARE @BuildingRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'Building');
DECLARE @InvestmentPropertyRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'InvestmentProperty');
DECLARE @RawGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawGrain');
DECLARE @FinishedGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedGrain');
DECLARE @ByproductGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ByproductGrain');
DECLARE @RawVehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawVehicle');
DECLARE @FinishedVehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedVehicle');
DECLARE @RawOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawOil');
DECLARE @FinishedOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedOil');
DECLARE @ByproductOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ByproductOil');
DECLARE @WorkInProgressRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'WorkInProgress');
DECLARE @MedicineRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'Medicine');
DECLARE @ConstructionMaterialRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ConstructionMaterial');
DECLARE @EmployeeBenefitRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'EmployeeBenefit');
DECLARE @FinishedServiceRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedService');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CheckReceived');