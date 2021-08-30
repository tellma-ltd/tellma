INSERT INTO @AgentDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey], [CenterVisibility], [CurrencyVisibility],[ImageVisibility], [LocationVisibility], [FromDateVisibility], [FromDateLabel], [ToDateVisibility], [ToDateLabel], [TaxIdentificationNumberVisibility],[BankAccountNumberVisibility], [Agent1Visibility], [UserCardinality]) VALUES
(0, N'Creditor', N'Creditor', N'Creditors', N'hands', N'Financials',100,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'Single'),
(1, N'Debtor', N'Debtor', N'Debtors', N'hand-holding-usd', N'Financials',105,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None'),
(2, N'Owner', N'Owner', N'Owners', N'power-off', N'Financials',110,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(3, N'Partner', N'Partner', N'Partners', N'user-tie', N'Financials',115,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(4, N'Supplier', N'Supplier', N'Suppliers', N'user-tag', N'Purchasing',120,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(5, N'Customer', N'Customer', N'Customers ', N'user-shield', N'Sales',125,N'None', N'None', N'None', N'None', N'Optional', N'Customer Since', N'None', N'None', N'Optional', N'None', N'None', N'Single'),
(6, N'Employee', N'Employee', N'Employees', N'user-friends', N'HumanCapital',130,N'None', N'None', N'Optional', N'None', N'Optional', N'Joining Date', N'Optional', N'Termination Date', N'Optional', N'Optional', N'None', N'Single'),
(7, N'FamilyMember', N'Family Member', N'Family Members', N'user-circle', N'HumanCapital',135,N'None', N'None', N'Optional', N'None', N'Optional', N'DOB', N'None', N'None', N'None', N'None', N'Required', N'None'),
(8, N'Bank', N'Bank', N'Banks', N'landmark', N'Cash',140,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(9, N'BankBranch', N'Bank Branch', N'Bank Branches', N'university', N'Cash',145,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(10, N'Other', N'Other', N'Others', N'air-freshener', N'Financials',150,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(11, N'BankAccount', N'Bank Account', N'Bank Accounts', N'book', N'Cash',155,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None'),
(12, N'CashOnHandAccount', N'Cash Account', N'Cash On Hand Accounts', N'door-closed', N'Cash',160,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(13, N'Warehouse', N'Warehouse', N'Warehouses', N'warehouse', N'Inventory',165,N'Optional', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(14, N'TaxDepartment', N'Tax Department', N'Tax Departments', N'angry', N'Financials',170,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None'),
(15, N'ProductionUnit', N'Production Unit', N'Production Units', N'clipboard-list', N'Production',175,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(16, N'IncomingShipment', N'Incoming Shipment', N'Incoming Shipments', N'ship', N'Purchasing',180,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(17, N'Farm', N'Farm', N'Farms', N'Industry', N'Production',185,N'Optional', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(18, N'Prospect', N'Prospect', N'Prospects', N'kiss-wink-heart', N'Marketing',190,N'Required', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(19, N'Contact', N'Contact', N'Contacts', N'user-circle', N'Marketing',195,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(20, N'LandMember', N'Land', N'Land', N'sign', N'FixedAssets',200,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(21, N'BuildingsMember', N'Buildings', N'Buildings', N'building', N'FixedAssets',205,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(22, N'MachineryMember', N'Machinery', N'Machinery', N'cogs', N'FixedAssets',210,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(23, N'ShipsMember', N'Ship', N'Ships', N'ship', N'FixedAssets',215,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(24, N'AircraftMember', N'Aircraft', N'Aircrafts', N'plane', N'FixedAssets',220,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(25, N'MotorVehiclesMember', N'Motor Vehicle', N'Motor Vehicles', N'car', N'FixedAssets',225,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(26, N'FixturesAndFittingsMember', N'Fixture and fitting', N'Fixtures and fittings', N'puzzle-piece', N'FixedAssets',230,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(27, N'OfficeEquipmentMember', N'Office equipment', N'Office equipment', N'fax', N'FixedAssets',235,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(28, N'ComputerEquipmentMember', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets',236,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(29, N'CommunicationAndNetworkEquipmentMember', N'Comm. Network Equipment', N'Comm. Network Equipment', N'network-wired', N'FixedAssets',237,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(30, N'NetworkInfrastructureMember', N'Network Infrastructure', N'Network Infrastructure', N'project-diagram', N'FixedAssets',238,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(31, N'BearerPlantsMember', N'Bearer plant', N'Bearer plants', N'holly-berry', N'FixedAssets',240,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(32, N'TangibleExplorationAndEvaluationAssetsMember', N'Tangible exploration and evaluation assets', N'Tangible exploration and evaluation assets', N'download', N'FixedAssets',245,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(33, N'MiningAssetsMember', N'Mining asset', N'Mining assets', N'hammer', N'FixedAssets',250,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(34, N'OilAndGasAssetsMember', N'Oil and gas asset', N'Oil and gas assets', N'gas-pump', N'FixedAssets',255,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(35, N'PowerGeneratingAssetsMember', N'Power Generating Asset', N'Power Generating Assets', N'bolt', N'FixedAssets',256,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(36, N'LeaseholdImprovementsMember', N'Leasehold Improvement', N'Leasehold Improvements', N'paint-roller', N'FixedAssets',257,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(37, N'ConstructionInProgressMember', N'Construction In Progress', N'Construction In Progress', N'drafting-compass', N'FixedAssets',258,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(38, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'Owner-occupied property measured using investment property fair value model', N'Owner-occupied property measured using investment property fair value model', N'campground', N'FixedAssets',260,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(39, N'OtherPropertyPlantAndEquipmentMember', N'Other property, plant and equipment', N'Other property, plant and equipment', N'tags', N'FixedAssets',265,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(40, N'BrandNamesMember', N'Brand name', N'Brand names', N'copyright', N'FixedAssets',270,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(41, N'IntangibleExplorationAndEvaluationAssetsMember', N'Intangible exploration and evaluation asset', N'Intangible exploration and evaluation assets', N'draw-polygon', N'FixedAssets',275,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(42, N'MastheadsAndPublishingTitlesMember', N'Masthead and publishing title', N'Mastheads and publishing titles', N'newspaper', N'FixedAssets',280,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(43, N'ComputerSoftwareMember', N'Computer software', N'Computer software', N'laptop-code', N'FixedAssets',285,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(44, N'LicencesMember', N'Licence', N'Licences', N'', N'FixedAssets',290,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(45, N'GSMLicencesMember', N'GSM licence', N'GSM licences', N'', N'FixedAssets',291,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(46, N'UMTSLicencesMember', N'UMTS licence', N'UMTS licences', N'', N'FixedAssets',292,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(47, N'LTELicencesMember', N'LTE licence', N'LTE licences', N'', N'FixedAssets',293,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(48, N'GamingLicencesMember', N'Gaming licence', N'Gaming licences', N'dragon', N'FixedAssets',294,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(49, N'FranchisesMember', N'Franchise', N'Franchises', N'', N'FixedAssets',295,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(50, N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRights', N'Copyright, patent, industrial property right, service, or operating right', N'Copyrights, patents and other industrial property rights, service and operating rights', N'copyright', N'FixedAssets',296,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(51, N'AirportLandingRightsMember', N'Airport landing right', N'Airport landing rights', N'', N'FixedAssets',297,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(52, N'MiningRightsMember', N'Mining right', N'Mining rights', N'', N'FixedAssets',298,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(53, N'BroadcastingRightsMember', N'Broadcasting right', N'Broadcasting rights', N'', N'FixedAssets',299,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(54, N'ServiceConcessionRightsMember', N'Service concession right', N'Service concession rights', N'', N'FixedAssets',300,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(55, N'RecipesFormulaeModelsDesignsAndPrototypesMember', N'Recipe, formula, model, design or prototype', N'Recipes, formulae, models, designs and prototypes', N'pencil-ruler', N'FixedAssets',301,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(56, N'CustomerrelatedIntangibleAssetsMember', N'Customer-related intangible asset', N'Customer-related intangible assets', N'', N'FixedAssets',302,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(57, N'ValueOfBusinessAcquiredMember', N'Value of business acquired', N'Value of business acquired', N'', N'FixedAssets',303,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(58, N'CapitalisedDevelopmentExpenditureMember', N'Capitalised development expenditure', N'Capitalised development expenditure', N'', N'FixedAssets',304,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(59, N'TechnologybasedIntangibleAssetsMember', N'Technology-based intangible asset', N'Technology-based intangible assets', N'', N'FixedAssets',305,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(60, N'IntangibleAssetsUnderDevelopmentMember', N'Intangible asset under development', N'Intangible assets under development', N'chalkboard-teacher', N'FixedAssets',306,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(61, N'OtherIntangibleAssetsMember', N'Other intangible asset', N'Other intangible assets', N'lightbulb', N'FixedAssets',307,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(62, N'InvestmentPropertyCompletedMember', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets',190,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(63, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'Investment Property (under Construction)', N'Investment Properties (under Construction)', N'store-slash', N'FixedAssets',200,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None');

UPDATE @AgentDefinitions SET 
    [ContactAddressVisibility] = N'None',
    [ContactEmailVisibility] = N'None',
    [ContactMobileVisibility] = N'None',
    [CurrencyVisibility] = N'None',
    [Date1Visibility] = N'None',
    [Date2Visibility] = N'None',
    [Date3Visibility] = N'None',
    [Date4Visibility] = N'None',
    [DateOfBirthVisibility] = N'None',
    [Decimal1Visibility] = N'None',
    [Decimal2Visibility] = N'None',
    [DescriptionVisibility] = N'None',
    [ExternalReferenceVisibility] = N'None',
    [Int1Visibility] = N'None',
    [Int2Visibility] = N'None',
    [Lookup1Visibility] = N'None',
    [Lookup2Visibility] = N'None',
    [Lookup3Visibility] = N'None',
    [Lookup4Visibility] = N'None',
    [Lookup5Visibility] = N'None',
    [Lookup6Visibility] = N'None',
    [Lookup7Visibility] = N'None',
    [Lookup8Visibility] = N'None',
    [Text1Visibility] = N'None',
    [Text2Visibility] = N'None',
    [Text3Visibility] = N'None',
    [Text4Visibility] = N'None',
    [HasAttachments] = 0;

UPDATE @AgentDefinitions
SET
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Market Segment', [Lookup1DefinitionId] = @MarketSegmentLKD
WHERE [Code] IN ( N'Customer')

UPDATE @AgentDefinitions
SET 
	[CurrencyVisibility] = N'Required',
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Bank Account Type', [Lookup1DefinitionId] = @BankAccountTypeLKD,
	[Agent1Label] = N'Bank Branch', [Agent1Label2] = N'الفرع', [Agent1Label3] = N'银行支行'
WHERE [Code] IN ( N'BankAccount')

UPDATE @AgentDefinitions
SET 
	[Agent1Label] = N'Bank', [Agent1Label2] = N'البنك', [Agent1Label3] = N'银行'
WHERE [Code] IN ( N'BankBranch')

UPDATE @AgentDefinitions
SET 
	[Lookup1Visibility]					= N'Optional',
	[Lookup1Label]						= N'Manufacturer',
	[Lookup1DefinitionId]				= @ITEquipmentManufacturerLKD,
	[Lookup2Visibility]					= N'Optional',
	[Lookup2Label]						= N'Operating System',
	[Lookup2DefinitionId]				= @OperatingSystemLKD
WHERE [Code] = N'ComputerEquipmentMember';

UPDATE @AgentDefinitions
SET 
	[Lookup1Visibility] = N'Optional',
	[Lookup1Label] = N'Make',
	[Lookup1DefinitionId] = @VehicleMakeLKD
WHERE [Code] IN (
	'MotorVehiclesMember'
);

INSERT INTO @ValidationErrors
EXEC [api].[AgentDefinitions__Save]
	@Entities = @AgentDefinitions,
	@UserId = @AdminUserId,
    @Culture = @PrimaryLanguageId,
    @NeutralCulture = @PrimaryLanguageId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'AgentDefinitions: Error Provisioning'
	GOTO Err_Label;
END;

INSERT INTO @AgentDefinitionIds([Id], [Index]) SELECT [Id], [Id] FROM dbo.[AgentDefinitions] WHERE [Code] in (
	N'Supplier', N'Customer',  N'Employee', N'Bank', N'BankBranch', N'BankAccount',  N'CashOnHandAccount',
	N'Warehouse', N'TaxDepartment', N'JobOrder', N'Shipment', N'MotorVehiclesMember', N'OfficeEquipmentMember',
	N'ComputerEquipmentMember', N'ConstructionInProgress'
);

EXEC [dal].[AgentDefinitions__UpdateState]
	@Ids = @AgentDefinitionIds,
	@State = N'Visible',
	@UserId = @AdminUserId;

--Declarations
DECLARE @CreditorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Creditor');
DECLARE @DebtorRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Debtor');
DECLARE @OwnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Owner');
DECLARE @PartnerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Partner');
DECLARE @SupplierRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Supplier');
DECLARE @CustomerRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Customer');
DECLARE @EmployeeRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Employee');
DECLARE @FamilyMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'FamilyMember');
DECLARE @BankRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Bank');
DECLARE @BankBranchRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BankBranch');
DECLARE @OtherRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Other');
DECLARE @BankAccountRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BankAccount');
DECLARE @CashOnHandAccountRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'CashOnHandAccount');
DECLARE @WarehouseRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Warehouse');
DECLARE @TaxDepartmentRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'TaxDepartment');
DECLARE @ProductionUnitRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ProductionUnit');
DECLARE @IncomingShipmentRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'IncomingShipment');
DECLARE @FarmRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Farm');
DECLARE @ProspectRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Prospect');
DECLARE @ContactRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Contact');
DECLARE @LandMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'LandMember');
DECLARE @BuildingsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BuildingsMember');
DECLARE @MachineryMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MachineryMember');
DECLARE @ShipsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ShipsMember');
DECLARE @AircraftMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'AircraftMember');
DECLARE @MotorVehiclesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MotorVehiclesMember');
DECLARE @FixturesAndFittingsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'FixturesAndFittingsMember');
DECLARE @OfficeEquipmentMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OfficeEquipmentMember');
DECLARE @ComputerEquipmentMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ComputerEquipmentMember');
DECLARE @CommunicationAndNetworkEquipmentMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'CommunicationAndNetworkEquipmentMember');
DECLARE @NetworkInfrastructureMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'NetworkInfrastructureMember');
DECLARE @BearerPlantsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BearerPlantsMember');
DECLARE @TangibleExplorationAndEvaluationAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'TangibleExplorationAndEvaluationAssetsMember');
DECLARE @MiningAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MiningAssetsMember');
DECLARE @OilAndGasAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OilAndGasAssetsMember');
DECLARE @PowerGeneratingAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'PowerGeneratingAssetsMember');
DECLARE @LeaseholdImprovementsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'LeaseholdImprovementsMember');
DECLARE @ConstructionInProgressMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ConstructionInProgressMember');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember');
DECLARE @OtherPropertyPlantAndEquipmentMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OtherPropertyPlantAndEquipmentMember');
DECLARE @BrandNamesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BrandNamesMember');
DECLARE @IntangibleExplorationAndEvaluationAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'IntangibleExplorationAndEvaluationAssetsMember');
DECLARE @MastheadsAndPublishingTitlesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MastheadsAndPublishingTitlesMember');
DECLARE @ComputerSoftwareMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ComputerSoftwareMember');
DECLARE @LicencesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'LicencesMember');
DECLARE @GSMLicencesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'GSMLicencesMember');
DECLARE @UMTSLicencesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'UMTSLicencesMember');
DECLARE @LTELicencesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'LTELicencesMember');
DECLARE @GamingLicencesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'GamingLicencesMember');
DECLARE @FranchisesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'FranchisesMember');
DECLARE @CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRightsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRights');
DECLARE @AirportLandingRightsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'AirportLandingRightsMember');
DECLARE @MiningRightsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MiningRightsMember');
DECLARE @BroadcastingRightsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BroadcastingRightsMember');
DECLARE @ServiceConcessionRightsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ServiceConcessionRightsMember');
DECLARE @RecipesFormulaeModelsDesignsAndPrototypesMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'RecipesFormulaeModelsDesignsAndPrototypesMember');
DECLARE @CustomerrelatedIntangibleAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'CustomerrelatedIntangibleAssetsMember');
DECLARE @ValueOfBusinessAcquiredMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ValueOfBusinessAcquiredMember');
DECLARE @CapitalisedDevelopmentExpenditureMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'CapitalisedDevelopmentExpenditureMember');
DECLARE @TechnologybasedIntangibleAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'TechnologybasedIntangibleAssetsMember');
DECLARE @IntangibleAssetsUnderDevelopmentMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'IntangibleAssetsUnderDevelopmentMember');
DECLARE @OtherIntangibleAssetsMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OtherIntangibleAssetsMember');
DECLARE @InvestmentPropertyCompletedMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'InvestmentPropertyCompletedMember');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopmentMemberRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'InvestmentPropertyUnderConstructionOrDevelopmentMember');

UPDATE [AgentDefinitions]
SET [Agent1DefinitionId] =
	CASE
		WHEN [Code] IN (N'FamilyMember', N'CashOnHandAccount') THEN @EmployeeRLD
		WHEN [Code] IN (N'BankBranch') THEN @BankRLD
		WHEN [Code] IN (N'BankAccount') THEN @BankBranchRLD
		WHEN [Code] IN (N'Contact') THEN @ProspectRLD
	END;


INSERT INTO @Centers([Index],[ParentIndex],
	[Name],					[Name2],					[Code],[CenterType]) VALUES
(0,NULL,N'Banan',			N'بنان',					N'0',	N'BusinessUnit'),	
(1,0,	N'Executive',		N'التنفيذي',				N'1',	N'Administration'),
(2,0,	N'Marketing & Sales',N'التسويق والمبيعات',		N'2',	N'Sale'),
(3,0,	N'Operations',		N'التشغيل',					N'3',	N'Operation'),
(4,0,	N'Services',		N'الخدمات',					N'9',	N'Service');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@UserId = @AdminUserId;

DECLARE @ExecutiveCtr INT = (SELECT [Id] FROM [Centers] WHERE [Code] = N'0');

DELETE FROM @Agents; DELETE FROM @AgentUsers;
INSERT INTO @Agents
([Index],	[Code], [Name]) VALUES
(0,			N'VAT', N'VAT Department');

EXEC [api].[Agents__Save]
	@DefinitionId = @TaxDepartmentRLD,
	@Entities = @Agents,
	@AgentUsers = @AgentUsers,
	@UserId = @AdminUserId;