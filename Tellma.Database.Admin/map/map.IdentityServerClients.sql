CREATE FUNCTION [map].[IdentityServerClients] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[IdentityServerClients]
);
