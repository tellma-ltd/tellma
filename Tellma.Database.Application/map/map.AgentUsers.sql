CREATE FUNCTION [map].[AgentUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AgentUsers]
);