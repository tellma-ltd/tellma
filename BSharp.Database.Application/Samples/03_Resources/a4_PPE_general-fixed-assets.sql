	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[ResourceTypeParentList]) VALUES
	(N'fixed-assets',	N'Fixed Assets',	N'Fixed Assets',	N'PropertyPlantAndEquipment');
	
	DECLARE @FixedAssets dbo.ResourceList;
	INSERT INTO @FixedAssets ([Index],
	[ResourceTypeId],							[Name],				[TimeUnitId]) VALUES
	(0, N'CommunicationAndNetworkEquipment',	N'Asus Router',		dbo.fn_UnitName__Id(N'Yr')),
	(1, N'OfficeEquipment',						N'HP Deskjet',		dbo.fn_UnitName__Id(N'Yr')),
	(2, N'FixturesAndFittings',					N'Office Chair MA',	dbo.fn_UnitName__Id(N'Yr')),
	(3, N'FixturesAndFittings',					N'Office Chair AA',	dbo.fn_UnitName__Id(N'Yr'));

	EXEC [api].[Resources__Save]
		@DefinitionId = N'fixed-assets',
		@Entities = @FixedAssets,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets)'
		GOTO Err_Label;
	END;
	IF @DebugResources = 1 
	BEGIN
		SELECT  N'fixed-assets' AS [Resource Definition]
		DECLARE @FixedAssetsIds dbo.IdList;
		INSERT INTO @FixedAssetsIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'fixed-assets';

		SELECT ResourceTypeId, [Name] AS 'Fixed Asset', [TimeUnit] AS 'Usage In' -- Custodian/Location, etc.... from DLE
		FROM rpt.Resources(@FixedAssetsIds);
	END