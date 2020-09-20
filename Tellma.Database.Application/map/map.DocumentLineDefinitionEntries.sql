CREATE FUNCTION [map].[DocumentLineDefinitionEntries]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentLineDefinitionEntries]
);
