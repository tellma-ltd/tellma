CREATE PROCEDURE [dbo].[api_MeasurementUnits__Delete]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsDeleted BIT = 1,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[bll_MeasurementUnits_Validate__Delete]
		@Ids = @Ids,
		@ValidationErrorsJson = @ValidationErrorsJson OUTPUT;

	IF @ValidationErrorsJson IS NOT NULL
		RETURN;

	EXEC [dbo].[dal_MeasurementUnits__Delete]
		@Ids = @Ids, @IsDeleted = @IsDeleted;