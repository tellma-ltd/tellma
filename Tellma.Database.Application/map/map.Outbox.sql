CREATE FUNCTION [map].[Outbox]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentAssignmentsHistory]
);
