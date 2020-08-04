CREATE FUNCTION [map].[Workflows]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Workflows]
);
