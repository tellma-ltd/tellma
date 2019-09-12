/* Use Cases
Completed
	- Inserting
	- Updating
	- Deleting
Missing	
	- Activating
	- Deactivating
*/
BEGIN -- Cleanup & Declarations
	DECLARE @R1 [dbo].ResourceList, @R2 [dbo].ResourceList, @R3 [dbo].ResourceList, @R4 [dbo].ResourceList, @R5 [dbo].ResourceList, @R6 [dbo].ResourceList;
	DECLARE @RP1 [dbo].ResourcePickList, @RP2 [dbo].ResourcePickList, @RP3 [dbo].ResourcePickList, @RP4 [dbo].ResourcePickList, @RP5 [dbo].ResourcePickList, @RP6 [dbo].ResourcePickList;
	DECLARE @R1Ids dbo.[IdList], @R2Ids dbo.[IdList], @R3Ids dbo.[idList];
	DECLARE @R1IndexedIds dbo.IndexedIdList, @R2IndexedIds dbo.IndexedIdList, @R3IndexedIds dbo.IndexedIdList;


END
	INSERT INTO dbo.ResourceClassifications
	([ResourceType],					[Name],									[IsLeaf]) VALUES
	(N'property-plant-and-equipment',	N'Property, plant and equipment',		0),
	(N'investment-property',			N'Investment property',					1),
	(N'intangible-assets',				N'Intangible assets other than goodwill',1),
	(N'financial-assets',				N'Financial assets',					1),
	(N'investments',					N'Investments',							1),	
	(N'biological-assets',				N'Biological assets',					1),
	(N'inventories',					N'Inventories',							0),
	(N'cash-and-cash-equivalents',		N'Cash and cash equivalents',			1),
	(N'trade-and-other-receivables',	N'Trade and other receivables',			1);
DECLARE @RTPPE INT, @RTFIA INT, @RTSTK INT, @RTCCE INT
SELECT 
	@RTPPE = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'property-plant-and-equipment' AND [Code] = N''),
	@RTFIA = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'financial-assets' AND [Code] = N''),
	@RTSTK = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'inventories' AND [Code] = N''),
	@RTCCE = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'cash-and-cash-equivalents' AND [Code] = N'')

BEGIN -- Inserting
	INSERT INTO @R1 ([Index],
	[ResourceClassificationId],	[Name],					[Code],		[UnitId], [CountUnitId]) VALUES
	(0, @RTCCE,					N'Cash/ETB',			N'ETB',		@ETBUnit, @pcsUnit),
	(1, @RTCCE,					N'Cash/USD',			N'USD',		@USDUnit, @pcsUnit),
	(2, @RTCCE,					N'Received Checks/ETB',	N'RCKETB',	@ETBUnit, @pcsUnit); --1
DECLARE @RCKETBIndex INT = (SELECT [Index] FROM @R1 WHERE [Code] = N'RCKETB');
DECLARE @CBEBank INT, @AWBBank INT;
	INSERT INTO @RP1([Index], 
	[ResourceIndex], [ProductionDate], [Code], [MonetaryValue], [IssuingBankId]) VALUES
	(0,@RCKETBIndex,	N'2017.10.01',	N'101009',	6900,		@CBEBank),
	(1,@RCKETBIndex,	N'2017.10.15',	N'2308',	17550,		@AWBBank);

	EXEC [api].[Resources__Save]
		@ResourceType = N'cash-and-cash-equivalents',
		@Resources = @R1,
		@Picks = @RP1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Cash and cash equivalents'
		GOTO Err_Label;
	END;

	INSERT INTO dbo.ResourceClassifications
	([ResourceType],	[ParentId],	[Name],			[Code], [IsLeaf]) VALUES
	(N'inventories',	@RTSTK,		N'Vehicles',	N'1',	0);
DECLARE @RTVHC INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'inventories' AND [Code] = N'1');

	INSERT INTO dbo.ResourceClassifications
	([ResourceType],	[ParentId],	[Name],			[Code], [IsLeaf]) VALUES
	(N'inventories',	@RTVHC,		N'Sedan',		N'11',	1),
	(N'inventories',	@RTVHC,		N'4x Drive',	N'12',	1),
	(N'inventories',	@RTVHC,		N'Sports',		N'13',	1);

DECLARE @RCVS INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'inventories' AND [Code] = N'11');
	INSERT INTO @R2 ([Index],
	[ResourceClassificationId],	[Name],					[Code],	[UnitId]) VALUES
	(0, @RCVS,					N'Toyota Camry 2018',	NULL,	@pcsUnit),--1
	(1, @RCVS,					N'Fake',				NULL,	@pcsUnit),--1
	(2, @RCVS,					N'Toyota Yaris 2018',	NULL,	@pcsUnit);--1

DECLARE @ToyotaCamryIndex INT = (SELECT [Index] FROM @R2 WHERE [Name] = N'Toyota Camry 2018');
DECLARE @ToyotaYarisIndex INT = (SELECT [Index] FROM @R2 WHERE [Name] = N'Toyota Yaris 2018');
	INSERT INTO @RP2([Index],
	[ResourceIndex],		[ProductionDate], [Code]) VALUES
	(0,@ToyotaCamryIndex,	N'2017.10.01',		N'101'),
	(1,@ToyotaCamryIndex,	N'2017.10.15',		N'102'),
	(2,@ToyotaCamryIndex,	N'2017.10.15',		N'199'),
	(3,@ToyotaYarisIndex,	N'2017.10.01',		N'201');

	INSERT INTO dbo.ResourceClassifications
	([ResourceType],	[ParentId],	[Name],				[Code], [IsLeaf], [HasMass], [HasCount]) VALUES
	(N'inventories',	@RTSTK,		N'Raw Materials',	N'2',	1,			1,			1);
DECLARE @RCRM INT = (SELECT [Id] FROM dbo.ResourceClassifications WHERE [ResourceType] = N'inventories' AND [Code] = N'2');

	INSERT INTO @R2 ([Index],
	[ResourceClassificationId],	[Name],					[Code],			[UnitId], [CountUnitId]) VALUES
	(3, @RCRM,					N'HR 1000MMx1.9MM',		N'HR1000x1.9',	@KgUnit, @pcsUnit),
	(4, @RCRM,					N'CR 1000MMx1.4MM',		N'CR1000x1.4',	@KgUnit, @pcsUnit);

DECLARE @HotRollIndex INT =  (SELECT [Index] FROM @R2 WHERE [Name] = N'HR 1000MMx1.9MM');
	INSERT INTO @RP2([Index],
	[ResourceIndex], [ProductionDate], [Code], [Mass]) VALUES
	(4,@HotRollIndex,		N'2017.10.01',	N'54001', 7891),
	(5,@HotRollIndex,		N'2017.10.15',	N'54002', 6985),
	(6,@HotRollIndex,		N'2017.10.15',	N'60032', 7320),
	(7,@HotRollIndex,		N'2017.10.01',	N'60342', 7100);

	EXEC [api].[Resources__Save]
		@ResourceType = N'inventories',
		@Resources = @R2,
		@Picks = @RP2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting inventories'
		GOTO Err_Label;
	END;

	INSERT INTO @R3 ([Index],
	[ResourceClassificationId],	[Name],				[Code],			[UnitId]) VALUES
	(0, @RTFIA,					N'Common Stock',	N'CMNSTCK',		@shareUnit),
	(1, @RTFIA,					N'Premium Stock',	N'PRMMSTCK',	@shareUnit);

	EXEC [api].[Resources__Save]
		@ResourceType = N'financial-assets',
		@Resources = @R3,
		@Picks = @RP3,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting financial assets'
		GOTO Err_Label;
	END;

	IF @DebugResources = 1
	BEGIN
		SELECT * FROM dbo.ResourceClassifications ORDER BY [ResourceType], [Code];
		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
		EXEC rpt.[sp_ResourcesPicks] @R2Ids;
	END

	--(6, N'general-goods',		N'Teddy bear',			NULL,		NULL,			@pcsUnit),



--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],		[Code],	[SystemCode], [UnitId]) VALUES
--	(11, N'general-goods',		N'Cotton',	NULL,	NULL,		@KgUnit);

--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],		[Code],	[SystemCode], [UnitId]) VALUES
--	(12, N'general-goods',		N'Oil',		NULL,	NULL,		@LiterUnit),
--	(13, N'general-goods',		N'Diesel',	NULL,	NULL,		@LiterUnit);

--INSERT INTO @R1 ([Index],
--[IfrsResourceClassificationId],	[Name],					[Code],		[SystemCode],	[UnitId]) VALUES
--	(14, N'wages-and-salaries',	N'Basic',			NULL,		N'Basic',		@moUnit),
--	(15, N'wages-and-salaries',	N'Transportation',	NULL,		N'Transportation',@moUnit),
--	(16, N'wages-and-salaries',	N'Holiday Overtime',NULL,		N'HolidayOvertime',@hrUnit),
--	(17, N'wages-and-salaries',	N'Rest Overtime',	NULL,		N'RestOvertime',@hrUnit),
--	(18, N'wages-and-salaries',	N'Labor (hourly)',	NULL,		N'LaborHourly',	@hrUnit),
--	(19, N'wages-and-salaries',	N'Labor (daily)',	NULL,		N'LaborDaily',	@dayUnit),
--	(20, N'PPEServices',		N'Girgi Office',	N'Goff',	NULL,			@moUnit),
--	(21, N'PPEServices',		N'Car 101 - Svc',	N'101D',	NULL,			@moUnit),
--	(22, N'PPEServices',		N'Car 102 - Svc',	N'102D',	NULL,			@dayUnit);

--	EXEC [api].[Resources__Save]
--		@Resources = @R1,
--		@Picks = @RP1,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Inserting Resources'
--		GOTO Err_Label;
--	END;
--	IF @DebugResources = 1
--	BEGIN
--		--SELECT * FROM dbo.Resources;
--		INSERT INTO @R1Ids SELECT [Id] FROM dbo.Resources;
--		EXEC rpt.[sp_ResourcesPicks] @R1Ids;
--	END
END

--BEGIN -- Updating
--	INSERT INTO @R2 ([Index],
--		[Id], [UnitId], [IfrsResourceClassificationId], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId], [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	)
--	SELECT ROW_NUMBER() OVER (ORDER BY [Id]),
--		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId], [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	FROM [dbo].Resources
--	WHERE [Name] IN (N'Toyota Camry 2018')
--	INSERT INTO @RP2 ( [ResourceIndex], [Id], [ResourceId],	[Code], [ProductionDate])
--	SELECT				R2.[Index], RI.[Id], RI.[ResourceId],  RI.[Code], RI.[ProductionDate]
--	FROM [dbo].[ResourcePicks] RI
--	JOIN @R2 R2 ON RI.ResourceId = R2.[Id]
--	WHERE ResourceId IN (SELECT [Id] FROM @R2);

--	UPDATE @R2
--	SET 
--		[Name] = [Name] + N' - (New)';

--	DELETE FROM @RP2 WHERE [Code] = N'199';

--	EXEC [api].[Resources__Save]
--		@Resources = @R2,
--		@Picks = @RP2,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Updating Resources'
--		GOTO Err_Label;
--	END
--	IF @DebugResources = 1
--	BEGIN
--		--SELECT * FROM @R2;
--		--SELECT * FROM @RP2;
--		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
--		EXEC rpt.[sp_ResourcesPicks] @R2Ids;
--	END
--END

--BEGIN -- Deleting
--	INSERT INTO @R3 ([Index],
--		[Id], [UnitId], [IfrsResourceClassificationId], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	)
--	SELECT ROW_NUMBER() OVER (ORDER BY [Id]),
--		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
--		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
--	FROM [dbo].Resources
--	WHERE [Name] LIKE N'Fake%';

--	INSERT INTO @R3IndexedIds SELECT [Index], [Id] FROM @R3

--	EXEC [api].[Resources__Delete]
--		@IndexedIds = @R3IndexedIds,
--		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--	IF @ValidationErrorsJson IS NOT NULL 
--	BEGIN
--		Print 'Deleting Resources'
--		GOTO Err_Label;
--	END
--	IF @DebugResources = 1
--	BEGIN
--		INSERT INTO @R3Ids SELECT [Id] FROM dbo.Resources;
--		EXEC rpt.[sp_ResourcesPicks] @R3Ids;
--	END
--END 

DECLARE @ETB int, @USD int, @CommonStock int;
DECLARE @Camry2018 int, @Cotton int, @TeddyBear int, @Car1 int, @Car2 int;
DECLARE @HOvertime int, @ROvertime int, @Basic int, @Transportation int, 
		@LaborHourly int, @LaborDaily int, @Car1Svc int, @GOff int;
DECLARE @HR1000x1_9 INT, @CR1000x1_4 INT;
DECLARE @Oil INT, @Diesel INT;

SELECT 
	@ETB = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'ETB'), 
	@USD = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'USD'),
	@Camry2018 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Toyota Camry 2018'),
	@Car1 = (SELECT [Id] FROM [dbo].[ResourcePicks] WHERE [Code] = N'101'),
	@Car2 = (SELECT [Id] FROM [dbo].[ResourcePicks] WHERE [Code] = N'102'),
	--@Car1Svc = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'101D'),
	@GOff = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'Goff'),
	@Cotton = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Cotton'),
	@TeddyBear = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Teddy bear'),
	@CommonStock = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Common Stock'),
	@HOvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'HolidayOvertime'),
	@ROvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'RestOvertime'),
	@Basic = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Basic'),
	@Transportation = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Transportation'),
	@LaborHourly = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborHourly'),
	@LaborDaily = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborDaily'),
	@HR1000x1_9 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'HR1000x1.9'),
	@CR1000x1_4 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'CR1000x1.4'),
	@Oil = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Oil'),
	@Diesel = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Diesel');