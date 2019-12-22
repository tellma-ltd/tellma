CREATE FUNCTION [map].[DocumentAssignmentsHistory] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentAssignmentsHistory]
);