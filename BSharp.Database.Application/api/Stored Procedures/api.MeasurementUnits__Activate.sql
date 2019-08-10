CREATE PROCEDURE [api].[MeasurementUnits__Activate]
	@Ids [dbo].[IdList] READONLY,
	@IsActive BIT
AS
SET NOCOUNT ON;
	EXEC [dal].[MeasurementUnits__Activate] @Ids = @Ids, @IsActive = @IsActive;