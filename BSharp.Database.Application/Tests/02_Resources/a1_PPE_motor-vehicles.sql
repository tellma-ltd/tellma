	-- We look at the specialized Excel files in the General Services department, and we add Resource definitions accordingly
	INSERT INTO dbo.ResourceDefinitions (
		[Id],			[TitlePlural],		[TitleSingular],	[ResourceTypeParentList], [Lookup1Visibility], [Lookup1Label], [Lookup1DefinitionId]) VALUES
	(N'motor-vehicles',	N'Motor Vehicles',	N'Motor Vehicle',	N'MotorVehicles',		N'Required',			N'Make',		N'vehicle-makes');
	
	INSERT INTO dbo.ResourceClassifications (
	[ResourceDefinitionId],	[Name],		[IsLeaf],	[Node]) VALUES
	(N'motor-vehicles',		N'Cars',	0,			N'/1/'),
	(N'motor-vehicles',		N'Minivans',1,			N'/2/');

	DECLARE @MotorVehicles dbo.ResourceList;
	INSERT INTO @MotorVehicles ([Index],
	[ResourceTypeId],		[ResourceClassificationId],		[Name],		[MonetaryValueCurrencyId],	[LengthUnitId],				[AvailableSince], [Lookup1Id],									[Text1]) VALUES
	(0, N'MotorVehicles',	dbo.fn_RCName__Id(N'Cars'),		N'Prius 2018 - AA 78172',	N'USD',		dbo.fn_UnitName__Id(N'Km'),	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'), N'AA 78172'),--1
	(1, N'MotorVehicles',	dbo.fn_RCName__Id(N'Cars'),		N'Prius 2018 - BX 54662',	N'USD',		dbo.fn_UnitName__Id(N'Km'),	N'2017.10.01',		dbo.fn_Lookup(N'vehicle-makes', N'Toyota'), N'BX54662'),--1
	(2, N'MotorVehicles',	dbo.fn_RCName__Id(N'Minivans'),	N'Minivan 2019 - AA 100000',N'USD',		dbo.fn_UnitName__Id(N'Km'),	N'2018.12.01' ,		dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'), N'AA100000'),
	(3, N'MotorVehicles',	dbo.fn_RCName__Id(N'Minivans'), N'Minivan 2019 - LM 999812',N'USD',		dbo.fn_UnitName__Id(N'Km'),	N'2018.12.01' ,		dbo.fn_Lookup(N'vehicle-makes', N'Mercedes'), N'LM999812')
	;

	EXEC [api].[Resources__Save]
		@DefinitionId = N'motor-vehicles',
		@Entities = @MotorVehicles,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting PPE (motor-vehicles)'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1 
	BEGIN
		SELECT  N'motor-vehicles' AS [Resource Definition]
		DECLARE @MotorVehiclesIds dbo.IdList;
		INSERT INTO @MotorVehiclesIds SELECT [Id] FROM dbo.Resources WHERE [ResourceDefinitionId] = N'motor-vehicles';

		SELECT ResourceTypeId, Classification, [Name] AS 'Vehcile', [Currency] AS 'Price In',	[LengthUnit] AS 'Usage In',	[AvailableSince] AS 'Production Date', [Lookup1], [Text1] AS 'Plate #'
		FROM rpt.Resources(@MotorVehiclesIds);
	END