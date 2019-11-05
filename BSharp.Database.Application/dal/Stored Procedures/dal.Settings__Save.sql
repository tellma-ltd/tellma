CREATE PROCEDURE [dal].[Settings__Save]
	@ShortCompanyName NVARCHAR(255),
	@PrimaryLanguageId NVARCHAR(255),
	@DefinitionsVersion UNIQUEIDENTIFIER,
	@SettingsVersion UNIQUEIDENTIFIER,
	@FunctionalCurrencyId NCHAR(3)
AS
SET NOCOUNT ON;
	DECLARE @Now DATETIMEOFFSET(7) = SYSDATETIMEOFFSET();
	DECLARE @UserId INT = CONVERT(INT, SESSION_CONTEXT(N'UserId'));

IF Exists(SELECT * FROM dbo.Settings)
	UPDATE dbo.[Settings]
	SET 
		[ShortCompanyName]		= @ShortCompanyName,
		[PrimaryLanguageId]		= @PrimaryLanguageId,
		[DefinitionsVersion]	= @DefinitionsVersion,
		[SettingsVersion]		= @SettingsVersion,
		[FunctionalCurrencyId]	= @FunctionalCurrencyId,
		[ModifiedAt]			= @Now,
		[ModifiedById]			= @UserId
ELSE
	INSERT dbo.[Settings] ([ShortCompanyName], [PrimaryLanguageId], [DefinitionsVersion], [SettingsVersion], [FunctionalCurrencyId])
	VALUES(@ShortCompanyName, @PrimaryLanguageId, @DefinitionsVersion, @SettingsVersion, @FunctionalCurrencyId);