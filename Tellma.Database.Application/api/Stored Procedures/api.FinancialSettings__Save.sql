CREATE PROCEDURE [api].[FinancialSettings__Save]
	@FunctionalCurrencyId NCHAR (3),
	@TaxIdentificationNumber NVARCHAR (50),
	@FirstDayOfPeriod TINYINT,
	@ArchiveDate DATE,
	@ValidateOnly BIT = 0,
	@Top INT = 200,
	@UserId INT,
	@Culture NVARCHAR(50),
	@NeutralCulture NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	EXEC [dbo].[SetSessionCulture] @Culture = @Culture, @NeutralCulture = @NeutralCulture;
	
	DECLARE @IsError BIT;
	EXEC [bll].[FinancialSettings_Validate__Save]
		@FunctionalCurrencyId = @FunctionalCurrencyId,
		@TaxIdentificationNumber = @TaxIdentificationNumber,
		@FirstDayOfPeriod = @FirstDayOfPeriod,
		@ArchiveDate = @ArchiveDate,
		@Top = @Top,
		@IsError = @IsError OUTPUT;

	-- If there are validation errors don't proceed
	IF @IsError = 1 OR @ValidateOnly = 1
		RETURN;
	
	EXEC [dal].[FinancialSettings__Save]
		@FunctionalCurrencyId = @FunctionalCurrencyId,
		@TaxIdentificationNumber = @TaxIdentificationNumber,
		@FirstDayOfPeriod = @FirstDayOfPeriod,
		@ArchiveDate = @ArchiveDate,
		@UserId = @UserId;
END;