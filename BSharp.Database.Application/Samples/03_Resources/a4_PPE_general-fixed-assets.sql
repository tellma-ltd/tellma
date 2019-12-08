	INSERT INTO dbo.ResourceDefinitions (
		[Id],					[TitlePlural],				[TitleSingular],	[IdentifierLabel]) VALUES
	(N'general-fixed-assets',	N'General fixed assets',	N'Geneal fixed asset',	N'Used By');
	UPDATE dbo.ResourceClassifications SET ResourceDefinitionId = N'general-fixed-assets' WHERE [Id] = dbo.fn_RCCode__Id(N'FixturesAndFittings');

	DECLARE @FixedAssets dbo.ResourceList;
	INSERT INTO @FixedAssets ([Index],
		[OperatingSegmentId],	[ResourceClassificationId],		[Name],			[TimeUnitId],				[Identifier]) VALUES
	(0, @OS_IT, dbo.fn_RCCode__Id(N'FixturesAndFittings'),	N'Office Chair',dbo.fn_UnitName__Id(N'Yr'), N'MA'),
	(1, @OS_IT, dbo.fn_RCCode__Id(N'FixturesAndFittings'),	N'Office Chair',dbo.fn_UnitName__Id(N'Yr'), N'AA');

	EXEC [api].[Resources__Save]
		@DefinitionId = N'general-fixed-assets',
		@Entities = @FixedAssets,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets)'
		GOTO Err_Label;
	END;
	IF @DebugResources = 1 
	BEGIN
		SELECT  N'general-fixed-assets' AS [Resource Definition]
		DECLARE @FixedAssetsIds dbo.IdList;
		INSERT INTO @FixedAssetsIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'general-fixed-assets';

		SELECT [Name] AS 'Fixed Asset', [Identifier] AS N'Used By', [TimeUnit] AS 'Usage In', [OperatingSegment] -- Custodian/Location, etc.... from DLE
		FROM rpt.Resources(@FixedAssetsIds);
	END