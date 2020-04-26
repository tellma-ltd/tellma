/*
NOTE: DEFINITIONS ARE IN A DIFFERENT FILE. THIS IS THE FILE FOR RECORDS ONLY

'101' -- Banan SD, USD, en
'102' -- Banan ET, ETB, en
'103' -- Lifan Cars, ETB, en/zh
'104' -- Walia Steel, ETB, en/am
*/
DECLARE @Lookups dbo.LookupList, @DefinitionId INT;
IF @DB = N'100' -- ACME, USD, en/ar/zh
BEGIN
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'body-colors');
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,N'Black',		N'أسود'),
	(1,N'White',		N'أبيض'),
	(2,N'Silver',		N'فضي'),
	(3,N'Navy Blue',	N'أزرق');
END

ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	SET @DefinitionId = @it_equipment_manufacturersDef
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Dell'),
	(1,	N'HP'),
	(2,	N'Apple'),
	(3,	N'Microsoft'),
	(4, N'Lenovo');


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
	SET @DefinitionId = @operating_systemsDef
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(1,	N'Windows 10'),
	(2,	N'Windows Server 2017'),
	(3,	N'iOS 13');
END

ELSE IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	SET @DefinitionId = @it_equipment_manufacturersDef
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
	SET @DefinitionId = @operating_systemsDef
	INSERT INTO @Lookups([Index],
	[Name]) VALUES
	(0,	N'Windows XP'),
	(1,	N'Windows 10'),
	(2,	N'Windows Server 2017'),
	(3,	N'iOS 13');
END

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = 'body-colors');
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
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'vehicle-makes');
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,	N'Toyota',	N'تويوتا'),
	(1,	N'Mercedes',N'مرسيدس'),
	(2,	N'Honda',	N'هوندا'),
	(3,	N'BMW',		N'بي أم دبليو');
END

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'steel-thicknesses');
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
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'vehicle-makes');
	INSERT INTO @Lookups([Index],
	[Name],			[Name2]) VALUES
	(0,	N'Toyota',	N'تويوتا'),
	(1,	N'Mercedes',N'مرسيدس'),
	(2,	N'Honda',	N'هوندا'),
	(3,	N'BMW',		N'بي أم دبليو');
END

ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
/*
	(0,N'paper-origins',	N'Paper Origin',	N'مصدر الورق',		N'Paper Origins',	N'مصادر الورق'),
	(1,N'paper-groups',		N'Paper Group',		N'مجموعة الورق',	N'Paper Groups',	N'مجموعات الورق'),
	(2,N'paper-types',		N'Paper Type',		N'نوع الورق',		N'Paper Types',		N'أنواع الورق');
*/
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'paper-origins');
	INSERT INTO @Lookups([Index],
	[Name],				[Name2]) VALUES
	(0,	N'Thai',		N'نايلاندي'),
	(1,	N'Finnish',		N'فنلندي'),
	(2, N'German',		N'ألماني');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DELETE FROM @Lookups;
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'paper-groups');
	INSERT INTO @Lookups([Index],
	[Name],							[Name2]) VALUES
	(0,	N'Carbonless Coated paper',	N'ورق مكربن صفائح'),
	(1,	N'Newspaper Roll paper',	N'رول ورق جريدة'),
	(2,	N'Coated Paper Glazed',		N'ورق لامع صفائح');

	EXEC [api].Lookups__Save
	@DefinitionId = @DefinitionId,
	@Entities = @Lookups,
	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	DELETE FROM @Lookups;
	SET @DefinitionId = (SELECT [Id] FROM dbo.LookupDefinitions WHERE [Code] = N'paper-types');
	INSERT INTO @Lookups([Index],
	[Name],						[Name2]) VALUES
	(0,	N'Commercial',			N'ورق تجاري'),
	(1,	N'Newspaper',			N'ورق جريدة')	;

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