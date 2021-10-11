CREATE FUNCTION [map].[LineDefinitionEntryResourceDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[LineDefinitionEntryId],
		[ResourceDefinitionId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[LineDefinitionEntryResourceDefinitions]
);