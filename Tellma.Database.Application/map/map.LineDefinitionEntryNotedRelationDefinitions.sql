CREATE FUNCTION [map].[LineDefinitionEntryNotedRelationDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntryNotedRelationDefinitions]
);
