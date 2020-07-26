INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey],[ParticipantId]) VALUES
(0, N'LandMember', N'Land', N'Lands', N'sign', N'FixedAssets',10,NULL),
(1, N'BuildingsMember', N'Building', N'Buildings', N'building', N'FixedAssets',20,NULL),
(2, N'MachineryMember', N'Machinery', N'Machineries', N'cogs', N'FixedAssets',30,NULL),
(3, N'MotorVehiclesMember', N'Motor Vehicle', N'Motor Vehicles', N'car', N'FixedAssets',40,NULL),
(4, N'FixturesAndFittingsMember', N'Fixture and Fitting', N'Fixtures and Fittings', N'puzzle-piece', N'FixedAssets',50,NULL),
(5, N'OfficeEquipmentMember', N'Office Equipment', N'Office Equipment', N'fax', N'FixedAssets',60,NULL),
(6, N'ComputerEquipmentMember', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets',70,NULL),
(7, N'CommunicationAndNetworkEquipmentMember', N'Comm. Network Equipment', N'Comm. Network Equipment', N'network-wired', N'FixedAssets',80,NULL),
(8, N'NetworkInfrastructureMember', N'Network Infrastructure', N'Network Infrastructure', N'project-diagram', N'FixedAssets',90,NULL),
(9, N'BearerPlantsMember', N'Bearer Plant', N'Bearer Plants', N'holly-berry', N'FixedAssets',100,NULL),
(10, N'TangibleExplorationAndEvaluationAssetsMember', N'Exploration Asset', N'Exploration Assets', N'download', N'FixedAssets',110,NULL),
(11, N'MiningAssetsMember', N'Mining Asset', N'Mining Assets', N'hammer', N'FixedAssets',120,NULL),
(12, N'OilAndGasAssetsMember', N'Oil and Gas Asset', N'Oil and Gas Assets', N'gas-pump', N'FixedAssets',130,NULL),
(13, N'PowerGeneratingAssetsMember', N'Power Generating Asset', N'Power Generating Assets', N'bolt', N'FixedAssets',140,NULL),
(14, N'LeaseholdImprovementsMember', N'Leasehold Improvement', N'Leasehold Improvements', N'paint-roller', N'FixedAssets',150,NULL),
(15, N'ConstructionInProgressMember', N'Construction In Progress', N'Construction In Progress', N'drafting-compass', N'FixedAssets',160,NULL),
(16, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'Owner Occupied Property', N'Owner Occupied Property', N'campground', N'FixedAssets',170,NULL),
(17, N'OtherPropertyPlantAndEquipmentMember', N'Other Fixed Asset', N'Other Fixed Assets', N'tags', N'FixedAssets',180,NULL),
(18, N'InvestmentPropertyCompletedMember', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets',190,NULL),
(19, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'Investment Property (under Construction)', N'Investment Properties (under Construction)', N'store-slash', N'FixedAssets',200,NULL),
(20, N'Merchandise', N'Merchandise', N'Merchandise', N'barcode', N'Purchasing',210,NULL),
(21, N'CurrentFoodAndBeverage', N'Food and Beverage', N'Food and Beverage', N'utensils', N'Purchasing',220,NULL),
(22, N'CurrentAgriculturalProduce', N'Agricultural Produce', N'Agricultural Produce', N'carrot', N'Production',230,NULL),
(23, N'FinishedGoods', N'Finished Good', N'Finished Goods', N'gifts', N'Production',240,NULL),
(24, N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property For Sale', N'Properties For Sale', N'sign', N'Sales',250,NULL),
(25, N'WorkInProgress', N'Work In Progress', N'Works In Progress', N'spinner', N'Production',260,NULL),
(26, N'RawMaterials', N'Raw Material', N'Raw Materials', N'boxes', N'Purchasing',270,NULL),
(27, N'ProductionSupplies', N'Production Supply', N'Production Supplies', N'parachute-box', N'Purchasing',280,NULL),
(28, N'CurrentPackagingAndStorageMaterials', N'Packaging and Storage Material', N'Packaging and Storage Materials', N'box', N'Production',285,NULL),
(29, N'SpareParts', N'Spare Part', N'Spare Parts', N'undo-alt', N'Purchasing',290,NULL),
(30, N'CurrentFuel', N'Fuel', N'Fuel', N'gas-pump', N'Purchasing',300,NULL),
(31, N'OtherInventories', N'Other Inventory', N'Other Inventories', N'shapes', N'Purchasing',310,NULL),
(32, N'TradeMedicine', N'Medicine', N'Medicines', N'pills', N'Purchasing',90,NULL),
(33, N'TradeConstructionMaterial', N'Construction Material', N'Construction Materials', N'building', N'Purchasing',100,NULL),
(34, N'TradeSparePart', N'Spare Part (sale)', N'Spare Parts (sale)', N'recycle', N'Purchasing',110,NULL),
(35, N'FinishedGrain', N'Cleaned Grain', N'Cleaned Grains', N'boxes', N'Production',20,NULL),
(36, N'ByproductGrain', N'Reject Grain', N'Reject Grains', N'recycle', N'Production',30,NULL),
(37, N'FinishedVehicle', N'Assembled Vehicle', N'Assembled Vehicles', N'car-side', N'Production',50,NULL),
(38, N'FinishedOil', N'Processed Oil (milling)', N'Processed Oils (milling)', N'tint', N'Production',60,NULL),
(39, N'ByproductOil', N'Oil Byproduct', N'Oils Byproducts', N'tint-slash', N'Production',70,NULL),
(40, N'RawGrain', N'Raw Grain', N'Raw Grains', N'tractor', N'Purchasing',10,NULL),
(41, N'RawVehicle', N'Vehicle Component', N'Vehicles Components', N'cogs', N'Purchasing',40,NULL),
(42, N'RevenueService', N'Revenue Service', N'Revenue Services', N'hands-helping', N'Sales',10,NULL),
(43, N'EmployeeBenefit', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital',20,NULL),
(44, N'CheckReceived', N'Check Received', N'Checks Received', N'money-check', N'Financials',30,@CustomerCD),
(45, N'Accruals', N'Accrual', N'Accruals', N'', N'Financials',40,@SupplierCD),
(46, N'AccruedIncome', N'Accrued Income', N'Accrued Incomes', N'', N'Financials',50,@CustomerCD),
(47, N'BalancesWithBanks', N'Bank Account', N'Bank Accounts', N'', N'Financials',60,@BankCD),
(48, N'Cash', N'Cash', N'Cashs', N'', N'Financials',70,@GovernmentCD),
(49, N'ConsumerLoans', N'Consumer Loan', N'Consumer Loans', N'', N'Financials',80,@EmployeeCD),
(50, N'CurrentBilledButNotIssued', N'Billed Not Issued', N'Billed Not Issued', N'', N'Financials',90,@CustomerCD),
(51, N'DeferredIncome', N'Deferred Income', N'Deferred Incomes', N'', N'Financials',100,@CustomerCD),
(52, N'Prepayments', N'Prepayment', N'Prepayments', N'', N'Financials',110,@SupplierCD),
(53, N'ReceivablesFromRentalOfProperties', N'Rental Receivable', N'Rental Receivables', N'', N'Financials',120,@CustomerCD),
(54, N'ReceivablesFromSaleOfProperties', N'Sale Receivable', N'Sale Receivables', N'', N'Financials',130,@CustomerCD),
(56, N'RefundsProvision', N'Refund Provision', N'Refund Provisions', N'', N'Financials',150,@CustomerCD),
(57, N'RentDeferredIncome', N'Rent Deferred Income', N'Rent Deferred Incomes', N'', N'Financials',160,@CustomerCD),
(58, N'RestructuringProvision', N'Restructuring Provision', N'Restructuring Provisions', N'', N'Financials',170,NULL),
(59, N'RetentionPayable', N'Retention Payable', N'Retention Payables', N'', N'Financials',180,@SupplierCD),
(60, N'TradePayable', N'Trade Payable', N'Trade Payables', N'', N'Financials',190,@SupplierCD),
(61, N'TradeReceivable', N'Trade Receivable', N'Trade Receivables', N'', N'Financials',200,@CustomerCD),
(62, N'WarrantyProvision', N'Warranty Provision', N'Warranty Provisions', N'', N'Financials',210,NULL);

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
		[UnitCardinality] = N'None' , Decimal1Visibility = N'Optional', 
		[Decimal1Label] = N'Area (m^2)', Decimal1Label2 = N'مساحة (م^2)'
	WHERE [Code] = N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness'


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
		[Lookup2Visibility] = N'Required',
		[Lookup2Label] = N'Oilseed Type',
		[Lookup2DefinitionId] = @GrainTypeLKD
	WHERE [Code] IN (
		'FinishedOil'
	);

	UPDATE @ResourceDefinitions
	SET 
		[Lookup3Visibility] = N'Required',
		[Lookup3Label] = N'Quality Level',
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
		[MonetaryValueVisibility] = N'Required',
		[Lookup4Visibility] = N'Required',
		[Lookup4Label] = N'Bank',
		[Lookup4DefinitionId] = @BankLKD
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
DECLARE @LandMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LandMember');
DECLARE @BuildingsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BuildingsMember');
DECLARE @MachineryMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MachineryMember');
DECLARE @MotorVehiclesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MotorVehiclesMember');
DECLARE @FixturesAndFittingsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FixturesAndFittingsMember');
DECLARE @OfficeEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OfficeEquipmentMember');
DECLARE @ComputerEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ComputerEquipmentMember');
DECLARE @CommunicationAndNetworkEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CommunicationAndNetworkEquipmentMember');
DECLARE @NetworkInfrastructureMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'NetworkInfrastructureMember');
DECLARE @BearerPlantsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BearerPlantsMember');
DECLARE @TangibleExplorationAndEvaluationAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TangibleExplorationAndEvaluationAssetsMember');
DECLARE @MiningAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MiningAssetsMember');
DECLARE @OilAndGasAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OilAndGasAssetsMember');
DECLARE @PowerGeneratingAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PowerGeneratingAssetsMember');
DECLARE @LeaseholdImprovementsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LeaseholdImprovementsMember');
DECLARE @ConstructionInProgressMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ConstructionInProgressMember');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember');
DECLARE @OtherPropertyPlantAndEquipmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OtherPropertyPlantAndEquipmentMember');
DECLARE @InvestmentPropertyCompletedMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'InvestmentPropertyCompletedMember');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'InvestmentPropertyUnderConstructionOrDevelopmentMember');
DECLARE @MerchandiseRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Merchandise');
DECLARE @CurrentFoodAndBeverageRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentFoodAndBeverage');
DECLARE @CurrentAgriculturalProduceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentAgriculturalProduce');
DECLARE @FinishedGoodsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedGoods');
DECLARE @PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness');
DECLARE @WorkInProgressRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'WorkInProgress');
DECLARE @RawMaterialsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RawMaterials');
DECLARE @ProductionSuppliesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ProductionSupplies');
DECLARE @CurrentPackagingAndStorageMaterialsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentPackagingAndStorageMaterials');
DECLARE @SparePartsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SpareParts');
DECLARE @CurrentFuelRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentFuel');
DECLARE @OtherInventoriesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OtherInventories');
DECLARE @TradeMedicineRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeMedicine');
DECLARE @TradeConstructionMaterialRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeConstructionMaterial');
DECLARE @TradeSparePartRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeSparePart');
DECLARE @FinishedGrainRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedGrain');
DECLARE @ByproductGrainRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ByproductGrain');
DECLARE @FinishedVehicleRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedVehicle');
DECLARE @FinishedOilRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinishedOil');
DECLARE @ByproductOilRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ByproductOil');
DECLARE @RawGrainRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RawGrain');
DECLARE @RawVehicleRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RawVehicle');
DECLARE @RevenueServiceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RevenueService');
DECLARE @EmployeeBenefitRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeBenefit');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CheckReceived');
DECLARE @AccrualsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Accruals');
DECLARE @AccruedIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'AccruedIncome');
DECLARE @BalancesWithBanksRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BalancesWithBanks');
DECLARE @CashRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Cash');
DECLARE @ConsumerLoansRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ConsumerLoans');
DECLARE @CurrentBilledButNotIssuedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentBilledButNotIssued');
DECLARE @DeferredIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'DeferredIncome');
DECLARE @PrepaymentsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Prepayments');
DECLARE @ReceivablesFromRentalOfPropertiesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ReceivablesFromRentalOfProperties');
DECLARE @ReceivablesFromSaleOfPropertiesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ReceivablesFromSaleOfProperties');
DECLARE @RefundsProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RefundsProvision');
DECLARE @RentDeferredIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RentDeferredIncome');
DECLARE @RestructuringProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RestructuringProvision');
DECLARE @RetentionPayableRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RetentionPayable');
DECLARE @TradePayableRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradePayable');
DECLARE @TradeReceivableRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TradeReceivable');
DECLARE @WarrantyProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'WarrantyProvision');

/*
Variables
(@LandMemberRD),
(@BuildingsMemberRD),
(@MachineryMemberRD),
(@MotorVehiclesMemberRD),
(@FixturesAndFittingsMemberRD),
(@OfficeEquipmentMemberRD),
(@ComputerEquipmentMemberRD),
(@CommunicationAndNetworkEquipmentMemberRD),
(@NetworkInfrastructureMemberRD),
(@BearerPlantsMemberRD),
(@TangibleExplorationAndEvaluationAssetsMemberRD),
(@MiningAssetsMemberRD),
(@OilAndGasAssetsMemberRD),
(@PowerGeneratingAssetsMemberRD),
(@LeaseholdImprovementsMemberRD),
(@ConstructionInProgressMemberRD),
(@OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRD),
(@OtherPropertyPlantAndEquipmentMemberRD),
(@InvestmentPropertyCompletedMemberRD),
(@InvestmentPropertyUnderConstructionOrDevelopmentMemberRD),
(@MerchandiseRD),
(@CurrentFoodAndBeverageRD),
(@CurrentAgriculturalProduceRD),
(@FinishedGoodsRD),
(@PropertyIntendedForSaleInOrdinaryCourseOfBusinessRD),
(@WorkInProgressRD),
(@RawMaterialsRD),
(@ProductionSuppliesRD),
(@CurrentPackagingAndStorageMaterialsRD),
(@SparePartsRD),
(@CurrentFuelRD),
(@OtherInventoriesRD),
(@TradeMedicineRD),
(@TradeConstructionMaterialRD),
(@TradeSparePartRD),
(@FinishedGrainRD),
(@ByproductGrainRD),
(@FinishedVehicleRD),
(@FinishedOilRD),
(@ByproductOilRD),
(@RawGrainRD),
(@RawVehicleRD),
(@RevenueServiceRD),
(@EmployeeBenefitRD),
(@CheckReceivedRD),
(@AccrualsRD),
(@AccruedIncomeRD),
(@BalancesWithBanksRD),
(@CashRD),
(@ConsumerLoansRD),
(@CurrentBilledButNotIssuedRD),
(@DeferredIncomeRD),
(@PrepaymentsRD),
(@ReceivablesFromRentalOfPropertiesRD),
(@ReceivablesFromSaleOfPropertiesRD),
(@RefundsProvisionRD),
(@RentDeferredIncomeRD),
(@RestructuringProvisionRD),
(@RetentionPayableRD),
(@TradePayableRD),
(@TradeReceivableRD),
(@WarrantyProvisionRD);
*/