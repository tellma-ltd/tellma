CREATE FUNCTION [map].[Lookups] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Lookups]
);
