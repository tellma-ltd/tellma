CREATE FUNCTION [map].[IfrsConcepts]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[IfrsConcepts]
);
