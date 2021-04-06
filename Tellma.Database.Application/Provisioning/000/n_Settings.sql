EXEC [api].[GeneralSettings__Save]
	@ShortCompanyName = @ShortCompanyName,
	@ShortCompanyName2 = @ShortCompanyName2,
	@ShortCompanyName3 = @ShortCompanyName3,
	@PrimaryLanguageId = @PrimaryLanguageId,
	@SecondaryLanguageId = @SecondaryLanguageId,
	@TernaryLanguageId = @TernaryLanguageId,
	@PrimaryCalendar = N'GC',
	@SecondaryCalendar = N'ET',
	@DateFormat =NULL,
	@TimeFormat =NULL,
	@BrandColor	= @BrandColor,
	@ValidationErrorsJson = @ValidationErrorsJson;