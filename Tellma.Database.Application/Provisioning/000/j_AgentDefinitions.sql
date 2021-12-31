INSERT INTO @AgentDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey], [CenterVisibility], [CurrencyVisibility],[ImageVisibility], [LocationVisibility], [FromDateVisibility], [FromDateLabel], [ToDateVisibility], [ToDateLabel], [TaxIdentificationNumberVisibility],[BankAccountNumberVisibility], [Agent1Visibility], [UserCardinality]) VALUES
(0, N'Creditor', N'Creditor', N'Creditors', N'hands', N'Financials',100,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'Single'),
(1, N'Debtor', N'Debtor', N'Debtors', N'hand-holding-usd', N'Financials',105,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None'),
(2, N'Owner', N'Owner', N'Owners', N'power-off', N'Financials',110,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(3, N'Partner', N'Partner', N'Partners', N'user-tie', N'Financials',115,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(4, N'Supplier', N'Supplier', N'Suppliers', N'user-tag', N'Purchasing',120,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(5, N'Customer', N'Customer', N'Customers ', N'user-shield', N'Sales',125,N'None', N'None', N'None', N'None', N'Optional', N'Customer Since', N'None', N'None', N'Optional', N'None', N'None', N'Single'),
(6, N'Employee', N'Employee', N'Employees', N'user-friends', N'HumanCapital',130,N'None', N'None', N'Optional', N'None', N'Optional', N'Joining Date', N'None', N'None', N'Optional', N'Optional', N'Optional', N'Single'),
(7, N'PurchaseInvoice', N'Purchase Invoice', N'Purchase Invoices', N'user-tag', N'Purchasing',120,N'Required', N'Required', N'None', N'None', N'Optional', N'Opening Date', N'Optional', N'Closing Date', N'None', N'None', N'Required', N'Single'),
(8, N'SalesInvoice', N'Sales Invoice', N'Sales Invoices', N'user-shield', N'Sales',125,N'Required', N'Required', N'None', N'None', N'Optional', N'Opening Date', N'Optional', N'Closing Date', N'None', N'None', N'Required', N'Single'),
(9, N'EmployeeLoan', N'Employee Loan', N'Employees Loans', N'user-friends', N'HumanCapital',130,N'Required', N'Required', N'None', N'None', N'Optional', N'Opening Date', N'Optional', N'Closing Date', N'None', N'None', N'Required', N'Single'),
(10, N'FamilyMember', N'Family Member', N'Family Members', N'user-circle', N'HumanCapital',135,N'None', N'None', N'Optional', N'None', N'Optional', N'DOB', N'None', N'None', N'None', N'None', N'Required', N'None'),
(11, N'Bank', N'Bank', N'Banks', N'landmark', N'Cash',140,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(12, N'Other', N'Other', N'Others', N'air-freshener', N'Financials',150,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(13, N'BankAccount', N'Bank Account', N'Bank Accounts', N'book', N'Cash',155,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None'),
(14, N'CashOnHandAccount', N'Cash Account', N'Cash On Hand Accounts', N'door-closed', N'Cash',160,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(15, N'BankBorrowingAccount', N'Bank Borrowing Account', N'Bank Borrowing Accounts', N'hand-holding-usd', N'Cash',165,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None'),
(16, N'Warehouse', N'Warehouse', N'Warehouses', N'warehouse', N'Inventory',20,N'Required', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(17, N'AisleBinShelf', N'Aisle/Bin/Shelf', N'Aisles/Bins/Shelves', N'th-large', N'Inventory',30,N'Required', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(18, N'TaxDepartment', N'Tax Department', N'Tax Departments', N'angry', N'Financials',160,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None'),
(19, N'ProductionOrder', N'Production Order', N'Production Orders', N'clipboard-list', N'Production',175,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(20, N'IncomingShipment', N'Incoming Shipment', N'Incoming Shipments', N'ship', N'Purchasing',180,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(21, N'Farm', N'Farm', N'Farms', N'industry', N'Production',185,N'Optional', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(22, N'Prospect', N'Prospect', N'Prospects', N'kiss-wink-heart', N'Marketing',190,N'Required', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(23, N'Contact', N'Contact', N'Contacts', N'user-circle', N'Marketing',195,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(24, N'LandMember', N'Land', N'Land', N'sign', N'FixedAssets',200,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(25, N'BuildingsMember', N'Buildings', N'Buildings', N'building', N'FixedAssets',205,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(26, N'MachineryMember', N'Machinery', N'Machinery', N'cogs', N'FixedAssets',210,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(27, N'ShipsMember', N'Ship', N'Ships', N'ship', N'FixedAssets',215,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(28, N'AircraftMember', N'Aircraft', N'Aircrafts', N'plane', N'FixedAssets',220,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(29, N'MotorVehiclesMember', N'Motor Vehicle', N'Motor Vehicles', N'car', N'FixedAssets',225,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(30, N'FixturesAndFittingsMember', N'Fixture and fitting', N'Fixtures and fittings', N'puzzle-piece', N'FixedAssets',230,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(31, N'OfficeEquipmentMember', N'Office equipment', N'Office equipment', N'fax', N'FixedAssets',235,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(32, N'ComputerEquipmentMember', N'Computer Equipment', N'Computer Equipment', N'laptop', N'FixedAssets',240,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(33, N'CommunicationAndNetworkEquipmentMember', N'Comm. Network Equipment', N'Comm. Network Equipment', N'broadcast-tower', N'FixedAssets',245,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(34, N'NetworkInfrastructureMember', N'Network Infrastructure', N'Network Infrastructures', N'project-diagram', N'FixedAssets',250,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(35, N'BearerPlantsMember', N'Bearer plant', N'Bearer plants', N'holly-berry', N'FixedAssets',255,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(36, N'TangibleExplorationAndEvaluationAssetsMember', N'Tangible exploration and evaluation assets', N'Tangible exploration and evaluation assets', N'download', N'FixedAssets',260,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(37, N'MiningAssetsMember', N'Mining asset', N'Mining assets', N'hammer', N'FixedAssets',265,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(38, N'OilAndGasAssetsMember', N'Oil and gas asset', N'Oil and gas assets', N'gas-pump', N'FixedAssets',270,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(39, N'PowerGeneratingAssetsMember', N'Power Generating Asset', N'Power Generating Assets', N'bolt', N'FixedAssets',275,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(40, N'LeaseholdImprovementsMember', N'Leasehold Improvement', N'Leasehold Improvements', N'paint-roller', N'FixedAssets',280,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(41, N'ConstructionInProgressMember', N'Construction In Progress', N'Construction In Progress', N'drafting-compass', N'FixedAssets',285,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(42, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember', N'Owner-occupied property measured using investment property fair value model', N'Owner-occupied property measured using investment property fair value model', N'campground', N'FixedAssets',290,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(43, N'OtherPropertyPlantAndEquipmentMember', N'Other property, plant and equipment', N'Other property, plant and equipment', N'tags', N'FixedAssets',295,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(44, N'BrandNamesMember', N'Brand name', N'Brand names', N'trademark', N'FixedAssets',300,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(45, N'IntangibleExplorationAndEvaluationAssetsMember', N'Intangible exploration and evaluation asset', N'Intangible exploration and evaluation assets', N'draw-polygon', N'FixedAssets',305,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(46, N'MastheadsAndPublishingTitlesMember', N'Masthead and publishing title', N'Mastheads and publishing titles', N'newspaper', N'FixedAssets',310,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(47, N'ComputerSoftwareMember', N'Computer software', N'Computer software', N'laptop-code', N'FixedAssets',315,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(48, N'LicencesMember', N'Licence', N'Licences', N'id-badge', N'FixedAssets',320,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(49, N'FranchisesMember', N'Franchise', N'Franchises', N'industry', N'FixedAssets',345,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(50, N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRightsMember', N'Copyright, patent, industrial property right, service, or operating right', N'Copyrights, patents and other industrial property rights, service and operating rights', N'copyright', N'FixedAssets',350,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(51, N'AirportLandingRightsMember', N'Airport landing right', N'Airport landing rights', N'plane-arrival', N'FixedAssets',355,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(52, N'MiningRightsMember', N'Mining right', N'Mining rights', N'mountain', N'FixedAssets',360,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(53, N'BroadcastingRightsMember', N'Broadcasting right', N'Broadcasting rights', N'broadcast-tower', N'FixedAssets',365,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(54, N'ServiceConcessionRightsMember', N'Service concession right', N'Service concession rights', N'snowplow', N'FixedAssets',370,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(55, N'RecipesFormulaeModelsDesignsAndPrototypesMember', N'Recipe, formula, model, design or prototype', N'Recipes, formulae, models, designs or prototypes', N'pencil-ruler', N'FixedAssets',375,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(56, N'CustomerrelatedIntangibleAssetsMember', N'Customer-related intangible asset', N'Customer-related intangible assets', N'receipt', N'FixedAssets',380,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(57, N'ValueOfBusinessAcquiredMember', N'Value of business acquired', N'Value of business acquired', N'gifts', N'FixedAssets',385,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(58, N'CapitalisedDevelopmentExpenditureMember', N'Capitalised development expenditure', N'Capitalised development expenditure', N'', N'FixedAssets',390,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(59, N'TechnologybasedIntangibleAssetsMember', N'Technology-based intangible asset', N'Technology-based intangible assets', N'microchip', N'FixedAssets',395,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(60, N'IntangibleAssetsUnderDevelopmentMember', N'Intangible asset under development', N'Intangible assets under development', N'chalkboard-teacher', N'FixedAssets',400,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(61, N'OtherIntangibleAssetsMember', N'Other intangible asset', N'Other intangible assets', N'lightbulb', N'FixedAssets',405,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(62, N'InvestmentPropertyCompletedMember', N'Investment Property', N'Investment Properties', N'city', N'FixedAssets',410,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(63, N'InvestmentPropertyUnderConstructionOrDevelopmentMember', N'Investment Property (under Construction)', N'Investment Properties (under Construction)', N'store-slash', N'FixedAssets',415,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(64, N'Subsidiary', N'Subsidiary', N'Subsidiaries', N'sitemap', N'Financials',170,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None', N'None'),
(65, N'JointVenture', N'Joint Venture', N'Joint Ventures', N'handshake', N'Financials',180,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(66, N'Associate', N'Associate', N'Associates', N'user-tie', N'Financials',190,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None', N'None'),
(67, N'University', N'University', N'Universities', N'university', N'HumanCapital',50,N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None', N'None'),
(68, N'Project', N'Project', N'Projects', N'project-diagram', N'Production',60,N'None', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None', N'None');
UPDATE @AgentDefinitions SET 
    [ContactAddressVisibility] = N'None',
    [ContactEmailVisibility] = N'None',
    [ContactMobileVisibility] = N'None',
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
	[Agent1Visibility] = N'None',
    [HasAttachments] = 0;

UPDATE @AgentDefinitions
SET
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Market Segment', [Lookup1DefinitionId] = @MarketSegmentLKD
WHERE [Code] IN ( N'Customer')

UPDATE @AgentDefinitions
SET 
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Bank Account Type', [Lookup1DefinitionId] = @BankAccountTypeLKD,
	[Agent1Label] = N'Bank', [Agent1Label2] = N'البنك', [Agent1Label3] = N'银行支行'
WHERE [Code] IN ( N'BankAccount')

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

UPDATE @AgentDefinitions
SET
	[Agent1DefinitionIndex] = (SELECT [Index] FROM @AgentDefinitions WHERE [Code] = N'Employee'), 
	[Agent1Label] = N'Direct Supervisor',
	[Agent1Label2] = N'المسؤول المباشر',
	[Agent1Label3] = N'直接主管'
WHERE [Code] = N'Employee'
-- Lookup1: Gender, Lookup2: Religion, Lookup3: Bloodtype

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

INSERT INTO @AgentDefinitionIds([Id], [Index]) SELECT [Id], [Id] FROM dbo.[AgentDefinitions]
--WHERE [Code] in (
--	N'Supplier', N'Customer',  N'Employee', N'Bank', N'BankAccount',  N'CashOnHandAccount',
--	N'Warehouse', N'TaxDepartment', N'JobOrder', N'Shipment', N'MotorVehiclesMember', N'OfficeEquipmentMember',
--	N'ComputerEquipmentMember', N'ConstructionInProgress',
--	N'Land'
--);

EXEC [dal].[AgentDefinitions__UpdateState]
	@Ids = @AgentDefinitionIds,
	@State = N'Visible',
	@UserId = @AdminUserId;

-- Declarations
DECLARE @CreditorAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Creditor');
DECLARE @DebtorAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Debtor');
DECLARE @OwnerAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Owner');
DECLARE @PartnerAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Partner');
DECLARE @SupplierAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Supplier');
DECLARE @CustomerAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Customer');
DECLARE @EmployeeAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Employee');
DECLARE @PurchaseInvoiceAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'PurchaseInvoice');
DECLARE @SalesInvoiceAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'SalesInvoice');
DECLARE @EmployeeLoanAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'EmployeeLoan');
DECLARE @FamilyMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'FamilyMember');
DECLARE @BankAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Bank');
DECLARE @OtherAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Other');
DECLARE @BankAccountAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'BankAccount');
DECLARE @CashOnHandAccountAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'CashOnHandAccount');
DECLARE @BankBorrowingAccountAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'BankBorrowingAccount');
DECLARE @WarehouseAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Warehouse');
DECLARE @AisleBinShelfAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'AisleBinShelf');
DECLARE @TaxDepartmentAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'TaxDepartment');
DECLARE @ProductionOrderAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ProductionOrder');
DECLARE @IncomingShipmentAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'IncomingShipment');
DECLARE @FarmAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Farm');
DECLARE @ProspectAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Prospect');
DECLARE @ContactAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Contact');
DECLARE @LandMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'LandMember');
DECLARE @BuildingsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'BuildingsMember');
DECLARE @MachineryMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'MachineryMember');
DECLARE @ShipsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ShipsMember');
DECLARE @AircraftMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'AircraftMember');
DECLARE @MotorVehiclesMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'MotorVehiclesMember');
DECLARE @FixturesAndFittingsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'FixturesAndFittingsMember');
DECLARE @OfficeEquipmentMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'OfficeEquipmentMember');
DECLARE @ComputerEquipmentMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ComputerEquipmentMember');
DECLARE @CommunicationAndNetworkEquipmentMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'CommunicationAndNetworkEquipmentMember');
DECLARE @NetworkInfrastructureMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'NetworkInfrastructureMember');
DECLARE @BearerPlantsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'BearerPlantsMember');
DECLARE @TangibleExplorationAndEvaluationAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'TangibleExplorationAndEvaluationAssetsMember');
DECLARE @MiningAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'MiningAssetsMember');
DECLARE @OilAndGasAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'OilAndGasAssetsMember');
DECLARE @PowerGeneratingAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'PowerGeneratingAssetsMember');
DECLARE @LeaseholdImprovementsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'LeaseholdImprovementsMember');
DECLARE @ConstructionInProgressMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ConstructionInProgressMember');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelMember');
DECLARE @OtherPropertyPlantAndEquipmentMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'OtherPropertyPlantAndEquipmentMember');
DECLARE @BrandNamesMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'BrandNamesMember');
DECLARE @IntangibleExplorationAndEvaluationAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'IntangibleExplorationAndEvaluationAssetsMember');
DECLARE @MastheadsAndPublishingTitlesMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'MastheadsAndPublishingTitlesMember');
DECLARE @ComputerSoftwareMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ComputerSoftwareMember');
DECLARE @LicencesMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'LicencesMember');
DECLARE @FranchisesMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'FranchisesMember');
DECLARE @CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRightsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRightsMember');
DECLARE @AirportLandingRightsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'AirportLandingRightsMember');
DECLARE @MiningRightsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'MiningRightsMember');
DECLARE @BroadcastingRightsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'BroadcastingRightsMember');
DECLARE @ServiceConcessionRightsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ServiceConcessionRightsMember');
DECLARE @RecipesFormulaeModelsDesignsAndPrototypesMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'RecipesFormulaeModelsDesignsAndPrototypesMember');
DECLARE @CustomerrelatedIntangibleAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'CustomerrelatedIntangibleAssetsMember');
DECLARE @ValueOfBusinessAcquiredMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'ValueOfBusinessAcquiredMember');
DECLARE @CapitalisedDevelopmentExpenditureMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'CapitalisedDevelopmentExpenditureMember');
DECLARE @TechnologybasedIntangibleAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'TechnologybasedIntangibleAssetsMember');
DECLARE @IntangibleAssetsUnderDevelopmentMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'IntangibleAssetsUnderDevelopmentMember');
DECLARE @OtherIntangibleAssetsMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'OtherIntangibleAssetsMember');
DECLARE @InvestmentPropertyCompletedMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'InvestmentPropertyCompletedMember');
DECLARE @InvestmentPropertyUnderConstructionOrDevelopmentMemberAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'InvestmentPropertyUnderConstructionOrDevelopmentMember');
DECLARE @SubsidiaryAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Subsidiary');
DECLARE @JointVentureAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'JointVenture');
DECLARE @AssociateAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Associate');
DECLARE @UniversityAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'University');
DECLARE @ProjectAD INT = (SELECT [Id] FROM dbo.[AgentDefinitions] WHERE [Code] = N'Project');

UPDATE [AgentDefinitions]
SET [Agent1DefinitionId] =
	CASE
		WHEN [Code] IN (
			N'FamilyMember',
			N'CashOnHandAccount') THEN @EmployeeAD
		WHEN [Code] IN (N'BankAccount') THEN @BankAD
		WHEN [Code] IN (N'Contact') THEN @ProspectAD
		WHEN [Code] IN (
			N'MachineryMember',
			N'MotorVehiclesMember',
			N'OfficeEquipmentMember',
			N'ComputerEquipmentMember',
			N'CommunicationAndNetworkEquipmentMember',
			N'NetworkInfrastructureMember',
			N'PowerGeneratingAssetsMember',
			N'ComputerSoftwareMember'	
		) THEN @SupplierAD
	END;




INSERT INTO @Centers([Index],[ParentIndex],
	[Name],					[Name2],					[Code],[CenterType]) VALUES
(0,NULL,N'ACME',			N'أكمي',					N'0',	N'BusinessUnit'),	
(1,0,	N'Executive',		N'التنفيذي',				N'1',	N'Administration'),
(2,0,	N'Marketing & Sales',N'التسويق والمبيعات',		N'2',	N'Sale'),
(3,0,	N'Operations',		N'التشغيل',					N'3',	N'Operation'),
(4,0,	N'Services',		N'الخدمات',					N'9',	N'Service');

EXEC [api].[Centers__Save]
	@Entities = @Centers,
	@UserId = @AdminUserId;

DELETE FROM @Agents; DELETE FROM @AgentUsers;
INSERT INTO @Agents
([Index],	[Code], [Name]) VALUES
(0,			N'VAT', N'VAT Department');

EXEC [api].[Agents__Save]
	@DefinitionId = @TaxDepartmentAD,
	@Entities = @Agents,
	@AgentUsers = @AgentUsers,
	@UserId = @AdminUserId;