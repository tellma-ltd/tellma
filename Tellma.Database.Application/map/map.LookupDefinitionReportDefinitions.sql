CREATE FUNCTION [map].[LookupDefinitionReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LookupDefinitionReportDefinitions]
);
