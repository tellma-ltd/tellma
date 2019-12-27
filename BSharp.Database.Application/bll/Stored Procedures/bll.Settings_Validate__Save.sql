CREATE PROCEDURE [bll].[Settings_Validate__Save]
	@ShortCompanyName NVARCHAR(255),
	@PrimaryLanguageId NVARCHAR(255),
	@SecondaryLanguageId NVARCHAR(255),
	@TernaryLanguageId NVARCHAR(255),
	@DefinitionsVersion UNIQUEIDENTIFIER,
	@SettingsVersion UNIQUEIDENTIFIER,
	@FunctionalCurrencyId NCHAR(3),
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

 	SELECT TOP (@Top) * FROM @ValidationErrors;