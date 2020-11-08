CREATE FUNCTION [map].[ResourceDefinitionReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ResourceDefinitionReportDefinitions]
);
