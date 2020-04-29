CREATE FUNCTION [map].[Contracts] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Contracts]
);