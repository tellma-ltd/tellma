CREATE FUNCTION [map].[AccountTypeNotedAgentDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AccountTypeId],
		[NotedAgentDefinitionId],
		-- Audit details
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[AccountTypeNotedAgentDefinitions]
);
