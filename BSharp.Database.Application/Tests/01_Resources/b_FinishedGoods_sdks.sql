	INSERT INTO dbo.ResourceDefinitions (
	[Id],		[TitlePlural],	[TitleSingular],[ResourceTypeId], [Lookup1Visibility], [Lookup1Lable], [Lookup1DefinitionId]) VALUES
	(N'sdks',	N'SDKs',		N'SDK',			N'FinishedGoods');
DECLARE
@R6 [dbo].ResourceList, @RP6 [dbo].ResourcePickList;
-- vehicles for sales. When using one for internal purposes, it is converted to motor vehicle
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'vehicles'
					[Name],		[IsLeaf],	[Node]) VALUES
	(N'vehicles',	N'Cars',	1,			N'/1/'),
	(N'vehicles',	N'Sedan',	1,			N'/1/1/'),
	(N'vehicles',	N'4xDrive',	1,			N'/1/2/'),
	(N'vehicles',	N'Sports',	1,			N'/1/3/'),
	(N'vehicles',	N'Trucks',	0,			N'/2/');

	DECLARE @RCSedan INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceDefinitionId] = N'vehicles' AND [Node] = N'/1/1/');
	INSERT INTO @R6 ([Index],
	[ResourceClassificationId],	[Name]) VALUES
	(0, @RCSedan,				N'Toyota Camry 2018'),--1
	(1, @RCSedan,				N'Fake'),--1
	(2, @RCSedan,				N'Toyota Yaris 2018');--1
	INSERT INTO @RP6([Index], [ResourceIndex],
			[ProductionDate],	[Code], [Name]) VALUES
	(0,0,	N'2017.10.01',		N'101', N'101, Red/White/Leather'),
	(1,0,	N'2017.10.15',		N'102', N'102, Black/Black/Wool'),
	(2,0,	N'2017.10.15',		N'199', N'199'),
	(3,2,	N'2017.10.01',		N'201', N'201');
	EXEC [api].[Resources__Save] -- N'vehicles'
		@DefinitionId = N'vehicles',
		@Resources = @R6,
--		@Picks = @RP6,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting vehicles'
		GOTO Err_Label;
	END;
	DECLARE @TC INT = (SELECT [Id] FROM dbo.Resources WHERE [Name] = N'Toyota Camry 2018' AND [ResourceDefinitionId] = N'vehicles');
	INSERT INTO dbo.ResourcePicks
	([ResourceId], [ProductionDate], [Code], [Name])
	SELECT	@TC, [ProductionDate], [Code], [Name] FROM @RP6;