CREATE FUNCTION [map].[ReportColumnDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDimensionDefinitions] WHERE [Discriminator] = N'Column'
);
