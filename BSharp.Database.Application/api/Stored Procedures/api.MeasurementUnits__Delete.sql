CREATE PROCEDURE [api].[MeasurementUnits__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [bll].[MeasurementUnits_Validate__Delete]
		@Ids = @Ids,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dal].[MeasurementUnits__Delete] @Ids = @Ids;