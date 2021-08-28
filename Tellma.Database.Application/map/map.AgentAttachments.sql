CREATE FUNCTION [map].[AgentAttachments]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AgentAttachments]
);
