CREATE FUNCTION [map].[Users] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, IIF(ExternalId IS NULL, 'Invited', 'Member') As [State] FROM [dbo].[Users]
)
