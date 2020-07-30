CREATE FUNCTION [map].[AccountTypeCustodyDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AccountTypeCustodyDefinitions]
);
