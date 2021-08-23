CREATE FUNCTION [map].[Roles] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, TODATETIMEOFFSET([ValidFrom], '+00:00') AS [SavedAt] FROM [dbo].[Roles]
);
