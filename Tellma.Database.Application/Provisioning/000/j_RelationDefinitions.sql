INSERT INTO @RelationDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey], [CenterVisibility], [CurrencyVisibility],[ImageVisibility], [LocationVisibility], [FromDateVisibility], [FromDateLabel], [ToDateVisibility], [ToDateLabel], [TaxIdentificationNumberVisibility],[BankAccountNumberVisibility], [Relation1Visibility], [UserCardinality]) VALUES
(0, N'Creditor', N'Creditor', N'Creditors', N'hands', N'Financials',100,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'Single'),
(1, N'Debtor', N'Debtor', N'Debtors', N'hand-holding-usd', N'Financials',105,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None'),
(2, N'Owner', N'Owner', N'Owners', N'power-off', N'Financials',110,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(3, N'Partner', N'Partner', N'Partners', N'user-tie', N'Financials',115,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(4, N'Supplier', N'Supplier', N'Suppliers', N'user-tag', N'Purchasing',120,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None', N'Single'),
(5, N'Customer', N'Customer', N'Customers ', N'user-shield', N'Sales',125,N'None', N'None', N'None', N'None', N'Optional', N'Customer Since', N'None', N'None', N'Optional', N'None', N'None', N'Single'),
(6, N'Employee', N'Employee', N'Employees', N'user-friends', N'HumanCapital',130,N'None', N'None', N'Optional', N'None', N'Optional', N'Joining Date', N'Optional', N'Termination Date', N'Optional', N'Optional', N'None', N'Single'),
(7, N'FamilyMember', N'Family Member', N'Family Members', N'user-circle', N'HumanCapital',135,N'None', N'None', N'Optional', N'None', N'Optional', N'DOB', N'None', N'None', N'None', N'None', N'Required', N'None'),
(8, N'Bank', N'Bank', N'Banks', N'landmark', N'Cash',140,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(9, N'BankBranch', N'Bank Branch', N'Bank Branches', N'university', N'Cash',145,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(10, N'Other', N'Other', N'Others', N'air-freshener', N'Financials',150,N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(11, N'BankAccount', N'Bank Account', N'Bank Accounts', N'book', N'Cash',155,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'Optional', N'None'),
(12, N'CashOnHandAccount', N'Cash Account', N'Cash On Hand Accounts', N'door-closed', N'Cash',160,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(13, N'Warehouse', N'Warehouse', N'Warehouses', N'warehouse', N'Inventory',165,N'Optional', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(14, N'TaxDepartment', N'Tax Department', N'Tax Departments', N'angry', N'Financials',170,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None', N'None'),
(15, N'JobOrder', N'Job Order', N'Job Orders', N'clipboard-list', N'Production',175,N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(16, N'Shipment', N'Shipment', N'Shipments', N'ship', N'Purchasing',180,N'Required', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(17, N'Farm', N'Farm', N'Farms', N'Industry', N'Production',185,N'Optional', N'None', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(18, N'Prospect', N'Prospect', N'Prospects', N'kiss-wink-heart', N'Marketing',190,N'Required', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(19, N'Contact', N'Contact', N'Contacts', N'user-circle', N'Marketing',195,N'None', N'None', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Required', N'None'),
(20, N'Land', N'Land', N'Land', N'sign', N'FixedAssets',200,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None'),
(21, N'Buildings', N'Buildings', N'Buildings', N'building', N'FixedAssets',205,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(22, N'Machinery', N'Machinery', N'Machinery', N'cogs', N'FixedAssets',210,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(23, N'Ships', N'Ship', N'Ships', N'ship', N'FixedAssets',215,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(24, N'Aircraft', N'Aircraft', N'Aircrafts', N'plane', N'FixedAssets',220,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(25, N'MotorVehicles', N'Motor Vehicle', N'Motor Vehicles', N'car', N'FixedAssets',225,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(26, N'FixturesAndFittings', N'Fixture and fitting', N'Fixtures and fittings', N'puzzle-piece', N'FixedAssets',230,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(27, N'OfficeEquipment', N'Office equipment', N'Office equipment', N'fax', N'FixedAssets',235,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(28, N'BearerPlants', N'Bearer plant', N'Bearer plants', N'holly-berry', N'FixedAssets',240,N'None', N'Required', N'Optional', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(29, N'TangibleExplorationAndEvaluationAssets', N'Tangible exploration and evaluation assets', N'Tangible exploration and evaluation assets', N'download', N'FixedAssets',245,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(30, N'MiningAssets', N'Mining asset', N'Mining assets', N'hammer', N'FixedAssets',250,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(31, N'OilAndGasAssets', N'Oil and gas asset', N'Oil and gas assets', N'gas-pump', N'FixedAssets',255,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(32, N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel', N'Owner-occupied property measured using investment property fair value model', N'Owner-occupied property measured using investment property fair value model', N'campground', N'FixedAssets',260,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(33, N'OtherPropertyPlantAndEquipment', N'Other property, plant and equipment', N'Other property, plant and equipment', N'tags', N'FixedAssets',265,N'None', N'Required', N'Optional', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(34, N'BrandNames', N'Brand name', N'Brand names', N'copyright', N'FixedAssets',270,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(35, N'IntangibleExplorationAndEvaluationAssets', N'Intangible exploration and evaluation asset', N'Intangible exploration and evaluation assets', N'draw-polygon', N'FixedAssets',275,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(36, N'MastheadsAndPublishingTitles', N'Masthead and publishing title', N'Mastheads and publishing titles', N'newspaper', N'FixedAssets',280,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(37, N'ComputerSoftware', N'Computer software', N'Computer software', N'laptop-code', N'FixedAssets',285,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(38, N'LicencesAndFranchises', N'Licence and franchise', N'Licences and franchises', N'file-contract', N'FixedAssets',290,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(39, N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRights', N'Copyright, patent, industrial property right, service, or operating right', N'Copyrights, patents and other industrial property rights, service and operating rights', N'copyright', N'FixedAssets',295,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(40, N'RecipesFormulaeModelsDesignsAndPrototypes', N'Recipe, formula, model, design or prototype', N'Recipes, formulae, models, designs and prototypes', N'pencil-ruler', N'FixedAssets',300,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(41, N'IntangibleAssetsUnderDevelopment', N'Intangible asset under development', N'Intangible assets under development', N'chalkboard-teacher', N'FixedAssets',305,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None'),
(42, N'OtherIntangibleAssets', N'Other intangible asset', N'Other intangible assets', N'lightbulb', N'FixedAssets',310,N'None', N'Required', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'None', N'Optional', N'None');

UPDATE @RelationDefinitions
SET
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Market Segment', [Lookup1DefinitionId] = @MarketSegmentLKD
WHERE [Code] IN ( N'Customer')

UPDATE @RelationDefinitions
SET 
	[CurrencyVisibility] = N'Required',
	[Lookup1Visibility] = N'Optional', [Lookup1Label] = N'Bank Account Type', [Lookup1DefinitionId] = @BankAccountTypeLKD,
	[Relation1Label] = N'Bank Branch', [Relation1Label2] = N'الفرع', [Relation1Label3] = N'银行支行'
WHERE [Code] IN ( N'BankAccount')

UPDATE @RelationDefinitions
SET 
	[Relation1Label] = N'Bank', [Relation1Label2] = N'البنك', [Relation1Label3] = N'银行'
WHERE [Code] IN ( N'BankBranch')

EXEC [api].[RelationDefinitions__Save]
	@Entities = @RelationDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'RelationDefinitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

INSERT INTO @RelationDefinitionIds([Id]) SELECT [Id] FROM dbo.RelationDefinitions;

EXEC [dal].[RelationDefinitions__UpdateState]
	@Ids = @RelationDefinitionIds,
	@State = N'Visible';

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
DECLARE @JobOrderRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'JobOrder');
DECLARE @ShipmentRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Shipment');
DECLARE @FarmRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Farm');
DECLARE @ProspectRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Prospect');
DECLARE @ContactRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Contact');
DECLARE @LandRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Land');
DECLARE @BuildingsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Buildings');
DECLARE @MachineryRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Machinery');
DECLARE @ShipsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Ships');
DECLARE @AircraftRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'Aircraft');
DECLARE @MotorVehiclesRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MotorVehicles');
DECLARE @FixturesAndFittingsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'FixturesAndFittings');
DECLARE @OfficeEquipmentRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OfficeEquipment');
DECLARE @BearerPlantsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BearerPlants');
DECLARE @TangibleExplorationAndEvaluationAssetsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'TangibleExplorationAndEvaluationAssets');
DECLARE @MiningAssetsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MiningAssets');
DECLARE @OilAndGasAssetsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OilAndGasAssets');
DECLARE @OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModelRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OwneroccupiedPropertyMeasuredUsingInvestmentPropertyFairValueModel');
DECLARE @OtherPropertyPlantAndEquipmentRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OtherPropertyPlantAndEquipment');
DECLARE @BrandNamesRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'BrandNames');
DECLARE @IntangibleExplorationAndEvaluationAssetsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'IntangibleExplorationAndEvaluationAssets');
DECLARE @MastheadsAndPublishingTitlesRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'MastheadsAndPublishingTitles');
DECLARE @ComputerSoftwareRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'ComputerSoftware');
DECLARE @LicencesAndFranchisesRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'LicencesAndFranchises');
DECLARE @CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRightsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'CopyrightsPatentsAndOtherIndustrialPropertyRightsServiceAndOperatingRights');
DECLARE @RecipesFormulaeModelsDesignsAndPrototypesRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'RecipesFormulaeModelsDesignsAndPrototypes');
DECLARE @IntangibleAssetsUnderDevelopmentRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'IntangibleAssetsUnderDevelopment');
DECLARE @OtherIntangibleAssetsRLD INT = (SELECT [Id] FROM dbo.[RelationDefinitions] WHERE [Code] = N'OtherIntangibleAssets');

UPDATE RelationDefinitions
SET [Relation1DefinitionId]  = @EmployeeRLD
WHERE [Code] IN (N'FamilyMember', N'CashOnHandAccount');

UPDATE RelationDefinitions
SET [Relation1DefinitionId]  = @BankRLD
WHERE [Code] IN (N'BankBranch');


UPDATE RelationDefinitions
SET [Relation1DefinitionId]  = @BankBranchRLD
WHERE [Code] IN (N'BankAccount');

UPDATE RelationDefinitions
SET [Relation1DefinitionId]  = @ProspectRLD
WHERE [Code] IN (N'Contact');