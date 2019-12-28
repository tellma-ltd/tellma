CREATE PROCEDURE [api].[Settings__Save]
	@ShortCompanyName NVARCHAR(255),
	@PrimaryLanguageId NVARCHAR(255),
	@SecondaryLanguageId NVARCHAR(255),
	@TernaryLanguageId NVARCHAR(255),
	@DefinitionsVersion UNIQUEIDENTIFIER,
	@SettingsVersion UNIQUEIDENTIFIER,
	@FunctionalCurrencyId NCHAR(3),
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
-- Validate
	INSERT INTO @ValidationErrors
	EXEC [bll].[Settings_Validate__Save]
		@ShortCompanyName = @ShortCompanyName,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@TernaryLanguageId = @TernaryLanguageId,
		@DefinitionsVersion = @DefinitionsVersion,
		@SettingsVersion = @SettingsVersion,
		@FunctionalCurrencyId = @FunctionalCurrencyId;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);
-- TODO: use Setting data type not Table type
	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	
	EXEC [dal].[Settings__Save]
		@ShortCompanyName = @ShortCompanyName,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@TernaryLanguageId = @TernaryLanguageId,
		@DefinitionsVersion = @DefinitionsVersion,
		@SettingsVersion = @SettingsVersion,
		@FunctionalCurrencyId = @FunctionalCurrencyId;
END;