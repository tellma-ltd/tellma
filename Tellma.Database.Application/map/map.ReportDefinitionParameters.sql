CREATE FUNCTION [map].[ReportDefinitionParameters]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDefinitionParameters]
);
