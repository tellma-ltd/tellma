CREATE PROCEDURE [dbo].[api_ProductCategories__Delete]
	@Ids [IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @ValidationErrors [dbo].[ValidationErrorList];

	INSERT INTO @ValidationErrors
	EXEC [dbo].[bll_ProductCategories_Validate__Delete]
		@Entities = @Ids;

	SELECT @ValidationErrorsJson = 
	(
		SELECT *
		FROM @ValidationErrors
		FOR JSON PATH
	);

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_ProductCategories__Delete]
		@Entities = @Ids;
END;