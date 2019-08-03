CREATE PROCEDURE [dbo].[api_MeasurementUnits__Save]
	@Entities [MeasurementUnitList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [dbo].[bll_MeasurementUnits_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_MeasurementUnits__Save]
		@Entities = @Entities
END;