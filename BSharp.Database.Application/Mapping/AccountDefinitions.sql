CREATE FUNCTION [map].[AccountDefinitions]()
RETURNS TABLE AS 
RETURN (
	SELECT *
	FROM dbo.[AccountDefinitions]
);