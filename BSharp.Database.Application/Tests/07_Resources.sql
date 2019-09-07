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
	DECLARE @R1 [dbo].ResourceList, @R2 [dbo].ResourceList, @R3 [dbo].ResourceList;
	DECLARE @RI1 [dbo].ResourcePickList, @RI2 [dbo].ResourcePickList, @RI3 [dbo].ResourcePickList;
	DECLARE @R1Ids dbo.[IdList], @R2Ids dbo.[IdList], @R3Ids dbo.[idList];
	DECLARE @R1IndexedIds dbo.IndexedIdList, @R2IndexedIds dbo.IndexedIdList, @R3IndexedIds dbo.IndexedIdList;

	DECLARE @ETB int, @USD int, @CommonStock int;
	DECLARE @Camry2018 int, @Cotton int, @TeddyBear int, @Car1 int, @Car2 int;
	DECLARE @HOvertime int, @ROvertime int, @Basic int, @Transportation int, 
			@LaborHourly int, @LaborDaily int, @Car1Svc int, @GOff int;
	DECLARE @HR1000x1_9 INT, @CR1000x1_4 INT;
	DECLARE @Oil INT, @Diesel INT;
END
BEGIN -- Inserting
	INSERT INTO @R1 ([Index],
		[ResourceType],		[Name],					[Code],		[UnitId]) VALUES
	(0, N'money',			N'ETB',					N'ETB',		@ETBUnit),
	(1, N'money',			N'USD',					N'USD',		@USDUnit),
	(2, N'money',			N'ETB Incoming Checks',	N'ICKETB',	@ETBUnit); --1
DECLARE @ICKETBIndex INT = (SELECT [Index] FROM @R1 WHERE [Code] = N'ICKETB');
DECLARE @CBEBank INT, @AWBBank INT;
INSERT INTO @RI1([ResourceIndex], [ProductionDate], [Code], [MoneyAmount], [IssuingBankId]) VALUES
				(@ICKETBIndex,	N'2017.10.01',		N'101009', 6900,		@CBEBank),
				(@ICKETBIndex,	N'2017.10.15',		N'2308', 17550,			@AWBBank);

INSERT INTO @R1 ([Index],
		[ResourceType],			[Name],					[Code],		[SystemCode], [UnitId]) VALUES
	(3, N'motor-vehicles',		N'Toyota Camry 2018',	NULL,		NULL,			@pcsUnit),--1
	(4, N'motor-vehicles',		N'Fake',				NULL,		NULL,			@pcsUnit),--1
	(5, N'motor-vehicles',		N'Toyota Yaris 2018',	NULL,		NULL,			@pcsUnit),--1
	(6, N'general-goods',		N'Teddy bear',			NULL,		NULL,			@pcsUnit),
	(7, N'financial-instruments',N'Common Stock',		N'CMNSTCK',	N'CMNSTCK',		@shareUnit),
	(8, N'financial-instruments',N'Premium Stock',		N'PRMMSTCK',NULL,			@shareUnit);
DECLARE @ToyotaCamryIndex INT = (SELECT [Index] FROM @R1 WHERE [Name] = N'Toyota Camry 2018');
DECLARE @ToyotaYarisIndex INT = (SELECT [Index] FROM @R1 WHERE [Name] = N'Toyota Yaris 2018');
INSERT INTO @RI1([ResourceIndex], [ProductionDate], [Code]) VALUES
				(@ToyotaCamryIndex,	N'2017.10.01',		N'101'),
				(@ToyotaCamryIndex,	N'2017.10.15',		N'102'),
				(@ToyotaCamryIndex,	N'2017.10.15',		N'199'),
				(@ToyotaYarisIndex,	N'2017.10.01',		N'201');
INSERT INTO @R1 ([Index],
		[ResourceType],		[Name],					[Code],			[UnitId]) VALUES
	(9, N'raw-materials',	N'HR 1000MMx1.9MM',		N'HR1000x1.9',	@KgUnit),
	(10, N'raw-materials',	N'CR 1000MMx1.4MM',		N'CR1000x1.4',	@KgUnit);
DECLARE @HotRollIndex INT =  (SELECT [Index] FROM @R1 WHERE [Name] = N'HR 1000MMx1.9MM');
INSERT INTO @RI1([ResourceIndex], [ProductionDate], [Code], [Mass]) VALUES
				(@HotRollIndex,		N'2017.10.01',	N'54001', 7891),
				(@HotRollIndex,		N'2017.10.15',	N'54002', 6985),
				(@HotRollIndex,		N'2017.10.15',	N'60032', 7320),
				(@HotRollIndex,		N'2017.10.01',	N'60342', 7100);
INSERT INTO @R1 ([Index],
		[ResourceType],		[Name],		[Code],	[SystemCode], [UnitId]) VALUES
	(11, N'general-goods',	N'Cotton',	NULL,	NULL,		@KgUnit);

INSERT INTO @R1 ([Index],
		[ResourceType],		[Name],		[Code],	[SystemCode], [UnitId]) VALUES
	(12, N'general-goods',	N'Oil',		NULL,	NULL,		@LiterUnit),
	(13, N'general-goods',	N'Diesel',	NULL,	NULL,		@LiterUnit);

INSERT INTO @R1 ([Index],
		[ResourceType],		[Name],					[Code],		[SystemCode],	[UnitId]) VALUES
	(14, N'wages-and-salaries',	N'Basic',			NULL,		N'Basic',		@moUnit),
	(15, N'wages-and-salaries',	N'Transportation',	NULL,		N'Transportation',@moUnit),
	(16, N'wages-and-salaries',	N'Holiday Overtime',NULL,		N'HolidayOvertime',@hrUnit),
	(17, N'wages-and-salaries',	N'Rest Overtime',	NULL,		N'RestOvertime',@hrUnit),
	(18, N'wages-and-salaries',	N'Labor (hourly)',	NULL,		N'LaborHourly',	@hrUnit),
	(19, N'wages-and-salaries',	N'Labor (daily)',	NULL,		N'LaborDaily',	@dayUnit),
	(20, N'PPEServices',		N'Girgi Office',	N'Goff',	NULL,			@moUnit),
	(21, N'PPEServices',		N'Car 101 - Svc',	N'101D',	NULL,			@moUnit),
	(22, N'PPEServices',		N'Car 102 - Svc',	N'102D',	NULL,			@dayUnit);

	--INSERT INTO @R1IndexedIds([Index], [Id])
	EXEC [api].[Resources__Save]
		@Resources = @R1,
		@Picks = @RI1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Resources'
		GOTO Err_Label;
	END;
	IF @DebugResources = 1
	BEGIN
		SELECT * FROM dbo.Resources;
		INSERT INTO @R1Ids SELECT [Id] FROM dbo.Resources;
		EXEC rpt.[sp_ResourcesPicks] @R1Ids;
	END
END

BEGIN -- Updating
	INSERT INTO @R2 ([Index],
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
		[CurrencyId], [MassUnitId], [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	)
	SELECT ROW_NUMBER() OVER (PARTITION BY [Id] ORDER BY [Id]),
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
		[CurrencyId], [MassUnitId], [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	FROM [dbo].Resources
	WHERE [Name] IN (N'Toyota Camry 2018')
	INSERT INTO @RI2 ( [ResourceIndex], [Id], [ResourceId],	[Code], [ProductionDate])
	SELECT				R2.[Index], RI.[Id], RI.[ResourceId],  RI.[Code], RI.[ProductionDate]
	FROM [dbo].[ResourcePicks] RI
	JOIN @R2 R2 ON RI.ResourceId = R2.[Id]
	WHERE ResourceId IN (SELECT [Id] FROM @R2);

	UPDATE @R2
	SET 
		[Name] = [Name] + N' - (New)';

	DELETE FROM @RI2 WHERE [Code] = N'199';

	EXEC [api].[Resources__Save]
		@Resources = @R2,
		@Picks = @RI2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Updating Resources'
		GOTO Err_Label;
	END
	IF @DebugResources = 1
	BEGIN
		--SELECT * FROM @R2;
		--SELECT * FROM @RI2;
		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
		EXEC rpt.[sp_ResourcesPicks] @R2Ids;
	END
END

BEGIN -- Deleting
	INSERT INTO @R3 ([Index],
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	)
	SELECT ROW_NUMBER() OVER (PARTITION BY [Id] ORDER BY [Id]),
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode],
		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	FROM [dbo].Resources
	WHERE [Name] LIKE N'Fake%';

	INSERT INTO @R3IndexedIds SELECT [Index], [Id] FROM @R3

	EXEC [api].[Resources__Delete]
		@IndexedIds = @R3IndexedIds,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Deleting Resources'
		GOTO Err_Label;
	END
	IF @DebugResources = 1
	BEGIN
		INSERT INTO @R3Ids SELECT [Id] FROM dbo.Resources;
		EXEC rpt.[sp_ResourcesPicks] @R3Ids;
	END
END 

SELECT 
	@ETB = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'ETB'), 
	@USD = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'USD'),
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