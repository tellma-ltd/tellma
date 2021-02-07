CREATE FUNCTION [map].[ReportDefinitionColumns]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDefinitionDimensions] WHERE [Discriminator] = N'Column'
);
