CREATE PROCEDURE [dbo].[api_ProductCategories__Save]
	@Entities [ProductCategoryList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [dbo].[bll_ProductCategories_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_ProductCategories__Save]
		@Entities = @Entities;
END;