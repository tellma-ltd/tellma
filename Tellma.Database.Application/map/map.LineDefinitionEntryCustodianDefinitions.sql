CREATE FUNCTION [map].[LineDefinitionEntryCustodianDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntryCustodianDefinitions]
);