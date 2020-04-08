CREATE FUNCTION [map].[Resources] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Resources]
);
