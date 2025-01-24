CREATE FUNCTION [dal].[ft_Reconciliation__Load_Unreconciled_Internal](
-- Extracted from [dal].[Reconciliation__Load_Unreconciled]. Used in printing template adhoc
	@AccountId		INT, 
	@AgentId		INT, 
	@AsOfDate		DATE
)
RETURNS @Result TABLE (
	EntryId				INT,
	PostingDate			DATE ,
	NotedAmount 		DECIMAL(19, 4),
	Memo				NVARCHAR(255),
	DocumentCode		NVARCHAR(50),
	IsReconciledLater	BIT
)
AS BEGIN
	WITH ReconciledEntriesAsOfDate AS (
		SELECT DISTINCT RE.[EntryId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Reconciliations R ON RE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationExternalEntries REE ON REE.ReconciliationId = R.Id
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.Id
		WHERE EE.PostingDate <= @AsOfDate	
		AND EE.[AccountId] = @AccountId
		AND EE.[AgentId] = @AgentId
	),
	WhollyReversedEntriesAsOfDate AS (
		SELECT DISTINCT RE.[ReconciliationId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		WHERE L.PostingDate <= @AsOfDate
		AND L.[State] = 4
		AND D.[State] = 1
		AND	E.[AccountId] = @AccountId
		AND E.[AgentId] = @AgentId
		AND RE.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationExternalEntries)
		EXCEPT
		SELECT DISTINCT RE.[ReconciliationId]
		FROM dbo.ReconciliationEntries RE
		JOIN dbo.Entries E ON RE.EntryId = E.[Id]
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		WHERE L.PostingDate > @AsOfDate
		AND L.[State] = 4
		AND D.[State] = 1
		AND	E.[AccountId] = @AccountId
		AND E.[AgentId] = @AgentId
	)
	INSERT @Result(EntryId, PostingDate, NotedAmount, Memo, DocumentCode, IsReconciledLater)
	SELECT E.[Id], L.[PostingDate], E.[Direction] * E.[MonetaryValue] AS [NotedAmount],	D.Memo, D.Code As DocumentCode,
		CAST(IIF(E.[Id] IN (SELECT [EntryId] FROM dbo.ReconciliationEntries), 1, 0) AS BIT) AS IsReconciledLater
	FROM dbo.Entries E
	JOIN dbo.Lines L ON E.[LineId] = L.[Id]
	JOIN map.Documents() D ON L.[DocumentId] = D.[Id] 
	WHERE E.[AgentId] = @AgentId
	AND E.[AccountId] = @AccountId
	AND L.[State] = 4
	AND D.[State] = 1
	AND E.[MonetaryValue] <> 0 -- MA: 2024-12-09 to exclude exchange variance transactions
	AND L.[PostingDate] <= @AsOfDate
	-- Exclude if it was reconciled with an external entry before AsOfDate
	AND E.[Id] NOT IN (SELECT EntryId FROM ReconciledEntriesAsOfDate)
	-- Exclude if it was reversed with an internal entry before AsOfDate
	AND E.[Id] NOT IN (
			SELECT EntryId FROM dbo.ReconciliationEntries 
			WHERE ReconciliationId IN (SELECT [ReconciliationId] FROM WhollyReversedEntriesAsOfDate)
	)
	ORDER BY L.[PostingDate], D.[Code], E.[MonetaryValue], E.[ExternalReference];

	RETURN;
END
GO