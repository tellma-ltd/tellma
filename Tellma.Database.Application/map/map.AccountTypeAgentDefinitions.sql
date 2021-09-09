CREATE FUNCTION [map].[AccountTypeAgentDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AccountTypeId],
		[AgentDefinitionId],
		[SavedById],
		[ValidFrom],
		[ValidTo]
	FROM [dbo].[AccountTypeAgentDefinitions]
);
