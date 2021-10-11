CREATE FUNCTION [map].[AgentDefinitionReportDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AgentDefinitionId],
		[ReportDefinitionId],
		[Name],
		[Name2],
		[Name3],
		[Index],
		[SavedById],
		[ValidFrom],
		[ValidTo]	
	FROM [dbo].[AgentDefinitionReportDefinitions]
);