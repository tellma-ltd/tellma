CREATE FUNCTION [map].[AdminUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, IIF(ExternalId IS NULL, 'Invited', 'Member') As [State] FROM [dbo].[AdminUsers]
);
