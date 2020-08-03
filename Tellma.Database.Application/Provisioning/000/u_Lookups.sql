 -- ITEquipmentManufacturer
SET @DefinitionId = @ITEquipmentManufacturerLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name]) VALUES
(0, N'Microsoft', N'Microsoft'),
(1, N'Apple', N'Apple'),
(2, N'HP', N'HP'),
(3, N'IBM', N'IBM'),
(4, N'Lenovo', N'Lenovo'),
(5, N'Dell', N'Dell'),
(6, N'Toshiba', N'Toshiba');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'ITEquipmentManufacturer Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- OperatingSystem
SET @DefinitionId = @OperatingSystemLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name]) VALUES
(201, N'Windows10', N'Windows 10'),
(202, N'Windows8', N'Windows 8'),
(203, N'Windows7', N'Windows 7'),
(204, N'Windows95', N'Windows 95'),
(205, N'WindowsXP', N'Windows XP'),
(206, N'MacOs', N'Mac Os');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'OperatingSystem Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- BodyColor
SET @DefinitionId = @BodyColorLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name]) VALUES
(0, N'White', N'White '),
(1, N'Silver', N'Silver '),
(2, N'Black', N'Black '),
(3, N'DarkBlue', N'Dark Blue'),
(4, N'DarkGray', N'Dark Gray'),
(5, N'Red', N'Red '),
(6, N'DarkGreen', N'Dark Green'),
(7, N'LightBrown', N'Light Brown');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'BodyColor Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- VehicleMake
SET @DefinitionId = @VehicleMakeLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name]) VALUES
(301, N'Toyota', N'Toyota'),
(302, N'Volkswagen', N'Volkswagen'),
(303, N'Ford', N'Ford'),
(304, N'Honda', N'Honda'),
(305, N'Nissan', N'Nissan'),
(306, N'Hyundai', N'Hyundai'),
(307, N'Chevrolet', N'Chevrolet'),
(308, N'Suzuki', N'Suzuki'),
(309, N'Kia', N'Kia'),
(310, N'Mercedes', N'Mercedes'),
(311, N'Renault', N'Renault'),
(312, N'BMW', N'BMW');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'BodyColor Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- SteelThickness
SET @DefinitionId = @SteelThicknessLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],[Code],[Name]) VALUES
(401, N'0.3', N'0.3'),
(402, N'0.4', N'0.4'),
(403, N'0.7', N'0.7'),
(404, N'1.2', N'1.2'),
(405, N'1.4', N'1.4'),
(406, N'1.9', N'1.9');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'SteelThickness Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- PaperOrigin, temporary
SET @DefinitionId = @PaperOriginLKD; DELETE FROM @Lookups;
	INSERT INTO @Lookups([Index],
	[Name],				[Name2]) VALUES
	(0,	N'Thai',		N'تايلاندي'),
	(1,	N'Finnish',		N'فنلندي'),
	(2, N'German',		N'ألماني');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'PaperOrigin Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- PaperGroup, temporary
SET @DefinitionId = @PaperGroupLKD; DELETE FROM @Lookups;
INSERT INTO @Lookups([Index],
	[Name],							[Name2]) VALUES
	(0,	N'Carbonless Coated paper',	N'ورق مكربن صفائح'),
	(1,	N'Newspaper Roll paper',	N'رول ورق جريدة'),
	(2,	N'Coated Paper Glazed',		N'ورق لامع صفائح');
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'PaperGroup Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- PaperType, temporary
SET @DefinitionId = @PaperTypeLKD; DELETE FROM @Lookups;
	INSERT INTO @Lookups([Index],
	[Name],						[Name2]) VALUES
	(0,	N'Commercial',			N'ورق تجاري'),
	(1,	N'Newspaper',			N'ورق جريدة')	;
EXEC [api].Lookups__Save
@DefinitionId = @DefinitionId,
@Entities = @Lookups,
@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'PaperType Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;
-- GrainClassification
SET @DefinitionId = @GrainClassificationLKD; DELETE FROM @Lookups;

-- GrainType
SET @DefinitionId = @GrainTypeLKD; DELETE FROM @Lookups;
-- Quality
SET @DefinitionId = @QualityLKD; DELETE FROM @Lookups;
-- BankAccountType
SET @DefinitionId = @BankAccountTypeLKD; DELETE FROM @Lookups;
-- MarketSegment
SET @DefinitionId = @MarketSegmentLKD; DELETE FROM @Lookups;

-- Declarations
DECLARE @MicrosoftLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Microsoft');
DECLARE @AppleLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Apple');
DECLARE @HPLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'HP');
DECLARE @IBMLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'IBM');
DECLARE @LenovoLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Lenovo');
DECLARE @DellLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Dell');
DECLARE @ToshibaLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Toshiba');

DECLARE @Windows10LKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Windows10');
DECLARE @Windows8LKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Windows8');
DECLARE @Windows7LKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Windows7');
DECLARE @Windows95LKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Windows95');
DECLARE @WindowsXPLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'WindowsXP');
DECLARE @MacOsLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'MacOs');

DECLARE @WhiteLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'White');
DECLARE @SilverLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Silver');
DECLARE @BlackLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Black');
DECLARE @DarkBlueLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'DarkBlue');
DECLARE @DarkGrayLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'DarkGray');
DECLARE @RedLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Red');
DECLARE @DarkGreenLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'DarkGreen');
DECLARE @LightBrownLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'LightBrown');

DECLARE @ToyotaLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Toyota');
DECLARE @VolkswagenLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Volkswagen');
DECLARE @FordLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Ford');
DECLARE @HondaLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Honda');
DECLARE @NissanLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Nissan');
DECLARE @HyundaiLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Hyundai');
DECLARE @ChevroletLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Chevrolet');
DECLARE @SuzukiLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Suzuki');
DECLARE @KiaLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Kia');
DECLARE @MercedesLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Mercedes');
DECLARE @RenaultLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'Renault');
DECLARE @BMWLKP INT = (SELECT [Id] FROM dbo.Roles WHERE [Code] = N'BMW');