CREATE PROCEDURE [dbo].[api_ResourceLooup1s__Save]
	@Entities [ResourceLookupList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [dbo].[bll_ResourceLookup1s_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_ResourceLookup1s__Save]
		@Entities = @Entities
END;