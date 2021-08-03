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
	@PrimaryCalendar NVARCHAR (2) = NULL,
	@SecondaryCalendar NVARCHAR (2) = NULL,
	@DateFormat NVARCHAR (50) = NULL,
	@TimeFormat NVARCHAR (50) = NULL,
	@BrandColor NCHAR (7) = NULL,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50) = N'en',
	@NeutralCulture NVARCHAR(50) = N'en'
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;

	DECLARE @IsError BIT;
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
		@PrimaryCalendar = @PrimaryCalendar,
		@SecondaryCalendar =@SecondaryCalendar,
		@DateFormat =@DateFormat,
		@TimeFormat =@TimeFormat,
		@BrandColor = @BrandColor,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
	
	EXEC [dal].[GeneralSettings__Save]
		@ShortCompanyName = @ShortCompanyName,
		@ShortCompanyName2 = @ShortCompanyName2,
		@ShortCompanyName3 = @ShortCompanyName3,
		@PrimaryLanguageId = @PrimaryLanguageId,
		@PrimaryLanguageSymbol = @PrimaryLanguageSymbol,
		@SecondaryLanguageId = @SecondaryLanguageId,
		@SecondaryLanguageSymbol = @SecondaryLanguageSymbol,
		@TernaryLanguageId = @TernaryLanguageId,
		@TernaryLanguageSymbol = @TernaryLanguageSymbol,
		@PrimaryCalendar = @PrimaryCalendar,
		@SecondaryCalendar = @SecondaryCalendar,
		@DateFormat = @DateFormat,
		@TimeFormat = @TimeFormat,
		@BrandColor = @BrandColor,
		@UserId = @UserId;
END;