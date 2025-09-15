CREATE FUNCTION [dal].[ft_BankAccounts__Unreconciled]
(
	@AsOfDate DATE = NULL
)
RETURNS @returntable TABLE
(
	[AgentId0] INT,
	[Quantity0] INT,           -- Count of unreconciled internal entries
	[Decimal1] DECIMAL(19,4),  -- Max unreconciled internal amount
	[NotedAmount0] INT,        -- Count of unreconciled external entries
	[Decimal2] DECIMAL(19,4),  -- Max unreconciled external amount
	[Memo] NVARCHAR(255),      -- Account Number (ExternalReference)
	[Time10] DATE              -- Bank Last Date
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
	
	-- Get reconciled internal entries as of date (from first function logic)
	ReconciledInternalEntries AS (
		SELECT DISTINCT 
			RE.[EntryId],
			E.[AccountId],
			E.[AgentId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		JOIN dbo.Entries E ON RE.EntryId = E.Id
		WHERE EE.PostingDate <= @AsOfDate	
		AND E.[AccountId] IN (SELECT [Id] FROM BankAccounts)
	),
	
	-- Get wholly reversed internal entries as of date (from first function logic)
	WhollyReversedInternalEntries AS (
		SELECT DISTINCT 
			RE.[EntryId],
			E.[AccountId],
			E.[AgentId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		WHERE RE.ReconciliationId IN (
			-- Reconciliations that have only internal entries and were posted before AsOfDate
			SELECT DISTINCT RE2.[ReconciliationId]
			FROM dbo.ReconciliationEntries RE2
			JOIN dbo.Entries E2 ON RE2.EntryId = E2.[Id]
			JOIN dbo.Lines L2 ON L2.[Id] = E2.[LineId]
			JOIN dbo.Documents D2 ON D2.[Id] = L2.[DocumentId]
			WHERE L2.PostingDate <= @AsOfDate
			AND L2.[State] = 4
			AND D2.[State] = 1
			AND E2.[AccountId] IN (SELECT [Id] FROM BankAccounts)
			AND RE2.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationExternalEntries)
			EXCEPT
			-- Exclude reconciliations that have entries posted after AsOfDate
			SELECT DISTINCT RE3.[ReconciliationId]
			FROM dbo.ReconciliationEntries RE3
			JOIN dbo.Entries E3 ON RE3.EntryId = E3.[Id]
			JOIN dbo.Lines L3 ON L3.[Id] = E3.[LineId]
			JOIN dbo.Documents D3 ON D3.[Id] = L3.[DocumentId]
			WHERE L3.PostingDate > @AsOfDate
			AND L3.[State] = 4
			AND D3.[State] = 1
			AND E3.[AccountId] IN (SELECT [Id] FROM BankAccounts)
		)
		AND E.[AccountId] IN (SELECT [Id] FROM BankAccounts)
	),
	
	-- Get reconciled external entries as of date (from second function logic)
	ReconciledExternalEntries AS (
		SELECT DISTINCT 
			REE.[ExternalEntryId],
			EE.[AccountId],
			EE.[AgentId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationEntries RE ON RE.ReconciliationId = R.Id
		JOIN dbo.Entries E ON RE.EntryId = E.Id
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE L.PostingDate <= @AsOfDate
		AND L.[State] = 4
		AND D.[State] = 1
		AND E.[AccountId] IN (SELECT [Id] FROM BankAccounts)
	),
	
	-- Get wholly reversed external entries as of date (from second function logic)
	WhollyReversedExternalEntries AS (
		SELECT DISTINCT 
			REE.[ExternalEntryId],
			EE.[AccountId],
			EE.[AgentId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE REE.ReconciliationId IN (
			-- Reconciliations that have only external entries and were posted before AsOfDate
			SELECT DISTINCT REE2.[ReconciliationId]
			FROM dbo.ReconciliationExternalEntries REE2
			JOIN dbo.ExternalEntries EE2 ON REE2.ExternalEntryId = EE2.[Id]
			WHERE EE2.PostingDate <= @AsOfDate
			AND EE2.[AccountId] IN (SELECT [Id] FROM BankAccounts)
			AND REE2.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationEntries)
			EXCEPT
			-- Exclude reconciliations that have external entries posted after AsOfDate
			SELECT DISTINCT REE3.[ReconciliationId]
			FROM dbo.ReconciliationExternalEntries REE3
			JOIN dbo.ExternalEntries EE3 ON REE3.ExternalEntryId = EE3.[Id]
			WHERE EE3.PostingDate > @AsOfDate
			AND EE3.[AccountId] IN (SELECT [Id] FROM BankAccounts)
		)
		AND EE.[AccountId] IN (SELECT [Id] FROM BankAccounts)
	),
	
	-- Get unreconciled internal entries
	UnreconciledInternal AS (
		SELECT
			E.[AgentId],
			E.[AccountId],
			COUNT(*) as UnreconciledCount,
			MAX(ABS(E.[Direction] * E.[MonetaryValue])) as MaxAmount
		FROM dbo.Entries E
		JOIN dbo.Lines L ON E.[LineId] = L.[Id]
		JOIN dbo.Documents D ON L.[DocumentId] = D.[Id]
		WHERE E.[AccountId] IN (SELECT [Id] FROM BankAccounts)
		AND L.[State] = 4
		AND D.[State] = 1
		AND E.[MonetaryValue] <> 0 -- Exclude exchange variance transactions
		AND L.[PostingDate] <= @AsOfDate
		-- Exclude reconciled entries
		AND NOT EXISTS (
			SELECT 1 FROM ReconciledInternalEntries RIE 
			WHERE RIE.EntryId = E.Id 
			AND RIE.AccountId = E.AccountId 
			AND RIE.AgentId = E.AgentId
		)
		-- Exclude wholly reversed entries
		AND NOT EXISTS (
			SELECT 1 FROM WhollyReversedInternalEntries WRIE 
			WHERE WRIE.EntryId = E.Id 
			AND WRIE.AccountId = E.AccountId 
			AND WRIE.AgentId = E.AgentId
		)
		GROUP BY E.[AgentId], E.[AccountId]
	),
	
	-- Get unreconciled external entries
	UnreconciledExternal AS (
		SELECT
			EE.[AgentId],
			EE.[AccountId],
			COUNT(*) as UnreconciledCount,
			MAX(ABS(EE.[Direction] * EE.[MonetaryValue])) as MaxAmount
		FROM dbo.ExternalEntries EE
		WHERE EE.[AccountId] IN (SELECT [Id] FROM BankAccounts)
		AND EE.[PostingDate] <= @AsOfDate
		-- Exclude reconciled external entries
		AND NOT EXISTS (
			SELECT 1 FROM ReconciledExternalEntries REE 
			WHERE REE.ExternalEntryId = EE.Id 
			AND REE.AccountId = EE.AccountId 
			AND REE.AgentId = EE.AgentId
		)
		-- Exclude wholly reversed external entries
		AND NOT EXISTS (
			SELECT 1 FROM WhollyReversedExternalEntries WREE 
			WHERE WREE.ExternalEntryId = EE.Id 
			AND WREE.AccountId = EE.AccountId 
			AND WREE.AgentId = EE.AgentId
		)
		GROUP BY EE.[AgentId], EE.[AccountId]
	),
	
	-- Get last external entry posting date for each agent
	LastExternalEntriesPostingDate AS (
		SELECT 
			EE.[AgentId], 
			AG.[ExternalReference], 
			MAX(EE.[PostingDate]) AS BankLastDate
		FROM dbo.ExternalEntries EE
		JOIN dbo.[Agents] AG ON AG.[Id] = EE.[AgentId]
		JOIN dbo.[AgentDefinitions] AGD ON AGD.[Id] = AG.[DefinitionId]
		WHERE AGD.[Code] = N'BankAccount'
		AND EE.[AccountId] IN (SELECT [Id] FROM BankAccounts)
		GROUP BY EE.[AgentId], AG.[ExternalReference]
	)
	
	INSERT @returntable([AgentId0], [Quantity0], [Decimal1], [NotedAmount0], [Decimal2], [Memo], [Time10])
	SELECT 
		COALESCE(UI.AgentId, UE.AgentId) AS AgentId,
		ISNULL(UI.UnreconciledCount, 0) AS InternalUnreconciledCount,
		ISNULL(UI.MaxAmount, 0) AS MaxInternalAmount,
		ISNULL(UE.UnreconciledCount, 0) AS ExternalUnreconciledCount,
		ISNULL(UE.MaxAmount, 0) AS MaxExternalAmount,
		LEPD.[ExternalReference] AS AccountNumber,
		LEPD.BankLastDate
	FROM UnreconciledInternal UI
	FULL OUTER JOIN UnreconciledExternal UE 
		ON UI.AgentId = UE.AgentId AND UI.AccountId = UE.AccountId
	JOIN LastExternalEntriesPostingDate LEPD 
		ON LEPD.[AgentId] = COALESCE(UI.AgentId, UE.AgentId)
	WHERE ISNULL(UI.UnreconciledCount, 0) > 0 OR ISNULL(UE.UnreconciledCount, 0) > 0
	ORDER BY (ISNULL(UI.UnreconciledCount, 0) + ISNULL(UE.UnreconciledCount, 0)) DESC;
	
	RETURN
END