CREATE FUNCTION [map].[Contracts] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, [Location].STAsBinary() AS [LocationWkb] FROM [dbo].[Contracts]
);