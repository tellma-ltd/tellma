	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[ResourceTypeParentList],			[DescriptorIdLabel]) VALUES
	(N'fixed-assets',	N'Fixed Assets',	N'Fixed Assets',	N'PropertyPlantAndEquipment',		N'Used By');
	
	DECLARE @FixedAssets dbo.ResourceList;
	INSERT INTO @FixedAssets ([Index],
	[ResourceTypeId],							[Name],				[TimeUnitId],				[DescriptorId]) VALUES
	(0, N'CommunicationAndNetworkEquipment',	N'Asus Router',		dbo.fn_UnitName__Id(N'Yr'),	NULL),
	(1, N'OfficeEquipment',						N'HP Deskjet',		dbo.fn_UnitName__Id(N'Yr'),	NULL),
	(2, N'FixturesAndFittings',					N'Office Chair',	dbo.fn_UnitName__Id(N'Yr'), N'MA'),
	(3, N'FixturesAndFittings',					N'Office Chair',	dbo.fn_UnitName__Id(N'Yr'), N'AA');

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

		SELECT ResourceTypeId, [Name] AS 'Fixed Asset', [DescriptorId] AS N'Used By', [TimeUnit] AS 'Usage In' -- Custodian/Location, etc.... from DLE
		FROM rpt.Resources(@FixedAssetsIds);
	END