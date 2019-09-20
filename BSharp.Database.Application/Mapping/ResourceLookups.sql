CREATE FUNCTION [map].[ResourceLookups] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ResourceLookups]
);
