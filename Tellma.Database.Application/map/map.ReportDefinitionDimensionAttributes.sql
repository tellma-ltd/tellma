CREATE FUNCTION [map].[ReportDefinitionDimensionAttributes]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDefinitionDimensionAttributes]
);
