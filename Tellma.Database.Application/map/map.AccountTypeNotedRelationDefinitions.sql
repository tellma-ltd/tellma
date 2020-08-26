CREATE FUNCTION [map].[AccountTypeNotedRelationDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT 
	0 AS Id,
	0 AS [AccountTypeId],
	0 AS [ResourceDefinitionId],
	-- Audit details
	0 AS [SavedById],
	NULL AS [ValidFrom],
	NULL [ValidTo]	
	WHERE 1=0
);
