CREATE FUNCTION [map].[AccountTypeCustodianDefinitions] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[AccountTypeCustodianDefinitions]
);
