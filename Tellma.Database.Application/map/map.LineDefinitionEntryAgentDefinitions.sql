CREATE FUNCTION [map].[LineDefinitionEntryAgentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Id],
		[LineDefinitionEntryId],
		[AgentDefinitionId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionEntryAgentDefinitions]
);