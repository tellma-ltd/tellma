IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	PRINT N'BSharp.' + @DB;

END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	PRINT N'BSharp.' + @DB;

END
IF @DB = N'103' -- Lifan Cars, SAR, en/ar/zh
BEGIN
	PRINT N'BSharp.' + @DB;

END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	PRINT N'BSharp.' + @DB;
END

EXEC [api].[Settings__Save]
	@ShortCompanyName = @ShortCompanyName,
	@PrimaryLanguageId = @PrimaryLanguageId,
	@SecondaryLanguageId = @SecondaryLanguageId,
	@TernaryLanguageId = @TernaryLanguageId,
	@DefinitionsVersion = @DefinitionsVersion,
	@SettingsVersion = @SettingsVersion,
	@FunctionalCurrencyId = @FunctionalCurrencyId,
	@ValidationErrorsJson = @ValidationErrorsJson;