BEGIN -- Cleanup & Declarations
	DECLARE @MU1 [dbo].MeasurementUnitList, @MU2 [dbo].MeasurementUnitList, @MU3 [dbo].MeasurementUnitList,
			@MeasurementUnitsIds dbo.[IndexedIdList];
	DECLARE @ETBUnit INT, @USDUnit INT, @eaUnit INT, @pcsUnit INT, @shareUnit INT, @kgUnit INT,
			@wmoUnit INT, @hrUnit INT, @yrUnit INT, @dayUnit INT, @moUnit INT;
END
BEGIN -- Inserting
	INSERT INTO @MU1 (
	[Name], [UnitType], [Description], [UnitAmount], [BaseAmount], [Code]) VALUES
	(N'AED', N'Money', N'UAE Dirham', 3.67, 1, N'AED'),
	(N'd', N'Time', N'Day', 1, 86400, NULL),
	(N'dozen', N'Count', N'Dozen', 1, 12, NULL),
	(N'ea', N'Pure', N'Each', 1, 1, NULL),
	(N'ETB', N'Money', N'Ethiopian Birr', 27.8, 1, N'ETB'),
	(N'g', N'Mass', N'Gram', 1, 1, NULL),
	(N'hr', N'Time', N'Hour', 1, 3600, NULL),
	(N'in', N'Distance', N'inch', 1, 2.541, NULL),
	(N'kg', N'Mass', N'Kilogram', 1, 1000, NULL),
	(N'ltr', N'Volume', N'Liter', 1, 1, NULL),
	(N'm', N'Distance', N'meter', 1, 1, NULL),
	(N'min', N'Time', N'minute', 1, 60, NULL),
	(N'mo', N'Time', N'Month', 1, 2592000, NULL),
	(N'mt', N'Mass', N'Metric ton', 1, 1000000, NULL),
	(N'pcs', N'Count', N'Pieces', 1, 1, NULL),
	(N's', N'Time', N'second', 1, 1, NULL),
	(N'share', N'Pure', N'Shares', 1, 1, NULL),
	(N'USD', N'Money', N'US Dollar', 1, 1, N'USD'),
	(N'usg', N'Volume', N'US Gallon', 1, 3.785411784, NULL),
	(N'wd', N'Time', N'work day', 1, 8, NULL),
	(N'wk', N'Time', N'week', 1, 604800, NULL),
	(N'wmo', N'Time', N'work month', 1, 1248, NULL),
	(N'wwk', N'Time', N'work week', 1, 48, NULL),
	(N'wyr', N'Time', N'work year', 1, 14976, NULL),
	(N'yr', N'Time', N'Year', 1, 31104000, NULL);

	EXEC [api].[MeasurementUnits__Save]
		@Entities = @MU1,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT

	--SELECT * FROM @MU1;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'MeasurementUnits: Inserting'
		GOTO Err_Label;
	END
END

-- Display units whose code starts with m
INSERT INTO @MU2 ([Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount])
SELECT [Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount]
FROM [dbo].MeasurementUnits
WHERE [Name] Like 'm%';
SET @RowCount = @@ROWCOUNT;

-- Inserting
DECLARE @TestingValidation bit = 0
IF (@TestingValidation = 1)
INSERT INTO @MU2
	([Name], [UnitType], [Description], [UnitAmount], [BaseAmount], [Code]) Values
	(N'AED', N'Money', N'AE Dirhams', 3.67, 1, N'AED'),
	(N'c', N'Time', N'Century', 1, 3110400000, NULL),
	(N'dozen', N'Count', N'Dazzina', 1, 12, NULL);
-- Updating
UPDATE @MU2 
SET 
--	[Name] = N'pcs',
	[Description] = N'Metric Ton' -- Capitalizing the letter T
WHERE [Name] = N'mt';

-- SELECT * FROM @MU2;
DELETE FROM @MU2 WHERE [Name] Like 'm%' AND [Name] <> N'mt';
-- Calling Save API
EXEC [api].[MeasurementUnits__Save]
	@Entities = @MU2,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL
BEGIN
	Print 'MeasurementUnits: Updating'
	GOTO Err_Label;
END

INSERT INTO @MU3 ([Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount])
SELECT [Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount]
FROM [dbo].MeasurementUnits
WHERE [Name] Like 'm%';

-- Calling Delete API
INSERT INTO @MeasurementUnitsIds([Index], [Id]) SELECT [Index], [Id] FROM @MU3;
EXEC [api].[MeasurementUnits__Delete]
	@Ids = @MeasurementUnitsIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT

--SELECT * FROM [dbo].[fs_MeasurementUnits]();

IF @ValidationErrorsJson IS NOT NULL
BEGIN
	Print 'MeasurementUnits: Deleting'
	GOTO Err_Label;
END

SELECT
	@ETBUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'ETB'),
	@USDUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'USD'),
	@KgUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'Kg'),
	@pcsUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'pcs'),
	@eaUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'ea'),
	@shareUnit = (SELECT [Id] FROM [dbo].MeasurementUnits WHERE [Name] = N'share'),
	@wmoUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'wmo'),
	@hrUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'hr'),
	@yrUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'yr'),
	@dayUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'd'),
	@moUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'mo');