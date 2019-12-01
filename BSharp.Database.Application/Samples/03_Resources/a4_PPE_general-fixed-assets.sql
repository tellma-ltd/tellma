	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[DescriptorIdLabel]) VALUES
	(N'fixed-assets',	N'Fixed Assets',	N'Fixed Assets',	N'Used By');
	
	DECLARE @FixedAssets dbo.ResourceList;
	INSERT INTO @FixedAssets ([Index],
		[ResourceClassificationId],								[Name],				[TimeUnitId],				[DescriptorId]) VALUES
	(0, dbo.fn_RCCode__Id(N'CommunicationAndNetworkEquipment'),	N'Asus Router',		dbo.fn_UnitName__Id(N'Yr'),	NULL),
	(1, dbo.fn_RCCode__Id(N'OfficeEquipment'),					N'HP Deskjet',		dbo.fn_UnitName__Id(N'Yr'),	NULL),
	(2, dbo.fn_RCCode__Id( N'FixturesAndFittings'),				N'Office Chair',	dbo.fn_UnitName__Id(N'Yr'), N'MA'),
	(3, dbo.fn_RCCode__Id(N'FixturesAndFittings'),				N'Office Chair',	dbo.fn_UnitName__Id(N'Yr'), N'AA');

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

		SELECT [Name] AS 'Fixed Asset', [DescriptorId] AS N'Used By', [TimeUnit] AS 'Usage In' -- Custodian/Location, etc.... from DLE
		FROM rpt.Resources(@FixedAssetsIds);
	END