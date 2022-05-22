CREATE PROCEDURE [rpt].[BankAccounts__Reconciled]
	@FromDate DATE = NULL
AS
	WITH BankAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Concept] = N'BalancesWithBanks'
		)
	)
	SELECT RL.[Name] AS BankAccount,Year(R.CreatedAt) AS [Year], Month(R.CreatedAt) As [Month],
		ReconciledCount = COUNT(DISTINCT R.[Id])
	FROM dbo.Reconciliations R
	JOIN dbo.ReconciliationExternalEntries REE ON R.[Id] = REE.ReconciliationId
	JOIN dbo.ExternalEntries EE ON REE.[ExternalEntryId] = EE.[Id]
	JOIN dbo.[Agents] RL ON EE.[AgentId] = RL.[Id]
	WHERE EE.[AccountId] IN (SELECT [Id] FROM BankAccounts)
	AND (@FromDate IS NULL OR R.[CreatedAt] >= @FromDate)
	GROUP BY EE.[AgentId], RL.[Name], Year(R.CreatedAt),Month(R.CreatedAt)
	ORDER BY Year(R.CreatedAt), Month(R.CreatedAt);