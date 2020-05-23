
IF @DB = N'100' -- ACME, USD, en/ar/zh playground
BEGIN
	SET @ShortCompanyName2 = N'أكمي إنترناشيونال';
	SET @ShortCompanyName3= N'ACME国际';
END
IF @DB = N'101' -- Banan SD, USD, en
BEGIN
	SET @ShortCompanyName2 = N'بنان السودان';
END
IF @DB = N'102' -- Banan ET, ETB, en
BEGIN
	PRINT N'Tellma.' + @DB;
END
IF @DB = N'103' -- Lifan Cars, ETB, en/zh
BEGIN
	SET @ShortCompanyName2 = N'扬帆汽车'
END
IF @DB = N'104' -- Walia Steel, ETB, en/am
BEGIN
	SET @ShortCompanyName2 = N'ዋልያ ብረት ኢንዱስትሪ'
END
IF @DB = N'105' -- Simpex, SAR, en/ar
BEGIN
	SET @ShortCompanyName2 = N'سيمبكس'
END
IF @DB = N'106' -- Walia Steel, ETB, en/am
BEGIN
	SET @ShortCompanyName2 = N'ሶሬቲ ትሬዲንግ'
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