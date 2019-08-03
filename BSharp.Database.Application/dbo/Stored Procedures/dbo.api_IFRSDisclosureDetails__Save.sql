CREATE PROCEDURE [dbo].[api_IfrsDisclosureDetails__Save]
	@Entities [IfrsDisclosureDetailList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [dbo].[bll_IfrsDisclosureDetails_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;
	
	EXEC [dbo].[dal_IfrsDisclosureDetails__Save]
		@Entities = @Entities;
END;