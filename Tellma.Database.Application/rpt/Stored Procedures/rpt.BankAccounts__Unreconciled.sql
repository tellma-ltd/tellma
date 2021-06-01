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
		SELECT EE.[RelationId], RL.[ExternalReference], MAX(EE.[PostingDate]) AS BankLastDate
		FROM dbo.ExternalEntries EE
		JOIN dbo.Relations RL ON C.[Id] = EE.[RelationId]
		JOIN dbo.RelationDefinitions RLD ON RLD.[Id] = RL.[DefinitionId]
		WHERE RLD.[Code] = N'BankAccount'
		GROUP BY EE.[RelationId], RL.[ExternalReference]
	)
	SELECT COALESCE(TE.[Name], TEE.[Name]) AS BankAccount,
		TE.UnreconciledEntriesCount,-- TE.UnreconciledEntriesBalance,
		TEE.UnreconciledExternalEntriesCount, -- TEE.UnreconciledExternalEntriesBalance
		LEPD.[ExternalReference] AS [Account Number], LEPD.BankLastDate
	FROM
	(
		SELECT
			E.[RelationId], RL.[Name],
			UnreconciledEntriesCount = COUNT(*),
			UnreconciledEntriesBalance = SUM(
				IIF (L.[PostingDate] <= @AsOfDate , E.[Direction] * E.[MonetaryValue], -E.[Direction] * E.[MonetaryValue])
			)
		FROM dbo.Entries E
		JOIN dbo.Relations RL ON E.[RelationId] = RL.[Id]
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
			E.[RelationId], RL.[Name]
	) TE
	FULL OUTER JOIN
	(
		SELECT
			E.[RelationId], RL.[Name],
			UnreconciledExternalEntriesCount = COUNT(*),
			UnreconciledExternalEntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
		FROM dbo.ExternalEntries E
		JOIN dbo.Relations RL ON E.[RelationId] = RL.[Id]
		WHERE
			E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
		AND E.AccountId IN (SELECT [Id] FROM BankAccounts)
		AND E.[PostingDate] <= @AsOfDate
		GROUP BY
			E.[RelationId], RL.[Name]
	) TEE ON TE.[RelationId] = TEE.[RelationId]
	JOIN LastExternalEntriesPostingDate LEPD ON LEPD.[RelationId] = COALESCE(TE.[RelationId], TEE.[RelationId])
	ORDER BY ISNULL([UnreconciledExternalEntriesCount], 0) + ISNULL([UnreconciledEntriesCount], 0) DESC;