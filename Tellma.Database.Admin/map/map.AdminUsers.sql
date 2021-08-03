CREATE FUNCTION [map].[AdminUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AdminUsers]
);
