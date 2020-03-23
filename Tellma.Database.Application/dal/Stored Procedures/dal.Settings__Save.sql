CREATE PROCEDURE [dal].[Settings__Save]
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
	@ArchiveDate DATE = '1900.01.01'
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	SET @PrimaryLanguageSymbol = ISNULL(@PrimaryLanguageSymbol, dbo.fn_LanguageId__Symbol(@PrimaryLanguageId));
	SET @SecondaryLanguageSymbol = ISNULL(@SecondaryLanguageSymbol, dbo.fn_LanguageId__Symbol(@SecondaryLanguageId));
	SET @TernaryLanguageSymbol = ISNULL(@TernaryLanguageSymbol, dbo.fn_LanguageId__Symbol(@TernaryLanguageId));

IF Exists(SELECT * FROM dbo.Settings)
	UPDATE dbo.[Settings]
	SET 
		[ShortCompanyName]		= @ShortCompanyName,
		[ShortCompanyName2]		= @ShortCompanyName2,
		[ShortCompanyName3]		= @ShortCompanyName3,
		[FunctionalCurrencyId]	= @FunctionalCurrencyId,
		[PrimaryLanguageId]		= @PrimaryLanguageId,
		[PrimaryLanguageSymbol] = @PrimaryLanguageSymbol,
		[SecondaryLanguageId]	= @SecondaryLanguageId,
		[SecondaryLanguageSymbol] = @SecondaryLanguageSymbol,
		[TernaryLanguageId]		= @TernaryLanguageId,
		[TernaryLanguageSymbol] = @TernaryLanguageSymbol,
		[BrandColor]			= @BrandColor,
		[DefinitionsVersion]	= @DefinitionsVersion,
		[SettingsVersion]		= @SettingsVersion,
		[ArchiveDate]			= @ArchiveDate,
		[ModifiedAt]			= @Now,
		[ModifiedById]			= @UserId
ELSE
	INSERT dbo.[Settings] (
		[ShortCompanyName],
		[ShortCompanyName2],
		[ShortCompanyName3],
		[FunctionalCurrencyId],
		[PrimaryLanguageId],
		[PrimaryLanguageSymbol],
		[SecondaryLanguageId],
		[SecondaryLanguageSymbol],
		[TernaryLanguageId],
		[TernaryLanguageSymbol],
		[BrandColor],
		[DefinitionsVersion], 
		[SettingsVersion],
		[ArchiveDate]
	)
	VALUES(
		@ShortCompanyName,
		@ShortCompanyName2,
		@ShortCompanyName3,
		@FunctionalCurrencyId,
		@PrimaryLanguageId,
		@PrimaryLanguageSymbol,
		@SecondaryLanguageId,
		@SecondaryLanguageSymbol,
		@TernaryLanguageId,
		@TernaryLanguageSymbol,
		@BrandColor,
		@DefinitionsVersion, 
		@SettingsVersion,
		@ArchiveDate
	);