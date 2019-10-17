	INSERT INTO dbo.ResourceDefinitions (
	[Id],		[TitlePlural],	[TitleSingular],[ResourceTypeParentList], [Lookup1Visibility], [Lookup1Lable], [Lookup1DefinitionId]) VALUES
	(N'sdks',	N'SDKs',		N'SDK',			N'FinishedGoods');
	
DECLARE @Sdks [dbo].ResourceList;

	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'vehicles'
					[Name],		[IsLeaf],	[Node]) VALUES
	(N'vehicles',	N'Cars',	1,			N'/1/'),
	(N'vehicles',	N'Sedan',	1,			N'/1/1/'),
	(N'vehicles',	N'4xDrive',	1,			N'/1/2/'),
	(N'vehicles',	N'Sports',	1,			N'/1/3/'),
	(N'vehicles',	N'Trucks',	0,			N'/2/');

	INSERT INTO @Sdks ([Index],
	[ResourceClassificationId],		[Code],	[Name],				[ProductionDate],	[Description] ) VALUES
	(0, dbo.fn_RCName__Id(N'Sedan'),N'101',	N'Toyota Camry 2018',	N'2017.10.01',	N'Red/White/Leather'),
	(1, dbo.fn_RCName__Id(N'Sedan'),N'102',	N'Toyota Camry 2018',	N'2017.10.15',	N'Black/Black/Wool'),
	(2, dbo.fn_RCName__Id(N'Sedan'),N'103',	N'Toyota Camry 2018',	N'2017.10.01',	N'Red/White/Leather'),
	(3, dbo.fn_RCName__Id(N'Sedan'),N'104',	N'Toyota Camry 2018',	N'2017.10.01',	N'Red/White/Leather'),
	(4, dbo.fn_RCName__Id(N'Sedan'),N'199',	N'Fake',				NULL,			N''),--1
	(5, dbo.fn_RCName__Id(N'Sedan'),N'201',	N'Toyota Yaris 2018',	N'2017.10.01',	N'Red/White/Leather');--1

	EXEC [api].[Resources__Save]
		@DefinitionId = N'sdks',
		@Resources = @Sdks,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting sdks'
		GOTO Err_Label;
	END;