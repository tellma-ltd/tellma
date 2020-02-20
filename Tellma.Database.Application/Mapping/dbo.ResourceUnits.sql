CREATE FUNCTION [map].[ResourceUnits] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ResourceUnits]
);
