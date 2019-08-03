BEGIN -- Cleanup & Declarations
	DECLARE @MeasurementUnitsDTO [dbo].MeasurementUnitList, @MeasurementUnitsIds dbo.[IndexedUuidList];
	DECLARE @ETBUnit INT, @USDUnit INT, @eaUnit INT, @pcsUnit INT, @shareUnit INT, @kgUnit INT,
			@wmoUnit INT, @hrUnit INT, @yrUnit INT, @dayUnit INT, @moUnit INT;
END
BEGIN -- Inserting
	INSERT INTO @MeasurementUnitsDTO (
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

	EXEC [dbo].[api_MeasurementUnits__Save]
		@Entities = @MeasurementUnitsDTO,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'MeasurementUnits: Place 1'
		GOTO Err_Label;
	END
END

-- Display units whose code starts with m
DELETE FROM @MeasurementUnitsDTO;
INSERT INTO @MeasurementUnitsDTO ([Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount], [IsDirty])
SELECT [Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount], 0
FROM [dbo].MeasurementUnits
WHERE [Name] Like 'm%';

-- Inserting
DECLARE @TestingValidation bit = 0
IF (@TestingValidation = 1)
INSERT INTO @MeasurementUnitsDTO
	([Name], [UnitType], [Description], [UnitAmount], [BaseAmount], [Code]) Values
	(N'AED', N'Money', N'AE Dirhams', 3.67, 1, N'AED'),
	(N'c', N'Time', N'Century', 1, 3110400000, NULL),
	(N'dozen', N'Count', N'Dazzina', 1, 12, NULL);
-- Updating
UPDATE @MeasurementUnitsDTO 
SET 
--	[Name] = N'pcs',
	[Description] = N'Metric Ton',
	[IsDirty] = 1
WHERE [Name] = N'mt';

DELETE FROM @MeasurementUnitsDTO WHERE [IsDirty] = 0;-- [EntityState] = N'Unchanged';
-- Calling Save API
EXEC [dbo].[api_MeasurementUnits__Save]
	@Entities = @MeasurementUnitsDTO,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT

IF @ValidationErrorsJson IS NOT NULL
BEGIN
	Print 'MeasurementUnits: Place 2'
	GOTO Err_Label;
END

DELETE FROM @MeasurementUnitsDTO;
INSERT INTO @MeasurementUnitsDTO ([Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount], [IsDirty])
SELECT [Id], [Code], [UnitType], [Name], [Description], [UnitAmount], [BaseAmount], 0
FROM [dbo].MeasurementUnits
WHERE [Name] Like 'm%';

-- Calling Delete API
INSERT INTO @MeasurementUnitsIds([Index], [Id]) SELECT [Index], [Id] FROM @MeasurementUnitsDTO
EXEC [dbo].[api_MeasurementUnits__Delete]
	@Ids = @MeasurementUnitsIds,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT

IF @ValidationErrorsJson IS NOT NULL
BEGIN
	Print 'MeasurementUnits: Place 3'
	GOTO Err_Label;
END

	SELECT MU.Code, MU.[Name], MU.[Description], MU.BaseAmount, MU.IsActive, 
	LUC.[Name] AS CreatedBy, MU.CreatedAt, LUM.[Name] AS ModifiedBy, MU.ModifiedAt, IsDeleted
	FROM [dbo].MeasurementUnits MU
	JOIN dbo.[Users] LUC ON MU.CreatedById = LUC.Id
	JOIN dbo.[Users] LUM ON MU.ModifiedById = LUM.Id
--	WHERE IsDeleted = 0;

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
