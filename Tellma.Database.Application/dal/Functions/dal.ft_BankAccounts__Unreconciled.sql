CREATE FUNCTION [dal].[ft_BankAccounts__Unreconciled]
(
	@AsOfDate		DATE = NULL
)
RETURNS @returntable TABLE
(
	[AgentId0] INT,
	[Quantity0]	INT,
	[Decimal1]	INT,
	[NotedAmount0] INT,
	[Decimal2]	INT,
	[Memo]	NVARCHAR (255),
	[Time10]	DATE
)
AS
BEGIN
	SET @AsOfDate = ISNULL(@AsOfDate, CAST(GETDATE() AS DATE));
	WITH BankAccounts AS (
		SELECT [Id] FROM dbo.Accounts
		WHERE AccountTypeId IN (
			SELECT [Id] FROM dbo.AccountTypes
			WHERE [Concept] = N'BalancesWithBanks'
		)
	),
	LastExternalEntriesPostingDate AS (
		SELECT EE.[AgentId], AG.[ExternalReference], MAX(EE.[PostingDate]) AS BankLastDate
		FROM dbo.ExternalEntries EE
		JOIN dbo.[Agents] AG ON AG.[Id] = EE.[AgentId]
		JOIN dbo.[AgentDefinitions] AGD ON AGD.[Id] = AG.[DefinitionId]
		WHERE AGD.[Code] = N'BankAccount'
		GROUP BY EE.[AgentId], AG.[ExternalReference]
	)
	INSERT @returntable([AgentId0], [Quantity0], [Decimal1], [NotedAmount0], [Decimal2], [Memo], [Time10])
	SELECT COALESCE(TE.[AgentId], TEE.[AgentId]) AS BankAccount,
		TE.UnreconciledEntriesCount,
		TE.MaxEntriesAmount AS MaxBookAmount,
		TEE.UnreconciledExternalEntriesCount,
		TEE.MaxExternalEntriesAmount AS MaxBankAmount,
		LEPD.[ExternalReference] AS [Account Number], LEPD.BankLastDate
	FROM
	(
		SELECT
			E.[AgentId],
			UnreconciledEntriesCount = COUNT(*),
			MaxEntriesAmount = MAX(E.[MonetaryValue])
		FROM dbo.Entries E
		JOIN dbo.[Agents] AG ON E.[AgentId] = AG.[Id]
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		LEFT JOIN (
			SELECT DISTINCT RE.[EntryId]
			FROM dbo.ReconciliationEntries RE
			JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
			JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
			JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
--			WHERE EE.PostingDate <=  @AsOfDate	
		) T ON T.[EntryId] = E.[Id]
		WHERE
			T.[EntryId] IS NULL
		AND E.AccountId IN (SELECT [Id] FROM BankAccounts)
		AND	L.[State] = 4
		AND L.[PostingDate] <= @AsOfDate
		GROUP BY
			E.[AgentId], AG.[Name]
	) TE
	FULL OUTER JOIN
	(
		SELECT
			E.[AgentId],
			UnreconciledExternalEntriesCount = COUNT(*),
			MaxExternalEntriesAmount = MAX(E.[MonetaryValue])
		FROM dbo.ExternalEntries E
		JOIN dbo.[Agents] AG ON E.[AgentId] = AG.[Id]
		WHERE
			E.[Id] NOT IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries)
		AND E.AccountId IN (SELECT [Id] FROM BankAccounts)
		AND E.[PostingDate] <= @AsOfDate
		GROUP BY
			E.[AgentId], AG.[Name]
	) TEE ON TE.[AgentId] = TEE.[AgentId]
	JOIN LastExternalEntriesPostingDate LEPD ON LEPD.[AgentId] = COALESCE(TE.[AgentId], TEE.[AgentId])
	ORDER BY ISNULL([UnreconciledExternalEntriesCount], 0) + ISNULL([UnreconciledEntriesCount], 0) DESC;
	
	
	RETURN
END
GO