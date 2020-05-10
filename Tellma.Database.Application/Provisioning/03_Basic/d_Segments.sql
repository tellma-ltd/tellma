DECLARE @Segments dbo.LookupList;

IF @DB = N'100' -- ACME, USD, en/ar/zh
 PRINT @DB
ELSE IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	INSERT INTO @Segments([Index],
				[Name],				[Name2],			[Code])
	SELECT 0,	[ShortCompanyName],	[ShortCompanyName2],N''
	FROM dbo.Settings

END
ELSE IF @DB = N'102' -- Banan ET, ETB, en
INSERT INTO @Segments([Index],
			[Name],				[Code])
SELECT 0,[ShortCompanyName],	N''
FROM dbo.Settings

ELSE IF @DB = N'103' -- Lifan Cars, ETB, en/zh
INSERT INTO @Segments([Index],
			[Name],				[Name2],			[Code])
SELECT 0,[ShortCompanyName],[ShortCompanyName2],	N''
FROM dbo.Settings

ELSE IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	INSERT INTO @Segments([Index],
				[Name],				[Name2],			[Code])
	SELECT 0,[ShortCompanyName],	[ShortCompanyName2],N''
	FROM dbo.Settings
END
ELSE IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	INSERT INTO @Segments([Index],
				[Name],				[Name2],			[Code])
	SELECT 0,[ShortCompanyName],[ShortCompanyName2],	N''
	FROM dbo.Settings;
END
ELSE IF @DB = N'106' -- Soreti, ETB, en/am
BEGIN
	INSERT INTO @Segments([Index],
				[Name],				[Name2],			[Code])
	SELECT 0,[ShortCompanyName],[ShortCompanyName2],	N''
	FROM dbo.Settings

END
EXEC [dal].[Segments__Save]
	@Entities = @Segments--,	@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

--IF @ValidationErrorsJson IS NOT NULL 
--BEGIN
--	Print 'Centers: Inserting: ' + @ValidationErrorsJson
--	GOTO Err_Label;
--END;

DECLARE @MAIN_OS INT = (SELECT TOP 1 [Id] FROM dbo.Segments);