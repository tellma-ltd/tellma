INSERT INTO @ResourceDefinitions([Index], [Code], [ResourceDefinitionType],[TitleSingular], [TitlePlural], [MainMenuIcon],
[MainMenuSection], [MainMenuSortKey],[CurrencyVisibility],[CenterVisibility],[ImageVisibility],[DescriptionVisibility],
[LocationVisibility],[FromDateVisibility],[FromDateLabel],[ToDateVisibility],[ToDateLabel],[Decimal1Visibility],[Decimal1Label],
[IdentifierVisibility],[IdentifierLabel],[UnitCardinality], [DefaultUnitId],[MonetaryValueVisibility],[ParticipantDefinitionId]) VALUES
(0, N'InvestmentPropertyCompletedMember', N'InvestmentProperty', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets', 190,  N'Required', N'None', N'None', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(1, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'InvestmentProperty', N'Investment Property (under Construction)', N'Investment Properties (under Construction)', N'store-slash', N'FixedAssets', 200,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(2, N'Merchandise', N'InventoriesTotal', N'Merchandise', N'Merchandise', N'barcode', N'Purchasing', 210,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(3, N'CurrentFoodAndBeverage', N'InventoriesTotal', N'Food and Beverage', N'Food and Beverage', N'utensils', N'Purchasing', 220,  N'Required', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(4, N'CurrentAgriculturalProduce', N'InventoriesTotal', N'Agricultural Produce', N'Agricultural Produce', N'carrot', N'Production', 230,  N'Required', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(5, N'FinishedGoods', N'InventoriesTotal', N'Finished Good', N'Finished Goods', N'gifts', N'Production', 240,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(6, N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'InventoriesTotal', N'Property For Sale', N'Properties For Sale', N'sign', N'Sales', 250,  N'Required', N'None', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'None',NULL, N'None',NULL),
(7, N'WorkInProgress', N'InventoriesTotal', N'Work In Progress', N'Works In Progress', N'spinner', N'Production', 260,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(8, N'RawMaterials', N'InventoriesTotal', N'Raw Material', N'Raw Materials', N'boxes', N'Inventory', 270,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(9, N'ProductionSupplies', N'InventoriesTotal', N'Production Supply', N'Production Supplies', N'parachute-box', N'Purchasing', 280,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(10, N'CurrentPackagingAndStorageMaterials', N'InventoriesTotal', N'Packaging and Storage Material', N'Packaging and Storage Materials', N'box', N'Production', 285,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(11, N'SpareParts', N'InventoriesTotal', N'Spare Part', N'Spare Parts', N'undo-alt', N'Purchasing', 290,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(12, N'CurrentFuel', N'InventoriesTotal', N'Fuel', N'Fuel', N'gas-pump', N'Purchasing', 300,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@ltr, N'None',NULL),
(13, N'OtherInventories', N'InventoriesTotal', N'Other Inventory', N'Other Inventories', N'shapes', N'Purchasing', 310,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(14, N'CustomerPointService', N'Miscellaneous', N'Service (Revenue)', N'Services (Revenues)', N'hands-helping', N'Sales', 10,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(15, N'CustomerPeriodService', N'Miscellaneous', N'Rental (Revenue)', N'Rentals (Revenues)', N'hourglass-half', N'Sales', 15,  N'Required', N'Required', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(16, N'SupplierPointService', N'Miscellaneous', N'Service (Expense)', N'Services (Expenses)', N'hands-helping', N'Purchasing', 20,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(17, N'SupplierPeriodService', N'Miscellaneous', N'Rental (Expense)', N'Rentals (Expenses)', N'hourglass-half', N'Purchasing', 25,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(18, N'SalaryAllowance', N'Miscellaneous', N'Salary Allowance', N'Salary Allowances', N'money-bill-wave', N'HumanCapital', 30,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(19, N'EmployeeBenefit', N'Miscellaneous', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital', 35,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(20, N'OvertimeBenefit', N'Miscellaneous', N'Overtime Benefit', N'Overtime Benefits', N'user-clock', N'HumanCapital', 40,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(21, N'SocialSecurityBenefit', N'Miscellaneous', N'Social Security Benefit', N'Social Security Benefits', N'', N'HumanCapital', 45,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(22, N'EmployeeDeduction', N'Miscellaneous', N'Employee Deduction', N'Employee Deductions', N'', N'HumanCapital', 50,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(23, N'SoftwareComponent', N'Miscellaneous', N'Software Component', N'Software Components', N'code', N'Sales', 55,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(24, N'CheckReceived', N'OtherFinancialAssets', N'Check Received', N'Checks Received', N'money-check', N'Financials', 65,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Check Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'None',NULL, N'Required',@CustomerRLD),
(25, N'LeaveType', N'Miscellaneous', N'Leave Type', N'Leave Types', N'', N'HumanCapital', 55,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@d, N'None',NULL),
(26, N'EmployeeJob', N'Miscellaneous', N'Employee Job', N'Employee Jobs', N'', N'HumanCapital', 56,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(27, N'MarketingResource', N'Miscellaneous', N'Marketing Resource', N'Marketing Resources', N'', N'Marketing', 90,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(28, N'WarrantyProvision', N'Provisions', N'Warranty Provision', N'Warranty Provisions', N'', N'Financials', 85,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',NULL),
(29, N'DepreciationBase', N'PropertyPlantAndEquipment', N'Dep. Base', N'Dep. Bases', N'', N'FixedAssets', 210,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL);

UPDATE @ResourceDefinitions SET 
    [Decimal2Visibility] = N'None',
    [EconomicOrderQuantityVisibility] = N'None',
    [Int1Visibility] = N'None',
    [Int2Visibility] = N'None',
    [Lookup1Visibility] = N'None',
    [Lookup2Visibility] = N'None',
    [Lookup3Visibility] = N'None',
    [Lookup4Visibility] = N'None',
    [MonetaryValueVisibility] = N'None',
    [ParticipantVisibility] = N'None',
    [ReorderLevelVisibility] = N'None',
    [Resource1Visibility] = N'None',
    [Text1Visibility] = N'None',
    [Text2Visibility] = N'None',
    [UnitMassVisibility] = N'None',
    [VatRateVisibility] = N'None';

UPDATE @ResourceDefinitions
	SET [ParticipantVisibility] = N'Required'
	WHERE [ParticipantDefinitionId] IS NOT NULL

	UPDATE @ResourceDefinitions
	SET 
		[DescriptionVisibility]				= N'Optional'
	WHERE [Code] IN (N'RevenueService');

	UPDATE @ResourceDefinitions
	SET 
		[Text1Visibility] = N'Required',
		[Text1Label] = N'Check Number',
		[Lookup4Visibility] = N'Required'
		--[Lookup4Label] = N'Bank',
		--[Lookup4DefinitionId] = @BankLKD
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

--Declarations
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
DECLARE @CustomerPointServiceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CustomerPointService');
DECLARE @CustomerPeriodServiceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CustomerPeriodService');
DECLARE @SupplierPointServiceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SupplierPointService');
DECLARE @SupplierPeriodServiceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SupplierPeriodService');
DECLARE @SalaryAllowanceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SalaryAllowance');
DECLARE @EmployeeBenefitRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeBenefit');
DECLARE @OvertimeBenefitRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OvertimeBenefit');
DECLARE @SocialSecurityBenefitRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SocialSecurityBenefit');
DECLARE @EmployeeDeductionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeDeduction');
DECLARE @SoftwareComponentRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SoftwareComponent');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CheckReceived');
DECLARE @LeaveTypeRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LeaveType');
DECLARE @EmployeeJobRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeJob');
DECLARE @MarketingResourceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MarketingResource');
DECLARE @WarrantyProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'WarrantyProvision');
DECLARE @DepreciationBaseRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'DepreciationBase');