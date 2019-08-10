CREATE PROCEDURE [dbo].[api_ResponsibilityCenters__Save]
	@Entities [ResponsibilityCenterList] READONLY,
	@ReturnIds BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [dbo].[bll_ResponsibilityCenters_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_ResponsibilityCenters__Save]
		@Entities = @Entities;
END;