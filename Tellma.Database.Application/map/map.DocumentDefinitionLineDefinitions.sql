CREATE FUNCTION [map].[DocumentDefinitionLineDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentDefinitionLineDefinitions]
);