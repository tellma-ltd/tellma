CREATE PROCEDURE [api].[MeasurementUnits__Activate]
	@IndexedIds [dbo].[IndexedIdList] READONLY,
	@IsActive BIT
AS
SET NOCOUNT ON;
	DECLARE @Ids dbo.IdList;
	INSERT INTO @Ids SELECT [Id] FROM @IndexedIds;
	EXEC [dal].[MeasurementUnits__Activate] @Ids = @Ids, @IsActive = @IsActive;