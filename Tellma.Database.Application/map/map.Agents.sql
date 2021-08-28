CREATE FUNCTION [map].[Agents] ()
RETURNS TABLE
AS
RETURN (
	SELECT *, [Location].STAsBinary() AS [LocationWkb] FROM [dbo].[Agents]
);