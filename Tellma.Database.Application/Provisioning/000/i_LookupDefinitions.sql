INSERT INTO @LookupDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ITEquipmentManufacturer', N'IT Manufacturer', N'IT Manufacturers', N'microchip', N'Administration',1),
(1, N'OperatingSystem', N'Operating System', N'Operating Systems', N'laptop-code', N'Administration',2),
(2, N'BodyColor', N'Body Color', N'Body Colors', N'palette', N'Inventory',3),
(3, N'VehicleMake', N'Vehicle Make', N'Vehicle Makes', N'car', N'Administration',4),
(4, N'SteelThickness', N'Thickness', N'Thicknesses', N'ruler', N'Inventory',5),
(5, N'PaperOrigin', N'Paper Origin', N'Paper Origins', N'map', N'Administration',6),
(6, N'PaperGroup', N'Paper Group', N'Paper Groups', N'copy', N'Administration',7),
(7, N'PaperType', N'Paper Type', N'Paper Types', N'scroll', N'Administration',8),
(8, N'GrainClassification', N'Grain Group', N'Grain Groups', N'tree', N'Inventory',9),
(9, N'GrainType', N'Grain Type', N'Grain Types', N'seedling', N'Inventory',10),
(10, N'Quality', N'Quality Level', N'Quality Levels', N'certificate', N'Inventory',11),
(11, N'BankAccountType', N'Bank Account Type', N'Bank Account Types', N'ellipsis-h', N'Cash',12),
(13, N'MarketSegment', N'Market Segment', N'Market Segments', N'search-dollar', N'Sales',13),
(14, N'LoanType', N'Loan Type', N'Loan Types', N'clipboard-list', N'HumanCapital',35),
(15, N'Citizenship', N'Citizenship', N'Citizenships', N'globe', N'HumanCapital',14),
(16, N'Gender', N'Gender', N'Genders', N'venus-mars', N'HumanCapital',15),
(17, N'Religion', N'Religion', N'Religions', N'moon', N'HumanCapital',16),
(18, N'BloodType', N'Blood Type', N'Blood Types', N'vial', N'HumanCapital',17),
(19, N'DegreeType', N'Degree Type', N'Degree Types', N'graduation-cap', N'HumanCapital',18),
(20, N'Specialization', N'Specialization', N'Specializations', N'user-md', N'HumanCapital',19),
(21, N'DisabilityType', N'Disability Type', N'Disability Types', N'blind', N'HumanCapital',20);


-- INSERT INTO @ValidationErrors
INSERT INTO @ValidationErrors
EXEC [api].[LookupDefinitions__Save]
	@Entities = @LookupDefinitions,
	@ReturnIds = 0,
	@UserId = @AdminUserId;
	
IF EXISTS (SELECT [Key] FROM @ValidationErrors)
BEGIN
	Print 'LookupDefinitions: Error Provisioning'
	GOTO Err_Label;
END;

EXEC [dal].[LookupDefinitions__UpdateState]
	@Ids = @LookupDefinitionIds,
	@State = N'Visible',
	@UserId = @AdminUserId;

--Declarations
--Declarations
DECLARE @ITEquipmentManufacturerLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'ITEquipmentManufacturer');
DECLARE @OperatingSystemLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'OperatingSystem');
DECLARE @BodyColorLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'BodyColor');
DECLARE @VehicleMakeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'VehicleMake');
DECLARE @SteelThicknessLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'SteelThickness');
DECLARE @PaperOriginLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'PaperOrigin');
DECLARE @PaperGroupLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'PaperGroup');
DECLARE @PaperTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'PaperType');
DECLARE @GrainClassificationLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'GrainClassification');
DECLARE @GrainTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'GrainType');
DECLARE @QualityLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Quality');
DECLARE @BankAccountTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'BankAccountType');
DECLARE @MarketSegmentLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'MarketSegment');
DECLARE @LoanTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'LoanType');
DECLARE @CitizenshipLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Citizenship');
DECLARE @GenderLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Gender');
DECLARE @ReligionLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Religion');
DECLARE @BloodTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'BloodType');
DECLARE @DegreeTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'DegreeType');
DECLARE @SpecializationLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Specialization');
DECLARE @DisabilityTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'DisabilityType');