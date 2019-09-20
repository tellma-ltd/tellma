CREATE FUNCTION [map].[MeasurementUnits] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[MeasurementUnits]
);
