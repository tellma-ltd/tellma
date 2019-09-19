CREATE FUNCTION [rpt].[Currencies] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Currencies]
);
