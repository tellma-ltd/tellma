CREATE FUNCTION [map].[LineDefinitionEntryNotedResourceDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionEntryId],
		[NotedResourceDefinitionId],
		-- Audit details
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionEntryNotedResourceDefinitions]
);