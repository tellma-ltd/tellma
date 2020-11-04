CREATE FUNCTION [map].[Users] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, IIF([ExternalId] IS NULL, 'Invited', 'Member') As [State], CAST(IIF([PushEndpoint] IS NULL, 0, 1) AS BIT) AS [PushEnabled] FROM [dbo].[Users]
)
