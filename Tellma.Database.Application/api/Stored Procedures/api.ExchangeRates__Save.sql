CREATE PROCEDURE [api].[ExchangeRates__Save]
	@Entities [ExchangeRateList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	EXEC [bll].[ExchangeRates_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[ExchangeRates__Save]
		@Entities = @Entities,
		@ReturnIds = @ReturnIds;
END