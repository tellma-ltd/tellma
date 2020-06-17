CREATE FUNCTION [map].[ResourceDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ResourceDefinitions]
);
