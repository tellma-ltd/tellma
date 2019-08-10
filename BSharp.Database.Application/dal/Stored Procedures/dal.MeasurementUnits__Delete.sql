CREATE PROCEDURE [dal].[MeasurementUnits__Delete]
	@Ids [dbo].[IdList] READONLY
AS
	DELETE FROM [dbo].MeasurementUnits 
	WHERE Id IN (SELECT Id FROM @Ids);