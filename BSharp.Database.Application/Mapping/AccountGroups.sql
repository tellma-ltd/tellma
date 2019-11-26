CREATE FUNCTION [map].[AccountGroups]()
RETURNS TABLE AS 
RETURN (
	SELECT *
	FROM dbo.[AccountGroups]
);