CREATE FUNCTION [map].[LookupDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[LookupDefinitions]
);
