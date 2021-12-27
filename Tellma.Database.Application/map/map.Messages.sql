CREATE FUNCTION [map].[Messages]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[Messages]
);