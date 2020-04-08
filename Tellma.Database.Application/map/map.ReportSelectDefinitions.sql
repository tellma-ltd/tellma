CREATE FUNCTION [map].[ReportSelectDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ReportSelectDefinitions]
);
