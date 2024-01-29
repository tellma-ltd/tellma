CREATE PROCEDURE [dal].[GeneralSettings__Save]
	@CompanyName NVARCHAR(255) = NULL,
	@CompanyName2 NVARCHAR(255) = NULL,
	@CompanyName3 NVARCHAR(255) = NULL,
	@CustomFieldsJson NVARCHAR(MAX) = NULL,
	@CountryCode NVARCHAR(2) = NULL,
	@ShortCompanyName NVARCHAR(255),
	@ShortCompanyName2 NVARCHAR(255) = NULL,
	@ShortCompanyName3 NVARCHAR(255) = NULL,
	@PrimaryLanguageId NVARCHAR(255),
	@PrimaryLanguageSymbol NVARCHAR (5) = NULL,
	@SecondaryLanguageId NVARCHAR(255) = NULL,
	@SecondaryLanguageSymbol NVARCHAR (5) = NULL,
	@TernaryLanguageId NVARCHAR(255) = NULL,
	@TernaryLanguageSymbol NVARCHAR (5) = NULL,
	@PrimaryCalendar NVARCHAR (2) = NULL,
	@SecondaryCalendar NVARCHAR (2) = NULL,
	@DateFormat NVARCHAR (50) = NULL,
	@TimeFormat NVARCHAR (50) = NULL,
	@BrandColor NCHAR (7) = NULL,
	@SupportEmails NVARCHAR (255) = NULL,
	@UserId INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();

	UPDATE [dbo].[Settings]
	SET 
		[CompanyName]			= @CompanyName,
		[CompanyName2]			= @CompanyName2,
		[CompanyName3]			= @CompanyName3,
		[CustomFieldsJson]		= @CustomFieldsJson,
		[CountryCode]			= @CountryCode,
		[ShortCompanyName]		= @ShortCompanyName,
		[ShortCompanyName2]		= @ShortCompanyName2,
		[ShortCompanyName3]		= @ShortCompanyName3,
		[PrimaryLanguageId]		= @PrimaryLanguageId,
		[PrimaryLanguageSymbol] = @PrimaryLanguageSymbol,
		[SecondaryLanguageId]	= @SecondaryLanguageId,
		[SecondaryLanguageSymbol] = @SecondaryLanguageSymbol,
		[TernaryLanguageId]		= @TernaryLanguageId,
		[TernaryLanguageSymbol] = @TernaryLanguageSymbol,
		[PrimaryCalendar]		= @PrimaryCalendar,
		[SecondaryCalendar]		= @SecondaryCalendar,
		[DateFormat]			= @DateFormat,
		[TimeFormat]			= @TimeFormat,
		[BrandColor]			= @BrandColor,
		[SupportEmails]			= @SupportEmails,
		[SettingsVersion]		= NEWID(), -- To trigger cache refresh
		[GeneralModifiedAt]		= @Now,
		[GeneralModifiedById]	= @UserId
END;