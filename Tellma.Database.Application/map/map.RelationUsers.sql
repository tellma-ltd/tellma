CREATE FUNCTION [map].[RelationUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[RelationUsers]
);