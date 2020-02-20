CREATE FUNCTION [map].[AgentRates] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AgentRates]
);
