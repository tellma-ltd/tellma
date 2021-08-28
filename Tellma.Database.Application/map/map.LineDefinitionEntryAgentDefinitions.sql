CREATE FUNCTION [map].[LineDefinitionEntryAgentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntryAgentDefinitions]
);