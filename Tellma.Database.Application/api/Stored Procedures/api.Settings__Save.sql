CREATE PROCEDURE [api].[Settings__Save]
	@ShortCompanyName NVARCHAR(255),
	@ShortCompanyName2 NVARCHAR(255) = NULL,
	@ShortCompanyName3 NVARCHAR(255) = NULL,
	@FunctionalCurrencyId NCHAR(3),
	@PrimaryLanguageId NVARCHAR(255),
	@PrimaryLanguageSymbol NVARCHAR (5) = NULL,
	@SecondaryLanguageId NVARCHAR(255) = NULL,
	@SecondaryLanguageSymbol NVARCHAR (5) = NULL,
	@TernaryLanguageId NVARCHAR(255) = NULL,
	@TernaryLanguageSymbol NVARCHAR (5) = NULL,
	@BrandColor NCHAR (7) = NULL,
	@DefinitionsVersion UNIQUEIDENTIFIER,
	@SettingsVersion UNIQUEIDENTIFIER,
	@ArchiveDate DATE = '1900.01.01',
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	EXEC [bll].[Settings_Validate__Save]
		@ShortCompanyName = @ShortCompanyName,
		@ShortCompanyName2 = @ShortCompanyName2,
		@ShortCompanyName3 = @ShortCompanyName3,
		@FunctionalCurrencyId = @FunctionalCurrencyId,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@PrimaryLanguageSymbol = @PrimaryLanguageSymbol,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@SecondaryLanguageSymbol = @SecondaryLanguageSymbol,
		@TernaryLanguageId = @TernaryLanguageId,
		@TernaryLanguageSymbol = @TernaryLanguageSymbol,
		@BrandColor = @BrandColor,
		@DefinitionsVersion = @DefinitionsVersion,
		@SettingsVersion = @SettingsVersion,
		@ArchiveDate = @ArchiveDate,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	
	EXEC [dal].[Settings__Save]
		@ShortCompanyName = @ShortCompanyName,
		@ShortCompanyName2 = @ShortCompanyName2,
		@ShortCompanyName3 = @ShortCompanyName3,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@TernaryLanguageId = @TernaryLanguageId,
		@DefinitionsVersion = @DefinitionsVersion,
		@SettingsVersion = @SettingsVersion,
		@FunctionalCurrencyId = @FunctionalCurrencyId;
END;