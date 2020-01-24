/*
NOTE: DEFINITIONS ARE IN A DIFFERENT FILE. THIS IS THE FILE FOR RECORDS ONLY

'101' -- Banan SD, USD, en
'102' -- Banan ET, ETB, en
'103' -- Lifan Cars, SAR, en/ar/zh
'104' -- Walia Steel, ETB, en/am
*/
DECLARE @Lookups dbo.LookupList, @DefinitionId NVARCHAR(50);
IF @DB = N'100' -- ACME, USD, en/ar/zh
BEGIN
	SET @DefinitionId = N'body-colors'
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,N'Black',		N'أسود'),
	(1,N'White',		N'أبيض'),
	(2,N'Silver',		N'فضي'),
	(3,N'Navy Blue',	N'أزرق');
END

ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	SET @DefinitionId = N'it-equipment-manufacturers'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Dell'),
	(1,	N'HP'),
	(2,	N'Apple'),
	(3,	N'Microsoft');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lookups: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;						

	DELETE FROM @Lookups;
	SET @DefinitionId = N'operating-systems'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Windows XP'),
	(1,	N'Windows 10'),
	(2,	N'Windows Server 2017'),
	(3,	N'iOS 13');
END

ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	SET @DefinitionId = N'it-equipment-manufacturers'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Dell'),
	(1,	N'HP'),
	(2,	N'Apple'),
	(3,	N'Microsoft');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL 
	BEGIN
		Print 'Lookups: Inserting: ' + @ValidationErrorsJson
		GOTO Err_Label;
	END;						

	DELETE FROM @Lookups;
	SET @DefinitionId = N'operating-systems'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Windows XP'),
	(1,	N'Windows 10'),
	(2,	N'Windows Server 2017'),
	(3,	N'iOS 13');
END

ELSE IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	SET @DefinitionId = N'body-colors'
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,N'Black',		N'أزرق'),
	(1,N'White',		N'أبيض'),
	(2,N'Silver',		N'فضي'),
	(3,N'Navy Blue',	N'أزرق');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DELETE FROM @Lookups;
	SET @DefinitionId = N'vehicle-makes'
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,	N'Toyota',	N'تويوتا'),
	(1,	N'Mercedes',N'مرسيدس'),
	(2,	N'Honda',	N'هوندا'),
	(3,	N'BMW',		N'بي أم دبليو');
END

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	SET @DefinitionId = N'steel-thicknesses'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'0.3'),
	(1,	N'0.4'),
	(2,	N'0.7'),
	(3,	N'1.2'),
	(4,	N'1.4'),
	(5,	N'1.9');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DELETE FROM @Lookups;
	SET @DefinitionId = N'vehicle-makes'
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,	N'Toyota',	N'تويوتا'),
	(1,	N'Mercedes',N'مرسيدس'),
	(2,	N'Honda',	N'هوندا'),
	(3,	N'BMW',		N'بي أم دبليو');
END

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

IF @ValidationErrorsJson IS NOT NULL 
BEGIN
	Print 'Lookups: Inserting: ' + @ValidationErrorsJson
	GOTO Err_Label;
END;						

IF @DebugLookups = 1
	SELECT * FROM map.Lookups();