CREATE FUNCTION [map].[LineDefinitionEntryNotedAgentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionEntryId],
		[NotedAgentDefinitionId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionEntryNotedAgentDefinitions]
);