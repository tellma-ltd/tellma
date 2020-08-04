CREATE FUNCTION [map].[LineDefinitionEntryResourceDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntryResourceDefinitions]
);
