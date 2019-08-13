CREATE PROCEDURE [dbo].[api_ProductCategories__Delete]
	@Ids [IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;

	--INSERT INTO @ValidationErrors
	EXEC [dbo].[bll_ProductCategories_Validate__Delete]
		@Entities = @Ids,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_ProductCategories__Delete]
		@Entities = @Ids;
END;