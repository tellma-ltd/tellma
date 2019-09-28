CREATE FUNCTION [rpt].[GLAccounts] (
	@Prefix NVARCHAR (50) = N''
)
RETURNS TABLE AS 
RETURN (
	SELECT
		[Id], 
		(SELECT [Id] FROM dbo.GLAccounts WHERE [ParentNode] = A.[Node]) AS [ParentId], 
		[AccountType],
		[Name],
		[Name2],
		[Name3],
		[Code]
	FROM dbo.GLAccounts A
	WHERE [Code] LIKE @Prefix + '%'
);