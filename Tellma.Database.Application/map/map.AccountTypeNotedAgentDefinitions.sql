CREATE FUNCTION [map].[AccountTypeNotedAgentDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AccountTypeNotedAgentDefinitions]
);
