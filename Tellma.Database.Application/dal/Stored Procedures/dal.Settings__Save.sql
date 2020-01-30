CREATE PROCEDURE [dal].[Settings__Save]
	@ShortCompanyName NVARCHAR(255),
	@ShortCompanyName2 NVARCHAR(255) = NULL,
	@ShortCompanyName3 NVARCHAR(255) = NULL,
	@PrimaryLanguageId NVARCHAR(255),
	@SecondaryLanguageId NVARCHAR(255),
	@TernaryLanguageId NVARCHAR(255),
	@DefinitionsVersion UNIQUEIDENTIFIER,
	@SettingsVersion UNIQUEIDENTIFIER,
	@FunctionalCurrencyId NCHAR(3)
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));
	DECLARE @PrimaryLanguageSymbol NVARCHAR (5) = dbo.fn_LanguageId__Symbol(@PrimaryLanguageId);
	DECLARE @SecondaryLanguageSymbol NVARCHAR (5) = dbo.fn_LanguageId__Symbol(@SecondaryLanguageId);
	DECLARE @TernaryLanguageSymbol NVARCHAR (5) = dbo.fn_LanguageId__Symbol(@TernaryLanguageId);
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
		[DefinitionsVersion]	= @DefinitionsVersion,
		[SettingsVersion]		= @SettingsVersion,
		[FunctionalCurrencyId]	= @FunctionalCurrencyId,
		[ModifiedAt]			= @Now,
		[ModifiedById]			= @UserId
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
		[DefinitionsVersion], 
		[SettingsVersion], 
		[FunctionalCurrencyId])
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
		@DefinitionsVersion, 
		@SettingsVersion, 
		@FunctionalCurrencyId);