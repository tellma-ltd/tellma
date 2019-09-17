CREATE FUNCTION [rpt].[ResourceLookups] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[ResourceLookups]
);
