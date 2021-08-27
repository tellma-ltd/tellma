CREATE FUNCTION [map].[SqlServers] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[SqlServers]
);
