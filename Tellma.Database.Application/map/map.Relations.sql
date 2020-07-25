CREATE FUNCTION [map].[Relations] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, [Location].STAsBinary() AS [LocationWkb] FROM [dbo].[Relations]
);