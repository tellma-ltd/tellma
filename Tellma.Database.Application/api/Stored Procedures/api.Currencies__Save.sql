CREATE PROCEDURE [api].[Currencies__Save]
	@Entities [CurrencyList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	-- Add here Code that is handled by C#

	EXEC [bll].[Currencies_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[Currencies__Save]
		@Entities = @Entities
END;