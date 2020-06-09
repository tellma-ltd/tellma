INSERT INTO @LookupDefinitions([Index], [Code], [TitleSingular], [TitlePlural], [MainMenuIcon], [MainMenuSection], [MainMenuSortKey]) VALUES
(0, N'it-equipment-manufacturers', N'IT Manufacturer', N'IT Manufacturers', N'microchip', N'Administration',60),
(1, N'operating-systems', N'Operating System', N'Operating Systems', N'laptop-code', N'Administration',60),
(2, N'body-colors', N'Body Color', N'Body Colors', N'palette', N'Administration',60),
(3, N'vehicle-makes', N'Vehicle Make', N'Vehicle Makes', N'car', N'Administration',70),
(4, N'steel-thicknesses', N'Thickness', N'Thicknesses', N'ruler', N'Administration',40),
(5, N'paper-origins', N'Paper Origin', N'Paper Origins', N'map', N'Administration',50),
(6, N'paper-groups', N'Paper Group', N'Paper Groups', N'copy', N'Administration',80),
(7, N'paper-types', N'Paper Type', N'Paper Types', N'scroll', N'Administration',90);

EXEC api.LookupDefinitions__Save
	@Entities = @LookupDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Lookup Definitions: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;

--Declarations
DECLARE @it_equipment_manufacturersLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'it-equipment-manufacturers');
DECLARE @operating_systemsLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'operating-systems');
DECLARE @body_colorsLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'body-colors');
DECLARE @vehicle_makesLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'vehicle-makes');
DECLARE @steel_thicknessesLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'steel-thicknesses');
DECLARE @paper_originsLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'paper-origins');
DECLARE @paper_groupsLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'paper-groups');
DECLARE @paper_typesLKD INT = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'paper-types');