CREATE FUNCTION [map].[Currencies] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Currencies]
);
