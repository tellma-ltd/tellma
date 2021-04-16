INSERT INTO @ResourceDefinitions([Index], [Code], [ResourceDefinitionType],[TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey],[CurrencyVisibility],[CenterVisibility],[ImageVisibility],[DescriptionVisibility],[LocationVisibility],[FromDateVisibility],[FromDateLabel],[ToDateVisibility],[ToDateLabel],[Decimal1Visibility],[Decimal1Label],[IdentifierVisibility],[IdentifierLabel],[UnitCardinality], [DefaultUnitId],[MonetaryValueVisibility],[ParticipantDefinitionId]) VALUES
(0, N'LandMember', N'PropertyPlantAndEquipment', N'Land', N'Lands', N'sign', N'FixedAssets', 10,  N'Required', N'None', N'None', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(1, N'BuildingsMember', N'PropertyPlantAndEquipment', N'Building', N'Buildings', N'building', N'FixedAssets', 20,  N'Required', N'None', N'None', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(2, N'MachineryMember', N'PropertyPlantAndEquipment', N'Machinery', N'Machineries', N'cogs', N'FixedAssets', 30,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(3, N'MotorVehiclesMember', N'PropertyPlantAndEquipment', N'Motor Vehicle', N'Motor Vehicles', N'car', N'FixedAssets', 40,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Plate #', N'Single',@mo, N'None',NULL),
(4, N'FixturesAndFittingsMember', N'PropertyPlantAndEquipment', N'Fixture and Fitting', N'Fixtures and Fittings', N'puzzle-piece', N'FixedAssets', 50,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(5, N'OfficeEquipmentMember', N'PropertyPlantAndEquipment', N'Office Equipment', N'Office Equipment', N'fax', N'FixedAssets', 60,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(6, N'ComputerEquipmentMember', N'PropertyPlantAndEquipment', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets', 70,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(7, N'CommunicationAndNetworkEquipmentMember', N'PropertyPlantAndEquipment', N'Comm. Network Equipment', N'Comm. Network Equipment', N'network-wired', N'FixedAssets', 80,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(8, N'NetworkInfrastructureMember', N'PropertyPlantAndEquipment', N'Network Infrastructure', N'Network Infrastructure', N'project-diagram', N'FixedAssets', 90,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(9, N'BearerPlantsMember', N'PropertyPlantAndEquipment', N'Bearer Plant', N'Bearer Plants', N'holly-berry', N'FixedAssets', 100,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(10, N'TangibleExplorationAndEvaluationAssetsMember', N'PropertyPlantAndEquipment', N'Exploration Asset', N'Exploration Assets', N'download', N'FixedAssets', 110,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(11, N'MiningAssetsMember', N'PropertyPlantAndEquipment', N'Mining Asset', N'Mining Assets', N'hammer', N'FixedAssets', 120,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(12, N'OilAndGasAssetsMember', N'PropertyPlantAndEquipment', N'Oil and Gas Asset', N'Oil and Gas Assets', N'gas-pump', N'FixedAssets', 130,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(13, N'PowerGeneratingAssetsMember', N'PropertyPlantAndEquipment', N'Power Generating Asset', N'Power Generating Assets', N'bolt', N'FixedAssets', 140,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'Optional', N'Tag #', N'Single',@mo, N'None',NULL),
(14, N'LeaseholdImprovementsMember', N'PropertyPlantAndEquipment', N'Leasehold Improvement', N'Leasehold Improvements', N'paint-roller', N'FixedAssets', 150,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(15, N'ConstructionInProgressMember', N'PropertyPlantAndEquipment', N'Construction In Progress', N'Construction In Progress', N'drafting-compass', N'FixedAssets', 160,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(16, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'PropertyPlantAndEquipment', N'Owner Occupied Property', N'Owner Occupied Property', N'campground', N'FixedAssets', 170,  N'Required', N'None', N'None', N'Optional', N'Optional', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(17, N'OtherPropertyPlantAndEquipmentMember', N'PropertyPlantAndEquipment', N'Other Fixed Asset', N'Other Fixed Assets', N'tags', N'FixedAssets', 180,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(18, N'InvestmentPropertyCompletedMember', N'InvestmentProperty', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets', 190,  N'Required', N'None', N'None', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(19, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'InvestmentProperty', N'Investment Property (under Construction)', N'Investment Properties (under Construction)', N'store-slash', N'FixedAssets', 200,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'Single',@mo, N'None',NULL),
(20, N'BrandNamesMember', N'IntangibleAssetsOtherThanGoodwill', N'Brand name', N'Brand names', N'', N'FixedAssets', 210,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(21, N'IntangibleExplorationAndEvaluationAssetsMember', N'IntangibleAssetsOtherThanGoodwill', N'Intangible exploration and evaluation asset', N'Intangible exploration and evaluation assets', N'', N'FixedAssets', 220,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(22, N'MastheadsAndPublishingTitlesMember', N'IntangibleAssetsOtherThanGoodwill', N'Mastheads and publishing title', N'Mastheads and publishing titles', N'', N'FixedAssets', 230,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(23, N'ComputerSoftwareMember', N'IntangibleAssetsOtherThanGoodwill', N'Computer software', N'Computer software', N'download', N'FixedAssets', 240,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(24, N'LicencesMember', N'IntangibleAssetsOtherThanGoodwill', N'Licence', N'Licences', N'', N'FixedAssets', 260,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(25, N'GSMLicencesMember', N'IntangibleAssetsOtherThanGoodwill', N'GSM licence', N'GSM licences', N'', N'FixedAssets', 270,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(26, N'UMTSLicencesMember', N'IntangibleAssetsOtherThanGoodwill', N'UMTS licence', N'UMTS licences', N'', N'FixedAssets', 280,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(27, N'LTELicencesMember', N'IntangibleAssetsOtherThanGoodwill', N'LTE licence', N'LTE licences', N'', N'FixedAssets', 290,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(28, N'GamingLicencesMember', N'IntangibleAssetsOtherThanGoodwill', N'Gaming licence', N'Gaming licences', N'dragon', N'FixedAssets', 300,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(29, N'FranchisesMember', N'IntangibleAssetsOtherThanGoodwill', N'Franchise', N'Franchises', N'', N'FixedAssets', 310,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(30, N'AirportLandingRightsMember', N'IntangibleAssetsOtherThanGoodwill', N'Airport landing right', N'Airport landing rights', N'plane-arrival', N'FixedAssets', 330,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(31, N'MiningRightsMember', N'IntangibleAssetsOtherThanGoodwill', N'Mining right', N'Mining rights', N'', N'FixedAssets', 340,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(32, N'BroadcastingRightsMember', N'IntangibleAssetsOtherThanGoodwill', N'Broadcasting right', N'Broadcasting rights', N'broadcast-tower', N'FixedAssets', 350,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(33, N'ServiceConcessionRightsMember', N'IntangibleAssetsOtherThanGoodwill', N'Service concession right', N'Service concession rights', N'', N'FixedAssets', 360,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(34, N'RecipesFormulaeModelsDesignsAndPrototypesMember', N'IntangibleAssetsOtherThanGoodwill', N'Recipes, formulae, model, design or prototype', N'Recipes, formulae, models, designs and prototypes', N'', N'FixedAssets', 370,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(35, N'CustomerrelatedIntangibleAssetsMember', N'IntangibleAssetsOtherThanGoodwill', N'Customer-related intangible asset', N'Customer-related intangible assets', N'', N'FixedAssets', 380,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(36, N'ValueOfBusinessAcquiredMember', N'IntangibleAssetsOtherThanGoodwill', N'Value of business acquired', N'Value of business acquired', N'', N'FixedAssets', 390,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(37, N'CapitalisedDevelopmentExpenditureMember', N'IntangibleAssetsOtherThanGoodwill', N'Capitalised development expenditure', N'Capitalised development expenditure', N'', N'FixedAssets', 400,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(38, N'IntangibleAssetsUnderDevelopmentMember', N'IntangibleAssetsOtherThanGoodwill', N'Intangible asset under development', N'Intangible assets under development', N'', N'FixedAssets', 410,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(39, N'TechnologybasedIntangibleAssetsMember', N'IntangibleAssetsOtherThanGoodwill', N'Technology-based intangible asset', N'Technology-based intangible assets', N'', N'FixedAssets', 420,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(40, N'OtherIntangibleAssetsMember', N'IntangibleAssetsOtherThanGoodwill', N'Other intangible asset', N'Other intangible assets', N'', N'FixedAssets', 430,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(41, N'Merchandise', N'InventoriesTotal', N'Merchandise', N'Merchandise', N'barcode', N'Purchasing', 210,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(42, N'CurrentFoodAndBeverage', N'InventoriesTotal', N'Food and Beverage', N'Food and Beverage', N'utensils', N'Purchasing', 220,  N'Required', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(43, N'CurrentAgriculturalProduce', N'InventoriesTotal', N'Agricultural Produce', N'Agricultural Produce', N'carrot', N'Production', 230,  N'Required', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Production Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(44, N'FinishedGoods', N'InventoriesTotal', N'Finished Good', N'Finished Goods', N'gifts', N'Production', 240,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(45, N'PropertyIntendedForSaleInOrdinaryCourseOfBusiness', N'InventoriesTotal', N'Property For Sale', N'Properties For Sale', N'sign', N'Sales', 250,  N'Required', N'None', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'Optional', N'Area (m^2)', N'None', N'', N'None',NULL, N'None',NULL),
(46, N'WorkInProgress', N'InventoriesTotal', N'Work In Progress', N'Works In Progress', N'spinner', N'Production', 260,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(47, N'RawMaterials', N'InventoriesTotal', N'Raw Material', N'Raw Materials', N'boxes', N'Inventory', 270,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(48, N'ProductionSupplies', N'InventoriesTotal', N'Production Supply', N'Production Supplies', N'parachute-box', N'Purchasing', 280,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(49, N'CurrentPackagingAndStorageMaterials', N'InventoriesTotal', N'Packaging and Storage Material', N'Packaging and Storage Materials', N'box', N'Production', 285,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(50, N'SpareParts', N'InventoriesTotal', N'Spare Part', N'Spare Parts', N'undo-alt', N'Purchasing', 290,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@pcs, N'None',NULL),
(51, N'CurrentFuel', N'InventoriesTotal', N'Fuel', N'Fuel', N'gas-pump', N'Purchasing', 300,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@ltr, N'None',NULL),
(52, N'OtherInventories', N'InventoriesTotal', N'Other Inventory', N'Other Inventories', N'shapes', N'Purchasing', 310,  N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(53, N'CustomerPointService', N'Miscellaneous', N'Service (Revenue)', N'Services (Revenues)', N'hands-helping', N'Sales', 10,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(54, N'CustomerPeriodService', N'Miscellaneous', N'Rental (Revenue)', N'Rentals (Revenues)', N'hourglass-half', N'Sales', 15,  N'Required', N'Required', N'Optional', N'Optional', N'Optional', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',@mo, N'None',NULL),
(55, N'SupplierPointService', N'Miscellaneous', N'Service (Expense)', N'Services (Expenses)', N'hands-helping', N'Purchasing', 20,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(56, N'SupplierPeriodService', N'Miscellaneous', N'Rental (Expense)', N'Rentals (Expenses)', N'hourglass-half', N'Purchasing', 25,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(57, N'SalaryAllowance', N'Miscellaneous', N'Salary Allowance', N'Salary Allowances', N'money-bill-wave', N'HumanCapital', 30,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(58, N'EmployeeBenefit', N'Miscellaneous', N'Employee Benefit', N'Employees Benefits', N'user-check', N'HumanCapital', 35,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(59, N'OvertimeBenefit', N'Miscellaneous', N'Overtime Benefit', N'Overtime Benefits', N'user-clock', N'HumanCapital', 40,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(60, N'SocialSecurityBenefit', N'Miscellaneous', N'Social Security Benefit', N'Social Security Benefits', N'', N'HumanCapital', 45,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(61, N'EmployeeDeduction', N'Miscellaneous', N'Employee Deduction', N'Employee Deductions', N'', N'HumanCapital', 50,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(62, N'SoftwareComponent', N'Miscellaneous', N'Software Component', N'Software Components', N'code', N'Sales', 55,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(63, N'SoftwareConsultancy', N'Miscellaneous', N'Software Consultancy', N'Software Consultancies', N'user-tie', N'Sales', 60,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'Single',NULL, N'None',NULL),
(64, N'CheckReceived', N'OtherFinancialAssets', N'Check Received', N'Checks Received', N'money-check', N'Financials', 65,  N'None', N'None', N'Optional', N'Optional', N'None', N'Optional', N'Check Date', N'Optional', N'Expiry Date', N'None', N'', N'None', N'', N'None',NULL, N'Required',@CustomerRLD),
(65, N'Attendance', N'Miscellaneous', N'Attendance', N'Attendances', N'user-clock', N'HumanCapital', 70,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL),
(66, N'Task', N'Miscellaneous', N'Task', N'Tasks', N'', N'Administration', 75,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'Required', N'Due Date', N'None', N'', N'None', N'', N'None',NULL, N'None',@EmployeeRLD),
(67, N'EmployeeLoan', N'OtherFinancialAssets', N'Employee Loan', N'Employees Loans', N'hand-holding-usd', N'HumanCapital', 80,  N'Required', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',@EmployeeRLD),
(68, N'WarrantyProvision', N'Provisions', N'Warranty Provision', N'Warranty Provisions', N'', N'Financials', 85,  N'Required', N'Required', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'Optional',NULL),
(69, N'MarketingResource', N'Miscellaneous', N'Marketing Resource', N'Marketing Resources', N'', N'Marketing', 90,  N'None', N'None', N'None', N'Optional', N'None', N'None', N'', N'None', N'', N'None', N'', N'None', N'', N'None',NULL, N'None',NULL);
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
DECLARE @BrandNamesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BrandNamesMember');
DECLARE @IntangibleExplorationAndEvaluationAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'IntangibleExplorationAndEvaluationAssetsMember');
DECLARE @MastheadsAndPublishingTitlesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MastheadsAndPublishingTitlesMember');
DECLARE @ComputerSoftwareMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ComputerSoftwareMember');
DECLARE @LicencesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LicencesMember');
DECLARE @GSMLicencesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'GSMLicencesMember');
DECLARE @UMTSLicencesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'UMTSLicencesMember');
DECLARE @LTELicencesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'LTELicencesMember');
DECLARE @GamingLicencesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'GamingLicencesMember');
DECLARE @FranchisesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'FranchisesMember');
DECLARE @AirportLandingRightsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'AirportLandingRightsMember');
DECLARE @MiningRightsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MiningRightsMember');
DECLARE @BroadcastingRightsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'BroadcastingRightsMember');
DECLARE @ServiceConcessionRightsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ServiceConcessionRightsMember');
DECLARE @RecipesFormulaeModelsDesignsAndPrototypesMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'RecipesFormulaeModelsDesignsAndPrototypesMember');
DECLARE @CustomerrelatedIntangibleAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CustomerrelatedIntangibleAssetsMember');
DECLARE @ValueOfBusinessAcquiredMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'ValueOfBusinessAcquiredMember');
DECLARE @CapitalisedDevelopmentExpenditureMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CapitalisedDevelopmentExpenditureMember');
DECLARE @IntangibleAssetsUnderDevelopmentMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'IntangibleAssetsUnderDevelopmentMember');
DECLARE @TechnologybasedIntangibleAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'TechnologybasedIntangibleAssetsMember');
DECLARE @OtherIntangibleAssetsMemberRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'OtherIntangibleAssetsMember');
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
DECLARE @SoftwareConsultancyRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'SoftwareConsultancy');
DECLARE @CheckReceivedRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'CheckReceived');
DECLARE @AttendanceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Attendance');
DECLARE @TaskRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'Task');
DECLARE @EmployeeLoanRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'EmployeeLoan');
DECLARE @WarrantyProvisionRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'WarrantyProvision');
DECLARE @MarketingResourceRD INT = (SELECT [Id] FROM dbo.[ResourceDefinitions] WHERE [Code] = N'MarketingResource');