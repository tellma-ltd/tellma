CREATE FUNCTION [map].[SqlDatabases] ()
RETURNS TABLE
AS
RETURN (
	SELECT * FROM [dbo].[SqlDatabases]
);
