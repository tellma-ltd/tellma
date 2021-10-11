CREATE FUNCTION [map].[AgentUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT
		[Id],
		[AgentId],
		[UserId],
		[CreatedAt],
		[CreatedById],
		[ModifiedAt],
		[ModifiedById]
	FROM [dbo].[AgentUsers]
);