CREATE FUNCTION [map].[ReportRowDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDimensionDefinitions] WHERE [Discriminator] = N'Row'
);
