CREATE FUNCTION [map].[Outbox]()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[DocumentAssignmentsHistory]
	WHERE [CreatedById] = CONVERT(INT, SESSION_CONTEXT(N'UserId'))
);
