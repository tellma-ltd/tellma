CREATE FUNCTION [map].[ReportDefinitionRows]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDefinitionDimensions] WHERE [Discriminator] = N'Row'
);
