CREATE FUNCTION [map].[ReportMeasureDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportMeasureDefinitions]
);
