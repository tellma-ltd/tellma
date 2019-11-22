CREATE FUNCTION [map].[AccountTypes]()
RETURNS TABLE AS 
RETURN (
	SELECT *
	FROM dbo.[AccountDefinitions]
);