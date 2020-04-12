CREATE FUNCTION [map].[LineDefinitionEntries]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LineDefinitionEntries]
);
