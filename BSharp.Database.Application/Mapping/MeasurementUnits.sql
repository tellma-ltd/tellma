CREATE FUNCTION [rpt].[MeasurementUnits] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[MeasurementUnits] WHERE [UnitType] <> N'Currency'
);
