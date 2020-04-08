CREATE FUNCTION [map].[ReportParameterDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportParameterDefinitions]
);
