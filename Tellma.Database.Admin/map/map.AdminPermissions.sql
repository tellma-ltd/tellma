CREATE FUNCTION [map].[AdminPermissions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AdminPermissions]
);
