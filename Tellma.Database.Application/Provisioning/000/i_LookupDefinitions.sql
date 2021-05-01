INSERT INTO @LookupDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'ITEquipmentManufacturer', N'IT Manufacturer', N'IT Manufacturers', N'microchip', N'Administration',1),
(1, N'OperatingSystem', N'Operating System', N'Operating Systems', N'laptop-code', N'Administration',2),
(2, N'VehicleMake', N'Vehicle Make', N'Vehicle Makes', N'car', N'Administration',4),
(3, N'BankAccountType', N'Bank Account Type', N'Bank Account Types', N'ellipsis-h', N'Administration',12),
(4, N'Bank', N'Bank', N'Banks', N'university', N'Administration',12),
(5, N'MarketSegment', N'Market Segment', N'Market Segments', N'search-dollar', N'Administration',13),
(6, N'LoanType', N'Loan Type', N'Loan Types', N'clipboard-list', N'HumanCapital',35),
(7, N'Citizenship', N'Citizenship', N'Citizenships', N'globe', N'Administration',14);

EXEC api.LookupDefinitions__Save
	@Entities = @LookupDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Lookup Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

INSERT INTO @LookupDefinitionIds([Id]) SELECT [Id] FROM dbo.LookupDefinitions;

EXEC [dal].[LookupDefinitions__UpdateState]
	@Ids = @LookupDefinitionIds,
	@State = N'Visible';


--Declarations
DECLARE @ITEquipmentManufacturerLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'ITEquipmentManufacturer');
DECLARE @OperatingSystemLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'OperatingSystem');
DECLARE @VehicleMakeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'VehicleMake');
DECLARE @BankAccountTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'BankAccountType');
DECLARE @BankLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Bank');
DECLARE @MarketSegmentLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'MarketSegment');
DECLARE @LoanTypeLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'LoanType');
DECLARE @CitizenshipLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'Citizenship');