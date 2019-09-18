DECLARE
@R6 [dbo].ResourceList, @RP6 [dbo].ResourcePickList;

	INSERT INTO dbo.ResourceClassifications ([ResourceDefinitionId], -- N'vehicles'
					[Name],		[IsLeaf],	[Node]) VALUES
	(N'vehicles',	N'Cars',	1,			N'/1/'),
	(N'vehicles',	N'Sedan',	1,			N'/1/1/'),
	(N'vehicles',	N'4xDrive',	1,			N'/1/2/'),
	(N'vehicles',	N'Sports',	1,			N'/1/3/'),
	(N'vehicles',	N'Trucks',	0,			N'/2/');

	DECLARE @RCVS INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceDefinitionId] = N'vehicles' AND [Node] = N'/1/1/');
	INSERT INTO @R6 ([Index],
	[ResourceClassificationId],	[Name],					[CountUnitId]) VALUES
	(0, @RCVS,					N'Toyota Camry 2018',	@pcsUnit),--1
	(1, @RCVS,					N'Fake',				@pcsUnit),--1
	(2, @RCVS,					N'Toyota Yaris 2018',	@pcsUnit);--1
	INSERT INTO @RP6([Index], [ResourceIndex],
			[ProductionDate],	[Code]) VALUES
	(0,0,	N'2017.10.01',		N'101'),
	(1,0,	N'2017.10.15',		N'102'),
	(2,0,	N'2017.10.15',		N'199'),
	(3,2,	N'2017.10.01',		N'201');
	EXEC [api].[Resources__Save] -- N'vehicles'
		@ResourceDefinitionId = N'vehicles',
		@Resources = @R6,
		@Picks = @RP6,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;
	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting vehicles'
		GOTO Err_Label;
	END;