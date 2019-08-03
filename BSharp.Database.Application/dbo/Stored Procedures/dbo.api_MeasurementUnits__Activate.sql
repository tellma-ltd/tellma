CREATE PROCEDURE [dbo].[api_MeasurementUnits__Activate]
	@Ids [dbo].[IndexedUuidList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dbo].[dal_MeasurementUnits__Activate] @Ids = @Ids, @IsActive = @IsActive;