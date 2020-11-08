CREATE FUNCTION [map].[CustodyDefinitionReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[CustodyDefinitionReportDefinitions]
);
