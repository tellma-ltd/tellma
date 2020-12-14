CREATE PROCEDURE [api].[GeneralSettings__Save]
	@ShortCompanyName NVARCHAR(255),
	@ShortCompanyName2 NVARCHAR(255) = NULL,
	@ShortCompanyName3 NVARCHAR(255) = NULL,
	@PrimaryLanguageId NVARCHAR(255),
	@PrimaryLanguageSymbol NVARCHAR (5) = NULL,
	@SecondaryLanguageId NVARCHAR(255) = NULL,
	@SecondaryLanguageSymbol NVARCHAR (5) = NULL,
	@TernaryLanguageId NVARCHAR(255) = NULL,
	@TernaryLanguageSymbol NVARCHAR (5) = NULL,
	@BrandColor NCHAR (7) = NULL,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors ValidationErrorList;
	INSERT INTO @ValidationErrors
	EXEC [bll].[GeneralSettings_Validate__Save]
		@ShortCompanyName = @ShortCompanyName,
		@ShortCompanyName2 = @ShortCompanyName2,
		@ShortCompanyName3 = @ShortCompanyName3,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@PrimaryLanguageSymbol = @PrimaryLanguageSymbol,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@SecondaryLanguageSymbol = @SecondaryLanguageSymbol,
		@TernaryLanguageId = @TernaryLanguageId,
		@TernaryLanguageSymbol = @TernaryLanguageSymbol,
		@BrandColor = @BrandColor

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	
	EXEC [dal].[GeneralSettings__Save]
		@ShortCompanyName = @ShortCompanyName,
		@ShortCompanyName2 = @ShortCompanyName2,
		@ShortCompanyName3 = @ShortCompanyName3,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@TernaryLanguageId = @TernaryLanguageId,
		@BrandColor = @BrandColor
END;