CREATE FUNCTION [map].[DocumentStatesHistory] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentStatesHistory]
);