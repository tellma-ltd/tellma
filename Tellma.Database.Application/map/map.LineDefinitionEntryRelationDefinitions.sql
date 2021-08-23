CREATE FUNCTION [map].[LineDefinitionEntryRelationDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntryRelationDefinitions]
);