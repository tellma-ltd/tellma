CREATE FUNCTION [map].[Relations] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Relations]
);