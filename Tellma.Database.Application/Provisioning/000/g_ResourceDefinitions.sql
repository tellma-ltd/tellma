INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'LandMember', N'Land', N'Lands', N'sign', N'FixedAssets',10),
(1, N'BuildingsMember', N'Building', N'Buildings', N'building', N'FixedAssets',20),
(2, N'MachineryMember', N'Machinery', N'Machineries', N'cogs', N'FixedAssets',30),
(3, N'MotorVehiclesMember', N'Motor vehicle', N'Motor vehicles', N'car', N'FixedAssets',40),
(4, N'FixturesAndFittingsMember', N'Fixture and fitting', N'Fixtures and fittings', N'', N'FixedAssets',50),
(5, N'OfficeEquipmentMember', N'Office equipment', N'Office equipment', N'fax', N'FixedAssets',60),
(6, N'ComputerEquipmentMember', N'Computer equipment', N'Computer equipment', N'laptop', N'FixedAssets',70),
(7, N'CommunicationAndNetworkEquipmentMember', N'Comm. network equipment', N'Comm. network equipment', N'network-wired', N'FixedAssets',80),
(8, N'NetworkInfrastructureMember', N'Network infrastructure', N'Network infrastructure', N'project-diagram', N'FixedAssets',90),
(9, N'BearerPlantsMember', N'Bearer plant', N'Bearer plants', N'holly-berry', N'FixedAssets',100),
(10, N'TangibleExplorationAndEvaluationAssetsMember', N'Exploration asset', N'Exploration assets', N'', N'FixedAssets',110),
(11, N'MiningAssetsMember', N'Mining asset', N'Mining assets', N'', N'FixedAssets',120),
(12, N'OilAndGasAssetsMember', N'Oil and gas assets', N'Oil and gas assets', N'', N'FixedAssets',130),
(13, N'PowerGeneratingAssetsMember', N'Power generating assets', N'Power generating assets', N'bolt', N'FixedAssets',140),
(14, N'LeaseholdImprovementsMember', N'Leasehold improvement', N'Leasehold improvements', N'', N'FixedAssets',150),
(15, N'ConstructionInProgressMember', N'Construction In progress', N'Construction in progress', N'', N'FixedAssets',160),
(16, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'Owner occupied property', N'Owner pccupied property', N'', N'FixedAssets',170),
(17, N'OtherPropertyPlantAndEquipmentMember', N'Other property, plant and equipment', N'Other property, plant and equipment', N'', N'FixedAssets',180),
(18, N'InvestmentPropertyCompletedMember', N'Investment property', N'Investment properties', N'city', N'FixedAssets',190),
(19, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'Investment property (Under Construction)', N'Investment properties (under construction)', N'', N'FixedAssets',200),
(20, N'Merchandise', N'Merchnandise', N'Merchnandise', N'', N'Purchasing',210),
(21, N'CurrentFoodAndBeverage', N'Food and beverage', N'', N'utensils', N'Purchasing',220),
(22, N'CurrentAgriculturalProduce', N'Agricultural produce', N'', N'', N'Production',230),
(23, N'FinishedGoods', N'Finished Good', N'Finished Goods', N'', N'Production',240),
(24, N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property for sale', N'Properties for sale', N'', N'Sales',250),
(25, N'WorkInProgress', N'Work in Progress', N'Works In Progress', N'spinner', N'Production',260),
(26, N'RawMaterials', N'Raw material', N'Raw materials', N'', N'Purchasing',270),
(27, N'ProductionSupplies', N'Production supply', N'Production supplies', N'', N'Purchasing',280),
(28, N'SpareParts', N'Spare part', N'Spare parts', N'', N'Purchasing',290),
(29, N'CurrentFuel', N'Fuel', N'Fuel', N'gas-pump', N'Purchasing',300),
(30, N'OtherInventories', N'Other inventory', N'Other inventories', N'', N'Purchasing',310),
(31, N'TradeMedicine', N'Medicine', N'Medicines', N'pills', N'Purchasing',90),
(32, N'TradeConstructionMaterial', N'Construction Material', N'Construction Materials', N'building', N'Purchasing',100),
(33, N'TradeSparePart', N'Spare Part (Sale)', N'Spare Parts (Sale)', N'recycle', N'Purchasing',110),
(34, N'FinishedGrain', N'Cleaned Grain', N'Cleaned Grains', N'boxes', N'Production',20),
(35, N'ByproductGrain', N'Reject Grain', N'Reject Grains', N'recycle', N'Production',30),
(36, N'FinishedVehicle', N'Assembled Vehicle', N'Assembled Vehicles', N'car-side', N'Production',50),
(37, N'FinishedOil', N'Processed Oil (Milling)', N'Processed Oils (Milling)', N'tint', N'Production',60),
(38, N'ByproductOil', N'Oil Byproduct', N'Oils Byproducts', N'tint-slash', N'Production',70),
(39, N'RawGrain', N'Raw Grain', N'Raw Grains', N'tractor', N'Purchasing',10),
(40, N'RawVehicle', N'Vehicle Component', N'Vehicles Components', N'cogs', N'Purchasing',40),
(41, N'RevenueService', N'Revenue Service', N'Revenue Services', N'hands-helping', N'Sales',10),
(42, N'EmployeeBenefit', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital',20),
(43, N'CheckReceived', N'Check Received', N'Checks Received', N'money-check', N'Financials',30);


	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility]				= N'Optional',
		[IdentifierLabel]					= N'Tag #',
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'OfficeEquipmentMember', N'ComputerEquipmentMember', N'CommunicationAndNetworkEquipmentMember', N'MachineryMember', 
					N'PowerGeneratingAssetsMember', N'OtherPropertyPlantAndEquipmentMember');

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility]					= N'Optional',
		[Lookup1Label]						= N'Manufacturer',
		[Lookup1DefinitionId]				= @ITEquipmentManufacturerLKD,
		[Lookup2Visibility]					= N'Optional',
		[Lookup2Label]						= N'Operating System',
		[Lookup2DefinitionId]				= @OperatingSystemLKD
	WHERE [Code] = N'ComputerEquipmentMember';

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'RevenueService');

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
	SET [CenterVisibility] = N'Required'
	WHERE [Code] IN (
		N'LandMember', N'BuildingsMember', N'MachineryMember', N'MotorVehiclesMember', N'FixturesAndFittingsMember', N'OfficeEquipmentMember',
		N'OfficeEquipmentMember',  N'CommunicationAndNetworkEquipmentMember', N'NetworkInfrastructureMember', N'BearerPlantsMember', 
		N'TangibleExplorationAndEvaluationAssetsMember', N'MiningAssetsMember', N'OilAndGasAssetsMember',  N'PowerGeneratingAssetsMember',
		N'LeaseholdImprovementsMember', N'ConstructionInProgressMember', N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember',
		N'OtherPropertyPlantAndEquipmentMember',
		N'InvestmentPropertyCompletedMember', N'InvestmentPropertyUnderConstructionOrDevelopmentMember'
	);

	UPDATE @ResourceDefinitions
	SET [LocationVisibility] = N'Optional'
	WHERE [Code] IN (
		N'LandMember', N'BuildingsMember', N'LeaseholdImprovementsMember', N'ConstructionInProgressMember',
		N'InvestmentPropertyCompletedMember', N'InvestmentPropertyUnderConstructionOrDevelopmentMember',
		'PropertyIntendedForSaleInOrdinaryCourseOfBusiness'
	);

	UPDATE @ResourceDefinitions
	SET [IdentifierVisibility] = N'Required'
	WHERE [Code] IN (
		--N'LandMember', N'BuildingsMember', N'LeaseholdImprovementsMember', N'ConstructionInProgressMember',
		--N'InvestmentPropertyCompletedMember', N'InvestmentPropertyUnderConstructionOrDevelopmentMember',
		'PropertyIntendedForSaleInOrdinaryCourseOfBusiness'
	);

	UPDATE @ResourceDefinitions
		SET [Decimal1Visibility] = N'Optional', [Decimal1Label] = N'Area (m^2)'
		WHERE [Code] IN (
			N'LandMember', N'BuildingsMember', N'LeaseholdImprovementsMember', N'ConstructionInProgressMember',
			N'InvestmentPropertyCompletedMember', N'InvestmentPropertyUnderConstructionOrDevelopmentMember'
		);

	UPDATE @ResourceDefinitions
	SET [UnitCardinality] = N'Single' 
	WHERE [Code] IN (
		N'LandMember',
		N'BuildingsMember', N'MachineryMember', N'MotorVehiclesMember', N'FixturesAndFittingsMember', N'OfficeEquipmentMember',
		N'OfficeEquipmentMember',  N'CommunicationAndNetworkEquipmentMember', N'NetworkInfrastructureMember', N'BearerPlantsMember', 
		N'TangibleExplorationAndEvaluationAssetsMember', N'MiningAssetsMember', N'OilAndGasAssetsMember',  N'PowerGeneratingAssetsMember',
		N'LeaseholdImprovementsMember', N'ConstructionInProgressMember', N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember',
		N'OtherPropertyPlantAndEquipmentMember',
		N'InvestmentPropertyCompletedMember', N'InvestmentPropertyUnderConstructionOrDevelopmentMember'
	);

	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility] = N'Required',
		[IdentifierLabel] = N'Plate #',
		[Lookup1Visibility] = N'Optional',
		[Lookup1Label] = N'Make',
		[Lookup1DefinitionId] = @VehicleMakeLKD
	WHERE [Code] IN (
		'MotorVehiclesMember'
	);

	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility] = N'Required',
		[IdentifierLabel] = N'Chassis #',
		[Lookup1Visibility] = N'Optional',
		[Lookup1Label] = N'Make',
		[Lookup1DefinitionId] = @VehicleMakeLKD,
		[Lookup2Visibility] = N'Optional',
		[Lookup2Label] = N'Body Color',
		[Lookup2DefinitionId] = @BodyColorLKD,
		[UnitCardinality] = N'None'
	WHERE [Code] IN (
		'FinishedVehicle'
	);

	UPDATE @ResourceDefinitions
	SET 
		[Lookup1Visibility] = N'Optional',
		[Lookup1Label] = N'Grain Group',
		[Lookup1DefinitionId] = @GrainClassificationLKD,
		[Lookup2Visibility] = N'Required',
		[Lookup2Label] = N'Grain Type',
		[Lookup2DefinitionId] = @GrainTypeLKD
	WHERE [Code] IN (
		'RawGrain', N'FinishedGrain', N'ByproductGrain'
	);

	UPDATE @ResourceDefinitions
	SET 
		[Lookup3Visibility] = N'Required',
		[Lookup3Label] = N'Qlaity Level',
		[Lookup3DefinitionId] = @QualityLKD
	WHERE [Code] IN (
		N'FinishedGrain'
	);

	UPDATE @ResourceDefinitions
	SET 
		[CurrencyVisibility] = N'Required',
		[FromDateVisibility] = N'Required',
		[FromDateLabel] = N'Check Date',
		[FromDateLabel2] = N'تاريخ الشيك',
		[Text1Visibility] = N'Required',
		[Text1Label] = N'Check Number',
		[UnitCardinality] = N'None',
		[MonetaryValueVisibility] = N'Required'
	WHERE [Code] IN (
		N'CheckReceived'
	);

EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--Declarations
DECLARE @LandMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'LandMember');
DECLARE @BuildingsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'BuildingsMember');
DECLARE @MachineryMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'MachineryMember');
DECLARE @MotorVehiclesMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'MotorVehiclesMember');
DECLARE @FixturesAndFittingsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FixturesAndFittingsMember');
DECLARE @OfficeEquipmentMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'OfficeEquipmentMember');
DECLARE @ComputerEquipmentMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ComputerEquipmentMember');
DECLARE @CommunicationAndNetworkEquipmentMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CommunicationAndNetworkEquipmentMember');
DECLARE @NetworkInfrastructureMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'NetworkInfrastructureMember');
DECLARE @BearerPlantsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'BearerPlantsMember');
DECLARE @TangibleExplorationAndEvaluationAssetsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TangibleExplorationAndEvaluationAssetsMember');
DECLARE @MiningAssetsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'MiningAssetsMember');
DECLARE @OilAndGasAssetsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'OilAndGasAssetsMember');
DECLARE @PowerGeneratingAssetsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'PowerGeneratingAssetsMember');
DECLARE @LeaseholdImprovementsMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'LeaseholdImprovementsMember');
DECLARE @ConstructionInProgressMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ConstructionInProgressMember');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember');
DECLARE @OtherPropertyPlantAndEquipmentMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'OtherPropertyPlantAndEquipmentMember');
DECLARE @InvestmentPropertyCompletedMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'InvestmentPropertyCompletedMember');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopmentMemberRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'InvestmentPropertyUnderConstructionOrDevelopmentMember');
DECLARE @MerchandiseRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'Merchandise');
DECLARE @CurrentFoodAndBeverageRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CurrentFoodAndBeverage');
DECLARE @CurrentAgriculturalProduceRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CurrentAgriculturalProduce');
DECLARE @FinishedGoodsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedGoods');
DECLARE @PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness');
DECLARE @WorkInProgressRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'WorkInProgress');
DECLARE @RawMaterialsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawMaterials');
DECLARE @ProductionSuppliesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ProductionSupplies');
DECLARE @SparePartsRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'SpareParts');
DECLARE @CurrentFuelRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CurrentFuel');
DECLARE @OtherInventoriesRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'OtherInventories');
DECLARE @TradeMedicineRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TradeMedicine');
DECLARE @TradeConstructionMaterialRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TradeConstructionMaterial');
DECLARE @TradeSparePartRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TradeSparePart');
DECLARE @FinishedGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedGrain');
DECLARE @ByproductGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ByproductGrain');
DECLARE @FinishedVehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedVehicle');
DECLARE @FinishedOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedOil');
DECLARE @ByproductOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ByproductOil');
DECLARE @RawGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawGrain');
DECLARE @RawVehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawVehicle');
DECLARE @RevenueServiceRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RevenueService');
DECLARE @EmployeeBenefitRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'EmployeeBenefit');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CheckReceived');