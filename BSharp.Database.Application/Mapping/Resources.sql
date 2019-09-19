CREATE FUNCTION [rpt].[Resources2] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Resources]
);
