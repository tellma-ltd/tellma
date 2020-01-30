	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
		[Id],					[TitlePlural],				[TitleSingular],	[IdentifierLabel]) VALUES
	(N'general-fixed-assets',	N'General fixed assets',	N'General fixed asset',	N'Used By');

	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DECLARE @FixedAssets dbo.ResourceList;
	INSERT INTO @FixedAssets ([Index],
		[AccountTypeId],					[Name],			[TimeUnitId],				[Identifier]) VALUES
	(0, dbo.fn_ATCode__Id(N'FixturesAndFittings'),	N'Office Chair',dbo.fn_UnitName__Id(N'Yr'), N'MA'),
	(1, dbo.fn_ATCode__Id(N'FixturesAndFittings'),	N'Office Chair',dbo.fn_UnitName__Id(N'Yr'), N'AA');

	EXEC [api].[Resources__Save]
		@DefinitionId = N'general-fixed-assets',
		@Entities = @FixedAssets,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (fixed-assets): ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;
	IF @DebugResources = 1 
	BEGIN
		SELECT  N'general-fixed-assets' AS [Resource Definition]
		DECLARE @FixedAssetsIds dbo.IdList;
		INSERT INTO @FixedAssetsIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'general-fixed-assets';

		SELECT [Name] AS 'Fixed Asset', [Identifier] AS N'Used By', [TimeUnit] AS 'Usage In' -- Custodian/Location, etc.... from DLE
		FROM rpt.Resources(@FixedAssetsIds);
	END