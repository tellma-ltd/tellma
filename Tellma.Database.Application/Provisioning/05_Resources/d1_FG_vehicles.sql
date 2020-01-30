IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	DELETE FROM @ResourceDefinitions;
	INSERT INTO @ResourceDefinitions (
	[Id],		[TitlePlural],	[TitleSingular], [Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId], [IdentifierVisibility]) VALUES
	(N'skds',	N'SKDs',		N'SKD',			N'Required',			N'Body Color',	N'body-colors',		N'Required');
	
	EXEC [api].[ResourceDefinitions__Save]
	@Entities = @ResourceDefinitions,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Resource Definitions: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;		

	DECLARE @FGVehiclesDescendantsTemp TABLE ([Code] NVARCHAR(255), [Name] NVARCHAR(255), [Node] HIERARCHYID, [IsAssignable] BIT DEFAULT 1, [Index] INT)

	INSERT INTO @FGVehiclesDescendantsTemp ([Index], -- N'vehicles'
	[Code],						[Name],		[Node]) VALUES
--	N'FinishedGoods',					N'/1/11/5/'
	(0,N'FGCarsExtension',		N'Cars',	N'/1/11/5/1/'),
	(1,N'FGSedanExtension',		N'Sedan',	N'/1/11/5/1/1/'),
	(2,N'FG4xDriveExtension',	N'4xDrive',	N'/1/11/5/1/2/'),
	(3,N'FGSportsExtension',	N'Sports',	N'/1/11/5/1/3/'),
	(4,N'FGTrucksExtension',	N'Trucks',	N'/1/11/5/2/');

	UPDATE @FGVehiclesDescendantsTemp SET IsAssignable = 0 WHERE [Index] = 0;

	DECLARE @FGVehiclesDescendants AccountTypeList;

	INSERT INTO @FGVehiclesDescendants ([Code], [Name], [ParentIndex], [IsAssignable], [Index])
	SELECT [Code], [Name], (SELECT [Index] FROM @FGVehiclesDescendantsTemp WHERE [Node] = RC.[Node].GetAncestor(1)) AS ParentIndex, [IsAssignable], [Index]
	FROM @FGVehiclesDescendantsTemp RC

	EXEC [api].[AccountTypes__Save]
	@Entities = @FGVehiclesDescendants,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'FG: Vehicles: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;	

	DECLARE @SKDs [dbo].ResourceList;
	INSERT INTO @SKDs ([Index], 
		[AccountTypeId],				[Identifier],	[Name],											[Lookup1Id]) VALUES
		-- N'Vehicles'
	(0, dbo.fn_ATCode__Id(N'FGCarsExtension'),	N'101',			N'Toyota Camry 2018 Navy Blue/White/Leather',	dbo.fn_Lookup(N'body-colors', N'Navy Blue')),
	(1, dbo.fn_ATCode__Id(N'FGCarsExtension'),	N'102',			N'Toyota Camry 2018 Black/Silver/Wool',			dbo.fn_Lookup(N'body-colors', N'Black')),
	(2, dbo.fn_ATCode__Id(N'FGSedanExtension'),N'199',			N'Fake',										NULL),--1
	(3, dbo.fn_ATCode__Id(N'FGSedanExtension'),N'201',			N'Toyota Yaris 2018 White/White/Leather',		dbo.fn_Lookup(N'body-colors', N'White'));--1

	EXEC [api].[Resources__Save]
		@DefinitionId = N'skds',
		@Entities = @SKDs,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting SKDs: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT N'skds' AS [Resource Definition]
		DECLARE @SKDIds dbo.IdList;
		INSERT INTO @SKDIds SELECT [Id] FROM dbo.Resources WHERE [DefinitionId] = N'skds';

		SELECT [Classification], [Name] AS 'SKD', Lookup1 AS 'Body Color'
		FROM rpt.Resources(@SKDIds);
	END
END