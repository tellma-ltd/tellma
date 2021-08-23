CREATE FUNCTION [map].[Accounts]()
RETURNS TABLE
AS
RETURN (
	SELECT A.*--, AC.IsBusinessUnit
	FROM dbo.Accounts A
	--JOIN map.AccountTypes() AC ON A.[AccountTypeId] = AC.[Id]
);