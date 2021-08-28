CREATE FUNCTION [map].[AgentDefinitionReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AgentDefinitionReportDefinitions]
);
