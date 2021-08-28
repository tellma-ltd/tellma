CREATE PROCEDURE [rpt].[BankAccounts__Unreconciled]
	@AsOfDate		DATE = NULL
AS
	SET @AsOfDate = ISNULL(@AsOfDate, CAST(GETDATE() AS DATE));
	WITH BankAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Concept] = N'BalancesWithBanks'
		)
	),
	LastExternalEntriesPostingDate AS (
		SELECT EE.[AgentId], RL.[ExternalReference], MAX(EE.[PostingDate]) AS BankLastDate
		FROM dbo.ExternalEntries EE
		JOIN dbo.[Agents] RL ON RL.[Id] = EE.[AgentId]
		JOIN dbo.[AgentDefinitions] RLD ON RLD.[Id] = RL.[DefinitionId]
		WHERE RLD.[Code] = N'BankAccount'
		GROUP BY EE.[AgentId], RL.[ExternalReference]
	)
	SELECT COALESCE(TE.[Name], TEE.[Name]) AS BankAccount,
		TE.UnreconciledEntriesCount,
		FORMAT(TE.MaxEntriesAmount, 'N0', 'en-us') AS MaxBookAmount,
		TEE.UnreconciledExternalEntriesCount,
		FORMAT(TEE.MaxExternalEntriesAmount, 'N0', 'en-us') AS MaxBankAmount,
		LEPD.[ExternalReference] AS [Account Number], LEPD.BankLastDate
	FROM
	(
		SELECT
			E.[AgentId], RL.[Name],
			UnreconciledEntriesCount = COUNT(*),
			MaxEntriesAmount = MAX(E.[MonetaryValue])
		FROM dbo.Entries E
		JOIN dbo.[Agents] RL ON E.[AgentId] = RL.[Id]
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		LEFT JOIN (
			SELECT DISTINCT RE.[EntryId]
			FROM dbo.ReconciliationEntries RE
			JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
			JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
			JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
			WHERE EE.PostingDate <=  @AsOfDate	
		
		) T ON T.[EntryId] = E.[Id]
		WHERE
			T.[EntryId] IS NULL
		AND E.AccountId IN (SELECT [Id] FROM BankAccounts)
		AND	L.[State] = 4
		AND L.[PostingDate] <= @AsOfDate
		GROUP BY
			E.[AgentId], RL.[Name]
	) TE
	FULL OUTER JOIN
	(
		SELECT
			E.[AgentId], RL.[Name],
			UnreconciledExternalEntriesCount = COUNT(*),
			MaxExternalEntriesAmount = MAX(E.[MonetaryValue])
		FROM dbo.ExternalEntries E
		JOIN dbo.[Agents] RL ON E.[AgentId] = RL.[Id]
		WHERE
			E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
		AND E.AccountId IN (SELECT [Id] FROM BankAccounts)
		AND E.[PostingDate] <= @AsOfDate
		GROUP BY
			E.[AgentId], RL.[Name]
	) TEE ON TE.[AgentId] = TEE.[AgentId]
	JOIN LastExternalEntriesPostingDate LEPD ON LEPD.[AgentId] = COALESCE(TE.[AgentId], TEE.[AgentId])
	ORDER BY ISNULL([UnreconciledExternalEntriesCount], 0) + ISNULL([UnreconciledEntriesCount], 0) DESC;