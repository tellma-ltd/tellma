CREATE PROCEDURE [bll].[GeneralSettings_Validate__Save]
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
	@Enforce2faOnLocalAccounts BIT = NULL,
	@EnforceNoExternalAccounts BIT = NULL,
	@Top INT = 200,
	@IsError BIT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
	-- Set @IsError
	SET @IsError = CASE WHEN EXISTS(SELECT 1 FROM @ValidationErrors) THEN 1 ELSE 0 END;

	SELECT TOP (@Top) * FROM @ValidationErrors;
END;