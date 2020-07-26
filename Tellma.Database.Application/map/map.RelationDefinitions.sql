CREATE FUNCTION [map].[RelationDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[RelationDefinitions]
);
