CREATE PROCEDURE [dal].[MeasurementUnits__Delete]
	@Ids [dbo].[IndexedIdList] READONLY
AS
	DELETE FROM [dbo].MeasurementUnits 
	WHERE Id IN (SELECT Id FROM @Ids);