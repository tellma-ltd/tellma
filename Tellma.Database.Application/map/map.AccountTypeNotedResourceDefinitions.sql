CREATE FUNCTION [map].[AccountTypeNotedResourceDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AccountTypeId],
		[NotedResourceDefinitionId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[AccountTypeNotedResourceDefinitions]
);