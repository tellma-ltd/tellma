CREATE FUNCTION [map].[ReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportDefinitions]
);
