CREATE FUNCTION [map].[AdminUsers] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, IIF(ExternalId IS NULL, 'New', 'Confirmed') As [State] FROM [dbo].[AdminUsers]
);
