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
		SELECT EE.[CustodyId], C.[ExternalReference], MAX(EE.[PostingDate]) AS BankLastDate
		FROM dbo.ExternalEntries EE
		JOIN dbo.Custodies C ON C.[Id] = EE.[CustodyId]
		JOIN dbo.CustodyDefinitions CD ON CD.[Id] = C.[DefinitionId]
		WHERE CD.[Code] = N'BankAccount'
		GROUP BY EE.[CustodyId], C.[ExternalReference]
	)
	SELECT COALESCE(TE.[Name], TEE.[Name]) AS BankAccount,
		TE.UnreconciledEntriesCount,-- TE.UnreconciledEntriesBalance,
		TEE.UnreconciledExternalEntriesCount,--, TEE.UnreconciledExternalEntriesBalance
		LEPD.[ExternalReference] AS [Account Number], LEPD.BankLastDate
	FROM
	(
		SELECT
			E.[CustodyId], C.[Name],
			UnreconciledEntriesCount = COUNT(*),
			UnreconciledEntriesBalance = SUM(
				IIF (L.[PostingDate] <= @AsOfDate , E.[Direction] * E.[MonetaryValue], -E.[Direction] * E.[MonetaryValue])
			)
		FROM dbo.Entries E
		JOIN dbo.Custodies C ON E.[CustodyId] = C.[Id]
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
			E.[CustodyId], C.[Name]
	) TE
	FULL OUTER JOIN
	(
		SELECT
			E.[CustodyId], C.[Name],
			UnreconciledExternalEntriesCount = COUNT(*),
			UnreconciledExternalEntriesBalance = SUM(E.[Direction] * E.[MonetaryValue])
		FROM dbo.ExternalEntries E
		JOIN dbo.Custodies C ON E.[CustodyId] = C.[Id]
		WHERE
			E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
		AND E.AccountId IN (SELECT [Id] FROM BankAccounts)
		AND E.[PostingDate] <= @AsOfDate
		GROUP BY
			E.[CustodyId], C.[Name]
	) TEE ON TE.[CustodyId] = TEE.[CustodyId]
	JOIN LastExternalEntriesPostingDate LEPD ON LEPD.[CustodyId] = COALESCE(TE.[CustodyId], TEE.[CustodyId])
	ORDER BY ISNULL([UnreconciledExternalEntriesCount], 0) + ISNULL([UnreconciledEntriesCount], 0) DESC;