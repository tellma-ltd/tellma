CREATE FUNCTION [map].[AccountTypeResourceDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT 
		[Id],
		[AccountTypeId],
		[ResourceDefinitionId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[AccountTypeResourceDefinitions]
);
