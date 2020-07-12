CREATE FUNCTION [map].[Accounts]()
RETURNS TABLE
AS
RETURN (
	SELECT A.*
	FROM [dbo].[Accounts] A
);
