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

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	SET @DefinitionId = N'paper-types'
	INSERT INTO @Lookups([Index],
	[Name],						[Name2]) VALUES
	(0,	N'Bond paper',			N'ورق طباعة'),
	(1,	N'Gloss coated paper',	N'ورق لامع'),
	(2,	N'Matt coated paper',	N'ورق مطفى'),
	(3,	N'Recycled paper',		N'ورق معاد تدويره'),
	(4,	N'Silk coated paper',	N'ورق مصقول ناعم'),
	(5,	N'Uncoated paper',		N'ورق غير مصقول'),
	(6,	N'Watermarked paper',	N'ورق علامة مائية')
	;

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DELETE FROM @Lookups;
	SET @DefinitionId = N'paper-sizes'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'A0 (841 x 1189 mm)'),
	(1,	N'A1 (594 x 841 mm)'),
	(2,	N'A2 (420 x 594 mm)'),
	(3,	N'A3 (297 x 420 mm)'),
	(4,	N'A4 (210 x 297 mm)'),
	(5,	N'A5 (148 x 210 mm)'),
	(6,	N'A6 (105 x 148 mm)'),
	(7,	N'A7 (74 x 105 mm)'),
	(8,	N'A8 (52 x 74 mm)'),
	(9,	N'A9 (37 x 52 mm)')	
	;

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DELETE FROM @Lookups;
	SET @DefinitionId = N'paper-weights'
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'35 gsm'),
	(1,	N'55 gsm'),
	(2,	N'90 gsm'),
	(3,	N'130 gsm'),
	(4,	N'180 gsm'),
	(5,	N'250 gsm'),
	(6,	N'280 gsm'),
	(7,	N'350 gsm')
	;


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