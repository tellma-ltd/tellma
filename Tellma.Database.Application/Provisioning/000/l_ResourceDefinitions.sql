INSERT INTO @ResourceDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey],[CurrencyVisibility],[CenterVisibility],[ImageVisibility],[DescriptionVisibility],[LocationVisibility],[FromDateVisibility],[FromDateLabel],[ToDateVisibility],[ToDateLabel],[Decimal1Visibility],[Decimal1Label],[IdentifierVisibility],[IdentifierLabel],[UnitCardinality], [DefaultUnitId],[MonetaryValueVisibility],[ParticipantDefinitionId]) VALUES
(0, N'LandMember', N'Land', N'Lands', N'sign', N'FixedAssets', 10,  N'None', N'Required', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(1, N'BuildingsMember', N'Building', N'Buildings', N'building', N'FixedAssets', 20,  N'None', N'Required', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(2, N'MachineryMember', N'Machinery', N'Machineries', N'cogs', N'FixedAssets', 30,  N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(3, N'MotorVehiclesMember', N'Motor Vehicle', N'Motor Vehicles', N'car', N'FixedAssets', 40,  N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Plate #', N'Single',@mo, N'None',NULL),
(4, N'FixturesAndFittingsMember', N'Fixture and Fitting', N'Fixtures and Fittings', N'puzzle-piece', N'FixedAssets', 50,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(5, N'OfficeEquipmentMember', N'Office Equipment', N'Office Equipment', N'fax', N'FixedAssets', 60,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(6, N'ComputerEquipmentMember', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets', 70,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(7, N'CommunicationAndNetworkEquipmentMember', N'Comm. Network Equipment', N'Comm. Network Equipment', N'network-wired', N'FixedAssets', 80,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(8, N'NetworkInfrastructureMember', N'Network Infrastructure', N'Network Infrastructure', N'project-diagram', N'FixedAssets', 90,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(9, N'BearerPlantsMember', N'Bearer Plant', N'Bearer Plants', N'holly-berry', N'FixedAssets', 100,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(10, N'TangibleExplorationAndEvaluationAssetsMember', N'Exploration Asset', N'Exploration Assets', N'download', N'FixedAssets', 110,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(11, N'MiningAssetsMember', N'Mining Asset', N'Mining Assets', N'hammer', N'FixedAssets', 120,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(12, N'OilAndGasAssetsMember', N'Oil and Gas Asset', N'Oil and Gas Assets', N'gas-pump', N'FixedAssets', 130,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(13, N'PowerGeneratingAssetsMember', N'Power Generating Asset', N'Power Generating Assets', N'bolt', N'FixedAssets', 140,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(14, N'LeaseholdImprovementsMember', N'Leasehold Improvement', N'Leasehold Improvements', N'paint-roller', N'FixedAssets', 150,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(15, N'ConstructionInProgressMember', N'Construction In Progress', N'Construction In Progress', N'drafting-compass', N'FixedAssets', 160,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(16, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'Owner Occupied Property', N'Owner Occupied Property', N'campground', N'FixedAssets', 170,  N'None', N'None', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(17, N'OtherPropertyPlantAndEquipmentMember', N'Other Fixed Asset', N'Other Fixed Assets', N'tags', N'FixedAssets', 180,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(18, N'InvestmentPropertyCompletedMember', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets', 190,  N'None', N'None', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(19, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'Investment Property (under Construction)', N'Investment Properties (under Construction)', N'store-slash', N'FixedAssets', 200,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(20, N'Merchandise', N'Merchandise', N'Merchandise', N'barcode', N'Purchasing', 210,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(21, N'CurrentFoodAndBeverage', N'Food and Beverage', N'Food and Beverage', N'utensils', N'Purchasing', 220,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(22, N'CurrentAgriculturalProduce', N'Agricultural Produce', N'Agricultural Produce', N'carrot', N'Production', 230,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(23, N'FinishedGoods', N'Finished Good', N'Finished Goods', N'gifts', N'Production', 240,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(24, N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'Property For Sale', N'Properties For Sale', N'sign', N'Sales', 250,  N'None', N'None', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'None',NULL, N'None',NULL),
(25, N'WorkInProgress', N'Work In Progress', N'Works In Progress', N'spinner', N'Production', 260,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(26, N'RawMaterials', N'Raw Material', N'Raw Materials', N'boxes', N'Purchasing', 270,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(27, N'ProductionSupplies', N'Production Supply', N'Production Supplies', N'parachute-box', N'Purchasing', 280,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(28, N'CurrentPackagingAndStorageMaterials', N'Packaging and Storage Material', N'Packaging and Storage Materials', N'box', N'Production', 285,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(29, N'SpareParts', N'Spare Part', N'Spare Parts', N'undo-alt', N'Purchasing', 290,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(30, N'CurrentFuel', N'Fuel', N'Fuel', N'gas-pump', N'Purchasing', 300,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@ltr, N'None',NULL),
(31, N'OtherInventories', N'Other Inventory', N'Other Inventories', N'shapes', N'Purchasing', 310,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(32, N'TradeMedicine', N'Medicine', N'Medicines', N'pills', N'Purchasing', 90,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Manufacturing Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(33, N'TradeConstructionMaterial', N'Construction Material', N'Construction Materials', N'building', N'Purchasing', 100,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(34, N'TradeSparePart', N'Spare Part (sale)', N'Spare Parts (sale)', N'recycle', N'Purchasing', 110,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(35, N'FinishedGrain', N'Cleaned Grain', N'Cleaned Grains', N'boxes', N'Production', 20,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@qn50, N'None',NULL),
(36, N'ByproductGrain', N'Reject Grain', N'Reject Grains', N'recycle', N'Production', 30,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(37, N'FinishedVehicle', N'Assembled Vehicle', N'Assembled Vehicles', N'car-side', N'Production', 50,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Chassis #', N'None',NULL, N'None',NULL),
(38, N'FinishedOil', N'Processed Oil (milling)', N'Processed Oils (milling)', N'tint', N'Production', 60,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(39, N'ByproductOil', N'Oil Byproduct', N'Oils Byproducts', N'tint-slash', N'Production', 70,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(40, N'RawGrain', N'Raw Grain', N'Raw Grains', N'tractor', N'Purchasing', 10,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@qn100, N'None',NULL),
(41, N'RawVehicle', N'Vehicle Component', N'Vehicles Components', N'cogs', N'Purchasing', 40,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(42, N'RevenueService', N'Revenue Service', N'Revenue Services', N'hands-helping', N'Sales', 10,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(43, N'EmployeeBenefit', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital', 20,  N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(44, N'CheckReceived', N'Check Received', N'Checks Received', N'money-check', N'Financials', 30,  N'Required', N'Required', N'Optional', N'Optional', N'None', N'Optional', N'Check Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'None',NULL, N'Required',@CustomerRLD),
(45, N'Accruals', N'Accrual', N'Accruals', N'', N'Financials', 40,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'Optional', N'Invoice Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@SupplierRLD),
(46, N'AccruedIncome', N'Accrued Income', N'Accrued Incomes', N'', N'Financials', 50,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(47, N'EmployeeLoan', N'Employee Loan', N'Employees Loans', N'', N'Financials', 80,  N'Required', N'Required', N'None', N'Optional', N'None', N'Optional', N'Loan Date', N'Optional', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@EmployeeRLD),
(48, N'CurrentBilledButNotIssued', N'Billed Not Issued', N'Billed Not Issued', N'', N'Financials', 90,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(49, N'DeferredIncome', N'Deferred Income', N'Deferred Incomes', N'', N'Financials', 100,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(50, N'Prepayments', N'Prepayment', N'Prepayments', N'', N'Financials', 110,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@SupplierRLD),
(51, N'SalaryAdvance', N'Salary Advance', N'Salary Advances', N'', N'Financials', 110,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@EmployeeRLD),
(52, N'ReceivablesFromRentalOfProperties', N'Rental Receivable', N'Rental Receivables', N'', N'Financials', 120,  N'Required', N'Required', N'None', N'Optional', N'None', N'Optional', N'Invoice Date', N'Optional', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(53, N'ReceivablesFromSaleOfProperties', N'Sale Receivable', N'Sale Receivables', N'', N'Financials', 130,  N'Required', N'Required', N'None', N'Optional', N'None', N'Optional', N'Invoice Date', N'Optional', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(54, N'RefundsProvision', N'Refund Provision', N'Refund Provisions', N'', N'Financials', 150,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(55, N'RentDeferredIncome', N'Rent Deferred Income', N'Rent Deferred Incomes', N'', N'Financials', 160,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(56, N'RestructuringProvision', N'Restructuring Provision', N'Restructuring Provisions', N'', N'Financials', 170,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',NULL),
(57, N'RetentionPayable', N'Retention Payable', N'Retention Payables', N'', N'Financials', 180,  N'Required', N'Required', N'None', N'Optional', N'None', N'Optional', N'Retention Date', N'Optional', N'Payback Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@SupplierRLD),
(58, N'TradePayable', N'Trade Payable', N'Trade Payables', N'', N'Financials', 190,  N'Required', N'Required', N'None', N'Optional', N'None', N'Optional', N'Invoice Date', N'Optional', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@SupplierRLD),
(59, N'TradeReceivable', N'Trade Receivable', N'Trade Receivables', N'', N'Financials', 200,  N'Required', N'Required', N'None', N'Optional', N'None', N'Optional', N'Invoice Date', N'Optional', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'Optional',@CustomerRLD),
(60, N'WarrantyProvision', N'Warranty Provision', N'Warranty Provisions', N'', N'Financials', 210,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',NULL);
UPDATE @ResourceDefinitions
	SET [ParticipantVisibility] = N'Required'
	WHERE [ParticipantDefinitionId] IS NOT NULL

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
	SET 
		[Lookup1Visibility] = N'Optional',
		[Lookup1Label] = N'Make',
		[Lookup1DefinitionId] = @VehicleMakeLKD
	WHERE [Code] IN (
		'MotorVehiclesMember'
	);

	UPDATE @ResourceDefinitions
	SET 
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
		[Text1Visibility] = N'Required',
		[Text1Label] = N'Check Number',
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
DECLARE @EmployeeLoanRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeLoan');
DECLARE @CurrentBilledButNotIssuedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CurrentBilledButNotIssued');
DECLARE @DeferredIncomeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'DeferredIncome');
DECLARE @PrepaymentsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Prepayments');
DECLARE @SalaryAdvanceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SalaryAdvance');
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
(@EmployeeLoanRD),
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