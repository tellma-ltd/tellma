BEGIN -- Cleanup & Declarations
	DECLARE @MU1 [dbo].MeasurementUnitList, @MU2 [dbo].MeasurementUnitList, @MU3 [dbo].MeasurementUnitList,
			@MUIndexedIds dbo.[IndexedIdList];
	DECLARE @ETBUnit INT, @USDUnit INT, @eaUnit INT, @pcsUnit INT, @shareUnit INT, @kgUnit INT, @LiterUnit INT,
			@wmoUnit INT, @hrUnit INT, @yrUnit INT, @dayUnit INT, @moUnit INT;
END
BEGIN -- Inserting
	INSERT INTO @MU1 (
	[Name], [UnitType], [Description], [UnitAmount], [BaseAmount], [Code]) VALUES
	(N'pack-6', N'Count', N'Pack of 6', 1, 6, NULL),
	(N'dozen', N'Count', N'Dozen', 1, 12, NULL);


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
	(N'AED', N'MonetaryValue', N'AE Dirhams', 3.67, 1, N'AED'),
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
WHERE [UnitType] = N'Distance';

-- Calling Delete API
INSERT INTO @MUIndexedIds([Index], [Id]) SELECT [Index], [Id] FROM @MU3;
EXEC [api].[MeasurementUnits__Delete]
	@IndexedIds = @MUIndexedIds,
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
	@LiterUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'ltr'),
	@pcsUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'pcs'),
	@eaUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'ea'),
	@shareUnit = (SELECT [Id] FROM [dbo].MeasurementUnits WHERE [Name] = N'share'),
	@wmoUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'wmo'),
	@hrUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'hr'),
	@yrUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'yr'),
	@dayUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'd'),
	@moUnit = (SELECT [Id] FROM [dbo].MeasurementUnits	WHERE [Name] = N'mo');