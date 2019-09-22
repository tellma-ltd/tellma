	DECLARE @R5 [dbo].ResourceList, @RP5 [dbo].ResourcePickList;
	
	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'property-plant-and-equipment'
										[Name],								[IsLeaf],	[Node]) VALUES
	(N'property-plant-and-equipment',	N'Land and buildings',					0,			N'/1/'),
	(N'property-plant-and-equipment',	N'Land',								1,			N'/1/1/'),
	(N'property-plant-and-equipment',	N'Buildings',							1,			N'/1/2/'),
	(N'property-plant-and-equipment',	N'Machinery',							1,			N'/2/'),
	(N'property-plant-and-equipment',	N'Vehicles',							0,			N'/3/'),
	(N'property-plant-and-equipment',	N'Ships',								1,			N'/3/1/'),
	(N'property-plant-and-equipment',	N'Aircraft',							1,			N'/3/2/'),
	(N'property-plant-and-equipment',	N'Motor vehicles',						1,			N'/3/3/'),
	(N'property-plant-and-equipment',	N'Fixture and fittings',				1,			N'/4/'),
	(N'property-plant-and-equipment',	N'Office equipment',					1,			N'/5/'),
--	(N'property-plant-and-equipment',	N'Computer equipment',					1,			N'/6/'),
	(N'property-plant-and-equipment',	N'Communication and network equipment',	1,			N'/7/'),
	(N'property-plant-and-equipment',	N'Nework infrastructure',				1,			N'/8/'),
	(N'property-plant-and-equipment',	N'Bearer plants',						1,			N'/9/'),
	(N'property-plant-and-equipment',	N'Bearer plants',						1,			N'/10/'),
	(N'property-plant-and-equipment',	N'Tangible exploration and evaluation assets',1,	N'/11/'),
	(N'property-plant-and-equipment',	N'Mining assets',						1,			N'/12/'),
	(N'property-plant-and-equipment',	N'Oil and gas assets',					1,			N'/13/'),
	(N'property-plant-and-equipment',	N'Power generating assets',				1,			N'/14/'),
	(N'property-plant-and-equipment',	N'Leashold improvements',				1,			N'/15/'),
	(N'property-plant-and-equipment',	N'Construction in progress',			0,			N'/16/'),
	(N'property-plant-and-equipment',	N'Affordable complexes',				1,			N'/16/1/'),
	(N'property-plant-and-equipment',	N'Luxury Complexes',					1,			N'/16/2/');

	DECLARE @RCMV INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceDefinitionId] = N'property-plant-and-equipment' AND [Node] = N'/3/3/');
	INSERT INTO @R5 ([Index],
	[ResourceClassificationId],	[Name],		[LengthUnitId]) VALUES
	(0, @RCMV,					N'Cars',	@kmUnit),--1
	(1, @RCMV,					N'Buses',	@kmUnit);
	INSERT INTO @RP5([Index], [ResourceIndex],
		[ProductionDate],	[Code], [Name]) VALUES
	(0,0,	N'2017.10.01',	N'101', N'Toyota Camry 2018, Plate AA12345'),
	(1,0,	N'2017.10.15',	N'102', N'Toyota Prius 2016, Plate BX54662'),
	(2,1,	N'2017.10.01',	N'103', N'Mercedes Minivan, Plate AA100000'),
	(3,1,	N'2017.10.15',	N'104', N'Toyota Minivan 2016, Plate LM999812');

	EXEC [api].[Resources__Save] -- N'property-plant-and-equipment'
		@DefinitionId = N'property-plant-and-equipment',
		@Resources = @R5,
--		@Picks = @RP5,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor vehicles)'
		GOTO Err_Label;
	END;
	DECLARE @CarsRC INT = (SELECT [Id] FROM dbo.Resources WHERE [Name] = N'Cars' AND [ResourceDefinitionId] = N'property-plant-and-equipment');
	INSERT INTO dbo.ResourcePicks
			([ResourceId],	[ProductionDate], [Code], [Name])
	SELECT	@CarsRC,		[ProductionDate], [Code], [Name] FROM @RP5;