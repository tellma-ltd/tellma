INSERT INTO @ResourceDefinitions([Index], [Code], [ResourceDefinitionType],[TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey],[CurrencyVisibility],[CenterVisibility],[ImageVisibility],[DescriptionVisibility],[LocationVisibility],[FromDateVisibility],[FromDateLabel],[ToDateVisibility],[ToDateLabel],[Decimal1Visibility],[Decimal1Label],[IdentifierVisibility],[IdentifierLabel],[UnitCardinality], [DefaultUnitId],[MonetaryValueVisibility],[Agent1DefinitionId]) VALUES
(1, N'Merchandise', N'InventoriesTotal', N'Merchandise', N'Merchandise', N'barcode', N'Purchasing', 210,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(2, N'CurrentFoodAndBeverage', N'InventoriesTotal', N'Food and Beverage', N'Food and Beverage', N'utensils', N'Purchasing', 220,  N'Required', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(3, N'CurrentAgriculturalProduce', N'InventoriesTotal', N'Agricultural Produce', N'Agricultural Produce', N'carrot', N'Production', 230,  N'Required', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(4, N'FinishedGoods', N'InventoriesTotal', N'Finished Good', N'Finished Goods', N'gifts', N'Production', 240,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(5, N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'InventoriesTotal', N'Property For Sale', N'Properties For Sale', N'sign', N'Sales', 250,  N'Required', N'None', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'None',NULL, N'None',NULL),
(6, N'WorkInProgress', N'InventoriesTotal', N'Work In Progress', N'Works In Progress', N'spinner', N'Production', 260,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(7, N'RawMaterials', N'InventoriesTotal', N'Raw Material', N'Raw Materials', N'boxes', N'Inventory', 270,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(8, N'ProductionSupplies', N'InventoriesTotal', N'Production Supply', N'Production Supplies', N'parachute-box', N'Purchasing', 280,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(9, N'CurrentPackagingAndStorageMaterials', N'InventoriesTotal', N'Packaging and Storage Material', N'Packaging and Storage Materials', N'box', N'Production', 285,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(10, N'SpareParts', N'InventoriesTotal', N'Spare Part', N'Spare Parts', N'undo-alt', N'Purchasing', 290,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(11, N'CurrentFuel', N'InventoriesTotal', N'Fuel', N'Fuel', N'gas-pump', N'Purchasing', 300,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@ltr, N'None',NULL),
(12, N'OtherInventories', N'InventoriesTotal', N'Other Inventory', N'Other Inventories', N'shapes', N'Purchasing', 310,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(13, N'LivingAnimalsMember', N'BiologicalAssets', N'Living Animal', N'Living Animals', N'kiwi-bird', N'Agriculture', 10,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(14, N'PlantsMember', N'BiologicalAssets', N'Plant', N'Plants', N'tree', N'Agriculture', 20,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(15, N'LivingAnimalsFlock', N'BiologicalAssets', N'Living Animal Flock', N'Living Animal Flocks', N'kiwi-bird', N'Agriculture', 10,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(16, N'PlantsBatch', N'BiologicalAssets', N'Plant Batch', N'Plant Batches', N'tree', N'Agriculture', 20,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(17, N'OrdinarySharesMember', N'OtherFinancialLiabilities', N'Ordinary share', N'Ordinary shares', N'', N'Financials', 75,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(18, N'PreferenceSharesMember', N'OtherFinancialLiabilities', N'Preference share', N'Preference shares', N'', N'Financials', 85,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(19, N'TelecommunicationServices', N'Miscellaneous', N'Telecommunication Service', N'Telecommunication Services', N'broadcast-tower', N'Sales', 95,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(20, N'TransportServices', N'Miscellaneous', N'Transport Service', N'Transport Services', N'bus', N'Sales', 105,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(21, N'InformationTechnologyMaintenanceAndSupportServices', N'Miscellaneous', N'IT Maintenance And Support Service', N'IT Maintenance And Support Services', N'', N'Sales', 115,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(22, N'InformationTechnologyConsultingServices', N'Miscellaneous', N'IT Consulting Service', N'IT Consulting Services', N'', N'Sales', 125,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(23, N'RoomOccupancyServices', N'Miscellaneous', N'Room Occupancy Service', N'Room Occupancy Services', N'', N'Sales', 135,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(24, N'CustomerOtherPointOfTimeServices', N'Miscellaneous', N'Service (Revenue)', N'Services (Revenues)', N'hands-helping', N'Sales', 10,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(25, N'CustomerOtherPeriodOfTimeServices', N'Miscellaneous', N'Rental (Revenue)', N'Rentals (Revenues)', N'hourglass-half', N'Sales', 15,  N'Required', N'Required', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(26, N'PerformanceObligations', N'Miscellaneous', N'Performance Obligation', N'Performance Obligations', N'', N'Sales', 20,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(27, N'SupplierOtherPointOfTimeServices', N'Miscellaneous', N'Service (Expense)', N'Services (Expenses)', N'hands-helping', N'Purchasing', 25,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(28, N'SupplierOtherPeriodOfTimeServices', N'Miscellaneous', N'Rental (Expense)', N'Rentals (Expenses)', N'hourglass-half', N'Purchasing', 25,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(29, N'EmployeeBenefits', N'Miscellaneous', N'Employee Benefit', N'Employees Benefits', N'@EmployeeBenefitsRD', N'HumanCapital', 35,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(30, N'OvertimeBenefits', N'Miscellaneous', N'Overtime Benefit', N'Overtime Benefits', N'user-clock', N'HumanCapital', 40,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(31, N'SocialSecurityBenefits', N'Miscellaneous', N'Social Security Benefit', N'Social Security Benefits', N'', N'HumanCapital', 45,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(32, N'EmployeeDeductions', N'Miscellaneous', N'Employee Deduction', N'Employee Deductions', N'', N'HumanCapital', 50,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(33, N'ChecksReceived', N'OtherFinancialAssets', N'Check Received', N'Checks Received', N'money-check', N'Financials', 65,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Check Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'None',NULL, N'Required',@CustomerAD),
(34, N'FinancialGuarantees', N'OtherFinancialAssets', N'Check Received', N'Checks Received', N'money-check', N'Financials', 65,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Check Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'None',NULL, N'Required',@CustomerAD),
(35, N'LeaveTypes', N'Miscellaneous', N'Leave Type', N'Leave Types', N'sign-out-alt', N'HumanCapital', 55,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@d, N'None',NULL),
(36, N'Jobs', N'Miscellaneous', N'Job', N'Jobs', N'user-md', N'HumanCapital', 56,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(37, N'MarketingActivities', N'Miscellaneous', N'Marketing Activity', N'Marketing Activities', N'', N'Marketing', 90,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(38, N'DepreciationBases', N'PropertyPlantAndEquipment', N'Dep. Base', N'Dep. Bases', N'percentage', N'FixedAssets', 210,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(39, N'AmortizationBases', N'IntangibleAssetsOtherThanGoodwill', N'Amo. Base', N'Amo. Bases', N'percent', N'FixedAssets', 220,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(65, N'Attendances', N'Miscellaneous', N'Attendance', N'Attendances', N'user-clock', N'HumanCapital', 70,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(66, N'Tasks', N'Miscellaneous', N'Task', N'Tasks', N'tasks', N'Administration', 75,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'Required', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'None',@EmployeeAD),
(67, N'AcademicDegrees', N'Miscellaneous', N'Academic Degree', N'Academic Degrees', N'user-graduate', N'HumanCapital', 85,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(68, N'ProfessionalCertifications', N'Miscellaneous', N'Professional Certification', N'Professional Certifications', N'certificate', N'HumanCapital', 110,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(69, N'Languages', N'Miscellaneous', N'Language', N'Languages', N'language', N'HumanCapital', 90,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(70, N'FamilyRelationships', N'Miscellaneous', N'Family Relationship', N'Family Relationships', N'user-friends', N'HumanCapital', 100,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(71, N'LicensesPermitsIds', N'Miscellaneous', N'License, Permit, Id', N'Licenses, Permits, Ids', N'id-badge', N'HumanCapital', 110,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(72, N'TransportationMeans', N'Miscellaneous', N'Transpotation Mean', N'Transpotation Means', N'bus', N'HumanCapital', 120,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(73, N'HousingMeans', N'Miscellaneous', N'Housing Mean', N'Housing Means', N'home', N'HumanCapital', 130,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(74, N'Breaches', N'Miscellaneous', N'Breach', N'Breaches', N'gavel', N'HumanCapital', 130,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL);

UPDATE @ResourceDefinitions SET 
    [Decimal2Visibility] = N'None',
    [Decimal3Visibility] = N'None',
    [Decimal4Visibility] = N'None',
    [EconomicOrderQuantityVisibility] = N'None',
    [Int1Visibility] = N'None',
    [Int2Visibility] = N'None',
    [Lookup1Visibility] = N'None',
    [Lookup2Visibility] = N'None',
    [Lookup3Visibility] = N'None',
    [Lookup4Visibility] = N'None',
    [MonetaryValueVisibility] = N'None',
    [Agent1Visibility] = N'None',
    [Agent2Visibility] = N'None',
    [ReorderLevelVisibility] = N'None',
    [Resource1Visibility] = N'None',
    [Resource2Visibility] = N'None',
    [Text1Visibility] = N'None',
    [Text2Visibility] = N'None',
    [UnitMassVisibility] = N'None',
    [VatRateVisibility] = N'None';

UPDATE @ResourceDefinitions
	SET [Agent1Visibility] = N'Required'
	WHERE [Agent1DefinitionId] IS NOT NULL

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'RevenueService');

	UPDATE @ResourceDefinitions
	SET 
		[Text1Visibility] = N'Required',
		[Text1Label] = N'Check Number',
		[Lookup4Visibility] = N'Required'
	WHERE [Code] IN (
		N'CheckReceived'
	);
	
INSERT INTO @ValidationErrors
EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@UserId = @AdminUserId,
    @Culture = @PrimaryLanguageId,
    @NeutralCulture = @PrimaryLanguageId;

	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'ResourceDefinitions: Error Provisioning'
	GOTO Err_Label;
END;

INSERT INTO @ResourceDefinitionIds([Id], [Index]) SELECT [Id], [Id] FROM dbo.[ResourceDefinitions]
--WHERE [Code] in (
--	N'Supplier', N'Customer',  N'Employee', N'Bank', N'BankAccount',  N'CashOnHandAccount',
--	N'Warehouse', N'TaxDepartment', N'JobOrder', N'Shipment', N'MotorVehiclesMember', N'OfficeEquipmentMember',
--	N'ComputerEquipmentMember', N'ConstructionInProgress',
--	N'Land'
--);

EXEC [dal].[ResourceDefinitions__UpdateState]
	@Ids = @ResourceDefinitionIds,
	@State = N'Visible',
	@UserId = @AdminUserId;

--Declarations
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
DECLARE @LivingAnimalsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LivingAnimalsMember');
DECLARE @PlantsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PlantsMember');
DECLARE @LivingAnimalsFlockRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LivingAnimalsFlock');
DECLARE @PlantsBatchRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PlantsBatch');
DECLARE @OrdinarySharesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OrdinarySharesMember');
DECLARE @PreferenceSharesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PreferenceSharesMember');
DECLARE @TelecommunicationServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TelecommunicationServices');
DECLARE @TransportServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TransportServices');
DECLARE @InformationTechnologyMaintenanceAndSupportServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'InformationTechnologyMaintenanceAndSupportServices');
DECLARE @InformationTechnologyConsultingServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'InformationTechnologyConsultingServices');
DECLARE @RoomOccupancyServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RoomOccupancyServices');
DECLARE @CustomerOtherPointOfTimeServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CustomerOtherPointOfTimeServices');
DECLARE @CustomerOtherPeriodOfTimeServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CustomerOtherPeriodOfTimeServices');
DECLARE @PerformanceObligationsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'PerformanceObligations');
DECLARE @SupplierOtherPointOfTimeServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SupplierOtherPointOfTimeServices');
DECLARE @SupplierOtherPeriodOfTimeServicesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SupplierOtherPeriodOfTimeServices');
DECLARE @EmployeeBenefitsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeBenefits');
DECLARE @OvertimeBenefitsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OvertimeBenefits');
DECLARE @SocialSecurityBenefitsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SocialSecurityBenefits');
DECLARE @EmployeeDeductionsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeDeductions');
DECLARE @ChecksReceivedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ChecksReceived');
DECLARE @FinancialGuaranteesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FinancialGuarantees');
DECLARE @LeaveTypesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LeaveTypes');
DECLARE @JobsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Jobs');
DECLARE @MarketingActivitiesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MarketingActivities');
DECLARE @DepreciationBasesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'DepreciationBases');
DECLARE @AmortizationBasesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'AmortizationBases');
DECLARE @AttendancesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Attendances');
DECLARE @TasksRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Tasks');
DECLARE @AcademicDegreesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'AcademicDegrees');
DECLARE @ProfessionalCertificationsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ProfessionalCertifications');
DECLARE @LanguagesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Languages');
DECLARE @FamilyRelationshipsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FamilyRelationships');
DECLARE @LicensesPermitsIdsRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LicensesPermitsIds');
DECLARE @TransportationMeansRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TransportationMeans');
DECLARE @HousingMeansRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'HousingMeans');
DECLARE @BreachesRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Breaches');