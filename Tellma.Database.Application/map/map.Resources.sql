CREATE FUNCTION [map].[Resources] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, [Location].STAsBinary() AS [LocationWkb] FROM [dbo].[Resources]
);
