CREATE PROCEDURE [api].[MeasurementUnits__Save]
	@Entities [MeasurementUnitList] READONLY,
	@ReturnEntities BIT = 0,
	@ValidationErrorsJson NVARCHAR(MAX) OUTPUT
AS
BEGIN
SET NOCOUNT ON;
-- Validate
	EXEC [bll].[MeasurementUnits_Validate__Save]
		@Entities = @Entities,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[MeasurementUnits__Save]
		@Entities = @Entities, @ReturnEntities = @ReturnEntities
END;