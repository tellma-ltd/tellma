IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	PRINT N'Tellma.' + @DB;

END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	PRINT N'Tellma.' + @DB;

END
IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	PRINT N'Tellma.' + @DB;

END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	PRINT N'Tellma.' + @DB;
END
IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	SET @ShortCompanyName2 = N'سيمبكس'
END

EXEC [api].[Settings__Save]
	@ShortCompanyName = @ShortCompanyName,
	@ShortCompanyName2 = @ShortCompanyName2,
	@ShortCompanyName3 = @ShortCompanyName3,
	@PrimaryLanguageId = @PrimaryLanguageId,
	@SecondaryLanguageId = @SecondaryLanguageId,
	@TernaryLanguageId = @TernaryLanguageId,
	@DefinitionsVersion = @DefinitionsVersion,
	@SettingsVersion = @SettingsVersion,
	@FunctionalCurrencyId = @FunctionalCurrencyId,
	@ValidationErrorsJson = @ValidationErrorsJson;