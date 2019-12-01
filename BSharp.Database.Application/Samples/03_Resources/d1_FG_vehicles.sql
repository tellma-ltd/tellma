	INSERT INTO dbo.ResourceDefinitions (
	[Id],		[TitlePlural],	[TitleSingular], [Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId]) VALUES
	(N'skds',	N'SKDs',		N'SKD',			N'Required',		N'Body Color',	N'body-colors');
	
DECLARE @SKDs [dbo].ResourceList;

	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'vehicles'
				[Name],		[IsLeaf],	[Node]) VALUES
	(N'skds',	N'Cars',	1,			N'/1/'),
	(N'skds',	N'Sedan',	1,			N'/1/1/'),
	(N'skds',	N'4xDrive',	1,			N'/1/2/'),
	(N'skds',	N'Sports',	1,			N'/1/3/'),
	(N'skds',	N'Trucks',	0,			N'/2/');

	INSERT INTO @SKDs ([Index],
		[ResourceClassificationId],	[DescriptorId],	[Name],									[Description] ) VALUES
		-- N'Vehicles'
	(0, dbo.fn_RCCode__Id(N'Sedan'),N'101',			N'Toyota Camry 2018 Red/White/Leather',	N'Red/White/Leather'),
	(1, dbo.fn_RCCode__Id(N'Sedan'),N'102',			N'Toyota Camry 2018 Black/Black/Wool',	N'Black/Black/Wool'),
	(3, dbo.fn_RCCode__Id(N'Sedan'),N'199',			N'Fake',				NULL),--1
	(4, dbo.fn_RCCode__Id(N'Sedan'),N'201',			N'Toyota Yaris 2018 Red/White/Leather',	N'Red/White/Leather');--1

	EXEC [api].[Resources__Save]
		@DefinitionId = N'skds',
		@Entities = @SKDs,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting SKDs'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT N'skds' AS [Resource Definition]
		DECLARE @SKDIds dbo.IdList;
		INSERT INTO @SKDIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'skds';

		SELECT ResourceClassificationId, [Name] AS 'SKD', [MassUnit] AS 'Weight In', [CountUnit] AS 'Count In'
		FROM rpt.Resources(@SKDIds);
	END