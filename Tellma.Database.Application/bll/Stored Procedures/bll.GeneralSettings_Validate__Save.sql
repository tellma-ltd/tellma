CREATE PROCEDURE [bll].[GeneralSettings_Validate__Save]
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
	@Top INT = 10
AS
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];
	
 	SELECT TOP (@Top) * FROM @ValidationErrors;