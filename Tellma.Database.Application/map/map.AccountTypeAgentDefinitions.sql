CREATE FUNCTION [map].[AccountTypeAgentDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AccountTypeAgentDefinitions]
);
