CREATE FUNCTION [map].[Accounts]()
RETURNS TABLE
AS
RETURN (
	WITH ExpendituresParentAccountTypes AS (
		SELECT [Node]
		FROM dbo.[AccountTypes]
		WHERE [Concept] IN (
			N'Revenue',
			N'ExpenseByNature'
		)
	),
	ExpendituresAccountTypes AS (
		SELECT ATC.[Id]
		FROM dbo.[AccountTypes] ATC
		JOIN ExpendituresParentAccountTypes ATP ON ATC.[Node].IsDescendantOf(ATP.[Node]) = 1
	),
	ExpendituresAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (SELECT [Id] FROM ExpendituresAccountTypes)
	)
	SELECT A.*, IIF(EA.[Id] IS NULL, 1, 0) AS IsBusinessUnit
	FROM dbo.Accounts A
	LEFT JOIN ExpendituresAccounts EA ON A.[Id] = EA.[Id]
);