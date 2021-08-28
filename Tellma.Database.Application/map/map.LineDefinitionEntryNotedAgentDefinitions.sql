CREATE FUNCTION [map].[LineDefinitionEntryNotedAgentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntryNotedAgentDefinitions]
);