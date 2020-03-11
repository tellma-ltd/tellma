CREATE FUNCTION [map].[DocumentDefinitions]()
RETURNS TABLE
AS
RETURN (
	SELECT E.* FROM [dbo].[DocumentDefinitions] E
);
