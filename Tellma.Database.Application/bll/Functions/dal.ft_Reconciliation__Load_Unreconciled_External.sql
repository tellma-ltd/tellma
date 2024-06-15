CREATE FUNCTION [dal].[ft_Reconciliation__Load_Unreconciled_External](
-- Extracted from [dal].[Reconciliation__Load_Unreconciled]. Used only for printing template adhoc
	@AccountId		INT, 
	@AgentId		INT, 
	@AsOfDate		DATE
)
RETURNS @Result TABLE (
	EntryId				INT,
	PostingDate			DATE ,
	NotedAmount 		DECIMAL(19,4),
	ExternalReference	NVARCHAR(50),
	IsReconciledLater	BIT)
AS BEGIN
	WITH ReconciledExternalEntriesAsOfDate AS (
		SELECT DISTINCT REE.[ExternalEntryId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.Reconciliations R ON REE.ReconciliationId = R.Id
		JOIN dbo.ReconciliationEntries RE ON RE.ReconciliationId = R.Id
		JOIN dbo.Entries E ON RE.EntryId = E.Id
		JOIN dbo.Lines L ON L.[Id] = E.[LineId]
		JOIN dbo.Documents D ON D.[Id] = L.[DocumentId]
		WHERE L.PostingDate <=  @AsOfDate
		AND L.[State] = 4
		AND D.[State] = 1
		AND E.[AccountId] = @AccountId
		AND E.[AgentId] = @AgentId
	),
	WhollyReversedExternalEntriesAsOfDate AS (
		SELECT DISTINCT REE.[ReconciliationId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE EE.PostingDate <= @AsOfDate
		AND	EE.[AccountId] = @AccountId
		AND EE.[AgentId] = @AgentId
		AND REE.ReconciliationId NOT IN (SELECT ReconciliationId FROM dbo.ReconciliationEntries)
		EXCEPT
		SELECT DISTINCT REE.[ReconciliationId]
		FROM dbo.ReconciliationExternalEntries REE
		JOIN dbo.ExternalEntries EE ON REE.ExternalEntryId = EE.[Id]
		WHERE EE.PostingDate > @AsOfDate
		AND	EE.[AccountId] = @AccountId
		AND EE.[AgentId] = @AgentId
	)
	INSERT @Result (EntryId, PostingDate, NotedAmount, ExternalReference, IsReconciledLater)
	SELECT EE.[Id], EE.[PostingDate], EE.[Direction] * EE.[MonetaryValue], EE.[ExternalReference],
		CAST(IIF(EE.[Id] IN (SELECT [ExternalEntryId] FROM dbo.ReconciliationExternalEntries), 1, 0) AS BIT) AS IsReconciledLater
	FROM dbo.ExternalEntries EE
	WHERE EE.[AgentId] = @AgentId
	AND EE.[AccountId] = @AccountId
	AND	EE.[PostingDate] <= @AsOfDate
	-- Exclude if it was reconciled with internal entry before as of date
	AND EE.[Id] NOT IN (SELECT [ExternalEntryId] FROM ReconciledExternalEntriesAsOfDate)
	-- exclude if it was reconciled with external 
	AND EE.[Id] NOT IN (
		SELECT ExternalEntryId FROM dbo.ReconciliationExternalEntries 
		WHERE ReconciliationId IN (SELECT [ReconciliationId] FROM WhollyReversedExternalEntriesAsOfDate)
	)
	ORDER BY EE.[PostingDate], EE.[MonetaryValue], EE.[ExternalReference]

	RETURN;
END
GO