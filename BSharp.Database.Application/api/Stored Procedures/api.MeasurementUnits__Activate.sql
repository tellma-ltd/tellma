CREATE PROCEDURE [api].[MeasurementUnits__Activate]
	@Ids [dbo].[IndexedIdList] READONLY,
	@IsActive BIT,
	@ValidationErrorsJson NVARCHAR(MAX) = NULL OUTPUT
AS
SET NOCOUNT ON;
	EXEC [dal].[MeasurementUnits__Activate] @Ids = @Ids, @IsActive = @IsActive;