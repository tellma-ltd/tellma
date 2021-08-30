INSERT INTO @LookupDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ITEquipmentManufacturer', N'IT Manufacturer', N'IT Manufacturers', N'microchip', N'Administration',1),
(1, N'OperatingSystem', N'Operating System', N'Operating Systems', N'laptop-code', N'Administration',2),
(2, N'VehicleMake', N'Vehicle Make', N'Vehicle Makes', N'car', N'Administration',4),
(3, N'BankAccountType', N'Bank Account Type', N'Bank Account Types', N'ellipsis-h', N'Administration',12),
(5, N'MarketSegment', N'Market Segment', N'Market Segments', N'search-dollar', N'Administration',13),
(6, N'LoanType', N'Loan Type', N'Loan Types', N'clipboard-list', N'HumanCapital',35),
(7, N'Citizenship', N'Citizenship', N'Citizenships', N'globe', N'Administration',14);


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