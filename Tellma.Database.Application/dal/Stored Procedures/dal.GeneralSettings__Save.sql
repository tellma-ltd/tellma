CREATE PROCEDURE [dal].[GeneralSettings__Save]
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
	@BrandColor NCHAR (7) = NULL
	-- Financial Settings
	--@FunctionalCurrencyId NCHAR(3),
	--@ArchiveDate DATE = '1900.01.01'
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	SET @PrimaryLanguageSymbol = ISNULL(@PrimaryLanguageSymbol, dbo.fn_LanguageId__Symbol(@PrimaryLanguageId));
	SET @SecondaryLanguageSymbol = ISNULL(@SecondaryLanguageSymbol, dbo.fn_LanguageId__Symbol(@SecondaryLanguageId));
	SET @TernaryLanguageSymbol = ISNULL(@TernaryLanguageSymbol, dbo.fn_LanguageId__Symbol(@TernaryLanguageId));
	SET @PrimaryCalendar = ISNULL(@PrimaryCalendar, N'GC');
	SET @DateFormat = ISNULL(@DateFormat, N'yyyy-MM-dd');
	SET @TimeFormat = ISNULL(@TimeFormat, N'HH:mm:ss');

IF Exists(SELECT * FROM dbo.Settings)
	UPDATE dbo.[Settings]
	SET 
		[ShortCompanyName]		= @ShortCompanyName,
		[ShortCompanyName2]		= @ShortCompanyName2,
		[ShortCompanyName3]		= @ShortCompanyName3,
		[PrimaryLanguageId]		= @PrimaryLanguageId,
		[PrimaryLanguageSymbol] = @PrimaryLanguageSymbol,
		[SecondaryLanguageId]	= @SecondaryLanguageId,
		[SecondaryLanguageSymbol] = @SecondaryLanguageSymbol,
		[TernaryLanguageId]		= @TernaryLanguageId,
		[TernaryLanguageSymbol] = @TernaryLanguageSymbol,
		[PrimaryCalendar]		= ISNULL(@PrimaryCalendar, [PrimaryCalendar]),
		[SecondaryCalendar]		= @SecondaryCalendar,
		[DateFormat]			= ISNULL(@DateFormat, [DateFormat]),
		[TimeFormat]			= ISNULL(@TimeFormat, [TimeFormat]),
		[BrandColor]			= @BrandColor,
		[SettingsVersion]		= NEWID(), -- To trigger cache refresh
		[GeneralModifiedAt]		= @Now,
		[GeneralModifiedById]	= @UserId
ELSE
	INSERT dbo.[Settings] (
		[ShortCompanyName],
		[ShortCompanyName2],
		[ShortCompanyName3],
		[PrimaryLanguageId],
		[PrimaryLanguageSymbol],
		[SecondaryLanguageId],
		[SecondaryLanguageSymbol],
		[TernaryLanguageId],
		[TernaryLanguageSymbol],
		[PrimaryCalendar],
		[SecondaryCalendar],
		[DateFormat],
		[TimeFormat],
		[BrandColor]
	)
	VALUES(
		@ShortCompanyName,
		@ShortCompanyName2,
		@ShortCompanyName3,
		@PrimaryLanguageId,
		@PrimaryLanguageSymbol,
		@SecondaryLanguageId,
		@SecondaryLanguageSymbol,
		@TernaryLanguageId,
		@TernaryLanguageSymbol,
		@PrimaryCalendar,
		@SecondaryCalendar,
		@DateFormat,
		@TimeFormat,
		@BrandColor
	);