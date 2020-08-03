CREATE FUNCTION [map].[Custodies] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, [Location].STAsBinary() AS [LocationWkb] FROM [dbo].[Custodies]
);