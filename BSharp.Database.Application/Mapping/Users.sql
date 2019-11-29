CREATE FUNCTION [map].[Users] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, IIF(ExternalId IS NULL, 'New', 'Confirmed') As [State] FROM [dbo].[Users]
)
