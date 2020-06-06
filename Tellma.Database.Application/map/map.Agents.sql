CREATE FUNCTION [map].[Agents] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Agents]
);