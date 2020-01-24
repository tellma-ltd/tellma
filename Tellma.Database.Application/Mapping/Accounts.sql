CREATE FUNCTION [map].[Accounts]()
RETURNS TABLE
AS
RETURN (
	SELECT *, ~[IsDeprecated] AS [IsActive] FROM [dbo].[Accounts]
);
