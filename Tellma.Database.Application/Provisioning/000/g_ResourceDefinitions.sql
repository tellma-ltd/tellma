INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'LandMember', N'Land', N'Lands', N'', N'',10),
(1, N'BuildingsMember', N'Building', N'Buildings', N'building', N'FixedAssets',20),
(2, N'MachineryMember', N'Machinery', N'Machineries', N'cogs', N'FixedAssets',30),
(3, N'MotorVehiclesMember', N'Vehicle', N'Vehicles', N'car', N'FixedAssets',40),
(4, N'FixturesAndFittingsMember', N'Fixture and fitting', N'Fixtures and fittings', N'', N'FixedAssets',50),
(5, N'OfficeEquipmentMember', N'Office Equipment', N'Office Equipment', N'fax', N'FixedAssets',60),
(6, N'ComputerEquipmentMember', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets',70),
(7, N'CommunicationAndNetworkEquipmentMember', N'Comm. Network Equipment', N'Comm. Network Equipment', N'', N'FixedAssets',80),
(8, N'NetworkInfrastructureMember', N'Network Infrastructure', N'Network Infrastructure', N'', N'FixedAssets',90),
(9, N'BearerPlantsMember', N'Bearer Plants', N'Bearer Plants', N'', N'FixedAssets',100),
(10, N'TangibleExplorationAndEvaluationAssetsMember', N'Exploration Assets', N'Exploration Assets', N'', N'FixedAssets',110),
(11, N'MiningAssetsMember', N'Mining Assets', N'Mining Assets', N'', N'FixedAssets',120),
(12, N'OilAndGasAssetsMember', N'Oil and Gas Assets', N'Oil and Gas Assets', N'', N'FixedAssets',130),
(13, N'PowerGeneratingAssetsMember', N'Power Generating Assets', N'Power Generating Assets', N'', N'FixedAssets',140),
(14, N'LeaseholdImprovementsMember', N'Leasehold Improvements', N'Leasehold Improvements', N'', N'FixedAssets',150),
(15, N'ConstructionInProgressMember', N'Construction In progress', N'Construction In progress', N'', N'FixedAssets',160),
(16, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'Owner Occupied Property', N'Owner Occupied Property', N'', N'FixedAssets',170),
(17, N'OtherPropertyPlantAndEquipmentMember', N'Other property, plant and equipment', N'Other property, plant and equipment', N'', N'FixedAssets',180),
(18, N'InvestmentPropertyCompletedMember', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets',190),
(19, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'Investment Property (Under Construction)', N'Investment Properties (under construction)', N'', N'FixedAssets',200),
(20, N'RawGrain', N'Raw Grain', N'Raw Grains', N'tractor', N'Purchasing',10),
(21, N'FinishedGrain', N'Cleaned Grain', N'Cleaned Grains', N'boxes', N'Production',20),
(22, N'ByproductGrain', N'Reject Grain', N'Reject Grains', N'recycle', N'Production',30),
(23, N'RawVehicle', N'Vehicle Component', N'Vehicles Components', N'cogs', N'Purchasing',40),
(24, N'FinishedVehicle', N'Assembled Vehicle', N'Assembled Vehicles', N'car-side', N'Production',50),
(25, N'FinishedOil', N'Processed Oil (Milling)', N'Processed Oils (Milling)', N'tint', N'Production',60),
(26, N'ByproductOil', N'Oil Byproduct', N'Oils Byproducts', N'tint-slash', N'Production',70),
(27, N'WorkInProgress', N'Work in Progress', N'Works In Progress', N'spinner', N'Production',80),
(28, N'TradeMedicine', N'Medicine', N'Medicines', N'pills', N'Purchasing',90),
(29, N'TradeConstructionMaterial', N'Construction Material', N'Construction Materials', N'building', N'Purchasing',100),
(30, N'TradeSparePart', N'Spare Part (Sale)', N'Spare Parts (Sale)', N'recycle', N'Purchasing',110),
(31, N'RevenueService', N'Revenue Service', N'Revenue Services', N'hands-helping', N'Sales',10),
(32, N'EmployeeBenefit', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital',20),
(33, N'CheckReceived', N'Check Received', N'Checks Received', N'money-check', N'Financials',30);


	UPDATE @ResourceDefinitions
	SET 
		[IdentifierVisibility]				= N'Optional',
		[IdentifierLabel]					= N'Tag #',
		[CurrencyVisibility]				= N'Required',
		[DescriptionVisibility]				= N'Optional',
		[CenterVisibility]					= N'Required',
		[ResidualMonetaryValueVisibility]	= N'Required',
		[ResidualValueVisibility]			= N'Required'
	WHERE [Code] IN (N'ComputerEquipmentMember', N'MachineryMember');

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
	SET [LocationVisibility] = N'Optional'
	WHERE [Code] IN (
		N'LandMember', N'BuildingsMember', N'LeaseholdImprovementsMember', N'ConstructionInProgressMember',
		N'InvestmentPropertyCompletedMember', N'InvestmentPropertyUnderConstructionOrDevelopmentMember'
	)

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
DECLARE @RawGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawGrain');
DECLARE @FinishedGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedGrain');
DECLARE @ByproductGrainRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ByproductGrain');
DECLARE @RawVehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RawVehicle');
DECLARE @FinishedVehicleRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedVehicle');
DECLARE @FinishedOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'FinishedOil');
DECLARE @ByproductOilRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'ByproductOil');
DECLARE @WorkInProgressRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'WorkInProgress');
DECLARE @TradeMedicineRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TradeMedicine');
DECLARE @TradeConstructionMaterialRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TradeConstructionMaterial');
DECLARE @TradeSparePartRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'TradeSparePart');
DECLARE @RevenueServiceRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'RevenueService');
DECLARE @EmployeeBenefitRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'EmployeeBenefit');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.ResourceDefinitions WHERE [Code] = N'CheckReceived');