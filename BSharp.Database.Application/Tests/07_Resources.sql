/* Use Cases
	- Inserting
	- Updating
	- Deleting
	To ve added
	- Activating
	- Deactivating

*/
/*
INSERT INTO dbo.[Resources]([ResourceType], [Name], [Code], [ValueMeasure], [UnitId], [CurrencyId], [CountUnitId]) VALUES
(N'currencies', N'USD', N'101', N'Currency', @USDUnit, @USDUnit, NULL), --  -- Currency, Mass, Volumne, Area, Length, Count, Time
(N'financial-instruments', N'Common Shares', N'301', N'Count', @pcsUnit, NULL, @pcsUnit);
DECLARE @USD INT, @CommonStock INT;
SELECT @USD = [Id] FROM dbo.[Resources] WHERE Code = N'101';
SELECT @CommonStock = [Id] FROM dbo.[Resources] WHERE Code = N'301';
*/
BEGIN -- Cleanup & Declarations
	DECLARE @R1 [dbo].ResourceList, @R2 [dbo].ResourceList, @R3 [dbo].ResourceList;
	DECLARE @R1Ids dbo.[IdList], @R2Ids dbo.[IdList], @R3Ids dbo.[idList];
	DECLARE @R1IndexedIds dbo.IndexedIdList, @R2IndexedIds dbo.IndexedIdList, @R3IndexedIds dbo.IndexedIdList;

	DECLARE @ETB int, @USD int, @CommonStock int;
	DECLARE @Camry2018 int, @Cotton int, @TeddyBear int, @Car1 int, @Car2 int;
	DECLARE @HOvertime int, @ROvertime int, @Basic int, @Transportation int, 
			@LaborHourly int, @LaborDaily int, @Car1Svc int, @GOff int;
END
BEGIN -- Inserting
	INSERT INTO @R1
	([ResourceType],		[Name],					[Code],		[SystemCode], [ValueMeasure], [CurrencyId]) VALUES
	(N'currencies',			N'ETB',					N'ETB',		N'Functional',	N'Currency',	@ETBUnit),
	(N'currencies',			N'USD',					N'USD',		NULL,			N'Currency',	@USDUnit);
INSERT INTO @R1
	([ResourceType],		[Name],					[Code],		[SystemCode], [ValueMeasure], [CountUnitId]) VALUES
	(N'MotorVehicles',		N'Toyota Camry 2018',	NULL,		NULL,			N'Count',		@pcsUnit),
	(N'SKD',				N'Toy. Cam. 18 - 101',	N'101',		NULL,			N'Count',		@eaUnit),
	(N'SKD',				N'Toy. Cam. 18 - 102',	N'102',		NULL,			N'Count',		@eaUnit),
	(N'SKD',				N'Fake',				N'199',		NULL,			N'Count',		@eaUnit),
	(N'GeneralGoods',		N'Teddy bear',			NULL,		NULL,			N'Count',		@pcsUnit),
	(N'financial-instruments',N'Common Stock',		N'CMNSTCK',	N'CMNSTCK',		N'Count',		@shareUnit),
	(N'financial-instruments',N'Premium Stock',		N'PRMMSTCK',NULL,			N'Count',		@shareUnit);
INSERT INTO @R1
	([ResourceType],		[Name],					[Code],		[SystemCode], [ValueMeasure], [MassUnitId]) VALUES
	(N'GeneralGoods',		N'Cotton',				NULL,		NULL,			N'Mass',		@KgUnit);
INSERT INTO @R1
	([ResourceType],		[Name],					[Code],		[SystemCode], [ValueMeasure], [TimeUnitId]) VALUES
	(N'WagesAndSalaries',	N'Basic',				NULL,		N'Basic',		N'Time',		@moUnit),
	(N'WagesAndSalaries',	N'Transportation',		NULL,		N'Transportation',N'Time',		@moUnit),
	(N'WagesAndSalaries',	N'Holiday Overtime',	NULL,		N'HolidayOvertime',N'Time',		@hrUnit),
	(N'WagesAndSalaries',	N'Rest Overtime',		NULL,		N'RestOvertime',N'Time',		@hrUnit),
	(N'WagesAndSalaries',	N'Labor (hourly)',		NULL,		N'LaborHourly',	N'Time',		@hrUnit),
	(N'WagesAndSalaries',	N'Labor (daily)',		NULL,		N'LaborDaily',	N'Time',		@dayUnit),
	(N'PPEServices',		N'Girgi Office',		N'Goff',	NULL,			N'Time',		@moUnit),
	(N'PPEServices',		N'Car 101 - Svc',		N'101D',	NULL,			N'Time',		@moUnit),
	(N'PPEServices',		N'Car 102 - Svc',		N'102D',	NULL,			N'Time',		@dayUnit);

	--INSERT INTO @R1IndexedIds([Index], [Id])
	EXEC [api].[Resources__Save]
		@Entities = @R1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Inserting Resources'
		GOTO Err_Label;
	END;
	IF @DebugResources = 1
	BEGIN
		INSERT INTO @R1Ids SELECT [Id] FROM dbo.Resources; --@R1IndexedIds;
		SELECT * FROM rpt.Resources(@R1Ids);
	END
END
BEGIN -- Updating
	INSERT INTO @R2 (
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode], [ValueMeasure],
		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	)
	SELECT
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode], [ValueMeasure],
		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	FROM [dbo].Resources
	WHERE [ResourceType] IN (N'MotorVehicles', N'SKD')

	UPDATE @R2
	SET 
		[Name] = [Name] + N' - (New)'

	EXEC [api].[Resources__Save]
		@Entities = @R2,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Updating Resources'
		GOTO Err_Label;
	END
	IF @DebugResources = 1
	BEGIN
		INSERT INTO @R2Ids SELECT [Id] FROM dbo.Resources;
		SELECT * FROM rpt.Resources (@R2Ids);
	END
END
BEGIN -- Deleting
	INSERT INTO @R3 (
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode], [ValueMeasure],
		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	)
	SELECT
		[Id], [UnitId], [ResourceType], [Name], [Code], [SystemCode], [ValueMeasure],
		[CurrencyId], [MassUnitId]	, [VolumeUnitId], [AreaUnitId], [LengthUnitId], [TimeUnitId], [CountUnitId]
	FROM [dbo].Resources
	WHERE [Name] = N'Fake';

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
		SELECT * FROM rpt.Resources (@R3Ids);
	END
END 

SELECT 
	@ETB = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'ETB'), 
	@USD = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'USD'),
	@Camry2018 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Toyota Camry 2018'),
	@Car1 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'101'),
	@Car2 = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'102'),
	@Car1Svc = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'101D'),
	@GOff = (SELECT [Id] FROM [dbo].[Resources] WHERE [Code] = N'Goff'),
	@Cotton = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Cotton'),
	@TeddyBear = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Teddy bear'),
	@CommonStock = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Common Stock'),
	@HOvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'HolidayOvertime'),
	@ROvertime = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'RestOvertime'),
	@Basic = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Basic'),
	@Transportation = (SELECT [Id] FROM [dbo].[Resources] WHERE [Name] = N'Transportation'),
	@LaborHourly = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborHourly'),
	@LaborDaily = (SELECT [Id] FROM [dbo].[Resources] WHERE [SystemCode] = N'LaborDaily');