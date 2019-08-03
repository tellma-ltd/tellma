CREATE PROCEDURE [dbo].[api_Resources__Save]
	@Entities [dbo].[ResourceList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [dbo].[bll_Resources_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_Resources__Save]
		@Resources = @Entities;
END;